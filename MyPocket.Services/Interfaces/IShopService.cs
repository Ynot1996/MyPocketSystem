using MyPocket.Shared.ViewModels.Gamification;

namespace MyPocket.Services.Interfaces
{
    public interface IShopService
    {
        /// <summary>Full catalog annotated with what this user owns / has equipped.</summary>
        Task<IReadOnlyList<ShopEntry>> GetShopAsync(Guid userId);

        /// <summary>Buys an item with coins. Outfits are equipped automatically.</summary>
        Task<PurchaseResult> PurchaseAsync(Guid userId, string itemCode);

        /// <summary>Wears an owned outfit (or takes it off if already worn). False if not owned / not an outfit.</summary>
        Task<bool> ToggleOutfitAsync(Guid userId, string itemCode);
    }
}
