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
        [Display(Name = "用戶ID")]
        public Guid UserId { get; set; }

        [Required]
        [Display(Name = "電子郵件")]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [Display(Name = "密碼雜湊")]
        public string PasswordHash { get; set; } = null!;

        [Display(Name = "暱稱")]
        public string? Nickname { get; set; }

        [Required]
        [Display(Name = "角色")]
        public string Role { get; set; } = null!;

        [Display(Name = "建立日期")]
        public DateTime CreationDate { get; set; }

        [Display(Name = "最後登入日期")]
        public DateTime LastLoginDate { get; set; }

        [Display(Name = "更新時間")]
        public DateTime UpdatedAt { get; set; }

        [Display(Name = "已刪除")]
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
