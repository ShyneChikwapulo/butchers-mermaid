using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace ButchersMermaid.Services;

public interface IAdminAuthService
{
    bool IsAuthenticated(HttpContext context);
    bool TryLogin(HttpContext context, string password);
    void Logout(HttpContext context);
}

public class AdminAuthService : IAdminAuthService
{
    private const string CookieName = "bm_admin";
    private const int CookieDays = 7;

    private readonly IConfiguration _config;
    private readonly ILogger<AdminAuthService> _logger;

    public AdminAuthService(IConfiguration config, ILogger<AdminAuthService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public bool IsAuthenticated(HttpContext context)
    {
        var cookie = context.Request.Cookies[CookieName];
        if (string.IsNullOrEmpty(cookie)) return false;

        var expected = ComputeToken();
        // Constant-time comparison to avoid timing attacks.
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(cookie),
            Encoding.UTF8.GetBytes(expected));
    }

    public bool TryLogin(HttpContext context, string password)
    {
        var configured = _config["Admin:Password"];
        if (string.IsNullOrEmpty(configured))
        {
            _logger.LogError("Admin:Password is not configured. Login rejected.");
            return false;
        }

        if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(password),
                Encoding.UTF8.GetBytes(configured)))
        {
            _logger.LogWarning("Failed admin login attempt from {Ip}",
                context.Connection.RemoteIpAddress);
            return false;
        }

        context.Response.Cookies.Append(CookieName, ComputeToken(), new CookieOptions
        {
            HttpOnly = true,
            Secure = context.Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(CookieDays),
            Path = "/",
        });

        return true;
    }

    public void Logout(HttpContext context)
    {
        context.Response.Cookies.Delete(CookieName);
    }

    /// <summary>
    /// The cookie value is an HMAC of a fixed string, keyed by the admin password.
    /// Changing the password invalidates all existing admin sessions.
    /// </summary>
    private string ComputeToken()
    {
        var password = _config["Admin:Password"] ?? "unset";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(password));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes("butchers-mermaid-admin-v1"));
        return Convert.ToBase64String(hash);
    }
}