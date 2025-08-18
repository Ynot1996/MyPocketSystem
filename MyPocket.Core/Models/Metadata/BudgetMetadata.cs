using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace MyPocket.Core.Models.Metadata
{
    [ModelMetadataType(typeof(BudgetMetadata))]
    public partial class Budget
    {
    }

    public class BudgetMetadata
    {
        [Required(ErrorMessage = "預算ID為必填")]
        [Display(Name = "預算ID")]
        public Guid BudgetId { get; set; }

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

        [Required(ErrorMessage = "年度為必填")]
        [Display(Name = "年度")]
        [StringLength(4, ErrorMessage = "{0}長度必須為4個字元", MinimumLength = 4)]
        public string BudgetYear { get; set; } = null!;

        [Required(ErrorMessage = "月份為必填")]
        [Display(Name = "月份")]
        [StringLength(2, ErrorMessage = "{0}長度必須為2個字元", MinimumLength = 2)]
        public string BudgetMonth { get; set; } = null!;

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