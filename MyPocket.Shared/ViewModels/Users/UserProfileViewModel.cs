using System.ComponentModel.DataAnnotations;

namespace MyPocket.Shared.ViewModels.Users
{
    public class UserProfileViewModel
    {
        public Guid UserId { get; set; }
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
        [Display(Name = "暱稱")]
        public string? Nickname { get; set; }
        [Display(Name = "註冊日期")]
        public DateTime CreationDate { get; set; }
        [Display(Name = "最後登入")]
        public DateTime LastLoginDate { get; set; }
    }
}
