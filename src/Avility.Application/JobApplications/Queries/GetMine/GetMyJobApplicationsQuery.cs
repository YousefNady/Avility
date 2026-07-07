using Avility.Application.Common.Models;
using Avility.Application.JobApplications.Dtos;
using MediatR;

namespace Avility.Application.JobApplications.Queries.GetMine;

public sealed record GetMyJobApplicationsQuery(int PageNumber = 1, int PageSize = 10) : IRequest<PagedResult<JobApplicationDto>>;
