using Avility.Application.JobPostings.Dtos;
using MediatR;

namespace Avility.Application.JobPostings.Queries.GetById;

public sealed record GetJobPostingByIdQuery(Guid JobPostingId) : IRequest<JobPostingDto>;
