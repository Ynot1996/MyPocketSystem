using MyPocket.Core.Models;
using MyPocket.Shared.ViewModels.Categories;

namespace MyPocket.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<UserCategoryViewModel> GetUserCategoriesAsync(Guid userId);
        Task CreateCategoryAsync(Guid userId, string name, string type);
        Task DeleteCategoryAsync(Guid userId, Guid categoryId);
    }
}
