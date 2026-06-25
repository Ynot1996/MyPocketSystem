using Microsoft.AspNetCore.Mvc;
using MyPocket.Web.Services;

namespace MyPocket.Web.Controllers
{
    public class CurrencyController : Controller
    {
        // Stores the user's preferred currency in a cookie so subsequent
        // requests render amounts with the right symbol.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Set(string code, string? returnUrl)
        {
            if (!string.IsNullOrEmpty(code))
            {
                Response.Cookies.Append(
                    CurrencyService.CookieName,
                    code,
                    new CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.AddYears(1),
                        IsEssential = true,
                        SameSite = SameSiteMode.Lax
                    });
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer))
            {
                return Redirect(referer);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
