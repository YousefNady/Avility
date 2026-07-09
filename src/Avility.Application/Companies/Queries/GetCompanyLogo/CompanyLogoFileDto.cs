namespace Avility.Application.Companies.Queries.GetCompanyLogo;

public sealed record CompanyLogoFileDto(Stream Content, string ContentType, string FileName);