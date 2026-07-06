using Avility.Application.Common.Exceptions;
using Avility.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Avility.Application.Companies.Commands.VerifyCompany;

public sealed class VerifyCompanyCommandHandler : IRequestHandler<VerifyCompanyCommand>
{
    private readonly IApplicationDbContext _dbContext;

    public VerifyCompanyCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(VerifyCompanyCommand request, CancellationToken cancellationToken)
    {
        var company = await _dbContext.Companies.FirstOrDefaultAsync(c => c.Id == request.CompanyId, cancellationToken)
            ?? throw new NotFoundException("Company", request.CompanyId);

        company.Verify();
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
