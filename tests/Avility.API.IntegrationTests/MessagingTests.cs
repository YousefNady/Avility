using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Avility.API.Common.Responses;
using Avility.Application.Auth;
using Avility.Application.Common.Constants;
using Avility.Application.Companies.Dtos;
using Avility.Application.Common.Models;
using Avility.Application.JobPostings.Dtos;
using Avility.Application.Messages.Dtos;
using Avility.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Avility.API.IntegrationTests;

public class MessagingTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public MessagingTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<string> AuthTokenAsync(string email, string role)
    {
        var register = await _client.PostAsJsonAsync("/api/v1/auth/register", new { email, password = "Password123", role });
        return (await register.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>())!.Data!.AccessToken;
    }

    private async Task<(string CompanyToken, Guid PostingId)> SetUpVerifiedCompanyWithPublishedPostingAsync()
    {
        var companyEmail = $"co-{Guid.NewGuid()}@test.com";
        var companyToken = await AuthTokenAsync(companyEmail, "Company");

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", companyToken);
        await _client.PostAsJsonAsync("/api/v1/companies/me", new
        {
            companyName = "Acme Inc", companySize = "ElevenToFifty", foundedYear = 2015,
            country = "Egypt", governorate = "Giza", city = "Giza"
        });
        var getMe = await _client.GetAsync("/api/v1/companies/me");
        var companyId = (await getMe.Content.ReadFromJsonAsync<ApiResponse<CompanyProfileDto>>())!.Data!.Id;

        var adminEmail = $"admin-{Guid.NewGuid()}@test.com";
        await AuthTokenAsync(adminEmail, "JobSeeker");
        using (var scope = _factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByEmailAsync(adminEmail);
            await userManager.AddToRoleAsync(user!, Roles.Admin);
        }
        var adminLogin = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email = adminEmail, password = "Password123" });
        var adminToken = (await adminLogin.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>())!.Data!.AccessToken;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        await _client.PostAsync($"/api/v1/companies/{companyId}/verify", null);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", companyToken);
        var create = await _client.PostAsJsonAsync("/api/v1/jobpostings", new
        {
            title = "Backend Engineer", description = "Build APIs",
            employmentType = "FullTime", experienceLevel = "MidLevel", isRemote = true
        });
        var postingId = (await create.Content.ReadFromJsonAsync<ApiResponse<JobPostingDto>>())!.Data!.Id;
        await _client.PostAsync($"/api/v1/jobpostings/{postingId}/publish", null);

        return (companyToken, postingId);
    }

    private async Task<Guid> ApplyAsJobSeekerAsync(string token, Guid postingId)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        await _client.PostAsJsonAsync("/api/v1/jobseekers/me", new
        {
            fullName = "Sara Ahmed", phoneNumber = "+201234567890", yearsOfExperience = 3,
            currentJobTitle = "Backend Developer", country = "Egypt", governorate = "Giza", city = "Giza"
        });
        var apply = await _client.PostAsJsonAsync("/api/v1/jobapplications", new { jobPostingId = postingId, coverLetter = (string?)null });
        return (await apply.Content.ReadFromJsonAsync<ApiResponse<Avility.Application.JobApplications.Dtos.JobApplicationDto>>())!.Data!.Id;
    }

    [Fact]
    public async Task JobSeekerAndCompany_CanExchangeMessages_OnSharedApplication()
    {
        var (companyToken, postingId) = await SetUpVerifiedCompanyWithPublishedPostingAsync();
        var seekerToken = await AuthTokenAsync($"js-{Guid.NewGuid()}@test.com", "JobSeeker");
        var applicationId = await ApplyAsJobSeekerAsync(seekerToken, postingId);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", seekerToken);
        var seekerMessage = await _client.PostAsJsonAsync($"/api/v1/jobapplications/{applicationId}/messages", new { body = "Hi, I'm very interested in this role!" });
        Assert.Equal(HttpStatusCode.OK, seekerMessage.StatusCode);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", companyToken);
        var companyReply = await _client.PostAsJsonAsync($"/api/v1/jobapplications/{applicationId}/messages", new { body = "Thanks! Can you share your availability?" });
        Assert.Equal(HttpStatusCode.OK, companyReply.StatusCode);

        var thread = await _client.GetAsync($"/api/v1/jobapplications/{applicationId}/messages");
        var page = (await thread.Content.ReadFromJsonAsync<ApiResponse<PagedResult<MessageDto>>>())!.Data!;

        Assert.Equal(2, page.Items.Count);
        Assert.Equal("Hi, I'm very interested in this role!", page.Items[0].Body);
        Assert.Equal("Thanks! Can you share your availability?", page.Items[1].Body);
    }

    [Fact]
    public async Task UnrelatedUser_CannotSendOrReadMessages()
    {
        var (_, postingId) = await SetUpVerifiedCompanyWithPublishedPostingAsync();
        var seekerToken = await AuthTokenAsync($"js-{Guid.NewGuid()}@test.com", "JobSeeker");
        var applicationId = await ApplyAsJobSeekerAsync(seekerToken, postingId);

        var outsiderToken = await AuthTokenAsync($"js-{Guid.NewGuid()}@test.com", "JobSeeker");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", outsiderToken);

        var send = await _client.PostAsJsonAsync($"/api/v1/jobapplications/{applicationId}/messages", new { body = "Not my business" });
        Assert.Equal(HttpStatusCode.Forbidden, send.StatusCode);

        var read = await _client.GetAsync($"/api/v1/jobapplications/{applicationId}/messages");
        Assert.Equal(HttpStatusCode.Forbidden, read.StatusCode);
    }
    
    [Fact]
    public async Task SendMessage_ViaRest_InvokesMessageNotifier()
    {
        FakeMessageNotifier.Clear();
        var (companyToken, postingId) = await SetUpVerifiedCompanyWithPublishedPostingAsync();
        var seekerToken = await AuthTokenAsync($"js-{Guid.NewGuid()}@test.com", "JobSeeker");
        var applicationId = await ApplyAsJobSeekerAsync(seekerToken, postingId);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", seekerToken);
        await _client.PostAsJsonAsync($"/api/v1/jobapplications/{applicationId}/messages", new { body = "Hello via REST" });

        Assert.Contains(FakeMessageNotifier.Notified, m => m.JobApplicationId == applicationId && m.Body == "Hello via REST");
    }
}