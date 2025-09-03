using System.ComponentModel.DataAnnotations;

namespace MyPocket.Shared.DTOs
{
    public class SubscriptionPlanDTO
    {
        public Guid PlanId { get; set; }
        public string PlanName { get; set; } = null!;
        public decimal Price { get; set; }
    }

    public class CreateSubscriptionPlanDTO
    {
        [Required]
        [MaxLength(50)]
        public string PlanName { get; set; } = null!;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "價格必須大於0")]
        public decimal Price { get; set; }
    }

    public class UpdateSubscriptionPlanDTO
    {
        [Required]
        [MaxLength(50)]
        public string PlanName { get; set; } = null!;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "價格必須大於0")]
        public decimal Price { get; set; }
    }

    public class UserSubscriptionDTO
    {
        public Guid SubscriptionId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = null!;
        public Guid PlanId { get; set; }
        public string PlanName { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = null!;
    }

    public class CreateUserSubscriptionDTO
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid PlanId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public string Status { get; set; } = null!;
    }

    public class UpdateUserSubscriptionDTO
    {
        public DateTime? EndDate { get; set; }
        public string Status { get; set; } = null!;
    }
}