using Avility.Application.JobPostings.Dtos;
using MediatR;

namespace Avility.Application.JobPostings.Commands.Close;

public sealed record CloseJobPostingCommand(Guid JobPostingId) : IRequest<JobPostingDto>;
