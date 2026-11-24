# MyPocket: Gamified, AI-assisted personal finance

A full-stack personal finance web app built with **ASP.NET Core 8 (MVC)** and **Entity Framework Core**, deployed to **Azure App Service**. A **GitHub Actions** pipeline tests every pull request and deploys `main`.

**🌐 Live demo:** <https://mypocket-web-app.azurewebsites.net/>. Click **Try it now** to use it without signing up.

MyPocket is built for people who live across currencies (international students, expats, frequent travellers). Logging a transaction takes seconds: snap a receipt, say it out loud, or type `coffee 3.5 GBP`. Every entry earns coins and grows an **isometric city**, where each spending category is a building. The **Insights** page explains where the money went in plain language, and Premium members get an **AI coach**.

The UI is bilingual (English / 繁體中文) and supports six currencies (GBP, USD, EUR, TWD, JPY, KRW).

---

## Table of contents

- [Feature highlights](#feature-highlights)
- [Architecture](#architecture)
- [Technology stack](#technology-stack)
- [Engineering highlights](#engineering-highlights-for-recruiters)
- [Testing](#testing)
- [CI / CD](#ci--cd)
- [Running locally](#running-locally)
- [Configuration](#configuration)
- [Default credentials](#default-credentials-demo-only)
- [Deployment](#deployment)
- [Project layout](#project-layout)

---

## Feature highlights

### Onboarding
- **Try first, sign up later.** `/try` creates a real guest account with a week of multi-currency demo data. It is rate-limited to 5 per hour per IP.
- A banner reminds guests that **registering or linking Google keeps their data**. Upgrading converts the same database row in place, so nothing is copied or lost. Logging into an existing account merges the guest's data into it.
- **Google Sign-In** through ASP.NET Core external authentication. Guest accounts older than 7 days are cleaned up on startup.

### Capture (the fastest path from spending to a record)
- **Receipt scan.** Take a photo (it opens the rear camera on phones) and Claude's vision model extracts the amount, currency, date, merchant and category into the form. Images are processed in memory and never stored.
- **Voice input** uses the browser's Web Speech API. The transcript goes through the same parser as typed text.
- **Natural-language entry** like `lunch 12.5 pounds yesterday`. If the AI is not configured or unavailable, a **rule-based parser** (currency symbols and words, keyword-to-category matching, en/zh) handles it, so the feature never hard-fails.

### Gamification
- **Coins:** +10 per entry (daily cap of 10 entries), +5 for scanned or spoken entries, plus a **streak bonus** on the first entry of each day. Coins are stored in an append-only ledger.
- **City:** one building per category. Buildings level up by **number of entries, not amount spent**, so the game rewards the tracking habit rather than spending. It is rendered as isometric SVG with day/dusk/night skies, level-up animations and walking residents.
- **Shop:** buy decorations, residents and outfits with coins.

### Insights
- KPIs, a category doughnut, a six-month income/expense trend, a per-currency breakdown, and **plain-language highlights** such as "Dining is 38% of your spending".
- Free members see the conclusions only; the calculation logic stays server-side. **The AI coach is Premium-only.** It receives monthly category totals, never individual transactions, and returns at most three actionable tips.

### Core finance
- Transactions CRUD with per-row currency, budgets with progress, saving goals, and custom categories.
- Cross-currency totals use reference exchange rates.
- Subscription plans (Free / Premium).
- Admin area: users, bulk subscription updates, categories and announcements.

---

## Architecture

```
MyPocketSystem.sln
├── MyPocket.Core         Domain entities (User, Transaction, CoinLedgerEntry, UserItem, …)
├── MyPocket.DataAccess   EF Core DbContext + seeding + idempotent startup DDL
├── MyPocket.Services     Business logic behind interfaces
│   ├── Accounts/         Guest create / upgrade / merge / cleanup
│   ├── Gamification/     Coin ledger, streaks, city model, shop catalog
│   └── Ai/               Claude client, receipt scanner, text parser, spending coach
├── MyPocket.Shared       ViewModels, localization service + en/zh JSON
├── MyPocket.Web          MVC host: controllers, Razor views, JS (city, capture)
├── MyPocket.API          REST API project (for the future mobile client)
├── MyPocket.Mobile       .NET MAUI shell (starter, not deployed)
└── MyPocket.Tests        xUnit tests (71) on EF Core InMemory
```

Dependencies point one way: `Web` → `Services` → `DataAccess` → `Core`.

Key design decisions:

- **AI behind an interface with a deterministic fallback.** `IAiJsonClient` wraps the Anthropic SDK. Every AI feature degrades gracefully: the text parser falls back to rules, the scanner hides itself when the API is not configured, and insights are computed without AI. Tests use a fake client, so the suite never calls the network.
- **Structured outputs, not prompt-and-pray.** Each AI call sends a JSON Schema (`additionalProperties: false`), and the result is validated again server-side. For example, a category must be one of the user's real categories, and the currency must be supported.
- **Guest = real user row (`IsGuest`).** Every feature works for guests unchanged, and upgrading is an `UPDATE`, not a migration of data out of localStorage.
- **Append-only coin ledger.** The balance is the sum of the entries, so it is auditable and race-tolerant, and there is no mutable balance column to drift.
- **Services return result keys, not strings.** Controllers localize them, which keeps services language-agnostic and testable.

---

## Technology stack

| Layer | Choice |
|---|---|
| Runtime | .NET 8 (`TimeProvider` for testable clocks) |
| Web | ASP.NET Core MVC, Razor views, ViewComponents |
| ORM / DB | EF Core 9 → Azure SQL Serverless (prod), InMemory (tests and zero-config local) |
| Auth | Cookie auth + BCrypt, Google OAuth, guest accounts, cookie re-validation |
| AI | Anthropic C# SDK: Claude with structured JSON output, vision (receipts), server-side model fallback |
| Voice | Web Speech API (in-browser speech-to-text) |
| Abuse / cost control | ASP.NET Core rate limiter (guest creation, AI calls), `IMemoryCache` for advice |
| Observability | Application Insights (custom events: GuestStarted, TransactionCreated, ReceiptScanned, …), structured logging, `/healthz` |
| Front end | Bootstrap 5, hand-written isometric SVG renderer, Chart.js 4 |
| i18n | Custom JSON localization, en-US / zh-TW |
| CI / CD | GitHub Actions: tests on every PR, OIDC deploy to App Service from `main` |

---

## Engineering highlights (for recruiters)

1. **Guest mode with in-place upgrade and merge** ([`AccountService`](MyPocket.Services/Accounts/AccountService.cs)). This is the same idea as Firebase anonymous-account linking: try instantly, keep your data when you sign up, and when you log into an existing account the guest's records are merged with de-duplication.
2. **LLM integration built for production** ([`MyPocket.Services/Ai`](MyPocket.Services/Ai)):
   - Structured JSON output with schema validation.
   - Refusal handling.
   - Low effort for cheap extraction.
   - Server-side model fallbacks.
   - A configurable model.
   - Per-user daily rate limits and a 6-hour advice cache.
   - A rule-based fallback that keeps the feature working with no API key.
3. **Privacy by design.** Receipt images are never persisted, and the coach sees only aggregates.
4. **Gamification that doesn't reward spending.** Levels are count-based, coins have a daily cap, and streaks are computed from the ledger. It is covered by 16 tests that use a fixed `TimeProvider`.
5. **Security fixes found in review.** These included an anonymous-reachable admin controller, over-posting on admin user edit, CSRF gaps, a hardcoded DB password in config, and stale cookies that outlived deleted or cleaned-up users (now re-validated every 5 minutes, see [`CookieUserValidator`](MyPocket.Web/Infrastructure/CookieUserValidator.cs)).
6. **Resilient Azure SQL Serverless.** Retry-on-failure and a backoff loop around seeding handle cold starts. Guarded startup DDL reshapes already-deployed databases.
7. **Observability.** Product events feed Application Insights (a funnel from guest started to registered to transactions created), and there is a `/healthz` endpoint for App Service health checks.
8. **CI that gates merges.** Pull requests run the test suite, and only `main` deploys.

---

## Testing

```bash
dotnet test MyPocket.Tests/MyPocket.Tests.csproj
```

The suite has **71 tests**:

| Area | Test files | What they cover |
|---|---|---|
| Services | `Category`, `Transaction`, `Budget`, `SavingGoal`, `Subscription` | CRUD, including ownership checks |
| Gamification | `GamificationServiceTests` | Caps, streaks across midnight, city levels |
| Shop | `ShopServiceTests` | Purchases and outfit toggling |
| Accounts | `AccountServiceTests` | Guest create, upgrade, merge and cleanup |
| AI | `AiCaptureTests` | AI path vs fallback path, hallucinated categories rejected, coach insights |

- Each test gets a fresh EF Core InMemory database.
- AI tests use a `FakeAiClient`, so they never touch the network.

---

## CI / CD

[`.github/workflows/deploy.yml`](.github/workflows/deploy.yml):

- **Pull requests:** restore, then `dotnet test`. A red test blocks the merge.
- **Push to `main`:** run the tests, then `dotnet publish`, then `azure/login@v2` (OIDC, no publish profile), then `azure/webapps-deploy@v3`.

---

## Running locally

```bash
git clone https://github.com/Ynot1996/MyPocketSystem.git
cd MyPocketSystem
dotnet run --project MyPocket.Web --launch-profile http
```

It runs at `http://localhost:5244`.

**No database needed.** When no connection string is configured, the app uses an in-memory database that resets on restart. To use SQL Server instead:

```bash
dotnet user-secrets set "ConnectionStrings:MyPocketDBConnection" "<your connection string>" --project MyPocket.Web
```

---

## Configuration

All settings are optional. Each feature switches off cleanly when its setting is missing.

| Setting (App Service env var form) | Enables |
|---|---|
| `ConnectionStrings__MyPocketDBConnection` | SQL Server instead of InMemory |
| `Authentication__Google__ClientId` / `Authentication__Google__ClientSecret` | Google Sign-In |
| `Ai__AnthropicApiKey` (or `ANTHROPIC_API_KEY`) | Receipt scan, AI text parsing, AI coach |
| `Ai__Model` | Overrides the default `claude-opus-5-5`, e.g. `claude-haiku-5-5` for lower cost |
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | Telemetry and traffic monitoring |

Locally, use `dotnet user-secrets set "<Key:With:Colons>" "<value>" --project MyPocket.Web`. Never commit secrets.

---

## Default credentials (demo only)

- **Admin:** `admin@example.com` / `12345678`
- **Free member:** `freemember@example.com` / `12345678`

These are placeholders for local development and the public demo.

---

## Deployment

See [DEPLOY_AZURE.md](DEPLOY_AZURE.md) for the resource group, SQL Server, App Service, OIDC trust and GitHub secrets.

---

## Project layout

```
MyPocketSystem/
├── .github/workflows/deploy.yml          # PR: test · main: test → publish → OIDC → Azure
├── MyPocket.Core/Models/                 # Entities incl. CoinLedgerEntry, UserItem
├── MyPocket.DataAccess/Data/             # DbContext, DbInitializer (seed + startup DDL)
├── MyPocket.Services/
│   ├── Accounts/AccountService.cs        # Guest lifecycle
│   ├── Gamification/                     # GamificationService, ShopService, City/Shop catalogs
│   ├── Ai/                               # AiJsonClient, CaptureServices, SpendingCoach
│   └── Interfaces/
├── MyPocket.Shared/
│   ├── Resources/locales/{en-US,zh-TW}.json
│   └── ViewModels/{Gamification,Capture}/
├── MyPocket.Web/
│   ├── Program.cs                        # DI, auth, rate limits, telemetry, health checks
│   ├── Infrastructure/                   # Claims, rate-limit policies, telemetry, cookie validator
│   ├── ViewComponents/CoinBadge…         # Nav coin counter
│   ├── Areas/User/Controllers/           # Transactions (dashboard), Capture, Insights, Shop, …
│   └── wwwroot/
│       ├── css/app.css                   # v2 design system
│       └── js/{city,capture,transaction-form}.js
├── MyPocket.Tests/                       # 71 xUnit tests
└── DEPLOY_AZURE.md
```

---

## License

See [LICENSE.txt](LICENSE.txt).
