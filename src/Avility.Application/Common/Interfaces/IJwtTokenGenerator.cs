namespace Avility.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAtUtc) GenerateAccessToken(Guid userId, string email, IEnumerable<string> roles);
    (string Token, DateTime ExpiresAtUtc) GenerateRefreshToken();
}
