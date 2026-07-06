namespace Avility.Application.Auth;

public sealed record AuthResponse(string AccessToken, string RefreshToken, DateTime AccessTokenExpiresAt);
