using Avility.Application.JobSeekers.Dtos;
using MediatR;

namespace Avility.Application.JobSeekers.Queries.GetMyResume;

public sealed record GetMyResumeQuery : IRequest<ResumeFileResult>;