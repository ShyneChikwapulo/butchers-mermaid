using System.Text;
using ButchersMermaid.Data;

namespace ButchersMermaid.Endpoints;

public static class SitemapEndpoints
{
    // Public routes and their relative priority. Higher = more important.
    private static readonly (string Path, string ChangeFreq, string Priority)[] Routes =
    {
        ("",          "weekly",  "1.0"),   // home
        ("/menu",     "weekly",  "0.9"),
        ("/butchery", "monthly", "0.8"),
        ("/restaurant","monthly","0.8"),
        ("/deli",     "monthly", "0.8"),
        ("/sea",      "monthly", "0.7"),
        ("/wholesale","monthly", "0.7"),
        ("/contact",  "monthly", "0.6"),
        ("/about",    "monthly", "0.5"),
    };

    public static IEndpointRouteBuilder MapSitemapEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/sitemap.xml", (SiteConfig site) =>
        {
            var today = DateTime.UtcNow.ToString("yyyy-MM-dd");

            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

            foreach (var (path, changeFreq, priority) in Routes)
            {
                var url = site.Url(path);
                sb.AppendLine("  <url>");
                sb.AppendLine($"    <loc>{url}</loc>");
                sb.AppendLine($"    <lastmod>{today}</lastmod>");
                sb.AppendLine($"    <changefreq>{changeFreq}</changefreq>");
                sb.AppendLine($"    <priority>{priority}</priority>");
                sb.AppendLine("  </url>");
            }

            sb.AppendLine("</urlset>");

            return Results.Content(sb.ToString(), "application/xml", Encoding.UTF8);
        });

        return app;
    }
}