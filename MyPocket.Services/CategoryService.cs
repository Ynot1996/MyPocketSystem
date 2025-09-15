using Microsoft.EntityFrameworkCore;
using MyPocket.Core.Models;
using MyPocket.DataAccess.Data;
using MyPocket.Services.Interfaces;
using MyPocket.Shared.ViewModels.Categories;

namespace MyPocket.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly MyPocketDBContext _context;
        private static readonly Guid AdminUserId = Guid.Parse("ef908f96-9557-477d-97f1-4b1d766150bc");
        public CategoryService(MyPocketDBContext context)
        {
            _context = context;
        }

        public async Task<UserCategoryViewModel> GetUserCategoriesAsync(Guid userId)
        {
            // 記錄傳入的 userId
            System.Diagnostics.Debug.WriteLine($"CategoryService called with userId: {userId}");
            System.Diagnostics.Debug.WriteLine($"Hardcoded AdminUserId: {AdminUserId}");

            var categories = await _context.Categories.Where(c => !c.IsDeleted).ToListAsync();

            // 記錄篩選後的類別數量
            System.Diagnostics.Debug.WriteLine($"Total categories from DB: {categories.Count}");
            System.Diagnostics.Debug.WriteLine($"DefaultIncomeCategories count: {categories.Count(c => c.UserId == AdminUserId && c.CategoryType == "收入")}");
            System.Diagnostics.Debug.WriteLine($"DefaultExpenseCategories count: {categories.Count(c => c.UserId == AdminUserId && c.CategoryType == "支出")}");
            System.Diagnostics.Debug.WriteLine($"UserIncomeCategories count: {categories.Count(c => c.UserId == userId && c.CategoryType == "收入")}");
            System.Diagnostics.Debug.WriteLine($"UserExpenseCategories count: {categories.Count(c => c.UserId == userId && c.CategoryType == "支出")}");

            return new UserCategoryViewModel
            {
                DefaultIncomeCategories = categories.Where(c => c.UserId == AdminUserId && c.CategoryType == "收入").ToList(),
                DefaultExpenseCategories = categories.Where(c => c.UserId == AdminUserId && c.CategoryType == "支出").ToList(),
                UserIncomeCategories = categories.Where(c => c.UserId == userId && c.CategoryType == "收入").ToList(),
                UserExpenseCategories = categories.Where(c => c.UserId == userId && c.CategoryType == "支出").ToList()
            };
        }

        public async Task CreateCategoryAsync(Guid userId, string name, string type)
        {
            if (!string.IsNullOrWhiteSpace(name) && (type == "收入" || type == "支出"))
            {
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
        }

        public async Task DeleteCategoryAsync(Guid userId, Guid categoryId)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryId == categoryId && c.UserId == userId);
            if (category != null)
            {
                category.IsDeleted = true;
                category.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}
