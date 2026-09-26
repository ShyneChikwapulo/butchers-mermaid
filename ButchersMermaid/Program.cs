using ButchersMermaid.Components;
using ButchersMermaid.Data;
using Microsoft.EntityFrameworkCore;

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

// ---------- Blazor ----------
builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

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

app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();