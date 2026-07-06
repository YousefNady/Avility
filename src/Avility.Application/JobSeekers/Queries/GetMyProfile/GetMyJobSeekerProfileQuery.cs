using Avility.Application.JobSeekers.Dtos;
using MediatR;

namespace Avility.Application.JobSeekers.Queries.GetMyProfile;

public sealed record GetMyJobSeekerProfileQuery : IRequest<JobSeekerProfileDto>;
