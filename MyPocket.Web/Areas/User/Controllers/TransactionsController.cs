using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPocket.Core.Models;
using MyPocket.Services.Interfaces;
using System.Security.Claims;

namespace MyPocket.Web.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Roles = "FreeMember")]
    public class TransactionsController : Controller
    {
        private readonly ITransactionService _transactionService;
        private readonly ISavingGoalService _savingGoalService;
        public TransactionsController(ITransactionService transactionService, ISavingGoalService savingGoalService)
        {
            _transactionService = transactionService;
            _savingGoalService = savingGoalService;
        }

        public async Task<IActionResult> Index()
        {          
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }
            var userId = Guid.Parse(userIdString);

            var transactions = await _transactionService.GetUserTransactionsAsync(userId) ?? new List<Transaction>();
            var now = DateTime.UtcNow;
            var savingGoals = await _savingGoalService.GetUserGoalsAsync(userId);
            ViewBag.SavingGoals = savingGoals;
            ViewBag.Categories = ViewBag.Categories ?? new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>();
            // ��l ViewBag.Budgets�BViewBag.Categories ���i�̻ݨD�`�J��L service
            return View(transactions);
        }

        // ��l CRUD �]�Чאּ�I�s _transactionService
    }
}