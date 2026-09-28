namespace ButchersMermaid.Data;

/// <summary>
/// Runtime configuration values resolved from IConfiguration at startup.
/// Populated by DI, accessed as a singleton.
/// </summary>
public class SiteConfig
{
    public string BaseUrl { get; init; } = string.Empty;

    /// <summary>Trailing-slash-safe combination of base URL and path.</summary>
    public string Url(string path)
    {
        var trimmed = path.TrimStart('/').TrimEnd('/');
        return string.IsNullOrEmpty(trimmed)
            ? $"{BaseUrl.TrimEnd('/')}/"
            : $"{BaseUrl.TrimEnd('/')}/{trimmed}";
    }

    /// <summary>Default OG image, used when a page doesn't specify one.</summary>
    public string DefaultOgImage => Url("images/business/hero-interior.jpg");
}