using MediatR;

namespace Avility.Application.Admin.Queries.GetUserDetails;

public sealed record GetUserDetailsQuery(Guid UserId) : IRequest<UserDetailsDto>;