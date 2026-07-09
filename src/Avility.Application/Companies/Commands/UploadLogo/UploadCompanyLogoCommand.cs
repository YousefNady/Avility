using Avility.Application.Companies.Dtos;
using MediatR;

namespace Avility.Application.Companies.Commands.UploadLogo;

public sealed record UploadCompanyLogoCommand(Stream Content, string FileName, string ContentType, long ContentLength)
    : IRequest<CompanyProfileDto>;