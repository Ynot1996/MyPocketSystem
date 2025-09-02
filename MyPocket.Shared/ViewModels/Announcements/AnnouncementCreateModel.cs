using MyPocket.Shared.Resources;
using MyPocket.Shared.Validation;
using System.ComponentModel.DataAnnotations;

namespace MyPocket.Shared.ViewModels.Announcements
{
    public class AnnouncementCreateModel
    {
        [LocalizedRequired("TitleRequired")]
        [LocalizedStringLength(100, "TitleLength")]
        public string Title { get; set; } = null!;

        [LocalizedRequired("ContentRequired")]
        public string Content { get; set; } = null!;
    }
}