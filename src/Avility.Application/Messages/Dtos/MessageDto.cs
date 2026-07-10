namespace Avility.Application.Messages.Dtos;

public sealed record MessageDto(
    Guid Id,
    Guid JobApplicationId,
    Guid SenderUserId,
    string Body,
    DateTime CreatedAt);