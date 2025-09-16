using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPocket.Core.Models;

[Table("UserSubscription")]
public partial class UserSubscription
{
    [Key]
    public Guid SubscriptionId { get; set; }

    public Guid UserId { get; set; }

    public Guid PlanId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

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
