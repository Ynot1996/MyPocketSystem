using MyPocket.Services;

namespace MyPocket.Tests
{
    public class CategoryServiceTests
    {
        [Fact]
        public async Task GetUserCategories_ReturnsAdminDefaults_AsDefaultCategories()
        {
            using var context = TestHelper.NewContext();
            var admin = TestHelper.SeedAdmin(context);
            TestHelper.SeedCategory(context, admin.UserId, "餐飲", "支出");
            TestHelper.SeedCategory(context, admin.UserId, "薪資", "收入");

            var user = TestHelper.SeedUser(context);
            var service = new CategoryService(context);

            var result = await service.GetUserCategoriesAsync(user.UserId);

            Assert.Single(result.DefaultExpenseCategories);
            Assert.Equal("餐飲", result.DefaultExpenseCategories[0].CategoryName);
            Assert.Single(result.DefaultIncomeCategories);
            Assert.Empty(result.UserExpenseCategories);
            Assert.Empty(result.UserIncomeCategories);
        }

        [Fact]
        public async Task GetUserCategories_ReturnsEmpty_WhenAdminMissing()
        {
            using var context = TestHelper.NewContext();
            var user = TestHelper.SeedUser(context);
            var service = new CategoryService(context);

            var result = await service.GetUserCategoriesAsync(user.UserId);

            Assert.Empty(result.DefaultExpenseCategories);
            Assert.Empty(result.DefaultIncomeCategories);
        }

        [Fact]
        public async Task CreateCategory_AddsUserOwnedCategory()
        {
            using var context = TestHelper.NewContext();
            var user = TestHelper.SeedUser(context);
            var service = new CategoryService(context);

            await service.CreateCategoryAsync(user.UserId, "自訂支出", "支出");

            var saved = context.Categories.Single();
            Assert.Equal(user.UserId, saved.UserId);
            Assert.Equal("自訂支出", saved.CategoryName);
            Assert.False(saved.IsDeleted);
        }

        [Fact]
        public async Task CreateCategory_IgnoresInvalidType()
        {
            using var context = TestHelper.NewContext();
            var user = TestHelper.SeedUser(context);
            var service = new CategoryService(context);

            await service.CreateCategoryAsync(user.UserId, "Bad", "Invalid");

            Assert.Empty(context.Categories);
        }

        [Fact]
        public async Task DeleteCategory_SoftDeletesOnlyOwnersCategory()
        {
            using var context = TestHelper.NewContext();
            var user = TestHelper.SeedUser(context);
            var other = TestHelper.SeedUser(context, "other@example.com");
            var mine = TestHelper.SeedCategory(context, user.UserId, "Mine", "支出");
            var theirs = TestHelper.SeedCategory(context, other.UserId, "Theirs", "支出");

            var service = new CategoryService(context);

            // Deleting another user's category must be a no-op.
            await service.DeleteCategoryAsync(user.UserId, theirs.CategoryId);
            Assert.False(context.Categories.Single(c => c.CategoryId == theirs.CategoryId).IsDeleted);

            await service.DeleteCategoryAsync(user.UserId, mine.CategoryId);
            Assert.True(context.Categories.Single(c => c.CategoryId == mine.CategoryId).IsDeleted);
        }
    }
}
