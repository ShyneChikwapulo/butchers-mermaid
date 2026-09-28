namespace ButchersMermaid.Tests.Services;

/// <summary>
/// A test HttpMessageHandler that returns a canned response and captures
/// the outgoing request for assertions.
/// </summary>
public class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

    public HttpRequestMessage? LastRequest { get; private set; }
    public string? LastRequestBody { get; private set; }

    public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        _responder = responder;
    }

    public static StubHttpMessageHandler ReturnsOk(string body = "{\"id\":\"fake-id\"}") =>
        new(_ => new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent(body),
        });

    public static StubHttpMessageHandler ReturnsStatus(System.Net.HttpStatusCode status, string body) =>
        new(_ => new HttpResponseMessage(status)
        {
            Content = new StringContent(body),
        });

    public static StubHttpMessageHandler Throws(Exception ex) =>
        new(_ => throw ex);

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequest = request;
        if (request.Content is not null)
        {
            LastRequestBody = await request.Content.ReadAsStringAsync(cancellationToken);
        }
        return _responder(request);
    }
}