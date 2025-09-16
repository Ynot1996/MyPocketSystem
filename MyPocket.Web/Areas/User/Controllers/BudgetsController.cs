using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPocket.Services.Interfaces;
using MyPocket.Shared.ViewModels.Budgets;
using System.Security.Claims;

namespace MyPocket.Web.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Roles = "FreeMember,PaidMember")]
    public class BudgetsController : Controller
    {
        private readonly IBudgetService _budgetService;
        public BudgetsController(IBudgetService budgetService)
        {
            _budgetService = budgetService;
        }

        public async Task<IActionResult> Index()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
                return RedirectToAction("Login", "Account", new { area = "" });

            var now = DateTime.UtcNow;
            var vm = await _budgetService.GetUserBudgetsAsync(userId, now.Year, now.Month);
            return View(vm);
        }

        public async Task<IActionResult> Create(Guid? categoryId = null)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
                return RedirectToAction("Login", "Account", new { area = "" });

            var categories = await _budgetService.GetUserExpenseCategoriesAsync(userId);
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

            if (!ModelState.IsValid)
            {
                var categories = await _budgetService.GetUserExpenseCategoriesAsync(userId);
                ViewBag.Categories = categories;
                ViewBag.SelectedCategoryId = model.CategoryId;
                return View(model);
            }

            await _budgetService.CreateBudgetAsync(userId, model);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
                return RedirectToAction("Login", "Account", new { area = "" });

            var vm = await _budgetService.GetBudgetDetailsAsync(userId, id);
            if (vm == null) return NotFound();
            return View(vm);
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
                return RedirectToAction("Login", "Account", new { area = "" });

            var vm = await _budgetService.GetBudgetDetailsAsync(userId, id);
            if (vm == null) return NotFound();
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

            var result = await _budgetService.UpdateBudgetAsync(userId, model);
            if (!result) return NotFound();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
                return RedirectToAction("Login", "Account", new { area = "" });

            var vm = await _budgetService.GetBudgetDetailsAsync(userId, id);
            if (vm == null) return NotFound();
            return View(vm);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid BudgetId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
                return RedirectToAction("Login", "Account", new { area = "" });

            var result = await _budgetService.DeleteBudgetAsync(userId, BudgetId);
            if (!result) return NotFound();
            return RedirectToAction("Index");
        }
    }
}
