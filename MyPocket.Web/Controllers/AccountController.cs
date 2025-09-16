using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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
        public AccountController(MyPocketDBContext context, ISubscriptionService subscriptionService)
        {
            _context = context;
            _subscriptionService = subscriptionService;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
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