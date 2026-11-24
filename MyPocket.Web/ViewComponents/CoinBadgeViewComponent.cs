using Microsoft.AspNetCore.Mvc;
using MyPocket.Services.Interfaces;
using MyPocket.Web.Infrastructure;

namespace MyPocket.Web.ViewComponents
{
    /// <summary>Nav-bar coin counter, linking to the shop. Renders nothing for anonymous visitors.</summary>
    public class CoinBadgeViewComponent : ViewComponent
    {
        private readonly IGamificationService _gamification;

        public CoinBadgeViewComponent(IGamificationService gamification)
        {
            _gamification = gamification;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = UserClaimsPrincipal.UserIdOrNull();
            if (userId == null)
            {
                return Content(string.Empty);
            }

            var balance = await _gamification.GetBalanceAsync(userId.Value);
            return View(balance);
        }
    }
}
