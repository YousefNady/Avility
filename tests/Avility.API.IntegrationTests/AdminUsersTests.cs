using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Avility.API.Common.Responses;
using Avility.Application.Auth;
using Avility.Application.Common.Constants;
using Avility.Application.Common.Models;
using Avility.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Avility.API.IntegrationTests;

public class AdminUsersTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AdminUsersTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<string> RegisterAndGetTokenAsync(string role)
    {
        var email = $"user-{Guid.NewGuid()}@test.com";
        var register = await _client.PostAsJsonAsync("/api/v1/auth/register", new { email, password = "Password123", role });
        return (await register.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>())!.Data!.AccessToken;
    }

    private async Task<string> RegisterAndGetAdminTokenAsync()
    {
        var email = $"admin-{Guid.NewGuid()}@test.com";
        var register = await _client.PostAsJsonAsync("/api/v1/auth/register", new { email, password = "Password123", role = "JobSeeker" });

        using (var scope = _factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByEmailAsync(email);
            await userManager.AddToRoleAsync(user!, Roles.Admin);
        }

        var login = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = "Password123" });
        return (await login.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>())!.Data!.AccessToken;
    }

    [Fact]
    public async Task GetUsers_AsAdmin_ReturnsPagedUsers()
    {
        await RegisterAndGetTokenAsync("JobSeeker");
        var adminToken = await RegisterAndGetAdminTokenAsync();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var response = await _client.GetAsync("/api/v1/admin/users?pageSize=5");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = (await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<UserSummaryDto>>>())!.Data!;
        Assert.True(page.TotalCount >= 1);
    }

    [Fact]
    public async Task GetUsers_AsNonAdmin_ReturnsForbidden()
    {
        var token = await RegisterAndGetTokenAsync("JobSeeker");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/v1/admin/users");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
    
    [Fact]
    public async Task GetUserDetails_ForJobSeeker_IncludesJobSeekerProfileOnly()
    {
        var seekerEmail = $"js-{Guid.NewGuid()}@test.com";
        var register = await _client.PostAsJsonAsync("/api/v1/auth/register", new { email = seekerEmail, password = "Password123", role = "JobSeeker" });
        var seekerToken = (await register.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>())!.Data!.AccessToken;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", seekerToken);
        await _client.PostAsJsonAsync("/api/v1/jobseekers/me", new
        {
            fullName = "Detail Test Seeker", phoneNumber = "+201234567890", yearsOfExperience = 2,
            currentJobTitle = "Tester", country = "Egypt", governorate = "Giza", city = "Giza"
        });

        Guid userId;
        using (var scope = _factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            userId = (await userManager.FindByEmailAsync(seekerEmail))!.Id;
        }

        var adminEmail = $"admin-{Guid.NewGuid()}@test.com";
        await _client.PostAsJsonAsync("/api/v1/auth/register", new { email = adminEmail, password = "Password123", role = "JobSeeker" });
        using (var scope = _factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var admin = await userManager.FindByEmailAsync(adminEmail);
            await userManager.AddToRoleAsync(admin!, Roles.Admin);
        }
        var adminLogin = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email = adminEmail, password = "Password123" });
        var adminToken = (await adminLogin.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>())!.Data!.AccessToken;

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var response = await _client.GetAsync($"/api/v1/admin/users/{userId}");
        var details = (await response.Content.ReadFromJsonAsync<ApiResponse<Avility.Application.Admin.Queries.GetUserDetails.UserDetailsDto>>())!.Data!;

        Assert.NotNull(details.JobSeekerProfile);
        Assert.Null(details.CompanyProfile);
        Assert.Equal("Detail Test Seeker", details.JobSeekerProfile!.FullName);
    }
}