using Avility.Application.JobApplications.Dtos;
using MediatR;

namespace Avility.Application.JobApplications.Commands.Accept;

public sealed record AcceptJobApplicationCommand(Guid JobApplicationId) : IRequest<JobApplicationDto>;
