using System.ComponentModel.DataAnnotations;

namespace MyPocket.Shared.DTOs
{
    public class CategoryDTO
    {
        public Guid CategoryId { get; set; }
        public Guid UserId { get; set; }
        public string CategoryName { get; set; } = null!;
        public string CategoryType { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateCategoryDTO
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string CategoryName { get; set; } = null!;

        [Required]
        public string CategoryType { get; set; } = null!;
    }

    public class UpdateCategoryDTO
    {
        [Required]
        [MaxLength(50)]
        public string CategoryName { get; set; } = null!;

        [Required]
        public string CategoryType { get; set; } = null!;
    }
}