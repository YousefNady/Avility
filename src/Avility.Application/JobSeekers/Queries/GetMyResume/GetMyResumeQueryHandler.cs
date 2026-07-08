using Avility.Application.Common.Exceptions;
using Avility.Application.Common.Interfaces;
using Avility.Application.JobSeekers.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Avility.Application.JobSeekers.Queries.GetMyResume;

public sealed class GetMyResumeQueryHandler : IRequestHandler<GetMyResumeQuery, ResumeFileResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorageService _fileStorage;

    public GetMyResumeQueryHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser, IFileStorageService fileStorage)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _fileStorage = fileStorage;
    }

    public async Task<ResumeFileResult> Handle(GetMyResumeQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new InvalidOperationException("User is not authenticated.");

        var jobSeeker = await _dbContext.JobSeekers.AsNoTracking().FirstOrDefaultAsync(js => js.UserId == userId, cancellationToken)
                        ?? throw new NotFoundException("JobSeeker profile", userId);

        if (string.IsNullOrEmpty(jobSeeker.ResumeUrl))
        {
            throw new NotFoundException("Resume", userId);
        }

        var file = await _fileStorage.GetAsync(jobSeeker.ResumeUrl, cancellationToken);

        if (file is not { } resume)
        {
            throw new NotFoundException("Resume", userId);
        }

        return new ResumeFileResult(
            resume.Content,
            resume.ContentType,
            resume.FileName);
    }
}