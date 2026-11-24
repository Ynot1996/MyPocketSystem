using MyPocket.DataAccess.Data;
using MyPocket.Services.Gamification;
using MyPocket.Shared.ViewModels.Gamification;

namespace MyPocket.Tests
{
    public class ShopServiceTests
    {
        private static readonly DateTime Now = new(2026, 11, 2, 9, 0, 0, DateTimeKind.Utc);

        private static (ShopService shop, GamificationService gamification) Build(MyPocketDBContext context)
        {
            var clock = new FixedTimeProvider(Now);
            var gamification = new GamificationService(context, clock);
            return (new ShopService(context, gamification, clock), gamification);
        }

        private static void GiveCoins(MyPocketDBContext context, Guid userId, int amount)
        {
            context.CoinLedger.Add(new Core.Models.CoinLedgerEntry
            {
                EntryId = Guid.NewGuid(),
                UserId = userId,
                Amount = amount,
                Reason = "Test",
                CreatedAt = Now
            });
            context.SaveChanges();
        }

        [Fact]
        public async Task Purchase_DeductsCoinsAndGrantsItem()
        {
            using var context = TestHelper.NewContext();
            var user = TestHelper.SeedUser(context);
            GiveCoins(context, user.UserId, 100);
            var (shop, gamification) = Build(context);

            var result = await shop.PurchaseAsync(user.UserId, "tree");

            Assert.Equal(PurchaseResult.Success, result);
            Assert.Equal(100 - ShopCatalog.Find("tree")!.Price, await gamification.GetBalanceAsync(user.UserId));
            Assert.Contains((await shop.GetShopAsync(user.UserId)), e => e.Item.Code == "tree" && e.Owned);
        }

        [Fact]
        public async Task Purchase_FailsWhenBalanceTooLow()
        {
            using var context = TestHelper.NewContext();
            var user = TestHelper.SeedUser(context);
            GiveCoins(context, user.UserId, 10);
            var (shop, gamification) = Build(context);

            var result = await shop.PurchaseAsync(user.UserId, "dragon");

            Assert.Equal(PurchaseResult.InsufficientCoins, result);
            Assert.Equal(10, await gamification.GetBalanceAsync(user.UserId));
            Assert.Empty(context.UserItems);
        }

        [Fact]
        public async Task Purchase_SameItemTwice_IsRejected()
        {
            using var context = TestHelper.NewContext();
            var user = TestHelper.SeedUser(context);
            GiveCoins(context, user.UserId, 1000);
            var (shop, _) = Build(context);

            await shop.PurchaseAsync(user.UserId, "cat");
            var second = await shop.PurchaseAsync(user.UserId, "cat");

            Assert.Equal(PurchaseResult.AlreadyOwned, second);
            Assert.Single(context.UserItems);
        }

        [Fact]
        public async Task Purchase_UnknownItem_IsRejected()
        {
            using var context = TestHelper.NewContext();
            var user = TestHelper.SeedUser(context);
            GiveCoins(context, user.UserId, 1000);
            var (shop, _) = Build(context);

            Assert.Equal(PurchaseResult.UnknownItem, await shop.PurchaseAsync(user.UserId, "spaceship"));
        }

        [Fact]
        public async Task Purchase_Outfit_IsWornAndReplacesPreviousOutfit()
        {
            using var context = TestHelper.NewContext();
            var user = TestHelper.SeedUser(context);
            GiveCoins(context, user.UserId, 1000);
            var (shop, _) = Build(context);

            await shop.PurchaseAsync(user.UserId, "cap");
            await shop.PurchaseAsync(user.UserId, "tophat");

            var entries = await shop.GetShopAsync(user.UserId);
            Assert.True(entries.Single(e => e.Item.Code == "tophat").Equipped);
            Assert.False(entries.Single(e => e.Item.Code == "cap").Equipped);
        }

        [Fact]
        public async Task ToggleOutfit_TogglesOnlyOwnedOutfits()
        {
            using var context = TestHelper.NewContext();
            var user = TestHelper.SeedUser(context);
            GiveCoins(context, user.UserId, 1000);
            var (shop, _) = Build(context);

            Assert.False(await shop.ToggleOutfitAsync(user.UserId, "crown")); // not owned
            await shop.PurchaseAsync(user.UserId, "cap");
            Assert.False(await shop.ToggleOutfitAsync(user.UserId, "tree"));  // not an outfit

            Assert.True(await shop.ToggleOutfitAsync(user.UserId, "cap"));    // take it off
            Assert.False((await shop.GetShopAsync(user.UserId)).Single(e => e.Item.Code == "cap").Equipped);
        }
    }
}
