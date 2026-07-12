using Avility.Application.JobApplications.Dtos;
using MediatR;

namespace Avility.Application.JobApplications.Commands.MoveToUnderReview;

public sealed record MoveApplicationToUnderReviewCommand(Guid JobApplicationId) : IRequest<JobApplicationDto>;
