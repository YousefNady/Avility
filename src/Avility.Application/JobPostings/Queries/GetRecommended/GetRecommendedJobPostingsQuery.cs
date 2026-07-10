using Avility.Application.Common.Models;
using Avility.Application.JobPostings.Dtos;
using MediatR;

namespace Avility.Application.JobPostings.Queries.GetRecommended;

public sealed record GetRecommendedJobPostingsQuery(
    int PageNumber = 1,
    int PageSize = 10) : IRequest<PagedResult<JobPostingDto>>;