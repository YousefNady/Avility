using Avility.Application.Common.Exceptions;
using Avility.Application.Common.Interfaces;
using Avility.Application.JobSeekers.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Avility.Application.JobSeekers.Commands.UploadResume;

public sealed class UploadJobSeekerResumeCommandHandler : IRequestHandler<UploadJobSeekerResumeCommand, JobSeekerProfileDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorageService _fileStorage;

    public UploadJobSeekerResumeCommandHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser, IFileStorageService fileStorage)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _fileStorage = fileStorage;
    }

    public async Task<JobSeekerProfileDto> Handle(UploadJobSeekerResumeCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new InvalidOperationException("User is not authenticated.");

        var jobSeeker = await _dbContext.JobSeekers.FirstOrDefaultAsync(js => js.UserId == userId, cancellationToken)
                        ?? throw new NotFoundException("JobSeeker profile", userId);

        var storageKey = await _fileStorage.SaveAsync(request.Content, request.FileName, request.ContentType, "resumes", cancellationToken);;

        jobSeeker.SetResumeUrl(storageKey);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return jobSeeker.ToDto();
    }
}