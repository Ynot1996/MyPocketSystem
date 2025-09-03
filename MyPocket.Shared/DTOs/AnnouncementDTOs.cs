using System.ComponentModel.DataAnnotations;

namespace MyPocket.Shared.DTOs
{
    public class AnnouncementDTO
    {
        public Guid AnnouncementId { get; set; }
        public Guid AdminId { get; set; }
        public string AdminName { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public DateTime PublishDate { get; set; }
    }

    public class CreateAnnouncementDTO
    {
        [Required]
        public Guid AdminId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = null!;

        [Required]
        public string Content { get; set; } = null!;
    }

    public class UpdateAnnouncementDTO
    {
        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = null!;

        [Required]
        public string Content { get; set; } = null!;
    }
}