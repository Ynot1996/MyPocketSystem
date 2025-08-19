using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPocket.Core.Models
{
    [Table("Category")]
    public partial class Category
    {
        [Key]
        public Guid CategoryId { get; set; }
        public Guid UserId { get; set; }
        public string CategoryName { get; set; } = null!;
        public string CategoryType { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
        [InverseProperty("Category")]
        public virtual ICollection<Budget> Budgets { get; set; } = new List<Budget>();
        [InverseProperty("Category")]
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        [ForeignKey("UserId")]
        [InverseProperty("Categories")]
        public virtual User User { get; set; } = null!;
    }
}