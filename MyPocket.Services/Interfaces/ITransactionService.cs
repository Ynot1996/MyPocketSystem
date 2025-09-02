using MyPocket.Core.Models;
using MyPocket.Shared.ViewModels.Transactions;

namespace MyPocket.Services.Interfaces
{
    public interface ITransactionService
    {
        /// <summary>
        /// 獲取用戶的所有交易記錄
        /// </summary>
        /// <param name="userId">用戶ID</param>
        /// <returns>交易記錄列表</returns>
        /// <exception cref="ArgumentException">當用戶ID為空時拋出</exception>
        Task<List<Transaction>> GetUserTransactionsAsync(Guid userId);

        /// <summary>
        /// 獲取用戶指定月份的交易記錄
        /// </summary>
        /// <param name="userId">用戶ID</param>
        /// <param name="year">年份</param>
        /// <param name="month">月份</param>
        /// <returns>交易記錄列表</returns>
        /// <exception cref="ArgumentException">當參數無效時拋出</exception>
        Task<List<Transaction>> GetUserTransactionsByMonthAsync(Guid userId, int year, int month);

        /// <summary>
        /// 獲取特定交易記錄
        /// </summary>
        /// <param name="userId">用戶ID</param>
        /// <param name="transactionId">交易ID</param>
        /// <returns>交易記錄，如果不存在則返回null</returns>
        /// <exception cref="ArgumentException">當參數無效時拋出</exception>
        Task<Transaction?> GetTransactionAsync(Guid userId, Guid transactionId);

        /// <summary>
        /// 計算指定時間範圍內的儲蓄金額
        /// </summary>
        /// <param name="userId">用戶ID</param>
        /// <param name="start">開始時間</param>
        /// <param name="end">結束時間</param>
        /// <returns>儲蓄金額</returns>
        /// <exception cref="ArgumentException">當用戶ID為空時拋出</exception>
        Task<decimal> CalculateCurrentSavingAsync(Guid userId, DateTime start, DateTime end);

        /// <summary>
        /// 創建新的交易記錄
        /// </summary>
        /// <param name="userId">用戶ID</param>
        /// <param name="model">交易資料模型</param>
        /// <returns>操作結果，包含成功/失敗訊息和創建的交易記錄</returns>
        /// <exception cref="ArgumentException">當用戶ID為空時拋出</exception>
        Task<(bool success, string message, Transaction? transaction)> CreateTransactionAsync(Guid userId, TransactionCreateModel model);

        /// <summary>
        /// 刪除交易記錄
        /// </summary>
        /// <param name="userId">用戶ID</param>
        /// <param name="transactionId">交易ID</param>
        /// <returns>操作結果，包含成功/失敗訊息</returns>
        /// <exception cref="ArgumentException">當參數無效時拋出</exception>
        Task<(bool success, string message)> DeleteTransactionAsync(Guid userId, Guid transactionId);
    }
}
