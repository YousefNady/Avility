namespace Avility.Application.Companies.Dtos;

public sealed record CompanyProfileDto(
    Guid Id,
    string CompanyName,
    string? Description,
    string? Industry,
    string? WebsiteUrl,
    string? LogoUrl,
    string CompanySize,
    int FoundedYear,
    string VerificationStatus,
    string Country,
    string Governorate,
    string City);