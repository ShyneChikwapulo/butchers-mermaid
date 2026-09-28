using System.Net;
using ButchersMermaid.Data.Entities;
using ButchersMermaid.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;

namespace ButchersMermaid.Tests.Services;

public class EmailServiceTests
{
    private const string ApiKey = "re_test_key";
    private const string NotifyAddress = "test-inbox@example.com";

    private static EmailService CreateService(
        StubHttpMessageHandler handler,
        string? apiKey = ApiKey,
        string? notifyAddress = NotifyAddress)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Resend:ApiKey"] = apiKey,
                ["Resend:NotifyAddress"] = notifyAddress,
            })
            .Build();

        var http = new HttpClient(handler);

        return new EmailService(http, config, NullLogger<EmailService>.Instance);
    }

    private static Enquiry SampleEnquiry(EnquiryType type = EnquiryType.Contact) => new()
    {
        Id = 1,
        Type = type,
        Name = "Test User",
        Email = "user@example.com",
        Phone = "+27 21 555 1234",
        Subject = "Booking enquiry",
        Message = "Hello,\nI would like to book a table.",
        SubmittedAt = new DateTime(2026, 9, 28, 12, 0, 0, DateTimeKind.Utc),
        BusinessName = type == EnquiryType.Wholesale ? "Test Restaurant" : null,
        BusinessType = type == EnquiryType.Wholesale ? "Restaurant" : null,
        MonthlyVolume = type == EnquiryType.Wholesale ? "50–200 kg" : null,
    };

    [Fact]
    public async Task SendEnquiryNotification_ReturnsFalse_WhenApiKeyMissing()
    {
        var handler = StubHttpMessageHandler.ReturnsOk();
        var service = CreateService(handler, apiKey: null);

        var (success, error) = await service.SendEnquiryNotificationAsync(SampleEnquiry());

        Assert.False(success);
        Assert.Contains("ApiKey", error);
        Assert.Null(handler.LastRequest);
    }

    [Fact]
    public async Task SendEnquiryNotification_ReturnsFalse_WhenNotifyAddressMissing()
    {
        var handler = StubHttpMessageHandler.ReturnsOk();
        var service = CreateService(handler, notifyAddress: null);

        var (success, error) = await service.SendEnquiryNotificationAsync(SampleEnquiry());

        Assert.False(success);
        Assert.Contains("NotifyAddress", error);
        Assert.Null(handler.LastRequest);
    }

    [Fact]
    public async Task SendEnquiryNotification_PostsToResend_WithCorrectHeaders()
    {
        var handler = StubHttpMessageHandler.ReturnsOk();
        var service = CreateService(handler);

        var (success, _) = await service.SendEnquiryNotificationAsync(SampleEnquiry());

        Assert.True(success);
        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://api.resend.com/emails", handler.LastRequest!.RequestUri!.ToString());
        Assert.Equal("Bearer", handler.LastRequest.Headers.Authorization?.Scheme);
        Assert.Equal(ApiKey, handler.LastRequest.Headers.Authorization?.Parameter);
    }

    [Fact]
    public async Task SendEnquiryNotification_SendsToNotifyAddress()
    {
        var handler = StubHttpMessageHandler.ReturnsOk();
        var service = CreateService(handler);

        await service.SendEnquiryNotificationAsync(SampleEnquiry());

        Assert.NotNull(handler.LastRequestBody);
        Assert.Contains(NotifyAddress, handler.LastRequestBody!);
    }

    [Fact]
    public async Task SendEnquiryNotification_IncludesReplyToEnquirerEmail()
    {
        var handler = StubHttpMessageHandler.ReturnsOk();
        var service = CreateService(handler);
        var enquiry = SampleEnquiry();
        enquiry.Email = "specific-sender@example.com";

        await service.SendEnquiryNotificationAsync(enquiry);

        Assert.NotNull(handler.LastRequestBody);
        Assert.Contains("specific-sender@example.com", handler.LastRequestBody!);
        Assert.Contains("reply_to", handler.LastRequestBody!);
    }

    [Fact]
    public async Task SendEnquiryNotification_UsesWholesaleSubjectForWholesaleEnquiries()
    {
        var handler = StubHttpMessageHandler.ReturnsOk();
        var service = CreateService(handler);
        var enquiry = SampleEnquiry(EnquiryType.Wholesale);

        await service.SendEnquiryNotificationAsync(enquiry);

        Assert.NotNull(handler.LastRequestBody);
        Assert.Contains("Wholesale", handler.LastRequestBody!);
        Assert.Contains("Test Restaurant", handler.LastRequestBody!);
    }

    [Fact]
    public async Task SendEnquiryNotification_HtmlEncodesUserInput()
    {
        var handler = StubHttpMessageHandler.ReturnsOk();
        var service = CreateService(handler);
        var enquiry = SampleEnquiry();
        enquiry.Name = "<script>alert(1)</script>";
        enquiry.Message = "Normal message with <b>tags</b>";

        await service.SendEnquiryNotificationAsync(enquiry);

        Assert.NotNull(handler.LastRequestBody);

        // The body is JSON. System.Text.Json escapes HTML-sensitive characters
        // inside string values (e.g. & becomes \u0026), so we parse the JSON to
        // assert against the actual HTML that would be sent to Resend.
        using var doc = JsonDocument.Parse(handler.LastRequestBody!);
        var html = doc.RootElement.GetProperty("html").GetString()!;

        // Encoded form must appear in the decoded HTML
        Assert.Contains("&lt;script&gt;", html);
        Assert.Contains("&lt;b&gt;tags&lt;/b&gt;", html);
        // Raw form must NOT appear
        Assert.DoesNotContain("<script>alert", html);
    }

    [Fact]
    public async Task SendEnquiryNotification_ReturnsFalse_WhenResendReturns400()
    {
        var handler = StubHttpMessageHandler.ReturnsStatus(
            HttpStatusCode.BadRequest,
            "{\"error\":\"invalid from address\"}");
        var service = CreateService(handler);

        var (success, error) = await service.SendEnquiryNotificationAsync(SampleEnquiry());

        Assert.False(success);
        Assert.Contains("400", error);
        Assert.Contains("invalid from address", error);
    }

    [Fact]
    public async Task SendEnquiryNotification_ReturnsFalse_WhenHttpThrows()
    {
        var handler = StubHttpMessageHandler.Throws(new HttpRequestException("network down"));
        var service = CreateService(handler);

        var (success, error) = await service.SendEnquiryNotificationAsync(SampleEnquiry());

        Assert.False(success);
        Assert.Contains("network down", error);
    }
}