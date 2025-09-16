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
        [MinLength(6, ErrorMessage = "密碼長度至少6碼")]
        public string Password { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = null!;

        public string? Nickname { get; set; }
    }
}