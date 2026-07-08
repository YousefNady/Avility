using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Avility.API.Common.Responses;
using Avility.Application.Auth;
using Xunit;

namespace Avility.API.IntegrationTests;

public class JobSeekerProfileTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public JobSeekerProfileTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> RegisterAndGetTokenAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email = $"js-{Guid.NewGuid()}@test.com",
            password = "Password123",
            role = "JobSeeker"
        });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        return body!.Data!.AccessToken;
    }

    private static readonly object ValidProfilePayload = new
    {
        fullName = "Sara Ahmed",
        phoneNumber = "+201234567890",
        yearsOfExperience = 3,
        currentJobTitle = "Backend Developer",
        country = "Egypt",
        governorate = "Giza",
        city = "Giza"
    };

    [Fact]
    public async Task CreateThenGetProfile_ReturnsCreatedProfile()
    {
        var token = await RegisterAndGetTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var create = await _client.PostAsJsonAsync("/api/v1/jobseekers/me", ValidProfilePayload);
        Assert.Equal(HttpStatusCode.OK, create.StatusCode);

        var get = await _client.GetAsync("/api/v1/jobseekers/me");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
    }

    [Fact]
    public async Task CreateProfile_Twice_ReturnsBadRequest()
    {
        var token = await RegisterAndGetTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        await _client.PostAsJsonAsync("/api/v1/jobseekers/me", ValidProfilePayload);
        var second = await _client.PostAsJsonAsync("/api/v1/jobseekers/me", ValidProfilePayload);

        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
    }

    [Fact]
    public async Task GetProfile_WhenNoneExists_ReturnsNotFound()
    {
        var token = await RegisterAndGetTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/v1/jobseekers/me");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetProfile_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/v1/jobseekers/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProfile_ChangesFields()
    {
        var token = await RegisterAndGetTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        await _client.PostAsJsonAsync("/api/v1/jobseekers/me", ValidProfilePayload);

        var update = await _client.PutAsJsonAsync("/api/v1/jobseekers/me", new
        {
            fullName = "Sara A.",
            headline = "Senior Backend Developer",
            phoneNumber = "+201234567890",
            yearsOfExperience = 5,
            currentJobTitle = "Senior Backend Developer",
            country = "Egypt",
            governorate = "Giza",
            city = "Giza"
        });

        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
    }

    [Fact]
    public async Task UploadResume_WithValidPdf_Succeeds()
    {
        var token = await RegisterAndGetTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        await _client.PostAsJsonAsync("/api/v1/jobseekers/me", ValidProfilePayload);

        using var content = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(new byte[] { 0x25, 0x50, 0x44, 0x46 });
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
        content.Add(fileContent, "file", "resume.pdf");

        var response = await _client.PostAsync("/api/v1/jobseekers/me/resume", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UploadResume_WithDisallowedType_ReturnsBadRequest()
    {
        var token = await RegisterAndGetTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        await _client.PostAsJsonAsync("/api/v1/jobseekers/me", ValidProfilePayload);

        using var content = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(new byte[] { 1, 2, 3 });
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
        content.Add(fileContent, "file", "resume.png");

        var response = await _client.PostAsync("/api/v1/jobseekers/me/resume", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UploadThenDownloadResume_ReturnsSameContent()
    {
        var token = await RegisterAndGetTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        await _client.PostAsJsonAsync("/api/v1/jobseekers/me", ValidProfilePayload);

        var bytes = new byte[] { 0x25, 0x50, 0x44, 0x46, 0x01 };
        using var content = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(bytes);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
        content.Add(fileContent, "file", "resume.pdf");
        await _client.PostAsync("/api/v1/jobseekers/me/resume", content);

        var download = await _client.GetAsync("/api/v1/jobseekers/me/resume");
        var downloaded = await download.Content.ReadAsByteArrayAsync();

        Assert.Equal(HttpStatusCode.OK, download.StatusCode);
        Assert.Equal(bytes, downloaded);
    }

    [Fact]
    public async Task DownloadResume_WithoutUpload_ReturnsNotFound()
    {
        var token = await RegisterAndGetTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        await _client.PostAsJsonAsync("/api/v1/jobseekers/me", ValidProfilePayload);

        var response = await _client.GetAsync("/api/v1/jobseekers/me/resume");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}