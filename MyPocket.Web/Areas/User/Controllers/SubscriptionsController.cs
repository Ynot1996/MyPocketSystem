using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MyPocket.Services;
using System.Security.Claims;

namespace MyPocket.Web.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Roles = "FreeMember,PaidMember")]
    public class SubscriptionsController : Controller
    {
        private readonly ISubscriptionService _subscriptionService;

        public SubscriptionsController(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Plans()
        {
            var plans = await _subscriptionService.GetAllPlansAsync();

            if (User.Identity?.IsAuthenticated ?? false)
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (Guid.TryParse(userIdString, out Guid userId))
                {
                    ViewBag.CurrentSubscription = await _subscriptionService.GetActiveSubscriptionAsync(userId);
                    ViewBag.IsSubscriptionActive = await _subscriptionService.IsSubscriptionActiveAsync(userId);
                }
            }

            return View(plans);
        }

        [HttpGet]
        public async Task<IActionResult> CurrentPlan()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var subscription = await _subscriptionService.GetActiveSubscriptionAsync(userId);
            if (subscription == null)
            {
                TempData["ErrorMessage"] = "您目前沒有活躍的訂閱方案。";
                return RedirectToAction("Plans");
            }

            return View(subscription);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Subscribe(Guid planId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var plan = await _subscriptionService.GetPlanByIdAsync(planId);
            if (plan == null)
            {
                TempData["ErrorMessage"] = "找不到指定的訂閱方案。";
                return RedirectToAction("Plans");
            }

            var result = await _subscriptionService.SubscribeAsync(userId, planId);
            if (result)
            {
                TempData["SuccessMessage"] = $"成功訂閱 {plan.PlanName}！";
                return RedirectToAction("Plans");
            }

            TempData["ErrorMessage"] = "訂閱失敗，請稍後再試。";
            return RedirectToAction("Plans");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var isActive = await _subscriptionService.IsSubscriptionActiveAsync(userId);
            if (!isActive)
            {
                TempData["ErrorMessage"] = "您目前沒有活躍的訂閱。";
                return RedirectToAction("Plans");
            }

            var result = await _subscriptionService.CancelSubscriptionAsync(userId);
            if (result)
            {
                TempData["SuccessMessage"] = "已成功取消訂閱。";
            }
            else
            {
                TempData["ErrorMessage"] = "取消訂閱失敗，請稍後再試。";
            }

            return RedirectToAction("Plans");
        }
    }
}