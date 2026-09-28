using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ButchersMermaid.Data.Entities;

namespace ButchersMermaid.Services;

public interface IEmailService
{
    Task<(bool Success, string? Error)> SendEnquiryNotificationAsync(Enquiry enquiry);
}

public class EmailService : IEmailService
{
    private const string ResendEndpoint = "https://api.resend.com/emails";

    // On Resend's free tier without a verified domain, the only permitted
    // sender is onboarding@resend.dev, and the only permitted recipient is
    // the address tied to your Resend account. For the demo, all enquiries
    // forward to the demo owner's inbox.
    private const string FromAddress = "The Butcher's Mermaid <onboarding@resend.dev>";

    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;
    private readonly string? _apiKey;
    private readonly string? _notifyAddress;

    public EmailService(
        HttpClient http,
        IConfiguration config,
        ILogger<EmailService> logger)
    {
        _http = http;
        _config = config;
        _logger = logger;
        _apiKey = config["Resend:ApiKey"];
        _notifyAddress = config["Resend:NotifyAddress"];

        if (!string.IsNullOrEmpty(_apiKey))
        {
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _apiKey);
        }
    }

    public async Task<(bool Success, string? Error)> SendEnquiryNotificationAsync(Enquiry enquiry)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            const string msg = "Resend:ApiKey is not configured.";
            _logger.LogWarning(msg);
            return (false, msg);
        }

        if (string.IsNullOrWhiteSpace(_notifyAddress))
        {
            const string msg = "Resend:NotifyAddress is not configured.";
            _logger.LogWarning(msg);
            return (false, msg);
        }

        var subject = enquiry.Type == EnquiryType.Wholesale
            ? $"[Wholesale] {enquiry.BusinessName ?? enquiry.Name}"
            : $"[Contact] {enquiry.Subject ?? "General enquiry"}";

        var html = BuildHtml(enquiry);

        var payload = new
        {
            from = FromAddress,
            to = new[] { _notifyAddress },
            subject,
            html,
            reply_to = enquiry.Email,
        };

        try
        {
            var response = await _http.PostAsJsonAsync(ResendEndpoint, payload);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation(
                    "Enquiry email sent for enquiry {Id}", enquiry.Id);
                return (true, null);
            }

            var body = await response.Content.ReadAsStringAsync();
            var error = $"Resend returned {(int)response.StatusCode}: {Truncate(body, 500)}";
            _logger.LogWarning("Enquiry email failed for enquiry {Id}: {Error}",
                enquiry.Id, error);
            return (false, error);
        }
        catch (Exception ex)
        {
            var error = $"Exception: {ex.Message}";
            _logger.LogError(ex,
                "Exception sending enquiry email for enquiry {Id}", enquiry.Id);
            return (false, error);
        }
    }

    private static string BuildHtml(Enquiry enquiry)
    {
        var typeLabel = enquiry.Type == EnquiryType.Wholesale
            ? "Wholesale Enquiry"
            : "Contact Enquiry";

        var extraRows = enquiry.Type == EnquiryType.Wholesale
            ? $"""
               <tr><td style="padding:6px 0;color:#8a7a60;">Business</td>
                   <td style="padding:6px 0;">{Encode(enquiry.BusinessName)}</td></tr>
               <tr><td style="padding:6px 0;color:#8a7a60;">Business type</td>
                   <td style="padding:6px 0;">{Encode(enquiry.BusinessType)}</td></tr>
               <tr><td style="padding:6px 0;color:#8a7a60;">Monthly volume</td>
                   <td style="padding:6px 0;">{Encode(enquiry.MonthlyVolume)}</td></tr>
               """
            : $"""
               <tr><td style="padding:6px 0;color:#8a7a60;">Subject</td>
                   <td style="padding:6px 0;">{Encode(enquiry.Subject)}</td></tr>
               """;

        return $"""
            <!DOCTYPE html>
            <html>
            <body style="font-family:Georgia,serif;color:#1a1a1a;max-width:640px;margin:0 auto;padding:24px;">
                <h1 style="font-size:20px;color:#8B6420;border-bottom:1px solid #d9c78a;padding-bottom:8px;margin:0 0 20px;">
                    {typeLabel}
                </h1>
                <table style="width:100%;border-collapse:collapse;font-size:14px;">
                    <tr><td style="padding:6px 0;color:#8a7a60;width:140px;">Name</td>
                        <td style="padding:6px 0;">{Encode(enquiry.Name)}</td></tr>
                    <tr><td style="padding:6px 0;color:#8a7a60;">Email</td>
                        <td style="padding:6px 0;"><a href="mailto:{Encode(enquiry.Email)}">{Encode(enquiry.Email)}</a></td></tr>
                    <tr><td style="padding:6px 0;color:#8a7a60;">Phone</td>
                        <td style="padding:6px 0;">{Encode(enquiry.Phone)}</td></tr>
                    {extraRows}
                    <tr><td style="padding:6px 0;color:#8a7a60;vertical-align:top;">Message</td>
                        <td style="padding:6px 0;white-space:pre-wrap;">{Encode(enquiry.Message)}</td></tr>
                    <tr><td style="padding:6px 0;color:#8a7a60;">Received</td>
                        <td style="padding:6px 0;">{enquiry.SubmittedAt:yyyy-MM-dd HH:mm} UTC</td></tr>
                </table>
                <p style="font-size:12px;color:#8a7a60;margin-top:32px;border-top:1px solid #eee;padding-top:12px;">
                    Submitted via thebutchersmermaid.co.za demo site.
                </p>
            </body>
            </html>
            """;
    }

    private static string Encode(string? value) =>
        System.Net.WebUtility.HtmlEncode(value ?? "—");

    private static string Truncate(string value, int max) =>
        value.Length <= max ? value : value[..max] + "…";
}