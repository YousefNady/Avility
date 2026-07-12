using Avility.Domain.Entities;

namespace Avility.Application.JobSeekers.Dtos;

public static class JobSeekerMappingExtensions
{
    public static JobSeekerProfileDto ToDto(this JobSeeker entity) => new(
        entity.Id,
        entity.FullName,
        entity.Headline,
        entity.Bio,
        entity.ResumeUrl,
        entity.PhoneNumber,
        entity.YearsOfExperience,
        entity.CurrentJobTitle,
        entity.LinkedInUrl,
        entity.GitHubUrl,
        entity.PortfolioUrl,
        entity.Location.Country,
        entity.Location.Governorate,
        entity.Location.City,
        entity.DisabilityCategories.Select(c => c.ToString()).ToList(),
        entity.AccommodationNotes);
}
