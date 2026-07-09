using System.Net;
using System.Net.Http.Json;
using Avility.API.Common.Responses;
using Avility.Application.Auth;
using Xunit;

namespace Avility.API.IntegrationTests;

public class PasswordResetTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public PasswordResetTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> RegisterAsync(string email)
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email,
            password = "Password123",
            role = "JobSeeker"
        });
        response.EnsureSuccessStatusCode();
        return email;
    }

    [Fact]
    public async Task ForgotPassword_ExistingEmail_SendsEmailAndReturnsOk()
    {
        FakeEmailSender.Clear();
        var email = await RegisterAsync($"reset-{Guid.NewGuid()}@test.com");

        var response = await _client.PostAsJsonAsync("/api/v1/auth/forgot-password", new { email });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(FakeEmailSender.Sent, e => e.ToEmail == email);
    }

    [Fact]
    public async Task ForgotPassword_UnknownEmail_StillReturnsOk()
    {
        FakeEmailSender.Clear();
        var email = $"unknown-{Guid.NewGuid()}@test.com";

        var response = await _client.PostAsJsonAsync("/api/v1/auth/forgot-password", new { email });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain(FakeEmailSender.Sent, e => e.ToEmail == email);
    }

    [Fact]
    public async Task ResetPassword_WithValidToken_AllowsLoginWithNewPassword()
    {
        FakeEmailSender.Clear();
        var email = await RegisterAsync($"reset-{Guid.NewGuid()}@test.com");

        await _client.PostAsJsonAsync("/api/v1/auth/forgot-password", new { email });
        var sent = FakeEmailSender.Sent.Single(e => e.ToEmail == email);
        var token = sent.Body.Split("Reset token: ")[1].Split('\n')[0].Trim();

        var reset = await _client.PostAsJsonAsync("/api/v1/auth/reset-password", new
        {
            email,
            token,
            newPassword = "BrandNewPassword123"
        });
        Assert.Equal(HttpStatusCode.OK, reset.StatusCode);

        var loginOld = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = "Password123" });
        Assert.Equal(HttpStatusCode.BadRequest, loginOld.StatusCode);

        var loginNew = await _client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = "BrandNewPassword123" });
        Assert.Equal(HttpStatusCode.OK, loginNew.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_WithInvalidToken_ReturnsBadRequest()
    {
        var email = await RegisterAsync($"reset-{Guid.NewGuid()}@test.com");

        var reset = await _client.PostAsJsonAsync("/api/v1/auth/reset-password", new
        {
            email,
            token = "not-a-real-token",
            newPassword = "BrandNewPassword123"
        });

        Assert.Equal(HttpStatusCode.BadRequest, reset.StatusCode);
    }
}