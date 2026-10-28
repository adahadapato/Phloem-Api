# Phloem API

The backend for **Phloem** (live at [phloemmarket.com](https://phloemmarket.com)), a marketplace for African food from home cooks and
food businesses in **Ghana, Nigeria and the United Kingdom**, with a parcel service for sending food between countries
(e.g. Ghana → UK).

One API serves both front ends:

| Front end | Repository | Talks to |
|---|---|---|
| Website (React + Vite) | `adahadapato/Phloem-web` | `https://phloemapi.phloemmarket.com` |
| Mobile app (.NET MAUI) | `adahadapato/phloem-mobile` | `https://phloemapi.phloemmarket.com` |

Production base address: **`https://phloemapi.phloemmarket.com`** · local: **`https://localhost:7103`**

---

## Contents

1. [What the API does](#1-what-the-api-does)
2. [Tech stack](#2-tech-stack)
3. [Architecture](#3-architecture)
4. [Solution layout](#4-solution-layout)
5. [Getting started (local development)](#5-getting-started-local-development)
6. [Configuration](#6-configuration)
7. [Database, migrations and seed data](#7-database-migrations-and-seed-data)
8. [Authentication, roles and security](#8-authentication-roles-and-security)
9. [API reference](#9-api-reference)
10. [Errors](#10-errors)
11. [Caching](#11-caching)
12. [Files, images and email](#12-files-images-and-email)
13. [Terms acceptance](#13-terms-acceptance)
14. [Background jobs](#14-background-jobs)
15. [Deployment (CI/CD)](#15-deployment-cicd)
16. [Adding a feature: the checklist](#16-adding-a-feature-the-checklist)
17. [Troubleshooting](#17-troubleshooting)
18. [Roadmap](#18-roadmap)

---

## 1. What the API does

**Buyers**
- Browse dishes and shops by country and category, search with **predictions while typing**, and see dish pages.
- See **Home suggestions**: "Order again", "You might like" (each card with its reason) and "Popular in Ghana".
- Keep a basket, check it against today's prices and opening times, choose a delivery time and address, and place
  orders. A basket with dishes from several shops becomes one order per shop.
- Pay (a **test payment** is switched on until a payment provider is connected), cancel within the allowed time,
  and use "Buy it again".
- Manage their account: profile, photo, delivery addresses (each with a recipient name and phone), notification
  switches, password and **two-factor sign-in**.

**Sellers**
- Open a shop (date of birth, home address and age 18+ required; must agree to the Seller terms), upload documents
  for approval, and set a logo, cover photo, delivery fee (inside the country's range) and daily order cut-off.
- Add dishes from the **approved dish-name list** (or suggest a new name), with prices inside the admin-set range
  for that country, and photos (enhanced and resized automatically).
- Accept, decline, send out and mark orders as delivered; see daily sales.

**Parcels ("Send food to the UK")**
- See open routes and speeds, get a price quote, check **what can be sent**, book a parcel, pay once it's weighed,
  cancel before it leaves, and track it (public tracking page by tracking number).
- Hub staff receive and weigh parcels and move them along their journey.

**Admin**
- Shops: approve, reject, verify, block, review documents (through signed, expiring links).
- Users: search, suspend / reinstate, give and take roles (an admin can't remove their own Admin role or the last admin's).
- Dish names: approve suggestions, set price ranges per country.
- Pricing: Phloem's commission per country, shop delivery-fee ranges, delivery times and their extra charges.
- Shipping: routes, drop-off hubs, rates per speed, switching routes on and off, the "what can be sent" list.
- Texts: publish new versions of the **Terms & Conditions**, **Privacy Policy** and **Help** page (shown by the
  website and the app straight from the database), and see how many people have accepted the current Terms.
- **Contact us** inbox (admins and support agents).

---

## 2. Tech stack

| Area | Choice | Version |
|---|---|---|
| Runtime | .NET | 10 |
| Web | ASP.NET Core **minimal APIs** | 10 |
| Architecture | Clean architecture: Domain → Application → Persistence / Infrastructure → Presentation | |
| CQRS / pipeline | MediatR | 14.2.0 |
| Validation | FluentValidation | 12.2.0 |
| Database | SQL Server with Entity Framework Core | 10.0.x |
| Identity | ASP.NET Core Identity (EF stores) + JWT bearer tokens + refresh tokens | 10.0.x |
| External sign-in | Google (ready; switched off until the keys are added) | 10.x |
| Caching | `HybridCache` (in memory now; a shared cache such as Redis can be added without code changes) | 10.11.0 |
| Email | MailKit (SMTP) | 4.18.1 |
| Images | Magick.NET Q8 (AnyCPU) | 15.0.0 |
| Logging | Serilog (console + rolling files) | 10.0.0 |
| API docs | OpenAPI document + Swagger UI | 10.x / 10.3.0 |
| Rate limiting | ASP.NET Core built-in rate limiter | 10 |
| Hosting | IIS on SmarterASP.NET (self-contained, win-x86) | |
| CI/CD | GitHub Actions | |

---

## 3. Architecture

### 3.1 Layers and the dependency rule

```
                ┌──────────────────────────────┐
                │   Phloem.Presentation        │  minimal-API endpoints, auth, CORS, OpenAPI,
                │   (the web host)             │  rate limiting, ProblemDetails, Program.cs
                └──────────────┬───────────────┘
                               │ sends commands / queries (MediatR)
                ┌──────────────▼───────────────┐
                │   Phloem.Application         │  commands, queries, handlers, validators, DTOs,
                │                              │  pipeline behaviours, caching, TermsGate
                └──────────────┬───────────────┘
                               │ uses interfaces only
                ┌──────────────▼───────────────┐
                │   Phloem.Domain              │  entities, value objects, enums, rules,
                │   (no dependencies)          │  Result/Error, repository & service interfaces
                └──────────────▲───────────────┘
                               │ implement the Domain's interfaces
     ┌─────────────────────────┴───────────────────────────┐
┌────┴──────────────────┐                     ┌────────────┴───────────────┐
│ Phloem.Persistence    │                     │ Phloem.Infrastructure      │
│ EF Core DbContext,    │                     │ Identity/JWT, email (SMTP),│
│ configurations,       │                     │ file storage, images,      │
│ repositories,         │                     │ settings, hosted services  │
│ migrations, seeding   │                     │                            │
└───────────────────────┘                     └────────────────────────────┘
```

- **Domain** knows nothing else. Business rules live in entities (`Order.Place`, `Shipment.Book`, `Vendor.Open`,
  `Dish.ChangePrice`…) and return `Result` / `Result<T>` instead of throwing for expected failures.
- **Application** orchestrates: one handler per use case, talking only to Domain interfaces
  (`IOrderRepository`, `IEmailSender`, `ICurrentUser`, `IFileStorage`…).
- **Persistence** and **Infrastructure** implement those interfaces. Nothing above them references EF Core, MailKit,
  Magick.NET or Identity directly.
- **Presentation** maps HTTP to MediatR and `Result` to HTTP. Endpoints contain no business logic.

### 3.2 What happens to a request

```
HTTP request
  → Serilog request logging → exception handler → HTTPS redirect (not in Development)
  → static files (/uploads) → OpenAPI/Swagger → CORS → authentication/authorization → rate limiter
  → endpoint (Presentation/Endpoints/…)              e.g. POST /api/orders
  → ISender.Send(command)
      → LoggingBehavior            logs the request and how long it took
      → ValidationBehavior         FluentValidation → 400 with field errors
      → CachingBehavior            [Cacheable] queries are served from HybridCache
      → CacheInvalidationBehavior  [InvalidatesCache] commands clear tags after success
      → Handler                    loads aggregates → calls Domain methods → saves (IUnitOfWork)
  → Result → .ToHttp() → 200/201/204, or ProblemDetails (400/401/403/404/409/429)
```

### 3.3 Conventions worth knowing

- **Result pattern.** Handlers return `Result`/`Result<T>`; `ResultExtensions.ToHttp()` turns an `Error` into
  ProblemDetails with a stable `code` (e.g. `Order.NotFound`). Exceptions are for bugs only.
- **Errors live in one place per area**: `Phloem.Domain/Shared/Errors/*Errors.cs`.
- **Every configuration key** is listed in `Phloem.Domain/Constants/AppSettings.cs`; every cache tag in
  `Phloem.Application/Common/Caching/CacheTags.cs`; every role in `Phloem.Domain/Constants/Roles.cs`.
- **Soft delete** on every entity (`IsDeleted`, `DeletedAt`) with a global query filter; `CreatedAt` / `UpdatedAt`
  are set by an interceptor.
- **Enums travel as names** in JSON (`"Ghana"`, `"TodayEvening"`); numbers are still accepted.
- **Who "me" is** always comes from the access token (`ICurrentUser`), never from the URL or body.

---

## 4. Solution layout

```
Phloem.sln
├── Phloem.Domain/
│   ├── Constants/          AppSettings (all config keys), Roles
│   ├── Entities/           UserProfile, Vendor, Dish, DishCatalog, Order, Shipment, ShipmentRoute, LegalDocument,
│   │                       TermsAcceptance, ContactMessage, ParcelItemRule, Commission, DeliveryPricing, …
│   ├── Enums/              Country, Currency, OrderStatus, ShipmentStatus, DeliveryWindow, LegalDocumentType, …
│   ├── Interfaces/         repositories, IEmailSender, IFileStorage, ICurrentUser, IAuthService, …
│   ├── Models/             small read models (PurchasedDish, DishPairCount, ContactDetails, …)
│   ├── Shared/Errors/      one *Errors.cs per area
│   ├── Shared/Results/     Result, Result<T>, Error, PagedResult, PageRequest
│   └── ValueObjects/       EmailAddress, PhoneNumber, ResidentialAddress, ImageFile, DocumentFile, Url
├── Phloem.Application/
│   ├── Common/Behaviours/  Logging, Validation, Caching, CacheInvalidation
│   ├── Common/Caching/     [Cacheable], [InvalidatesCache], CacheTags
│   ├── Common/Terms/       TermsGate (Terms acceptance rules)
│   └── Features/<Area>/    Commands/ Queries/ Dtos/ Validators/  — Accounts, Auth, Commissions, Contact,
│                           Countries, DeliveryPricings, DishCatalogs, Dishes, LegalDocuments, Orders,
│                           ParcelItemRules, PriceRanges, ShipmentRoutes, Shipments, Suggestions, Vendors
├── Phloem.Persistence/
│   ├── Configurations/     one IEntityTypeConfiguration per entity (+ EntityConfiguration base)
│   ├── Repositories/       EF implementations of the Domain repositories
│   ├── Migrations/         EF Core migrations (applied automatically at start-up)
│   ├── Seeding/            DbSeeder, StarterDataSeeder, DemoDataSeeder, Data/*.json
│   ├── Interceptors/       audit timestamps
│   └── PhloemDbContext.cs
├── Phloem.Infrastructure/
│   ├── Services/Identity/  AuthService, AccountService, UserRoleService (ASP.NET Core Identity)
│   ├── Services/Accounts/  SuspensionExpiryService (hourly hosted service)
│   └── Settings/           Application, Contact, Email (SMTP), Identity (JWT, lockout, passwords, links),
│                           Images (Magick.NET), Storage (local disk + signed private links)
├── Phloem.Presentation/    the web host
│   ├── Endpoints/<Area>/   minimal-API endpoint groups
│   ├── Extensions/         EndpointExtensions (MapPhloemEndpoints), ResultExtensions (ToHttp), storage
│   ├── Middleware/         GlobalExceptionHandler
│   ├── Settings/           Authentication, CORS, OpenAPI, RateLimiting
│   ├── appsettings*.json
│   ├── web.config          IIS hosting
│   └── Program.cs
└── Phloem.Worker/          reserved for scheduled jobs (empty for now)
```

---

## 5. Getting started (local development)

### 5.1 Prerequisites

- **.NET 10 SDK** — `dotnet --version` should print `10.x`.
- **SQL Server**: LocalDB (comes with Visual Studio), SQL Server Express/Developer, or a Docker container.
- **EF Core tools**: `dotnet tool install --global dotnet-ef` (or `dotnet tool restore` if a local tool manifest is used).
- Optional, for email: a local test inbox such as **smtp4dev** (`dotnet tool install -g Rnwood.Smtp4dev`, then
  `smtp4dev`). The default settings already point at `localhost:25`, so every email lands there and nothing reaches
  real people.
- Visual Studio 2022 17.14+/2026, Rider, or VS Code with C# Dev Kit.

### 5.2 Clone and restore

```bash
git clone https://github.com/adahadapato/Phloem-Api.git
cd Phloem-Api
dotnet restore Phloem.sln
```

### 5.3 Secrets (User Secrets — never in appsettings.json)

The secret values are empty placeholders in `appsettings.json`. Set them once for your machine:

```bash
cd Phloem.Presentation

dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\\MSSQLLocalDB;Database=Phloem;Trusted_Connection=True;TrustServerCertificate=True"
dotnet user-secrets set "Jwt:Key"                    "<at least 32 random characters>"
dotnet user-secrets set "Storage:Local:SigningKey"   "<another 32+ random characters, different from Jwt:Key>"
dotnet user-secrets set "SeedData:AdminPassword"     "<password for the first admin account>"

# optional
dotnet user-secrets set "SeedData:DemoOwnerPassword" "<lets you sign in as the demo sellers>"
dotnet user-secrets set "Email:Password"             "<SMTP password, if you use a real mail server>"
dotnet user-secrets set "Authentication:Google:ClientId"     "<when Google sign-in is switched on>"
dotnet user-secrets set "Authentication:Google:ClientSecret" "<when Google sign-in is switched on>"
```

A quick way to make a random key: `openssl rand -base64 48` (or PowerShell:
`[Convert]::ToBase64String((1..48 | % { Get-Random -Max 256 }))`).

### 5.4 Run

```bash
dotnet run --project Phloem.Presentation --launch-profile https
```

| What | Where |
|---|---|
| API (https) | `https://localhost:7103` |
| API (http) | `http://localhost:5175` (used by the Android emulator for pictures) |
| Swagger UI | `https://localhost:7103/swagger` (Development only: `OpenApi:Enabled` is `true` there) |
| OpenAPI JSON | `https://localhost:7103/openapi/v1.json` |
| Health | `https://localhost:7103/health` |

On start-up the API **applies any pending migrations and seeds the database** (first admin, starter data, and demo
shops if `SeedData:DemoDataEnabled` is `true`). Sign in with `SeedData:AdminEmail` and the password you set.

Trust the development certificate once if your browser complains: `dotnet dev-certs https --trust`.

### 5.5 Running the front ends against your local API

- **Website**: `npm run dev` in `Phloem-web` uses `.env.development` → `https://localhost:7103`. The local origins
  `http(s)://localhost:5173` are already allowed by CORS in `appsettings.Development.json`.
- **Mobile app**: Debug builds read `Settings/appsettings.Development.json` (see the app's README); the Android
  emulator reaches your PC at `10.0.2.2`.

---

## 6. Configuration

All keys are documented in `Phloem.Domain/Constants/AppSettings.cs`. Settings are **checked at start-up**
(`ValidateOnStart`), so a missing or invalid value stops the app immediately with a clear message.

| Section | What it controls | Secrets in it |
|---|---|---|
| `AppSettings` | `ApiBaseUrl`, `WebsiteBaseUrl` (no trailing slash) — used for file links and links in emails | — |
| `ConnectionStrings` | `DefaultConnection` (SQL Server) | **yes** |
| `Jwt` | `Key` (32+ chars), `Issuer`, `Audience`, `ExpiryMinutes` (15), `RefreshTokenDays` (30), `TwoFactorChallengeMinutes` (5) | `Key` |
| `Identity` | password rules (8+ chars, digit, upper, lower), lockout (5 tries → 15 min), email-confirmation and reset-link paths, authenticator issuer | — |
| `Authentication` | `AppCallbackUrl` (`phloem://auth`), `LoginCodeSeconds`, `Google:ClientId/ClientSecret` | Google secret |
| `Email` | `FromName`, `FromAddress`, `ReplyTo:{Support,Sellers,Shipping}`, `TeamInbox`, SMTP `Host`/`Port`/`Security`/`Username`/`Password` | `Password` |
| `Contact` | Contact us page: `Phone`, `WhatsApp`, `Address`, `OpeningHours`, `ReplyTime` (empty = hidden). The email shown is `Email:ReplyTo:Support` | — |
| `Cors` | `AllowedOrigins` (the website) | — |
| `Storage` | `Provider` (`Local`), public/private folders, `PrivateDownloadPath`, `SigningKey` | `SigningKey` |
| `Images` | max upload sizes (dish 10 MB, other 5 MB), resize, WebP quality, blur check, dish photo enhancement | — |
| `DataProtection` | application name and key folder (`App_Data/keys`) | — |
| `SeedData` | first admin (`AdminEmail`, `AdminPassword`, name, country), starter data and demo data switches/paths | `AdminPassword`, `DemoOwnerPassword` |
| `Payments` | `TestPaymentsEnabled` (the temporary "test payment" endpoints) | — |
| `OpenApi` | `Enabled` (off in production), title, paths, contact | — |
| `Serilog` | log levels and sinks (`logs/phloem-.log`, kept 14 days) | — |

**Files**

| File | Used | Notes |
|---|---|---|
| `appsettings.json` | always | committed; secrets are `""` placeholders |
| `appsettings.Development.json` | `ASPNETCORE_ENVIRONMENT=Development` | local API address, Swagger on, local CORS origins; **removed from the published output** |
| `appsettings.Production.json` | production | committed with `""` placeholders; the CI pipeline writes the secrets into the published copy |
| User Secrets | Development | `UserSecretsId` is in `Phloem.Presentation.csproj` |
| Environment variables | anywhere | double underscore for `:` — e.g. `Email__Password` |

> ⚠️ **Before going live:** the `Contact` section and the `Email` addresses (`no-reply@phloem.app`,
> `support@phloem.app`, `team@phloem.app`…) are **placeholders**. Replace them with the real details (the site is on
> `phloemmarket.com`) and the real SMTP server.

---

## 7. Database, migrations and seed data

### 7.1 Migrations

Migrations live in `Phloem.Persistence/Migrations` and are **applied automatically when the API starts**
(`DbSeeder.SeedDatabaseAsync`).

| Migration | What it adds |
|---|---|
| `Initial` | the core schema: identity, profiles, shops, dishes, dish list, orders, shipments, routes, pricing |
| `PersonalDetails` | date of birth and home address on profiles |
| `UserSuspension` | suspending and reinstating users |
| `ShopCutoffAndDecline` | shop order cut-off time; declining orders |
| `AddLegalDocuments` | Terms, Privacy Policy and Help texts (versioned) |
| `AddParcelItemRules` | the "what can be sent" list |
| `AddressRecipient` | recipient name and phone on delivery addresses |
| `AddContactMessages` | the Contact us inbox |
| `AddTermsAcceptances` | proof of who accepted which Terms / saw which Privacy Policy |

Add one after changing an entity or a configuration:

```bash
dotnet ef migrations add <Name> --project Phloem.Persistence --startup-project Phloem.Presentation
dotnet ef database update     --project Phloem.Persistence --startup-project Phloem.Presentation   # optional: start-up does it too
```

If the API refuses to start with **`PendingModelChangesWarning`**, the model changed but no migration was added
(or a migration was created before the code changed): add it (or remove and re-add the last one with
`dotnet ef migrations remove`).

### 7.2 Seed data

| Seeder | When | What |
|---|---|---|
| First admin | the admin email doesn't exist yet | an account with the `Admin` role (`SeedData:Admin*`) |
| `StarterDataSeeder` (`Seeding/Data/starter-data.json`) | `SeedData:StarterDataEnabled`; **each block only when its table is empty** | commissions, delivery pricing and delivery-time charges, the dish-name list with price ranges, shipping routes and rates, the "what can be sent" list, and the first Terms / Privacy / Help texts |
| `DemoDataSeeder` (`Seeding/Data/demo-data.json`) | `SeedData:DemoDataEnabled`; once per demo shop | demo sellers with approved shops, logos, cover photos, delivery fees, ratings and dishes with photos |

Because blocks only run on empty tables, anything an admin changes or deletes is **never brought back** by a
restart. **Turn `DemoDataEnabled` off before launch.**

---

## 8. Authentication, roles and security

### 8.1 Signing in

| Step | Endpoint | Notes |
|---|---|---|
| Sign up | `POST /api/auth/register` | creates the login and the profile in one transaction; records Terms + Privacy acceptance |
| Sign in | `POST /api/auth/login` | returns tokens, or a two-factor challenge |
| Two-factor | `POST /api/auth/login/two-factor` | authenticator code or a recovery code |
| Renew | `POST /api/auth/refresh` | refresh tokens rotate; a reused or revoked token is refused |
| Sign out | `POST /api/auth/logout` | revokes the refresh token |
| Google | `GET /api/auth/external/google` → callback → `POST /api/auth/external/exchange` | the app receives a short-lived login code at `phloem://auth`; switched on when `Authentication:Google` is set |

- **Access tokens** (JWT) last **15 minutes**; **refresh tokens** last **30 days**.
- **Lockout** after 5 wrong passwords, for 15 minutes. Passwords: 8+ characters with a digit, an upper-case and a
  lower-case letter.
- Email confirmation and password reset links point at the website (`/confirm-email`, `/reset-password`); they rely
  on the data-protection keys in `App_Data/keys`, which must survive deployments.

### 8.2 Roles

| Role | Who | Can |
|---|---|---|
| `Customer` | everyone who signs up | buy, send parcels, manage their account |
| `Seller` | given automatically when an admin **approves** the shop; removed if blocked/closed | manage their shop, dishes and orders |
| `Admin` | staff | everything under `/api/admin/*` |
| `CatalogManager` | staff | reserved for dish-list work (the dish-list endpoints are Admin-only for now) |
| `ShipmentManager` | hub staff | `/api/staff/shipments`: receive, weigh, move parcels along |
| `SupportAgent` | support staff | the Contact us inbox (`/api/admin/contact-messages`) |

Admins give and take staff roles on **Admin → Users**. An admin can't take away their own Admin role, and the last
admin can't lose it.

### 8.3 Other protections

- **CORS**: only the origins in `Cors:AllowedOrigins`.
- **Rate limit**: `POST /api/contact/messages` — 5 per 10 minutes per connection (429 with a friendly message).
  A hidden "website" field on the form catches robots.
- **Private documents** (shop registration, hygiene certificates, owner ID) are stored outside the public folder and
  served only through **signed, expiring `/private-files` links**.
- **Suspended users** can't sign in; suspensions with an end date are lifted automatically every hour.
- **Uploaded images** are re-encoded (WebP) and resized, so original files are never served as-is.

---

## 9. API reference

Everything is under **`/api`**. The full, always-current list with request/response shapes is in Swagger
(`/swagger` in Development). 🔒 = needs sign-in; 🛡 = needs a role.

<details>
<summary><b>Auth</b> — <code>/api/auth</code></summary>

| Method | Path | |
|---|---|---|
| POST | `/register` | sign up |
| POST | `/login` | sign in |
| POST | `/login/two-factor` | second step |
| POST | `/refresh` | new tokens |
| POST | `/logout` | sign out |
| POST | `/forgot-password` · `/reset-password` | password reset by email |
| POST | `/confirm-email` | confirm an email address |
| GET | `/external/providers` | which external sign-ins are on |
| GET | `/external/google` · `/external/google/callback` | Google sign-in |
| POST | `/external/exchange` | swap the app's login code for tokens |
</details>

<details>
<summary><b>Me</b> — <code>/api/me</code> 🔒</summary>

| Method | Path | |
|---|---|---|
| GET / PUT | `/` | my profile (includes `terms` status) / change name, country, phone |
| PUT | `/notifications` | notification switches |
| PUT | `/personal-details` | date of birth + home address |
| PUT / DELETE | `/photo` | profile photo |
| POST | `/addresses` · PUT/DELETE `/addresses/{id}` · PUT `/addresses/{id}/default` | delivery addresses |
| PUT | `/password` | change password |
| GET | `/two-factor` · `/two-factor/setup` | 2FA status / QR setup |
| POST | `/two-factor/enable` · `/two-factor/disable` · `/two-factor/recovery-codes` | 2FA |
| GET | `/suggestions?country=&take=` | Home: Order again + You might like |
| POST | `/terms/accept` · `/privacy/seen` | accept the latest Terms / acknowledge the Privacy notice |
</details>

<details>
<summary><b>Browsing</b> — countries, dishes, shops, dish list, delivery times, texts</summary>

| Method | Path | |
|---|---|---|
| GET | `/countries` | countries, currencies, flags, where parcels can go |
| GET | `/dishes?country=&category=&search=&shopId=&sort=&page=&pageSize=` | dish listing |
| GET | `/dishes/{id}` | a dish's page |
| GET | `/dishes/suggest?q=&country=&take=` | search predictions (dish names, then shops) |
| GET | `/dishes/popular?country=&take=` | "Popular in …" (signed out) |
| GET | `/shops` · `/shops/top` · `/shops/{id}` | shops |
| GET | `/dish-catalog` 🔒 · POST `/dish-catalog/suggestions` 🔒 | the dish-name list (sellers) / suggest a name |
| GET | `/delivery-windows?country=` | delivery times and their charges |
| GET | `/legal/{Terms\|Privacy\|Help}` | the current text |
| GET | `/parcel-rules` | what can be sent |
| GET | `/contact` · POST `/contact/messages` | Contact us details / send a message (rate limited) |
</details>

<details>
<summary><b>Orders</b> — <code>/api/orders</code> 🔒</summary>

| Method | Path | |
|---|---|---|
| POST | `/basket/check` | today's prices, stock, cut-offs and fees for a basket |
| POST | `/` | place orders (one per shop; needs the latest Terms) |
| GET | `/` | my orders (tabs, date, search, pages) |
| GET | `/recent-dishes` | "Buy it again" |
| GET | `/{orderNumber}` | order details |
| POST | `/{orderNumber}/payment` | prepare payment (today's prices) |
| POST | `/{orderNumber}/test-payment` | temporary test payment (`Payments:TestPaymentsEnabled`) |
| POST | `/{orderNumber}/cancel` | cancel |
| GET | `/{orderNumber}/buy-again` | what can go back in the basket |
</details>

<details>
<summary><b>My shop</b> — <code>/api/shops</code>, <code>/api/shops/mine/*</code> 🔒</summary>

| Method | Path | |
|---|---|---|
| POST | `/shops` | open a shop (`agreedToSellerTerms` required) |
| GET / PUT | `/shops/mine` | my shop / name, email, phone |
| PUT / DELETE | `/shops/mine/logo` · `/shops/mine/cover-photo` | pictures |
| POST | `/shops/mine/documents` | upload a document for approval |
| GET / PUT | `/shops/mine/delivery-fee` | my delivery fee (inside the country's range) |
| PUT | `/shops/mine/order-cutoff` | daily order cut-off |
| GET / POST | `/shops/mine/dishes` | my dishes / add one (needs the latest Terms) |
| PUT / DELETE | `/shops/mine/dishes/{id}` | change / remove |
| PUT | `/shops/mine/dishes/{id}/price` | new price (inside the range) |
| POST / DELETE | `/shops/mine/dishes/{id}/photos[/{photoId}]` | photos |
| GET | `/shops/mine/orders` · `/shops/mine/orders/stats` | incoming orders, daily sales |
| POST | `/shops/mine/orders/{n}/accept` · `/decline` · `/send-out` · `/delivered` | move an order along |
</details>

<details>
<summary><b>Parcels</b> — <code>/api/shipments</code>, <code>/api/staff/shipments</code></summary>

| Method | Path | |
|---|---|---|
| GET | `/shipments/routes` | open routes and speeds |
| POST | `/shipments/quote` | price estimate |
| GET | `/shipments/track/{trackingNumber}` | public tracking |
| POST 🔒 | `/shipments` | book (shipping terms ticked; recorded) |
| GET 🔒 | `/shipments` · `/shipments/{trackingNumber}` | my parcels |
| POST 🔒 | `/shipments/{trackingNumber}/cancel` · `/test-payment` | cancel / temporary test payment |
| GET 🛡 | `/staff/shipments` · `/staff/shipments/{n}` | hub list (ShipmentManager, Admin) |
| POST 🛡 | `/staff/shipments/{n}/receive` · `/progress` | weigh at the hub / next step |
</details>

<details>
<summary><b>Admin</b> — <code>/api/admin/*</code> 🛡 Admin (Contact messages: Admin or SupportAgent)</summary>

| Area | Endpoints |
|---|---|
| Shops | GET `/shops` · `/shops/pending` · `/shops/stats` · `/shops/{id}` · `/shops/{id}/documents/{docId}/link`; POST `/shops/{id}/approve` · `/reject` · `/documents/{docId}/review`; PUT `/shops/{id}/verified` · `/blocked` |
| Users | GET `/users`; POST `/users/{id}/suspend` · `/reinstate`; PUT/DELETE `/users/{id}/roles/{role}` |
| Dish names | GET `/dish-catalog` · `/dish-catalog/suggestions`; POST `/dish-catalog/{id}/approve` · `/reject`; PUT `/dish-catalog/{id}/price-ranges/{country}` |
| Commission | GET `/commissions`; PUT `/commissions/{country}` |
| Delivery pricing | GET `/delivery-pricing`; PUT `/delivery-pricing/{country}` · `/{country}/windows/{window}`; DELETE `/{country}/windows/{window}` |
| Shipping routes | GET/POST `/shipment-routes`; PUT `/{from}/{to}/drop-off` · `/active` · `/rates/{speed}`; DELETE `/{from}/{to}/rates/{speed}` |
| What can be sent | POST `/parcel-rules`; PUT `/parcel-rules/order` · `/parcel-rules/{id}`; DELETE `/parcel-rules/{id}` |
| Texts | POST `/legal/{type}` (publish a new version); GET `/legal/acceptance` |
| Contact messages | GET `/contact-messages`; PUT `/contact-messages/{id}/status` |
</details>

Plus `GET /health` (outside `/api`, not in Swagger) and signed `/private-files/…` links.

---

## 10. Errors

Every failure is a **ProblemDetails** (RFC 9457) body with a stable `code`:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.10",
  "title": "Conflict",
  "status": 409,
  "detail": "We've updated our Terms & Conditions. Please read and accept them to continue.",
  "code": "Terms.NotAccepted"
}
```

Validation failures (400) add `errors: { "Field": ["message", …] }`. `detail` is written for people and is shown
as-is by the website and the app; front ends act on `code`.

| Status | When |
|---|---|
| 400 | validation failed |
| 401 | not signed in, or the token ran out |
| 403 | signed in, but not allowed (role) |
| 404 | not found (or not yours) |
| 409 | a business rule (e.g. `Terms.NotAccepted`, `Terms.Outdated`, an order that can't be cancelled any more) |
| 429 | rate limit (`RateLimit.TooManyRequests`) |
| 500 | a bug — logged with full details; the response doesn't leak them |

---

## 11. Caching

Read-heavy, public queries are cached with `HybridCache` through `[Cacheable(tag, Minutes = …)]`; commands that
change the data carry `[InvalidatesCache(tag)]`. Tags and who uses them are documented in `CacheTags.cs`
(e.g. `Dishes` covers the listing, `GET /dishes/popular` and search predictions; `LegalDocuments` covers the texts).

**Never cached:** anything personal (orders, suggestions, profile), admin screens, checkout.

---

## 12. Files, images and email

- **Public files** (profile photos, shop logos and covers, dish photos and thumbnails): `wwwroot/uploads`, served
  at `/uploads/…`; links are absolute, built from `AppSettings:ApiBaseUrl`.
- **Private files** (shop documents): `wwwroot/private-uploads`, only through signed, expiring links.
- **Images**: Magick.NET converts to WebP, resizes (with thumbnails for cards), turns away blurry dish photos
  (`Images:BlurThreshold`) and lightly sharpens the rest (`Images` settings).
- **Email** (`IEmailSender`, SMTP via MailKit): from `Email:FromAddress`; the Reply-To is chosen per audience
  (`Support`, `Sellers`, `Shipping`). Team alerts go to `Email:TeamInbox`; Contact us emails reply straight to the
  customer. An email that fails is logged and never undoes the saved change.

> The folders `wwwroot/uploads`, `wwwroot/private-uploads`, `App_Data` and `logs` hold **runtime data** and must
> survive every deployment (the pipeline is set up to keep them).

---

## 13. Terms acceptance

`Application/Common/Terms/TermsGate.cs` holds the rules; every acceptance is a row in `TermsAcceptances`
(who, Terms or Privacy, version, where, reference, when) and is **never changed**.

| Where | Recorded as |
|---|---|
| Sign-up | Terms + Privacy (`SignUp`) |
| Opening a shop | Terms (`OpenShop`), "I agree to the Seller terms" required |
| Booking a parcel | Terms (`SendParcel`) + tracking number |
| Each order at checkout | Terms (`Checkout`) + order number ("By placing your order you agree…") |
| "We've updated our Terms" → Accept | Terms (`UpdatedTerms`) |
| "We've updated our Privacy Policy" → Got it | Privacy (`PrivacyNotice`) |

Publishing a new Terms version on **Admin → Terms & Help** means everyone must accept it: until they do, **placing
orders and adding dishes return `409 Terms.NotAccepted`**, while browsing, their orders, Contact us and their account
still work. `GET /api/me` returns the person's `terms` status so the website and the app can ask.

---

## 14. Background jobs

| Job | Where | When | What |
|---|---|---|---|
| `SuspensionExpiryService` | Infrastructure (hosted service) | hourly | lifts suspensions whose end date has passed |

`Phloem.Worker` is reserved for future scheduled jobs (e.g. nightly suggestion scoring, payment reconciliation).

---

## 15. Deployment (CI/CD)

`.github/workflows` → **Backend CI/CD**, on every push to `main` (pull requests build only; manual runs allowed):

1. Restore and build `Phloem.sln` in Release.
2. Publish `Phloem.Presentation` **self-contained for `win-x86`** (the host needs no .NET install and the default
   32-bit application pool works).
3. Upload the build as an artifact **before** any secrets are added.
4. Write the secrets into the published `appsettings.Production.json` and remove `appsettings.Development.json`.
5. Deploy to **SmarterASP.NET** with MSDeploy, keeping `wwwroot/uploads`, `wwwroot/private-uploads`, `App_Data`
   and `logs`.

**GitHub repository secrets**

| Secret | Goes to |
|---|---|
| `DEFAULT_CONNECTION` | `ConnectionStrings:DefaultConnection` |
| `JWT_KEY` | `Jwt:Key` (32+ characters) |
| `STORAGE_SIGNING_KEY` | `Storage:Local:SigningKey` (32+, different from the JWT key) |
| `SEED_ADMIN_PASSWORD` | `SeedData:AdminPassword` |
| `EMAIL_PASSWORD` | `Email:Password` |
| `SEED_DEMO_OWNER_PASSWORD` *(optional)* | `SeedData:DemoOwnerPassword` |
| `WEBSITE_NAME`, `SERVER_COMPUTER_NAME`, `SERVER_USERNAME`, `SERVER_PASSWORD` | SmarterASP.NET deployment |

**Before each deploy that changes the model:** add the migration locally (section 7) and commit it; the API applies
it on start-up.

---

## 16. Adding a feature: the checklist

1. **Show the folder structure first** and agree it before writing code.
2. **Domain**: entity/value object with its rules (throwing only for bugs), enum values only added at the end,
   errors in `Shared/Errors/<Area>Errors.cs`, repository interface.
3. **Persistence**: `<Entity>Configuration` (lengths, indexes), repository, `DbSet`, register in
   `DependencyInjection.cs`, **add a migration**.
4. **Application**: `Features/<Area>/{Commands,Queries,Dtos,Validators}`; validator messages written for people;
   `[Cacheable]` / `[InvalidatesCache]` with a tag from `CacheTags` when it applies.
5. **Infrastructure**: only if a new outside service or setting is needed (settings class + `AppSettings` keys +
   `ValidateOnStart`).
6. **Presentation**: endpoint group in `Endpoints/<Area>`, registered in `EndpointExtensions.MapPhloemEndpoints`,
   with `WithName`, `WithSummary`, `Produces…` so Swagger stays accurate.
7. Never break the dependency rule: nothing in Domain or Application references EF Core, Identity, MailKit or
   Magick.NET.

---

## 17. Troubleshooting

| Problem | Fix |
|---|---|
| `PendingModelChangesWarning` on start-up | add the missing migration (section 7.1) |
| Start-up fails with "… must be set" / options validation | a required setting or secret is missing (section 6) |
| Browser: CORS error | add the website's origin to `Cors:AllowedOrigins` |
| Android emulator can't load pictures | the app uses `http://10.0.2.2:5175` for files (see the app's README); start the `https` profile, which also listens on 5175 |
| Email links don't work after a deploy | `App_Data/keys` was lost; keep it between deployments |
| No emails arrive locally | run smtp4dev (localhost:25) or set a real SMTP server |
| 429 on Contact us while testing | wait 10 minutes, or restart the API (limits are in memory) |

---

## 18. Roadmap

- **"Chat with us" AI assistant**: built in .NET with `Microsoft.Extensions.AI` (Claude Haiku to launch; Ollama or
  OpenAI swappable in settings), read-only tools over the customer's own orders and parcels, answers grounded in the
  Help/Terms text, a numbers check before replying, and hand-over to the Contact messages inbox.
- A **learned recommender** (ML.NET) to replace the counting ranker once there are enough orders.
- A real **payment provider** to replace the test payments.
- **Google sign-in** switched on (keys pending).

---

**Licence:** not specified yet. © Phloem.
