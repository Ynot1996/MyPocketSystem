using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MyPocket.Core.Models;

[Table("UserSubscription")]
public partial class UserSubscription
{
    [Key]
    [Column("SubscriptionID")]
    public Guid SubscriptionId { get; set; }

    [Column("UserID")]
    public Guid UserId { get; set; }

    [Column("PlanID")]
    public Guid PlanId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime StartDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime EndDate { get; set; }

    [StringLength(15)]
    public string Status { get; set; } = null!;

    [InverseProperty("Subscription")]
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    [ForeignKey("PlanId")]
    [InverseProperty("UserSubscriptions")]
    public virtual SubscriptionPlan Plan { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("UserSubscriptions")]
    public virtual User User { get; set; } = null!;
}
