using System.ComponentModel.DataAnnotations;
using MyPocket.Shared.Resources;

namespace MyPocket.Shared.ViewModels.Transactions
{
    public class LocalizedValidationAttribute : ValidationAttribute
    {
        private readonly string _resourceKey;
        private readonly ILocalizationService _localizationService;

        public LocalizedValidationAttribute(string resourceKey)
        {
            _resourceKey = resourceKey;
            _localizationService = new JsonLocalizationService();
        }

        public override string FormatErrorMessage(string name)
        {
            return _localizationService.GetString(_resourceKey);
        }
    }

    public class TransactionCreateModel
    {
        [Required]
        [LocalizedValidation("CategoryRequired")]
        public Guid CategoryId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "AmountRange")]
        [LocalizedValidation("AmountRequired")]
        public decimal Amount { get; set; }

        [Required]
        [LocalizedValidation("DateRequired")]
        public DateTime TransactionDate { get; set; }

        [StringLength(100)]
        [LocalizedValidation("DescriptionLength")]
        public string? Description { get; set; }
    }
}
