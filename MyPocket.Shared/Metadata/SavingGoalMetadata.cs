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
        [Display(Name = "�ؼ�ID")]
        public Guid GoalId { get; set; }

        [Required]
        [Display(Name = "�Τ�ID")]
        public Guid UserId { get; set; }

        [Required]
        [Display(Name = "�ؼЦW��")]
        [StringLength(100)]
        public string GoalName { get; set; } = null!;

        [Required]
        [Display(Name = "�ؼФ��")]
        [DataType(DataType.DateTime)]
        public DateTime TargetDate { get; set; }

        [Display(Name = "�إ߮ɶ�")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "��s�ɶ�")]
        public DateTime UpdatedAt { get; set; }

        [Display(Name = "�w�R��")]
        public bool IsDeleted { get; set; }

        [Display(Name = "�Τ�")]
        public virtual User User { get; set; } = null!;
    }
}
