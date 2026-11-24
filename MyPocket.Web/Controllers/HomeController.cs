using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using MyPocket.Web.Models;
using System.Diagnostics;

namespace MyPocket.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // Signed-in visitors (including guests) go straight to their own space.
            if (User.Identity?.IsAuthenticated == true)
            {
                return User.IsInRole("Admin")
                    ? RedirectToAction("Index", "Users", new { area = "Admin" })
                    : RedirectToAction("Index", "Transactions", new { area = "User" });
            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            return LocalRedirect(returnUrl);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
