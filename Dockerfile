# ============================================================================
# The Butcher's Mermaid — Production Dockerfile
# ============================================================================
#
# Multi-stage build:
#   1. SDK stage: restore, publish.
#   2. Runtime stage: minimal ASP.NET image with just the published output.
#
# The build stage uses the "copy full source, then publish" pattern rather
# than the csproj-first caching pattern. The csproj-first pattern broke the
# Blazor static web assets pipeline and _framework/blazor.web.js was not
# published. Correctness over caching for this project.
# ============================================================================

# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the full source tree.
COPY ButchersMermaid/ ./ButchersMermaid/

# Publish the app.
WORKDIR /src/ButchersMermaid
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Container-friendly environment settings.
ENV DOTNET_RUNNING_IN_CONTAINER=true \
    ASPNETCORE_URLS=http://+:8080 \
    DOTNET_gcServer=0 \
    DOTNET_EnableDiagnostics=0 \
    ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

# Install curl for the container healthcheck.
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

# Copy the published output.
COPY --from=build /app/publish .

# Create writable directory for SQLite and hand ownership to the built-in
# 'app' user. Must happen AFTER the COPY because COPY runs as root.
# The .NET base image ships with an 'app' user (UID 1654) since .NET 8.
RUN mkdir -p /app/data \
    && chown -R app:app /app

# Run as the built-in non-root user.
USER app

# Healthcheck using the app's own base URL.
HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=3 \
    CMD curl -f http://localhost:8080/ || exit 1

ENTRYPOINT ["dotnet", "ButchersMermaid.dll"]