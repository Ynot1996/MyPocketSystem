using MyPocket.Core.Models;
using MyPocket.Shared.ViewModels.Transactions;

namespace MyPocket.Services.Interfaces
{
    public interface ITransactionService
    {
        /// <summary>
        /// ����Τ᪺�Ҧ�����O��
        /// </summary>
        /// <param name="userId">�Τ�ID</param>
        /// <returns>����O���C��</returns>
        /// <exception cref="ArgumentException">��Τ�ID���ŮɩߥX</exception>
        Task<List<Transaction>> GetUserTransactionsAsync(Guid userId);

        /// <summary>
        /// ����Τ���w���������O��
        /// </summary>
        /// <param name="userId">�Τ�ID</param>
        /// <param name="year">�~��</param>
        /// <param name="month">���</param>
        /// <returns>����O���C��</returns>
        /// <exception cref="ArgumentException">��ѼƵL�ĮɩߥX</exception>
        Task<List<Transaction>> GetUserTransactionsByMonthAsync(Guid userId, int year, int month);

        /// <summary>
        /// ����S�w����O��
        /// </summary>
        /// <param name="userId">�Τ�ID</param>
        /// <param name="transactionId">���ID</param>
        /// <returns>����O���A�p�G���s�b�h��^null</returns>
        /// <exception cref="ArgumentException">��ѼƵL�ĮɩߥX</exception>
        Task<Transaction?> GetTransactionAsync(Guid userId, Guid transactionId);

        /// <summary>
        /// �p����w�ɶ��d�򤺪��x�W���B
        /// </summary>
        /// <param name="userId">�Τ�ID</param>
        /// <param name="start">�}�l�ɶ�</param>
        /// <param name="end">�����ɶ�</param>
        /// <returns>�x�W���B</returns>
        /// <exception cref="ArgumentException">��Τ�ID���ŮɩߥX</exception>
        Task<decimal> CalculateCurrentSavingAsync(Guid userId, DateTime start, DateTime end);

        /// <summary>
        /// �Ыطs������O��
        /// </summary>
        /// <param name="userId">�Τ�ID</param>
        /// <param name="model">�����Ƽҫ�</param>
        /// <returns>�ާ@���G�A�]�t���\/���ѰT���M�Ыت�����O��</returns>
        /// <exception cref="ArgumentException">��Τ�ID���ŮɩߥX</exception>
        Task<(bool success, string message, Transaction? transaction)> CreateTransactionAsync(Guid userId, TransactionCreateModel model);

        /// <summary>
        /// �R������O��
        /// </summary>
        /// <param name="userId">�Τ�ID</param>
        /// <param name="transactionId">���ID</param>
        /// <returns>�ާ@���G�A�]�t���\/���ѰT��</returns>
        /// <exception cref="ArgumentException">��ѼƵL�ĮɩߥX</exception>
        Task<(bool success, string message)> DeleteTransactionAsync(Guid userId, Guid transactionId);
    }
}
