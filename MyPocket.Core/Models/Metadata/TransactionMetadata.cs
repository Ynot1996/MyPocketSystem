using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace MyPocket.Core.Models.Metadata
{
    [ModelMetadataType(typeof(TransactionMetadata))]
    public partial class Transaction
    {
    }

    public class TransactionMetadata
    {
        [Required(ErrorMessage = "交易ID為必填")]
        [Display(Name = "交易ID")]
        public Guid TransactionId { get; set; }

        [Required(ErrorMessage = "使用者ID為必填")]
        [Display(Name = "使用者ID")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "分類ID為必填")]
        [Display(Name = "分類ID")]
        public Guid CategoryId { get; set; }

        [Required(ErrorMessage = "金額為必填")]
        [Display(Name = "金額")]
        [Range(0, 999999999999.99, ErrorMessage = "金額必須介於{1}到{2}之間")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "交易類型為必填")]
        [Display(Name = "交易類型")]
        [StringLength(5, ErrorMessage = "{0}長度不能超過{1}個字元")]
        public string TransactionType { get; set; } = null!;

        [Required(ErrorMessage = "交易日期為必填")]
        [Display(Name = "交易日期")]
        [DataType(DataType.DateTime)]
        public DateTime TransactionDate { get; set; }

        [Display(Name = "描述")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "建立時間為必填")]
        [Display(Name = "建立時間")]
        public DateTime CreatedAt { get; set; }

        [Required(ErrorMessage = "更新時間為必填")]
        [Display(Name = "更新時間")]
        public DateTime UpdatedAt { get; set; }

        [Display(Name = "是否已刪除")]
        public bool IsDeleted { get; set; }
        
        [Display(Name = "分類")]
        public virtual Category Category { get; set; } = null!;

        [Display(Name = "使用者")]
        public virtual User User { get; set; } = null!;
    }
}