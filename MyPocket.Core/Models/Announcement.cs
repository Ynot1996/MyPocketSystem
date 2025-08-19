using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPocket.Core.Models
{
    [Table("Announcement")]
    public partial class Announcement
    {
        [Key]
        public Guid AnnouncementId { get; set; }

        public Guid AdminId { get; set; }

        public string Title { get; set; } = null!;

        public string Content { get; set; } = null!;

        public DateTime PublishDate { get; set; }

        [ForeignKey("AdminId")]
        [InverseProperty("Announcements")]
        public virtual User Admin { get; set; } = null!;
    }
}
