using Avility.Application.Messages.Dtos;
using MediatR;

namespace Avility.Application.Messages.Queries.GetUnreadCounts;

public sealed record GetUnreadMessageCountsQuery : IRequest<IReadOnlyList<ConversationUnreadCountDto>>;