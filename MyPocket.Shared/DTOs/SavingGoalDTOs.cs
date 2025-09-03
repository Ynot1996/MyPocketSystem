using System.ComponentModel.DataAnnotations;

namespace MyPocket.Shared.DTOs
{
    public class SavingGoalDTO
    {
        public Guid GoalId { get; set; }
        public Guid UserId { get; set; }
        public string GoalName { get; set; } = null!;
        public decimal TargetAmount { get; set; }
        public decimal CurrentAmount { get; set; }
        public DateTime TargetDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateSavingGoalDTO
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string GoalName { get; set; } = null!;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "目標金額必須大於0")]
        public decimal TargetAmount { get; set; }

        [Required]
        public DateTime TargetDate { get; set; }

        public decimal CurrentAmount { get; set; }
    }

    public class UpdateSavingGoalDTO
    {
        [Required]
        [MaxLength(100)]
        public string GoalName { get; set; } = null!;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "目標金額必須大於0")]
        public decimal TargetAmount { get; set; }

        [Required]
        public DateTime TargetDate { get; set; }

        public decimal CurrentAmount { get; set; }
    }
}