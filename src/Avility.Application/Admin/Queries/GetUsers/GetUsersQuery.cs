using Avility.Application.Common.Models;
using MediatR;

namespace Avility.Application.Admin.Queries.GetUsers;

public sealed record GetUsersQuery(
    string? Role = null,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PagedResult<UserSummaryDto>>;