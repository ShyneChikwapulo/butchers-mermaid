using System.Security.Cryptography;
using System.Text;
using ButchersMermaid.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace ButchersMermaid.Tests.Services;

public class AdminAuthServiceTests
{
    private const string CorrectPassword = "TestPassword!2025";
    private const string CookieName = "bm_admin";

    private static AdminAuthService CreateService(string? password = CorrectPassword)
    {
        var configData = new Dictionary<string, string?>
        {
            ["Admin:Password"] = password,
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        return new AdminAuthService(config, NullLogger<AdminAuthService>.Instance);
    }

    private static DefaultHttpContext CreateContext() => new();

    [Fact]
    public void TryLogin_ReturnsFalse_WhenPasswordIsWrong()
    {
        var service = CreateService();
        var context = CreateContext();

        var result = service.TryLogin(context, "wrong");

        Assert.False(result);
        Assert.False(context.Response.Headers.ContainsKey("Set-Cookie"));
    }

    [Fact]
    public void TryLogin_ReturnsFalse_WhenPasswordNotConfigured()
    {
        var service = CreateService(password: null);
        var context = CreateContext();

        var result = service.TryLogin(context, "anything");

        Assert.False(result);
    }

    [Fact]
    public void TryLogin_ReturnsTrue_AndSetsCookie_WhenPasswordIsCorrect()
    {
        var service = CreateService();
        var context = CreateContext();

        var result = service.TryLogin(context, CorrectPassword);

        Assert.True(result);
        var setCookie = context.Response.Headers["Set-Cookie"].ToString();
        Assert.Contains(CookieName, setCookie);
        Assert.Contains("httponly", setCookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=strict", setCookie, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void IsAuthenticated_ReturnsFalse_WhenNoCookiePresent()
    {
        var service = CreateService();
        var context = CreateContext();

        var result = service.IsAuthenticated(context);

        Assert.False(result);
    }

    [Fact]
    public void IsAuthenticated_ReturnsTrue_AfterSuccessfulLogin()
    {
        var service = CreateService();
        var loginContext = CreateContext();
        service.TryLogin(loginContext, CorrectPassword);

        // The login context set the cookie; simulate a later request
        // that carries the same cookie.
        var cookieValue = ExtractCookieValue(loginContext, CookieName);
        var requestContext = CreateContext();
        requestContext.Request.Headers["Cookie"] = $"{CookieName}={cookieValue}";

        var result = service.IsAuthenticated(requestContext);

        Assert.True(result);
    }

    [Fact]
    public void IsAuthenticated_ReturnsFalse_WhenCookieIsTampered()
    {
        var service = CreateService();
        var context = CreateContext();
        context.Request.Headers["Cookie"] = $"{CookieName}=garbage-value";

        var result = service.IsAuthenticated(context);

        Assert.False(result);
    }

    [Fact]
    public void IsAuthenticated_ReturnsFalse_AfterPasswordRotation()
    {
        // Log in with one password
        var serviceA = CreateService("FirstPassword!1");
        var loginContext = CreateContext();
        serviceA.TryLogin(loginContext, "FirstPassword!1");
        var cookieValue = ExtractCookieValue(loginContext, CookieName);

        // Same cookie, but the service is now configured with a different password
        var serviceB = CreateService("SecondPassword!2");
        var requestContext = CreateContext();
        requestContext.Request.Headers["Cookie"] = $"{CookieName}={cookieValue}";

        var result = serviceB.IsAuthenticated(requestContext);

        Assert.False(result);
    }

    [Fact]
    public void Logout_RemovesCookie()
    {
        var service = CreateService();
        var context = CreateContext();
        service.TryLogin(context, CorrectPassword);

        service.Logout(context);

        var setCookie = context.Response.Headers["Set-Cookie"].ToString();
        // Deleting a cookie writes a Set-Cookie with an expired date
        Assert.Contains(CookieName, setCookie);
        Assert.Contains("expires=", setCookie, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Pulls the value of a named cookie out of the Set-Cookie header that
    /// TryLogin appended to the response. Test-only helper.
    /// </summary>
    private static string ExtractCookieValue(HttpContext context, string cookieName)
    {
        var header = context.Response.Headers["Set-Cookie"].ToString();
        var prefix = $"{cookieName}=";
        var start = header.IndexOf(prefix, StringComparison.Ordinal) + prefix.Length;
        var end = header.IndexOf(';', start);
        return header[start..end];
    }
}