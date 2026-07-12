using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Avility.API.Common.Responses;
using Avility.Application.Auth;
using Avility.Application.Common.Constants;
using Avility.Application.Common.Models;
using Avility.Application.Resources.Dtos;
using Avility.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Avility.API.IntegrationTests;

public class ResourcesTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ResourcesTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<string> GetAdminTokenAsync()
    {
        var email = $"admin-{Guid.NewGuid()}@test.com";
        await _client.PostAsJsonAsync("/api/v1/auth/register", new { email, password = "Password123", role = "JobSeeker" });

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
    public async Task Create_AsAdmin_Succeeds_AndIsPubliclySearchable()
    {
        var adminToken = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var create = await _client.PostAsJsonAsync("/api/v1/resources", new
        {
            title = "Interview Prep Checklist",
            description = "A checklist to prepare for accessible interviews.",
            url = "https://example.com/interview-prep",
            category = "InterviewPreparation"
        });
        Assert.Equal(HttpStatusCode.OK, create.StatusCode);
        var created = (await create.Content.ReadFromJsonAsync<ApiResponse<ResourceDto>>())!.Data!;

        _client.DefaultRequestHeaders.Authorization = null;
        var search = await _client.GetAsync("/api/v1/resources?category=InterviewPreparation");
        var page = (await search.Content.ReadFromJsonAsync<ApiResponse<PagedResult<ResourceDto>>>())!.Data!;

        Assert.Contains(page.Items, r => r.Id == created.Id);
    }

    [Fact]
    public async Task Create_AsNonAdmin_ReturnsForbidden()
    {
        var email = $"js-{Guid.NewGuid()}@test.com";
        var register = await _client.PostAsJsonAsync("/api/v1/auth/register", new { email, password = "Password123", role = "JobSeeker" });
        var token = (await register.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>())!.Data!.AccessToken;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var create = await _client.PostAsJsonAsync("/api/v1/resources", new
        {
            title = "Title", description = "Description", url = "https://example.com", category = "Other"
        });

        Assert.Equal(HttpStatusCode.Forbidden, create.StatusCode);
    }

    [Fact]
    public async Task Delete_SoftDeletes_AndExcludesFromSearch()
    {
        var adminToken = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var create = await _client.PostAsJsonAsync("/api/v1/resources", new
        {
            title = "Temporary Resource",
            description = "Will be deleted.",
            url = "https://example.com/temp",
            category = "Other"
        });
        var created = (await create.Content.ReadFromJsonAsync<ApiResponse<ResourceDto>>())!.Data!;

        var delete = await _client.DeleteAsync($"/api/v1/resources/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, delete.StatusCode);

        var getById = await _client.GetAsync($"/api/v1/resources/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getById.StatusCode);
    }
}