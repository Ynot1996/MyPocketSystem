using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPocket.DataAccess.Data;
using MyPocket.Shared.ViewModels;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MyPocket.Web.Areas.User.Controllers
{
    [Area("User")]
    [Authorize]
    public class HomeController : Controller
    {
        private readonly MyPocketDBContext _context;
        
        public HomeController(MyPocketDBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
                return RedirectToAction("Login", "Account", new { area = "" });

            var user = await _context.Users
                .Include(u => u.UserSubscriptions)
                .FirstOrDefaultAsync(u => u.UserId == userId);
            
            // Get both past and future transactions
            var recentTransactions = await _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.UserId == userId && !t.IsDeleted)
                .OrderByDescending(t => t.TransactionDate)
                .Take(10)  // Increased to 10 transactions
                .ToListAsync();

            var transactionVMs = recentTransactions.Select(t => new TransactionViewModel
            {
                Transaction = t,
                CategoryName = t.Category?.CategoryName ?? ""
            }).ToList();

            var announcements = await _context.Announcements
                .OrderByDescending(a => a.PublishDate)
                .Take(3)
                .ToListAsync();
                
            ViewBag.User = user;
            ViewBag.RecentTransactions = transactionVMs;
            ViewBag.Announcements = announcements;
            return View();
        }
    }
}
