using System.ComponentModel.DataAnnotations;

namespace MyPocket.Shared.DTOs
{
    public class UserDTO
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = null!;
        public string? Nickname { get; set; }
        public string Role { get; set; } = null!;
        public DateTime CreationDate { get; set; }
        public DateTime LastLoginDate { get; set; }
    }

    public class CreateUserDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
        [Required]
        [MinLength(8)]
        public string Password { get; set; } = null!;

        public string? Nickname { get; set; }
    }

    public class UpdateUserDTO
    {
        [EmailAddress]
        public string Email { get; set; } = null!;
        public string? Nickname { get; set; }
        public string Role { get; set; } = null!;
    }
}