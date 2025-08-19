using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace MyPocket.Core.Models.Metadata
{
    [ModelMetadataType(typeof(UserMetadata))]
    public partial class User
    {
    }

    public class UserMetadata
    {
        [Required(ErrorMessage = "�ϥΪ�ID������")]
        [Display(Name = "�ϥΪ�ID")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "�q�l�l�󬰥���")]
        [EmailAddress(ErrorMessage = "�п�J���Ī��q�l�l��榡")]
        [Display(Name = "�q�l�l��")]
        [StringLength(250, ErrorMessage = "{0}���פ���W�L{1}�Ӧr��")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "�K�X������")]
        [Display(Name = "�K�X")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "�K�X���ץ�������{2}��{1}�Ӧr������")]
        public string PasswordHash { get; set; } = null!;

        [Display(Name = "�ʺ�")]
        [StringLength(50, ErrorMessage = "{0}���פ���W�L{1}�Ӧr��")]
        public string? Nickname { get; set; }

        [Required(ErrorMessage = "���⬰����")]
        [Display(Name = "����")]
        [StringLength(20, ErrorMessage = "{0}���פ���W�L{1}�Ӧr��")]
        public string Role { get; set; } = null!;

        [Required(ErrorMessage = "�إߤ��������")]
        [Display(Name = "�إߤ��")]
        [DataType(DataType.DateTime)]
        public DateTime CreationDate { get; set; }

        [Required(ErrorMessage = "�̫�n�J���������")]
        [Display(Name = "�̫�n�J���")]
        [DataType(DataType.DateTime)]
        public DateTime LastLoginDate { get; set; }

        [Required(ErrorMessage = "��s���������")]
        [Display(Name = "��s���")]
        [DataType(DataType.DateTime)]
        public DateTime UpdatedAt { get; set; }

        [Display(Name = "�O�_�w�R��")]
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