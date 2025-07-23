using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MyPocket.Core.Models;

[Table("Payment")]
public partial class Payment
{
    [Key]
    [Column("PaymentID")]
    public Guid PaymentId { get; set; }

    [Column("SubscriptionID")]
    public Guid SubscriptionId { get; set; }

    [Column(TypeName = "decimal(12, 2)")]
    public decimal PaymentAmount { get; set; }

    [StringLength(15)]
    public string PaymentWay { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime PaymentDate { get; set; }

    [StringLength(15)]
    public string Status { get; set; } = null!;

    [StringLength(100)]
    public string TransactionCode { get; set; } = null!;

    [ForeignKey("SubscriptionId")]
    [InverseProperty("Payments")]
    public virtual UserSubscription Subscription { get; set; } = null!;
}
