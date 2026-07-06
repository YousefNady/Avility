namespace Avility.Application.JobPostings.Dtos;

public sealed record JobPostingDto(
    Guid Id,
    Guid CompanyId,
    string Title,
    string Description,
    string? Requirements,
    string EmploymentType,
    string ExperienceLevel,
    bool IsRemote,
    string? Country,
    string? Governorate,
    string? City,
    decimal? SalaryMin,
    decimal? SalaryMax,
    string? SalaryCurrency,
    DateTime? ApplicationDeadline,
    string Status,
    DateTime? PublishedAt,
    DateTime? ClosedAt);
