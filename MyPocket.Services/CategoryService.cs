using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyPocket.Core.Models;
using MyPocket.DataAccess.Data;
using MyPocket.Services.Interfaces;
using MyPocket.Shared.ViewModels.Categories;

namespace MyPocket.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly MyPocketDBContext _context;
        private readonly ILogger<CategoryService> _logger;
        private static readonly Guid AdminUserId = Guid.Parse("ef908f96-9557-477d-97f1-4b1d766150bc");

        public CategoryService(MyPocketDBContext context, ILogger<CategoryService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<UserCategoryViewModel> GetUserCategoriesAsync(Guid userId)
        {
            try
            {
                _logger.LogInformation("開始獲取用戶 {UserId} 的分類資料", userId);

                var categories = await _context.Categories
                    .AsNoTracking()
                    .Where(c => !c.IsDeleted && (c.UserId == AdminUserId || c.UserId == userId))
                    .ToListAsync();

                var viewModel = new UserCategoryViewModel
                {
                    DefaultIncomeCategories = categories
                        .Where(c => c.UserId == AdminUserId && c.CategoryType == "收入")
                        .ToList(),
                    DefaultExpenseCategories = categories
                        .Where(c => c.UserId == AdminUserId && c.CategoryType == "支出")
                        .ToList(),
                    UserIncomeCategories = categories
                        .Where(c => c.UserId == userId && c.CategoryType == "收入")
                        .ToList(),
                    UserExpenseCategories = categories
                        .Where(c => c.UserId == userId && c.CategoryType == "支出")
                        .ToList()
                };

                return viewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取用戶 {UserId} 的分類資料時發生錯誤", userId);
                // 發生錯誤時返回空的 ViewModel，避免前端出錯
                return new UserCategoryViewModel
                {
                    DefaultIncomeCategories = new List<Category>(),
                    DefaultExpenseCategories = new List<Category>(),
                    UserIncomeCategories = new List<Category>(),
                    UserExpenseCategories = new List<Category>()
                };
            }
        }

        public async Task CreateCategoryAsync(Guid userId, string name, string type)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    _logger.LogWarning("Category name cannot be empty");
                    return;
                }

                if (type != "收入" && type != "支出")
                {
                    _logger.LogWarning("Invalid category type: {Type}", type);
                    return;
                }

                var category = new Category
                {
                    CategoryId = Guid.NewGuid(),
                    UserId = userId,
                    CategoryName = name.Trim(),
                    CategoryType = type,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category for user {UserId}", userId);
                throw;
            }
        }

        public async Task DeleteCategoryAsync(Guid userId, Guid categoryId)
        {
            try
            {
                var category = await _context.Categories
                    .FirstOrDefaultAsync(c => c.CategoryId == categoryId && c.UserId == userId);

                if (category == null)
                {
                    _logger.LogWarning("找不到要刪除的分類 {CategoryId}", categoryId);
                    return;
                }

                category.IsDeleted = true;
                category.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                _logger.LogInformation("成功刪除分類 {CategoryId}", categoryId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除分類 {CategoryId} 時發生錯誤", categoryId);
                throw;
            }
        }
    }
}
