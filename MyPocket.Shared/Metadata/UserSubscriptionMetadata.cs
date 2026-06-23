using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using MyPocket.Core.Models;

namespace MyPocket.Shared.Metadata
{
    [ModelMetadataType(typeof(UserSubscriptionMetadata))]
    public partial class UserSubscription { }

    public class UserSubscriptionMetadata
    {
        [Required]
        [Display(Name = "Subscription ID")]
        public Guid SubscriptionId { get; set; }

        [Required]
        [Display(Name = "User ID")]
        public Guid UserId { get; set; }

        [Required]
        [Display(Name = "Plan ID")]
        public Guid PlanId { get; set; }

        [Required]
        [Display(Name = "Start Date")]
        [DataType(DataType.DateTime)]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "End Date")]
        [DataType(DataType.DateTime)]
        public DateTime EndDate { get; set; }

        [Required]
        [Display(Name = "Status")]
        public string Status { get; set; } = null!;

        [Display(Name = "Payments")]
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

        [Display(Name = "Plan")]
        public virtual SubscriptionPlan Plan { get; set; } = null!;

        [Display(Name = "User")]
        public virtual User User { get; set; } = null!;
    }
}
