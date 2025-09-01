using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPocket.DataAccess.Data;
using MyPocket.Shared.ViewModels.Budgets;
using System.Security.Claims;

namespace MyPocket.Web.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Roles = "FreeMember")]
    public class BudgetsController : Controller
    {
        private readonly MyPocketDBContext _context;
        public BudgetsController(MyPocketDBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
                return RedirectToAction("Login", "Account", new { area = "" });

            var now = DateTime.UtcNow;
            var year = now.Year.ToString();
            var month = now.Month.ToString("D2");

            var budgets = await _context.Budgets
                .Where(b => b.UserId == userId && !b.IsDeleted && b.BudgetYear == year && b.BudgetMonth == month)
                .Include(b => b.Category)
                .ToListAsync();

            var transactions = await _context.Transactions
                .Where(t => t.UserId == userId && !t.IsDeleted && t.TransactionDate.Year == now.Year && t.TransactionDate.Month == now.Month)
                .ToListAsync();

            var vm = budgets.Select(b => new BudgetViewModel
            {
                BudgetId = b.BudgetId,
                CategoryId = b.CategoryId,
                CategoryName = b.Category?.CategoryName ?? "",
                CategoryType = b.Category?.CategoryType ?? "",
                Amount = b.Amount,
                BudgetYear = b.BudgetYear,
                BudgetMonth = b.BudgetMonth,
                Spent = transactions.Where(t => t.CategoryId == b.CategoryId).Sum(t => t.Amount)
            }).ToList();

            return View(vm);
        }

        public async Task<IActionResult> Create(Guid? categoryId = null)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
                return RedirectToAction("Login", "Account", new { area = "" });

            // 取得該用戶所有支出分類（含預設/系統分類）
            var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Role == "Admin");
            var categories = await _context.Categories
                .Where(c => !c.IsDeleted && c.CategoryType == "支出" && (c.UserId == userId || (adminUser != null && c.UserId == adminUser.UserId)))
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
            ViewBag.Categories = categories;
            ViewBag.SelectedCategoryId = categoryId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BudgetViewModel model)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
                return RedirectToAction("Login", "Account", new { area = "" });

            // 允許選擇預設(系統)分類
            var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Role == "Admin");
            var validCategory = await _context.Categories.AnyAsync(c => c.CategoryId == model.CategoryId && !c.IsDeleted && c.CategoryType == "支出" && (c.UserId == userId || (adminUser != null && c.UserId == adminUser.UserId)));
            if (!validCategory)
            {
                ModelState.AddModelError("CategoryId", "請選擇有效的支出分類");
            }
            if (!ModelState.IsValid)
            {
                var categories = await _context.Categories
                    .Where(c => !c.IsDeleted && c.CategoryType == "支出" && (c.UserId == userId || (adminUser != null && c.UserId == adminUser.UserId)))
                    .OrderBy(c => c.CategoryName)
                    .ToListAsync();
                ViewBag.Categories = categories;
                ViewBag.SelectedCategoryId = model.CategoryId;
                return View(model);
            }

            var now = DateTime.UtcNow;
            var budget = new MyPocket.Core.Models.Budget
            {
                BudgetId = Guid.NewGuid(),
                UserId = userId,
                CategoryId = model.CategoryId,
                Amount = model.Amount,
                BudgetYear = now.Year.ToString(),
                BudgetMonth = now.Month.ToString("D2"),
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            };
            _context.Budgets.Add(budget);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
                return RedirectToAction("Login", "Account", new { area = "" });

            var budget = await _context.Budgets.Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.BudgetId == id && b.UserId == userId && !b.IsDeleted);
            if (budget == null) return NotFound();

            int year = int.Parse(budget.BudgetYear);
            int month = int.Parse(budget.BudgetMonth);
            var spent = await _context.Transactions
                .Where(t => t.UserId == userId && !t.IsDeleted && t.TransactionDate.Year == year && t.TransactionDate.Month == month && t.CategoryId == budget.CategoryId)
                .SumAsync(t => t.Amount);

            var vm = new BudgetViewModel
            {
                BudgetId = budget.BudgetId,
                CategoryId = budget.CategoryId,
                CategoryName = budget.Category?.CategoryName ?? "",
                CategoryType = budget.Category?.CategoryType ?? "",
                Amount = budget.Amount,
                BudgetYear = budget.BudgetYear,
                BudgetMonth = budget.BudgetMonth,
                Spent = spent
            };
            return View(vm);
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
                return RedirectToAction("Login", "Account", new { area = "" });

            var budget = await _context.Budgets.Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.BudgetId == id && b.UserId == userId && !b.IsDeleted);
            if (budget == null) return NotFound();

            int year = int.Parse(budget.BudgetYear);
            int month = int.Parse(budget.BudgetMonth);
            var spent = await _context.Transactions
                .Where(t => t.UserId == userId && !t.IsDeleted && t.TransactionDate.Year == year && t.TransactionDate.Month == month && t.CategoryId == budget.CategoryId)
                .SumAsync(t => t.Amount);

            var vm = new BudgetViewModel
            {
                BudgetId = budget.BudgetId,
                CategoryId = budget.CategoryId,
                CategoryName = budget.Category?.CategoryName ?? "",
                CategoryType = budget.Category?.CategoryType ?? "",
                Amount = budget.Amount,
                BudgetYear = budget.BudgetYear,
                BudgetMonth = budget.BudgetMonth,
                Spent = spent
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BudgetViewModel model)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
                return RedirectToAction("Login", "Account", new { area = "" });

            if (!ModelState.IsValid)
                return View(model);

            var budget = await _context.Budgets.FirstOrDefaultAsync(b => b.BudgetId == model.BudgetId && b.UserId == userId && !b.IsDeleted);
            if (budget == null) return NotFound();

            budget.Amount = model.Amount;
            budget.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
                return RedirectToAction("Login", "Account", new { area = "" });

            var budget = await _context.Budgets.Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.BudgetId == id && b.UserId == userId && !b.IsDeleted);
            if (budget == null) return NotFound();

            int year = int.Parse(budget.BudgetYear);
            int month = int.Parse(budget.BudgetMonth);
            var spent = await _context.Transactions
                .Where(t => t.UserId == userId && !t.IsDeleted && t.TransactionDate.Year == year && t.TransactionDate.Month == month && t.CategoryId == budget.CategoryId)
                .SumAsync(t => t.Amount);

            var vm = new BudgetViewModel
            {
                BudgetId = budget.BudgetId,
                CategoryId = budget.CategoryId,
                CategoryName = budget.Category?.CategoryName ?? "",
                CategoryType = budget.Category?.CategoryType ?? "",
                Amount = budget.Amount,
                BudgetYear = budget.BudgetYear,
                BudgetMonth = budget.BudgetMonth,
                Spent = spent
            };
            return View(vm);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid BudgetId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
                return RedirectToAction("Login", "Account", new { area = "" });

            var budget = await _context.Budgets.FirstOrDefaultAsync(b => b.BudgetId == BudgetId && b.UserId == userId && !b.IsDeleted);
            if (budget == null) return NotFound();

            budget.IsDeleted = true;
            budget.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
