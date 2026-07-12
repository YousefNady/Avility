using Avility.Application.JobApplications.Dtos;
using MediatR;

namespace Avility.Application.JobApplications.Commands.Withdraw;

public sealed record WithdrawJobApplicationCommand(Guid JobApplicationId) : IRequest<JobApplicationDto>;
