using Microsoft.EntityFrameworkCore;
using MyPocket.Core.Models;
using MyPocket.DataAccess.Data;
using MyPocket.Services.Interfaces;
using MyPocket.Shared.ViewModels.Transactions;

namespace MyPocket.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly MyPocketDBContext _context;
        private readonly ICategoryService _categoryService;

        public TransactionService(MyPocketDBContext context, ICategoryService categoryService)
        {
            _context = context;
            _categoryService = categoryService;
        }

        public async Task<List<Transaction>> GetUserTransactionsAsync(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty.", nameof(userId));

            return await _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.UserId == userId && !t.IsDeleted)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<List<Transaction>> GetUserTransactionsByMonthAsync(Guid userId, int year, int month)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty.", nameof(userId));
            if (year < 1900 || year > 9999)
                throw new ArgumentException("Invalid year.", nameof(year));
            if (month < 1 || month > 12)
                throw new ArgumentException("Invalid month.", nameof(month));

            return await _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.UserId == userId &&
                           !t.IsDeleted &&
                           t.TransactionDate.Year == year &&
                           t.TransactionDate.Month == month)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<Transaction?> GetTransactionAsync(Guid userId, Guid transactionId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty.", nameof(userId));
            if (transactionId == Guid.Empty)
                throw new ArgumentException("Transaction ID cannot be empty.", nameof(transactionId));

            return await _context.Transactions
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId &&
                                        t.UserId == userId &&
                                        !t.IsDeleted);
        }

        public async Task<decimal> CalculateCurrentSavingAsync(Guid userId, DateTime start, DateTime end)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty.", nameof(userId));

            var income = await _context.Transactions
                .Where(t => t.UserId == userId &&
                           !t.IsDeleted &&
                           t.TransactionType == "收入" &&
                           t.TransactionDate >= start &&
                           t.TransactionDate <= end)
                .SumAsync(t => t.Amount);

            var expense = await _context.Transactions
                .Where(t => t.UserId == userId &&
                           !t.IsDeleted &&
                           t.TransactionType == "支出" &&
                           t.TransactionDate >= start &&
                           t.TransactionDate <= end)
                .SumAsync(t => t.Amount);

            return income - expense;
        }

        public async Task<(bool success, string message, Transaction? transaction)> CreateTransactionAsync(Guid userId, TransactionCreateModel model)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty.", nameof(userId));

            try
            {
                // Find the category and verify it belongs to the user or is a system default.
                var categoryViewModel = await _categoryService.GetUserCategoriesAsync(userId);
                var category = categoryViewModel.DefaultIncomeCategories
                    .Concat(categoryViewModel.DefaultExpenseCategories)
                    .Concat(categoryViewModel.UserIncomeCategories)
                    .Concat(categoryViewModel.UserExpenseCategories)
                    .FirstOrDefault(c => c.CategoryId == model.CategoryId);

                if (category == null)
                    return (false, "InvalidCategory", null);

                var transaction = new Transaction
                {
                    TransactionId = Guid.NewGuid(),
                    UserId = userId,
                    CategoryId = model.CategoryId,
                    Amount = model.Amount,
                    Currency = string.IsNullOrWhiteSpace(model.Currency) ? "GBP" : model.Currency.ToUpperInvariant(),
                    TransactionType = category.CategoryType,
                    TransactionDate = model.TransactionDate,
                    Description = model.Description,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();

                // Reload the category navigation property.
                await _context.Entry(transaction)
                    .Reference(t => t.Category)
                    .LoadAsync();

                return (true, "TransactionCreated", transaction);
            }
            catch (Exception ex)
            {
                return (false, $"CreateFailed|{ex.Message}", null);
            }
        }

        public async Task<(bool success, string message)> DeleteTransactionAsync(Guid userId, Guid transactionId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty.", nameof(userId));
            if (transactionId == Guid.Empty)
                throw new ArgumentException("Transaction ID cannot be empty.", nameof(transactionId));

            try
            {
                var transaction = await _context.Transactions
                    .FirstOrDefaultAsync(t => t.TransactionId == transactionId &&
                                            t.UserId == userId &&
                                            !t.IsDeleted);

                if (transaction == null)
                    return (false, "TransactionNotFound");

                transaction.IsDeleted = true;
                transaction.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return (true, "TransactionDeleted");
            }
            catch (Exception ex)
            {
                return (false, $"DeleteFailed|{ex.Message}");
            }
        }
    }
}