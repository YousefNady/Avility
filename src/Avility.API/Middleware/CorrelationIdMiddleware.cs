using Serilog.Context;

namespace Avility.API.Middleware;

/// <summary>
/// Assigns (or reuses an incoming) correlation ID per request, echoes it
/// back as a response header so a frontend can display/report it, and
/// pushes it into Serilog's LogContext so every log line for this
/// request - UseSerilogRequestLogging's summary line, MediatR's
/// LoggingBehaviour, GlobalExceptionHandler - carries the same ID without
/// any of those needing to know about correlation IDs themselves.
/// </summary>
public sealed class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-Id";

    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(HeaderName, out var incoming) && !string.IsNullOrWhiteSpace(incoming)
            ? incoming.ToString()
            : Guid.NewGuid().ToString();

        context.Items[HeaderName] = correlationId;
        context.Response.Headers[HeaderName] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}