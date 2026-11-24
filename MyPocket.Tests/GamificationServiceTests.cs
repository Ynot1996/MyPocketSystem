using MyPocket.Core.Models;
using MyPocket.Services.Gamification;

namespace MyPocket.Tests
{
    public class GamificationServiceTests
    {
        private static readonly DateTime Day1 = new(2026, 11, 2, 9, 0, 0, DateTimeKind.Utc);

        [Fact]
        public async Task Award_FirstTransactionOfDay_GrantsBaseCoinsPlusStreakBonus()
        {
            using var context = TestHelper.NewContext();
            var user = TestHelper.SeedUser(context);
            var service = new GamificationService(context, new FixedTimeProvider(Day1));

            var award = await service.AwardForTransactionAsync(user.UserId, autoCaptured: false);

            // 10 for the transaction + 5 * 1-day streak.
            Assert.Equal(15, award.Coins);
            Assert.Equal(1, award.StreakDays);
            Assert.Contains(GamificationService.ReasonStreak, award.Reasons);
            Assert.Equal(15, await service.GetBalanceAsync(user.UserId));
        }

        [Fact]
        public async Task Award_SecondTransactionSameDay_DoesNotRepeatStreakBonus()
        {
            using var context = TestHelper.NewContext();
            var user = TestHelper.SeedUser(context);
            var service = new GamificationService(context, new FixedTimeProvider(Day1));

            await service.AwardForTransactionAsync(user.UserId, autoCaptured: false);
            var second = await service.AwardForTransactionAsync(user.UserId, autoCaptured: false);

            Assert.Equal(GamificationService.CoinsPerTransaction, second.Coins);
            Assert.DoesNotContain(GamificationService.ReasonStreak, second.Reasons);
        }

        [Fact]
        public async Task Award_AutoCapturedEntry_AddsBonus()
        {
            using var context = TestHelper.NewContext();
            var user = TestHelper.SeedUser(context);
            var service = new GamificationService(context, new FixedTimeProvider(Day1));

            await service.AwardForTransactionAsync(user.UserId, autoCaptured: false); // use up the streak bonus
            var award = await service.AwardForTransactionAsync(user.UserId, autoCaptured: true);

            Assert.Equal(GamificationService.CoinsPerTransaction + GamificationService.AutoCaptureBonus, award.Coins);
            Assert.Contains(GamificationService.ReasonAutoCapture, award.Reasons);
        }

        [Fact]
        public async Task Award_StopsPayingAfterDailyCap()
        {
            using var context = TestHelper.NewContext();
            var user = TestHelper.SeedUser(context);
            var clock = new FixedTimeProvider(Day1);
            var service = new GamificationService(context, clock);

            for (var i = 0; i < GamificationService.DailyRewardedTransactions; i++)
            {
                await service.AwardForTransactionAsync(user.UserId, autoCaptured: false);
            }
            var balanceAtCap = await service.GetBalanceAsync(user.UserId);

            var overCap = await service.AwardForTransactionAsync(user.UserId, autoCaptured: true);

            Assert.True(overCap.DailyCapReached);
            Assert.Equal(0, overCap.Coins);
            Assert.Equal(balanceAtCap, await service.GetBalanceAsync(user.UserId));

            // The cap resets the next day.
            clock.Advance(TimeSpan.FromDays(1));
            var nextDay = await service.AwardForTransactionAsync(user.UserId, autoCaptured: false);
            Assert.False(nextDay.DailyCapReached);
            Assert.True(nextDay.Coins > 0);
        }

        [Fact]
        public async Task Streak_CountsConsecutiveDays_AndSurvivesUntilTheNextDayEnds()
        {
            using var context = TestHelper.NewContext();
            var user = TestHelper.SeedUser(context);
            var clock = new FixedTimeProvider(Day1);
            var service = new GamificationService(context, clock);

            for (var day = 0; day < 3; day++)
            {
                await service.AwardForTransactionAsync(user.UserId, autoCaptured: false);
                clock.Advance(TimeSpan.FromDays(1));
            }

            // Day 4, nothing logged yet: yesterday counts, so the streak is still 3.
            Assert.Equal(3, await service.GetStreakAsync(user.UserId));

            // Day 5 with nothing on day 4: the streak is broken.
            clock.Advance(TimeSpan.FromDays(1));
            Assert.Equal(0, await service.GetStreakAsync(user.UserId));
        }

        [Fact]
        public async Task StreakBonus_IsCappedAtSevenDays()
        {
            using var context = TestHelper.NewContext();
            var user = TestHelper.SeedUser(context);
            var clock = new FixedTimeProvider(Day1);
            var service = new GamificationService(context, clock);

            Shared.ViewModels.Gamification.CoinAward last = null!;
            for (var day = 0; day < 10; day++)
            {
                last = await service.AwardForTransactionAsync(user.UserId, autoCaptured: false);
                clock.Advance(TimeSpan.FromDays(1));
            }

            Assert.Equal(10, last.StreakDays);
            var expected = GamificationService.CoinsPerTransaction
                           + GamificationService.StreakBonusPerDay * GamificationService.StreakBonusMaxDays;
            Assert.Equal(expected, last.Coins);
        }

        [Fact]
        public async Task WelcomeBonus_IsGrantedOnlyOnce()
        {
            using var context = TestHelper.NewContext();
            var user = TestHelper.SeedUser(context);
            var service = new GamificationService(context, new FixedTimeProvider(Day1));

            await service.EnsureWelcomeBonusAsync(user.UserId);
            await service.EnsureWelcomeBonusAsync(user.UserId);

            Assert.Equal(GamificationService.WelcomeBonus, await service.GetBalanceAsync(user.UserId));
        }

        [Fact]
        public async Task City_BuildsOneBuildingPerCategory_WithLevelFromFrequency()
        {
            using var context = TestHelper.NewContext();
            var admin = TestHelper.SeedAdmin(context);
            var dining = TestHelper.SeedCategory(context, admin.UserId, "餐飲", "支出");
            var user = TestHelper.SeedUser(context);
            var custom = TestHelper.SeedCategory(context, user.UserId, "Gym", "支出");

            // Three small coffees and one large one: level depends on count, not amount.
            for (var i = 0; i < 3; i++) TestHelper.SeedTransaction(context, user.UserId, dining, 3m, Day1.AddMinutes(i));
            TestHelper.SeedTransaction(context, user.UserId, custom, 500m, Day1.AddHours(1));

            var service = new GamificationService(context, new FixedTimeProvider(Day1));
            var city = await service.GetCityAsync(user.UserId);

            Assert.Equal(2, city.Buildings.Count);

            var cafe = city.Buildings.Single(b => b.CategoryKey == "餐飲");
            Assert.Equal("Cafe", cafe.BuildingType);
            Assert.Equal(2, cafe.Level);        // thresholds 1, 3, 7...
            Assert.Equal(7, cafe.NextLevelAt);

            var house = city.Buildings.Single(b => b.CategoryKey == "Gym");
            Assert.Equal("House", house.BuildingType);
            Assert.Equal(1, house.Level);

            // First-used category takes the most central slot.
            Assert.Equal(CityCatalog.SlotOrder[0], cafe.Slot);
        }

        [Fact]
        public async Task City_IgnoresDeletedTransactions()
        {
            using var context = TestHelper.NewContext();
            var admin = TestHelper.SeedAdmin(context);
            var dining = TestHelper.SeedCategory(context, admin.UserId, "餐飲", "支出");
            var user = TestHelper.SeedUser(context);
            TestHelper.SeedTransaction(context, user.UserId, dining, 3m, Day1, isDeleted: true);

            var service = new GamificationService(context, new FixedTimeProvider(Day1));
            var city = await service.GetCityAsync(user.UserId);

            Assert.Empty(city.Buildings);
        }

        [Fact]
        public async Task City_IncludesOwnedDecorationsCharactersAndWornOutfit()
        {
            using var context = TestHelper.NewContext();
            var user = TestHelper.SeedUser(context);
            context.UserItems.AddRange(
                new UserItem { UserItemId = Guid.NewGuid(), UserId = user.UserId, ItemCode = "tree", PurchasedAt = Day1 },
                new UserItem { UserItemId = Guid.NewGuid(), UserId = user.UserId, ItemCode = "cat", PurchasedAt = Day1 },
                new UserItem { UserItemId = Guid.NewGuid(), UserId = user.UserId, ItemCode = "crown", PurchasedAt = Day1, IsEquipped = true });
            context.SaveChanges();

            var service = new GamificationService(context, new FixedTimeProvider(Day1));
            var city = await service.GetCityAsync(user.UserId);

            Assert.Single(city.Decorations);
            Assert.Single(city.Characters);
            Assert.Equal("👑", city.EquippedOutfitEmoji);
        }
    }
}
