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
        [Display(Name = "���ID")]
        public Guid PlanId { get; set; }

        [Required]
        [Display(Name = "��צW��")]
        [StringLength(100)]
        public string PlanName { get; set; } = null!;

        [Required]
        [Display(Name = "����")]
        public decimal Price { get; set; }

        [Required]
        [Display(Name = "�y�z")]
        public string Description { get; set; } = null!;
    }
}
