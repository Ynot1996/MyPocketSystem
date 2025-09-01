using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPocket.DataAccess.Data;
using MyPocket.Shared.ViewModels.Accounts;
using System.Security.Claims;


namespace MyPocket.Web.Controllers
{

    public class AccountController : Controller
    {
        private readonly MyPocketDBContext _context;

        public AccountController(MyPocketDBContext context)
        {
            _context = context;
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

                else if (user.Role == "FreeMember")
                    return RedirectToAction("Index", "Transactions", new { area = "User"});

                return RedirectToAction("Index", "Home");
            }
            ModelState.AddModelError(string.Empty, "帳號或密碼錯誤");
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
            if (!ModelState.IsValid)
                return View(model);

            if (await _context.Users.AnyAsync(u => u.Email == model.Email && !u.IsDeleted))
            {
                ModelState.AddModelError("Email", "此Email已被註冊");
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
            return RedirectToAction("Login");
        }
    }
}
