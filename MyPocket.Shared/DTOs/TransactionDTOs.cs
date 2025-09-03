using System.ComponentModel.DataAnnotations;

namespace MyPocket.Shared.DTOs
{
    public class TransactionDTO
    {
        public Guid TransactionId { get; set; }
        public Guid UserId { get; set; }
        public Guid CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public decimal Amount { get; set; }
        public string TransactionType { get; set; } = null!;
        public DateTime TransactionDate { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateTransactionDTO
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid CategoryId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "金額必須大於0")]
        public decimal Amount { get; set; }

        [Required]
        public string TransactionType { get; set; } = null!;

        [Required]
        public DateTime TransactionDate { get; set; }

        [MaxLength(200)]
        public string? Description { get; set; }
    }

    public class UpdateTransactionDTO
    {
        public Guid CategoryId { get; set; }
        
        [Range(0.01, double.MaxValue, ErrorMessage = "金額必須大於0")]
        public decimal Amount { get; set; }
        
        public string TransactionType { get; set; } = null!;
        
        public DateTime TransactionDate { get; set; }
        
        [MaxLength(200)]
        public string? Description { get; set; }
    }
}