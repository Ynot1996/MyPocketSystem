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
        public async Task<IActionResult> Create(string goalType, int year, int? month, decimal targetAmount)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
                return RedirectToAction("Login", "Account", new { area = "" });

            if (goalType != "Monthly" && goalType != "Yearly")
            {
                ModelState.AddModelError("goalType", "Goal type is required.");
                return View();
            }
            if (goalType == "Monthly" && (!month.HasValue || month < 1 || month > 12))
            {
                ModelState.AddModelError("month", "Month is required for monthly goal.");
                return View();
            }
            if (targetAmount <= 0)
            {
                ModelState.AddModelError("targetAmount", "Target amount must be greater than 0.");
                return View();
            }

            // Reuse the service's get-or-create so a goal for a period has exactly one
            // row. The old code built its own name ("2026 Monthly Goal (06)") which
            // never matched the service's "MonthlyGoal:2026-06", creating duplicates.
            var goal = goalType == "Monthly"
                ? await _savingGoalService.GetOrCreateMonthlyGoalAsync(userId, year, month!.Value)
                : await _savingGoalService.GetOrCreateYearlyGoalAsync(userId, year);

            goal.TargetAmount = targetAmount;
            goal.UpdatedAt = DateTime.UtcNow;
            await _savingGoalService.CreateOrUpdateGoalAsync(goal);
            TempData["SuccessMessage"] = "Saving goal saved successfully.";
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
        public async Task<IActionResult> Edit(Guid goalId, decimal targetAmount)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
                return RedirectToAction("Login", "Account", new { area = "" });

            var goals = await _savingGoalService.GetUserGoalsAsync(userId);
            var model = goals.FirstOrDefault(g => g.GoalId == goalId);
            if (model == null) return NotFound();

            if (targetAmount <= 0)
            {
                ModelState.AddModelError("targetAmount", "Target amount must be greater than 0.");
                return View(model);
            }

            // Recalculate current amount
            DateTime startDate = model.TargetDate.Month == 12 && model.TargetDate.Day == 31 ? new DateTime(model.TargetDate.Year, 1, 1) : new DateTime(model.TargetDate.Year, model.TargetDate.Month, 1);
            DateTime endDate = model.TargetDate;
            model.TargetAmount = targetAmount;
            model.CurrentAmount = await _savingGoalService.CalculateCurrentSavingAsync(userId, startDate, endDate);
            model.UpdatedAt = DateTime.UtcNow;
            await _savingGoalService.CreateOrUpdateGoalAsync(model);
            TempData["SuccessMessage"] = "Saving goal updated successfully.";
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
            TempData["SuccessMessage"] = "Saving goal deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
