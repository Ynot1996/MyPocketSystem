using Microsoft.EntityFrameworkCore;
using MyPocket.Core.Models;
using MyPocket.DataAccess.Data;

namespace MyPocket.Tests
{
    /// <summary>
    /// Builds isolated in-memory database contexts and seeds the minimal data
    /// (admin user + default categories) that mirrors what DbInitializer creates.
    /// </summary>
    public static class TestHelper
    {
        public const string AdminEmail = "admin@example.com";

        public static MyPocketDBContext NewContext()
        {
            var options = new DbContextOptionsBuilder<MyPocketDBContext>()
                // Unique name per call so tests never share state.
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new MyPocketDBContext(options);
        }

        public static User SeedAdmin(MyPocketDBContext context)
        {
            var admin = new User
            {
                UserId = Guid.NewGuid(),
                Email = AdminEmail,
                PasswordHash = "x",
                Nickname = "Admin",
                Role = "Admin",
                CreationDate = DateTime.UtcNow,
                LastLoginDate = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            context.Users.Add(admin);
            context.SaveChanges();
            return admin;
        }

        public static User SeedUser(MyPocketDBContext context, string email = "user@example.com", string role = "FreeMember")
        {
            var user = new User
            {
                UserId = Guid.NewGuid(),
                Email = email,
                PasswordHash = "x",
                Nickname = "User",
                Role = role,
                CreationDate = DateTime.UtcNow,
                LastLoginDate = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            context.Users.Add(user);
            context.SaveChanges();
            return user;
        }

        public static Category SeedCategory(MyPocketDBContext context, Guid ownerId, string name, string type)
        {
            var category = new Category
            {
                CategoryId = Guid.NewGuid(),
                UserId = ownerId,
                CategoryName = name,
                CategoryType = type,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            context.Categories.Add(category);
            context.SaveChanges();
            return category;
        }

        public static SubscriptionPlan SeedPlan(MyPocketDBContext context, string name, decimal price, int durationDays)
        {
            var plan = new SubscriptionPlan
            {
                PlanId = Guid.NewGuid(),
                PlanName = name,
                Price = price,
                DurationDays = durationDays,
                Description = name
            };
            context.SubscriptionPlans.Add(plan);
            context.SaveChanges();
            return plan;
        }
    }
}
