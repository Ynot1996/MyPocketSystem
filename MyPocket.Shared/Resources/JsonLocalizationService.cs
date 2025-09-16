using System.Text.Json;
using System.Text;

namespace MyPocket.Shared.Resources
{
    public interface ILocalizationService
    {
        string GetString(string key, string culture = "zh-TW");
    }

    public class JsonLocalizationService : ILocalizationService
    {
        private readonly Dictionary<string, Dictionary<string, string>> _localizations = new();
        private const string DEFAULT_CULTURE = "zh-TW";

        public JsonLocalizationService()
        {
            LoadResources();
        }

        private void LoadResources()
        {
            var basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "locales");
            var files = Directory.GetFiles(basePath, "*.json");

            var options = new JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            foreach (var file in files)
            {
                var culture = Path.GetFileNameWithoutExtension(file);
                // 強制用 UTF-8 讀取
                var json = File.ReadAllText(file, new UTF8Encoding(false));
                var resources = JsonSerializer.Deserialize<Dictionary<string, string>>(json, options);
                if (resources != null)
                {
                    _localizations[culture] = resources;
                }
            }
        }

        public string GetString(string key, string culture = DEFAULT_CULTURE)
        {
            if (_localizations.TryGetValue(culture, out var resources) &&
                resources.TryGetValue(key, out var value))
            {
                return value;
            }

            // Fallback to default culture
            if (culture != DEFAULT_CULTURE &&
                _localizations.TryGetValue(DEFAULT_CULTURE, out var defaultResources) &&
                defaultResources.TryGetValue(key, out var defaultValue))
            {
                return defaultValue;
            }

            return key; // Return key if translation not found
        }
    }
}