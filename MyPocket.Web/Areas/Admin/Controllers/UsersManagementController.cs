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
                TempData["SuccessMessage"] = success ? "訂閱計劃更新成功。" : "更新訂閱時發生錯誤。";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"更新訂閱時發生錯誤：{ex.Message}";
            }

            return RedirectToAction(nameof(UserSubscriptionHistory), new { userId });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSelectedSubscriptions([FromBody] UpdateSubscriptionRequest request)
        {
            if (request?.UserIds == null || request.UserIds.Count == 0 || request.PlanId == Guid.Empty)
            {
                return Json(new { success = false, message = "無效的請求參數。" });
            }

            try
            {
                foreach (var userId in request.UserIds)
                {
                    await _subscriptionService.SubscribeAsync(userId, request.PlanId);
                }
                return Json(new { success = true, message = $"已成功更新 {request.UserIds.Count} 位使用者的訂閱計劃。" });
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
                    .FirstOrDefaultAsync(p => p.PlanName.Contains("基本") || p.PlanName.Contains("免費"));

                if (basicPlan == null)
                {
                    TempData["ErrorMessage"] = "找不到基本方案。";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var user in users)
                {
                    user.Role = "FreeMember";
                    user.UpdatedAt = DateTime.UtcNow;
                    await _subscriptionService.SubscribeAsync(user.UserId, basicPlan.PlanId);
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "已成功將所有使用者更新為免費會員，並訂閱基本方案。";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"更新使用者狀態時發生錯誤：{ex.Message}";
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