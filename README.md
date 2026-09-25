# The Butcher's Mermaid — Demo Website

A marketing website for The Butcher's Mermaid, a high-end butchery,
restaurant, bar, and deli in Hout Bay, Cape Town.

Built as a portfolio demonstration. Not yet deployed on behalf of the
business; presented to the owner as a proposal.

## Stack

- .NET 10 / ASP.NET Core
- Blazor Web App (InteractiveServer render mode)
- EF Core — SQL Server (local dev), SQLite (production demo)
- Docker (multi-stage build)
- Render (free-tier hosting)
- Resend (HTTPS email API)
- xUnit

## Repository layout

- `ButchersMermaid/` — Blazor application
- `ButchersMermaid.Tests/` — xUnit tests (added Phase 6A)
- `docs/` — design and architecture notes
- `content.md` — canonical business content reference
- `menu.md` — transcribed menu (added before Phase 2)

## Status

Under active development. See `docs/roadmap.md` for phases.

## Local development

    dotnet run --project ButchersMermaid

Requires .NET 10 SDK. User secrets configured per developer:

    dotnet user-secrets set "Resend:ApiKey" "<key>" --project ButchersMermaid
    dotnet user-secrets set "Admin:Password" "<password>" --project ButchersMermaid