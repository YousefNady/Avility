using Avility.Application.Common.Exceptions;
using Avility.Application.Common.Interfaces;
using Avility.Application.JobPostings.Dtos;
using Avility.Domain.Enums;
using Avility.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Avility.Application.JobPostings.Commands.Update;

public sealed class UpdateJobPostingCommandHandler : IRequestHandler<UpdateJobPostingCommand, JobPostingDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public UpdateJobPostingCommandHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<JobPostingDto> Handle(UpdateJobPostingCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new InvalidOperationException("User is not authenticated.");

        var posting = await _dbContext.JobPostings.FirstOrDefaultAsync(p => p.Id == request.JobPostingId, cancellationToken)
            ?? throw new NotFoundException("JobPosting", request.JobPostingId);

        var owns = await _dbContext.Companies.AnyAsync(c => c.Id == posting.CompanyId && c.UserId == userId, cancellationToken);
        if (!owns)
        {
            throw new ForbiddenAccessException();
        }

        var location = request.IsRemote ? null : Location.Create(request.Country!, request.Governorate!, request.City!);
        var salary = request.SalaryMin is null
            ? null
            : SalaryRange.Create(request.SalaryMin.Value, request.SalaryMax!.Value, Enum.Parse<Currency>(request.SalaryCurrency!));

        posting.UpdateDetails(
            request.Title,
            request.Description,
            request.Requirements,
            Enum.Parse<EmploymentType>(request.EmploymentType),
            Enum.Parse<ExperienceLevel>(request.ExperienceLevel),
            request.IsRemote,
            location,
            salary,
            request.ApplicationDeadline);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return posting.ToDto();
    }
}
