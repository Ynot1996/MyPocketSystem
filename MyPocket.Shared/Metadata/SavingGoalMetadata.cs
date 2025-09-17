using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using MyPocket.Core.Models;

namespace MyPocket.Shared.Metadata
{
    [ModelMetadataType(typeof(SavingGoalMetadata))]
    public partial class SavingGoal { }

    public class SavingGoalMetadata
    {
        [Required]
        [Display(Name = "Goal ID")]
        public Guid GoalId { get; set; }

        [Required]
        [Display(Name = "User ID")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Please enter a goal name.")]
        [Display(Name = "Goal Name")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string GoalName { get; set; } = null!;

        [Required(ErrorMessage = "Please enter a target amount.")]
        [Display(Name = "Target Amount")]
        [Range(1, double.MaxValue, ErrorMessage = "Target amount must be greater than 0.")]
        public decimal TargetAmount { get; set; }

        [Required(ErrorMessage = "Please enter a current amount.")]
        [Display(Name = "Current Amount")]
        [Range(0, double.MaxValue, ErrorMessage = "Current amount cannot be negative.")]
        public decimal CurrentAmount { get; set; }

        [Required(ErrorMessage = "Please select a target date.")]
        [Display(Name = "Target Date")]
        [DataType(DataType.Date)]
        [FutureDate(ErrorMessage = "Target date must be in the future.")]
        public DateTime TargetDate { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Updated At")]
        public DateTime UpdatedAt { get; set; }

        [Display(Name = "Is Deleted")]
        public bool IsDeleted { get; set; }

        [Display(Name = "User")]
        public virtual User User { get; set; } = null!;
    }

    // Custom validation: Target date must be in the future
    public class FutureDateAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is DateTime date)
            {
                return date.Date > DateTime.Now.Date;
            }
            return true;
        }
    }
}
