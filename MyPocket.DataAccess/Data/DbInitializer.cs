using Microsoft.EntityFrameworkCore;
using MyPocket.Core.Models;

namespace MyPocket.DataAccess.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(MyPocketDBContext context)
        {
            await context.Database.EnsureCreatedAsync();

            // Initialize subscription plans if they don't exist
            if (!await context.SubscriptionPlans.AnyAsync())
            {
                var subscriptionPlans = new List<SubscriptionPlan>
                {
                    new SubscriptionPlan
                    {
                        PlanId = Guid.NewGuid(),
                        PlanName = "免費基本會員",
                        Price = 0M,
                        DurationDays = 36500, // 100年，實質上就是永久
                        Description = "基本功能：\n- 基本收支記錄\n- 單一預算設定\n- 基礎收支報表"
                    },
                    new SubscriptionPlan
                    {
                        PlanId = Guid.NewGuid(),
                        PlanName = "進階會員（月付）",
                        Price = 89M,
                        DurationDays = 30,
                        Description = "所有進階功能，月付方案：\n- 多重預算管理\n- 詳細圖表分析\n- 目標儲蓄追蹤\n- 自訂分類\n- 資料匯出"
                    },
                    new SubscriptionPlan
                    {
                        PlanId = Guid.NewGuid(),
                        PlanName = "進階會員（季付）",
                        Price = 249M,
                        DurationDays = 90,
                        Description = "所有進階功能，季付方案：\n- 比月付方案省83元\n- 多重預算管理\n- 詳細圖表分析\n- 目標儲蓄追蹤\n- 自訂分類\n- 資料匯出"
                    },
                    new SubscriptionPlan
                    {
                        PlanId = Guid.NewGuid(),
                        PlanName = "進階會員（半年）",
                        Price = 449M,
                        DurationDays = 180,
                        Description = "所有進階功能，半年方案：\n- 比月付方案省85元/月\n- 多重預算管理\n- 詳細圖表分析\n- 目標儲蓄追蹤\n- 自訂分類\n- 資料匯出"
                    },
                    new SubscriptionPlan
                    {
                        PlanId = Guid.NewGuid(),
                        PlanName = "進階會員（年付）",
                        Price = 799M,
                        DurationDays = 365,
                        Description = "所有進階功能，年付方案：\n- 比月付方案省269元/月\n- 多重預算管理\n- 詳細圖表分析\n- 目標儲蓄追蹤\n- 自訂分類\n- 資料匯出\n- 額外贈送1個月"
                    }
                };

                await context.SubscriptionPlans.AddRangeAsync(subscriptionPlans);
                await context.SaveChangesAsync();
            }

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
            if (adminUser == null)
            {
                throw new InvalidOperationException("Admin user not found. Database initialization failed.");
            }

            // Initialize default categories
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