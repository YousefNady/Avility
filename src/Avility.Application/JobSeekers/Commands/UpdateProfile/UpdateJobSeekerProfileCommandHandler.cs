using Avility.Application.Common.Exceptions;
using Avility.Application.Common.Interfaces;
using Avility.Application.JobSeekers.Dtos;
using Avility.Domain.ValueObjects;
using Avility.Domain.Enums; 
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Avility.Application.JobSeekers.Commands.UpdateProfile;

public sealed class UpdateJobSeekerProfileCommandHandler : IRequestHandler<UpdateJobSeekerProfileCommand, JobSeekerProfileDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public UpdateJobSeekerProfileCommandHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<JobSeekerProfileDto> Handle(UpdateJobSeekerProfileCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new InvalidOperationException("User is not authenticated.");

        var jobSeeker = await _dbContext.JobSeekers.FirstOrDefaultAsync(js => js.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("JobSeeker profile", userId);

        var location = Location.Create(request.Country, request.Governorate, request.City);

        jobSeeker.UpdateProfile(
            request.FullName,
            request.Headline,
            request.Bio,
            request.PhoneNumber,
            request.YearsOfExperience,
            request.CurrentJobTitle,
            location,
            request.LinkedInUrl,
            request.GitHubUrl,
            request.PortfolioUrl);
        
        if (request.DisabilityCategories is not null || request.AccommodationNotes is not null)
                {
                    jobSeeker.UpdateAccessibilityInfo(
                        request.DisabilityCategories?.Select(Enum.Parse<DisabilityCategory>).ToList(),
                        request.AccommodationNotes);
                }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return jobSeeker.ToDto();
    }
}
