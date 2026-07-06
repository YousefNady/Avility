using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Avility.API.Common.Responses;
using Avility.Application.Auth;
using Avility.Application.Common.Constants;
using Avility.Application.Companies.Dtos;
using Avility.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Avility.API.IntegrationTests;

public class CompanyProfileTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public CompanyProfileTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private static readonly object ValidProfilePayload = new
    {
        companyName = "Acme Inc",
        companySize = "ElevenToFifty",
        foundedYear = 2015,
        country = "Egypt",
        governorate = "Giza",
        city = "Giza"
    };

    private async Task<(string Token, string Email)> RegisterCompanyAsync()
    {
        var email = $"co-{Guid.NewGuid()}@test.com";
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email,
            password = "Password123",
            role = "Company"
        });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        return (body!.Data!.AccessToken, email);
    }

    [Fact]
    public async Task CreateThenGetProfile_ReturnsCreatedProfile()
    {
        var (token, _) = await RegisterCompanyAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var create = await _client.PostAsJsonAsync("/api/v1/companies/me", ValidProfilePayload);
        Assert.Equal(HttpStatusCode.OK, create.StatusCode);

        var get = await _client.GetAsync("/api/v1/companies/me");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
    }

    [Fact]
    public async Task CreateProfile_Twice_ReturnsBadRequest()
    {
        var (token, _) = await RegisterCompanyAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        await _client.PostAsJsonAsync("/api/v1/companies/me", ValidProfilePayload);
        var second = await _client.PostAsJsonAsync("/api/v1/companies/me", ValidProfilePayload);

        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
    }

    [Fact]
    public async Task CreateProfile_WithInvalidCompanySize_ReturnsBadRequest()
    {
        var (token, _) = await RegisterCompanyAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsJsonAsync("/api/v1/companies/me", new
        {
            companyName = "Acme Inc",
            companySize = "NotARealSize",
            foundedYear = 2015,
            country = "Egypt",
            governorate = "Giza",
            city = "Giza"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetProfile_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/v1/companies/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Verify_WithAdminRole_SetsVerifiedStatus()
    {
        var (token, email) = await RegisterCompanyAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        await _client.PostAsJsonAsync("/api/v1/companies/me", ValidProfilePayload);

        var getMe = await _client.GetAsync("/api/v1/companies/me");
        var profile = (await getMe.Content.ReadFromJsonAsync<ApiResponse<CompanyProfileDto>>())!.Data!;

        using (var scope = _factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByEmailAsync(email);
            await userManager.AddToRoleAsync(user!, Roles.Admin);
        }

        var adminLogin = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = "Password123" });
        var adminToken = (await adminLogin.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>())!.Data!.AccessToken;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var verify = await _client.PostAsync($"/api/v1/companies/{profile.Id}/verify", null);

        Assert.Equal(HttpStatusCode.OK, verify.StatusCode);
    }

    [Fact]
    public async Task Verify_WithoutAdminRole_ReturnsForbidden()
    {
        var (token, _) = await RegisterCompanyAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        await _client.PostAsJsonAsync("/api/v1/companies/me", ValidProfilePayload);

        var response = await _client.PostAsync($"/api/v1/companies/{Guid.NewGuid()}/verify", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
