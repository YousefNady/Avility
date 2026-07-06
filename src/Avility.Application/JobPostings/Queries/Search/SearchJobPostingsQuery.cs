using Avility.Application.Common.Models;
using Avility.Application.JobPostings.Dtos;
using MediatR;

namespace Avility.Application.JobPostings.Queries.Search;

public sealed record SearchJobPostingsQuery(
    string? Search = null,
    string? EmploymentType = null,
    string? ExperienceLevel = null,
    bool? IsRemote = null,
    int PageNumber = 1,
    int PageSize = 10) : IRequest<PagedResult<JobPostingDto>>;
