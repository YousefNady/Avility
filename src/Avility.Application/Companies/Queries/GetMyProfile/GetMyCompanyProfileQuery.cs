using Avility.Application.Companies.Dtos;
using MediatR;

namespace Avility.Application.Companies.Queries.GetMyProfile;

public sealed record GetMyCompanyProfileQuery : IRequest<CompanyProfileDto>;
