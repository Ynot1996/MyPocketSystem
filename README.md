# MyPocket — Personal Finance Management System

A full-stack personal finance web application built with **ASP.NET Core 8 (MVC)** and **Entity Framework Core**, deployed to **Azure App Service** with a **GitHub Actions CI/CD pipeline** that runs unit tests before every deploy.

**🌐 Live demo:** <https://mypocket-web-app.azurewebsites.net/>

Users can track income and expenses across **six currencies** (GBP, USD, EUR, TWD, JPY, KRW), set budgets and saving goals, and view analytics grouped by category, time period, and currency. The UI is bilingual (English / 繁體中文) via a cookie-driven culture switcher.

---

## Table of contents

- [Feature highlights](#feature-highlights)
- [Architecture](#architecture)
- [Technology stack](#technology-stack)
- [Engineering highlights](#engineering-highlights-for-recruiters)
- [Testing](#testing)
- [CI / CD](#ci--cd)
- [Running locally](#running-locally)
- [Default credentials](#default-credentials-demo-only)
- [Deployment](#deployment)
- [Project layout](#project-layout)

---

## Feature highlights

### End user
- **Authentication** — email + BCrypt-hashed password, cookie-based auth.
- **Transactions CRUD** — add / edit / delete, filter by month/year, delete-is-soft (`IsDeleted` flag).
- **Per-transaction currency** — each row stores its own ISO 4217 code; UI shows the native symbol (£ / $ / € / NT$ / ¥ / ₩).
- **Amount entry UX** — free-form decimal input, ± stepper that respects each currency's natural step (GBP 0.5, JPY 100), and quick-select presets per currency.
- **Budgets** per month, with spent/remaining progress bars; **saving goals** auto-rolled monthly/yearly.
- **Analytics** — pie chart of expense-by-category with Today / This Month / This Year / All Time filters; totals convert all rows to a chosen display currency via a **reference exchange-rate service**, alongside a per-currency breakdown that keeps the original amounts intact.
- **Subscription plans** — Free / Premium (Monthly / Quarterly / Half-yearly / Yearly), with upgrade and cancellation flows.
- **Bilingual UI** — language picker in the navbar, backed by a JSON-based `IStringLocalizer`-equivalent service.

### Admin area
- User management (view, deactivate), bulk subscription updates, category management, announcements CRUD.

---

## Architecture

The solution is split into seven projects, following a layered architecture:

```
MyPocketSystem.sln
├── MyPocket.Core         Domain entities (POCO models — no EF dependencies).
├── MyPocket.DataAccess   EF Core DbContext + fluent configuration + seeding.
├── MyPocket.Services     Business logic, exposed via interfaces (DI-friendly).
├── MyPocket.Shared       DTOs, ViewModels, localization service + resource JSON,
│                         validation attributes, metadata.
├── MyPocket.Web          ASP.NET Core MVC host — controllers, Razor views,
│                         currency/exchange-rate/icon helpers.
├── MyPocket.API          REST API project (for future mobile client).
├── MyPocket.Mobile       .NET MAUI shell (starter — not deployed).
└── MyPocket.Tests        xUnit unit tests (31) using EF Core InMemory.
```

Dependency direction is one-way: `Web` → `Services` → `DataAccess` → `Core`. `Shared` is referenced by everything that needs cross-cutting types (ViewModels, resource strings).

Key architectural choices:

- **Repository-less** — `Services` take `DbContext` directly; EF Core *is* the data-access abstraction. Keeps the layer thin and the service surface explicit.
- **Service-layer returns result keys, not strings** — e.g. `(false, "InvalidCategory", null)`. Controllers translate those keys through `ILocalizationService` before putting them in `TempData`, which makes the services language-agnostic and easy to unit-test.
- **Idempotent schema upgrades on startup** — `DbInitializer.ApplySchemaFixes` runs guarded `ALTER TABLE` statements on every boot so already-deployed databases can be reshaped without migrations.

---

## Technology stack

| Layer | Choice |
|---|---|
| Runtime | .NET 8 |
| Web framework | ASP.NET Core MVC (Razor views) |
| ORM | Entity Framework Core 9 (SQL Server provider) |
| Database (prod) | Azure SQL Database — Serverless (Free offer) |
| Database (test) | EF Core InMemory provider |
| Auth | Cookie authentication + BCrypt.Net password hashing |
| Internationalization | Custom `JsonLocalizationService` + `CookieRequestCultureProvider`, en-US / zh-TW |
| Charts | Chart.js + chartjs-plugin-datalabels |
| UI | Bootstrap 5 + Bootstrap Icons |
| Testing | xUnit + EF Core InMemory |
| Hosting | Azure App Service (Linux, F1 free tier) |
| CI / CD | GitHub Actions with Azure OIDC federated identity (no long-lived secrets) |

---

## Engineering highlights (for recruiters)

A few non-trivial things that show up in the code history:

1. **CI/CD with OIDC federated identity.** Deploys go `git push` → GitHub Actions → `dotnet test` gate → publish → Azure App Service, authenticated with OIDC (no `AZURE_PUBLISH_PROFILE` secret stored anywhere). Workflow: [`.github/workflows/deploy.yml`](.github/workflows/deploy.yml).

2. **Resilient Azure SQL access.** The DB runs on the free Serverless tier, which auto-pauses when idle. The DbContext registers `EnableRetryOnFailure(6, 10s, null)` so cold-start calls are retried transparently, and `DbInitializer` wraps itself in an exponential-backoff loop (up to 8 attempts) so the first request after a wake-up doesn't 500. See [`MyPocket.Web/Program.cs`](MyPocket.Web/Program.cs).

3. **Online schema migration without EF Migrations.** The project was originally scaffolded with a buggy `IsFixedLength()` on `TransactionType` (which produced `NCHAR(1)` and truncated `"支出"` on insert) and lacked a `Currency` column. Rather than reset the database, [`DbInitializer.ApplySchemaFixes`](MyPocket.DataAccess/Data/DbInitializer.cs) runs guarded `ALTER TABLE` statements on every boot that are safe to re-run.

4. **Multi-currency, no FX service.** Each transaction stores its own ISO 4217 code. `ICurrencyService` formats per-row amounts in the currency they were entered in, and `IExchangeRateService` provides a static reference table so cross-currency totals can be rendered in any display currency. No live FX dependency — honest about what it is, and easy to swap for a real feed later. See [`MyPocket.Web/Services/`](MyPocket.Web/Services/).

5. **Bilingual UI driven by data, not forked views.** A single set of Razor views renders in English or Traditional Chinese based on a cookie. DB-stored labels (e.g. `CategoryName = "餐飲"`) are kept in their original form; the UI looks them up as translation keys at display time (`@Localizer[item.CategoryName]`), so switching language costs zero extra DB rows.

6. **Unit tests gate the deploy.** 31 xUnit tests exercise the full CRUD surface of every service — including ownership checks ("Bob can't edit Alice's transaction") and decimal-precision regression tests ("1.80 must not round to 2"). The GitHub Actions job fails the deploy if any test fails.

7. **Honest UX feedback loop.** Controllers write every outcome to a `TempData["SuccessMessage" / "ErrorMessage"]` channel; a shared `_FlashMessages` partial in the layout surfaces them as alerts. Found and fixed a silent-failure class: the app was swallowing `DbUpdateException`s before this partial existed.

---

## Testing

```bash
dotnet test MyPocket.Tests/MyPocket.Tests.csproj
```

- **31 tests**, grouped by service (`CategoryServiceTests`, `TransactionServiceTests`, `BudgetServiceTests`, `SavingGoalServiceTests`, `SubscriptionServiceTests`).
- Each test spins up a fresh **EF Core InMemory** database (via [`TestHelper.NewContext()`](MyPocket.Tests/TestHelper.cs)) so tests are independent and don't need a SQL Server instance.
- Includes regression tests for real bugs found during development, e.g. `Create_PreservesDecimalAmount` (1.80 GBP must stay 1.80, not 2) and `Update_ChangesFields_AndOnlyForOwner` (cross-user write attempt returns `false` and leaves the row alone).

---

## CI / CD

[`.github/workflows/deploy.yml`](.github/workflows/deploy.yml) runs on every push to `main`:

1. `actions/setup-dotnet@v4` with .NET 8.
2. `dotnet test MyPocket.Tests/...` — a failure aborts the deploy.
3. `dotnet publish MyPocket.Web -c Release -o ./publish`.
4. `azure/login@v2` using OIDC with three repo secrets (`AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID`) — **no publish profile, no long-lived credentials**.
5. `azure/webapps-deploy@v3` ships the published folder to the App Service.

Can also be triggered manually from the Actions tab (`workflow_dispatch`).

---

## Running locally

### Prerequisites

- .NET 8 SDK
- A SQL Server instance (LocalDB, Docker, or your own) — the production database is Azure SQL, but anything SQL Server-compatible works for local dev.

### Steps

```bash
# 1. Clone
git clone https://github.com/Ynot1996/MyPocketSystem.git
cd MyPocketSystem

# 2. Set the connection string via user-secrets (preferred) or appsettings.Development.json
dotnet user-secrets init --project MyPocket.Web
dotnet user-secrets set "ConnectionStrings:MyPocketDBConnection" \
  "Server=(localdb)\\mssqllocaldb;Database=MyPocketDB;Trusted_Connection=True;" \
  --project MyPocket.Web

# 3. Run — DbInitializer will create the schema and seed admin + default categories
dotnet run --project MyPocket.Web
```

The app binds to `http://localhost:5244` by default. First launch seeds the admin account plus 16 default categories and the 5 subscription plans.

In production, the connection string is injected as the App Service environment variable `ConnectionStrings__MyPocketDBConnection` (double underscore — the standard .NET config override).

---

## Default credentials (demo only)

The seeder in [`DbInitializer`](MyPocket.DataAccess/Data/DbInitializer.cs) creates:

- **Admin** — `admin@example.com` / `12345678`
- **Free member** — `freemember@example.com` / `12345678`

These are placeholders for local development and the public demo. Rotate them before any real use.

---

## Deployment

The step-by-step Azure setup (resource group, SQL Server, App Service, OIDC trust, GitHub secrets) is documented in [DEPLOY_AZURE.md](DEPLOY_AZURE.md).

Current production target:

- Resource group: `mypocket-rg`
- SQL Server: `mypocket-db-server` (Serverless Free offer, SE Asia region)
- App Service: `mypocket-web-app` (Linux F1 free tier)

---

## Project layout

```
MyPocketSystem/
├── .github/workflows/deploy.yml        # CI/CD (test → publish → OIDC → Azure)
├── MyPocket.Core/Models/               # Entity POCOs (Transaction, Category, Budget, …)
├── MyPocket.DataAccess/Data/
│   ├── MyPocketDBContext.cs            # EF Core fluent config
│   └── DbInitializer.cs                # Seed + idempotent ALTER TABLE on startup
├── MyPocket.Services/                  # Business logic (6 services)
│   └── Interfaces/                     # DI contracts
├── MyPocket.Shared/
│   ├── Resources/
│   │   ├── JsonLocalizationService.cs  # Culture-aware string lookup + goal-name decoding
│   │   └── locales/{en-US,zh-TW}.json
│   ├── ViewModels/                     # Form / display models
│   ├── DTOs/
│   └── Validation/                     # Localized validation attributes
├── MyPocket.Web/
│   ├── Program.cs                      # DI, localization, EF retry policy, seed-with-retry
│   ├── Areas/{User,Admin}/             # MVC areas with their own controllers + views
│   ├── Controllers/                    # Account, Home, Language, Currency
│   ├── Services/
│   │   ├── CurrencyService.cs          # 6-currency metadata + formatter
│   │   ├── ExchangeRateService.cs      # Static reference rates
│   │   └── CategoryIcons.cs            # Category → Bootstrap Icon mapping
│   └── Views/Shared/                   # _LayoutUser/_LayoutAdmin, _FlashMessages,
│                                       # _LanguageSwitcher, _CurrencySwitcher
├── MyPocket.Tests/                     # xUnit + EF Core InMemory (31 tests)
├── DEPLOY_AZURE.md                     # Step-by-step cloud setup
└── README.md
```

---

## License

See [LICENSE.txt](LICENSE.txt).
