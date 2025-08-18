using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPocket.Core.Models;

public partial class Budget
{
    [Key]
    public Guid BudgetId { get; set; }
    public Guid UserId { get; set; }
    public Guid CategoryId { get; set; }
    public decimal Amount { get; set; }
    public string BudgetYear { get; set; } = null!;
    public string BudgetMonth { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    [ForeignKey("CategoryId")]
    [InverseProperty("Budgets")]
    public virtual Category Category { get; set; } = null!;
    [ForeignKey("UserId")]
    [InverseProperty("Budgets")]
    public virtual User User { get; set; } = null!;
}
