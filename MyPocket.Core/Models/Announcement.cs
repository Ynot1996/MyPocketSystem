using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MyPocket.Core.Models;

[Table("Announcement")]
public partial class Announcement
{
    [Key]
    [Column("AnnouncementID")]
    public Guid AnnouncementId { get; set; }

    [Column("AdminID")]
    public Guid AdminId { get; set; }

    [StringLength(200)]
    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime PublishDate { get; set; }

    [ForeignKey("AdminId")]
    [InverseProperty("Announcements")]
    public virtual User Admin { get; set; } = null!;
}
