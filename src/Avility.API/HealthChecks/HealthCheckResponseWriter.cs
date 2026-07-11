using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Avility.API.HealthChecks;

/// <summary>
/// Plain-text "Healthy"/"Unhealthy" doesn't say which dependency failed.
/// Renders the full HealthReport as JSON instead, so a status page or an
/// orchestrator can see each check's name, status, description, and
/// duration individually.
/// </summary>
public static class HealthCheckResponseWriter
{
    public static Task WriteJsonAsync(HttpContext context, HealthReport report)
    {
       // context.Response.ContentType = "application/json";

        var payload = new
        {
            status = report.Status.ToString(),
            totalDurationMs = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                durationMs = e.Value.Duration.TotalMilliseconds
            })
        };

        // return context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        return context.Response.WriteAsJsonAsync(payload);
    }
}