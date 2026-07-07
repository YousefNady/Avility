using Avility.Application.JobApplications.Dtos;
using MediatR;

namespace Avility.Application.JobApplications.Commands.Apply;

public sealed record ApplyToJobCommand(Guid JobPostingId, string? CoverLetter) : IRequest<JobApplicationDto>;
