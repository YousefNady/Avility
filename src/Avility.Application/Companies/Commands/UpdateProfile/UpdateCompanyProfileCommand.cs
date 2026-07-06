using Avility.Application.Companies.Dtos;
using MediatR;

namespace Avility.Application.Companies.Commands.UpdateProfile;

public sealed record UpdateCompanyProfileCommand(
    string CompanyName,
    string? Description,
    string? Industry,
    string? WebsiteUrl,
    string? LogoUrl,
    string CompanySize,
    int FoundedYear,
    string Country,
    string Governorate,
    string City) : IRequest<CompanyProfileDto>;
