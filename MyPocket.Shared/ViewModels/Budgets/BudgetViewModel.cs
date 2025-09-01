using System.ComponentModel.DataAnnotations;

namespace MyPocket.Shared.ViewModels.Budgets
{
    public class BudgetViewModel
    {
        public Guid BudgetId { get; set; }
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string BudgetYear { get; set; } = string.Empty;
        public string BudgetMonth { get; set; } = string.Empty;
        public decimal Spent { get; set; } // 本月已花費
        public decimal Remaining => Amount - Spent;
    }
}
