using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using MyPocket.Core.Models;

namespace MyPocket.Shared.Metadata
{
    [ModelMetadataType(typeof(AnnouncementMetadata))]
    public partial class Announcement { }

    public class AnnouncementMetadata
    {
        [Required(ErrorMessage = "���iID������")]
        [Display(Name = "���iID")]
        public Guid AnnouncementId { get; set; }

        [Required(ErrorMessage = "�޲z��ID������")]
        [Display(Name = "�޲z��ID")]
        public Guid AdminId { get; set; }

        [Required(ErrorMessage = "���D������")]
        [Display(Name = "���D")]
        [StringLength(200, ErrorMessage = "{0}���פ���W�L{1}�Ӧr��")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "���e������")]
        [Display(Name = "���e")]
        public string Content { get; set; } = null!;

        [Required(ErrorMessage = "�o�����������")]
        [Display(Name = "�o�����")]
        [DataType(DataType.DateTime)]
        public DateTime PublishDate { get; set; }

        [Display(Name = "�޲z��")]
        public virtual User Admin { get; set; } = null!;
    }
}
