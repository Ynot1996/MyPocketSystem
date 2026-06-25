using System.Globalization;

namespace MyPocket.Web.Services
{
    public record CurrencyOption(string Code, string Symbol, string DisplayName);

    public interface ICurrencyService
    {
        /// <summary>Code of the currency the current request is rendering in (e.g. "GBP").</summary>
        string CurrentCode { get; }
        /// <summary>Symbol of the current currency (e.g. "£").</summary>
        string CurrentSymbol { get; }
        /// <summary>All currencies the user can switch between.</summary>
        IReadOnlyList<CurrencyOption> Supported { get; }
        /// <summary>Format an amount with the current currency symbol and locale.</summary>
        string Format(decimal amount);
    }

    public class CurrencyService : ICurrencyService
    {
        public const string CookieName = "MyPocket.Currency";
        public const string DefaultCode = "GBP";

        private static readonly IReadOnlyList<CurrencyOption> Currencies = new[]
        {
            new CurrencyOption("GBP", "£", "British Pound"),
            new CurrencyOption("USD", "$", "US Dollar"),
            new CurrencyOption("EUR", "€", "Euro"),
            new CurrencyOption("TWD", "NT$", "New Taiwan Dollar")
        };

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

        public string CurrentSymbol =>
            Currencies.First(c => c.Code == CurrentCode).Symbol;

        public string Format(decimal amount)
        {
            // No FX conversion — display the stored numeric value with the chosen symbol.
            // CJK currencies (NT$) traditionally render without decimal cents; western ones use 2.
            var digits = CurrentCode == "TWD" ? 0 : 2;
            var formatted = amount.ToString("N" + digits, CultureInfo.InvariantCulture);
            return $"{CurrentSymbol}{formatted}";
        }
    }
}
