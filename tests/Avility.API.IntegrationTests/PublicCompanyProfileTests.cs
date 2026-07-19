using System.Net;
using System.Net.Http.Json;
using Avility.API.Common.Responses;
using Avility.Application.Companies.Dtos;
using Xunit;

namespace Avility.API.IntegrationTests;

public class PublicCompanyProfileTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public PublicCompanyProfileTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetPublicProfile_ExistingCompany_ReturnsPublicFieldsOnly()
    {
        // NOTE: adjust this setup to your existing pattern for
        // registering + creating a company profile (see JobPostingTests
        // or MessagingTests for the established helper shape).
        var email = $"co-{Guid.NewGuid()}@test.com";
        var register = await _client.PostAsJsonAsync("/api/v1/auth/register", new { email, password = "Password123", role = "Company" });
        var token = (await register.Content.ReadFromJsonAsync<ApiResponse<Avility.Application.Auth.AuthResponse>>())!.Data!.AccessToken;
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        await _client.PostAsJsonAsync("/api/v1/companies/me", new
        {
            companyName = "Public Test Co",
            companySize = "ElevenToFifty",
            foundedYear = 2020,
            country = "Egypt",
            governorate = "Giza",
            city = "Giza"
        });
        var me = await _client.GetAsync("/api/v1/companies/me");
        var companyId = (await me.Content.ReadFromJsonAsync<ApiResponse<CompanyProfileDto>>())!.Data!.Id;

        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync($"/api/v1/companies/{companyId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var profile = (await response.Content.ReadFromJsonAsync<ApiResponse<PublicCompanyProfileDto>>())!.Data!;
        Assert.Equal("Public Test Co", profile.CompanyName);
    }

    [Fact]
    public async Task GetPublicProfile_NonexistentCompany_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/v1/companies/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}