using MyPocket.Shared.Resources;
using MyPocket.Shared.Validation;
using System.ComponentModel.DataAnnotations;

namespace MyPocket.Shared.ViewModels.Transactions
{
    public class TransactionCreateModel
    {
        [LocalizedRequired("CategoryRequired")]
        public Guid CategoryId { get; set; }

        [LocalizedRequired("AmountRequired")]
        [LocalizedRange(0.01, double.MaxValue, "AmountRange")]
        public decimal Amount { get; set; }

        /// <summary>ISO 4217 code of the currency the user is entering this amount in.</summary>
        public string Currency { get; set; } = "GBP";

        [LocalizedRequired("DateRequired")]
        public DateTime TransactionDate { get; set; }

        [LocalizedStringLength(100, "DescriptionLength")]
        public string? Description { get; set; }
    }
}
