using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MyPocket.Core.Models;

[Table("SavingGoal")]
public partial class SavingGoal
{
    [Key]
    [Column("GoalID")]
    public Guid GoalId { get; set; }

    [Column("UserID")]
    public Guid UserId { get; set; }

    [StringLength(100)]
    public string GoalName { get; set; } = null!;

    [Column(TypeName = "decimal(12, 2)")]
    public decimal TargetAmount { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal CurrentAmount { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime TargetDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("SavingGoals")]
    public virtual User User { get; set; } = null!;
}
