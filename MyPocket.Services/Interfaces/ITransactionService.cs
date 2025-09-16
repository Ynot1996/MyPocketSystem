using MyPocket.Core.Models;
using MyPocket.Shared.ViewModels.Transactions;

namespace MyPocket.Services.Interfaces
{
    public interface ITransactionService
    {
        Task<List<Transaction>> GetUserTransactionsAsync(Guid userId);

        Task<List<Transaction>> GetUserTransactionsByMonthAsync(Guid userId, int year, int month);

        Task<Transaction?> GetTransactionAsync(Guid userId, Guid transactionId);

        Task<decimal> CalculateCurrentSavingAsync(Guid userId, DateTime start, DateTime end);

        Task<(bool success, string message, Transaction? transaction)> CreateTransactionAsync(Guid userId, TransactionCreateModel model);

        Task<(bool success, string message)> DeleteTransactionAsync(Guid userId, Guid transactionId);
    }
}
