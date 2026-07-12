using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Avility.API.HealthChecks;

/// <summary>
/// Confirms the local resume-storage directory exists and is writable -
/// resume upload/download would otherwise fail silently until someone
/// actually tries it. Reads the exact same configuration key and default
/// LocalFileStorageService itself uses, so this check can never drift
/// from the real storage path.
/// </summary>
public sealed class FileStorageHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;

    public FileStorageHealthCheck(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var rootPath = _configuration["FileStorage:LocalRootPath"] ?? Path.Combine(AppContext.BaseDirectory, "App_Data", "resumes");

        try
        {
            Directory.CreateDirectory(rootPath);

            var probeFile = Path.Combine(rootPath, $".healthcheck-{Guid.NewGuid()}");
            File.WriteAllText(probeFile, string.Empty);
            File.Delete(probeFile);

            return Task.FromResult(HealthCheckResult.Healthy($"'{rootPath}' is writable."));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy($"'{rootPath}' is not accessible.", ex));
        }
    }
}