using Avility.Application.Common.Models;
using Avility.Application.Companies.Dtos;
using MediatR;

namespace Avility.Application.Companies.Queries.GetByVerificationStatus;

public sealed record GetCompaniesByVerificationStatusQuery(
    string? Status = null,
    string? Search = null,
    int PageNumber = 1,
    int PageSize = 10) : IRequest<PagedResult<CompanyProfileDto>>;
