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
        [Display(Name = "����ID")]
        public Guid CategoryId { get; set; }

        [Required]
        [Display(Name = "�Τ�ID")]
        public Guid UserId { get; set; }

        [Required]
        [Display(Name = "�����W��")]
        [StringLength(50)]
        public string CategoryName { get; set; } = null!;

        [Required]
        [Display(Name = "��������")]
        public string CategoryType { get; set; } = null!;

        [Display(Name = "�إ߮ɶ�")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "��s�ɶ�")]
        public DateTime UpdatedAt { get; set; }

        [Display(Name = "�w�R��")]
        public bool IsDeleted { get; set; }

        [Display(Name = "�w��")]
        public virtual ICollection<Budget> Budgets { get; set; } = new List<Budget>();

        [Display(Name = "���")]
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

        [Display(Name = "�Τ�")]
        public virtual User User { get; set; } = null!;
    }
}
