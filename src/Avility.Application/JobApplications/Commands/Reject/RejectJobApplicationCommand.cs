using Avility.Application.JobApplications.Dtos;
using MediatR;

namespace Avility.Application.JobApplications.Commands.Reject;

public sealed record RejectJobApplicationCommand(Guid JobApplicationId) : IRequest<JobApplicationDto>;
