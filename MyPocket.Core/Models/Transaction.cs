using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPocket.Core.Models
{
    public partial class Transaction
    {
        [Key]
        public Guid TransactionId { get; set; }
        public Guid UserId { get; set; }
        public Guid CategoryId { get; set; }
        public decimal Amount { get; set; }
        public string TransactionType { get; set; } = null!;
        public DateTime TransactionDate { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
        [ForeignKey("CategoryId")]
        [InverseProperty("Transactions")]
        public virtual Category Category { get; set; } = null!;
        [ForeignKey("UserId")]
        [InverseProperty("Transactions")]
        public virtual User User { get; set; } = null!;
    }
}
