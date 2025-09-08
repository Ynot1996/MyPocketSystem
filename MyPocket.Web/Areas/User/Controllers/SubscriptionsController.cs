using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MyPocket.Services;
using System.Security.Claims;

namespace MyPocket.Web.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Roles = "FreeMember")]
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
                TempData["ErrorMessage"] = "�z�ثe�S�����D���q�\��סC";
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
                TempData["ErrorMessage"] = "�䤣����w���q�\��סC";
                return RedirectToAction("Plans");
            }

            var result = await _subscriptionService.SubscribeAsync(userId, planId);
            if (result)
            {
                TempData["SuccessMessage"] = $"���\�q�\ {plan.PlanName}�I";
                return RedirectToAction("Plans");
            }

            TempData["ErrorMessage"] = "�q�\���ѡA�еy��A�աC";
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
                TempData["ErrorMessage"] = "�z�ثe�S�����D���q�\�C";
                return RedirectToAction("Plans");
            }

            var result = await _subscriptionService.CancelSubscriptionAsync(userId);
            if (result)
            {
                TempData["SuccessMessage"] = "�w���\�����q�\�C";
            }
            else
            {
                TempData["ErrorMessage"] = "�����q�\���ѡA�еy��A�աC";
            }

            return RedirectToAction("Plans");
        }
    }
}