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
        [Required(ErrorMessage = "付款ID為必填")]
        [Display(Name = "付款ID")]
        public Guid PaymentId { get; set; }

        [Required(ErrorMessage = "訂閱ID為必填")]
        [Display(Name = "訂閱ID")]
        public Guid SubscriptionId { get; set; }

        [Required(ErrorMessage = "付款金額為必填")]
        [Display(Name = "付款金額")]
        [Range(0, 999999999999.99, ErrorMessage = "付款金額必須介於{1}到{2}之間")]
        public decimal PaymentAmount { get; set; }

        [Required(ErrorMessage = "付款方式為必填")]
        [Display(Name = "付款方式")]
        [StringLength(15, ErrorMessage = "{0}長度不能超過{1}個字元")]
        public string PaymentWay { get; set; } = null!;

        [Required(ErrorMessage = "付款日期為必填")]
        [Display(Name = "付款日期")]
        [DataType(DataType.DateTime)]
        public DateTime PaymentDate { get; set; }

        [Required(ErrorMessage = "狀態為必填")]
        [Display(Name = "狀態")]
        [StringLength(15, ErrorMessage = "{0}長度不能超過{1}個字元")]
        public string Status { get; set; } = null!;

        [Required(ErrorMessage = "交易編號為必填")]
        [Display(Name = "交易編號")]
        [StringLength(100, ErrorMessage = "{0}長度不能超過{1}個字元")]
        public string TransactionCode { get; set; } = null!;

        [Display(Name = "訂閱")]
        public virtual UserSubscription Subscription { get; set; } = null!;
    }
}