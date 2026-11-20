using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPocket.DataAccess.Data;
using MyPocket.Services;
using MyPocket.Shared.ViewModels.Accounts;
using System.Security.Claims;

namespace MyPocket.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly MyPocketDBContext _context;
        private readonly ISubscriptionService _subscriptionService;
        private readonly IConfiguration _configuration;

        public AccountController(
            MyPocketDBContext context,
            ISubscriptionService subscriptionService,
            IConfiguration configuration)
        {
            _context = context;
            _subscriptionService = subscriptionService;
            _configuration = configuration;
        }

        // True only when both Google secrets are configured; the Login view
        // reads this to decide whether to render the "Sign in with Google"
        // button (hidden during local dev before OAuth has been set up).
        private bool GoogleEnabled =>
            !string.IsNullOrEmpty(_configuration["Authentication:Google:ClientId"]) &&
            !string.IsNullOrEmpty(_configuration["Authentication:Google:ClientSecret"]);

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["GoogleEnabled"] = GoogleEnabled;
            return View();
        }

        /// <summary>
        /// Kicks off an external login flow (currently only Google). The
        /// provider redirects the browser away; when it comes back, the
        /// Google middleware's OnTicketReceived handler (see Program.cs)
        /// creates or updates the local User row and signs them in.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public IActionResult ExternalLogin(string provider, string? returnUrl = null)
        {
            if (!GoogleEnabled || provider != GoogleDefaults.AuthenticationScheme)
            {
                TempData["ErrorMessage"] = "That sign-in provider is not configured.";
                return RedirectToAction(nameof(Login), new { returnUrl });
            }

            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var props = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(props, provider);
        }

        /// <summary>
        /// Lands here after Google signs the user in via the Cookie scheme.
        /// By this point OnTicketReceived has already created the User row
        /// and built our domain claims, so all we do is pick a landing page.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            if (!string.IsNullOrEmpty(remoteError))
            {
                TempData["ErrorMessage"] = $"Sign-in failed: {remoteError}";
                return RedirectToAction(nameof(Login), new { returnUrl });
            }

            if (!User.Identity?.IsAuthenticated ?? true)
            {
                TempData["ErrorMessage"] = "Sign-in was cancelled.";
                return RedirectToAction(nameof(Login), new { returnUrl });
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            var role = User.FindFirstValue(ClaimTypes.Role);
            return role switch
            {
                "Admin" => RedirectToAction("Index", "Users", new { area = "Admin" }),
                "PaidMember" => RedirectToAction("Index", "PaidMember", new { area = "User" }),
                _ => RedirectToAction("Index", "Transactions", new { area = "User" })
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);
            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                user.LastLoginDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Name, user.Email),
                    new Claim("Nickname", user.Nickname ?? user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties { IsPersistent = true };
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity), authProperties);

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                if (user.Role == "Admin")
                    return RedirectToAction("Index", "Users", new { area = "Admin" });

                else if (user.Role == "PaidMember")
                    return RedirectToAction("Index", "PaidMember", new { area = "User" });

                else if (user.Role == "FreeMember")
                    return RedirectToAction("Index", "Transactions", new { area = "User" });

                return RedirectToAction("Index", "Home");
            }
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            ViewData["ReturnUrl"] = returnUrl;
            return View();
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

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // Password and confirm password match validation
            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError(nameof(model.ConfirmPassword), "Password and Confirm Password do not match.");
            }

            if (!ModelState.IsValid)
                return View(model);

            if (await _context.Users.AnyAsync(u => u.Email == model.Email && !u.IsDeleted))
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                return View(model);
            }

            var user = new MyPocket.Core.Models.User
            {
                UserId = Guid.NewGuid(),
                Email = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role = "FreeMember",
                Nickname = string.IsNullOrWhiteSpace(model.Nickname) ? model.Email.Split('@')[0] : model.Nickname,
                CreationDate = DateTime.UtcNow,
                LastLoginDate = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Auto subscribe to basic plan
            await _subscriptionService.SubscribeToBasicPlanAsync(user.UserId);

            TempData["SuccessMessage"] = "Registration successful! You have been subscribed to the basic plan.";
            return RedirectToAction("Login");
        }
    }
}