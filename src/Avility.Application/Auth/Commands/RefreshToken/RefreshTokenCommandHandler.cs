using Avility.Application.Common.Interfaces;
using Avility.Application.Common.Utilities;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Avility.Application.Auth.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IIdentityService _identityService;
    private readonly TokenIssuer _tokenIssuer;
    private readonly IDateTime _dateTime;

    public RefreshTokenCommandHandler(
        IApplicationDbContext dbContext,
        IIdentityService identityService,
        TokenIssuer tokenIssuer,
        IDateTime dateTime)
    {
        _dbContext = dbContext;
        _identityService = identityService;
        _tokenIssuer = tokenIssuer;
        _dateTime = dateTime;
    }

    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = TokenHasher.Hash(request.RefreshToken);

        var storedToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);

        if (storedToken is null || !storedToken.IsActive(_dateTime.UtcNow))
        {
            throw new ValidationException(new[] { new ValidationFailure("RefreshToken", "Invalid or expired refresh token.") });
        }

        var userInfo = await _identityService.GetUserInfoAsync(storedToken.UserId);
        if (userInfo is null)
        {
            throw new ValidationException(new[] { new ValidationFailure("RefreshToken", "Invalid or expired refresh token.") });
        }

        return await _tokenIssuer.RotateAsync(storedToken, userInfo.Value.Email, userInfo.Value.Roles, cancellationToken);
    }
}
