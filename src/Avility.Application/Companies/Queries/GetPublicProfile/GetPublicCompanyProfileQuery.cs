using Avility.Application.Companies.Dtos;
using MediatR;

namespace Avility.Application.Companies.Queries.GetPublicProfile;

public sealed record GetPublicCompanyProfileQuery(Guid CompanyId) : IRequest<PublicCompanyProfileDto>;