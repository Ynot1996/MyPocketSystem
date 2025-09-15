using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using MyPocket.DataAccess.Data;
using MyPocket.Shared.Resources;
using MyPocket.Services;
using MyPocket.Services.Interfaces;
using Google.Cloud.SecretManager.V1;

var builder = WebApplication.CreateBuilder(args);
var secretName = $"projects/302126295113/secrets/MyPocketDB-ConnectionString";

try
{
    // 創建 Secret Manager 客戶端
    var secretManagerClient = SecretManagerServiceClient.Create();
    // 存取 Secret 的內容
    var secret = secretManagerClient.AccessSecretVersion(secretName);
    var connectionString = secret.Payload.Data.ToStringUtf8();

    // 將從 Secret Manager 讀取的連線字串添加到配置中
    builder.Configuration["ConnectionStrings:MyPocketDBConnection"] = connectionString;
}
catch (Exception ex)
{
    // 如果讀取 Secret 失敗，這裡會捕捉到錯誤
    // 建議在本地端，你可以在 appsettings.json 中保留連線字串以進行開發
    Console.WriteLine($"Error accessing secret from Secret Manager: {ex.Message}");
    // 在生產環境中，這個錯誤很可能意味著權限問題或 Secret 不存在
}

builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization(options => {
        options.DataAnnotationLocalizerProvider = (type, factory) =>
            factory.Create(typeof(MyPocket.Shared.Resources.JsonLocalizationService));
    });

builder.Services.AddDbContext<MyPocketDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyPocketDBConnection")));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

// Add localization service
builder.Services.AddSingleton<ILocalizationService, JsonLocalizationService>();

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("zh-TW"), 
        new CultureInfo("en-US")  
    };

    options.DefaultRequestCulture = new RequestCulture("zh-TW");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<ISavingGoalService, SavingGoalService>();
builder.Services.AddScoped<IBudgetService, BudgetService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<MyPocket.Services.ISubscriptionService, SubscriptionService>();

////////////////////////////////////////////////////////////////////////////////////////////////
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<MyPocketDBContext>();
        await DbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
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
