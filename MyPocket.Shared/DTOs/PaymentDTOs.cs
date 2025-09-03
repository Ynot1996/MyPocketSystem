using System.ComponentModel.DataAnnotations;

namespace MyPocket.Shared.DTOs
{
    public class PaymentDTO
    {
        public Guid PaymentId { get; set; }
        public Guid SubscriptionId { get; set; }
        public decimal PaymentAmount { get; set; }
        public string PaymentWay { get; set; } = null!;
        public DateTime PaymentDate { get; set; }
        public string Status { get; set; } = null!;
        public string TransactionCode { get; set; } = null!;
    }

    public class CreatePaymentDTO
    {
        [Required]
        public Guid SubscriptionId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "付款金額必須大於0")]
        public decimal PaymentAmount { get; set; }

        [Required]
        public string PaymentWay { get; set; } = null!;

        public DateTime PaymentDate { get; set; }

        [Required]
        public string Status { get; set; } = null!;

        [Required]
        public string TransactionCode { get; set; } = null!;
    }

    public class UpdatePaymentDTO
    {
        public string Status { get; set; } = null!;
    }
}