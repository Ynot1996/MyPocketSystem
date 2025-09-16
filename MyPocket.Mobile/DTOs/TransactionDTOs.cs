using System;

namespace MyPocket.Mobile.DTOs
{
    public class TransactionDTO
    {
        public Guid TransactionId { get; set; }
        public Guid UserId { get; set; }
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public string Type { get; set; } = string.Empty; // 收入/支出
    }

    public class CreateTransactionDTO
    {
        public Guid UserId { get; set; }
        public Guid CategoryId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public string Type { get; set; } = string.Empty;
    }
}
