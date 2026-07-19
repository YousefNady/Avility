namespace Avility.Application.Messages.Dtos;

public sealed record ConversationUnreadCountDto(Guid JobApplicationId, int UnreadCount);