using System.ComponentModel.DataAnnotations;

namespace MyPocket.Shared.ViewModels.Users
{
    public class UserProfileViewModel
    {
        public Guid UserId { get; set; }
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
        [Display(Name = "�ʺ�")]
        public string? Nickname { get; set; }
        [Display(Name = "���U���")]
        public DateTime CreationDate { get; set; }
        [Display(Name = "�̫�n�J")]
        public DateTime LastLoginDate { get; set; }
    }
}
