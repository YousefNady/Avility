using System.Net.Http;
using Xunit;

namespace Avility.API.IntegrationTests;

public class CorrelationIdTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CorrelationIdTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Response_AlwaysIncludesCorrelationIdHeader()
    {
        var response = await _client.GetAsync("/health");

        Assert.True(response.Headers.Contains("X-Correlation-Id"));
        Assert.False(string.IsNullOrWhiteSpace(response.Headers.GetValues("X-Correlation-Id").Single()));
    }

    [Fact]
    public async Task IncomingCorrelationId_IsEchoedBack()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("X-Correlation-Id", "test-correlation-id-123");

        var response = await _client.SendAsync(request);

        Assert.Equal("test-correlation-id-123", response.Headers.GetValues("X-Correlation-Id").Single());
    }
}