using System.Text.Json;
using BCSMS.Application.Common.Exceptions;
using BCSMS.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace BCSMS.API.Middleware;

/// <summary>
/// Centralized exception handling middleware that maps application/domain exceptions
/// to standard RFC 7807 ProblemDetails HTTP responses without exposing sensitive internal details.
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
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, detail) = exception switch
        {
            ValidationException ex => (StatusCodes.Status400BadRequest, "Validation Error", ex.Message),
            UnauthorizedException ex => (StatusCodes.Status401Unauthorized, "Unauthorized", ex.Message),
            ForbiddenException ex => (StatusCodes.Status403Forbidden, "Forbidden", ex.Message),
            NotFoundException ex => (StatusCodes.Status404NotFound, "Not Found", ex.Message),
            ApplicationConflictException ex => (StatusCodes.Status409Conflict, "Conflict", ex.Message),
            DomainException ex => (StatusCodes.Status422UnprocessableEntity, "Unprocessable Entity", ex.Message),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error", "An unexpected error occurred. Please try again later.")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception occurred while processing HTTP request.");
        }
        else
        {
            _logger.LogWarning("Handled exception: {Title} ({StatusCode}) - {Detail}", title, statusCode, detail);
        }

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
