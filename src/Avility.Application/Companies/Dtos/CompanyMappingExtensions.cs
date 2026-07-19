using Avility.Domain.Entities;
using Avility.Domain.Enums;

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
        entity.Location.City,
        entity.CreatedAt);
    
    public static PublicCompanyProfileDto ToPublicDto(this Company entity) => new(
        entity.Id,
        entity.CompanyName,
        entity.Description,
        entity.Industry,
        entity.WebsiteUrl,
        entity.LogoUrl,
        entity.CompanySize.ToString(),
        entity.FoundedYear,
        entity.Location.Country,
        entity.Location.Governorate,
        entity.Location.City,
        entity.VerificationStatus == CompanyVerificationStatus.Verified);
}
