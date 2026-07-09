using Avility.Application.Companies.Dtos;
using MediatR;

namespace Avility.Application.Companies.Commands.DeleteLogo;

public sealed record DeleteCompanyLogoCommand : IRequest<CompanyProfileDto>;