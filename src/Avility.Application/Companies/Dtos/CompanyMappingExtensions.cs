using Avility.Domain.Entities;

namespace Avility.Application.Companies.Dtos;

public static class CompanyMappingExtensions
{
    public static CompanyProfileDto ToDto(this Company entity) => new(
        entity.Id,
        entity.CompanyName,
        entity.Description,
        entity.Industry,
        entity.WebsiteUrl,
        entity.LogoStorageKey,
        entity.LogoStorageKey is not null,
        entity.CompanySize.ToString(),
        entity.FoundedYear,
        entity.VerificationStatus.ToString(),
        entity.Location.Country,
        entity.Location.Governorate,
        entity.Location.City);
}
