namespace Avility.Application.JobSeekers.Dtos;

public sealed record JobSeekerProfileDto(
    Guid Id,
    string FullName,
    string? Headline,
    string? Bio,
    string? ResumeUrl,
    string PhoneNumber,
    int YearsOfExperience,
    string CurrentJobTitle,
    string? LinkedInUrl,
    string? GitHubUrl,
    string? PortfolioUrl,
    string Country,
    string Governorate,
    string City,
    IReadOnlyList<string> DisabilityCategories,
    string? AccommodationNotes);
