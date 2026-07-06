using Avility.Application.Companies.Dtos;
using MediatR;

namespace Avility.Application.Companies.Commands.CreateProfile;

public sealed record CreateCompanyProfileCommand(
    string CompanyName,
    string CompanySize,
    int FoundedYear,
    string Country,
    string Governorate,
    string City) : IRequest<CompanyProfileDto>;
