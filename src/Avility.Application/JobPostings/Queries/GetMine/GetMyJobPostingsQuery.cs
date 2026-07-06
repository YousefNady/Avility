using Avility.Application.Common.Models;
using Avility.Application.JobPostings.Dtos;
using MediatR;

namespace Avility.Application.JobPostings.Queries.GetMine;

public sealed record GetMyJobPostingsQuery(int PageNumber = 1, int PageSize = 10) : IRequest<PagedResult<JobPostingDto>>;
