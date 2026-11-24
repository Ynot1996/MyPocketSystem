using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MyPocket.Services;
using MyPocket.Shared.Resources;
using MyPocket.Web.Infrastructure;
using System.Security.Claims;

namespace MyPocket.Web.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Roles = "FreeMember,PaidMember")]
    public class SubscriptionsController : Controller
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly ILocalizationService _localizer;

        public SubscriptionsController(ISubscriptionService subscriptionService, ILocalizationService localizer)
        {
            _subscriptionService = subscriptionService;
            _localizer = localizer;
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
                TempData["ErrorMessage"] = _localizer.GetString("NoActiveSubscription");
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

            // Paying for a plan needs a real account; guest data would otherwise be
            // tied to a disposable identity.
            if (User.IsGuestUser())
            {
                TempData["ErrorMessage"] = _localizer.GetString("GuestMustRegisterToSubscribe");
                return RedirectToAction("Register", "Account", new { area = "" });
            }

            var plan = await _subscriptionService.GetPlanByIdAsync(planId);
            if (plan == null)
            {
                TempData["ErrorMessage"] = _localizer.GetString("PlanNotFound");
                return RedirectToAction("Plans");
            }

            var result = await _subscriptionService.SubscribeAsync(userId, planId);
            if (result)
            {
                TempData["SuccessMessage"] = string.Format(
                    _localizer.GetString("SubscribedSuccess"),
                    _localizer.GetString(plan.PlanName));
                return RedirectToAction("Plans");
            }

            TempData["ErrorMessage"] = _localizer.GetString("SubscribeFailed");
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
                TempData["ErrorMessage"] = _localizer.GetString("NoActiveSubscription");
                return RedirectToAction("Plans");
            }

            var result = await _subscriptionService.CancelSubscriptionAsync(userId);
            if (result)
            {
                TempData["SuccessMessage"] = _localizer.GetString("CancelledSuccess");
            }
            else
            {
                TempData["ErrorMessage"] = _localizer.GetString("CancelFailed");
            }

            return RedirectToAction("Plans");
        }
    }
}
