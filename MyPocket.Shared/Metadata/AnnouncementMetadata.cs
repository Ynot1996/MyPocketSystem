using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using MyPocket.Core.Models;

namespace MyPocket.Shared.Metadata
{
    [ModelMetadataType(typeof(AnnouncementMetadata))]
    public partial class Announcement { }

    public class AnnouncementMetadata
    {
        [Required(ErrorMessage = "公告ID為必填")]
        [Display(Name = "公告ID")]
        public Guid AnnouncementId { get; set; }

        [Required(ErrorMessage = "管理員ID為必填")]
        [Display(Name = "管理員ID")]
        public Guid AdminId { get; set; }

        [Required(ErrorMessage = "標題為必填")]
        [Display(Name = "標題")]
        [StringLength(200, ErrorMessage = "{0}長度不能超過{1}個字元")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "內容為必填")]
        [Display(Name = "內容")]
        public string Content { get; set; } = null!;

        [Required(ErrorMessage = "發布日期為必填")]
        [Display(Name = "發布日期")]
        [DataType(DataType.DateTime)]
        public DateTime PublishDate { get; set; }

        [Display(Name = "管理員")]
        public virtual User Admin { get; set; } = null!;
    }
}
