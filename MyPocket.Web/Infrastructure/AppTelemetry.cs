using Microsoft.ApplicationInsights;

namespace MyPocket.Web.Infrastructure
{
    /// <summary>
    /// Product analytics events (sign-ups, guest conversions, captures). Controllers
    /// depend on this interface rather than on the Application Insights SDK, so the
    /// backend can be swapped and the app runs fine with monitoring switched off.
    /// </summary>
    public interface IAppTelemetry
    {
        void TrackEvent(string name, IDictionary<string, string>? properties = null);
    }

    public class AppTelemetry : IAppTelemetry
    {
        private readonly TelemetryClient? _client;
        private readonly ILogger<AppTelemetry> _logger;

        public AppTelemetry(IServiceProvider services, ILogger<AppTelemetry> logger)
        {
            // Registered only when APPLICATIONINSIGHTS_CONNECTION_STRING is set.
            _client = services.GetService<TelemetryClient>();
            _logger = logger;
        }

        public void TrackEvent(string name, IDictionary<string, string>? properties = null)
        {
            _client?.TrackEvent(name, properties);
            // Structured log as well, so events show up in the App Service log stream
            // even when Application Insights isn't configured.
            _logger.LogInformation("AppEvent {EventName} {@Properties}", name, properties);
        }
    }
}
