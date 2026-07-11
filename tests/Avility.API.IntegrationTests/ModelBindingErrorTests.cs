using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Avility.API.IntegrationTests;

public class ModelBindingErrorTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ModelBindingErrorTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task MalformedJsonBody_ReturnsApiResponseEnvelope_NotDefaultProblemDetails()
    {
        var content = new StringContent("{ this is not valid json", Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/v1/auth/register", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;

        // The default ASP.NET Core shape uses "title"/"status"/"errors" at
        // the top level with no "success" field - asserting our envelope
        // is present confirms InvalidModelStateResponseFactory is wired up.
        Assert.False(body.GetProperty("success").GetBoolean());
        Assert.True(body.TryGetProperty("message", out _));
    }
}