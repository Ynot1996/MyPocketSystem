using MyPocket.Core.Models;
using MyPocket.Services;
using MyPocket.Shared.ViewModels.Transactions;

namespace MyPocket.Tests
{
    public class SavingGoalServiceTests
    {
        [Fact]
        public async Task GetOrCreateMonthlyGoal_CreatesNew_WhenMissing()
        {
            using var context = TestHelper.NewContext();
            var user = TestHelper.SeedUser(context);
            var categoryService = new CategoryService(context);
            var txService = new TransactionService(context, categoryService);
            var service = new SavingGoalService(context, txService);

            var goal = await service.GetOrCreateMonthlyGoalAsync(user.UserId, 2026, 6);

            Assert.NotNull(goal);
            Assert.Equal(user.UserId, goal.UserId);
            Assert.Contains("2026", goal.GoalName);
            Assert.Contains("06", goal.GoalName);
            Assert.Single(context.SavingGoals);
        }

        [Fact]
        public async Task GetOrCreateMonthlyGoal_RefreshesCurrentAmount_OnExistingGoal()
        {
            using var context = TestHelper.NewContext();
            var admin = TestHelper.SeedAdmin(context);
            var income = TestHelper.SeedCategory(context, admin.UserId, "薪資", "收入");
            var user = TestHelper.SeedUser(context);
            var categoryService = new CategoryService(context);
            var txService = new TransactionService(context, categoryService);
            var service = new SavingGoalService(context, txService);

            await service.GetOrCreateMonthlyGoalAsync(user.UserId, 2026, 6);
            Assert.Single(context.SavingGoals);

            await txService.CreateTransactionAsync(user.UserId, new TransactionCreateModel
            {
                CategoryId = income.CategoryId,
                Amount = 800m,
                TransactionDate = new DateTime(2026, 6, 15, 0, 0, 0, DateTimeKind.Utc)
            });

            var refreshed = await service.GetOrCreateMonthlyGoalAsync(user.UserId, 2026, 6);

            Assert.Single(context.SavingGoals);
            Assert.Equal(800m, refreshed.CurrentAmount);
        }

        [Fact]
        public async Task CreateOrUpdateGoal_AddsThenUpdates()
        {
            using var context = TestHelper.NewContext();
            var user = TestHelper.SeedUser(context);
            var categoryService = new CategoryService(context);
            var txService = new TransactionService(context, categoryService);
            var service = new SavingGoalService(context, txService);

            var goal = new SavingGoal
            {
                GoalId = Guid.NewGuid(),
                UserId = user.UserId,
                GoalName = "Buy a bike",
                TargetAmount = 10000m,
                CurrentAmount = 0m,
                TargetDate = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc)
            };

            await service.CreateOrUpdateGoalAsync(goal);
            Assert.Single(context.SavingGoals);

            goal.TargetAmount = 12000m;
            await service.CreateOrUpdateGoalAsync(goal);

            Assert.Single(context.SavingGoals);
            Assert.Equal(12000m, context.SavingGoals.Single().TargetAmount);
        }

        [Fact]
        public async Task DeleteGoal_SoftDeletes_AndOnlyForOwner()
        {
            using var context = TestHelper.NewContext();
            var alice = TestHelper.SeedUser(context, "alice@example.com");
            var bob = TestHelper.SeedUser(context, "bob@example.com");
            var categoryService = new CategoryService(context);
            var txService = new TransactionService(context, categoryService);
            var service = new SavingGoalService(context, txService);

            var goal = await service.GetOrCreateMonthlyGoalAsync(alice.UserId, 2026, 6);

            await service.DeleteGoalAsync(bob.UserId, goal.GoalId);
            Assert.False(context.SavingGoals.Single().IsDeleted);

            await service.DeleteGoalAsync(alice.UserId, goal.GoalId);
            Assert.True(context.SavingGoals.Single().IsDeleted);
        }
    }
}
