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
        [Display(Name = "User ID")]
        public Guid UserId { get; set; }

        [Required]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [Display(Name = "Password Hash")]
        public string PasswordHash { get; set; } = null!;

        [Display(Name = "Nickname")]
        public string? Nickname { get; set; }

        [Required]
        [Display(Name = "Role")]
        public string Role { get; set; } = null!;

        [Display(Name = "Created Date")]
        public DateTime CreationDate { get; set; }

        [Display(Name = "Last Login")]
        public DateTime LastLoginDate { get; set; }

        [Display(Name = "Updated At")]
        public DateTime UpdatedAt { get; set; }

        [Display(Name = "Deleted")]
        public bool IsDeleted { get; set; }

        [Display(Name = "Announcements")]
        public virtual ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();

        [Display(Name = "Budgets")]
        public virtual ICollection<Budget> Budgets { get; set; } = new List<Budget>();

        [Display(Name = "Categories")]
        public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

        [Display(Name = "Saving Goals")]
        public virtual ICollection<SavingGoal> SavingGoals { get; set; } = new List<SavingGoal>();

        [Display(Name = "Transactions")]
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

        [Display(Name = "Subscriptions")]
        public virtual ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();
    }
}
