using Microsoft.EntityFrameworkCore;
using MyPocket.Core.Models;
using MyPocket.DataAccess.Data;
using MyPocket.Services.Interfaces;
using MyPocket.Shared.ViewModels.Budgets;

namespace MyPocket.Services
{
    public class BudgetService : IBudgetService
    {
        private readonly MyPocketDBContext _context;
        public BudgetService(MyPocketDBContext context)
        {
            _context = context;
        }

        public async Task<List<BudgetViewModel>> GetUserBudgetsAsync(Guid userId, int year, int month)
        {
            var yearStr = year.ToString();
            var monthStr = month.ToString("D2");
            var budgets = await _context.Budgets
                .Where(b => b.UserId == userId && !b.IsDeleted && b.BudgetYear == yearStr && b.BudgetMonth == monthStr)
                .Include(b => b.Category)
                .ToListAsync();
            var transactions = await _context.Transactions
                .Where(t => t.UserId == userId && !t.IsDeleted && t.TransactionDate.Year == year && t.TransactionDate.Month == month)
                .ToListAsync();
            return budgets.Select(b => new BudgetViewModel
            {
                BudgetId = b.BudgetId,
                CategoryId = b.CategoryId,
                CategoryName = b.Category?.CategoryName ?? "",
                CategoryType = b.Category?.CategoryType ?? "",
                Amount = b.Amount,
                BudgetYear = b.BudgetYear,
                BudgetMonth = b.BudgetMonth,
                Spent = transactions.Where(t => t.CategoryId == b.CategoryId).Sum(t => t.Amount)
            }).ToList();
        }

        public async Task<BudgetViewModel?> GetBudgetDetailsAsync(Guid userId, Guid budgetId)
        {
            var budget = await _context.Budgets.Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.BudgetId == budgetId && b.UserId == userId && !b.IsDeleted);
            if (budget == null) return null;
            int year = int.Parse(budget.BudgetYear);
            int month = int.Parse(budget.BudgetMonth);
            var spent = await _context.Transactions
                .Where(t => t.UserId == userId && !t.IsDeleted && t.TransactionDate.Year == year && t.TransactionDate.Month == month && t.CategoryId == budget.CategoryId)
                .SumAsync(t => t.Amount);
            return new BudgetViewModel
            {
                BudgetId = budget.BudgetId,
                CategoryId = budget.CategoryId,
                CategoryName = budget.Category?.CategoryName ?? "",
                CategoryType = budget.Category?.CategoryType ?? "",
                Amount = budget.Amount,
                BudgetYear = budget.BudgetYear,
                BudgetMonth = budget.BudgetMonth,
                Spent = spent
            };
        }

        public async Task<List<Category>> GetUserExpenseCategoriesAsync(Guid userId)
        {
            var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Role == "Admin");
            return await _context.Categories
                .Where(c => !c.IsDeleted && c.CategoryType == "¤ä¥X" && (c.UserId == userId || (adminUser != null && c.UserId == adminUser.UserId)))
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
        }

        public async Task<bool> CreateBudgetAsync(Guid userId, BudgetViewModel model)
        {
            var now = DateTime.UtcNow;
            var budget = new Budget
            {
                BudgetId = Guid.NewGuid(),
                UserId = userId,
                CategoryId = model.CategoryId,
                Amount = model.Amount,
                BudgetYear = now.Year.ToString(),
                BudgetMonth = now.Month.ToString("D2"),
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            };
            _context.Budgets.Add(budget);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateBudgetAsync(Guid userId, BudgetViewModel model)
        {
            var budget = await _context.Budgets.FirstOrDefaultAsync(b => b.BudgetId == model.BudgetId && b.UserId == userId && !b.IsDeleted);
            if (budget == null) return false;
            budget.Amount = model.Amount;
            budget.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteBudgetAsync(Guid userId, Guid budgetId)
        {
            var budget = await _context.Budgets.FirstOrDefaultAsync(b => b.BudgetId == budgetId && b.UserId == userId && !b.IsDeleted);
            if (budget == null) return false;
            budget.IsDeleted = true;
            budget.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
