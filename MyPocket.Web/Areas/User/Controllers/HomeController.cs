using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MyPocket.Web.Areas.User.Controllers
{
    /// <summary>
    /// The member home used to be a separate summary page; the city dashboard on
    /// Transactions/Index replaced it. Kept as a redirect so old links and bookmarks work.
    /// </summary>
    [Area("User")]
    [Authorize]
    public class HomeController : Controller
    {
        public IActionResult Index() => RedirectToAction("Index", "Transactions", new { area = "User" });
    }
}
