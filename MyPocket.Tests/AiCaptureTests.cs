using System.Text.Json;
using MyPocket.Services.Ai;
using MyPocket.Shared.ViewModels.Capture;

namespace MyPocket.Tests
{
    /// <summary>Returns a canned JSON response (or throws) and records what it was asked.</summary>
    internal sealed class FakeAiClient : IAiJsonClient
    {
        private readonly string? _json;
        private readonly bool _throw;

        public FakeAiClient(string? json, bool configured = true, bool throwOnCall = false)
        {
            _json = json;
            IsConfigured = configured;
            _throw = throwOnCall;
        }

        public bool IsConfigured { get; }
        public int Calls { get; private set; }
        public AiImage? LastImage { get; private set; }

        public Task<JsonElement?> CompleteJsonAsync(string systemPrompt, string userText,
            IReadOnlyDictionary<string, JsonElement> schema, AiImage? image = null, CancellationToken cancellationToken = default)
        {
            Calls++;
            LastImage = image;
            if (_throw) throw new HttpRequestException("network down");
            if (_json == null) return Task.FromResult<JsonElement?>(null);
            using var doc = JsonDocument.Parse(_json);
            return Task.FromResult<JsonElement?>(doc.RootElement.Clone());
        }
    }

    public class AiCaptureTests
    {
        private static readonly AiImage Photo = new(new byte[] { 1, 2, 3 }, "image/jpeg");

        [Fact]
        public async Task Receipt_MapsAiOutputToDraft_WithDbCategoryKey()
        {
            var ai = new FakeAiClient("""
                {"merchant":"Pret A Manger","total":8.45,"currency":"GBP","date":"2026-11-20","category":"Dining","description":"Lunch"}
                """);
            var draft = await new ReceiptScanner(ai).ScanAsync(Photo, "GBP");

            Assert.NotNull(draft);
            Assert.Equal(8.45m, draft!.Amount);
            Assert.Equal("GBP", draft.Currency);
            Assert.Equal("餐飲", draft.CategoryKey);
            Assert.Equal(new DateTime(2026, 11, 20), draft.Date);
            Assert.Equal("Pret A Manger", draft.Merchant);
            Assert.True(draft.UsedAi);
            Assert.Same(Photo, ai.LastImage);
        }

        [Fact]
        public async Task Receipt_ZeroTotal_MeansNotAReceipt()
        {
            var ai = new FakeAiClient("""
                {"merchant":"","total":0,"currency":"GBP","date":"","category":"OtherExpense","description":""}
                """);
            Assert.Null(await new ReceiptScanner(ai).ScanAsync(Photo, "GBP"));
        }

        [Fact]
        public async Task Receipt_UnavailableWithoutApiKey()
        {
            var ai = new FakeAiClient(null, configured: false);
            var scanner = new ReceiptScanner(ai);

            Assert.False(scanner.IsAvailable);
            Assert.Null(await scanner.ScanAsync(Photo, "GBP"));
            Assert.Equal(0, ai.Calls);
        }

        [Fact]
        public async Task Receipt_ModelRefusal_ReturnsNull()
        {
            var ai = new FakeAiClient(null); // client returns null on a refusal
            Assert.Null(await new ReceiptScanner(ai).ScanAsync(Photo, "GBP"));
        }

        [Fact]
        public async Task TextParser_UsesAiWhenConfigured()
        {
            var ai = new FakeAiClient("""
                {"amount":12.5,"currency":"GBP","category":"Dining","description":"Lunch at Pret","merchant":"Pret"}
                """);
            var draft = await new TransactionTextParser(ai).ParseAsync("lunch 12.50 at pret", "GBP");

            Assert.Equal(12.5m, draft!.Amount);
            Assert.Equal("餐飲", draft.CategoryKey);
            Assert.True(draft.UsedAi);
        }

        [Fact]
        public async Task TextParser_FallsBackToRules_WhenAiFails()
        {
            var ai = new FakeAiClient(null, throwOnCall: true);
            var draft = await new TransactionTextParser(ai).ParseAsync("taxi £14.20", "USD");

            Assert.NotNull(draft);
            Assert.Equal(14.20m, draft!.Amount);
            Assert.Equal("GBP", draft.Currency);  // symbol beats the default
            Assert.Equal("交通", draft.CategoryKey);
            Assert.False(draft.UsedAi);
        }

        [Theory]
        [InlineData("coffee 3.40", 3.40, "GBP", "餐飲")]
        [InlineData("ramen 1200 yen", 1200, "JPY", null)]
        [InlineData("午餐 120 台幣", 120, "TWD", "餐飲")]
        [InlineData("salary 1,850.00 pounds", 1850, "GBP", "薪資")]
        [InlineData("€35 uniqlo", 35, "EUR", "購物")]
        public void RuleBasedParser_HandlesCommonPhrasings(string text, double amount, string currency, string? category)
        {
            var draft = TransactionTextParser.RuleBasedParse(text, "GBP");

            Assert.NotNull(draft);
            Assert.Equal((decimal)amount, draft!.Amount);
            Assert.Equal(currency, draft.Currency);
            Assert.Equal(category, draft.CategoryKey);
        }

        [Fact]
        public void RuleBasedParser_NoNumber_ReturnsNull()
        {
            Assert.Null(TransactionTextParser.RuleBasedParse("bought some groceries", "GBP"));
        }

        [Fact]
        public void Coach_FlagsDominantCategory_RisingSpend_AndSavingRate()
        {
            var summary = new SpendingSummary(
                Currency: "GBP",
                IncomeThisMonth: 1000m,
                ExpenseThisMonth: 400m,
                ExpenseLastMonth: 300m,
                ThisMonth: new[] { new CategorySpend("餐飲", 200m), new CategorySpend("交通", 200m) },
                LastMonth: new[] { new CategorySpend("餐飲", 100m), new CategorySpend("交通", 200m) },
                TransactionsThisMonth: 12);

            var insights = new SpendingCoach(new FakeAiClient(null, configured: false)).GetInsights(summary);

            var top = Assert.Single(insights, i => i.Key == "InsightTopCategory");
            Assert.Equal("warn", top.Tone);   // 50% of spend
            var up = Assert.Single(insights, i => i.Key == "InsightCategoryUp");
            Assert.Equal("餐飲", up.CategoryKey);
            Assert.Equal("100", up.Args[0]);  // doubled
            var saving = Assert.Single(insights, i => i.Key == "InsightSavingRate");
            Assert.Equal("60", saving.Args[0]);
        }

        [Fact]
        public void Coach_WithNoTransactions_SaysSo()
        {
            var empty = new SpendingSummary("GBP", 0, 0, 0, Array.Empty<CategorySpend>(), Array.Empty<CategorySpend>(), 0);
            var insights = new SpendingCoach(new FakeAiClient(null, configured: false)).GetInsights(empty);
            Assert.Equal("InsightNoData", Assert.Single(insights).Key);
        }

        [Fact]
        public async Task Coach_AiAdvice_IsCappedAtThreeTips()
        {
            var ai = new FakeAiClient("""
                {"headline":"Dining doubled","tips":[
                  {"title":"a","body":"one"},{"title":"b","body":"two"},
                  {"title":"c","body":"three"},{"title":"d","body":"four"}]}
                """);
            var summary = new SpendingSummary("GBP", 1000m, 400m, 300m,
                new[] { new CategorySpend("餐飲", 200m) }, Array.Empty<CategorySpend>(), 5);

            var advice = await new SpendingCoach(ai).GetAdviceAsync(summary, "en-US");

            Assert.Equal("Dining doubled", advice!.Headline);
            Assert.Equal(3, advice.Tips.Count);
        }
    }
}
