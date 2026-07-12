using Avility.Application.Common.Exceptions;
using Avility.Application.Common.Interfaces;
using Avility.Application.JobSeekers.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Avility.Application.JobSeekers.Queries.GetMyProfile;

public sealed class GetMyJobSeekerProfileQueryHandler : IRequestHandler<GetMyJobSeekerProfileQuery, JobSeekerProfileDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public GetMyJobSeekerProfileQueryHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<JobSeekerProfileDto> Handle(GetMyJobSeekerProfileQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new InvalidOperationException("User is not authenticated.");

        var jobSeeker = await _dbContext.JobSeekers
            .AsNoTracking()
            .FirstOrDefaultAsync(js => js.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("JobSeeker profile", userId);

        return jobSeeker.ToDto();
    }
}
