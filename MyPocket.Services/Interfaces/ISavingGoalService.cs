using MyPocket.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;       

namespace MyPocket.Services.Interfaces
{
    public interface ISavingGoalService
    {
        Task<SavingGoal> GetOrCreateMonthlyGoalAsync(Guid userId, int year, int month);
        Task<SavingGoal> GetOrCreateYearlyGoalAsync(Guid userId, int year);
        Task<decimal> CalculateCurrentSavingAsync(Guid userId, DateTime start, DateTime end);
        Task<List<SavingGoal>> GetUserGoalsAsync(Guid userId);
        Task CreateOrUpdateGoalAsync(SavingGoal goal);
        Task DeleteGoalAsync(Guid userId, Guid goalId);
    }
}
