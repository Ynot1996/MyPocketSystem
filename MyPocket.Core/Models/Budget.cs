using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MyPocket.Core.Models;

[Table("Budget")]
public partial class Budget
{
    [Key]
    [Column("BudgetID")]
    public Guid BudgetId { get; set; }

    [Column("UserID")]
    public Guid UserId { get; set; }

    [Column("CategoryID")]
    public Guid CategoryId { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal Amount { get; set; }

    [StringLength(4)]
    public string BudgetYear { get; set; } = null!;

    [StringLength(2)]
    public string BudgetMonth { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    [ForeignKey("CategoryId")]
    [InverseProperty("Budgets")]
    public virtual Category Category { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("Budgets")]
    public virtual User User { get; set; } = null!;
}
