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
        [Display(Name = "訂閱ID")]
        public Guid SubscriptionId { get; set; }

        [Required]
        [Display(Name = "用戶ID")]
        public Guid UserId { get; set; }

        [Required]
        [Display(Name = "方案ID")]
        public Guid PlanId { get; set; }

        [Required]
        [Display(Name = "開始日期")]
        [DataType(DataType.DateTime)]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "結束日期")]
        [DataType(DataType.DateTime)]
        public DateTime EndDate { get; set; }

        [Required]
        [Display(Name = "狀態")]
        public string Status { get; set; } = null!;

        [Display(Name = "付款")]
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

        [Display(Name = "方案")]
        public virtual SubscriptionPlan Plan { get; set; } = null!;

        [Display(Name = "用戶")]
        public virtual User User { get; set; } = null!;
    }
}
