using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPocket.DataAccess.Data;
using MyPocket.Services;
using MyPocket.Shared.Resources;
using ISubscriptionService = MyPocket.Services.ISubscriptionService;

namespace MyPocket.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersManagementController : Controller
    {
        private readonly MyPocketDBContext _context;
        private readonly ISubscriptionService _subscriptionService;
        private readonly ILocalizationService _localizer;

        public UsersManagementController(
            MyPocketDBContext context,
            ISubscriptionService subscriptionService,
            ILocalizationService localizer)
        {
            _context = context;
            _subscriptionService = subscriptionService;
            _localizer = localizer;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .Where(u => u.Role != "Admin" && !u.IsDeleted)
                .Include(u => u.UserSubscriptions)
                .ThenInclude(us => us.Plan)
                .OrderBy(u => u.Email)
                .ToListAsync();

            ViewBag.SubscriptionPlans = await _subscriptionService.GetAllPlansAsync();
            return View(users);
        }

        public async Task<IActionResult> UserSubscriptionHistory(Guid userId)
        {
            var user = await _context.Users
                .Include(u => u.UserSubscriptions)
                .ThenInclude(us => us.Plan)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return NotFound();

            ViewBag.SubscriptionPlans = await _subscriptionService.GetAllPlansAsync();
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSubscription(Guid userId, Guid planId)
        {
            try
            {
                var success = await _subscriptionService.SubscribeAsync(userId, planId);
                TempData["SuccessMessage"] = success
                    ? _localizer.GetString("SubscriptionUpdateOk")
                    : _localizer.GetString("SubscriptionUpdateError");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"{_localizer.GetString("SubscriptionUpdateError")}: {ex.Message}";
            }

            return RedirectToAction(nameof(UserSubscriptionHistory), new { userId });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSelectedSubscriptions([FromBody] UpdateSubscriptionRequest request)
        {
            if (request?.UserIds == null || request.UserIds.Count == 0 || request.PlanId == Guid.Empty)
            {
                return Json(new { success = false, message = _localizer.GetString("InvalidRequest") });
            }

            try
            {
                foreach (var userId in request.UserIds)
                {
                    await _subscriptionService.SubscribeAsync(userId, request.PlanId);
                }
                return Json(new
                {
                    success = true,
                    message = string.Format(_localizer.GetString("BulkSubscriptionUpdateOk"), request.UserIds.Count)
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAllUsersToFreeMember()
        {
            try
            {
                var users = await _context.Users
                    .Where(u => u.Role != "Admin" && !u.IsDeleted)
                    .ToListAsync();

                // Match the seeded free plan by its DB literal name; do not
                // localize this — it must match the row produced by DbInitializer.
                var basicPlan = await _context.SubscriptionPlans
                    .FirstOrDefaultAsync(p => p.PlanName.Contains("基本") || p.PlanName.Contains("免費"));

                if (basicPlan == null)
                {
                    TempData["ErrorMessage"] = _localizer.GetString("FreePlanNotFound");
                    return RedirectToAction(nameof(Index));
                }

                foreach (var user in users)
                {
                    user.Role = "FreeMember";
                    user.UpdatedAt = DateTime.UtcNow;
                    await _subscriptionService.SubscribeAsync(user.UserId, basicPlan.PlanId);
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = _localizer.GetString("AllUsersSetToFreeMember");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"{_localizer.GetString("UpdateUserStatusError")}: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }

    public class UpdateSubscriptionRequest
    {
        public List<Guid> UserIds { get; set; } = new();
        public Guid PlanId { get; set; }
    }
}
