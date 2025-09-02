using Microsoft.EntityFrameworkCore;
using MyPocket.DataAccess.Data;
using MyPocket.Services.Interfaces;
using MyPocket.Shared.ViewModels.Users;
using MyPocket.Core.Models;

namespace MyPocket.Services
{
    public class UserService : IUserService
    {
        private readonly MyPocketDBContext _context;
        public UserService(MyPocketDBContext context)
        {
            _context = context;
        }

        public async Task<UserProfileViewModel?> GetUserProfileAsync(Guid userId)
        {
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return null;
            return new UserProfileViewModel
            {
                UserId = user.UserId,
                Email = user.Email,
                Nickname = user.Nickname,
                CreationDate = user.CreationDate,
                LastLoginDate = user.LastLoginDate
            };
        }

        public async Task<bool> UpdateUserProfileAsync(UserProfileViewModel model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == model.UserId);
            if (user == null) return false;
            user.Nickname = model.Nickname;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
