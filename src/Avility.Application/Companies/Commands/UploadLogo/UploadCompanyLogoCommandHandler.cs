using Avility.Application.Common.Exceptions;
using Avility.Application.Common.Interfaces;
using Avility.Application.Companies.Dtos;
using Avility.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Avility.Application.Companies.Commands.UploadLogo;

public sealed class UploadCompanyLogoCommandHandler : IRequestHandler<UploadCompanyLogoCommand, CompanyProfileDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IFileStorageService _fileStorage;
    private readonly ICurrentUserService _currentUser;

    public UploadCompanyLogoCommandHandler(
        IApplicationDbContext dbContext,
        IFileStorageService fileStorage,
        ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _fileStorage = fileStorage;
        _currentUser = currentUser;
    }

    public async Task<CompanyProfileDto> Handle(UploadCompanyLogoCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
                     ?? throw new UnauthorizedAccessException();

        var company = await _dbContext.Companies
                          .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken)
                      ?? throw new NotFoundException(nameof(Company), userId);

        var previousStorageKey = company.LogoStorageKey;

        var storageKey = await _fileStorage.SaveAsync(request.Content, request.FileName, request.ContentType, "logos", cancellationToken);
        
        company.SetLogo(storageKey);

        await _dbContext.SaveChangesAsync(cancellationToken);

        if (previousStorageKey is not null)
        {
            await _fileStorage.DeleteAsync(previousStorageKey, cancellationToken);
        }

        return company.ToDto();
    }
}