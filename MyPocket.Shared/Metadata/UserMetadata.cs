using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using MyPocket.Core.Models;

namespace MyPocket.Shared.Metadata
{
    [ModelMetadataType(typeof(UserMetadata))]
    public partial class User { }

    public class UserMetadata
    {
        [Required]
        [Display(Name = "�Τ�ID")]
        public Guid UserId { get; set; }

        [Required]
        [Display(Name = "�q�l�l��")]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [Display(Name = "�K�X����")]
        public string PasswordHash { get; set; } = null!;

        [Display(Name = "�ʺ�")]
        public string? Nickname { get; set; }

        [Required]
        [Display(Name = "����")]
        public string Role { get; set; } = null!;

        [Display(Name = "�إߤ��")]
        public DateTime CreationDate { get; set; }

        [Display(Name = "�̫�n�J���")]
        public DateTime LastLoginDate { get; set; }

        [Display(Name = "��s�ɶ�")]
        public DateTime UpdatedAt { get; set; }

        [Display(Name = "�w�R��")]
        public bool IsDeleted { get; set; }

        [Display(Name = "���i")]
        public virtual ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();

        [Display(Name = "�w��")]
        public virtual ICollection<Budget> Budgets { get; set; } = new List<Budget>();

        [Display(Name = "����")]
        public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

        [Display(Name = "�x�W�ؼ�")]
        public virtual ICollection<SavingGoal> SavingGoals { get; set; } = new List<SavingGoal>();

        [Display(Name = "���")]
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

        [Display(Name = "�q�\")]
        public virtual ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();
    }
}
