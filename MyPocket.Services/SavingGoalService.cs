using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyPocket.Core.Models;
using MyPocket.DataAccess.Data;
using MyPocket.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MyPocket.Services
{
    public class SavingGoalService : ISavingGoalService
    {
        private readonly MyPocketDBContext _context;
        private readonly ITransactionService _transactionService;
        public SavingGoalService(MyPocketDBContext context, ITransactionService transactionService)
        {
            _context = context;
            _transactionService = transactionService;
        }

        public async Task<SavingGoal> GetOrCreateMonthlyGoalAsync(Guid userId, int year, int month)
        {
            var name = $"{year}年{month:D2}月儲蓄目標";
            var start = new DateTime(year, month, 1);
            var end = start.AddMonths(1).AddDays(-1);
            var goal = await _context.SavingGoals.FirstOrDefaultAsync(g => g.UserId == userId && g.GoalName == name && !g.IsDeleted);
            if (goal == null)
            {
                goal = new SavingGoal
                {
                    GoalId = Guid.NewGuid(),
                    UserId = userId,
                    GoalName = name,
                    TargetAmount = 0, // 用戶可後續設定
                    CurrentAmount = await _transactionService.CalculateCurrentSavingAsync(userId, start, end),
                    TargetDate = end,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };
                _context.SavingGoals.Add(goal);
                await _context.SaveChangesAsync();
            }
            else
            {
                goal.CurrentAmount = await _transactionService.CalculateCurrentSavingAsync(userId, start, end);
                goal.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            return goal;
        }

        public async Task<SavingGoal> GetOrCreateYearlyGoalAsync(Guid userId, int year)
        {
            var name = $"{year}年儲蓄目標";
            var start = new DateTime(year, 1, 1);
            var end = new DateTime(year, 12, 31);
            var goal = await _context.SavingGoals.FirstOrDefaultAsync(g => g.UserId == userId && g.GoalName == name && !g.IsDeleted);
            if (goal == null)
            {
                goal = new SavingGoal
                {
                    GoalId = Guid.NewGuid(),
                    UserId = userId,
                    GoalName = name,
                    TargetAmount = 0, // 用戶可後續設定
                    CurrentAmount = await _transactionService.CalculateCurrentSavingAsync(userId, start, end),
                    TargetDate = end,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };
                _context.SavingGoals.Add(goal);
                await _context.SaveChangesAsync();
            }
            else
            {
                goal.CurrentAmount = await _transactionService.CalculateCurrentSavingAsync(userId, start, end);
                goal.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            return goal;
        }

        public async Task<decimal> CalculateCurrentSavingAsync(Guid userId, DateTime start, DateTime end)
        {
            // 已遷移到 TransactionService，這裡僅為相容性保留
            return await _transactionService.CalculateCurrentSavingAsync(userId, start, end);
        }

        public async Task<List<SavingGoal>> GetUserGoalsAsync(Guid userId)
        {
            return await _context.SavingGoals.Where(g => g.UserId == userId && !g.IsDeleted).OrderByDescending(g => g.CreatedAt).ToListAsync();
        }

        public async Task CreateOrUpdateGoalAsync(SavingGoal goal)
        {
            var existing = await _context.SavingGoals.FirstOrDefaultAsync(g => g.GoalId == goal.GoalId && g.UserId == goal.UserId);
            if (existing == null)
            {
                goal.CreatedAt = DateTime.UtcNow;
                goal.UpdatedAt = DateTime.UtcNow;
                _context.SavingGoals.Add(goal);
            }
            else
            {
                existing.GoalName = goal.GoalName;
                existing.TargetAmount = goal.TargetAmount;
                existing.CurrentAmount = goal.CurrentAmount;
                existing.TargetDate = goal.TargetDate;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();
        }

        public async Task DeleteGoalAsync(Guid userId, Guid goalId)
        {
            var goal = await _context.SavingGoals.FirstOrDefaultAsync(g => g.GoalId == goalId && g.UserId == userId);
            if (goal != null)
            {
                goal.IsDeleted = true;
                goal.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}
