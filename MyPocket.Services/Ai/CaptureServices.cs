using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using MyPocket.Shared.ViewModels.Capture;

namespace MyPocket.Services.Ai
{
    public interface IReceiptScanner
    {
        bool IsAvailable { get; }

        /// <summary>Reads a receipt photo into a draft, or null if it couldn't be read.</summary>
        Task<CaptureDraft?> ScanAsync(AiImage image, string defaultCurrency, CancellationToken cancellationToken = default);
    }

    public interface ITransactionTextParser
    {
        /// <summary>
        /// Turns a sentence ("lunch 12.50 at Pret", from typing or voice) into a draft.
        /// Uses AI when configured and falls back to rules otherwise, so it never fails.
        /// </summary>
        Task<CaptureDraft?> ParseAsync(string text, string defaultCurrency, CancellationToken cancellationToken = default);
    }

    public class ReceiptScanner : IReceiptScanner
    {
        private const string SystemPrompt =
            "You read photos of purchase receipts for a personal finance app. " +
            "Extract the merchant name, the final total actually paid (after discounts and tax), " +
            "the currency, the purchase date, and the single best-fitting spending category. " +
            "If the image is not a receipt or the total is unreadable, return total 0. " +
            "Use an empty string for an unknown date. Keep the description under eight words.";

        private static readonly IReadOnlyDictionary<string, JsonElement> Schema = JsonSchema.Object(new
        {
            merchant = new { type = "string" },
            total = new { type = "number" },
            currency = new { type = "string", @enum = SupportedCurrencies.Codes },
            date = new { type = "string", description = "YYYY-MM-DD, or empty if not visible" },
            category = new { type = "string", @enum = CategoryLabels.Labels },
            description = new { type = "string" }
        }, "merchant", "total", "currency", "date", "category", "description");

        private readonly IAiJsonClient _ai;

        public ReceiptScanner(IAiJsonClient ai) => _ai = ai;

        public bool IsAvailable => _ai.IsConfigured;

        public async Task<CaptureDraft?> ScanAsync(AiImage image, string defaultCurrency, CancellationToken cancellationToken = default)
        {
            if (!_ai.IsConfigured) return null;

            var json = await _ai.CompleteJsonAsync(
                SystemPrompt,
                $"Extract this receipt. If no currency is printed, assume {defaultCurrency}.",
                Schema, image, cancellationToken);
            if (json is not { } root) return null;

            var total = root.GetProperty("total").GetDecimal();
            if (total <= 0) return null;

            var currency = root.GetProperty("currency").GetString();
            return new CaptureDraft(
                Amount: Math.Round(total, 2),
                Currency: SupportedCurrencies.IsSupported(currency) ? currency! : defaultCurrency,
                Date: ParseDate(root.GetProperty("date").GetString()),
                Merchant: NullIfBlank(root.GetProperty("merchant").GetString()),
                Description: NullIfBlank(root.GetProperty("description").GetString()),
                CategoryKey: CategoryLabels.KeyFor(root.GetProperty("category").GetString()),
                Source: "receipt",
                UsedAi: true);
        }

        internal static DateTime? ParseDate(string? value) =>
            DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d : null;

        internal static string? NullIfBlank(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
    }

    public class TransactionTextParser : ITransactionTextParser
    {
        private const string SystemPrompt =
            "You turn a short note about money (typed or dictated, in English or Chinese) into a transaction. " +
            "Pick the amount, the currency, the best-fitting category (income categories only for money received), " +
            "and a short description. If no currency is stated, use the default currency given. " +
            "Return amount 0 if the note contains no amount.";

        private static readonly IReadOnlyDictionary<string, JsonElement> Schema = JsonSchema.Object(new
        {
            amount = new { type = "number" },
            currency = new { type = "string", @enum = SupportedCurrencies.Codes },
            category = new { type = "string", @enum = CategoryLabels.Labels },
            description = new { type = "string" },
            merchant = new { type = "string" }
        }, "amount", "currency", "category", "description", "merchant");

        private readonly IAiJsonClient _ai;

        public TransactionTextParser(IAiJsonClient ai) => _ai = ai;

        public async Task<CaptureDraft?> ParseAsync(string text, string defaultCurrency, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            text = text.Trim();
            if (text.Length > 300) text = text[..300];

            if (_ai.IsConfigured)
            {
                try
                {
                    var json = await _ai.CompleteJsonAsync(
                        SystemPrompt, $"Default currency: {defaultCurrency}\nNote: {text}", Schema, null, cancellationToken);
                    if (json is { } root && root.GetProperty("amount").GetDecimal() > 0)
                    {
                        var currency = root.GetProperty("currency").GetString();
                        return new CaptureDraft(
                            Amount: Math.Round(root.GetProperty("amount").GetDecimal(), 2),
                            Currency: SupportedCurrencies.IsSupported(currency) ? currency! : defaultCurrency,
                            Date: null,
                            Merchant: ReceiptScanner.NullIfBlank(root.GetProperty("merchant").GetString()),
                            Description: ReceiptScanner.NullIfBlank(root.GetProperty("description").GetString()) ?? text,
                            CategoryKey: CategoryLabels.KeyFor(root.GetProperty("category").GetString()),
                            Source: "text",
                            UsedAi: true);
                    }
                }
                catch (Exception) when (!cancellationToken.IsCancellationRequested)
                {
                    // Network/API failure: fall through to the offline parser rather than
                    // losing the user's input.
                }
            }

            return RuleBasedParse(text, defaultCurrency);
        }

        // ---- Offline fallback -------------------------------------------------------

        private static readonly Regex AmountPattern = new(
            @"(?<sym>NT\$|£|\$|€|¥|₩)?\s*(?<num>\d{1,3}(?:,\d{3})+(?:\.\d{1,2})?|\d+(?:\.\d{1,2})?)\s*(?<word>gbp|pounds?|quid|usd|dollars?|eur|euros?|jpy|yen|円|krw|won|twd|ntd|nt|台幣|元)?",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly (string Key, string[] Words)[] CategoryKeywords =
        {
            ("餐飲", new[] { "lunch", "dinner", "breakfast", "coffee", "cafe", "food", "meal", "restaurant", "pret", "starbucks", "tesco meal", "餐", "飯", "咖啡", "早餐", "午餐", "晚餐", "吃" }),
            ("交通", new[] { "taxi", "uber", "bus", "train", "tube", "oyster", "metro", "fuel", "petrol", "flight", "交通", "車", "捷運", "公車", "高鐵", "計程車" }),
            ("購物", new[] { "shopping", "clothes", "amazon", "uniqlo", "shoes", "購物", "衣服", "買" }),
            ("娛樂", new[] { "cinema", "movie", "netflix", "spotify", "game", "concert", "bar", "pub", "電影", "遊戲", "娛樂" }),
            ("醫療", new[] { "pharmacy", "doctor", "dentist", "medicine", "boots", "醫", "藥" }),
            ("教育", new[] { "book", "course", "tuition", "school", "書", "課", "學費" }),
            ("房租", new[] { "rent", "房租", "租金" }),
            ("水電瓦斯", new[] { "electric", "gas bill", "water bill", "utilities", "電費", "水費", "瓦斯" }),
            ("通訊網路", new[] { "phone", "mobile", "internet", "broadband", "wifi", "電話", "網路" }),
            ("薪資", new[] { "salary", "wage", "pay day", "payday", "paycheck", "薪水", "薪資", "工資" }),
            ("獎金", new[] { "bonus", "獎金" }),
            ("其他收入", new[] { "refund", "gift from", "transfer from", "received", "收到", "退款", "匯款" })
        };

        /// <summary>Deterministic parser used when AI is unavailable; good enough for "coffee 3.40".</summary>
        public static CaptureDraft? RuleBasedParse(string text, string defaultCurrency)
        {
            var match = AmountPattern.Match(text);
            if (!match.Success) return null;

            var number = match.Groups["num"].Value.Replace(",", "");
            if (!decimal.TryParse(number, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount) || amount <= 0)
                return null;

            var currency = CurrencyFrom(match.Groups["sym"].Value, match.Groups["word"].Value) ?? defaultCurrency;

            var lower = text.ToLowerInvariant();
            var category = CategoryKeywords.FirstOrDefault(c => c.Words.Any(w => lower.Contains(w))).Key;

            var description = text.Remove(match.Index, match.Length).Trim(' ', ',', '-', ':');
            return new CaptureDraft(
                Amount: amount,
                Currency: currency,
                Date: null,
                Merchant: null,
                Description: string.IsNullOrWhiteSpace(description) ? text : description,
                CategoryKey: category,
                Source: "text",
                UsedAi: false);
        }

        private static string? CurrencyFrom(string symbol, string word)
        {
            switch (symbol.ToUpperInvariant())
            {
                case "£": return "GBP";
                case "€": return "EUR";
                case "¥": return "JPY";
                case "₩": return "KRW";
                case "NT$": return "TWD";
                case "$": return "USD";
            }
            return word.ToLowerInvariant() switch
            {
                "gbp" or "pound" or "pounds" or "quid" => "GBP",
                "usd" or "dollar" or "dollars" => "USD",
                "eur" or "euro" or "euros" => "EUR",
                "jpy" or "yen" or "円" => "JPY",
                "krw" or "won" => "KRW",
                "twd" or "ntd" or "nt" or "台幣" or "元" => "TWD",
                _ => null
            };
        }
    }
}
