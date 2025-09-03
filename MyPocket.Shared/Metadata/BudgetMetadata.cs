using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using MyPocket.Core.Models;

namespace MyPocket.Shared.Metadata
{
    [ModelMetadataType(typeof(BudgetMetadata))]
    public partial class Budget { }

    public class BudgetMetadata
    {
        [Required]
        [Display(Name = "預算ID")]
        public Guid BudgetId { get; set; }

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
        [Display(Name = "年度")]
        public string BudgetYear { get; set; } = null!;

        [Required]
        [Display(Name = "月份")]
        public string BudgetMonth { get; set; } = null!;

        [Display(Name = "建立時間")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "更新時間")]
        public DateTime UpdatedAt { get; set; }

        [Display(Name = "已刪除")]
        public bool IsDeleted { get; set; }

        [Display(Name = "分類")]
        public virtual Category Category { get; set; } = null!;

        [Display(Name = "用戶")]
        public virtual User User { get; set; } = null!;
    }
}
