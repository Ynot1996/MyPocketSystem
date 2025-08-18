using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace MyPocket.Core.Models.Metadata
{
    [ModelMetadataType(typeof(UserSubscriptionMetadata))]
    public partial class UserSubscription
    {
    }

    public class UserSubscriptionMetadata
    {
        [Required(ErrorMessage = "訂閱ID為必填")]
        [Display(Name = "訂閱ID")]
        public Guid SubscriptionId { get; set; }

        [Required(ErrorMessage = "使用者ID為必填")]
        [Display(Name = "使用者ID")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "方案ID為必填")]
        [Display(Name = "方案ID")]
        public Guid PlanId { get; set; }

        [Required(ErrorMessage = "開始日期為必填")]
        [Display(Name = "開始日期")]
        [DataType(DataType.DateTime)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "結束日期為必填")]
        [Display(Name = "結束日期")]
        [DataType(DataType.DateTime)]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "狀態為必填")]
        [Display(Name = "狀態")]
        [StringLength(15, ErrorMessage = "{0}長度不能超過{1}個字元")]
        public string Status { get; set; } = null!;

        [Display(Name = "付款")]
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

        [Display(Name = "方案")]
        public virtual SubscriptionPlan Plan { get; set; } = null!;

        [Display(Name = "使用者")]
        public virtual User User { get; set; } = null!;
    }
}