using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using MyPocket.Core.Models;

namespace MyPocket.Shared.Metadata
{
    [ModelMetadataType(typeof(CategoryMetadata))]
    public partial class Category { }

    public class CategoryMetadata
    {
        [Required]
        [Display(Name = "分類ID")]
        public Guid CategoryId { get; set; }

        [Required]
        [Display(Name = "用戶ID")]
        public Guid UserId { get; set; }

        [Required]
        [Display(Name = "分類名稱")]
        [StringLength(50)]
        public string CategoryName { get; set; } = null!;

        [Required]
        [Display(Name = "分類類型")]
        public string CategoryType { get; set; } = null!;

        [Display(Name = "建立時間")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "更新時間")]
        public DateTime UpdatedAt { get; set; }

        [Display(Name = "已刪除")]
        public bool IsDeleted { get; set; }

        [Display(Name = "預算")]
        public virtual ICollection<Budget> Budgets { get; set; } = new List<Budget>();

        [Display(Name = "交易")]
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

        [Display(Name = "用戶")]
        public virtual User User { get; set; } = null!;
    }
}