using Avility.Domain.Entities;

namespace Avility.Application.Resources.Dtos;

public static class ResourceMappingExtensions
{
    public static ResourceDto ToDto(this Resource entity) => new(
        entity.Id,
        entity.Title,
        entity.Description,
        entity.Url,
        entity.Category.ToString(),
        entity.CreatedAt);
}