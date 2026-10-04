# The Butcher's Mermaid

A website for **The Butcher's Mermaid**, a high-end butchery, restaurant, bar and deli in Hout Bay, Cape Town. Built with .NET 10 and Blazor.

**[Live demo](https://butchers-mermaid.onrender.com)** · Built solo by [Shine Chikwapulo](https://github.com/ShyneChikwapulo)

> **Status:** Portfolio project built as a proposal for the business. It is not deployed on the business's behalf. The demo runs on ephemeral SQLite, so enquiries and sessions reset on each deploy.

<!-- Screenshots: add to docs/screenshots/ and uncomment
![Home](docs/screenshots/home.png)
![Menu](docs/screenshots/menu.png)
![Admin](docs/screenshots/admin.png)
-->

## Features

- Pages for home, about, butchery, deli, restaurant, seafood, wholesale, menu and contact.
- Menu stored in the database (categories, items, prices) and seeded on first run.
- General and wholesale enquiry forms with validation and a honeypot field. Enquiries are saved and emailed to the owner through the Resend API.
- Password-protected admin dashboard to view enquiries, mark them read or unread, and delete them.
- SEO groundwork: shared meta-tag component, JSON-LD structured data and an XML sitemap endpoint.
- Design system documented in `docs/design-tokens.md`.

## Tech stack

| Area | Technology |
|---|---|
| Framework | .NET 10, ASP.NET Core, Blazor Web App (interactive server) |
| Data | EF Core 10, SQL Server (development), SQLite (production) |
| Email | Resend HTTPS API (SMTP is blocked on Render's free tier) |
| Packaging | Docker, multi-stage build, non-root `app` user |
| Hosting | Render |
| Tests | xUnit, 31 tests |

## Repository layout

```text
ButchersMermaid/          Blazor app (Components, Data, Endpoints, Migrations, Services, wwwroot)
ButchersMermaid.Tests/    xUnit tests
docs/design-tokens.md     Palette, typography, spacing
content.md, menu.md       Business content reference
Dockerfile
```

## Getting started

**Prerequisites:** .NET 10 SDK, Docker Desktop (for SQL Server).

```bash
git clone https://github.com/ShyneChikwapulo/butchers-mermaid.git
cd butchers-mermaid

# SQL Server in Docker
docker run -d --name butchers-mermaid-sql \
  -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=<choose-a-password>" -e "MSSQL_PID=Developer" \
  -p 1433:1433 mcr.microsoft.com/mssql/server:2022-latest

# Configuration (user secrets)
cd ButchersMermaid
dotnet user-secrets set "ConnectionStrings:DefaultConnection" 'Server=localhost,1433;Database=ButchersMermaid;User Id=sa;Password=<same-password>;TrustServerCertificate=True;Encrypt=False'
dotnet user-secrets set "Site:BaseUrl" "http://localhost:5225"
dotnet user-secrets set "Resend:ApiKey" "re_your_key"
dotnet user-secrets set "Resend:NotifyAddress" "you@example.com"
dotnet user-secrets set "Admin:Password" '<choose-an-admin-password>'
cd ..

dotnet run --project ButchersMermaid
```

Open <http://localhost:5225>. In Development, migrations run on startup and the menu is seeded if empty. The admin dashboard is at `/admin-b7f3e9`.

## Tests

```bash
dotnet test
```

31 xUnit tests cover the menu and enquiry repositories, admin authentication and the email service. The email tests use a stubbed `HttpMessageHandler` and check HTML-encoding of user input and failure paths.

## Docker

```bash
docker build -t butchers-mermaid:dev .
docker run --rm -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e "ConnectionStrings__DefaultConnection=Data Source=/app/data/butchers-mermaid.db" \
  -e "Site__BaseUrl=http://localhost:8080" \
  -e "Resend__ApiKey=re_your_key" -e "Resend__NotifyAddress=you@example.com" \
  -e "Admin__Password=<choose-an-admin-password>" \
  butchers-mermaid:dev
```

Outside Development the app uses SQLite and creates the schema on start.

## Roadmap

- Cinematic intro (Blender to Three.js), developed on a separate branch
- CI to run the test suite on every push
- Admin login hardening (rate limiting, per-session tokens)
- Persistent production database

<!-- CONFIRM: keep the cinematic-intro line public before it exists? -->

## Author

Shine Chikwapulo · [GitHub](https://github.com/ShyneChikwapulo) · [LinkedIn](https://www.linkedin.com/in/shine-chikwapulo-741b20265/)