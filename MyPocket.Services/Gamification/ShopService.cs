using Microsoft.EntityFrameworkCore;
using MyPocket.Core.Models;
using MyPocket.DataAccess.Data;
using MyPocket.Services.Interfaces;
using MyPocket.Shared.ViewModels.Gamification;

namespace MyPocket.Services.Gamification
{
    public class ShopService : IShopService
    {
        private readonly MyPocketDBContext _context;
        private readonly IGamificationService _gamification;
        private readonly TimeProvider _time;

        public ShopService(MyPocketDBContext context, IGamificationService gamification, TimeProvider time)
        {
            _context = context;
            _gamification = gamification;
            _time = time;
        }

        public async Task<IReadOnlyList<ShopEntry>> GetShopAsync(Guid userId)
        {
            var owned = await _context.UserItems
                .Where(i => i.UserId == userId)
                .ToDictionaryAsync(i => i.ItemCode, i => i.IsEquipped);

            return ShopCatalog.Items
                .Select(item => new ShopEntry(
                    item,
                    Owned: owned.ContainsKey(item.Code),
                    Equipped: owned.TryGetValue(item.Code, out var equipped) && equipped))
                .ToList();
        }

        public async Task<PurchaseResult> PurchaseAsync(Guid userId, string itemCode)
        {
            var item = ShopCatalog.Find(itemCode);
            if (item == null) return PurchaseResult.UnknownItem;

            if (await _context.UserItems.AnyAsync(i => i.UserId == userId && i.ItemCode == item.Code))
                return PurchaseResult.AlreadyOwned;

            var balance = await _gamification.GetBalanceAsync(userId);
            if (balance < item.Price) return PurchaseResult.InsufficientCoins;

            var now = _time.GetUtcNow().UtcDateTime;
            var isOutfit = item.Kind == ShopItemKind.Outfit;

            if (isOutfit)
            {
                await UnequipAllOutfitsAsync(userId);
            }

            _context.CoinLedger.Add(new CoinLedgerEntry
            {
                EntryId = Guid.NewGuid(),
                UserId = userId,
                Amount = -item.Price,
                Reason = $"ShopPurchase:{item.Code}",
                CreatedAt = now
            });
            _context.UserItems.Add(new UserItem
            {
                UserItemId = Guid.NewGuid(),
                UserId = userId,
                ItemCode = item.Code,
                IsEquipped = isOutfit,
                PurchasedAt = now
            });

            // The unique (UserId, ItemCode) index is the backstop if two purchase
            // requests for the same item race past the AnyAsync check above.
            await _context.SaveChangesAsync();
            return PurchaseResult.Success;
        }

        public async Task<bool> ToggleOutfitAsync(Guid userId, string itemCode)
        {
            var item = ShopCatalog.Find(itemCode);
            if (item == null || item.Kind != ShopItemKind.Outfit) return false;

            var owned = await _context.UserItems.FirstOrDefaultAsync(i => i.UserId == userId && i.ItemCode == item.Code);
            if (owned == null) return false;

            var wasEquipped = owned.IsEquipped;
            await UnequipAllOutfitsAsync(userId);
            owned.IsEquipped = !wasEquipped;
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task UnequipAllOutfitsAsync(Guid userId)
        {
            var outfitCodes = ShopCatalog.Items.Where(i => i.Kind == ShopItemKind.Outfit).Select(i => i.Code).ToList();
            var worn = await _context.UserItems
                .Where(i => i.UserId == userId && i.IsEquipped && outfitCodes.Contains(i.ItemCode))
                .ToListAsync();
            foreach (var w in worn) w.IsEquipped = false;
        }
    }
}
