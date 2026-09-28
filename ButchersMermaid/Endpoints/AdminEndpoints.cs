using ButchersMermaid.Services;

namespace ButchersMermaid.Endpoints;

public static class AdminEndpoints
{
    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin");

        // ---------- Login ----------
        // Antiforgery is disabled deliberately:
        //  - CSRF-login is low-severity (attacker forces you into THEIR session)
        //  - This is a demo admin; production would use real auth, not a password gate
        group.MapPost("/login", async (
            HttpContext context,
            IAdminAuthService auth,
            IFormCollection form) =>
        {
            var password = form["password"].ToString();

            if (auth.TryLogin(context, password))
            {
                return Results.Redirect("/admin-b7f3e9");
            }

            return Results.Redirect("/admin-b7f3e9?error=1");
        }).DisableAntiforgery();

        // ---------- Logout ----------
        // Antiforgery disabled for the same reason as login.
        group.MapPost("/logout", (HttpContext context, IAdminAuthService auth) =>
        {
            auth.Logout(context);
            return Results.Redirect("/admin-b7f3e9");
        }).DisableAntiforgery();

        return app;
    }
}