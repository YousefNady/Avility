using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Avility.API.Common.Responses;
using Avility.Application.Auth;
using Avility.Application.Companies.Dtos;
using Xunit;

namespace Avility.API.IntegrationTests;

public class CompanyLogoTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CompanyLogoTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> RegisterAndGetTokenAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email = $"co-{Guid.NewGuid()}@test.com",
            password = "Password123",
            role = "Company"
        });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        return body!.Data!.AccessToken;
    }

    private static readonly object ValidProfilePayload = new
    {
        companyName = "Acme Inc",
        companySize = "OneToTen", // adjust to actual enum member name if it differs
        foundedYear = 2020,
        country = "Egypt",
        governorate = "Giza",
        city = "Giza"
    };

    private async Task<Guid> RegisterCompanyAsync()
    {
        var token = await RegisterAndGetTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var create = await _client.PostAsJsonAsync("/api/v1/companies/me", ValidProfilePayload);
        create.EnsureSuccessStatusCode();
        var body = await create.Content.ReadFromJsonAsync<ApiResponse<CompanyProfileDto>>();
        return body!.Data!.Id;
    }

    [Fact]
    public async Task UploadLogo_WithValidPng_Succeeds()
    {
        await RegisterCompanyAsync();

        using var content = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(new byte[] { 0x89, 0x50, 0x4E, 0x47 });
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        content.Add(fileContent, "file", "logo.png");

        var response = await _client.PostAsync("/api/v1/companies/me/logo", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UploadLogo_WithDisallowedType_ReturnsBadRequest()
    {
        await RegisterCompanyAsync();

        using var content = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(new byte[] { 0x25, 0x50, 0x44, 0x46 });
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        content.Add(fileContent, "file", "logo.pdf");

        var response = await _client.PostAsync("/api/v1/companies/me/logo", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UploadThenGetLogo_Anonymously_ReturnsSameContent()
    {
        var companyId = await RegisterCompanyAsync();

        var bytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x01 };
        using var content = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(bytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        content.Add(fileContent, "file", "logo.png");
        await _client.PostAsync("/api/v1/companies/me/logo", content);

        _client.DefaultRequestHeaders.Authorization = null; // anonymous access
        var download = await _client.GetAsync($"/api/v1/companies/{companyId}/logo");
        var downloaded = await download.Content.ReadAsByteArrayAsync();

        Assert.Equal(HttpStatusCode.OK, download.StatusCode);
        Assert.Equal(bytes, downloaded);
    }

    [Fact]
    public async Task GetLogo_WithoutUpload_ReturnsNotFound()
    {
        var companyId = await RegisterCompanyAsync();

        var response = await _client.GetAsync($"/api/v1/companies/{companyId}/logo");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteLogo_RemovesIt()
    {
        var companyId = await RegisterCompanyAsync();

        using var content = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(new byte[] { 0x89, 0x50, 0x4E, 0x47 });
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        content.Add(fileContent, "file", "logo.png");
        await _client.PostAsync("/api/v1/companies/me/logo", content);

        var delete = await _client.DeleteAsync("/api/v1/companies/me/logo");
        Assert.Equal(HttpStatusCode.OK, delete.StatusCode);

        _client.DefaultRequestHeaders.Authorization = null;
        var getAfterDelete = await _client.GetAsync($"/api/v1/companies/{companyId}/logo");
        Assert.Equal(HttpStatusCode.NotFound, getAfterDelete.StatusCode);
    }

    [Fact]
    public async Task UploadLogo_WithoutToken_ReturnsUnauthorized()
    {
        using var content = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(new byte[] { 0x89, 0x50, 0x4E, 0x47 });
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        content.Add(fileContent, "file", "logo.png");

        var response = await _client.PostAsync("/api/v1/companies/me/logo", content);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}