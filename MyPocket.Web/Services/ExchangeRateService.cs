namespace MyPocket.Web.Services
{
    public interface IExchangeRateService
    {
        /// <summary>Convert <paramref name="amount"/> from one ISO currency to another using reference rates.</summary>
        decimal Convert(decimal amount, string fromCode, string toCode);
        /// <summary>Inspect the reference rate (1 unit of <paramref name="fromCode"/> → X <paramref name="toCode"/>).</summary>
        decimal Rate(string fromCode, string toCode);
        /// <summary>Human-friendly note shown next to converted totals (rates are static).</summary>
        string DisclaimerKey { get; }
    }

    public class ExchangeRateService : IExchangeRateService
    {
        // Reference rates expressed as: 1 unit of code = N GBP.
        // These are *approximate* values intended as a stable demo reference;
        // update here when needed. No live FX feed.
        private static readonly Dictionary<string, decimal> ToGbp = new(StringComparer.OrdinalIgnoreCase)
        {
            ["GBP"] = 1.000m,
            ["USD"] = 0.790m,
            ["EUR"] = 0.860m,
            ["TWD"] = 0.025m,
            ["JPY"] = 0.0053m,
            ["KRW"] = 0.00058m
        };

        public string DisclaimerKey => "ExchangeRateDisclaimer";

        public decimal Rate(string fromCode, string toCode)
        {
            if (string.Equals(fromCode, toCode, StringComparison.OrdinalIgnoreCase)) return 1m;
            if (!ToGbp.TryGetValue(fromCode, out var fromGbp)) fromGbp = 1m;
            if (!ToGbp.TryGetValue(toCode, out var toGbp)) toGbp = 1m;
            return fromGbp / toGbp;
        }

        public decimal Convert(decimal amount, string fromCode, string toCode) =>
            Math.Round(amount * Rate(fromCode, toCode), 2);
    }
}
