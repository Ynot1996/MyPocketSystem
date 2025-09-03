using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using MyPocket.Core.Models;

namespace MyPocket.Shared.Metadata
{
    [ModelMetadataType(typeof(TransactionMetadata))]
    public partial class Transaction { }

    public class TransactionMetadata
    {
        [Required]
        [Display(Name = "交易ID")]
        public Guid TransactionId { get; set; }

        [Required]
        [Display(Name = "用戶ID")]
        public Guid UserId { get; set; }

        [Required]
        [Display(Name = "分類ID")]
        public Guid CategoryId { get; set; }

        [Required]
        [Display(Name = "金額")]
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "交易類型")]
        public string TransactionType { get; set; } = null!;

        [Required]
        [Display(Name = "交易日期")]
        [DataType(DataType.DateTime)]
        public DateTime TransactionDate { get; set; }

        [Display(Name = "描述")]
        public string? Description { get; set; }

        [Display(Name = "建立時間")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "更新時間")]
        public DateTime UpdatedAt { get; set; }

        [Display(Name = "分類")]
        public virtual Category Category { get; set; } = null!;

        [Display(Name = "用戶")]
        public virtual User User { get; set; } = null!;
    }
}
