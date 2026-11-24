using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPocket.Services.Interfaces;
using MyPocket.Shared.Resources;
using MyPocket.Shared.ViewModels.Gamification;
using MyPocket.Web.Infrastructure;

namespace MyPocket.Web.Areas.User.Controllers
{
    /// <summary>Spend coins on decorations, characters and outfits for the city.</summary>
    [Area("User")]
    [Authorize(Roles = "FreeMember,PaidMember")]
    public class ShopController : Controller
    {
        private readonly IShopService _shop;
        private readonly IGamificationService _gamification;
        private readonly ILocalizationService _localizer;
        private readonly IAppTelemetry _telemetry;

        public ShopController(IShopService shop, IGamificationService gamification, ILocalizationService localizer, IAppTelemetry telemetry)
        {
            _shop = shop;
            _gamification = gamification;
            _localizer = localizer;
            _telemetry = telemetry;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.UserIdOrNull();
            if (userId == null) return RedirectToAction("Login", "Account", new { area = "" });

            ViewBag.Coins = await _gamification.GetBalanceAsync(userId.Value);
            return View(await _shop.GetShopAsync(userId.Value));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Buy(string itemCode)
        {
            var userId = User.UserIdOrNull();
            if (userId == null) return RedirectToAction("Login", "Account", new { area = "" });

            var result = await _shop.PurchaseAsync(userId.Value, itemCode);
            switch (result)
            {
                case PurchaseResult.Success:
                    TempData["SuccessMessage"] = _localizer.GetString("PurchaseSuccess");
                    _telemetry.TrackEvent("ShopPurchase", new Dictionary<string, string> { ["item"] = itemCode });
                    break;
                case PurchaseResult.InsufficientCoins:
                    TempData["ErrorMessage"] = _localizer.GetString("PurchaseNotEnoughCoins");
                    break;
                case PurchaseResult.AlreadyOwned:
                    TempData["ErrorMessage"] = _localizer.GetString("PurchaseAlreadyOwned");
                    break;
                default:
                    TempData["ErrorMessage"] = _localizer.GetString("PurchaseUnknownItem");
                    break;
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Wear(string itemCode)
        {
            var userId = User.UserIdOrNull();
            if (userId == null) return RedirectToAction("Login", "Account", new { area = "" });

            if (!await _shop.ToggleOutfitAsync(userId.Value, itemCode))
                TempData["ErrorMessage"] = _localizer.GetString("PurchaseUnknownItem");
            return RedirectToAction(nameof(Index));
        }
    }
}
