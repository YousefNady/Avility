using Avility.Application.Common.Exceptions;
using Avility.Application.Common.Interfaces;
using Avility.Application.Companies.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Avility.Application.Companies.Queries.GetPublicProfile;

public sealed class GetPublicCompanyProfileQueryHandler : IRequestHandler<GetPublicCompanyProfileQuery, PublicCompanyProfileDto>
{
    private readonly IApplicationDbContext _dbContext;

    public GetPublicCompanyProfileQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PublicCompanyProfileDto> Handle(GetPublicCompanyProfileQuery request, CancellationToken cancellationToken)
    {
        var company = await _dbContext.Companies.AsNoTracking().FirstOrDefaultAsync(c => c.Id == request.CompanyId, cancellationToken)
                      ?? throw new NotFoundException("Company", request.CompanyId);

        return company.ToPublicDto();
    }
}