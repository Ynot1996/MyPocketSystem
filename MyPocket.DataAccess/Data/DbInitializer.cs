using Microsoft.EntityFrameworkCore;
using MyPocket.Core.Models;

namespace MyPocket.DataAccess.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(MyPocketDBContext context)
        {
            await context.Database.EnsureCreatedAsync();

            if (await context.Users.AnyAsync(u => u.Role == "Admin"))
            {
                return; 
            }

            var adminPasswordHash = BCrypt.Net.BCrypt.HashPassword("12345678");

            var adminUser = new User
            {
                UserId = Guid.NewGuid(),
                Email = "admin@example.com",
                PasswordHash = adminPasswordHash,
                Nickname = "管理員",
                Role = "Admin",
                CreationDate = DateTime.UtcNow,
                LastLoginDate = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await context.Users.AddAsync(adminUser);
            await context.SaveChangesAsync();

        }
    }
}