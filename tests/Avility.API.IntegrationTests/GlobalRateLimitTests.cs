using System.Net;
using Xunit;

namespace Avility.API.IntegrationTests;

public class GlobalRateLimitTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public GlobalRateLimitTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task NormalUsage_IsNotThrottledByTheGlobalLimiter()
    {
        // Sanity check that the global limiter is wired up without
        // wrongly rejecting ordinary traffic - the "Testing" permit
        // limit is deliberately generous (see Program.cs), so this
        // proves the mechanism doesn't interfere with normal usage
        // rather than proving a 429 fires under abuse.
        for (var i = 0; i < 10; i++)
        {
            var response = await _client.GetAsync("/health/live");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}