using System.Globalization;
using System.Text;
using System.Text.Json;

namespace MyPocket.Shared.Resources
{
    public interface ILocalizationService
    {
        string GetString(string key, string? culture = null);
        string this[string key] { get; }
    }

    public class JsonLocalizationService : ILocalizationService
    {
        private readonly Dictionary<string, Dictionary<string, string>> _localizations = new();
        private const string DefaultCulture = "en-US";

        public JsonLocalizationService()
        {
            LoadResources();
        }

        private void LoadResources()
        {
            var basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "locales");
            if (!Directory.Exists(basePath))
            {
                return;
            }

            var files = Directory.GetFiles(basePath, "*.json");

            var options = new JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            foreach (var file in files)
            {
                var culture = Path.GetFileNameWithoutExtension(file);
                // Force UTF-8 (no BOM) to avoid mojibake in CJK strings.
                var json = File.ReadAllText(file, new UTF8Encoding(false));
                var resources = JsonSerializer.Deserialize<Dictionary<string, string>>(json, options);
                if (resources != null)
                {
                    _localizations[culture] = resources;
                }
            }
        }

        public string this[string key] => GetString(key);

        public string GetString(string key, string? culture = null)
        {
            // Resolve culture: explicit > current request UI culture > default.
            var resolved = !string.IsNullOrEmpty(culture)
                ? culture
                : CultureInfo.CurrentUICulture.Name;

            if (_localizations.TryGetValue(resolved, out var resources) &&
                resources.TryGetValue(key, out var value))
            {
                return value;
            }

            // Fallback to default culture.
            if (resolved != DefaultCulture &&
                _localizations.TryGetValue(DefaultCulture, out var defaultResources) &&
                defaultResources.TryGetValue(key, out var defaultValue))
            {
                return defaultValue;
            }

            // Last resort: return the key itself so missing translations are obvious in the UI.
            return key;
        }
    }
}
