using System.Net.Http;
using Xunit;

namespace Avility.API.IntegrationTests;

public class SecurityHeadersTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SecurityHeadersTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Theory]
    [InlineData("X-Content-Type-Options", "nosniff")]
    [InlineData("X-Frame-Options", "DENY")]
    [InlineData("Referrer-Policy", "strict-origin-when-cross-origin")]
    public async Task Response_IncludesExpectedSecurityHeader(string headerName, string expectedValue)
    {
        var response = await _client.GetAsync("/health");

        Assert.True(response.Headers.Contains(headerName));
        Assert.Equal(expectedValue, response.Headers.GetValues(headerName).Single());
    }

    [Fact]
    public async Task ErrorResponse_StillIncludesSecurityHeaders()
    {
        // A 404 from a non-existent route still passes through the
        // middleware pipeline before the exception handler, so it should
        // carry the same headers as a successful response.
        var response = await _client.GetAsync("/api/v1/this-route-does-not-exist");

        Assert.True(response.Headers.Contains("X-Content-Type-Options"));
    }
}