using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace MyPocket.Core.Models.Metadata
{
    [ModelMetadataType(typeof(PaymentMetadata))]
    public partial class Payment
    {
    }

    public class PaymentMetadata
    {
        [Required(ErrorMessage = "�I��ID������")]
        [Display(Name = "�I��ID")]
        public Guid PaymentId { get; set; }

        [Required(ErrorMessage = "�q�\ID������")]
        [Display(Name = "�q�\ID")]
        public Guid SubscriptionId { get; set; }

        [Required(ErrorMessage = "�I�ڪ��B������")]
        [Display(Name = "�I�ڪ��B")]
        [Range(0, 999999999999.99, ErrorMessage = "�I�ڪ��B��������{1}��{2}����")]
        public decimal PaymentAmount { get; set; }

        [Required(ErrorMessage = "�I�ڤ覡������")]
        [Display(Name = "�I�ڤ覡")]
        [StringLength(15, ErrorMessage = "{0}���פ���W�L{1}�Ӧr��")]
        public string PaymentWay { get; set; } = null!;

        [Required(ErrorMessage = "�I�ڤ��������")]
        [Display(Name = "�I�ڤ��")]
        [DataType(DataType.DateTime)]
        public DateTime PaymentDate { get; set; }

        [Required(ErrorMessage = "���A������")]
        [Display(Name = "���A")]
        [StringLength(15, ErrorMessage = "{0}���פ���W�L{1}�Ӧr��")]
        public string Status { get; set; } = null!;

        [Required(ErrorMessage = "����s��������")]
        [Display(Name = "����s��")]
        [StringLength(100, ErrorMessage = "{0}���פ���W�L{1}�Ӧr��")]
        public string TransactionCode { get; set; } = null!;

        [Display(Name = "�q�\")]
        public virtual UserSubscription Subscription { get; set; } = null!;
    }
}