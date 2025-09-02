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
        /// �����e�Τ�ID
        /// </summary>
        /// <returns>�Τ�ID</returns>
        /// <exception cref="UnauthorizedAccessException">��L�k����Τ�ID�ɩߥX</exception>
        private Guid GetUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                throw new UnauthorizedAccessException("�L�k�ѧO�Τᨭ��");
            }
            return userId;
        }

        /// <summary>
        /// ����O������
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = GetUserId();

                // �������O��
                var transactions = await _transactionService.GetUserTransactionsAsync(userId);
                
                // ����x�W�ؼ�
                var savingGoals = await _savingGoalService.GetUserGoalsAsync(userId);
                ViewBag.SavingGoals = savingGoals;

                // ��������C��
                var categoryViewModel = await _categoryService.GetUserCategoriesAsync(userId);
                
                // �X�֦��J�M��X����
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
                TempData["ErrorMessage"] = $"���J��Ʈɵo�Ϳ��~: {ex.Message}";
                return View(Enumerable.Empty<Core.Models.Transaction>());
            }
        }

        /// <summary>
        /// �s�W����O��
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
                TempData["ErrorMessage"] = $"���ˬd��J��ƬO�_���T: {errors}";
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
                TempData["ErrorMessage"] = $"�s�W����: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// �R������O��
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
                TempData["ErrorMessage"] = $"�R������: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}