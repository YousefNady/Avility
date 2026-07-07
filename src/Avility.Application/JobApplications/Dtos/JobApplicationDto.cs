namespace Avility.Application.JobApplications.Dtos;

public sealed record JobApplicationDto(
    Guid Id,
    Guid JobSeekerId,
    Guid JobPostingId,
    string Status,
    string? CoverLetter,
    DateTime AppliedAt);
