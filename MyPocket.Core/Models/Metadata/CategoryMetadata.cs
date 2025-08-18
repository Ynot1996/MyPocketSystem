using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace MyPocket.Core.Models.Metadata
{
    [ModelMetadataType(typeof(CategoryMetadata))]
    public partial class Category
    {
    }

    public class CategoryMetadata
    {
        [Required(ErrorMessage = "分類ID為必填")]
        [Display(Name = "分類ID")]
        public Guid CategoryId { get; set; }

        [Required(ErrorMessage = "使用者ID為必填")]
        [Display(Name = "使用者ID")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "分類名稱為必填")]
        [Display(Name = "分類名稱")]
        [StringLength(50, ErrorMessage = "{0}長度不能超過{1}個字元")]
        public string CategoryName { get; set; } = null!;

        [Required(ErrorMessage = "分類類型為必填")]
        [Display(Name = "分類類型")]
        [StringLength(50, ErrorMessage = "{0}長度不能超過{1}個字元")]
        public string CategoryType { get; set; } = null!;

        [Required(ErrorMessage = "建立時間為必填")]
        [Display(Name = "建立時間")]
        public DateTime CreatedAt { get; set; }

        [Required(ErrorMessage = "更新時間為必填")]
        [Display(Name = "更新時間")]
        public DateTime UpdatedAt { get; set; }

        [Display(Name = "是否已刪除")]
        public bool IsDeleted { get; set; }

        [Display(Name = "預算")]
        public virtual ICollection<Budget> Budgets { get; set; } = new List<Budget>();

        [Display(Name = "交易")]
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

        [Display(Name = "使用者")]
        public virtual User User { get; set; } = null!;
    }
}