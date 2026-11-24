using Microsoft.EntityFrameworkCore;
using MyPocket.Core.Models;
using MyPocket.DataAccess.Data;
using MyPocket.Services;
using MyPocket.Services.Accounts;
using MyPocket.Services.Gamification;
using MyPocket.Services.Interfaces;

namespace MyPocket.Tests
{
    public class AccountServiceTests
    {
        private static readonly DateTime Now = new(2026, 11, 20, 12, 0, 0, DateTimeKind.Utc);

        private static (AccountService accounts, GamificationService gamification, FixedTimeProvider clock) Build(MyPocketDBContext context)
        {
            var clock = new FixedTimeProvider(Now);
            var gamification = new GamificationService(context, clock);
            var accounts = new AccountService(context, new SubscriptionService(context), gamification, clock);
            return (accounts, gamification, clock);
        }

        /// <summary>Admin + the default categories the guest demo draws from + the free plan.</summary>
        private static void SeedReferenceData(MyPocketDBContext context)
        {
            var admin = TestHelper.SeedAdmin(context);
            foreach (var (name, type) in new[]
            {
                ("餐飲", "支出"), ("交通", "支出"), ("購物", "支出"), ("娛樂", "支出"),
                ("薪資", "收入"), ("其他收入", "收入")
            })
            {
                TestHelper.SeedCategory(context, admin.UserId, name, type);
            }
            TestHelper.SeedPlan(context, "免費基本會員", 0m, 36500);
        }

        [Fact]
        public async Task CreateGuest_SeedsMultiCurrencyDemo_AndWelcomeBonus()
        {
            using var context = TestHelper.NewContext();
            SeedReferenceData(context);
            var (accounts, gamification, _) = Build(context);

            var guest = await accounts.CreateGuestAsync();

            Assert.True(guest.IsGuest);
            Assert.EndsWith("@" + AccountService.GuestEmailDomain, guest.Email);
            var transactions = await context.Transactions.Where(t => t.UserId == guest.UserId).ToListAsync();
            Assert.Equal(8, transactions.Count);
            Assert.Contains(transactions, t => t.Currency == "JPY");
            Assert.Contains(transactions, t => t.Currency == "TWD");
            Assert.Contains(transactions, t => t.Currency == "EUR");
            Assert.Single(context.Budgets.Where(b => b.UserId == guest.UserId));
            Assert.Equal(GamificationService.WelcomeBonus, await gamification.GetBalanceAsync(guest.UserId));
        }

        [Fact]
        public async Task UpgradeGuest_KeepsDataAndClearsGuestFlag()
        {
            using var context = TestHelper.NewContext();
            SeedReferenceData(context);
            var (accounts, _, _) = Build(context);
            var guest = await accounts.CreateGuestAsync();

            var result = await accounts.UpgradeGuestAsync(guest.UserId, "tony@example.com", "s3cret-pass", "Tony");

            Assert.Equal(GuestUpgradeResult.Upgraded, result);
            var upgraded = await context.Users.SingleAsync(u => u.UserId == guest.UserId);
            Assert.False(upgraded.IsGuest);
            Assert.Equal("tony@example.com", upgraded.Email);
            Assert.True(BCrypt.Net.BCrypt.Verify("s3cret-pass", upgraded.PasswordHash));
            Assert.Equal(8, await context.Transactions.CountAsync(t => t.UserId == guest.UserId));
            Assert.True(await context.UserSubscriptions.AnyAsync(s => s.UserId == guest.UserId));
        }

        [Fact]
        public async Task UpgradeGuest_WithRegisteredEmail_IsRejected()
        {
            using var context = TestHelper.NewContext();
            SeedReferenceData(context);
            TestHelper.SeedUser(context, "taken@example.com");
            var (accounts, _, _) = Build(context);
            var guest = await accounts.CreateGuestAsync();

            var result = await accounts.UpgradeGuestAsync(guest.UserId, "taken@example.com", "s3cret-pass", null);

            Assert.Equal(GuestUpgradeResult.EmailTaken, result);
            Assert.True((await context.Users.SingleAsync(u => u.UserId == guest.UserId)).IsGuest);
        }

        [Fact]
        public async Task UpgradeGuest_OnRegularAccount_IsRejected()
        {
            using var context = TestHelper.NewContext();
            SeedReferenceData(context);
            var regular = TestHelper.SeedUser(context, "regular@example.com");
            var (accounts, _, _) = Build(context);

            var result = await accounts.UpgradeGuestAsync(regular.UserId, "new@example.com", "s3cret-pass", null);

            Assert.Equal(GuestUpgradeResult.NotAGuest, result);
        }

        [Fact]
        public async Task ExternalLogin_NewEmailWithoutGuest_CreatesFreeMember()
        {
            using var context = TestHelper.NewContext();
            SeedReferenceData(context);
            var (accounts, gamification, _) = Build(context);

            var user = await accounts.ResolveExternalLoginAsync("new@gmail.com", "New Person", currentGuestId: null);

            Assert.Equal("FreeMember", user.Role);
            Assert.False(user.IsGuest);
            Assert.Equal("New Person", user.Nickname);
            Assert.Equal(GamificationService.WelcomeBonus, await gamification.GetBalanceAsync(user.UserId));
        }

        [Fact]
        public async Task ExternalLogin_NewEmailWithGuest_UpgradesTheGuestInPlace()
        {
            using var context = TestHelper.NewContext();
            SeedReferenceData(context);
            var (accounts, _, _) = Build(context);
            var guest = await accounts.CreateGuestAsync();

            var user = await accounts.ResolveExternalLoginAsync("tony@gmail.com", "Tony", guest.UserId);

            Assert.Equal(guest.UserId, user.UserId);
            Assert.False(user.IsGuest);
            Assert.Equal("tony@gmail.com", user.Email);
            Assert.Equal(8, await context.Transactions.CountAsync(t => t.UserId == user.UserId));
        }

        [Fact]
        public async Task ExternalLogin_ExistingEmailWithGuest_MergesGuestDataAndClosesGuest()
        {
            using var context = TestHelper.NewContext();
            SeedReferenceData(context);
            var (accounts, gamification, _) = Build(context);

            var existing = await accounts.ResolveExternalLoginAsync("tony@gmail.com", "Tony", null); // has a welcome bonus
            var dining = await context.Categories.FirstAsync(c => c.CategoryName == "餐飲");
            TestHelper.SeedTransaction(context, existing.UserId, dining, 5m, Now);

            var guest = await accounts.CreateGuestAsync();
            await gamification.AwardForTransactionAsync(guest.UserId, autoCaptured: false); // 15 coins as a guest

            var resolved = await accounts.ResolveExternalLoginAsync("tony@gmail.com", "Tony", guest.UserId);

            Assert.Equal(existing.UserId, resolved.UserId);
            Assert.Equal(9, await context.Transactions.CountAsync(t => t.UserId == existing.UserId));
            Assert.True((await context.Users.SingleAsync(u => u.UserId == guest.UserId)).IsDeleted);
            // One welcome bonus (not two) + the coins earned while a guest.
            Assert.Equal(GamificationService.WelcomeBonus + 15, await gamification.GetBalanceAsync(existing.UserId));
        }

        [Fact]
        public async Task Merge_DropsGuestCopiesOfItemsAlreadyOwned()
        {
            using var context = TestHelper.NewContext();
            SeedReferenceData(context);
            var (accounts, _, _) = Build(context);
            var target = TestHelper.SeedUser(context, "target@example.com");
            var guest = await accounts.CreateGuestAsync();

            context.UserItems.AddRange(
                new UserItem { UserItemId = Guid.NewGuid(), UserId = target.UserId, ItemCode = "tree", PurchasedAt = Now },
                new UserItem { UserItemId = Guid.NewGuid(), UserId = guest.UserId, ItemCode = "tree", PurchasedAt = Now },
                new UserItem { UserItemId = Guid.NewGuid(), UserId = guest.UserId, ItemCode = "cat", PurchasedAt = Now });
            context.SaveChanges();

            await accounts.MergeGuestIntoAsync(guest.UserId, target.UserId);

            var owned = await context.UserItems.Where(i => i.UserId == target.UserId).Select(i => i.ItemCode).ToListAsync();
            Assert.Equal(2, owned.Count);
            Assert.Contains("tree", owned);
            Assert.Contains("cat", owned);
        }

        [Fact]
        public async Task CleanupStaleGuests_ClosesOnlyGuestsPastTheCutoff()
        {
            using var context = TestHelper.NewContext();
            SeedReferenceData(context);
            var (accounts, _, clock) = Build(context);

            var oldGuest = await accounts.CreateGuestAsync();
            clock.Advance(TimeSpan.FromDays(8));
            var freshGuest = await accounts.CreateGuestAsync();
            var regular = TestHelper.SeedUser(context, "keep@example.com");

            var closed = await accounts.CleanupStaleGuestsAsync(TimeSpan.FromDays(7));

            Assert.Equal(1, closed);
            Assert.True((await context.Users.SingleAsync(u => u.UserId == oldGuest.UserId)).IsDeleted);
            Assert.False((await context.Users.SingleAsync(u => u.UserId == freshGuest.UserId)).IsDeleted);
            Assert.False((await context.Users.SingleAsync(u => u.UserId == regular.UserId)).IsDeleted);
        }
    }
}
