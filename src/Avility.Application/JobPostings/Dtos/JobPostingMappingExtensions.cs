using Avility.Domain.Entities;
using Avility.Domain.Enums;

namespace Avility.Application.JobPostings.Dtos;

public static class JobPostingMappingExtensions
{
    public static JobPostingDto ToDto(this JobPosting entity, Company company) => new(
        entity.Id,
        entity.CompanyId,
        entity.Title,
        entity.Description,
        entity.Requirements,
        entity.EmploymentType.ToString(),
        entity.ExperienceLevel.ToString(),
        entity.IsRemote,
        entity.Location?.Country,
        entity.Location?.Governorate,
        entity.Location?.City,
        entity.Salary?.Min,
        entity.Salary?.Max,
        entity.Salary?.Currency.ToString(),
        entity.ApplicationDeadline,
        entity.Status.ToString(),
        entity.PublishedAt,
        entity.ClosedAt,
        entity.SupportedDisabilityCategories.Select(c => c.ToString()).ToList(),
        entity.AccommodationDetails,
        company.CompanyName,
        company.LogoUrl,
        company.VerificationStatus == CompanyVerificationStatus.Verified);
}
