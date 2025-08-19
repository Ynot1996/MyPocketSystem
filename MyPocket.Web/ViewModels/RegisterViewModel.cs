using System.ComponentModel.DataAnnotations;

namespace MyPocket.Web.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "�K�X���צܤ�6�X")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "�K�X�P�T�{�K�X���@�P")]
        public string ConfirmPassword { get; set; }

        public string? Nickname { get; set; }
    }
}