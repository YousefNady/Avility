using Avility.Application.JobPostings.Dtos;
using MediatR;

namespace Avility.Application.JobPostings.Commands.Publish;

public sealed record PublishJobPostingCommand(Guid JobPostingId) : IRequest<JobPostingDto>;
