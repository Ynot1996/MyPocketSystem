using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using MyPocket.Core.Models;

namespace MyPocket.Shared.Metadata
{
    [ModelMetadataType(typeof(PaymentMetadata))]
    public partial class Payment { }

    public class PaymentMetadata
    {
        [Required]
        [Display(Name = "�I��ID")]
        public Guid PaymentId { get; set; }

        [Required]
        [Display(Name = "�q�\ID")]
        public Guid SubscriptionId { get; set; }

        [Required]
        [Display(Name = "���B")]
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "�I�ڤ��")]
        [DataType(DataType.DateTime)]
        public DateTime PaymentDate { get; set; }

        [Required]
        [Display(Name = "���A")]
        public string Status { get; set; } = null!;

        [Display(Name = "�q�\")]
        public virtual UserSubscription Subscription { get; set; } = null!;
    }
}
