using Avility.Domain.Entities;

namespace Avility.Application.Messages.Dtos;

public static class MessageMappingExtensions
{
    public static MessageDto ToDto(this Message entity) => new(
        entity.Id,
        entity.JobApplicationId,
        entity.SenderUserId,
        entity.Body,
        entity.CreatedAt,
        entity.IsRead,
        entity.ReadAt);
}