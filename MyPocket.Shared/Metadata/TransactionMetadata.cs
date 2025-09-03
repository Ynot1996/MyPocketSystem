using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using MyPocket.Core.Models;

namespace MyPocket.Shared.Metadata
{
    [ModelMetadataType(typeof(TransactionMetadata))]
    public partial class Transaction { }

    public class TransactionMetadata
    {
        [Required]
        [Display(Name = "���ID")]
        public Guid TransactionId { get; set; }

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
        [Display(Name = "�������")]
        public string TransactionType { get; set; } = null!;

        [Required]
        [Display(Name = "������")]
        [DataType(DataType.DateTime)]
        public DateTime TransactionDate { get; set; }

        [Display(Name = "�y�z")]
        public string? Description { get; set; }

        [Display(Name = "�إ߮ɶ�")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "��s�ɶ�")]
        public DateTime UpdatedAt { get; set; }

        [Display(Name = "����")]
        public virtual Category Category { get; set; } = null!;

        [Display(Name = "�Τ�")]
        public virtual User User { get; set; } = null!;
    }
}
