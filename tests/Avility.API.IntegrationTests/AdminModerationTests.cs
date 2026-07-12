using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Avility.API.Common.Responses;
using Avility.Application.Auth;
using Avility.Application.Common.Constants;
using Avility.Application.Common.Models;
using Avility.Application.Companies.Dtos;
using Avility.Application.JobPostings.Dtos;
using Avility.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Avility.API.IntegrationTests;

public class AdminModerationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AdminModerationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<string> PromoteToAdminAndLoginAsync(string email)
    {
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
    public async Task GetCompanies_FilteredByPending_ReturnsUnverifiedCompany()
    {
        var email = $"co-{Guid.NewGuid()}@test.com";
        var register = await _client.PostAsJsonAsync("/api/v1/auth/register", new { email, password = "Password123", role = "Company" });
        var token = (await register.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>())!.Data!.AccessToken;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        await _client.PostAsJsonAsync("/api/v1/companies/me", new
        {
            companyName = "Acme Inc",
            companySize = "ElevenToFifty",
            foundedYear = 2015,
            country = "Egypt",
            governorate = "Giza",
            city = "Giza"
        });

        var adminToken = await PromoteToAdminAndLoginAsync(email);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _client.GetAsync("/api/v1/admin/companies?status=Pending");
        var page = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<CompanyProfileDto>>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(page!.Data!.Items, c => c.CompanyName == "Acme Inc");
    }

    [Fact]
    public async Task GetCompanies_AsNonAdmin_ReturnsForbidden()
    {
        var email = $"co-{Guid.NewGuid()}@test.com";
        var register = await _client.PostAsJsonAsync("/api/v1/auth/register", new { email, password = "Password123", role = "Company" });
        var token = (await register.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>())!.Data!.AccessToken;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/v1/admin/companies");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeactivateUser_ThenLogin_Fails()
    {
        var email = $"js-{Guid.NewGuid()}@test.com";
        await _client.PostAsJsonAsync("/api/v1/auth/register", new { email, password = "Password123", role = "JobSeeker" });

        var adminToken = await PromoteToAdminAndLoginAsync(email); // promotes the same user, but we deactivate before using it again
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByEmailAsync(email);

        var deactivate = await _client.PostAsync($"/api/v1/admin/users/{user!.Id}/deactivate", null);
        Assert.Equal(HttpStatusCode.OK, deactivate.StatusCode);

        var reLogin = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = "Password123" });
        Assert.Equal(HttpStatusCode.BadRequest, reLogin.StatusCode);
    }

    [Fact]
    public async Task DeactivateUser_UnknownId_ReturnsNotFound()
    {
        var email = $"js-{Guid.NewGuid()}@test.com";
        await _client.PostAsJsonAsync("/api/v1/auth/register", new { email, password = "Password123", role = "JobSeeker" });
        var adminToken = await PromoteToAdminAndLoginAsync(email);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _client.PostAsync($"/api/v1/admin/users/{Guid.NewGuid()}/deactivate", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AdminCloseJobPosting_ClosesRegardlessOfOwnership()
    {
        var companyEmail = $"co-{Guid.NewGuid()}@test.com";
        var register = await _client.PostAsJsonAsync("/api/v1/auth/register", new { email = companyEmail, password = "Password123", role = "Company" });
        var companyToken = (await register.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>())!.Data!.AccessToken;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", companyToken);
        await _client.PostAsJsonAsync("/api/v1/companies/me", new
        {
            companyName = "Acme Inc",
            companySize = "ElevenToFifty",
            foundedYear = 2015,
            country = "Egypt",
            governorate = "Giza",
            city = "Giza"
        });
        var create = await _client.PostAsJsonAsync("/api/v1/jobpostings", new
        {
            title = "Backend Engineer",
            description = "Build APIs",
            employmentType = "FullTime",
            experienceLevel = "MidLevel",
            isRemote = true
        });
        var postingId = (await create.Content.ReadFromJsonAsync<ApiResponse<JobPostingDto>>())!.Data!.Id;

        var adminEmail = $"admin-{Guid.NewGuid()}@test.com";
        await _client.PostAsJsonAsync("/api/v1/auth/register", new { email = adminEmail, password = "Password123", role = "JobSeeker" });
        var adminToken = await PromoteToAdminAndLoginAsync(adminEmail);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var close = await _client.PostAsync($"/api/v1/admin/jobpostings/{postingId}/close", null);

        Assert.Equal(HttpStatusCode.OK, close.StatusCode);
    }
}
