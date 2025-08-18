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
        [Required(ErrorMessage = "����ID������")]
        [Display(Name = "����ID")]
        public Guid CategoryId { get; set; }

        [Required(ErrorMessage = "�ϥΪ�ID������")]
        [Display(Name = "�ϥΪ�ID")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "�����W�٬�����")]
        [Display(Name = "�����W��")]
        [StringLength(50, ErrorMessage = "{0}���פ���W�L{1}�Ӧr��")]
        public string CategoryName { get; set; } = null!;

        [Required(ErrorMessage = "��������������")]
        [Display(Name = "��������")]
        [StringLength(50, ErrorMessage = "{0}���פ���W�L{1}�Ӧr��")]
        public string CategoryType { get; set; } = null!;

        [Required(ErrorMessage = "�إ߮ɶ�������")]
        [Display(Name = "�إ߮ɶ�")]
        public DateTime CreatedAt { get; set; }

        [Required(ErrorMessage = "��s�ɶ�������")]
        [Display(Name = "��s�ɶ�")]
        public DateTime UpdatedAt { get; set; }

        [Display(Name = "�O�_�w�R��")]
        public bool IsDeleted { get; set; }

        [Display(Name = "�w��")]
        public virtual ICollection<Budget> Budgets { get; set; } = new List<Budget>();

        [Display(Name = "���")]
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

        [Display(Name = "�ϥΪ�")]
        public virtual User User { get; set; } = null!;
    }
}