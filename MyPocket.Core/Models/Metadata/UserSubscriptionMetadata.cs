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
        [Required(ErrorMessage = "�q�\ID������")]
        [Display(Name = "�q�\ID")]
        public Guid SubscriptionId { get; set; }

        [Required(ErrorMessage = "�ϥΪ�ID������")]
        [Display(Name = "�ϥΪ�ID")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "���ID������")]
        [Display(Name = "���ID")]
        public Guid PlanId { get; set; }

        [Required(ErrorMessage = "�}�l���������")]
        [Display(Name = "�}�l���")]
        [DataType(DataType.DateTime)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "�������������")]
        [Display(Name = "�������")]
        [DataType(DataType.DateTime)]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "���A������")]
        [Display(Name = "���A")]
        [StringLength(15, ErrorMessage = "{0}���פ���W�L{1}�Ӧr��")]
        public string Status { get; set; } = null!;

        [Display(Name = "�I��")]
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

        [Display(Name = "���")]
        public virtual SubscriptionPlan Plan { get; set; } = null!;

        [Display(Name = "�ϥΪ�")]
        public virtual User User { get; set; } = null!;
    }
}