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
                throw new ArgumentException("�Τ�ID���ର��", nameof(userId));

            return await _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.UserId == userId && !t.IsDeleted)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<List<Transaction>> GetUserTransactionsByMonthAsync(Guid userId, int year, int month)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("�Τ�ID���ର��", nameof(userId));
            if (year < 1900 || year > 9999)
                throw new ArgumentException("�L�Ī��~��", nameof(year));
            if (month < 1 || month > 12)
                throw new ArgumentException("�L�Ī����", nameof(month));

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
                throw new ArgumentException("�Τ�ID���ର��", nameof(userId));
            if (transactionId == Guid.Empty)
                throw new ArgumentException("���ID���ର��", nameof(transactionId));

            return await _context.Transactions
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId && 
                                        t.UserId == userId && 
                                        !t.IsDeleted);
        }

        public async Task<decimal> CalculateCurrentSavingAsync(Guid userId, DateTime start, DateTime end)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("�Τ�ID���ର��", nameof(userId));

            var income = await _context.Transactions
                .Where(t => t.UserId == userId && 
                           !t.IsDeleted && 
                           t.TransactionType == "���J" && 
                           t.TransactionDate >= start && 
                           t.TransactionDate <= end)
                .SumAsync(t => t.Amount);

            var expense = await _context.Transactions
                .Where(t => t.UserId == userId && 
                           !t.IsDeleted && 
                           t.TransactionType == "��X" && 
                           t.TransactionDate >= start && 
                           t.TransactionDate <= end)
                .SumAsync(t => t.Amount);

            return income - expense;
        }

        public async Task<(bool success, string message, Transaction? transaction)> CreateTransactionAsync(Guid userId, TransactionCreateModel model)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("�Τ�ID���ର��", nameof(userId));

            try
            {
                // �d������ýT�{���ݩ�ӥΤ�άO�t���q�{����
                var categoryViewModel = await _categoryService.GetUserCategoriesAsync(userId);
                var category = categoryViewModel.DefaultIncomeCategories
                    .Concat(categoryViewModel.DefaultExpenseCategories)
                    .Concat(categoryViewModel.UserIncomeCategories)
                    .Concat(categoryViewModel.UserExpenseCategories)
                    .FirstOrDefault(c => c.CategoryId == model.CategoryId);

                if (category == null)
                    return (false, "�L�Ī�����", null);

                var transaction = new Transaction
                {
                    TransactionId = Guid.NewGuid(),
                    UserId = userId,
                    CategoryId = model.CategoryId,
                    Amount = model.Amount,
                    TransactionType = category.CategoryType,
                    TransactionDate = model.TransactionDate,
                    Description = model.Description,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();

                // ���s�[���������p
                await _context.Entry(transaction)
                    .Reference(t => t.Category)
                    .LoadAsync();

                return (true, "��������w���\�s�W", transaction);
            }
            catch (Exception ex)
            {
                return (false, $"�s�W����: {ex.Message}", null);
            }
        }

        public async Task<(bool success, string message)> DeleteTransactionAsync(Guid userId, Guid transactionId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("�Τ�ID���ର��", nameof(userId));
            if (transactionId == Guid.Empty)
                throw new ArgumentException("���ID���ର��", nameof(transactionId));

            try
            {
                var transaction = await _context.Transactions
                    .FirstOrDefaultAsync(t => t.TransactionId == transactionId && 
                                            t.UserId == userId && 
                                            !t.IsDeleted);

                if (transaction == null)
                    return (false, "�䤣��n�R�����������");

                transaction.IsDeleted = true;
                transaction.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return (true, "��������w���\�R��");
            }
            catch (Exception ex)
            {
                return (false, $"�R������: {ex.Message}");
            }
        }
    }
}
