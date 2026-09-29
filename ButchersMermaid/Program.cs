using ButchersMermaid.Components;
using ButchersMermaid.Data;
using Microsoft.EntityFrameworkCore;
using ButchersMermaid.Services;
using ButchersMermaid.Endpoints;
using Microsoft.AspNetCore.HttpOverrides;


var builder = WebApplication.CreateBuilder(args);

// ---------- Database ----------
// Local dev: SQL Server in Docker (connection string in user secrets)
// Production: SQLite file (connection string in env vars)
builder.Services.AddDbContextFactory<AppDbContext>(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            sql =>
            {
                sql.EnableRetryOnFailure();
                sql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            });
    }
    else
    {
        options.UseSqlite(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            sqlite => sqlite.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
    }
});

// ---------- Services ----------
builder.Services.AddScoped<IEnquiryRepository, EnquiryRepository>();
// ---------- Antiforgery ----------
// In production behind a proxy, the origin seen by the app may differ from
// what the browser sent. Relax the cookie so Blazor EditForm works.
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.Name = "bm_af";
});
builder.Services.AddScoped<IMenuRepository, MenuRepository>();
builder.Services.AddSingleton<IAdminAuthService, AdminAuthService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient<IEmailService, EmailService>();

// ---------- Site configuration ----------
var baseUrl = builder.Configuration["Site:BaseUrl"]
    ?? "https://butchers-mermaid.onrender.com";

builder.Services.AddSingleton(new SiteConfig
{
    BaseUrl = baseUrl.TrimEnd('/'),
});

// ---------- Blazor ----------
builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// ---------- Forwarded headers ----------
// Behind Render (and any reverse proxy), trust the X-Forwarded-* headers
// so ASP.NET Core knows the original scheme and host.
var forwardedOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor
                     | ForwardedHeaders.XForwardedHost
                     | ForwardedHeaders.XForwardedProto,
};
forwardedOptions.KnownIPNetworks.Clear();
forwardedOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedOptions);

// ---------- Database creation ----------
// Dev: apply migrations. Prod: EnsureCreated (no migrations in prod for this demo).
using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
    using var db = factory.CreateDbContext();

    if (app.Environment.IsDevelopment())
    {
        db.Database.Migrate();
    }
    else
    {
        db.Database.EnsureCreated();
    }

    // Seed menu if empty
    if (!db.MenuCategories.Any())
    {
        db.MenuCategories.AddRange(SeedData.GetMenu());
        db.SaveChanges();
    }
}

// ---------- Middleware pipeline ----------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

// Only redirect to HTTPS in environments where HTTPS is actually terminating.
// In Docker/Render, TLS terminates upstream and requests arrive as plain HTTP.
if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseAntiforgery();

app.MapStaticAssets();
app.UseStaticFiles();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapAdminEndpoints();
app.MapSitemapEndpoint();

app.Run();