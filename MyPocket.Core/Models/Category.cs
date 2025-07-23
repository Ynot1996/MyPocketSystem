using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MyPocket.Core.Models;

[Table("Category")]
public partial class Category
{
    [Key]
    [Column("CategoryID")]
    public Guid CategoryId { get; set; }

    [Column("UserID")]
    public Guid UserId { get; set; }

    [StringLength(50)]
    public string CategoryName { get; set; } = null!;

    [StringLength(50)]
    public string CategoryType { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
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
