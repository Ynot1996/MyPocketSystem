using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPocket.Core.Models;
using MyPocket.Services.Interfaces;
using System.Security.Claims;

namespace MyPocket.Web.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Roles = "FreeMember,PaidMember")]
    public class SavingGoalsController : Controller
    {
        private readonly ISavingGoalService _savingGoalService;
        public SavingGoalsController(ISavingGoalService savingGoalService)
        {
            _savingGoalService = savingGoalService;
        }

        public async Task<IActionResult> Index()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
                return RedirectToAction("Login", "Account", new { area = "" });

            var now = DateTime.UtcNow;
            await _savingGoalService.GetOrCreateMonthlyGoalAsync(userId, now.Year, now.Month);
            await _savingGoalService.GetOrCreateYearlyGoalAsync(userId, now.Year);
            var goals = await _savingGoalService.GetUserGoalsAsync(userId);
            return View(goals);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SavingGoal model)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
                return RedirectToAction("Login", "Account", new { area = "" });

            if (!ModelState.IsValid)
                return View(model);

            model.GoalId = Guid.NewGuid();
            model.UserId = userId;
            model.CreatedAt = DateTime.UtcNow;
            model.UpdatedAt = DateTime.UtcNow;
            model.IsDeleted = false;
            await _savingGoalService.CreateOrUpdateGoalAsync(model);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
                return RedirectToAction("Login", "Account", new { area = "" });

            var goals = await _savingGoalService.GetUserGoalsAsync(userId);
            var goal = goals.FirstOrDefault(g => g.GoalId == id);
            if (goal == null) return NotFound();
            return View(goal);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SavingGoal model)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
                return RedirectToAction("Login", "Account", new { area = "" });

            if (!ModelState.IsValid)
                return View(model);

            model.UserId = userId;
            model.UpdatedAt = DateTime.UtcNow;
            await _savingGoalService.CreateOrUpdateGoalAsync(model);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
                return RedirectToAction("Login", "Account", new { area = "" });

            var goals = await _savingGoalService.GetUserGoalsAsync(userId);
            var goal = goals.FirstOrDefault(g => g.GoalId == id);
            if (goal == null) return NotFound();
            return View(goal);
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
                return RedirectToAction("Login", "Account", new { area = "" });

            var goals = await _savingGoalService.GetUserGoalsAsync(userId);
            var goal = goals.FirstOrDefault(g => g.GoalId == id);
            if (goal == null) return NotFound();
            return View(goal);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
                return RedirectToAction("Login", "Account", new { area = "" });

            await _savingGoalService.DeleteGoalAsync(userId, id);
            return RedirectToAction(nameof(Index));
        }
    }
}
