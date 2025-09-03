using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MyPocket.Services.Interfaces;
using MyPocket.Shared.ViewModels.Transactions;
using System.Security.Claims;

namespace MyPocket.Web.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Roles = "FreeMember")]
    public class TransactionsController : Controller
    {
        private readonly ITransactionService _transactionService;
        private readonly ISavingGoalService _savingGoalService;
        private readonly ICategoryService _categoryService;

        public TransactionsController(
            ITransactionService transactionService,
            ISavingGoalService savingGoalService,
            ICategoryService categoryService)
        {
            _transactionService = transactionService;
            _savingGoalService = savingGoalService;
            _categoryService = categoryService;
        }

        /// <summary>
        /// 獲取當前用戶ID
        /// </summary>
        /// <returns>用戶ID</returns>
        /// <exception cref="UnauthorizedAccessException">當無法獲取用戶ID時拋出</exception>
        private Guid GetUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                throw new UnauthorizedAccessException("無法識別用戶身份");
            }
            return userId;
        }

        /// <summary>
        /// 交易記錄首頁
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = GetUserId();

                // 獲取交易記錄
                var transactions = await _transactionService.GetUserTransactionsAsync(userId);
                
                // 獲取儲蓄目標
                var savingGoals = await _savingGoalService.GetUserGoalsAsync(userId);
                ViewBag.SavingGoals = savingGoals;

                // 獲取分類列表
                var categoryViewModel = await _categoryService.GetUserCategoriesAsync(userId);
                
                // 合併收入和支出分類
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
        /// 新增交易記錄
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
        /// 刪除交易記錄
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