using Avility.Application.Common.Exceptions;
using Avility.Application.Common.Interfaces;
using Avility.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Avility.Application.Companies.Queries.GetCompanyLogo;

public sealed class GetCompanyLogoQueryHandler : IRequestHandler<GetCompanyLogoQuery, CompanyLogoFileDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IFileStorageService _fileStorage;

    public GetCompanyLogoQueryHandler(IApplicationDbContext dbContext, IFileStorageService fileStorage)
    {
        _dbContext = dbContext;
        _fileStorage = fileStorage;
    }

    public async Task<CompanyLogoFileDto> Handle(GetCompanyLogoQuery request, CancellationToken cancellationToken)
    {
        var company = await _dbContext.Companies
                          .FirstOrDefaultAsync(c => c.Id == request.CompanyId, cancellationToken)
                      ?? throw new NotFoundException(nameof(Company), request.CompanyId);

        if (company.LogoStorageKey is null)
        {
            throw new NotFoundException(nameof(Company), request.CompanyId);
        }

        var file = await _fileStorage.GetAsync(company.LogoStorageKey, cancellationToken)
                   ?? throw new NotFoundException(nameof(Company), request.CompanyId);

        return new CompanyLogoFileDto(file.Content, file.ContentType, file.FileName);
    }
}