using Avility.Application.Common.Exceptions;
using Avility.Application.Common.Interfaces;
using Avility.Application.JobPostings.Dtos;
using Avility.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Avility.Application.JobPostings.Queries.GetById;

public sealed class GetJobPostingByIdQueryHandler : IRequestHandler<GetJobPostingByIdQuery, JobPostingDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public GetJobPostingByIdQueryHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<JobPostingDto> Handle(GetJobPostingByIdQuery request, CancellationToken cancellationToken)
    {
        var posting = await _dbContext.JobPostings.AsNoTracking().FirstOrDefaultAsync(p => p.Id == request.JobPostingId, cancellationToken)
                      ?? throw new NotFoundException("JobPosting", request.JobPostingId);

        var company = await _dbContext.Companies.AsNoTracking().FirstOrDefaultAsync(c => c.Id == posting.CompanyId, cancellationToken)
                      ?? throw new NotFoundException("Company", posting.CompanyId);

        if (posting.Status != JobPostingStatus.Published)
        {
            var userId = _currentUser.UserId;
            var isOwner = userId is not null && company.UserId == userId;

            if (!isOwner)
            {
                throw new NotFoundException("JobPosting", request.JobPostingId);
            }
        }

        return posting.ToDto(company);
    }
}