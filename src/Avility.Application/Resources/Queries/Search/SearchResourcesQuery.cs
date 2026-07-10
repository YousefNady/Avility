using Avility.Application.Common.Models;
using Avility.Application.Resources.Dtos;
using MediatR;

namespace Avility.Application.Resources.Queries.Search;

public sealed record SearchResourcesQuery(
    string? Category = null,
    int PageNumber = 1,
    int PageSize = 10) : IRequest<PagedResult<ResourceDto>>;