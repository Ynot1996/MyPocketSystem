using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPocket.Core.Models;

/// <summary>
/// Append-only record of every coin earned or spent. The balance is the sum of
/// a user's entries, which keeps a full audit trail and avoids a mutable
/// balance column drifting out of sync with history.
/// </summary>
[Table("CoinLedger")]
public class CoinLedgerEntry
{
    [Key]
    public Guid EntryId { get; set; }

    public Guid UserId { get; set; }

    /// <summary>Positive for earnings, negative for purchases.</summary>
    public int Amount { get; set; }

    /// <summary>Machine-readable reason, e.g. "TransactionLogged" or "ShopPurchase:tree".</summary>
    [MaxLength(60)]
    public string Reason { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
