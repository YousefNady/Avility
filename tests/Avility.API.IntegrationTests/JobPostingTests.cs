using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Avility.Application.Common.Models;
using Avility.API.Common.Responses;
using Avility.Application.Auth;
using Avility.Application.Common.Constants;
using Avility.Application.Companies.Dtos;
using Avility.Application.JobPostings.Dtos;
using Avility.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Avility.API.IntegrationTests;

public class JobPostingTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public JobPostingTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private static readonly object CompanyProfilePayload = new
    {
        companyName = "Acme Inc",
        companySize = "ElevenToFifty",
        foundedYear = 2015,
        country = "Egypt",
        governorate = "Giza",
        city = "Giza"
    };

    private static readonly object PostingPayload = new
    {
        title = "Backend Engineer",
        description = "Build APIs",
        employmentType = "FullTime",
        experienceLevel = "MidLevel",
        isRemote = false,
        country = "Egypt",
        governorate = "Giza",
        city = "Giza"
    };

    private async Task<(string Token, string Email)> RegisterVerifiedCompanyAsync()
    {
        var email = $"co-{Guid.NewGuid()}@test.com";
        var register = await _client.PostAsJsonAsync("/api/v1/auth/register", new { email, password = "Password123", role = "Company" });
        register.EnsureSuccessStatusCode();
        var token = (await register.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>())!.Data!.AccessToken;

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        await _client.PostAsJsonAsync("/api/v1/companies/me", CompanyProfilePayload);
        var getMe = await _client.GetAsync("/api/v1/companies/me");
        var companyId = (await getMe.Content.ReadFromJsonAsync<ApiResponse<CompanyProfileDto>>())!.Data!.Id;

        using (var scope = _factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByEmailAsync(email);
            await userManager.AddToRoleAsync(user!, Roles.Admin);
        }

        var adminLogin = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = "Password123" });
        var adminToken = (await adminLogin.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>())!.Data!.AccessToken;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        await _client.PostAsync($"/api/v1/companies/{companyId}/verify", null);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return (token, email);
    }

    [Fact]
    public async Task Create_WithoutCompanyProfile_ReturnsBadRequest()
    {
        var email = $"co-{Guid.NewGuid()}@test.com";
        var register = await _client.PostAsJsonAsync("/api/v1/auth/register", new { email, password = "Password123", role = "Company" });
        var token = (await register.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>())!.Data!.AccessToken;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsJsonAsync("/api/v1/jobpostings", PostingPayload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Publish_WhenCompanyNotVerified_ReturnsBadRequest()
    {
        var email = $"co-{Guid.NewGuid()}@test.com";
        var register = await _client.PostAsJsonAsync("/api/v1/auth/register", new { email, password = "Password123", role = "Company" });
        var token = (await register.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>())!.Data!.AccessToken;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        await _client.PostAsJsonAsync("/api/v1/companies/me", CompanyProfilePayload);

        var create = await _client.PostAsJsonAsync("/api/v1/jobpostings", PostingPayload);
        var postingId = (await create.Content.ReadFromJsonAsync<ApiResponse<JobPostingDto>>())!.Data!.Id;

        var publish = await _client.PostAsync($"/api/v1/jobpostings/{postingId}/publish", null);

        Assert.Equal(HttpStatusCode.BadRequest, publish.StatusCode);
    }

    [Fact]
    public async Task Publish_WhenVerified_Succeeds_AndAppearsInPublicSearch()
    {
        await RegisterVerifiedCompanyAsync();

        var create = await _client.PostAsJsonAsync("/api/v1/jobpostings", PostingPayload);
        var postingId = (await create.Content.ReadFromJsonAsync<ApiResponse<JobPostingDto>>())!.Data!.Id;

        var publish = await _client.PostAsync($"/api/v1/jobpostings/{postingId}/publish", null);
        Assert.Equal(HttpStatusCode.OK, publish.StatusCode);

        using var anonymousClient = _factory.CreateClient();
        var search = await anonymousClient.GetAsync("/api/v1/jobpostings?search=Backend");
        var page = await search.Content.ReadFromJsonAsync<ApiResponse<PagedResult<JobPostingDto>>>();

        Assert.Equal(HttpStatusCode.OK, search.StatusCode);
        Assert.Contains(page!.Data!.Items, p => p.Id == postingId);
    }

    [Fact]
    public async Task GetById_DraftPosting_AsAnonymous_ReturnsNotFound()
    {
        await RegisterVerifiedCompanyAsync();
        var create = await _client.PostAsJsonAsync("/api/v1/jobpostings", PostingPayload);
        var postingId = (await create.Content.ReadFromJsonAsync<ApiResponse<JobPostingDto>>())!.Data!.Id;

        using var anonymousClient = _factory.CreateClient();
        var response = await anonymousClient.GetAsync($"/api/v1/jobpostings/{postingId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_ByNonOwningCompany_ReturnsForbidden()
    {
        await RegisterVerifiedCompanyAsync();
        var create = await _client.PostAsJsonAsync("/api/v1/jobpostings", PostingPayload);
        var postingId = (await create.Content.ReadFromJsonAsync<ApiResponse<JobPostingDto>>())!.Data!.Id;

        var otherEmail = $"co-{Guid.NewGuid()}@test.com";
        var otherRegister = await _client.PostAsJsonAsync("/api/v1/auth/register", new { email = otherEmail, password = "Password123", role = "Company" });
        var otherToken = (await otherRegister.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>())!.Data!.AccessToken;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", otherToken);
        await _client.PostAsJsonAsync("/api/v1/companies/me", CompanyProfilePayload);

        var update = await _client.PutAsJsonAsync($"/api/v1/jobpostings/{postingId}", new
        {
            title = "Hijacked",
            description = "Build APIs",
            employmentType = "FullTime",
            experienceLevel = "MidLevel",
            isRemote = false,
            country = "Egypt",
            governorate = "Giza",
            city = "Giza"
        });

        Assert.Equal(HttpStatusCode.Forbidden, update.StatusCode);
    }

    [Fact]
    public async Task GetMine_ReturnsOwnPostingsRegardlessOfStatus()
    {
        await RegisterVerifiedCompanyAsync();
        await _client.PostAsJsonAsync("/api/v1/jobpostings", PostingPayload);

        var response = await _client.GetAsync("/api/v1/jobpostings/mine");
        var page = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<JobPostingDto>>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Single(page!.Data!.Items);
    }
}
