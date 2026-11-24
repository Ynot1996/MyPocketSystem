using Microsoft.EntityFrameworkCore;
using MyPocket.Core.Models;
using MyPocket.DataAccess.Data;
using MyPocket.Services.Interfaces;

namespace MyPocket.Services.Accounts
{
    public class AccountService : IAccountService
    {
        /// <summary>Reserved TLD (RFC 2606) so a guest address can never collide with a real inbox.</summary>
        public const string GuestEmailDomain = "guest.invalid";
        private const string AdminEmail = "admin@example.com";

        private readonly MyPocketDBContext _context;
        private readonly ISubscriptionService _subscriptions;
        private readonly IGamificationService _gamification;
        private readonly TimeProvider _time;

        public AccountService(
            MyPocketDBContext context,
            ISubscriptionService subscriptions,
            IGamificationService gamification,
            TimeProvider time)
        {
            _context = context;
            _subscriptions = subscriptions;
            _gamification = gamification;
            _time = time;
        }

        private DateTime Now => _time.GetUtcNow().UtcDateTime;

        public async Task<User> CreateGuestAsync()
        {
            var now = Now;
            var guest = new User
            {
                UserId = Guid.NewGuid(),
                Email = $"guest-{Guid.NewGuid():N}@{GuestEmailDomain}",
                // Unusable password: guests sign in only via their cookie.
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString("N")),
                Nickname = "Guest",
                Role = "FreeMember",
                IsGuest = true,
                CreationDate = now,
                LastLoginDate = now,
                UpdatedAt = now
            };
            _context.Users.Add(guest);
            await _context.SaveChangesAsync();

            await SeedDemoDataAsync(guest.UserId, now);
            await _gamification.EnsureWelcomeBonusAsync(guest.UserId);
            return guest;
        }

        public async Task<GuestUpgradeResult> UpgradeGuestAsync(Guid guestId, string email, string password, string? nickname)
        {
            var guest = await _context.Users.FirstOrDefaultAsync(u => u.UserId == guestId && u.IsGuest && !u.IsDeleted);
            if (guest == null) return GuestUpgradeResult.NotAGuest;

            if (await EmailInUseAsync(email, exceptUserId: guestId)) return GuestUpgradeResult.EmailTaken;

            guest.Email = email.Trim();
            guest.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            guest.Nickname = string.IsNullOrWhiteSpace(nickname) ? email.Split('@')[0] : nickname.Trim();
            guest.IsGuest = false;
            guest.UpdatedAt = Now;
            await _context.SaveChangesAsync();

            await _subscriptions.SubscribeToBasicPlanAsync(guest.UserId);
            return GuestUpgradeResult.Upgraded;
        }

        public async Task<User> ResolveExternalLoginAsync(string email, string? displayName, Guid? currentGuestId)
        {
            var now = Now;
            var existing = await _context.Users.FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted && !u.IsGuest);
            var guest = currentGuestId.HasValue
                ? await _context.Users.FirstOrDefaultAsync(u => u.UserId == currentGuestId.Value && u.IsGuest && !u.IsDeleted)
                : null;

            if (existing != null)
            {
                if (guest != null)
                {
                    await MergeGuestIntoAsync(guest.UserId, existing.UserId);
                }
                existing.LastLoginDate = now;
                await _context.SaveChangesAsync();
                return existing;
            }

            if (guest != null)
            {
                // New email + guest session: upgrade the guest so its data is kept.
                guest.Email = email;
                guest.Nickname = displayName ?? email.Split('@')[0];
                guest.IsGuest = false;
                guest.LastLoginDate = now;
                guest.UpdatedAt = now;
                await _context.SaveChangesAsync();
                await _subscriptions.SubscribeToBasicPlanAsync(guest.UserId);
                return guest;
            }

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Email = email,
                // External-only account: the local password path can never match this.
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString("N")),
                Nickname = displayName ?? email.Split('@')[0],
                Role = "FreeMember",
                CreationDate = now,
                LastLoginDate = now,
                UpdatedAt = now
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            await _subscriptions.SubscribeToBasicPlanAsync(user.UserId);
            await _gamification.EnsureWelcomeBonusAsync(user.UserId);
            return user;
        }

        public async Task MergeGuestIntoAsync(Guid guestId, Guid targetUserId)
        {
            if (guestId == targetUserId) return;

            var guest = await _context.Users.FirstOrDefaultAsync(u => u.UserId == guestId && u.IsGuest && !u.IsDeleted);
            if (guest == null) return;

            foreach (var t in await _context.Transactions.Where(t => t.UserId == guestId).ToListAsync())
                t.UserId = targetUserId;

            foreach (var c in await _context.Categories.Where(c => c.UserId == guestId).ToListAsync())
                c.UserId = targetUserId;

            // Budgets and saving goals are one-per-period; keep the account's own on conflict.
            var targetBudgets = await _context.Budgets
                .Where(b => b.UserId == targetUserId && !b.IsDeleted)
                .Select(b => new { b.CategoryId, b.BudgetYear, b.BudgetMonth })
                .ToListAsync();
            foreach (var b in await _context.Budgets.Where(b => b.UserId == guestId).ToListAsync())
            {
                if (targetBudgets.Any(t => t.CategoryId == b.CategoryId && t.BudgetYear == b.BudgetYear && t.BudgetMonth == b.BudgetMonth))
                    b.IsDeleted = true;
                b.UserId = targetUserId;
            }

            var targetGoalNames = await _context.SavingGoals
                .Where(g => g.UserId == targetUserId && !g.IsDeleted)
                .Select(g => g.GoalName)
                .ToListAsync();
            foreach (var g in await _context.SavingGoals.Where(g => g.UserId == guestId).ToListAsync())
            {
                if (targetGoalNames.Contains(g.GoalName)) g.IsDeleted = true;
                g.UserId = targetUserId;
            }

            // Coins earned as a guest carry over; a second welcome bonus does not.
            var targetHasWelcome = await _context.CoinLedger.AnyAsync(e =>
                e.UserId == targetUserId && e.Reason == Gamification.GamificationService.ReasonWelcome);
            foreach (var e in await _context.CoinLedger.Where(e => e.UserId == guestId).ToListAsync())
            {
                if (targetHasWelcome && e.Reason == Gamification.GamificationService.ReasonWelcome)
                    _context.CoinLedger.Remove(e);
                else
                    e.UserId = targetUserId;
            }

            // Items are unique per user; drop the guest's copy of anything already owned.
            var targetItems = await _context.UserItems.Where(i => i.UserId == targetUserId).Select(i => i.ItemCode).ToListAsync();
            foreach (var item in await _context.UserItems.Where(i => i.UserId == guestId).ToListAsync())
            {
                if (targetItems.Contains(item.ItemCode))
                    _context.UserItems.Remove(item);
                else
                    item.UserId = targetUserId;
            }

            guest.IsDeleted = true;
            guest.UpdatedAt = Now;
            await _context.SaveChangesAsync();
        }

        public async Task<int> CleanupStaleGuestsAsync(TimeSpan olderThan)
        {
            var cutoff = Now - olderThan;
            var stale = await _context.Users
                .Where(u => u.IsGuest && !u.IsDeleted && u.CreationDate < cutoff)
                .ToListAsync();
            foreach (var g in stale)
            {
                g.IsDeleted = true;
                g.UpdatedAt = Now;
            }
            await _context.SaveChangesAsync();
            return stale.Count;
        }

        private Task<bool> EmailInUseAsync(string email, Guid exceptUserId) =>
            _context.Users.AnyAsync(u => u.Email == email && u.UserId != exceptUserId && !u.IsDeleted);

        /// <summary>
        /// A week of believable activity for a UK-based user who also spends in
        /// other currencies, so a first-time visitor immediately sees the
        /// multi-currency totals, a growing city and a budget in progress.
        /// </summary>
        private async Task SeedDemoDataAsync(Guid guestId, DateTime now)
        {
            var defaults = await _context.Categories
                .Where(c => c.User.Email == AdminEmail && !c.IsDeleted)
                .ToListAsync();
            Category? Cat(string name) => defaults.FirstOrDefault(c => c.CategoryName == name);

            var demo = new (string Category, decimal Amount, string Currency, int DaysAgo, string Note)[]
            {
                ("餐飲", 8.50m, "GBP", 0, "Lunch"),
                ("餐飲", 3.40m, "GBP", 1, "Coffee"),
                ("交通", 20.00m, "GBP", 1, "Oyster top-up"),
                ("餐飲", 12.80m, "GBP", 2, "Dinner with friends"),
                ("購物", 35.00m, "EUR", 3, "Weekend trip shopping"),
                ("娛樂", 1800m, "JPY", 4, "Cinema in Tokyo"),
                ("薪資", 420.00m, "GBP", 5, "Part-time pay"),
                ("其他收入", 5000m, "TWD", 6, "Family transfer")
            };

            foreach (var d in demo)
            {
                var category = Cat(d.Category);
                if (category == null) continue;
                var at = now.AddDays(-d.DaysAgo);
                _context.Transactions.Add(new Transaction
                {
                    TransactionId = Guid.NewGuid(),
                    UserId = guestId,
                    CategoryId = category.CategoryId,
                    Amount = d.Amount,
                    Currency = d.Currency,
                    TransactionType = category.CategoryType,
                    TransactionDate = at,
                    Description = d.Note,
                    CreatedAt = at,
                    UpdatedAt = at
                });
            }

            var dining = Cat("餐飲");
            if (dining != null)
            {
                _context.Budgets.Add(new Budget
                {
                    BudgetId = Guid.NewGuid(),
                    UserId = guestId,
                    CategoryId = dining.CategoryId,
                    Amount = 150m,
                    BudgetYear = now.Year.ToString(),
                    BudgetMonth = now.Month.ToString("D2"),
                    CreatedAt = now,
                    UpdatedAt = now
                });
            }

            await _context.SaveChangesAsync();
        }
    }
}
