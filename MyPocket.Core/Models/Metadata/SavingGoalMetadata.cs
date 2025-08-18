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
        [Required(ErrorMessage = "�ؼ�ID������")]
        [Display(Name = "�ؼ�ID")]
        public Guid GoalId { get; set; }

        [Required(ErrorMessage = "�ϥΪ�ID������")]
        [Display(Name = "�ϥΪ�ID")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "�ؼЦW�٬�����")]
        [Display(Name = "�ؼЦW��")]
        [StringLength(100, ErrorMessage = "{0}���פ���W�L{1}�Ӧr��")]
        public string GoalName { get; set; } = null!;

        [Required(ErrorMessage = "�ؼЪ��B������")]
        [Display(Name = "�ؼЪ��B")]
        [Range(0, 999999999999.99, ErrorMessage = "�ؼЪ��B��������{1}��{2}����")]
        public decimal TargetAmount { get; set; }

        [Required(ErrorMessage = "��e���B������")]
        [Display(Name = "��e���B")]
        [Range(0, 999999999999.99, ErrorMessage = "��e���B��������{1}��{2}����")]
        public decimal CurrentAmount { get; set; }

        [Required(ErrorMessage = "�ؼФ��������")]
        [Display(Name = "�ؼФ��")]
        [DataType(DataType.Date)]
        public DateTime TargetDate { get; set; }

        [Required(ErrorMessage = "�إ߮ɶ�������")]
        [Display(Name = "�إ߮ɶ�")]
        public DateTime CreatedAt { get; set; }

        [Required(ErrorMessage = "��s�ɶ�������")]
        [Display(Name = "��s�ɶ�")]
        public DateTime UpdatedAt { get; set; }

        [Display(Name = "�O�_�w�R��")]
        public bool IsDeleted { get; set; }
   
        [Display(Name = "�ϥΪ�")]
        public virtual User User { get; set; } = null!;
    }
}