namespace Avility.Application.Resources.Dtos;

public sealed record ResourceDto(
    Guid Id,
    string Title,
    string Description,
    string Url,
    string Category,
    DateTime CreatedAt);