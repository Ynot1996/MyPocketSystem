using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyPocket.Services.Interfaces;
using MyPocket.Shared.Resources;
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
        private readonly ILocalizationService _localizer;

        public TransactionsController(
            ITransactionService transactionService,
            ISavingGoalService savingGoalService,
            ICategoryService categoryService,
            IBudgetService budgetService,
            ILocalizationService localizer)
        {
            _transactionService = transactionService;
            _savingGoalService = savingGoalService;
            _categoryService = categoryService;
            _budgetService = budgetService;
            _localizer = localizer;
        }

        // Translates a service-returned key. Service may encode an inner error
        // detail as "Key|detail" (e.g. "CreateFailed|sql exception"); we look up
        // the key and append the detail for diagnostic visibility.
        private string Translate(string keyOrComposite)
        {
            if (string.IsNullOrEmpty(keyOrComposite)) return string.Empty;
            var pipe = keyOrComposite.IndexOf('|');
            if (pipe < 0) return _localizer.GetString(keyOrComposite);
            var key = keyOrComposite[..pipe];
            var detail = keyOrComposite[(pipe + 1)..];
            return $"{_localizer.GetString(key)}: {detail}";
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
                TempData["ErrorMessage"] = $"{_localizer.GetString("ErrorLoadingData")}: {ex.Message}";
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
                TempData["ErrorMessage"] = $"{_localizer.GetString("ValidationFailed")}: {errors}";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var userId = GetUserId();
                var (success, message, _) = await _transactionService.CreateTransactionAsync(userId, model);

                if (success)
                    TempData["SuccessMessage"] = Translate(message);
                else
                    TempData["ErrorMessage"] = Translate(message);
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
                TempData["ErrorMessage"] = $"{_localizer.GetString("CreateFailed")}: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Renders the edit form for a transaction the current user owns.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var userId = GetUserId();
                var tx = await _transactionService.GetTransactionAsync(userId, id);
                if (tx == null)
                {
                    TempData["ErrorMessage"] = _localizer.GetString("TransactionNotFound");
                    return RedirectToAction(nameof(Index));
                }

                // Reuse the same category dropdown as Create.
                var categoryViewModel = await _categoryService.GetUserCategoriesAsync(userId);
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

                ViewBag.TransactionId = id;

                return View(new TransactionCreateModel
                {
                    CategoryId = tx.CategoryId,
                    Amount = tx.Amount,
                    Currency = string.IsNullOrEmpty(tx.Currency) ? "GBP" : tx.Currency,
                    TransactionDate = tx.TransactionDate,
                    Description = tx.Description
                });
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
        }

        /// <summary>
        /// Persists edits to a transaction.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, TransactionCreateModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join(" ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                TempData["ErrorMessage"] = $"{_localizer.GetString("ValidationFailed")}: {errors}";
                return RedirectToAction(nameof(Edit), new { id });
            }

            try
            {
                var userId = GetUserId();
                var (success, message) = await _transactionService.UpdateTransactionAsync(userId, id, model);
                if (success)
                {
                    TempData["SuccessMessage"] = Translate(message);
                    return RedirectToAction(nameof(Index));
                }

                TempData["ErrorMessage"] = Translate(message);
                return RedirectToAction(nameof(Edit), new { id });
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Edit), new { id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"{_localizer.GetString("UpdateFailed")}: {ex.Message}";
                return RedirectToAction(nameof(Edit), new { id });
            }
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
                    TempData["SuccessMessage"] = Translate(message);
                else
                    TempData["ErrorMessage"] = Translate(message);
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
                TempData["ErrorMessage"] = $"{_localizer.GetString("DeleteFailed")}: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}