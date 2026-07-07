using Avility.Application.Common.Exceptions;
using Avility.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Avility.Application.Admin.Commands.DeactivateUser;

public sealed class DeactivateUserCommandHandler : IRequestHandler<DeactivateUserCommand>
{
    private readonly IIdentityService _identityService;
    private readonly IApplicationDbContext _dbContext;
    private readonly IDateTime _dateTime;

    public DeactivateUserCommandHandler(IIdentityService identityService, IApplicationDbContext dbContext, IDateTime dateTime)
    {
        _identityService = identityService;
        _dbContext = dbContext;
        _dateTime = dateTime;
    }

    public async Task Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        var succeeded = await _identityService.SetUserActiveStatusAsync(request.UserId, isActive: false);
        if (!succeeded)
        {
            throw new NotFoundException("User", request.UserId);
        }

        // A deactivated account keeps any already-issued access token valid until it expires
        // (JWTs are stateless), but revoking refresh tokens stops it from getting a new one.
        var activeTokens = await _dbContext.RefreshTokens
            .Where(rt => rt.UserId == request.UserId && rt.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.Revoke(_dateTime.UtcNow);
        }

        if (activeTokens.Count > 0)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
