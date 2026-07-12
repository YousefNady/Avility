using Avility.Application.JobSeekers.Dtos;
using MediatR;

namespace Avility.Application.JobSeekers.Commands.UpdateProfile;

public sealed record UpdateJobSeekerProfileCommand(
    string FullName,
    string? Headline,
    string? Bio,
    string PhoneNumber,
    int YearsOfExperience,
    string CurrentJobTitle,
    string Country,
    string Governorate,
    string City,
    string? LinkedInUrl,
    string? GitHubUrl,
    string? PortfolioUrl,
    IReadOnlyList<string>? DisabilityCategories = null,
    string? AccommodationNotes = null) : IRequest<JobSeekerProfileDto>;

