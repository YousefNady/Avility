using Avility.API.Common.Responses;
using Avility.Application.Common.Exceptions;
using Avility.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;

namespace Avility.API.Middleware;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, response) = exception switch
        {
            ValidationException validationEx => (StatusCodes.Status400BadRequest,
                ApiResponse<object>.FailureResponse("Validation failed.", ToErrorDictionary(validationEx))),
            
            NotFoundException notFoundEx => (StatusCodes.Status404NotFound,
                ApiResponse<object>.FailureResponse(notFoundEx.Message)),
            
            ForbiddenAccessException forbiddenEx => (StatusCodes.Status403Forbidden,
                ApiResponse<object>.FailureResponse(forbiddenEx.Message)),
            
            DomainException domainEx => (StatusCodes.Status400BadRequest,
                ApiResponse<object>.FailureResponse(domainEx.Message)),

            _ => (StatusCodes.Status500InternalServerError,
                ApiResponse<object>.FailureResponse("An unexpected error occurred."))
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception");
        }

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }

    private static Dictionary<string, string[]> ToErrorDictionary(ValidationException ex) =>
        ex.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
}
