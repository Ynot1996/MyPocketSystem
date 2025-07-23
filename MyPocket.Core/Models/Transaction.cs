using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MyPocket.Core.Models;

[Table("Transaction")]
public partial class Transaction
{
    [Key]
    [Column("TransactionID")]
    public Guid TransactionId { get; set; }

    [Column("UserID")]
    public Guid UserId { get; set; }

    [Column("CategoryID")]
    public Guid CategoryId { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal Amount { get; set; }

    [StringLength(5)]
    public string TransactionType { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime TransactionDate { get; set; }

    public string? Description { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    [ForeignKey("CategoryId")]
    [InverseProperty("Transactions")]
    public virtual Category Category { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("Transactions")]
    public virtual User User { get; set; } = null!;
}
