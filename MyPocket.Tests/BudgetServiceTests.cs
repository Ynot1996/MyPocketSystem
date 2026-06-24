using MyPocket.Services;
using MyPocket.Shared.ViewModels.Budgets;
using MyPocket.Shared.ViewModels.Transactions;

namespace MyPocket.Tests
{
    public class BudgetServiceTests
    {
        [Fact]
        public async Task CreateBudget_StoresCurrentMonth()
        {
            using var context = TestHelper.NewContext();
            var admin = TestHelper.SeedAdmin(context);
            var category = TestHelper.SeedCategory(context, admin.UserId, "餐飲", "支出");
            var user = TestHelper.SeedUser(context);
            var service = new BudgetService(context);

            var ok = await service.CreateBudgetAsync(user.UserId, new BudgetViewModel
            {
                CategoryId = category.CategoryId,
                Amount = 5000m
            });

            Assert.True(ok);
            var saved = context.Budgets.Single();
            Assert.Equal(user.UserId, saved.UserId);
            Assert.Equal(5000m, saved.Amount);
            Assert.Equal(DateTime.UtcNow.Year.ToString(), saved.BudgetYear);
            Assert.Equal(DateTime.UtcNow.Month.ToString("D2"), saved.BudgetMonth);
        }

        [Fact]
        public async Task GetUserBudgets_ReturnsSpentFromMatchingMonth()
        {
            using var context = TestHelper.NewContext();
            var admin = TestHelper.SeedAdmin(context);
            var category = TestHelper.SeedCategory(context, admin.UserId, "餐飲", "支出");
            var user = TestHelper.SeedUser(context);

            var budgetService = new BudgetService(context);
            var categoryService = new CategoryService(context);
            var txService = new TransactionService(context, categoryService);

            await budgetService.CreateBudgetAsync(user.UserId, new BudgetViewModel
            {
                CategoryId = category.CategoryId,
                Amount = 1000m
            });

            // One in current month, one outside — only the current month should count.
            await txService.CreateTransactionAsync(user.UserId, new TransactionCreateModel
            {
                CategoryId = category.CategoryId,
                Amount = 300m,
                TransactionDate = DateTime.UtcNow
            });
            await txService.CreateTransactionAsync(user.UserId, new TransactionCreateModel
            {
                CategoryId = category.CategoryId,
                Amount = 9999m,
                TransactionDate = DateTime.UtcNow.AddMonths(-3)
            });

            var list = await budgetService.GetUserBudgetsAsync(user.UserId, DateTime.UtcNow.Year, DateTime.UtcNow.Month);

            var item = Assert.Single(list);
            Assert.Equal(1000m, item.Amount);
            Assert.Equal(300m, item.Spent);
            Assert.Equal(700m, item.Remaining);
        }

        [Fact]
        public async Task UpdateBudget_OnlyAllowsOwner()
        {
            using var context = TestHelper.NewContext();
            var admin = TestHelper.SeedAdmin(context);
            var category = TestHelper.SeedCategory(context, admin.UserId, "餐飲", "支出");
            var alice = TestHelper.SeedUser(context, "alice@example.com");
            var bob = TestHelper.SeedUser(context, "bob@example.com");
            var service = new BudgetService(context);

            await service.CreateBudgetAsync(alice.UserId, new BudgetViewModel
            {
                CategoryId = category.CategoryId,
                Amount = 100m
            });
            var aliceBudget = context.Budgets.Single();

            var bobAttempt = await service.UpdateBudgetAsync(bob.UserId, new BudgetViewModel
            {
                BudgetId = aliceBudget.BudgetId,
                Amount = 999m
            });
            Assert.False(bobAttempt);
            Assert.Equal(100m, context.Budgets.Single().Amount);

            var aliceAttempt = await service.UpdateBudgetAsync(alice.UserId, new BudgetViewModel
            {
                BudgetId = aliceBudget.BudgetId,
                Amount = 250m
            });
            Assert.True(aliceAttempt);
            Assert.Equal(250m, context.Budgets.Single().Amount);
        }

        [Fact]
        public async Task DeleteBudget_SoftDeletes_AndOnlyForOwner()
        {
            using var context = TestHelper.NewContext();
            var admin = TestHelper.SeedAdmin(context);
            var category = TestHelper.SeedCategory(context, admin.UserId, "餐飲", "支出");
            var alice = TestHelper.SeedUser(context, "alice@example.com");
            var bob = TestHelper.SeedUser(context, "bob@example.com");
            var service = new BudgetService(context);

            await service.CreateBudgetAsync(alice.UserId, new BudgetViewModel
            {
                CategoryId = category.CategoryId,
                Amount = 100m
            });
            var budgetId = context.Budgets.Single().BudgetId;

            Assert.False(await service.DeleteBudgetAsync(bob.UserId, budgetId));
            Assert.False(context.Budgets.Single().IsDeleted);

            Assert.True(await service.DeleteBudgetAsync(alice.UserId, budgetId));
            Assert.True(context.Budgets.Single().IsDeleted);
        }

        [Fact]
        public async Task GetUserExpenseCategories_IncludesOwnAndAdminDefaults()
        {
            using var context = TestHelper.NewContext();
            var admin = TestHelper.SeedAdmin(context);
            TestHelper.SeedCategory(context, admin.UserId, "餐飲", "支出");
            TestHelper.SeedCategory(context, admin.UserId, "薪資", "收入"); // income, should be filtered out
            var user = TestHelper.SeedUser(context);
            TestHelper.SeedCategory(context, user.UserId, "自訂", "支出");
            var service = new BudgetService(context);

            var list = await service.GetUserExpenseCategoriesAsync(user.UserId);

            Assert.Equal(2, list.Count);
            Assert.All(list, c => Assert.Equal("支出", c.CategoryType));
        }
    }
}
