using Avility.Application.Common.Interfaces;
using Avility.Application.Companies.Dtos;
using Avility.Domain.Entities;
using Avility.Domain.Enums;
using Avility.Domain.ValueObjects;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Avility.Application.Companies.Commands.CreateProfile;

public sealed class CreateCompanyProfileCommandHandler : IRequestHandler<CreateCompanyProfileCommand, CompanyProfileDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public CreateCompanyProfileCommandHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<CompanyProfileDto> Handle(CreateCompanyProfileCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new InvalidOperationException("User is not authenticated.");

        var alreadyExists = await _dbContext.Companies.AnyAsync(c => c.UserId == userId, cancellationToken);
        if (alreadyExists)
        {
            throw new ValidationException(new[] { new ValidationFailure("Profile", "A Company profile already exists for this account.") });
        }

        var location = Location.Create(request.Country, request.Governorate, request.City);
        var companySize = Enum.Parse<CompanySize>(request.CompanySize);

        var company = Company.Create(userId, request.CompanyName, companySize, request.FoundedYear, location);

        _dbContext.Companies.Add(company);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return company.ToDto();
    }
}
