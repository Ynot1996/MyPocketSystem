using System.ComponentModel.DataAnnotations;

namespace MyPocket.Shared.DTOs
{
    public class BudgetDTO
    {
        public Guid BudgetId { get; set; }
        public Guid UserId { get; set; }
        public Guid CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public decimal Amount { get; set; }
        public string BudgetYear { get; set; } = null!;
        public string BudgetMonth { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateBudgetDTO
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid CategoryId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Budget amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "Year must be a 4-digit number")]
        public string BudgetYear { get; set; } = null!;

        [Required]
        [RegularExpression(@"^(0[1-9]|1[0-2])$", ErrorMessage = "Month must be between 01 and 12")]
        public string BudgetMonth { get; set; } = null!;
    }

    public class UpdateBudgetDTO
    {
        public Guid CategoryId { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Budget amount must be greater than 0")]
        public decimal Amount { get; set; }

        [RegularExpression(@"^\d{4}$", ErrorMessage = "Year must be a 4-digit number")]
        public string BudgetYear { get; set; } = null!;

        [RegularExpression(@"^(0[1-9]|1[0-2])$", ErrorMessage = "Month must be between 01 and 12")]
        public string BudgetMonth { get; set; } = null!;
    }
}