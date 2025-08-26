using MyPocket.Shared.Resources;
using MyPocket.Shared.Validation;
using System.ComponentModel.DataAnnotations;

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
        [LocalizedRequired("CategoryRequired")]
        public Guid CategoryId { get; set; }

        [LocalizedRequired("AmountRequired")]
        [LocalizedRange(0.01, double.MaxValue, "AmountRange")]
        public decimal Amount { get; set; }

        [LocalizedRequired("DateRequired")]
        public DateTime TransactionDate { get; set; }

        [LocalizedStringLength(100, "DescriptionLength")]
        public string? Description { get; set; }
    }
}
