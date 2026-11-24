using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using MyPocket.DataAccess.Data;
using MyPocket.Services;
using MyPocket.Services.Interfaces;
using MyPocket.Shared.Resources;
using MyPocket.Shared.ViewModels.Accounts;
using MyPocket.Web.Infrastructure;
using System.Security.Claims;

namespace MyPocket.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly MyPocketDBContext _context;
        private readonly ISubscriptionService _subscriptionService;
        private readonly IAccountService _accounts;
        private readonly IGamificationService _gamification;
        private readonly ILocalizationService _localizer;
        private readonly IAppTelemetry _telemetry;
        private readonly IConfiguration _configuration;

        public AccountController(
            MyPocketDBContext context,
            ISubscriptionService subscriptionService,
            IAccountService accounts,
            IGamificationService gamification,
            ILocalizationService localizer,
            IAppTelemetry telemetry,
            IConfiguration configuration)
        {
            _context = context;
            _subscriptionService = subscriptionService;
            _accounts = accounts;
            _gamification = gamification;
            _localizer = localizer;
            _telemetry = telemetry;
            _configuration = configuration;
        }

        // True only when both Google secrets are configured; views use it to decide
        // whether to render "Continue with Google".
        private bool GoogleEnabled =>
            !string.IsNullOrEmpty(_configuration["Authentication:Google:ClientId"]) &&
            !string.IsNullOrEmpty(_configuration["Authentication:Google:ClientSecret"]);

        /// <summary>Everyone except admins lands on the main dashboard (the city view).</summary>
        private IActionResult RedirectToLanding(string? role, string? returnUrl = null)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return role == "Admin"
                ? RedirectToAction("Index", "Users", new { area = "Admin" })
                : RedirectToAction("Index", "Transactions", new { area = "User" });
        }

        // ------------------------------------------------------------------
        // Guest mode ("try before you sign up")
        // ------------------------------------------------------------------

        /// <summary>
        /// Shareable demo link (e.g. on a CV). Crawlers don't run JavaScript, so the
        /// page auto-submits a POST for real browsers without letting bots create
        /// accounts just by following a link.
        /// </summary>
        [HttpGet("/try")]
        [AllowAnonymous]
        public IActionResult Try()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToLanding(User.FindFirstValue(ClaimTypes.Role));
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting(RateLimitPolicies.GuestStart)]
        public async Task<IActionResult> StartGuest()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToLanding(User.FindFirstValue(ClaimTypes.Role));

            var guest = await _accounts.CreateGuestAsync();
            await HttpContext.SignInAsync(guest);
            _telemetry.TrackEvent("GuestStarted");

            TempData["SuccessMessage"] = _localizer.GetString("GuestWelcome");
            return RedirectToAction("Index", "Transactions", new { area = "User" });
        }

        // ------------------------------------------------------------------
        // Password login
        // ------------------------------------------------------------------

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["GoogleEnabled"] = GoogleEnabled;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted && !u.IsGuest);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, _localizer.GetString("InvalidLogin"));
                ViewData["ReturnUrl"] = returnUrl;
                ViewData["GoogleEnabled"] = GoogleEnabled;
                return View();
            }

            // Logging into a real account from guest mode keeps what the guest entered.
            var guestId = User.IsGuestUser() ? User.UserIdOrNull() : null;
            if (guestId.HasValue)
            {
                await _accounts.MergeGuestIntoAsync(guestId.Value, user.UserId);
            }

            user.LastLoginDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await HttpContext.SignInAsync(user);
            return RedirectToLanding(user.Role, returnUrl);
        }

        // ------------------------------------------------------------------
        // Google
        // ------------------------------------------------------------------

        /// <summary>
        /// Starts the Google flow. Account resolution (including upgrading or merging
        /// a guest) happens in the Google OnTicketReceived handler in Program.cs.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public IActionResult ExternalLogin(string provider, string? returnUrl = null)
        {
            if (!GoogleEnabled || provider != GoogleDefaults.AuthenticationScheme)
            {
                TempData["ErrorMessage"] = _localizer.GetString("ProviderNotConfigured");
                return RedirectToAction(nameof(Login), new { returnUrl });
            }

            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            return Challenge(new AuthenticationProperties { RedirectUri = redirectUrl }, provider);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            if (!string.IsNullOrEmpty(remoteError) || User.Identity?.IsAuthenticated != true)
            {
                TempData["ErrorMessage"] = _localizer.GetString("ExternalLoginFailed");
                return RedirectToAction(nameof(Login), new { returnUrl });
            }

            return RedirectToLanding(User.FindFirstValue(ClaimTypes.Role), returnUrl);
        }

        // ------------------------------------------------------------------
        // Registration
        // ------------------------------------------------------------------

        [HttpGet]
        public IActionResult Register()
        {
            ViewData["GoogleEnabled"] = GoogleEnabled;
            ViewData["IsGuest"] = User.IsGuestUser();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            ViewData["GoogleEnabled"] = GoogleEnabled;
            ViewData["IsGuest"] = User.IsGuestUser();

            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError(nameof(model.ConfirmPassword), _localizer.GetString("PasswordsDoNotMatch"));
            }
            if (!ModelState.IsValid)
                return View(model);

            // Guest → real account: upgrade the existing row so nothing is lost.
            var guestId = User.IsGuestUser() ? User.UserIdOrNull() : null;
            if (guestId.HasValue)
            {
                var result = await _accounts.UpgradeGuestAsync(guestId.Value, model.Email, model.Password, model.Nickname);
                if (result == GuestUpgradeResult.EmailTaken)
                {
                    ModelState.AddModelError(nameof(model.Email), _localizer.GetString("EmailAlreadyRegistered"));
                    return View(model);
                }
                if (result == GuestUpgradeResult.Upgraded)
                {
                    var upgraded = await _context.Users.SingleAsync(u => u.UserId == guestId.Value);
                    await HttpContext.SignInAsync(upgraded);
                    _telemetry.TrackEvent("GuestConverted", new Dictionary<string, string> { ["method"] = "password" });
                    TempData["SuccessMessage"] = _localizer.GetString("GuestUpgraded");
                    return RedirectToLanding(upgraded.Role);
                }
            }

            if (await _context.Users.AnyAsync(u => u.Email == model.Email && !u.IsDeleted))
            {
                ModelState.AddModelError(nameof(model.Email), _localizer.GetString("EmailAlreadyRegistered"));
                return View(model);
            }

            var now = DateTime.UtcNow;
            var user = new MyPocket.Core.Models.User
            {
                UserId = Guid.NewGuid(),
                Email = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role = "FreeMember",
                Nickname = string.IsNullOrWhiteSpace(model.Nickname) ? model.Email.Split('@')[0] : model.Nickname,
                CreationDate = now,
                LastLoginDate = now,
                UpdatedAt = now
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await _subscriptionService.SubscribeToBasicPlanAsync(user.UserId);
            await _gamification.EnsureWelcomeBonusAsync(user.UserId);
            _telemetry.TrackEvent("UserRegistered", new Dictionary<string, string> { ["method"] = "password" });

            // Sign straight in instead of bouncing the new user back to the login form.
            await HttpContext.SignInAsync(user);
            TempData["SuccessMessage"] = _localizer.GetString("RegistrationComplete");
            return RedirectToLanding(user.Role);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
