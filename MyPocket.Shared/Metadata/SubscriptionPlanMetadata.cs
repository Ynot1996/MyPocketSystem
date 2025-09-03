using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using MyPocket.Core.Models;

namespace MyPocket.Shared.Metadata
{
    [ModelMetadataType(typeof(SubscriptionPlanMetadata))]
    public partial class SubscriptionPlan { }

    public class SubscriptionPlanMetadata
    {
        [Required]
        [Display(Name = "方案ID")]
        public Guid PlanId { get; set; }

        [Required]
        [Display(Name = "方案名稱")]
        [StringLength(100)]
        public string PlanName { get; set; } = null!;

        [Required]
        [Display(Name = "價格")]
        public decimal Price { get; set; }

        [Required]
        [Display(Name = "描述")]
        public string Description { get; set; } = null!;
    }
}
