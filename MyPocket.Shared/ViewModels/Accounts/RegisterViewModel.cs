using System.ComponentModel.DataAnnotations;

namespace MyPocket.Shared.ViewModels.Accounts
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!; 

        [Required]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "�K�X���צܤ�6�X")]
        public string Password { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = null!;

        public string? Nickname { get; set; }
    }
}