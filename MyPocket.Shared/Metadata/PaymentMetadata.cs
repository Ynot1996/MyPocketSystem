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
        [Display(Name = "付款ID")]
        public Guid PaymentId { get; set; }

        [Required]
        [Display(Name = "訂閱ID")]
        public Guid SubscriptionId { get; set; }

        [Required]
        [Display(Name = "金額")]
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "付款日期")]
        [DataType(DataType.DateTime)]
        public DateTime PaymentDate { get; set; }

        [Required]
        [Display(Name = "狀態")]
        public string Status { get; set; } = null!;

        [Display(Name = "訂閱")]
        public virtual UserSubscription Subscription { get; set; } = null!;
    }
}