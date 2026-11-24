using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;
using MyPocket.DataAccess.Data;
using MyPocket.Shared.Resources;
using MyPocket.Services;
using MyPocket.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization(options => {
        options.DataAnnotationLocalizerProvider = (type, factory) =>
            factory.Create(typeof(MyPocket.Shared.Resources.JsonLocalizationService));
    });

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

// Cookie auth = how we remember a signed-in user across requests.
// Google = one external identity provider the user can use to sign in.
// External providers always still produce a *cookie* session afterwards,
// so the pipeline is: Google -> validated identity -> cookie sign-in.
var authBuilder = builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
{
    // Only register the Google scheme when both secrets are present.
    // This lets the app run locally even if the dev hasn't configured OAuth yet;
    // the "Sign in with Google" button checks the same flag before rendering.
    authBuilder.AddGoogle(options =>
    {
        options.ClientId = googleClientId;
        options.ClientSecret = googleClientSecret;
        // Google redirects back here after the user grants consent. ASP.NET
        // Core's middleware handles /signin-google automatically; we only
        // need to make sure the same path is registered in Google Cloud Console.
        options.CallbackPath = "/signin-google";
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

        // OnTicketReceived runs *after* Google verified the user but *before*
        // the cookie is written. It's the one place where we can map
        // Google's raw claims (email, name) to our own domain claims
        // (UserId, Role) so downstream Authorize attributes behave normally.
        options.Events.OnTicketReceived = async context =>
        {
            var db = context.HttpContext.RequestServices.GetRequiredService<MyPocketDBContext>();
            var subs = context.HttpContext.RequestServices.GetRequiredService<MyPocket.Services.ISubscriptionService>();

            var email = context.Principal?.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
            {
                context.Fail("Google did not return an email address.");
                return;
            }

            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);
            if (user == null)
            {
                // First-time Google user → provision a local account. We still
                // set a non-null PasswordHash because the column is NOT NULL,
                // but it's an unguessable random value — the local password
                // login path will always reject it.
                user = new MyPocket.Core.Models.User
                {
                    UserId = Guid.NewGuid(),
                    Email = email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString("N")),
                    Nickname = context.Principal?.FindFirstValue(ClaimTypes.Name)
                               ?? context.Principal?.FindFirstValue(ClaimTypes.GivenName)
                               ?? email.Split('@')[0],
                    Role = "FreeMember",
                    CreationDate = DateTime.UtcNow,
                    LastLoginDate = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };
                db.Users.Add(user);
                await db.SaveChangesAsync();
                await subs.SubscribeToBasicPlanAsync(user.UserId);
            }
            else
            {
                user.LastLoginDate = DateTime.UtcNow;
                await db.SaveChangesAsync();
            }

            // Replace Google's raw principal with one carrying our domain claims
            // (UserId, Role). Everything inside the app reads these via
            // ClaimTypes.NameIdentifier / ClaimTypes.Role.
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Email),
                new Claim("Nickname", user.Nickname ?? user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            context.Principal = new ClaimsPrincipal(identity);
        };
    });
}

// Add localization service
builder.Services.AddSingleton<ILocalizationService, JsonLocalizationService>();

// Currency formatting service (cookie-based preference, default GBP)
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

builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<ISavingGoalService, SavingGoalService>();
builder.Services.AddScoped<IBudgetService, BudgetService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<MyPocket.Services.ISubscriptionService, SubscriptionService>();

// Gamification: coins, streaks, the city view and the item shop.
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IGamificationService, MyPocket.Services.Gamification.GamificationService>();
builder.Services.AddScoped<IShopService, MyPocket.Services.Gamification.ShopService>();

////////////////////////////////////////////////////////////////////////////////////////////////
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var context = services.GetRequiredService<MyPocketDBContext>();

    // Azure SQL Serverless may be paused on cold start; retry seeding until the DB is reachable.
    const int maxAttempts = 8;
    for (int attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            await DbInitializer.Initialize(context);
            logger.LogInformation("Database seed completed on attempt {Attempt}.", attempt);
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

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
