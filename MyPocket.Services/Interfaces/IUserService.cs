using MyPocket.Shared.ViewModels.Users;
using MyPocket.Core.Models;

namespace MyPocket.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserProfileViewModel?> GetUserProfileAsync(Guid userId);
        Task<bool> UpdateUserProfileAsync(UserProfileViewModel model);
    }
}
