namespace MyPocket.Services.Gamification
{
    /// <summary>Visual definition of a building type.</summary>
    public record BuildingSpec(string Type, string Emoji, string Color);

    /// <summary>
    /// Maps categories to city buildings and defines how buildings level up.
    ///
    /// Design choice: a building's level is driven by how *often* the user logs
    /// in that category, never by how much money they spend. Rewarding amount
    /// would encourage spending more, which is the opposite of what a finance
    /// app should reward.
    /// </summary>
    public static class CityCatalog
    {
        public const int GridSize = 6;

        /// <summary>Transactions required to reach level 1..5.</summary>
        public static readonly int[] LevelThresholds = { 1, 3, 7, 15, 30 };

        // Keys are the seeded DB category names (stored in Chinese); see DbInitializer.
        private static readonly Dictionary<string, BuildingSpec> ByCategory = new()
        {
            ["餐飲"] = new("Cafe", "☕", "#f59e0b"),
            ["交通"] = new("Station", "🚉", "#3b82f6"),
            ["購物"] = new("Mall", "🛍️", "#ec4899"),
            ["娛樂"] = new("Cinema", "🎬", "#8b5cf6"),
            ["醫療"] = new("Clinic", "🏥", "#ef4444"),
            ["教育"] = new("School", "🏫", "#10b981"),
            ["房租"] = new("Apartment", "🏠", "#f97316"),
            ["水電瓦斯"] = new("PowerPlant", "⚡", "#eab308"),
            ["通訊網路"] = new("Tower", "📡", "#06b6d4"),
            ["保險"] = new("Bank", "🏦", "#64748b"),
            ["其他支出"] = new("Workshop", "🔧", "#94a3b8"),
            ["薪資"] = new("Office", "🏢", "#0ea5e9"),
            ["獎金"] = new("Plaza", "🎉", "#22c55e"),
            ["投資收入"] = new("Skyscraper", "🏙️", "#6366f1"),
            ["利息收入"] = new("Vault", "💰", "#84cc16"),
            ["其他收入"] = new("Market", "🏪", "#a3a3a3")
        };

        /// <summary>User-defined categories all become a generic house.</summary>
        public static readonly BuildingSpec CustomCategory = new("House", "🏡", "#14b8a6");

        public static BuildingSpec For(string? categoryName) =>
            categoryName != null && ByCategory.TryGetValue(categoryName, out var spec) ? spec : CustomCategory;

        public static int LevelFor(int transactionCount) =>
            LevelThresholds.Count(threshold => transactionCount >= threshold);

        public static int? NextLevelAt(int transactionCount)
        {
            foreach (var threshold in LevelThresholds)
            {
                if (transactionCount < threshold) return threshold;
            }
            return null;
        }

        /// <summary>
        /// Grid cells ordered from the centre outwards, so the first buildings a
        /// user unlocks sit in the middle of the map and the city grows outward.
        /// </summary>
        public static IReadOnlyList<int> SlotOrder { get; } = BuildSlotOrder();

        private static IReadOnlyList<int> BuildSlotOrder()
        {
            var centre = (GridSize - 1) / 2.0;
            return Enumerable.Range(0, GridSize * GridSize)
                .OrderBy(slot =>
                {
                    var row = slot / GridSize;
                    var col = slot % GridSize;
                    return Math.Abs(row - centre) + Math.Abs(col - centre);
                })
                .ThenBy(slot => slot)
                .ToList();
        }
    }
}
