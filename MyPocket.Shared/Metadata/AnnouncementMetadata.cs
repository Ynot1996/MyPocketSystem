using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using MyPocket.Core.Models;

namespace MyPocket.Shared.Metadata
{
    [ModelMetadataType(typeof(AnnouncementMetadata))]
    public partial class Announcement { }

    public class AnnouncementMetadata
    {
        [Required(ErrorMessage = "Announcement ID is required.")]
        [Display(Name = "Announcement ID")]
        public Guid AnnouncementId { get; set; }

        [Required(ErrorMessage = "Admin ID is required.")]
        [Display(Name = "Admin ID")]
        public Guid AdminId { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [Display(Name = "Title")]
        [StringLength(200, ErrorMessage = "{0} cannot exceed {1} characters.")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Content is required.")]
        [Display(Name = "Content")]
        public string Content { get; set; } = null!;

        [Required(ErrorMessage = "Publish date is required.")]
        [Display(Name = "Publish Date")]
        [DataType(DataType.DateTime)]
        public DateTime PublishDate { get; set; }

        [Display(Name = "Admin")]
        public virtual User Admin { get; set; } = null!;
    }
}
