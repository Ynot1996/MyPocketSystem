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
        [Required(ErrorMessage = "使用者ID為必填")]
        [Display(Name = "使用者ID")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "電子郵件為必填")]
        [EmailAddress(ErrorMessage = "請輸入有效的電子郵件格式")]
        [Display(Name = "電子郵件")]
        [StringLength(250, ErrorMessage = "{0}長度不能超過{1}個字元")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "密碼為必填")]
        [Display(Name = "密碼")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "密碼長度必須介於{2}到{1}個字元之間")]
        public string PasswordHash { get; set; } = null!;

        [Display(Name = "暱稱")]
        [StringLength(50, ErrorMessage = "{0}長度不能超過{1}個字元")]
        public string? Nickname { get; set; }

        [Required(ErrorMessage = "角色為必填")]
        [Display(Name = "角色")]
        [StringLength(20, ErrorMessage = "{0}長度不能超過{1}個字元")]
        public string Role { get; set; } = null!;

        [Required(ErrorMessage = "建立日期為必填")]
        [Display(Name = "建立日期")]
        [DataType(DataType.DateTime)]
        public DateTime CreationDate { get; set; }

        [Required(ErrorMessage = "最後登入日期為必填")]
        [Display(Name = "最後登入日期")]
        [DataType(DataType.DateTime)]
        public DateTime LastLoginDate { get; set; }

        [Required(ErrorMessage = "更新日期為必填")]
        [Display(Name = "更新日期")]
        [DataType(DataType.DateTime)]
        public DateTime UpdatedAt { get; set; }

        [Display(Name = "是否已刪除")]
        public bool IsDeleted { get; set; }

        [Display(Name = "公告")]
        public virtual ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();

        [Display(Name = "預算")]
        public virtual ICollection<Budget> Budgets { get; set; } = new List<Budget>();

        [Display(Name = "分類")]
        public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

        [Display(Name = "儲蓄目標")]
        public virtual ICollection<SavingGoal> SavingGoals { get; set; } = new List<SavingGoal>();

        [Display(Name = "交易")]
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

        [Display(Name = "訂閱")]
        public virtual ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();
    }
}