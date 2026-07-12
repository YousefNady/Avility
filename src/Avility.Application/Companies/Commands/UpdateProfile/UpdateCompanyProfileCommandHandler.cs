using Avility.Application.Common.Exceptions;
using Avility.Application.Common.Interfaces;
using Avility.Application.Companies.Dtos;
using Avility.Domain.Enums;
using Avility.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Avility.Application.Companies.Commands.UpdateProfile;

public sealed class UpdateCompanyProfileCommandHandler : IRequestHandler<UpdateCompanyProfileCommand, CompanyProfileDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public UpdateCompanyProfileCommandHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<CompanyProfileDto> Handle(UpdateCompanyProfileCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new InvalidOperationException("User is not authenticated.");

        var company = await _dbContext.Companies.FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Company profile", userId);

        var location = Location.Create(request.Country, request.Governorate, request.City);
        var companySize = Enum.Parse<CompanySize>(request.CompanySize);

        company.UpdateProfile(
            request.CompanyName,
            request.Description,
            request.Industry,
            request.WebsiteUrl,
            request.LogoUrl,
            companySize,
            request.FoundedYear,
            location);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return company.ToDto();
    }
}
