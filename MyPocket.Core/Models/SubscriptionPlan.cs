using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPocket.Core.Models
{
    [Table("SubscriptionPlan")]
    public partial class SubscriptionPlan
    {
        [Key]
        public Guid PlanId { get; set; }
        public string PlanName { get; set; } = null!;
        public decimal Price { get; set; }
        public int DurationDays { get; set; }
        public string? Description { get; set; }
        [InverseProperty("Plan")]
        public virtual ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();
    }
}