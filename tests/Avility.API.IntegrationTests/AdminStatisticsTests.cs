using System.Net.Http.Headers;
using System.Net.Http.Json;
using Avility.API.Common.Responses;
using Avility.Application.Admin.Queries.GetPlatformStatistics;
using Avility.Application.Auth;
using Avility.Application.Common.Constants;
using Avility.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Avility.API.IntegrationTests;

public class AdminStatisticsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AdminStatisticsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetStatistics_ReflectsRegisteredJobSeekers()
    {
        var seekerEmail = $"js-{Guid.NewGuid()}@test.com";
        var register = await _client.PostAsJsonAsync("/api/v1/auth/register", new { email = seekerEmail, password = "Password123", role = "JobSeeker" });
        var seekerToken = (await register.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>())!.Data!.AccessToken;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", seekerToken);
        await _client.PostAsJsonAsync("/api/v1/jobseekers/me", new
        {
            fullName = "Test Seeker",
            phoneNumber = "+201234567890",
            yearsOfExperience = 1,
            currentJobTitle = "Tester",
            country = "Egypt",
            governorate = "Giza",
            city = "Giza"
        });

        var adminEmail = $"admin-{Guid.NewGuid()}@test.com";
        await _client.PostAsJsonAsync("/api/v1/auth/register", new { email = adminEmail, password = "Password123", role = "JobSeeker" });
        using (var scope = _factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByEmailAsync(adminEmail);
            await userManager.AddToRoleAsync(user!, Roles.Admin);
        }
        var adminLogin = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email = adminEmail, password = "Password123" });
        var adminToken = (await adminLogin.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>())!.Data!.AccessToken;

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var response = await _client.GetAsync("/api/v1/admin/statistics");
        var stats = (await response.Content.ReadFromJsonAsync<ApiResponse<PlatformStatisticsDto>>())!.Data!;

        Assert.True(stats.TotalJobSeekers >= 1);
        Assert.True(stats.ActiveUsers >= 2);
    }

    [Fact]
    public async Task GetStatistics_AsNonAdmin_ReturnsForbidden()
    {
        var email = $"js-{Guid.NewGuid()}@test.com";
        var register = await _client.PostAsJsonAsync("/api/v1/auth/register", new { email, password = "Password123", role = "JobSeeker" });
        var token = (await register.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>())!.Data!.AccessToken;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/v1/admin/statistics");

        Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }
}