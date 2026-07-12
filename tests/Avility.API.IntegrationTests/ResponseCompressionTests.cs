using System.Net.Http;
using Xunit;

namespace Avility.API.IntegrationTests;

public class ResponseCompressionTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ResponseCompressionTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Response_IsCompressed_WhenClientAcceptsGzip()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/health/live");
        request.Headers.Add("Accept-Encoding", "gzip");

        var response = await _client.SendAsync(request);

        Assert.Contains("gzip", response.Content.Headers.ContentEncoding);
    }

    [Fact]
    public async Task Response_IsUncompressed_WhenClientSendsNoAcceptEncoding()
    {
        // No Accept-Encoding header at all - compression middleware
        // should never force encoding on a client that didn't ask for it.
        var response = await _client.GetAsync("/health/live");

        Assert.Empty(response.Content.Headers.ContentEncoding);
    }
}