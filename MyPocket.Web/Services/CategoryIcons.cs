namespace MyPocket.Web.Services
{
    /// <summary>
    /// Maps category names (DB literal Chinese seed values) to Bootstrap Icon
    /// class names so the UI can show a glyph next to each category. Lookups fall
    /// back to a generic tag icon for unknown / user-defined categories.
    /// </summary>
    public static class CategoryIcons
    {
        private static readonly Dictionary<string, string> Map = new()
        {
            // Expense
            ["餐飲"]     = "bi-cup-hot-fill",
            ["交通"]     = "bi-bus-front-fill",
            ["購物"]     = "bi-bag-fill",
            ["娛樂"]     = "bi-controller",
            ["醫療"]     = "bi-heart-pulse-fill",
            ["教育"]     = "bi-mortarboard-fill",
            ["房租"]     = "bi-house-door-fill",
            ["水電瓦斯"] = "bi-lightning-charge-fill",
            ["通訊網路"] = "bi-wifi",
            ["保險"]     = "bi-shield-fill-check",
            ["其他支出"] = "bi-three-dots",
            // Income
            ["薪資"]     = "bi-cash-coin",
            ["獎金"]     = "bi-gift-fill",
            ["投資收入"] = "bi-graph-up-arrow",
            ["利息收入"] = "bi-bank",
            ["其他收入"] = "bi-three-dots"
        };

        public static string For(string? categoryName)
        {
            if (!string.IsNullOrEmpty(categoryName) && Map.TryGetValue(categoryName, out var icon))
            {
                return icon;
            }
            return "bi-tag";
        }
    }
}
