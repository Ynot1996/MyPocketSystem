using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MyPocket.Web.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Roles = "PaidMember")]
    public class PaidMemberController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
