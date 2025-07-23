using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MyPocket.Core.Models;

[Table("SubscriptionPlan")]
public partial class SubscriptionPlan
{
    [Key]
    [Column("PlanID")]
    public Guid PlanId { get; set; }

    [StringLength(50)]
    public string PlanName { get; set; } = null!;

    [Column(TypeName = "decimal(12, 2)")]
    public decimal Price { get; set; }

    public int DurationDays { get; set; }

    public string? Description { get; set; }

    [InverseProperty("Plan")]
    public virtual ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();
}
