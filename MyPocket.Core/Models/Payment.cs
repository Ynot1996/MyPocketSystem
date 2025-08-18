using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPocket.Core.Models
{
    public partial class Payment
    {
        [Key]
        public Guid PaymentId { get; set; }
        public Guid SubscriptionId { get; set; }
        public decimal PaymentAmount { get; set; }
        public string PaymentWay { get; set; } = null!;
        public DateTime PaymentDate { get; set; }
        public string Status { get; set; } = null!;
        public string TransactionCode { get; set; } = null!;
        [ForeignKey("SubscriptionId")]
        [InverseProperty("Payments")]
        public virtual UserSubscription Subscription { get; set; } = null!;
    }
}
