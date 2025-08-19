using Microsoft.EntityFrameworkCore;
using MyPocket.Core.Models;

namespace MyPocket.DataAccess.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(MyPocketDBContext context)
        {
            await context.Database.EnsureCreatedAsync();

            bool hasUser = await context.Users.AnyAsync(u => u.Email == "freemember@example.com");
            if (!hasUser)
            {
                var normalUser = new User
                {
                    UserId = Guid.NewGuid(),
                    Email = "freemember@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("12345678"),
                    Nickname = "測試免費用戶",
                    Role = "FreeMember",
                    CreationDate = DateTime.UtcNow,
                    LastLoginDate = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };
                await context.Users.AddAsync(normalUser);
                await context.SaveChangesAsync();
            }
           
            bool hasAdmin = await context.Users.AnyAsync(u => u.Email == "admin@example.com");
            if (!hasAdmin)
            {
                var admin = new User
                {
                    UserId = Guid.NewGuid(),
                    Email = "admin@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("12345678"),
                    Nickname = "管理員",
                    Role = "Admin",
                    CreationDate = DateTime.UtcNow,
                    LastLoginDate = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };
                await context.Users.AddAsync(admin);
                await context.SaveChangesAsync();
            }

            var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@example.com");

            var defaultCategories = new List<Category>
            {
                new Category { CategoryId = Guid.NewGuid(), UserId = adminUser.UserId, CategoryName = "餐飲", CategoryType = "支出", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsDeleted = false },
                new Category { CategoryId = Guid.NewGuid(), UserId = adminUser.UserId, CategoryName = "交通", CategoryType = "支出", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsDeleted = false },
                new Category { CategoryId = Guid.NewGuid(), UserId = adminUser.UserId, CategoryName = "購物", CategoryType = "支出", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsDeleted = false },
                new Category { CategoryId = Guid.NewGuid(), UserId = adminUser.UserId, CategoryName = "娛樂", CategoryType = "支出", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsDeleted = false },
                new Category { CategoryId = Guid.NewGuid(), UserId = adminUser.UserId, CategoryName = "醫療", CategoryType = "支出", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsDeleted = false },
                new Category { CategoryId = Guid.NewGuid(), UserId = adminUser.UserId, CategoryName = "教育", CategoryType = "支出", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsDeleted = false },
                new Category { CategoryId = Guid.NewGuid(), UserId = adminUser.UserId, CategoryName = "房租", CategoryType = "支出", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsDeleted = false },
                new Category { CategoryId = Guid.NewGuid(), UserId = adminUser.UserId, CategoryName = "水電瓦斯", CategoryType = "支出", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsDeleted = false },
                new Category { CategoryId = Guid.NewGuid(), UserId = adminUser.UserId, CategoryName = "通訊網路", CategoryType = "支出", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsDeleted = false },
                new Category { CategoryId = Guid.NewGuid(), UserId = adminUser.UserId, CategoryName = "保險", CategoryType = "支出", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsDeleted = false },
                new Category { CategoryId = Guid.NewGuid(), UserId = adminUser.UserId, CategoryName = "其他支出", CategoryType = "支出", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsDeleted = false },
                new Category { CategoryId = Guid.NewGuid(), UserId = adminUser.UserId, CategoryName = "薪資", CategoryType = "收入", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsDeleted = false },
                new Category { CategoryId = Guid.NewGuid(), UserId = adminUser.UserId, CategoryName = "獎金", CategoryType = "收入", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsDeleted = false },
                new Category { CategoryId = Guid.NewGuid(), UserId = adminUser.UserId, CategoryName = "投資收入", CategoryType = "收入", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsDeleted = false },
                new Category { CategoryId = Guid.NewGuid(), UserId = adminUser.UserId, CategoryName = "利息收入", CategoryType = "收入", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsDeleted = false },
                new Category { CategoryId = Guid.NewGuid(), UserId = adminUser.UserId, CategoryName = "其他收入", CategoryType = "收入", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsDeleted = false }
            };

            foreach (var category in defaultCategories)
            {
                bool exists = await context.Categories.AnyAsync(c => c.UserId == adminUser.UserId && c.CategoryName == category.CategoryName && c.CategoryType == category.CategoryType);
                if (!exists)
                {
                    await context.Categories.AddAsync(category);
                }
            }
            await context.SaveChangesAsync();
        }
    }
}