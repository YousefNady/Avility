using MediatR;

namespace Avility.Application.Companies.Commands.RejectCompany;

public sealed record RejectCompanyCommand(Guid CompanyId) : IRequest;
