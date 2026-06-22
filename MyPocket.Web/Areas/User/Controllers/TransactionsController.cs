using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyPocket.Services.Interfaces;
using MyPocket.Shared.ViewModels.Transactions;
using System.Security.Claims;

namespace MyPocket.Web.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Roles = "FreeMember,PaidMember")]
    public class TransactionsController : Controller
    {
        private readonly ITransactionService _transactionService;
        private readonly ISavingGoalService _savingGoalService;
        private readonly ICategoryService _categoryService;
        private readonly IBudgetService _budgetService;

        public TransactionsController(
            ITransactionService transactionService,
            ISavingGoalService savingGoalService,
            ICategoryService categoryService,
            IBudgetService budgetService)
        {
            _transactionService = transactionService;
            _savingGoalService = savingGoalService;
            _categoryService = categoryService;
            _budgetService = budgetService;
        }

        /// <summary>
        /// Gets the current user's ID from the authentication claims.
        /// </summary>
        /// <returns>The current user's ID.</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user ID cannot be resolved.</exception>
        private Guid GetUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                throw new UnauthorizedAccessException("Unable to identify the current user.");
            }
            return userId;
        }

        /// <summary>
        /// Transaction list home page.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = GetUserId();
                var currentYear = DateTime.Now.Year;
                var currentMonth = DateTime.Now.Month;

                System.Diagnostics.Debug.WriteLine($"User ID in Index action: {userId}");

                var transactions = await _transactionService.GetUserTransactionsAsync(userId);

                var savingGoals = await _savingGoalService.GetUserGoalsAsync(userId);
                ViewBag.SavingGoals = savingGoals;

                var budgets = await _budgetService.GetUserBudgetsAsync(userId, currentYear, currentMonth);
                ViewBag.Budgets = budgets;

                var categoryViewModel = await _categoryService.GetUserCategoriesAsync(userId);

                // Merge income and expense categories into a single list for the dropdown.
                var allCategories = categoryViewModel.DefaultIncomeCategories
                    .Concat(categoryViewModel.DefaultExpenseCategories)
                    .Concat(categoryViewModel.UserIncomeCategories)
                    .Concat(categoryViewModel.UserExpenseCategories);

                ViewBag.Categories = allCategories
                    .Select(c => new SelectListItem
                    {
                        Value = c.CategoryId.ToString(),
                        Text = c.CategoryName,
                        Group = new SelectListGroup { Name = c.CategoryType }
                    })
                    .OrderBy(x => x.Group.Name)
                    .ThenBy(x => x.Text);

                return View(transactions);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View(Enumerable.Empty<Core.Models.Transaction>());
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"載入資料時發生錯誤: {ex.Message}";
                return View(Enumerable.Empty<Core.Models.Transaction>());
            }
        }

        /// <summary>
        /// Creates a new transaction.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TransactionCreateModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join(" ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                TempData["ErrorMessage"] = $"請檢查輸入資料是否正確: {errors}";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var userId = GetUserId();
                var (success, message, _) = await _transactionService.CreateTransactionAsync(userId, model);

                if (success)
                    TempData["SuccessMessage"] = message;
                else
                    TempData["ErrorMessage"] = message;
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"新增失敗: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Deletes a transaction.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var userId = GetUserId();
                var (success, message) = await _transactionService.DeleteTransactionAsync(userId, id);

                if (success)
                    TempData["SuccessMessage"] = message;
                else
                    TempData["ErrorMessage"] = message;
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"刪除失敗: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}