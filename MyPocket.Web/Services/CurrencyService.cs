using System.Globalization;

namespace MyPocket.Web.Services
{
    /// <param name="Code">ISO 4217 code, e.g. "GBP".</param>
    /// <param name="Symbol">Display symbol, e.g. "£".</param>
    /// <param name="DisplayName">Human-readable name shown in pickers.</param>
    /// <param name="Decimals">Number of fraction digits when formatting (JPY/KRW/TWD = 0).</param>
    /// <param name="Step">Increment for the Amount stepper (cents for western, whole units for CJK).</param>
    /// <param name="Presets">Quick-select preset amounts commonly used for this currency.</param>
    public record CurrencyOption(
        string Code,
        string Symbol,
        string DisplayName,
        int Decimals,
        decimal Step,
        decimal[] Presets);

    public interface ICurrencyService
    {
        /// <summary>The user's chosen "display currency" for totals views (cookie-backed).</summary>
        string CurrentCode { get; }
        /// <summary>Symbol of the current display currency.</summary>
        string CurrentSymbol { get; }
        /// <summary>All currencies the user can pick from.</summary>
        IReadOnlyList<CurrencyOption> Supported { get; }
        /// <summary>Get the metadata for a given currency code (falls back to GBP).</summary>
        CurrencyOption Get(string? code);
        /// <summary>Format an amount in the supplied currency (per-transaction display).</summary>
        string Format(decimal amount, string? code);
        /// <summary>Format an amount in the user's current display currency.</summary>
        string Format(decimal amount);
    }

    public class CurrencyService : ICurrencyService
    {
        public const string CookieName = "MyPocket.Currency";
        public const string DefaultCode = "GBP";

        private static readonly IReadOnlyList<CurrencyOption> Currencies = new[]
        {
            new CurrencyOption("GBP", "£",   "British Pound",       2, 0.50m, new[] { 1m, 5m, 10m, 20m, 50m, 100m }),
            new CurrencyOption("USD", "$",   "US Dollar",           2, 0.50m, new[] { 1m, 5m, 10m, 20m, 50m, 100m }),
            new CurrencyOption("EUR", "€",   "Euro",                2, 0.50m, new[] { 1m, 5m, 10m, 20m, 50m, 100m }),
            new CurrencyOption("TWD", "NT$", "New Taiwan Dollar",   0, 10m,   new[] { 50m, 100m, 200m, 500m, 1000m, 5000m }),
            new CurrencyOption("JPY", "¥",   "Japanese Yen",        0, 100m,  new[] { 100m, 500m, 1000m, 5000m, 10000m }),
            new CurrencyOption("KRW", "₩",   "Korean Won",          0, 100m,  new[] { 1000m, 5000m, 10000m, 50000m, 100000m })
        };

        private static readonly CurrencyOption Fallback = Currencies.First(c => c.Code == DefaultCode);
        private readonly IHttpContextAccessor _http;

        public CurrencyService(IHttpContextAccessor http)
        {
            _http = http;
        }

        public IReadOnlyList<CurrencyOption> Supported => Currencies;

        public string CurrentCode
        {
            get
            {
                var cookie = _http.HttpContext?.Request.Cookies[CookieName];
                if (!string.IsNullOrEmpty(cookie) && Currencies.Any(c => c.Code == cookie))
                {
                    return cookie!;
                }
                return DefaultCode;
            }
        }

        public string CurrentSymbol => Get(CurrentCode).Symbol;

        public CurrencyOption Get(string? code) =>
            (code != null ? Currencies.FirstOrDefault(c => c.Code == code) : null) ?? Fallback;

        public string Format(decimal amount, string? code)
        {
            var c = Get(code);
            var formatted = amount.ToString("N" + c.Decimals, CultureInfo.InvariantCulture);
            return $"{c.Symbol}{formatted}";
        }

        public string Format(decimal amount) => Format(amount, CurrentCode);
    }
}
