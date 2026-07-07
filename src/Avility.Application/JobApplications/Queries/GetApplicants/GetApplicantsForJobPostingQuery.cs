using Avility.Application.Common.Models;
using Avility.Application.JobApplications.Dtos;
using MediatR;

namespace Avility.Application.JobApplications.Queries.GetApplicants;

public sealed record GetApplicantsForJobPostingQuery(
    Guid JobPostingId,
    string? Status = null,
    int PageNumber = 1,
    int PageSize = 10) : IRequest<PagedResult<JobApplicationDto>>;
