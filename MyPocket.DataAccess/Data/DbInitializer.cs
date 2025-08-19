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
            }

            bool hasAdmin = await context.Users.AnyAsync(u => u.Email == "admin@example.com");
            if (!hasAdmin)
            {
                var adminUser = new User
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
                await context.Users.AddAsync(adminUser);
            }
   
            await context.SaveChangesAsync();
        }
    }
}