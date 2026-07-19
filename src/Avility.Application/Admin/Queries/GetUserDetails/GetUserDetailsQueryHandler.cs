using Avility.Application.Common.Exceptions;
using Avility.Application.Common.Interfaces;
using Avility.Application.Companies.Dtos;
using Avility.Application.JobSeekers.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Avility.Application.Admin.Queries.GetUserDetails;

public sealed class GetUserDetailsQueryHandler : IRequestHandler<GetUserDetailsQuery, UserDetailsDto>
{
    private readonly IIdentityService _identityService;
    private readonly IApplicationDbContext _dbContext;

    public GetUserDetailsQueryHandler(IIdentityService identityService, IApplicationDbContext dbContext)
    {
        _identityService = identityService;
        _dbContext = dbContext;
    }

    public async Task<UserDetailsDto> Handle(GetUserDetailsQuery request, CancellationToken cancellationToken)
    {
        var userInfo = await _identityService.GetUserDetailsAsync(request.UserId)
                       ?? throw new NotFoundException("User", request.UserId);

        var jobSeeker = await _dbContext.JobSeekers.AsNoTracking()
            .FirstOrDefaultAsync(js => js.UserId == request.UserId, cancellationToken);
        var company = await _dbContext.Companies.AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == request.UserId, cancellationToken);

        return new UserDetailsDto(
            request.UserId,
            userInfo.Email,
            userInfo.Roles,
            userInfo.IsActive,
            userInfo.CreatedAt,
            userInfo.LastLoginAt,
            jobSeeker?.ToDto(),
            company?.ToDto());
    }
}