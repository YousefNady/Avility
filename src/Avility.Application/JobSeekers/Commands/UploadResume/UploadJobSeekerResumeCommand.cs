using Avility.Application.JobSeekers.Dtos;
using MediatR;

namespace Avility.Application.JobSeekers.Commands.UploadResume;

public sealed record UploadJobSeekerResumeCommand(Stream Content, string FileName, string ContentType, long ContentLength)
    : IRequest<JobSeekerProfileDto>;