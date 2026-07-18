using Avility.Application.Common.Interfaces;
using Avility.Application.Common.Models;
using MediatR;

namespace Avility.Application.Admin.Queries.GetUsers;

public sealed class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PagedResult<UserSummaryDto>>
{
    private readonly IIdentityService _identityService;

    public GetUsersQueryHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public Task<PagedResult<UserSummaryDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken) =>
        _identityService.GetUsersAsync(request.PageNumber, request.PageSize, request.Role, cancellationToken);
}