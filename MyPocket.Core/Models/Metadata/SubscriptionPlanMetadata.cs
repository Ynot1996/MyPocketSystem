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
        [Required(ErrorMessage = "���ID������")]
        [Display(Name = "���ID")]
        public Guid PlanId { get; set; }

        [Required(ErrorMessage = "��צW�٬�����")]
        [Display(Name = "��צW��")]
        [StringLength(50, ErrorMessage = "{0}���פ���W�L{1}�Ӧr��")]
        public string PlanName { get; set; } = null!;

        [Required(ErrorMessage = "���欰����")]
        [Display(Name = "����")]
        [Range(0, 999999999999.99, ErrorMessage = "���楲������{1}��{2}����")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "�ϥΤѼƬ�����")]
        [Display(Name = "�ϥΤѼ�")]
        [Range(1, 3650, ErrorMessage = "�ϥΤѼƥ�������{1}��{2}�Ѥ���")]
        public int DurationDays { get; set; }

        [Display(Name = "�y�z")]
        [StringLength(500, ErrorMessage = "{0}���פ���W�L{1}�Ӧr��")]
        public string? Description { get; set; }

        [Display(Name = "�ϥΪ̭q�\")]
        public virtual ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();
    }
}