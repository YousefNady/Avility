using Avility.Application.Common.Exceptions;
using Avility.Application.Common.Interfaces;
using Avility.Application.Companies.Dtos;
using Avility.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Avility.Application.Companies.Commands.DeleteLogo;

public sealed class DeleteCompanyLogoCommandHandler : IRequestHandler<DeleteCompanyLogoCommand, CompanyProfileDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IFileStorageService _fileStorage;
    private readonly ICurrentUserService _currentUser;

    public DeleteCompanyLogoCommandHandler(
        IApplicationDbContext dbContext,
        IFileStorageService fileStorage,
        ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _fileStorage = fileStorage;
        _currentUser = currentUser;
    }

    public async Task<CompanyProfileDto> Handle(DeleteCompanyLogoCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
                     ?? throw new UnauthorizedAccessException();

        var company = await _dbContext.Companies
                          .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken)
                      ?? throw new NotFoundException(nameof(Company), userId);

        if (company.LogoStorageKey is { } storageKey)
        {
            await _fileStorage.DeleteAsync(storageKey, cancellationToken);
            company.RemoveLogo();
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return company.ToDto();
    }
}