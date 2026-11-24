using System.Text.Json;
using Anthropic;
using Anthropic.Models.Beta.Messages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MyPocket.Services.Ai
{
    /// <summary>An image to send alongside the prompt (e.g. a receipt photo).</summary>
    public record AiImage(byte[] Data, string MediaType);

    /// <summary>
    /// Minimal LLM abstraction: prompt (+ optional image) in, schema-valid JSON out.
    /// Features depend on this, not on the Anthropic SDK, so they can be unit-tested
    /// with a fake and the provider can be swapped.
    /// </summary>
    public interface IAiJsonClient
    {
        /// <summary>False when no API key is configured; callers fall back to non-AI paths.</summary>
        bool IsConfigured { get; }

        /// <returns>The parsed JSON object, or null if the model declined or returned nothing usable.</returns>
        Task<JsonElement?> CompleteJsonAsync(
            string systemPrompt,
            string userText,
            IReadOnlyDictionary<string, JsonElement> schema,
            AiImage? image = null,
            CancellationToken cancellationToken = default);
    }

    /// <summary>Claude implementation using structured outputs (guaranteed schema-valid JSON).</summary>
    public class AnthropicJsonClient : IAiJsonClient
    {
        public const string DefaultModel = "claude-opus-5-5";

        private readonly AnthropicClient? _client;
        private readonly string _model;
        private readonly ILogger<AnthropicJsonClient> _logger;

        public AnthropicJsonClient(IConfiguration configuration, ILogger<AnthropicJsonClient> logger)
        {
            _logger = logger;
            // Ai:Model lets the operator trade quality for cost (e.g. claude-haiku-5-5)
            // without a code change.
            _model = configuration["Ai:Model"] ?? DefaultModel;

            var apiKey = configuration["Ai:AnthropicApiKey"] ?? configuration["ANTHROPIC_API_KEY"];
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                _client = new AnthropicClient { ApiKey = apiKey };
            }
        }

        public bool IsConfigured => _client != null;

        public async Task<JsonElement?> CompleteJsonAsync(
            string systemPrompt,
            string userText,
            IReadOnlyDictionary<string, JsonElement> schema,
            AiImage? image = null,
            CancellationToken cancellationToken = default)
        {
            if (_client == null) return null;

            var content = new List<BetaContentBlockParam>();
            if (image != null)
            {
                content.Add(new BetaImageBlockParam
                {
                    Source = new BetaBase64ImageSource
                    {
                        Data = Convert.ToBase64String(image.Data),
                        MediaType = image.MediaType
                    }
                });
            }
            content.Add(new BetaTextBlockParam { Text = userText });

            var response = await _client.Beta.Messages.Create(new MessageCreateParams
            {
                Model = _model,
                MaxTokens = 4096,
                // Extraction and short advice don't need deep reasoning; low effort
                // keeps latency and cost down.
                OutputConfig = new BetaOutputConfig
                {
                    Effort = Effort.Low,
                    Format = new BetaJsonOutputFormat { Schema = schema.ToDictionary(kv => kv.Key, kv => kv.Value) }
                },
                // If a safety classifier declines, Anthropic re-runs the request on a
                // recommended fallback model inside the same call.
                Betas = ["server-side-fallback-2026-07-01"],
                Fallbacks = new Default(),
                System = systemPrompt,
                Messages = [new BetaUserMessageParam { Content = content }]
            }, cancellationToken);

            // Always check the stop reason before reading content.
            if (response.StopReason == "refusal")
            {
                _logger.LogWarning("AI request was declined (refusal).");
                return null;
            }

            foreach (var block in response.Content)
            {
                if (block.TryPickText(out var text) && !string.IsNullOrWhiteSpace(text.Text))
                {
                    using var doc = JsonDocument.Parse(text.Text);
                    return doc.RootElement.Clone();
                }
            }
            return null;
        }
    }

    /// <summary>Small helper to build JSON Schemas for structured outputs.</summary>
    public static class JsonSchema
    {
        public static IReadOnlyDictionary<string, JsonElement> Object(object properties, params string[] required) =>
            new Dictionary<string, JsonElement>
            {
                ["type"] = JsonSerializer.SerializeToElement("object"),
                ["properties"] = JsonSerializer.SerializeToElement(properties),
                ["required"] = JsonSerializer.SerializeToElement(required),
                ["additionalProperties"] = JsonSerializer.SerializeToElement(false)
            };
    }
}
