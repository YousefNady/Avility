using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Avility.API.Common.Responses;
using Avility.Application.Auth;
using Avility.Application.Common.Constants;
using Avility.Application.Common.Models;
using Avility.Application.Companies.Dtos;
using Avility.Application.JobApplications.Dtos;
using Avility.Application.JobPostings.Dtos;
using Avility.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Avility.API.IntegrationTests;

public class JobApplicationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public JobApplicationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<string> RegisterJobSeekerAsync()
    {
        var email = $"js-{Guid.NewGuid()}@test.com";
        var register = await _client.PostAsJsonAsync("/api/v1/auth/register", new { email, password = "Password123", role = "JobSeeker" });
        var token = (await register.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>())!.Data!.AccessToken;

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        await _client.PostAsJsonAsync("/api/v1/jobseekers/me", new
        {
            fullName = "Sara Ahmed",
            phoneNumber = "+201234567890",
            yearsOfExperience = 3,
            currentJobTitle = "Backend Developer",
            country = "Egypt",
            governorate = "Giza",
            city = "Giza"
        });

        return token;
    }

    private async Task<(string Token, Guid PostingId)> RegisterCompanyWithPublishedPostingAsync()
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
        var create = await _client.PostAsJsonAsync("/api/v1/jobpostings", new
        {
            title = "Backend Engineer",
            description = "Build APIs",
            employmentType = "FullTime",
            experienceLevel = "MidLevel",
            isRemote = true
        });
        var postingId = (await create.Content.ReadFromJsonAsync<ApiResponse<JobPostingDto>>())!.Data!.Id;
        await _client.PostAsync($"/api/v1/jobpostings/{postingId}/publish", null);

        return (token, postingId);
    }

    [Fact]
    public async Task Apply_ToPublishedPosting_Succeeds()
    {
        var (_, postingId) = await RegisterCompanyWithPublishedPostingAsync();
        var seekerToken = await RegisterJobSeekerAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", seekerToken);

        var response = await _client.PostAsJsonAsync("/api/v1/jobapplications", new { jobPostingId = postingId, coverLetter = "I'd love to join!" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Apply_Twice_ReturnsBadRequest()
    {
        var (_, postingId) = await RegisterCompanyWithPublishedPostingAsync();
        var seekerToken = await RegisterJobSeekerAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", seekerToken);

        await _client.PostAsJsonAsync("/api/v1/jobapplications", new { jobPostingId = postingId, coverLetter = (string?)null });
        var second = await _client.PostAsJsonAsync("/api/v1/jobapplications", new { jobPostingId = postingId, coverLetter = (string?)null });

        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
    }

    [Fact]
    public async Task Apply_ThenWithdraw_ThenReapply_ReturnsBadRequest()
    {
        var (_, postingId) = await RegisterCompanyWithPublishedPostingAsync();
        var seekerToken = await RegisterJobSeekerAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", seekerToken);

        var apply = await _client.PostAsJsonAsync("/api/v1/jobapplications", new { jobPostingId = postingId, coverLetter = (string?)null });
        var applicationId = (await apply.Content.ReadFromJsonAsync<ApiResponse<JobApplicationDto>>())!.Data!.Id;

        var withdraw = await _client.PostAsync($"/api/v1/jobapplications/{applicationId}/withdraw", null);
        Assert.Equal(HttpStatusCode.OK, withdraw.StatusCode);

        var reapply = await _client.PostAsJsonAsync("/api/v1/jobapplications", new { jobPostingId = postingId, coverLetter = (string?)null });
        Assert.Equal(HttpStatusCode.BadRequest, reapply.StatusCode);
    }

    [Fact]
    public async Task Apply_WithoutJobSeekerProfile_ReturnsBadRequest()
    {
        var (_, postingId) = await RegisterCompanyWithPublishedPostingAsync();

        var email = $"js-{Guid.NewGuid()}@test.com";
        var register = await _client.PostAsJsonAsync("/api/v1/auth/register", new { email, password = "Password123", role = "JobSeeker" });
        var token = (await register.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>())!.Data!.AccessToken;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsJsonAsync("/api/v1/jobapplications", new { jobPostingId = postingId, coverLetter = (string?)null });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Withdraw_ByNonOwner_ReturnsForbidden()
    {
        var (_, postingId) = await RegisterCompanyWithPublishedPostingAsync();
        var seekerToken = await RegisterJobSeekerAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", seekerToken);
        var apply = await _client.PostAsJsonAsync("/api/v1/jobapplications", new { jobPostingId = postingId, coverLetter = (string?)null });
        var applicationId = (await apply.Content.ReadFromJsonAsync<ApiResponse<JobApplicationDto>>())!.Data!.Id;

        var otherSeekerToken = await RegisterJobSeekerAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", otherSeekerToken);

        var response = await _client.PostAsync($"/api/v1/jobapplications/{applicationId}/withdraw", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CompanyOwner_CanReviewAndAcceptApplication()
    {
        var (companyToken, postingId) = await RegisterCompanyWithPublishedPostingAsync();
        var seekerToken = await RegisterJobSeekerAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", seekerToken);
        var apply = await _client.PostAsJsonAsync("/api/v1/jobapplications", new { jobPostingId = postingId, coverLetter = (string?)null });
        var applicationId = (await apply.Content.ReadFromJsonAsync<ApiResponse<JobApplicationDto>>())!.Data!.Id;

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", companyToken);
        var review = await _client.PostAsync($"/api/v1/jobapplications/{applicationId}/under-review", null);
        var accept = await _client.PostAsync($"/api/v1/jobapplications/{applicationId}/accept", null);

        Assert.Equal(HttpStatusCode.OK, review.StatusCode);
        Assert.Equal(HttpStatusCode.OK, accept.StatusCode);
    }

    [Fact]
    public async Task GetApplicants_ByNonOwningCompany_ReturnsForbidden()
    {
        var (_, postingId) = await RegisterCompanyWithPublishedPostingAsync();

        var otherEmail = $"co-{Guid.NewGuid()}@test.com";
        var otherRegister = await _client.PostAsJsonAsync("/api/v1/auth/register", new { email = otherEmail, password = "Password123", role = "Company" });
        var otherToken = (await otherRegister.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>())!.Data!.AccessToken;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", otherToken);

        var response = await _client.GetAsync($"/api/v1/jobpostings/{postingId}/applicants");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetMine_ReturnsSeekerOwnApplications()
    {
        var (_, postingId) = await RegisterCompanyWithPublishedPostingAsync();
        var seekerToken = await RegisterJobSeekerAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", seekerToken);
        await _client.PostAsJsonAsync("/api/v1/jobapplications", new { jobPostingId = postingId, coverLetter = (string?)null });

        var response = await _client.GetAsync("/api/v1/jobapplications/mine");
        var page = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<JobApplicationDto>>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Single(page!.Data!.Items);
    }
}
