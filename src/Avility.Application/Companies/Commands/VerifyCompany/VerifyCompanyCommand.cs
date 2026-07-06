using MediatR;

namespace Avility.Application.Companies.Commands.VerifyCompany;

public sealed record VerifyCompanyCommand(Guid CompanyId) : IRequest;
