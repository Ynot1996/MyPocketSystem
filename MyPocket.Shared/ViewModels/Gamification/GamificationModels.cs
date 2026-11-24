namespace MyPocket.Shared.ViewModels.Gamification
{
    /// <summary>One building in the user's city; one per spending/income category.</summary>
    /// <param name="CategoryKey">DB category name (seed literal, e.g. "餐飲"); localize at display time.</param>
    /// <param name="BuildingType">Stable building key, e.g. "Cafe"; also the localization key suffix.</param>
    /// <param name="NextLevelAt">Transaction count needed for the next level, or null at max level.</param>
    /// <param name="Slot">Grid cell index (row * GridSize + column).</param>
    public record CityBuilding(
        string CategoryKey,
        string BuildingType,
        string Emoji,
        string Color,
        int Level,
        int TransactionCount,
        int? NextLevelAt,
        int Slot);

    /// <summary>A purchased shop item placed in the city.</summary>
    public record CityPlacedItem(string ItemCode, string Kind, string Emoji, int Slot);

    public record CityViewModel(
        IReadOnlyList<CityBuilding> Buildings,
        IReadOnlyList<CityPlacedItem> Decorations,
        IReadOnlyList<CityPlacedItem> Characters,
        string? EquippedOutfitEmoji,
        int GridSize);

    /// <summary>What the user earned for logging one transaction.</summary>
    /// <param name="Reasons">Ledger reasons granted, e.g. ["TransactionLogged", "StreakBonus"].</param>
    public record CoinAward(int Coins, int StreakDays, bool DailyCapReached, IReadOnlyList<string> Reasons);

    public static class ShopItemKind
    {
        public const string Decoration = "Decoration";
        public const string Character = "Character";
        public const string Outfit = "Outfit";
    }

    /// <param name="NameKey">Localization key for the display name.</param>
    public record ShopItem(string Code, string Kind, string Emoji, int Price, string NameKey);

    public enum PurchaseResult
    {
        Success,
        UnknownItem,
        AlreadyOwned,
        InsufficientCoins
    }

    public record ShopEntry(ShopItem Item, bool Owned, bool Equipped);
}
