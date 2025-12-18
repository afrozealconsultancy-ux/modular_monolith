using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ModularMonolith.Shared.Abstractions.Exceptions;
using System.Text.Json;

namespace ModularMonolith.Shared.Infrastructure.Middleware;

/// <summary>
/// Global exception handling middleware.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An unhandled exception occurred");
            await HandleExceptionAsync(context, exception);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, error) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, new ErrorResponse("NotFound", exception.Message)),
            ValidationException validationEx => (StatusCodes.Status400BadRequest, new ErrorResponse("Validation", exception.Message, validationEx.Errors)),
            ConflictException => (StatusCodes.Status409Conflict, new ErrorResponse("Conflict", exception.Message)),
            UnauthorizedException => (StatusCodes.Status401Unauthorized, new ErrorResponse("Unauthorized", exception.Message)),
            ForbiddenException => (StatusCodes.Status403Forbidden, new ErrorResponse("Forbidden", exception.Message)),
            ModularMonolithException => (StatusCodes.Status400BadRequest, new ErrorResponse("BusinessError", exception.Message)),
            _ => (StatusCodes.Status500InternalServerError, new ErrorResponse("InternalServerError", "An internal server error occurred"))
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var json = JsonSerializer.Serialize(error, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }

    private record ErrorResponse(string Code, string Message, IReadOnlyList<string>? Errors = null);
}
