using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;
using MyPocket.Core.Models;
using MyPocket.Services.Ai;
using MyPocket.Services.Interfaces;
using MyPocket.Shared.Resources;
using MyPocket.Shared.ViewModels.Capture;
using MyPocket.Web.Infrastructure;
using MyPocket.Web.Services;

namespace MyPocket.Web.Areas.User.Controllers
{
    /// <summary>Statistics and coaching: where the money went, and what to do about it.</summary>
    [Area("User")]
    [Authorize(Roles = "FreeMember,PaidMember")]
    public class InsightsController : Controller
    {
        public const string ExpenseType = "支出";
        public const string IncomeType = "收入";

        private readonly ITransactionService _transactions;
        private readonly ISpendingCoach _coach;
        private readonly ICurrencyService _currency;
        private readonly IExchangeRateService _rates;
        private readonly IMemoryCache _cache;
        private readonly ILocalizationService _localizer;
        private readonly IAppTelemetry _telemetry;

        public InsightsController(
            ITransactionService transactions,
            ISpendingCoach coach,
            ICurrencyService currency,
            IExchangeRateService rates,
            IMemoryCache cache,
            ILocalizationService localizer,
            IAppTelemetry telemetry)
        {
            _transactions = transactions;
            _coach = coach;
            _currency = currency;
            _rates = rates;
            _cache = cache;
            _localizer = localizer;
            _telemetry = telemetry;
        }

        public record MonthPoint(string Label, decimal Income, decimal Expense);

        public class InsightsViewModel
        {
            public string Currency { get; init; } = "GBP";
            public SpendingSummary Summary { get; init; } = null!;
            public IReadOnlyList<Insight> Insights { get; init; } = Array.Empty<Insight>();
            public IReadOnlyList<MonthPoint> Trend { get; init; } = Array.Empty<MonthPoint>();
            /// <summary>This month's expense per currency, in its own currency.</summary>
            public IReadOnlyList<(string Code, decimal Amount)> ByCurrency { get; init; } = Array.Empty<(string, decimal)>();
            public bool IsPremium { get; init; }
            public bool AiAvailable { get; init; }
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.UserIdOrNull();
            if (userId == null) return RedirectToAction("Login", "Account", new { area = "" });

            var rows = await _transactions.GetUserTransactionsAsync(userId.Value);
            var currency = _currency.CurrentCode;
            var now = DateTime.UtcNow;
            var monthStart = new DateTime(now.Year, now.Month, 1);

            var trend = Enumerable.Range(0, 6)
                .Select(i => monthStart.AddMonths(-5 + i))
                .Select(start =>
                {
                    var inMonth = rows.Where(r => r.TransactionDate >= start && r.TransactionDate < start.AddMonths(1)).ToList();
                    return new MonthPoint(
                        start.ToString("MMM", CultureInfo.CurrentUICulture),
                        Sum(inMonth.Where(IsIncome), currency),
                        Sum(inMonth.Where(IsExpense), currency));
                })
                .ToList();

            var thisMonthExpenses = rows.Where(r => IsExpense(r) && r.TransactionDate >= monthStart).ToList();
            var summary = BuildSummary(rows, currency, now);

            return View(new InsightsViewModel
            {
                Currency = currency,
                Summary = summary,
                Insights = _coach.GetInsights(summary),
                Trend = trend,
                ByCurrency = thisMonthExpenses
                    .GroupBy(r => r.Currency)
                    .Select(g => (g.Key, g.Sum(r => r.Amount)))
                    .OrderByDescending(x => x.Item2)
                    .ToList(),
                IsPremium = User.IsInRole("PaidMember"),
                AiAvailable = _coach.AiAvailable
            });
        }

        /// <summary>
        /// Premium AI advice, fetched by the page via AJAX. Cached per user, day,
        /// currency and language for six hours so reloading the page doesn't re-bill.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "PaidMember")]
        [EnableRateLimiting(RateLimitPolicies.Ai)]
        public async Task<IActionResult> Advice(CancellationToken cancellationToken)
        {
            if (!_coach.AiAvailable)
                return Problem(_localizer.GetString("AiNotConfigured"), statusCode: StatusCodes.Status503ServiceUnavailable);

            var userId = User.UserIdOrNull()!.Value;
            var currency = _currency.CurrentCode;
            var culture = CultureInfo.CurrentUICulture.Name;
            var cacheKey = $"advice:{userId}:{DateTime.UtcNow:yyyyMMdd}:{currency}:{culture}";

            if (!_cache.TryGetValue(cacheKey, out CoachAdvice? advice))
            {
                var rows = await _transactions.GetUserTransactionsAsync(userId);
                advice = await _coach.GetAdviceAsync(BuildSummary(rows, currency, DateTime.UtcNow), culture, cancellationToken);
                if (advice != null)
                {
                    _cache.Set(cacheKey, advice, TimeSpan.FromHours(6));
                    _telemetry.TrackEvent("CoachAdviceGenerated");
                }
            }

            return advice == null
                ? UnprocessableEntity(new { error = _localizer.GetString("AdviceUnavailable") })
                : Ok(advice);
        }

        private SpendingSummary BuildSummary(IReadOnlyCollection<Transaction> rows, string currency, DateTime now)
        {
            var thisStart = new DateTime(now.Year, now.Month, 1);
            var lastStart = thisStart.AddMonths(-1);

            var thisMonth = rows.Where(r => r.TransactionDate >= thisStart).ToList();
            var lastMonth = rows.Where(r => r.TransactionDate >= lastStart && r.TransactionDate < thisStart).ToList();

            return new SpendingSummary(
                Currency: currency,
                IncomeThisMonth: Sum(thisMonth.Where(IsIncome), currency),
                ExpenseThisMonth: Sum(thisMonth.Where(IsExpense), currency),
                ExpenseLastMonth: Sum(lastMonth.Where(IsExpense), currency),
                ThisMonth: ByCategory(thisMonth.Where(IsExpense), currency),
                LastMonth: ByCategory(lastMonth.Where(IsExpense), currency),
                TransactionsThisMonth: thisMonth.Count);
        }

        private IReadOnlyList<CategorySpend> ByCategory(IEnumerable<Transaction> rows, string currency) =>
            rows.GroupBy(r => r.Category.CategoryName)
                .Select(g => new CategorySpend(g.Key, Sum(g, currency)))
                .OrderByDescending(c => c.Amount)
                .ToList();

        // Each row is converted from its own currency before summing.
        private decimal Sum(IEnumerable<Transaction> rows, string currency) =>
            rows.Sum(r => _rates.Convert(r.Amount, string.IsNullOrEmpty(r.Currency) ? "GBP" : r.Currency, currency));

        private static bool IsExpense(Transaction t) => t.TransactionType.Trim() == ExpenseType;
        private static bool IsIncome(Transaction t) => t.TransactionType.Trim() == IncomeType;
    }
}
