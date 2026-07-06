using Avility.Application.Common.Exceptions;
using Avility.Application.Common.Interfaces;
using Avility.Application.Companies.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Avility.Application.Companies.Queries.GetMyProfile;

public sealed class GetMyCompanyProfileQueryHandler : IRequestHandler<GetMyCompanyProfileQuery, CompanyProfileDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public GetMyCompanyProfileQueryHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<CompanyProfileDto> Handle(GetMyCompanyProfileQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new InvalidOperationException("User is not authenticated.");

        var company = await _dbContext.Companies
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Company profile", userId);

        return company.ToDto();
    }
}
