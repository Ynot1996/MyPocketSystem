using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace MyPocket.Core.Models.Metadata
{
    [ModelMetadataType(typeof(SavingGoalMetadata))]
    public partial class SavingGoal
    {
    }

    public class SavingGoalMetadata
    {       
        [Required(ErrorMessage = "目標ID為必填")]
        [Display(Name = "目標ID")]
        public Guid GoalId { get; set; }

        [Required(ErrorMessage = "使用者ID為必填")]
        [Display(Name = "使用者ID")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "目標名稱為必填")]
        [Display(Name = "目標名稱")]
        [StringLength(100, ErrorMessage = "{0}長度不能超過{1}個字元")]
        public string GoalName { get; set; } = null!;

        [Required(ErrorMessage = "目標金額為必填")]
        [Display(Name = "目標金額")]
        [Range(0, 999999999999.99, ErrorMessage = "目標金額必須介於{1}到{2}之間")]
        public decimal TargetAmount { get; set; }

        [Required(ErrorMessage = "當前金額為必填")]
        [Display(Name = "當前金額")]
        [Range(0, 999999999999.99, ErrorMessage = "當前金額必須介於{1}到{2}之間")]
        public decimal CurrentAmount { get; set; }

        [Required(ErrorMessage = "目標日期為必填")]
        [Display(Name = "目標日期")]
        [DataType(DataType.Date)]
        public DateTime TargetDate { get; set; }

        [Required(ErrorMessage = "建立時間為必填")]
        [Display(Name = "建立時間")]
        public DateTime CreatedAt { get; set; }

        [Required(ErrorMessage = "更新時間為必填")]
        [Display(Name = "更新時間")]
        public DateTime UpdatedAt { get; set; }

        [Display(Name = "是否已刪除")]
        public bool IsDeleted { get; set; }
   
        [Display(Name = "使用者")]
        public virtual User User { get; set; } = null!;
    }
}