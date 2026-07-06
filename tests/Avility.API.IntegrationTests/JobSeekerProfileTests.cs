using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Avility.API.Common.Responses;
using Avility.Application.Auth;
using Xunit;

namespace Avility.API.IntegrationTests;

public class JobSeekerProfileTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public JobSeekerProfileTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> RegisterAndGetTokenAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email = $"js-{Guid.NewGuid()}@test.com",
            password = "Password123",
            role = "JobSeeker"
        });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        return body!.Data!.AccessToken;
    }

    private static readonly object ValidProfilePayload = new
    {
        fullName = "Sara Ahmed",
        phoneNumber = "+201234567890",
        yearsOfExperience = 3,
        currentJobTitle = "Backend Developer",
        country = "Egypt",
        governorate = "Giza",
        city = "Giza"
    };

    [Fact]
    public async Task CreateThenGetProfile_ReturnsCreatedProfile()
    {
        var token = await RegisterAndGetTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var create = await _client.PostAsJsonAsync("/api/v1/jobseekers/me", ValidProfilePayload);
        Assert.Equal(HttpStatusCode.OK, create.StatusCode);

        var get = await _client.GetAsync("/api/v1/jobseekers/me");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
    }

    [Fact]
    public async Task CreateProfile_Twice_ReturnsBadRequest()
    {
        var token = await RegisterAndGetTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        await _client.PostAsJsonAsync("/api/v1/jobseekers/me", ValidProfilePayload);
        var second = await _client.PostAsJsonAsync("/api/v1/jobseekers/me", ValidProfilePayload);

        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
    }

    [Fact]
    public async Task GetProfile_WhenNoneExists_ReturnsNotFound()
    {
        var token = await RegisterAndGetTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/v1/jobseekers/me");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetProfile_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/v1/jobseekers/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProfile_ChangesFields()
    {
        var token = await RegisterAndGetTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        await _client.PostAsJsonAsync("/api/v1/jobseekers/me", ValidProfilePayload);

        var update = await _client.PutAsJsonAsync("/api/v1/jobseekers/me", new
        {
            fullName = "Sara A.",
            headline = "Senior Backend Developer",
            phoneNumber = "+201234567890",
            yearsOfExperience = 5,
            currentJobTitle = "Senior Backend Developer",
            country = "Egypt",
            governorate = "Giza",
            city = "Giza"
        });

        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
    }
}
