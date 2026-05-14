using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SchoolInfo.Application.Common.Exceptions;
using SchoolInfo.Domain.Exceptions;

namespace SchoolInfo.API.Middleware;

/// <summary>
/// TÃ¼m uygulamada oluÅŸan hatalarÄ± yakalayÄ±p standart bir formatta ve doÄŸru HTTP statÃ¼ kodlarÄ± ile dÃ¶nen middleware.
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
            _logger.LogError(ex, "Uygulama Ã§alÄ±ÅŸÄ±rken bir hata oluÅŸtu.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        context.Response.StatusCode = exception switch
        {
            ValidationException => (int)HttpStatusCode.UnprocessableEntity, // 422
            StudentNotFoundException => (int)HttpStatusCode.NotFound, // 404
            UnauthorizedClassroomAccessException => (int)HttpStatusCode.Forbidden, // 403
            DomainException => (int)HttpStatusCode.BadRequest, // 400
            UnauthorizedAccessException => (int)HttpStatusCode.Forbidden, // 403
            _ => (int)HttpStatusCode.InternalServerError // 500
        };

        var response = new
        {
            StatusCode = context.Response.StatusCode,
            Message = exception.Message,
            Errors = exception is ValidationException validationException ? validationException.Errors : null
        };

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        return context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}
