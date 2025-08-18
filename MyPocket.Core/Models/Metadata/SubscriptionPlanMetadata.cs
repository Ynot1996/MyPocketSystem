using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace MyPocket.Core.Models.Metadata
{
    [ModelMetadataType(typeof(SubscriptionPlanMetadata))]
    public partial class SubscriptionPlan
    {
    }

    public class SubscriptionPlanMetadata
    {
        [Required(ErrorMessage = "方案ID為必填")]
        [Display(Name = "方案ID")]
        public Guid PlanId { get; set; }

        [Required(ErrorMessage = "方案名稱為必填")]
        [Display(Name = "方案名稱")]
        [StringLength(50, ErrorMessage = "{0}長度不能超過{1}個字元")]
        public string PlanName { get; set; } = null!;

        [Required(ErrorMessage = "價格為必填")]
        [Display(Name = "價格")]
        [Range(0, 999999999999.99, ErrorMessage = "價格必須介於{1}到{2}之間")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "使用天數為必填")]
        [Display(Name = "使用天數")]
        [Range(1, 3650, ErrorMessage = "使用天數必須介於{1}到{2}天之間")]
        public int DurationDays { get; set; }

        [Display(Name = "描述")]
        [StringLength(500, ErrorMessage = "{0}長度不能超過{1}個字元")]
        public string? Description { get; set; }

        [Display(Name = "使用者訂閱")]
        public virtual ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();
    }
}