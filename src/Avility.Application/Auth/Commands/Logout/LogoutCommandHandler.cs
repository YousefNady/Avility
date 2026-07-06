using Avility.Application.Common.Interfaces;
using Avility.Application.Common.Utilities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Avility.Application.Auth.Commands.Logout;

public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IDateTime _dateTime;

    public LogoutCommandHandler(IApplicationDbContext dbContext, IDateTime dateTime)
    {
        _dbContext = dbContext;
        _dateTime = dateTime;
    }

    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = TokenHasher.Hash(request.RefreshToken);

        var storedToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);

        if (storedToken is not null && !storedToken.IsRevoked)
        {
            storedToken.Revoke(_dateTime.UtcNow);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
