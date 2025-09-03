using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using MyPocket.Core.Models;

namespace MyPocket.Shared.Metadata
{
    [ModelMetadataType(typeof(SavingGoalMetadata))]
    public partial class SavingGoal { }

    public class SavingGoalMetadata
    {
        [Required]
        [Display(Name = "目標ID")]
        public Guid GoalId { get; set; }

        [Required]
        [Display(Name = "用戶ID")]
        public Guid UserId { get; set; }

        [Required]
        [Display(Name = "目標名稱")]
        [StringLength(100)]
        public string GoalName { get; set; } = null!;

        [Required]
        [Display(Name = "目標日期")]
        [DataType(DataType.DateTime)]
        public DateTime TargetDate { get; set; }

        [Display(Name = "建立時間")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "更新時間")]
        public DateTime UpdatedAt { get; set; }

        [Display(Name = "已刪除")]
        public bool IsDeleted { get; set; }

        [Display(Name = "用戶")]
        public virtual User User { get; set; } = null!;
    }
}
