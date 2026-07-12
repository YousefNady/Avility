using Avility.Domain.Entities;

namespace Avility.Application.JobApplications.Dtos;

public static class JobApplicationMappingExtensions
{
    public static JobApplicationDto ToDto(this JobApplication entity) => new(
        entity.Id,
        entity.JobSeekerId,
        entity.JobPostingId,
        entity.Status.ToString(),
        entity.CoverLetter,
        entity.AppliedAt);
}
