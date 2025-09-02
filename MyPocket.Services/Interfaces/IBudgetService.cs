using MyPocket.Shared.ViewModels.Budgets;
using MyPocket.Core.Models;

namespace MyPocket.Services.Interfaces
{
    public interface IBudgetService
    {
        Task<List<BudgetViewModel>> GetUserBudgetsAsync(Guid userId, int year, int month);
        Task<BudgetViewModel?> GetBudgetDetailsAsync(Guid userId, Guid budgetId);
        Task<List<Category>> GetUserExpenseCategoriesAsync(Guid userId);
        Task<bool> CreateBudgetAsync(Guid userId, BudgetViewModel model);
        Task<bool> UpdateBudgetAsync(Guid userId, BudgetViewModel model);
        Task<bool> DeleteBudgetAsync(Guid userId, Guid budgetId);
    }
}
