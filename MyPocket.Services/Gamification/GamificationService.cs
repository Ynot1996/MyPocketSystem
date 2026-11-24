using Microsoft.EntityFrameworkCore;
using MyPocket.Core.Models;
using MyPocket.DataAccess.Data;
using MyPocket.Services.Interfaces;
using MyPocket.Shared.ViewModels.Gamification;

namespace MyPocket.Services.Gamification
{
    public class GamificationService : IGamificationService
    {
        public const string ReasonTransactionLogged = "TransactionLogged";
        public const string ReasonAutoCapture = "AutoCaptureBonus";
        public const string ReasonStreak = "StreakBonus";
        public const string ReasonWelcome = "WelcomeBonus";

        public const int CoinsPerTransaction = 10;
        public const int AutoCaptureBonus = 5;
        /// <summary>Only this many transactions per day earn coins, so the reward can't be farmed.</summary>
        public const int DailyRewardedTransactions = 10;
        public const int StreakBonusPerDay = 5;
        public const int StreakBonusMaxDays = 7;
        public const int WelcomeBonus = 50;

        private readonly MyPocketDBContext _context;
        private readonly TimeProvider _time;

        public GamificationService(MyPocketDBContext context, TimeProvider time)
        {
            _context = context;
            _time = time;
        }

        public async Task<int> GetBalanceAsync(Guid userId) =>
            await _context.CoinLedger.Where(e => e.UserId == userId).SumAsync(e => (int?)e.Amount) ?? 0;

        public async Task<CoinAward> AwardForTransactionAsync(Guid userId, bool autoCaptured)
        {
            var now = _time.GetUtcNow().UtcDateTime;
            var today = now.Date;
            var tomorrow = today.AddDays(1);

            var rewardedToday = await _context.CoinLedger.CountAsync(e =>
                e.UserId == userId &&
                e.Reason == ReasonTransactionLogged &&
                e.CreatedAt >= today && e.CreatedAt < tomorrow);

            if (rewardedToday >= DailyRewardedTransactions)
            {
                return new CoinAward(0, await GetStreakAsync(userId), DailyCapReached: true, Array.Empty<string>());
            }

            var reasons = new List<string> { ReasonTransactionLogged };
            var coins = CoinsPerTransaction;
            _context.CoinLedger.Add(Entry(userId, CoinsPerTransaction, ReasonTransactionLogged, now));

            if (autoCaptured)
            {
                reasons.Add(ReasonAutoCapture);
                coins += AutoCaptureBonus;
                _context.CoinLedger.Add(Entry(userId, AutoCaptureBonus, ReasonAutoCapture, now));
            }

            await _context.SaveChangesAsync();

            // Streak bonus is paid once per day, on the first rewarded entry.
            var streak = await GetStreakAsync(userId);
            if (rewardedToday == 0 && streak > 0)
            {
                var bonus = StreakBonusPerDay * Math.Min(streak, StreakBonusMaxDays);
                reasons.Add(ReasonStreak);
                coins += bonus;
                _context.CoinLedger.Add(Entry(userId, bonus, ReasonStreak, now));
                await _context.SaveChangesAsync();
            }

            return new CoinAward(coins, streak, DailyCapReached: false, reasons);
        }

        public async Task<int> GetStreakAsync(Guid userId)
        {
            var today = _time.GetUtcNow().UtcDateTime.Date;
            var since = today.AddDays(-90);

            var days = (await _context.CoinLedger
                    .Where(e => e.UserId == userId && e.Reason == ReasonTransactionLogged && e.CreatedAt >= since)
                    .Select(e => e.CreatedAt)
                    .ToListAsync())
                .Select(d => d.Date)
                .ToHashSet();

            // A streak is still "alive" if the user logged yesterday but not yet today.
            var cursor = days.Contains(today) ? today : today.AddDays(-1);
            var streak = 0;
            while (days.Contains(cursor))
            {
                streak++;
                cursor = cursor.AddDays(-1);
            }
            return streak;
        }

        public async Task EnsureWelcomeBonusAsync(Guid userId)
        {
            var alreadyGranted = await _context.CoinLedger.AnyAsync(e => e.UserId == userId && e.Reason == ReasonWelcome);
            if (alreadyGranted) return;

            _context.CoinLedger.Add(Entry(userId, WelcomeBonus, ReasonWelcome, _time.GetUtcNow().UtcDateTime));
            await _context.SaveChangesAsync();
        }

        public async Task<CityViewModel> GetCityAsync(Guid userId)
        {
            var groups = await _context.Transactions
                .Where(t => t.UserId == userId && !t.IsDeleted)
                .GroupBy(t => t.Category.CategoryName)
                .Select(g => new { Name = g.Key, Count = g.Count(), FirstUsed = g.Min(t => t.CreatedAt) })
                .ToListAsync();

            var slots = CityCatalog.SlotOrder;
            var slotIndex = 0;

            // Oldest category first, so the earliest habits sit in the city centre.
            var buildings = new List<CityBuilding>();
            foreach (var g in groups.OrderBy(g => g.FirstUsed).ThenBy(g => g.Name))
            {
                if (slotIndex >= slots.Count) break;
                var spec = CityCatalog.For(g.Name);
                buildings.Add(new CityBuilding(
                    CategoryKey: g.Name,
                    BuildingType: spec.Type,
                    Emoji: spec.Emoji,
                    Color: spec.Color,
                    Level: CityCatalog.LevelFor(g.Count),
                    TransactionCount: g.Count,
                    NextLevelAt: CityCatalog.NextLevelAt(g.Count),
                    Slot: slots[slotIndex++]));
            }

            var owned = await _context.UserItems
                .Where(i => i.UserId == userId)
                .OrderBy(i => i.PurchasedAt)
                .ToListAsync();

            var decorations = new List<CityPlacedItem>();
            var characters = new List<CityPlacedItem>();
            string? outfit = null;

            foreach (var userItem in owned)
            {
                var item = ShopCatalog.Find(userItem.ItemCode);
                if (item == null) continue;

                switch (item.Kind)
                {
                    case ShopItemKind.Decoration when slotIndex < slots.Count:
                        decorations.Add(new CityPlacedItem(item.Code, item.Kind, item.Emoji, slots[slotIndex++]));
                        break;
                    case ShopItemKind.Character:
                        // Characters walk the streets rather than occupying a lot.
                        characters.Add(new CityPlacedItem(item.Code, item.Kind, item.Emoji, -1));
                        break;
                    case ShopItemKind.Outfit when userItem.IsEquipped:
                        outfit = item.Emoji;
                        break;
                }
            }

            return new CityViewModel(buildings, decorations, characters, outfit, CityCatalog.GridSize);
        }

        private static CoinLedgerEntry Entry(Guid userId, int amount, string reason, DateTime at) => new()
        {
            EntryId = Guid.NewGuid(),
            UserId = userId,
            Amount = amount,
            Reason = reason,
            CreatedAt = at
        };
    }
}
