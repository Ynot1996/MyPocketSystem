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
        [Display(Name = "�w��ID")]
        public Guid BudgetId { get; set; }

        [Required]
        [Display(Name = "�Τ�ID")]
        public Guid UserId { get; set; }

        [Required]
        [Display(Name = "����ID")]
        public Guid CategoryId { get; set; }

        [Required]
        [Display(Name = "���B")]
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "�~��")]
        public string BudgetYear { get; set; } = null!;

        [Required]
        [Display(Name = "���")]
        public string BudgetMonth { get; set; } = null!;

        [Display(Name = "�إ߮ɶ�")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "��s�ɶ�")]
        public DateTime UpdatedAt { get; set; }

        [Display(Name = "�w�R��")]
        public bool IsDeleted { get; set; }

        [Display(Name = "����")]
        public virtual Category Category { get; set; } = null!;

        [Display(Name = "�Τ�")]
        public virtual User User { get; set; } = null!;
    }
}
