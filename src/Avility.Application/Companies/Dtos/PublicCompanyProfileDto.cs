namespace Avility.Application.Companies.Dtos;

public sealed record PublicCompanyProfileDto(
    Guid Id,
    string CompanyName,
    string? Description,
    string? Industry,
    string? WebsiteUrl,
    string? LogoUrl,
    string CompanySize,
    int FoundedYear,
    string Country,
    string Governorate,
    string City,
    bool IsVerified);