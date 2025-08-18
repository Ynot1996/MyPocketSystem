using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPocket.Core.Models;

public partial class SavingGoal
{
    [Key]
    public Guid GoalId { get; set; }
    public Guid UserId { get; set; }
    public string GoalName { get; set; } = null!;
    public decimal TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; }
    public DateTime TargetDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    [ForeignKey("UserId")]
    [InverseProperty("SavingGoals")]
    public virtual User User { get; set; } = null!;
}
