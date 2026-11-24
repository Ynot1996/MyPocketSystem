namespace MyPocket.Services.Ai
{
    /// <summary>
    /// English labels the AI chooses from, mapped to the seeded DB category names.
    /// The model sees readable labels; the app stores the original keys.
    /// </summary>
    public static class CategoryLabels
    {
        public static readonly IReadOnlyDictionary<string, string> LabelToKey = new Dictionary<string, string>
        {
            ["Dining"] = "餐飲",
            ["Transport"] = "交通",
            ["Shopping"] = "購物",
            ["Entertainment"] = "娛樂",
            ["Medical"] = "醫療",
            ["Education"] = "教育",
            ["Rent"] = "房租",
            ["Utilities"] = "水電瓦斯",
            ["Communication"] = "通訊網路",
            ["Insurance"] = "保險",
            ["OtherExpense"] = "其他支出",
            ["Salary"] = "薪資",
            ["Bonus"] = "獎金",
            ["Investment"] = "投資收入",
            ["Interest"] = "利息收入",
            ["OtherIncome"] = "其他收入"
        };

        public static readonly string[] Labels = LabelToKey.Keys.ToArray();

        public static string? KeyFor(string? label) =>
            label != null && LabelToKey.TryGetValue(label, out var key) ? key : null;
    }

    /// <summary>Currencies the app supports; must match the web CurrencyService list.</summary>
    public static class SupportedCurrencies
    {
        public static readonly string[] Codes = { "GBP", "USD", "EUR", "TWD", "JPY", "KRW" };

        public static bool IsSupported(string? code) =>
            code != null && Codes.Contains(code.ToUpperInvariant());
    }
}
