using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;
using System.Threading.RateLimiting;
using MyPocket.DataAccess.Data;
using MyPocket.Shared.Resources;
using MyPocket.Services;
using MyPocket.Services.Interfaces;
using MyPocket.Web.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization(options => {
        options.DataAnnotationLocalizerProvider = (type, factory) =>
            factory.Create(typeof(MyPocket.Shared.Resources.JsonLocalizationService));
    });

// ---------------------------------------------------------------------------
// Database
// ---------------------------------------------------------------------------
// With no connection string configured (fresh clone, CI smoke runs, recruiters
// trying the repo) fall back to an in-memory database so `dotnet run` works with
// zero setup. Production always supplies ConnectionStrings__MyPocketDBConnection.
var connectionString = builder.Configuration.GetConnectionString("MyPocketDBConnection");
var useInMemoryDb = string.IsNullOrWhiteSpace(connectionString);
builder.Services.AddDbContext<MyPocketDBContext>(options =>
{
    if (useInMemoryDb)
    {
        options.UseInMemoryDatabase("MyPocketLocal");
    }
    else
    {
        options.UseSqlServer(
            connectionString,
            sql => sql.EnableRetryOnFailure(
                maxRetryCount: 6,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null));
    }
});

// JSON/AJAX endpoints send the antiforgery token in this header instead of a form field.
builder.Services.AddAntiforgery(options => options.HeaderName = "RequestVerificationToken");

// ---------------------------------------------------------------------------
// Authentication: cookie session + optional Google
// ---------------------------------------------------------------------------
// External providers always still produce a *cookie* session afterwards:
// Google -> validated identity -> our own claims -> cookie sign-in.
var authBuilder = builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
        options.SlidingExpiration = true;
        options.Events.OnValidatePrincipal = CookieUserValidator.ValidateAsync;
    });

var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
{
    // Registered only when both secrets exist, so local dev runs without OAuth set up.
    authBuilder.AddGoogle(options =>
    {
        options.ClientId = googleClientId;
        options.ClientSecret = googleClientSecret;
        options.CallbackPath = "/signin-google";
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

        // Runs after Google verified the user but before our cookie is written:
        // map Google's identity onto a local account and our own claims.
        options.Events.OnTicketReceived = async context =>
        {
            var email = context.Principal?.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
            {
                context.Fail("Google did not return an email address.");
                return;
            }
            var displayName = context.Principal?.FindFirstValue(ClaimTypes.Name)
                              ?? context.Principal?.FindFirstValue(ClaimTypes.GivenName);

            // The authentication middleware handles /signin-google before it
            // populates HttpContext.User, so read the existing (guest) cookie explicitly.
            var existingSession = await context.HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var guestId = existingSession.Succeeded && existingSession.Principal!.IsGuestUser()
                ? existingSession.Principal!.UserIdOrNull()
                : null;

            var accounts = context.HttpContext.RequestServices.GetRequiredService<IAccountService>();
            var user = await accounts.ResolveExternalLoginAsync(email, displayName, guestId);

            var telemetry = context.HttpContext.RequestServices.GetRequiredService<IAppTelemetry>();
            telemetry.TrackEvent(guestId.HasValue ? "GuestConverted" : "GoogleSignIn",
                new Dictionary<string, string> { ["method"] = "google" });

            context.Principal = UserClaims.Build(user);
        };
    });
}

// ---------------------------------------------------------------------------
// Rate limiting (abuse + cost protection)
// ---------------------------------------------------------------------------
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Anonymous guest creation: 5 per hour per IP.
    options.AddPolicy(RateLimitPolicies.GuestStart, http =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: http.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromHours(1),
                QueueLimit = 0
            }));

    // AI features call a paid API: 30 requests per day per signed-in user.
    options.AddPolicy(RateLimitPolicies.Ai, http =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: http.User.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? http.Connection.RemoteIpAddress?.ToString()
                          ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromDays(1),
                QueueLimit = 0
            }));
});

// Azure App Service terminates TLS at a front end and forwards the client IP in
// X-Forwarded-For; without this every request appears to come from the proxy,
// which would make the per-IP rate limit global.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// ---------------------------------------------------------------------------
// Monitoring
// ---------------------------------------------------------------------------
// Application Insights is enabled only when a connection string is configured
// (App Service setting APPLICATIONINSIGHTS_CONNECTION_STRING). It collects
// requests, dependencies, exceptions and the custom product events below.
if (!string.IsNullOrEmpty(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
{
    builder.Services.AddApplicationInsightsTelemetry();
}
builder.Services.AddSingleton<IAppTelemetry, AppTelemetry>();
builder.Services.AddHealthChecks();

// ---------------------------------------------------------------------------
// Localization & currency
// ---------------------------------------------------------------------------
builder.Services.AddSingleton<ILocalizationService, JsonLocalizationService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<MyPocket.Web.Services.ICurrencyService, MyPocket.Web.Services.CurrencyService>();
builder.Services.AddSingleton<MyPocket.Web.Services.IExchangeRateService, MyPocket.Web.Services.ExchangeRateService>();

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("en-US"),
        new CultureInfo("zh-TW")
    };

    options.DefaultRequestCulture = new RequestCulture("en-US");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

// ---------------------------------------------------------------------------
// Domain services
// ---------------------------------------------------------------------------
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<ISavingGoalService, SavingGoalService>();
builder.Services.AddScoped<IBudgetService, BudgetService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<MyPocket.Services.ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<IAccountService, MyPocket.Services.Accounts.AccountService>();

// Gamification: coins, streaks, the city view and the item shop.
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IGamificationService, MyPocket.Services.Gamification.GamificationService>();
builder.Services.AddScoped<IShopService, MyPocket.Services.Gamification.ShopService>();

// AI capture & coaching (Claude). Enabled when Ai:AnthropicApiKey (App Service
// setting Ai__AnthropicApiKey) or ANTHROPIC_API_KEY is set; without it, receipt
// scanning is hidden and text capture uses the offline rule-based parser.
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<MyPocket.Services.Ai.IAiJsonClient, MyPocket.Services.Ai.AnthropicJsonClient>();
builder.Services.AddSingleton<MyPocket.Services.Ai.IReceiptScanner, MyPocket.Services.Ai.ReceiptScanner>();
builder.Services.AddSingleton<MyPocket.Services.Ai.ITransactionTextParser, MyPocket.Services.Ai.TransactionTextParser>();
builder.Services.AddSingleton<MyPocket.Services.Ai.ISpendingCoach, MyPocket.Services.Ai.SpendingCoach>();

////////////////////////////////////////////////////////////////////////////////////////////////
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var context = services.GetRequiredService<MyPocketDBContext>();

    if (useInMemoryDb)
    {
        logger.LogWarning("No connection string configured: using an in-memory database. Data resets on restart.");
    }

    // Azure SQL Serverless may be paused on cold start; retry seeding until the DB is reachable.
    const int maxAttempts = 8;
    for (int attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            await DbInitializer.Initialize(context);
            logger.LogInformation("Database seed completed on attempt {Attempt}.", attempt);

            // Guest accounts are disposable; close ones nobody converted within a week.
            var closed = await services.GetRequiredService<IAccountService>().CleanupStaleGuestsAsync(TimeSpan.FromDays(7));
            if (closed > 0) logger.LogInformation("Closed {Count} stale guest accounts.", closed);
            break;
        }
        catch (Exception ex) when (attempt < maxAttempts)
        {
            var delay = TimeSpan.FromSeconds(Math.Min(30, attempt * 5));
            logger.LogWarning(ex, "Database seed attempt {Attempt} failed; retrying in {Delay}s.", attempt, delay.TotalSeconds);
            await Task.Delay(delay);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database seed failed after {MaxAttempts} attempts.", maxAttempts);
        }
    }
}

app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseRequestLocalization();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

// Liveness probe for uptime monitors / App Service health check.
app.MapHealthChecks("/healthz");

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
