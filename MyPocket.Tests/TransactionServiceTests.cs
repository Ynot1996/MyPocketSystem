using MyPocket.Services;
using MyPocket.Shared.ViewModels.Transactions;

namespace MyPocket.Tests
{
    public class TransactionServiceTests
    {
        [Fact]
        public async Task Create_AddsExpense_UsingDefaultCategory()
        {
            // This is the user-reported bug: new user tries to add an expense
            // against a default (admin-owned) category and it must succeed.
            using var context = TestHelper.NewContext();
            var admin = TestHelper.SeedAdmin(context);
            var category = TestHelper.SeedCategory(context, admin.UserId, "餐飲", "支出");
            var user = TestHelper.SeedUser(context);

            var categoryService = new CategoryService(context);
            var service = new TransactionService(context, categoryService);

            var (success, message, transaction) = await service.CreateTransactionAsync(
                user.UserId,
                new TransactionCreateModel
                {
                    CategoryId = category.CategoryId,
                    Amount = 120m,
                    TransactionDate = DateTime.UtcNow,
                    Description = "lunch"
                });

            Assert.True(success, $"Expected success but got: {message}");
            Assert.NotNull(transaction);
            Assert.Equal(120m, transaction!.Amount);
            Assert.Equal("支出", transaction.TransactionType);
            Assert.Equal(user.UserId, transaction.UserId);
            Assert.Single(context.Transactions);
        }

        [Fact]
        public async Task Create_PreservesDecimalAmount()
        {
            // Regression: a 1.8 GBP entry used to render as 2 because of N0
            // formatting. Service must store the exact decimal value.
            using var context = TestHelper.NewContext();
            var admin = TestHelper.SeedAdmin(context);
            var category = TestHelper.SeedCategory(context, admin.UserId, "餐飲", "支出");
            var user = TestHelper.SeedUser(context);
            var categoryService = new CategoryService(context);
            var service = new TransactionService(context, categoryService);

            var (success, _, transaction) = await service.CreateTransactionAsync(
                user.UserId,
                new TransactionCreateModel
                {
                    CategoryId = category.CategoryId,
                    Amount = 1.8m,
                    Currency = "GBP",
                    TransactionDate = DateTime.UtcNow
                });

            Assert.True(success);
            Assert.Equal(1.8m, transaction!.Amount);
        }

        [Fact]
        public async Task Create_PersistsCurrencyCode()
        {
            using var context = TestHelper.NewContext();
            var admin = TestHelper.SeedAdmin(context);
            var category = TestHelper.SeedCategory(context, admin.UserId, "餐飲", "支出");
            var user = TestHelper.SeedUser(context);
            var categoryService = new CategoryService(context);
            var service = new TransactionService(context, categoryService);

            var (_, _, tx) = await service.CreateTransactionAsync(user.UserId, new TransactionCreateModel
            {
                CategoryId = category.CategoryId,
                Amount = 100m,
                Currency = "jpy", // intentionally lowercase
                TransactionDate = DateTime.UtcNow
            });

            Assert.Equal("JPY", tx!.Currency);
        }

        [Fact]
        public async Task Create_AddsIncome_UsingOwnCategory()
        {
            using var context = TestHelper.NewContext();
            var user = TestHelper.SeedUser(context);
            var category = TestHelper.SeedCategory(context, user.UserId, "Side Income", "收入");

            var categoryService = new CategoryService(context);
            var service = new TransactionService(context, categoryService);

            var (success, _, transaction) = await service.CreateTransactionAsync(
                user.UserId,
                new TransactionCreateModel
                {
                    CategoryId = category.CategoryId,
                    Amount = 5000m,
                    TransactionDate = DateTime.UtcNow
                });

            Assert.True(success);
            Assert.Equal("收入", transaction!.TransactionType);
        }

        [Fact]
        public async Task Create_RejectsCategoryFromAnotherUser()
        {
            using var context = TestHelper.NewContext();
            var alice = TestHelper.SeedUser(context, "alice@example.com");
            var bob = TestHelper.SeedUser(context, "bob@example.com");
            var bobsCategory = TestHelper.SeedCategory(context, bob.UserId, "Bob Cat", "支出");

            var categoryService = new CategoryService(context);
            var service = new TransactionService(context, categoryService);

            var (success, _, _) = await service.CreateTransactionAsync(
                alice.UserId,
                new TransactionCreateModel
                {
                    CategoryId = bobsCategory.CategoryId,
                    Amount = 10m,
                    TransactionDate = DateTime.UtcNow
                });

            Assert.False(success);
            Assert.Empty(context.Transactions);
        }

        [Fact]
        public async Task Create_ThrowsForEmptyUserId()
        {
            using var context = TestHelper.NewContext();
            var categoryService = new CategoryService(context);
            var service = new TransactionService(context, categoryService);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.CreateTransactionAsync(Guid.Empty, new TransactionCreateModel
                {
                    CategoryId = Guid.NewGuid(),
                    Amount = 1m,
                    TransactionDate = DateTime.UtcNow
                }));
        }

        [Fact]
        public async Task GetUserTransactions_ExcludesDeleted_AndOrdersByDateDescending()
        {
            using var context = TestHelper.NewContext();
            var admin = TestHelper.SeedAdmin(context);
            var category = TestHelper.SeedCategory(context, admin.UserId, "餐飲", "支出");
            var user = TestHelper.SeedUser(context);
            var categoryService = new CategoryService(context);
            var service = new TransactionService(context, categoryService);

            await service.CreateTransactionAsync(user.UserId, new TransactionCreateModel
            {
                CategoryId = category.CategoryId,
                Amount = 100m,
                TransactionDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });
            var (_, _, mid) = await service.CreateTransactionAsync(user.UserId, new TransactionCreateModel
            {
                CategoryId = category.CategoryId,
                Amount = 200m,
                TransactionDate = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc)
            });
            await service.CreateTransactionAsync(user.UserId, new TransactionCreateModel
            {
                CategoryId = category.CategoryId,
                Amount = 300m,
                TransactionDate = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc)
            });

            await service.DeleteTransactionAsync(user.UserId, mid!.TransactionId);

            var list = await service.GetUserTransactionsAsync(user.UserId);

            Assert.Equal(2, list.Count);
            Assert.Equal(300m, list[0].Amount);
            Assert.Equal(100m, list[1].Amount);
        }

        [Fact]
        public async Task GetUserTransactionsByMonth_FiltersToTargetMonth()
        {
            using var context = TestHelper.NewContext();
            var admin = TestHelper.SeedAdmin(context);
            var category = TestHelper.SeedCategory(context, admin.UserId, "餐飲", "支出");
            var user = TestHelper.SeedUser(context);
            var categoryService = new CategoryService(context);
            var service = new TransactionService(context, categoryService);

            await service.CreateTransactionAsync(user.UserId, new TransactionCreateModel
            {
                CategoryId = category.CategoryId,
                Amount = 1m,
                TransactionDate = new DateTime(2026, 2, 15, 0, 0, 0, DateTimeKind.Utc)
            });
            await service.CreateTransactionAsync(user.UserId, new TransactionCreateModel
            {
                CategoryId = category.CategoryId,
                Amount = 2m,
                TransactionDate = new DateTime(2026, 6, 20, 0, 0, 0, DateTimeKind.Utc)
            });

            var june = await service.GetUserTransactionsByMonthAsync(user.UserId, 2026, 6);

            Assert.Single(june);
            Assert.Equal(2m, june[0].Amount);
        }

        [Fact]
        public async Task CalculateCurrentSaving_ReturnsIncomeMinusExpense_InRange()
        {
            using var context = TestHelper.NewContext();
            var admin = TestHelper.SeedAdmin(context);
            var income = TestHelper.SeedCategory(context, admin.UserId, "薪資", "收入");
            var expense = TestHelper.SeedCategory(context, admin.UserId, "餐飲", "支出");
            var user = TestHelper.SeedUser(context);
            var categoryService = new CategoryService(context);
            var service = new TransactionService(context, categoryService);

            await service.CreateTransactionAsync(user.UserId, new TransactionCreateModel
            {
                CategoryId = income.CategoryId,
                Amount = 1000m,
                TransactionDate = new DateTime(2026, 6, 10, 0, 0, 0, DateTimeKind.Utc)
            });
            await service.CreateTransactionAsync(user.UserId, new TransactionCreateModel
            {
                CategoryId = expense.CategoryId,
                Amount = 300m,
                TransactionDate = new DateTime(2026, 6, 15, 0, 0, 0, DateTimeKind.Utc)
            });
            // Outside the range — must be ignored.
            await service.CreateTransactionAsync(user.UserId, new TransactionCreateModel
            {
                CategoryId = income.CategoryId,
                Amount = 9999m,
                TransactionDate = new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc)
            });

            var saving = await service.CalculateCurrentSavingAsync(
                user.UserId,
                new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 6, 30, 0, 0, 0, DateTimeKind.Utc));

            Assert.Equal(700m, saving);
        }

        [Fact]
        public async Task Update_ChangesFields_AndOnlyForOwner()
        {
            using var context = TestHelper.NewContext();
            var admin = TestHelper.SeedAdmin(context);
            var dining = TestHelper.SeedCategory(context, admin.UserId, "餐飲", "支出");
            var transport = TestHelper.SeedCategory(context, admin.UserId, "交通", "支出");
            var alice = TestHelper.SeedUser(context, "alice@example.com");
            var bob = TestHelper.SeedUser(context, "bob@example.com");

            var categoryService = new CategoryService(context);
            var service = new TransactionService(context, categoryService);

            var (_, _, tx) = await service.CreateTransactionAsync(alice.UserId, new TransactionCreateModel
            {
                CategoryId = dining.CategoryId,
                Amount = 1.8m,
                Currency = "GBP",
                TransactionDate = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc)
            });

            // Bob cannot edit Alice's transaction.
            var (bobOk, _) = await service.UpdateTransactionAsync(bob.UserId, tx!.TransactionId, new TransactionCreateModel
            {
                CategoryId = transport.CategoryId,
                Amount = 99m,
                Currency = "USD",
                TransactionDate = DateTime.UtcNow
            });
            Assert.False(bobOk);

            var (aliceOk, message) = await service.UpdateTransactionAsync(alice.UserId, tx.TransactionId, new TransactionCreateModel
            {
                CategoryId = transport.CategoryId,
                Amount = 3.67m,
                Currency = "USD",
                TransactionDate = new DateTime(2026, 6, 10, 0, 0, 0, DateTimeKind.Utc),
                Description = "edited"
            });

            Assert.True(aliceOk, message);
            var updated = context.Transactions.Single();
            Assert.Equal(transport.CategoryId, updated.CategoryId);
            Assert.Equal(3.67m, updated.Amount);
            Assert.Equal("USD", updated.Currency);
            Assert.Equal("edited", updated.Description);
        }

        [Fact]
        public async Task Delete_SoftDeletes_AndOnlyForOwner()
        {
            using var context = TestHelper.NewContext();
            var admin = TestHelper.SeedAdmin(context);
            var category = TestHelper.SeedCategory(context, admin.UserId, "餐飲", "支出");
            var alice = TestHelper.SeedUser(context, "alice@example.com");
            var bob = TestHelper.SeedUser(context, "bob@example.com");
            var categoryService = new CategoryService(context);
            var service = new TransactionService(context, categoryService);

            var (_, _, aliceTx) = await service.CreateTransactionAsync(alice.UserId, new TransactionCreateModel
            {
                CategoryId = category.CategoryId,
                Amount = 50m,
                TransactionDate = DateTime.UtcNow
            });

            // Bob attempts to delete Alice's transaction — must fail.
            var (bobOk, _) = await service.DeleteTransactionAsync(bob.UserId, aliceTx!.TransactionId);
            Assert.False(bobOk);
            Assert.False(context.Transactions.Single().IsDeleted);

            var (aliceOk, _) = await service.DeleteTransactionAsync(alice.UserId, aliceTx.TransactionId);
            Assert.True(aliceOk);
            Assert.True(context.Transactions.Single().IsDeleted);
        }
    }
}
