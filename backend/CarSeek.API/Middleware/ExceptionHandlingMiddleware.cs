using System.Net;
using System.Text.Json;
using CarSeek.Application.Common.Exceptions;

namespace CarSeek.API.Middleware;

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
            _logger.LogError(ex, "An unhandled exception has occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var code = HttpStatusCode.InternalServerError; // 500 if unexpected

        var result = exception switch
        {
            ValidationException validationException => (HttpStatusCode.BadRequest, validationException.Errors),
            NotFoundException => (HttpStatusCode.NotFound, null),
            AuthenticationException => (HttpStatusCode.Unauthorized, null),
            InvalidOperationException => (HttpStatusCode.BadRequest, null),
            ForbiddenAccessException => (HttpStatusCode.Forbidden, null),
            _ => (HttpStatusCode.InternalServerError, null)
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)result.Item1;

        if (result.Item2 != null)
        {
            return context.Response.WriteAsync(JsonSerializer.Serialize(new { errors = result.Item2 }));
        }

        return context.Response.WriteAsync(JsonSerializer.Serialize(new { error = exception.Message }));
    }

    private static (HttpStatusCode code, string title, string message) MapException(Exception exception) =>
        exception switch
        {
            ValidationException validationException =>
                (HttpStatusCode.BadRequest, "Validation Error", JsonSerializer.Serialize(validationException.Errors)),
            NotFoundException notFoundException =>
                (HttpStatusCode.NotFound, notFoundException.Title, notFoundException.Message),
            AuthenticationException authenticationException =>
                (HttpStatusCode.Unauthorized, "Authentication Failed", authenticationException.Message),
            InvalidOperationException invalidOperationException =>
                (HttpStatusCode.BadRequest, "Invalid Operation", invalidOperationException.Message),
            ForbiddenAccessException forbiddenAccessException =>
                (HttpStatusCode.Forbidden, forbiddenAccessException.Title, forbiddenAccessException.Message),
            _ => (HttpStatusCode.InternalServerError, "Internal Server Error", "An unexpected error occurred.")
        };
}
