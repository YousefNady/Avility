using Avility.Application.Common.Interfaces;
using Avility.Application.JobSeekers.Dtos;
using Avility.Domain.Entities;
using Avility.Domain.ValueObjects;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Avility.Application.JobSeekers.Commands.CreateProfile;

public sealed class CreateJobSeekerProfileCommandHandler : IRequestHandler<CreateJobSeekerProfileCommand, JobSeekerProfileDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public CreateJobSeekerProfileCommandHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<JobSeekerProfileDto> Handle(CreateJobSeekerProfileCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new InvalidOperationException("User is not authenticated.");

        var alreadyExists = await _dbContext.JobSeekers.AnyAsync(js => js.UserId == userId, cancellationToken);
        if (alreadyExists)
        {
            throw new ValidationException(new[] { new ValidationFailure("Profile", "A JobSeeker profile already exists for this account.") });
        }

        var location = Location.Create(request.Country, request.Governorate, request.City);

        var jobSeeker = JobSeeker.Create(
            userId,
            request.FullName,
            request.PhoneNumber,
            request.YearsOfExperience,
            request.CurrentJobTitle,
            location);

        _dbContext.JobSeekers.Add(jobSeeker);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return jobSeeker.ToDto();
    }
}
