namespace Avility.API.Middleware;

/// <summary>
/// Adds standard security response headers to every response, including
/// error responses (registered ahead of the exception handler).
/// Deliberately does NOT include Content-Security-Policy: this API
/// serves JSON almost exclusively, and the one HTML surface (Swagger UI,
/// Development only) would need its own carefully-tested policy rather
/// than a generic one applied here - left out rather than risking a
/// misconfigured CSP that silently breaks Swagger UI.
/// </summary>
public sealed class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        context.Response.Headers.Append("Permissions-Policy", "geolocation=(), camera=(), microphone=()");

        await _next(context);
    }
}