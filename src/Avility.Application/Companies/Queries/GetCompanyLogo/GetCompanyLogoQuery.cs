using MediatR;

namespace Avility.Application.Companies.Queries.GetCompanyLogo;

public sealed record GetCompanyLogoQuery(Guid CompanyId) : IRequest<CompanyLogoFileDto>;