using Avility.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Avility.Infrastructure.Storage;

/// <summary>
/// Local-disk implementation. Swappable for a cloud provider (e.g. Azure
/// Blob, S3) later without touching Application - handlers depend only on
/// IFileStorageService.
/// </summary>
public sealed class LocalFileStorageService : IFileStorageService
{
    private readonly string _rootPath;

    public LocalFileStorageService(IConfiguration configuration)
    {
        _rootPath = configuration["FileStorage:LocalRootPath"] ?? Path.Combine(AppContext.BaseDirectory, "App_Data", "uploads");
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<string> SaveAsync(Stream content, string fileName, string contentType, string category, CancellationToken cancellationToken)
    {
        var categoryPath = Path.Combine(_rootPath, category);
        Directory.CreateDirectory(categoryPath);

        var storageKey = $"{category}/{Guid.NewGuid()}{Path.GetExtension(fileName)}";
        var fullPath = Path.Combine(_rootPath, storageKey);

        await using var fileStream = File.Create(fullPath);
        await content.CopyToAsync(fileStream, cancellationToken);

        return storageKey;
    }

    public Task<(Stream Content, string ContentType, string FileName)?> GetAsync(string storageKey, CancellationToken cancellationToken)
    {
        var fullPath = Path.Combine(_rootPath, storageKey);
        if (!File.Exists(fullPath))
        {
            return Task.FromResult<(Stream, string, string)?>(null);
        }

        Stream stream = File.OpenRead(fullPath);
        return Task.FromResult<(Stream, string, string)?>((stream, GetContentType(storageKey), storageKey));
    }
    
    public Task DeleteAsync(string storageKey, CancellationToken cancellationToken)
    {
        var fullPath = Path.Combine(_rootPath, storageKey);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    private static string GetContentType(string fileName) => Path.GetExtension(fileName).ToLowerInvariant() switch
    {
        ".pdf" => "application/pdf",
        ".doc" => "application/msword",
        ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".webp" => "image/webp",
        _ => "application/octet-stream"
    };
}