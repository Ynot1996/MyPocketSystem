using Microsoft.EntityFrameworkCore;
using MyPocket.Core.Models;
using MyPocket.DataAccess.Data;
using MyPocket.Services.Interfaces;

namespace MyPocket.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly MyPocketDBContext _context;
        public TransactionService(MyPocketDBContext context)
        {
            _context = context;
        }

        public async Task<List<Transaction>> GetUserTransactionsAsync(Guid userId)
        {
            return await _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.UserId == userId && !t.IsDeleted)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<List<Transaction>> GetUserTransactionsByMonthAsync(Guid userId, int year, int month)
        {
            return await _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.UserId == userId && !t.IsDeleted && t.TransactionDate.Year == year && t.TransactionDate.Month == month)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<Transaction?> GetTransactionAsync(Guid userId, Guid transactionId)
        {
            return await _context.Transactions
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId && t.UserId == userId && !t.IsDeleted);
        }

        public async Task<decimal> CalculateCurrentSavingAsync(Guid userId, DateTime start, DateTime end)
        {
            var income = await _context.Transactions.Where(t => t.UserId == userId && !t.IsDeleted && t.TransactionType == "收入" && t.TransactionDate >= start && t.TransactionDate <= end).SumAsync(t => t.Amount);
            var expense = await _context.Transactions.Where(t => t.UserId == userId && !t.IsDeleted && t.TransactionType == "支出" && t.TransactionDate >= start && t.TransactionDate <= end).SumAsync(t => t.Amount);
            return income - expense;
        }

        public async Task CreateTransactionAsync(Transaction transaction)
        {
            transaction.TransactionId = Guid.NewGuid();
            transaction.CreatedAt = DateTime.UtcNow;
            transaction.UpdatedAt = DateTime.UtcNow;
            transaction.IsDeleted = false;
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteTransactionAsync(Guid userId, Guid transactionId)
        {
            var transaction = await _context.Transactions.FirstOrDefaultAsync(t => t.TransactionId == transactionId && t.UserId == userId && !t.IsDeleted);
            if (transaction == null) return false;
            transaction.IsDeleted = true;
            transaction.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
