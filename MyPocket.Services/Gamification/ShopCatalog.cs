using MyPocket.Shared.ViewModels.Gamification;

namespace MyPocket.Services.Gamification
{
    /// <summary>Everything a user can buy with coins. Prices are in coins.</summary>
    public static class ShopCatalog
    {
        public static readonly IReadOnlyList<ShopItem> Items = new[]
        {
            new ShopItem("tree", ShopItemKind.Decoration, "🌳", 30, "Item_tree"),
            new ShopItem("flowers", ShopItemKind.Decoration, "🌷", 40, "Item_flowers"),
            new ShopItem("bench", ShopItemKind.Decoration, "🪑", 50, "Item_bench"),
            new ShopItem("fountain", ShopItemKind.Decoration, "⛲", 120, "Item_fountain"),
            new ShopItem("statue", ShopItemKind.Decoration, "🗽", 200, "Item_statue"),
            new ShopItem("ferris", ShopItemKind.Decoration, "🎡", 300, "Item_ferris"),

            new ShopItem("citizen", ShopItemKind.Character, "🧑", 60, "Item_citizen"),
            new ShopItem("cat", ShopItemKind.Character, "🐈", 80, "Item_cat"),
            new ShopItem("dog", ShopItemKind.Character, "🐕", 80, "Item_dog"),
            new ShopItem("robot", ShopItemKind.Character, "🤖", 250, "Item_robot"),
            new ShopItem("dragon", ShopItemKind.Character, "🐉", 500, "Item_dragon"),

            new ShopItem("cap", ShopItemKind.Outfit, "🧢", 40, "Item_cap"),
            new ShopItem("tophat", ShopItemKind.Outfit, "🎩", 90, "Item_tophat"),
            new ShopItem("crown", ShopItemKind.Outfit, "👑", 400, "Item_crown")
        };

        public static ShopItem? Find(string? code) =>
            Items.FirstOrDefault(i => string.Equals(i.Code, code, StringComparison.OrdinalIgnoreCase));
    }
}
