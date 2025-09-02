using MyPocket.Core.Models;

namespace MyPocket.Services.Interfaces
{
    public interface ITransactionService
    {
        Task<List<Transaction>> GetUserTransactionsAsync(Guid userId);
        Task<List<Transaction>> GetUserTransactionsByMonthAsync(Guid userId, int year, int month);
        Task<Transaction?> GetTransactionAsync(Guid userId, Guid transactionId);
        Task<decimal> CalculateCurrentSavingAsync(Guid userId, DateTime start, DateTime end);
        Task CreateTransactionAsync(Transaction transaction);
        Task<bool> DeleteTransactionAsync(Guid userId, Guid transactionId);
    }
}
