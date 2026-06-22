using Microsoft.EntityFrameworkCore;
using MyPocket.Core.Models;
using MyPocket.DataAccess.Data;
using MyPocket.Services.Interfaces;
using MyPocket.Shared.ViewModels.Categories;

namespace MyPocket.Services
{
    public class CategoryService : ICategoryService
    {
        private const string AdminEmail = "admin@example.com";
        private const string IncomeType = "收入";
        private const string ExpenseType = "支出";

        private readonly MyPocketDBContext _context;

        public CategoryService(MyPocketDBContext context)
        {
            _context = context;
        }

        public async Task<UserCategoryViewModel> GetUserCategoriesAsync(Guid userId)
        {
            // Resolve the admin user dynamically so defaults survive a fresh DB seed.
            var adminUserId = await _context.Users
                .Where(u => u.Email == AdminEmail)
                .Select(u => (Guid?)u.UserId)
                .FirstOrDefaultAsync();

            var categories = await _context.Categories
                .Where(c => !c.IsDeleted)
                .ToListAsync();

            return new UserCategoryViewModel
            {
                DefaultIncomeCategories = adminUserId is null
                    ? new List<Category>()
                    : categories.Where(c => c.UserId == adminUserId && c.CategoryType == IncomeType).ToList(),
                DefaultExpenseCategories = adminUserId is null
                    ? new List<Category>()
                    : categories.Where(c => c.UserId == adminUserId && c.CategoryType == ExpenseType).ToList(),
                UserIncomeCategories = categories.Where(c => c.UserId == userId && c.CategoryType == IncomeType).ToList(),
                UserExpenseCategories = categories.Where(c => c.UserId == userId && c.CategoryType == ExpenseType).ToList()
            };
        }

        public async Task CreateCategoryAsync(Guid userId, string name, string type)
        {
            if (!string.IsNullOrWhiteSpace(name) && (type == IncomeType || type == ExpenseType))
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
