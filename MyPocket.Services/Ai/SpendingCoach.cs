using System.Globalization;
using System.Text.Json;
using MyPocket.Shared.ViewModels.Capture;

namespace MyPocket.Services.Ai
{
    public interface ISpendingCoach
    {
        /// <summary>Free tier: deterministic insights (conclusions only, no formulas shown).</summary>
        IReadOnlyList<Insight> GetInsights(SpendingSummary summary);

        bool AiAvailable { get; }

        /// <summary>Premium tier: personalised advice from Claude, written in the user's language.</summary>
        Task<CoachAdvice?> GetAdviceAsync(SpendingSummary summary, string cultureName, CancellationToken cancellationToken = default);
    }

    public class SpendingCoach : ISpendingCoach
    {
        private const string SystemPrompt =
            "You are a friendly, practical money coach inside a budgeting app used by students and young " +
            "professionals who often live across currencies. You receive this month's and last month's spending " +
            "by category (already converted to one currency). Give a one-line headline and up to three concrete, " +
            "specific tips grounded in the numbers. Do not invent prices, shops or products, and do not give " +
            "investment advice. Keep each tip under 40 words.";

        private static readonly IReadOnlyDictionary<string, JsonElement> Schema = JsonSchema.Object(new
        {
            headline = new { type = "string" },
            tips = new
            {
                type = "array",
                items = new
                {
                    type = "object",
                    properties = new { title = new { type = "string" }, body = new { type = "string" } },
                    required = new[] { "title", "body" },
                    additionalProperties = false
                }
            }
        }, "headline", "tips");

        private readonly IAiJsonClient _ai;

        public SpendingCoach(IAiJsonClient ai) => _ai = ai;

        public bool AiAvailable => _ai.IsConfigured;

        public IReadOnlyList<Insight> GetInsights(SpendingSummary s)
        {
            var insights = new List<Insight>();
            if (s.TransactionsThisMonth == 0)
            {
                insights.Add(new Insight("InsightNoData", Array.Empty<string>(), "info"));
                return insights;
            }

            var top = s.ThisMonth.OrderByDescending(c => c.Amount).FirstOrDefault();
            if (top != null && s.ExpenseThisMonth > 0)
            {
                var dominant = top.Amount / s.ExpenseThisMonth >= 0.4m;
                insights.Add(new Insight("InsightTopCategory", new[] { Percent(top.Amount, s.ExpenseThisMonth) },
                    dominant ? "warn" : "info", top.CategoryKey));
            }

            // Biggest month-on-month rise among categories that existed last month.
            var lastByKey = s.LastMonth.ToDictionary(c => c.CategoryKey, c => c.Amount);
            var rising = s.ThisMonth
                .Where(c => lastByKey.TryGetValue(c.CategoryKey, out var last) && last > 0 && c.Amount > last * 1.2m)
                .Select(c => (c.CategoryKey, Change: (c.Amount - lastByKey[c.CategoryKey]) / lastByKey[c.CategoryKey]))
                .OrderByDescending(x => x.Change)
                .FirstOrDefault();
            if (rising.CategoryKey != null)
            {
                insights.Add(new Insight("InsightCategoryUp", new[] { Math.Round(rising.Change * 100).ToString(CultureInfo.InvariantCulture) }, "warn", rising.CategoryKey));
            }

            if (s.IncomeThisMonth > 0)
            {
                var saved = s.IncomeThisMonth - s.ExpenseThisMonth;
                insights.Add(saved >= 0
                    ? new Insight("InsightSavingRate", new[] { Percent(saved, s.IncomeThisMonth) }, "good")
                    : new Insight("InsightOverspent", new[] { Percent(-saved, s.IncomeThisMonth) }, "warn"));
            }

            if (s.ExpenseLastMonth > 0 && s.ExpenseThisMonth < s.ExpenseLastMonth)
            {
                insights.Add(new Insight("InsightSpendingDown", new[] { Percent(s.ExpenseLastMonth - s.ExpenseThisMonth, s.ExpenseLastMonth) }, "good"));
            }

            return insights;
        }

        public async Task<CoachAdvice?> GetAdviceAsync(SpendingSummary s, string cultureName, CancellationToken cancellationToken = default)
        {
            if (!_ai.IsConfigured || s.TransactionsThisMonth == 0) return null;

            var language = cultureName.StartsWith("zh", StringComparison.OrdinalIgnoreCase) ? "Traditional Chinese" : "English";
            var payload = JsonSerializer.Serialize(new
            {
                currency = s.Currency,
                incomeThisMonth = s.IncomeThisMonth,
                expenseThisMonth = s.ExpenseThisMonth,
                expenseLastMonth = s.ExpenseLastMonth,
                thisMonthByCategory = s.ThisMonth.Select(c => new { category = LabelFor(c.CategoryKey), amount = c.Amount }),
                lastMonthByCategory = s.LastMonth.Select(c => new { category = LabelFor(c.CategoryKey), amount = c.Amount })
            });

            var json = await _ai.CompleteJsonAsync(
                SystemPrompt, $"Reply in {language}.\nSpending data: {payload}", Schema, null, cancellationToken);
            if (json is not { } root) return null;

            var tips = root.GetProperty("tips").EnumerateArray()
                .Take(3)
                .Select(t => new CoachTip(t.GetProperty("title").GetString() ?? "", t.GetProperty("body").GetString() ?? ""))
                .Where(t => t.Body.Length > 0)
                .ToList();
            return new CoachAdvice(root.GetProperty("headline").GetString() ?? "", tips);
        }

        private static string LabelFor(string key) =>
            CategoryLabels.LabelToKey.FirstOrDefault(kv => kv.Value == key).Key ?? key;

        private static string Percent(decimal part, decimal whole) =>
            whole == 0 ? "0" : Math.Round(part / whole * 100).ToString(CultureInfo.InvariantCulture);
    }
}
