using Avility.Application.Common.Exceptions;
using Avility.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Avility.Application.Companies.Commands.RejectCompany;

public sealed class RejectCompanyCommandHandler : IRequestHandler<RejectCompanyCommand>
{
    private readonly IApplicationDbContext _dbContext;

    public RejectCompanyCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(RejectCompanyCommand request, CancellationToken cancellationToken)
    {
        var company = await _dbContext.Companies.FirstOrDefaultAsync(c => c.Id == request.CompanyId, cancellationToken)
            ?? throw new NotFoundException("Company", request.CompanyId);

        company.Reject();
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
