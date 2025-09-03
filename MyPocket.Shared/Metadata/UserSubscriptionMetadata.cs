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
        [Display(Name = "�q�\ID")]
        public Guid SubscriptionId { get; set; }

        [Required]
        [Display(Name = "�Τ�ID")]
        public Guid UserId { get; set; }

        [Required]
        [Display(Name = "���ID")]
        public Guid PlanId { get; set; }

        [Required]
        [Display(Name = "�}�l���")]
        [DataType(DataType.DateTime)]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "�������")]
        [DataType(DataType.DateTime)]
        public DateTime EndDate { get; set; }

        [Required]
        [Display(Name = "���A")]
        public string Status { get; set; } = null!;

        [Display(Name = "�I��")]
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

        [Display(Name = "���")]
        public virtual SubscriptionPlan Plan { get; set; } = null!;

        [Display(Name = "�Τ�")]
        public virtual User User { get; set; } = null!;
    }
}
