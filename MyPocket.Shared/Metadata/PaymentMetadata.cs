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
        [Display(Name = "¥I´ÚID")]
        public Guid PaymentId { get; set; }

        [Required]
        [Display(Name = "­q¾\ID")]
        public Guid SubscriptionId { get; set; }

        [Required]
        [Display(Name = "ª÷ÃB")]
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "¥I´Ú¤é´Á")]
        [DataType(DataType.DateTime)]
        public DateTime PaymentDate { get; set; }

        [Required]
        [Display(Name = "ª¬ºA")]
        public string Status { get; set; } = null!;

        [Display(Name = "­q¾\")]
        public virtual UserSubscription Subscription { get; set; } = null!;
    }
}
