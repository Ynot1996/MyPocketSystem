using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MyPocket.Core.Models;

[Table("User")]
[Index("Email", Name = "UQ__User__A9D1053438F23C0C", IsUnique = true)]
public partial class User
{
    [Key]
    [Column("UserID")]
    public Guid UserId { get; set; }

    [StringLength(250)]
    public string Email { get; set; } = null!;

    [StringLength(100)]
    public string PasswordHash { get; set; } = null!;

    [StringLength(50)]
    public string? Nickname { get; set; }

    [StringLength(20)]
    public string Role { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime CreationDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime LastLoginDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    [InverseProperty("Admin")]
    public virtual ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();

    [InverseProperty("User")]
    public virtual ICollection<Budget> Budgets { get; set; } = new List<Budget>();

    [InverseProperty("User")]
    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

    [InverseProperty("User")]
    public virtual ICollection<SavingGoal> SavingGoals { get; set; } = new List<SavingGoal>();

    [InverseProperty("User")]
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    [InverseProperty("User")]
    public virtual ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();
}
