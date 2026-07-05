using Avility.Application.Common.Interfaces;
using Avility.Application.Common.Utilities;
using Avility.Domain.Entities;

namespace Avility.Application.Auth;

public sealed class TokenIssuer
{
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly IApplicationDbContext _dbContext;
    private readonly IDateTime _dateTime;

    public TokenIssuer(IJwtTokenGenerator tokenGenerator, IApplicationDbContext dbContext, IDateTime dateTime)
    {
        _tokenGenerator = tokenGenerator;
        _dbContext = dbContext;
        _dateTime = dateTime;
    }

    public async Task<AuthResponse> IssueAsync(Guid userId, string email, IEnumerable<string> roles, CancellationToken cancellationToken)
    {
        var (accessToken, accessExpiresAt) = _tokenGenerator.GenerateAccessToken(userId, email, roles);
        var (refreshToken, refreshExpiresAt) = _tokenGenerator.GenerateRefreshToken();

        var refreshTokenEntity = RefreshToken.Create(userId, TokenHasher.Hash(refreshToken), refreshExpiresAt, _dateTime.UtcNow);
        _dbContext.RefreshTokens.Add(refreshTokenEntity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AuthResponse(accessToken, refreshToken, accessExpiresAt);
    }
}
