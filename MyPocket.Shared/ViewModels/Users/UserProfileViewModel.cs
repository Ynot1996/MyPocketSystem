using System.ComponentModel.DataAnnotations;

namespace MyPocket.Shared.ViewModels.Users
{
    public class UserProfileViewModel
    {
        public Guid UserId { get; set; }
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
        [Display(Name = "Nickname")]
        public string? Nickname { get; set; }
        [Display(Name = "Registration Date")]
        public DateTime CreationDate { get; set; }
        [Display(Name = "Last Login")]
        public DateTime LastLoginDate { get; set; }
    }
}
