using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPocket.Core.Models;

/// <summary>A shop item (decoration, character or outfit) a user has bought.</summary>
[Table("UserItem")]
public class UserItem
{
    [Key]
    public Guid UserItemId { get; set; }

    public Guid UserId { get; set; }

    /// <summary>Catalog code, e.g. "tree" or "cat". Matches ShopCatalog.</summary>
    [MaxLength(40)]
    public string ItemCode { get; set; } = null!;

    /// <summary>Only meaningful for outfits: at most one outfit is worn at a time.</summary>
    public bool IsEquipped { get; set; }

    public DateTime PurchasedAt { get; set; }
}
