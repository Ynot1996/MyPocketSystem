using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace MyPocket.Core.Models.Metadata
{
    [ModelMetadataType(typeof(TransactionMetadata))]
    public partial class Transaction
    {
    }

    public class TransactionMetadata
    {
        [Required(ErrorMessage = "���ID������")]
        [Display(Name = "���ID")]
        public Guid TransactionId { get; set; }

        [Required(ErrorMessage = "�ϥΪ�ID������")]
        [Display(Name = "�ϥΪ�ID")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "����ID������")]
        [Display(Name = "����ID")]
        public Guid CategoryId { get; set; }

        [Required(ErrorMessage = "���B������")]
        [Display(Name = "���B")]
        [Range(0, 999999999999.99, ErrorMessage = "���B��������{1}��{2}����")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "�������������")]
        [Display(Name = "�������")]
        [StringLength(5, ErrorMessage = "{0}���פ���W�L{1}�Ӧr��")]
        public string TransactionType { get; set; } = null!;

        [Required(ErrorMessage = "������������")]
        [Display(Name = "������")]
        [DataType(DataType.DateTime)]
        public DateTime TransactionDate { get; set; }

        [Display(Name = "�y�z")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "�إ߮ɶ�������")]
        [Display(Name = "�إ߮ɶ�")]
        public DateTime CreatedAt { get; set; }

        [Required(ErrorMessage = "��s�ɶ�������")]
        [Display(Name = "��s�ɶ�")]
        public DateTime UpdatedAt { get; set; }

        [Display(Name = "�O�_�w�R��")]
        public bool IsDeleted { get; set; }
        
        [Display(Name = "����")]
        public virtual Category Category { get; set; } = null!;

        [Display(Name = "�ϥΪ�")]
        public virtual User User { get; set; } = null!;
    }
}