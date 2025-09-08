using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPocket.DataAccess.Data;
using MyPocket.Services;

namespace MyPocket.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersManagementController : Controller
    {
        private readonly MyPocketDBContext _context;
        private readonly ISubscriptionService _subscriptionService;

        public UsersManagementController(MyPocketDBContext context, ISubscriptionService subscriptionService)
        {
            _context = context;
            _subscriptionService = subscriptionService;
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
                TempData["SuccessMessage"] = success ? "�q�\�p����s���\�C" : "��s�q�\�ɵo�Ϳ��~�C";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"��s�q�\�ɵo�Ϳ��~�G{ex.Message}";
            }

            return RedirectToAction(nameof(UserSubscriptionHistory), new { userId });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSelectedSubscriptions([FromBody] UpdateSubscriptionRequest request)
        {
            if (request?.UserIds == null || request.UserIds.Count == 0 || request.PlanId == Guid.Empty)
            {
                return Json(new { success = false, message = "�L�Ī��ШD�ѼơC" });
            }

            try
            {
                foreach (var userId in request.UserIds)
                {
                    await _subscriptionService.SubscribeAsync(userId, request.PlanId);
                }
                return Json(new { success = true, message = $"�w���\��s {request.UserIds.Count} ��ϥΪ̪��q�\�p���C" });
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

                var basicPlan = await _context.SubscriptionPlans
                    .FirstOrDefaultAsync(p => p.PlanName.Contains("��") || p.PlanName.Contains("�K�O"));

                if (basicPlan == null)
                {
                    TempData["ErrorMessage"] = "�䤣��򥻤�סC";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var user in users)
                {
                    user.Role = "FreeMember";
                    user.UpdatedAt = DateTime.UtcNow;
                    await _subscriptionService.SubscribeAsync(user.UserId, basicPlan.PlanId);
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "�w���\�N�Ҧ��ϥΪ̧�s���K�O�|���A�íq�\�򥻤�סC";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"��s�ϥΪ̪��A�ɵo�Ϳ��~�G{ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }

    public class UpdateSubscriptionRequest
    {
        public List<Guid> UserIds { get; set; }
        public Guid PlanId { get; set; }
    }
}