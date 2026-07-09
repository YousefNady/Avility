using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
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

public class JobApplicationNotificationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public JobApplicationNotificationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<(string Token, string Email)> RegisterJobSeekerAsync()
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

        return (token, email);
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
    public async Task Accept_SendsAcceptedEmailToJobSeeker()
    {
        FakeEmailSender.Clear();
        var (companyToken, postingId) = await RegisterCompanyWithPublishedPostingAsync();
        var (seekerToken, seekerEmail) = await RegisterJobSeekerAsync();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", seekerToken);
        var apply = await _client.PostAsJsonAsync("/api/v1/jobapplications", new { jobPostingId = postingId, coverLetter = (string?)null });
        var applicationId = (await apply.Content.ReadFromJsonAsync<ApiResponse<Avility.Application.JobApplications.Dtos.JobApplicationDto>>())!.Data!.Id;

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", companyToken);
        var accept = await _client.PostAsync($"/api/v1/jobapplications/{applicationId}/accept", null);

        Assert.Equal(HttpStatusCode.OK, accept.StatusCode);
        Assert.Contains(FakeEmailSender.Sent, e => e.ToEmail == seekerEmail && e.Subject.Contains("accepted"));
    }

    [Fact]
    public async Task Reject_SendsRejectedEmailToJobSeeker()
    {
        FakeEmailSender.Clear();
        var (companyToken, postingId) = await RegisterCompanyWithPublishedPostingAsync();
        var (seekerToken, seekerEmail) = await RegisterJobSeekerAsync();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", seekerToken);
        var apply = await _client.PostAsJsonAsync("/api/v1/jobapplications", new { jobPostingId = postingId, coverLetter = (string?)null });
        var applicationId = (await apply.Content.ReadFromJsonAsync<ApiResponse<Avility.Application.JobApplications.Dtos.JobApplicationDto>>())!.Data!.Id;

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", companyToken);
        var reject = await _client.PostAsync($"/api/v1/jobapplications/{applicationId}/reject", null);

        Assert.Equal(HttpStatusCode.OK, reject.StatusCode);
        Assert.Contains(FakeEmailSender.Sent, e => e.ToEmail == seekerEmail && e.Subject.Contains("rejected"));
    }
}