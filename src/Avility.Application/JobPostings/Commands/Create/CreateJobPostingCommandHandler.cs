using Avility.Application.Common.Interfaces;
using Avility.Application.JobPostings.Dtos;
using Avility.Domain.Entities;
using Avility.Domain.Enums;
using Avility.Domain.ValueObjects;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Avility.Application.JobPostings.Commands.Create;

public sealed class CreateJobPostingCommandHandler : IRequestHandler<CreateJobPostingCommand, JobPostingDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public CreateJobPostingCommandHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<JobPostingDto> Handle(CreateJobPostingCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new InvalidOperationException("User is not authenticated.");

        var company = await _dbContext.Companies.FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken)
            ?? throw new ValidationException(new[] { new ValidationFailure("Company", "You must complete your company profile before posting a job.") });

        var location = request.IsRemote ? null : Location.Create(request.Country!, request.Governorate!, request.City!);
        var salary = request.SalaryMin is null
            ? null
            : SalaryRange.Create(request.SalaryMin.Value, request.SalaryMax!.Value, Enum.Parse<Currency>(request.SalaryCurrency!));

        var posting = JobPosting.Create(
            company.Id,
            request.Title,
            request.Description,
            Enum.Parse<EmploymentType>(request.EmploymentType),
            Enum.Parse<ExperienceLevel>(request.ExperienceLevel),
            request.IsRemote,
            location,
            salary,
            request.ApplicationDeadline);
        
        if (request.SupportedDisabilityCategories is not null || request.AccommodationDetails is not null)
                {
                    posting.UpdateAccommodations(
                        request.SupportedDisabilityCategories?.Select(Enum.Parse<DisabilityCategory>).ToList(),
                        request.AccommodationDetails);
                }

        _dbContext.JobPostings.Add(posting);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return posting.ToDto();
    }
}
