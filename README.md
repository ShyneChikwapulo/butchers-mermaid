# The Butcher's Mermaid — Website

A marketing website for **The Butcher's Mermaid**, a high-end butchery, restaurant, bar, and deli in Hout Bay, Cape Town.

Built as a portfolio demonstration by **Shine Chikwapulo**. Not yet deployed on behalf of the business; presented to the owner as a proposal.

---

## Stack

- **.NET 10** / ASP.NET Core
- **Blazor Web App** — InteractiveServer render mode
- **EF Core 10** — SQL Server (local development), SQLite (production)
- **Docker** — multi-stage build
- **Render** — free-tier hosting
- **Resend** — HTTPS email API (SMTP is blocked on Render's free tier)
- **xUnit** — test suite

---

## Live Demo

Deployed at: **https://butchers-mermaid.onrender.com**

Production uses ephemeral SQLite. Enquiries and admin session data reset on each deploy.

---

## Repository Layout

```text
butchers-mermaid/
├── ButchersMermaid/                 # Blazor application
│   ├── Components/
│   │   ├── Layout/                 # NavMenu, Footer, MainLayout
│   │   ├── Pages/                  # Route components
│   │   │   └── Admin/              # Admin login + dashboard
│   │   └── Shared/                 # PageHero, SeoMeta, GoldDivider, etc.
│   ├── Data/
│   │   ├── Entities/               # EF Core entity classes
│   │   ├── AppDbContext.cs
│   │   ├── BusinessInfo.cs         # Business facts (hours, contact, URLs)
│   │   ├── SeedData.cs             # Menu seed
│   │   └── SiteConfig.cs            # Base URL, URL helpers
│   ├── Endpoints/                  # Minimal API endpoints (admin, sitemap)
│   ├── Migrations/                 # EF Core migrations
│   ├── Services/                   # Email, repositories, admin auth
│   └── wwwroot/                    # Static assets
│       ├── images/
│       │   ├── brand/              # Logo
│       │   ├── business/           # Photos of the business
│       │   └── illustrations/      # Hand-drawn engravings
│       └── app.css                 # Global design system
├── ButchersMermaid.Tests/          # xUnit test project
├── docs/
│   └── design-tokens.md            # Palette, typography, spacing
├── Dockerfile
├── content.md                      # Business content reference
├── menu.md                         # Transcribed menu
└── README.md
```

---

## Local Development

### Prerequisites

- .NET 10 SDK
- Docker Desktop (for SQL Server)
- `dotnet-ef` CLI tool:

```bash
dotnet tool install --global dotnet-ef
```

### First-Time Setup

#### 1. Clone the repository

```bash
git clone https://github.com/ShyneChikwapulo/butchers-mermaid.git
cd butchers-mermaid
```

#### 2. Start SQL Server in Docker

```bash
docker run -d \
  --name butchers-mermaid-sql \
  -e "ACCEPT_EULA=Y" \
  -e "MSSQL_SA_PASSWORD=ButchersMermaid!Dev2025" \
  -e "MSSQL_PID=Developer" \
  -p 1433:1433 \
  mcr.microsoft.com/mssql/server:2022-latest
```

#### 3. Set User Secrets

Navigate into the application directory:

```bash
cd ButchersMermaid
```

Set the required configuration values:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" 'Server=localhost,1433;Database=ButchersMermaid;User Id=sa;Password=ButchersMermaid!Dev2025;TrustServerCertificate=True;Encrypt=False'

dotnet user-secrets set "Site:BaseUrl" "http://localhost:5225"

dotnet user-secrets set "Resend:ApiKey" "re_your_key_here"

dotnet user-secrets set "Resend:NotifyAddress" "your-email@example.com"

dotnet user-secrets set "Admin:Password" 'YourAdminPassword!'
```

Then return to the repository root:

```bash
cd ..
```

#### 4. Run the Application

```bash
dotnet run --project ButchersMermaid
```

The application runs database migrations on startup and seeds the menu if the database is empty.

Open:

**http://localhost:5225**

---

## Running Tests

Run the full xUnit test suite:

```bash
dotnet test
```

The suite contains approximately **31 tests** covering:

- Repositories
- Admin authentication
- Email service

---

## Admin

The admin dashboard is available at:

```text
/admin-b7f3e9
```

The password is the value configured under:

```text
Admin:Password
```

### Admin capabilities

- View enquiries
- Mark enquiries as read/unread
- Delete enquiries
- Sign out

Enquiries are also emailed to the address configured under:

```text
Resend:NotifyAddress
```

---

## Docker

### Build the Image

```bash
docker build -t butchers-mermaid:dev .
```

### Run the Container Locally

```bash
docker run --rm \
  -p 8080:8080 \
  -e "ASPNETCORE_ENVIRONMENT=Production" \
  -e "ConnectionStrings__DefaultConnection=Data Source=/app/data/butchers-mermaid.db" \
  -e "Site__BaseUrl=http://localhost:8080" \
  -e "Resend__ApiKey=re_your_key" \
  -e "Resend__NotifyAddress=your-email@example.com" \
  -e "Admin__Password=YourAdminPassword!" \
  --name bm-test \
  butchers-mermaid:dev
```

Open:

**http://localhost:8080**

The container runs as a non-root user:

```text
appuser (UID 5678)
```

SQLite is stored at:

```text
/app/data/butchers-mermaid.db
```

---

## Deployment — Render

The application is deployed on **Render's free tier** as a Docker Web Service.

### Required Environment Variables

Set the following values in the Render dashboard under **Environment**:

| Key | Value |
|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ConnectionStrings__DefaultConnection` | `Data Source=/app/data/butchers-mermaid.db` |
| `Site__BaseUrl` | `https://butchers-mermaid.onrender.com` |
| `Resend__ApiKey` | `re_...` from your Resend account |
| `Resend__NotifyAddress` | The email tied to your Resend account |
| `Admin__Password` | Your admin password |

### Ephemeral Storage

Render's free tier does not offer persistent disks.

The SQLite database therefore lives on ephemeral storage and resets on every deploy.

This is acceptable for a demonstration project:

- Enquiries persist for the duration of a deployment.
- Enquiries do **not** persist across rebuilds/deployments.
- Admin session data is also reset when the application is redeployed.

### Production Persistence Options

For real production use, either:

- Upgrade to **Render Starter + persistent disk**, or
- Migrate to a hosted PostgreSQL database such as Render Postgres, Supabase, or Neon by swapping the EF Core provider.

---

## Production vs Development Behaviour

| Aspect | Development | Production |
|---|---|---|
| Database | SQL Server (Docker) | SQLite (file) |
| Schema | Migrations | `EnsureCreated()` |
| HTTPS redirect | Yes (HTTP port) | No (proxy terminates TLS) |
| Error handling | Development exception page | `/Error` page |
| Antiforgery cookie | Default | `SecurePolicy.None`, `SameSite=Lax` |
| Forwarded headers | Not applied | Trusted via `UseForwardedHeaders` |

---

## Configuration

All secrets are kept outside the repository.

### Local Development

.NET User Secrets are used for sensitive configuration.

See the [First-Time Setup](#first-time-setup) section above.

### Production

Render environment variables are used for production secrets and configuration.

`appsettings.json` contains only non-secret defaults and a fallback base URL.

---

## Known Limitations (Demo Scope)

This is a **demonstration project**, not a production deployment.

The following are deliberate scope boundaries:

- **No customer accounts or authentication.** The only authentication mechanism is the admin password gate.
- **No booking system.** Bookings link out to Dineplan, which the business already pays for.
- **No payments.** Payments are not within the scope of this project.
- **No CMS.** Menu and business data are seeded from C#; edits require a code change.
- **Admin password is a single shared secret.** There are no per-user accounts or rate limiting.
- **No CSRF token on admin login/logout.** Deliberate for the demonstration scope; the severity of a CSRF-login attack is considered low.
- **Ephemeral SQLite in production.** Enquiries reset on deployment.
- **No SEO submissions.** Sitemap and `robots.txt` are served, but the site has not been submitted to Google Search Console.
- **Accessibility basics covered.** Skip links, focus styles, contrast, and semantic HTML are implemented, but a full WCAG audit remains future work.

---

## Future Work (Post-Signing)

If the business owner signs off on the proposal:

- Custom domain (e.g. `thebutchersmermaid.co.za`) with TLS
- Migrate to a managed database for persistent data
- Register a sending domain with Resend so enquiry emails come from `@thebutchersmermaid.co.za`
- Wire forms to the business inbox instead of the demo owner's inbox
- Implement proper authentication using ASP.NET Core Identity or OpenID Connect
- Submit the site to Google Search Console and ping the sitemap
- Add privacy-respecting analytics such as Plausible or Fathom
- Make the menu database-editable through the admin dashboard

---

## Development Process

The project was built incrementally, phase-by-phase, in collaboration with an AI assistant.

Each phase was checkpointed so that every stage resulted in a working commit.

### Phase 0
**Project scaffold, content, design tokens**

- Project setup
- Initial business content
- Design tokens

### Phase 1A
**Design system, layout shell**

- Global design system
- Navigation
- Footer
- Main layout

### Phase 1B
**Home page**

- Homepage structure
- Hero section
- Core marketing content

### Phase 2A
**EF Core setup, entities, seed data**

- EF Core configuration
- Database entities
- Seed data

### Phase 2B
**Menu page, PageHero component**

- Menu implementation
- Reusable `PageHero` component

### Phase 2C
**Butchery, restaurant, deli, sea pages**

- Butchery page
- Restaurant page
- Deli page
- Sea page

### Phase 3A
**Enquiry entity, repository, admin auth**

- Enquiry entity
- Repository implementation
- Admin authentication

### Phase 3B
**Wholesale and contact forms, Resend email**

- Wholesale enquiry form
- Contact form
- Resend email integration

### Phase 3C
**About page placeholder**

- Initial About page implementation

### Phase 3D
**Admin dashboard**

- Enquiry management
- Read/unread status
- Delete functionality
- Sign-out functionality

### Phase 3.5
**Cleanup pass**

- General cleanup
- Refinements
- Code and UI improvements

### Phase 4A
**Test project, MenuRepository, tests**

- xUnit test project
- `MenuRepository` tests

### Phase 4B
**AdminAuthService and EmailService tests**

- `AdminAuthService` tests
- `EmailService` tests

### Phase 5A
**SEO metadata, Open Graph, JSON-LD**

- SEO metadata
- Open Graph metadata
- Structured data / JSON-LD

### Phase 5B
**robots.txt, sitemap, 404 and error pages**

- `robots.txt`
- XML sitemap
- 404 page
- Error page

### Phase 5C
**Skip link, focus styles, colour contrast**

- Accessibility improvements
- Skip navigation
- Focus states
- Colour contrast improvements

### Phase 5D
**Lighthouse analysis**

- Lighthouse analysis
- Identified further optimisations
- Fixes deferred where outside demonstration scope

### Phase 6A
**Docker**

- Multi-stage Docker build
- Production container configuration
- Non-root container execution

### Phase 6B
**Render deployment**

- Render Web Service configuration
- Production environment configuration
- SQLite production setup
- Resend configuration

---

## Licence

**Not licensed for reuse.**

This is a portfolio project built specifically for a particular business.