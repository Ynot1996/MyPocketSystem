using System.ComponentModel.DataAnnotations;

namespace MyPocket.Web.ViewModels
{
    public class TransactionCreateModel
    {
        // 只放表單上有的欄位，並加上驗證
        [Required(ErrorMessage = "請選擇類別")]
        public Guid CategoryId { get; set; }

        [Required(ErrorMessage = "請輸入金額")]
        [Range(0.01, double.MaxValue, ErrorMessage = "金額必須大於 0")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "請選擇交易日期")]
        public DateTime TransactionDate { get; set; }

        [StringLength(100, ErrorMessage = "備註長度不可超過 100 字")]
        public string? Description { get; set; }
    }
}
