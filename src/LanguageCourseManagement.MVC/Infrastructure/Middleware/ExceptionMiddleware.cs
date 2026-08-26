using LanguageCourseManagement.Application.Exceptions;
using LanguageCourseManagement.MVC.Exceptions.Handlers;
using LanguageCourseManagement.MVC.Exceptions.HttpProblemDetails;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LanguageCourseManagement.MVC.Infrastructure.Middleware;

/// <summary>
/// Yakalanmayan exception'ları HTTP ProblemDetails yanıtına dönüştüren middleware.
/// </summary>
public class ExceptionMiddleware
{
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            LogException(context, exception);

            if (context.Response.HasStarted)
            {
                _logger.LogWarning(
                    exception,
                    "The response has already started; the exception response cannot be rewritten. Rethrowing the original exception.");
                throw;
            }

            if (IsApiRequest(context))
            {
                context.Response.Clear();

                if (IsEnrollmentSettlementConflict(exception))
                {
                    await WriteProblemDetailsAsync(context.Response, new ConflictProblemDetails(
                        "The requested enrollment or settlement conflicts with an existing record."));
                    return;
                }

                if (IsFacilityNameConflict(exception))
                {
                    await WriteProblemDetailsAsync(context.Response, new ConflictProblemDetails(
                        "The facility name conflicts with an existing record."));
                    return;
                }

                await HandleExceptionAsync(context.Response, exception);
                return;
            }

            await RenderMvcErrorAsync(context);
        }
    }

    private async Task RenderMvcErrorAsync(HttpContext context)
    {
        context.Response.Clear();
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.SetEndpoint(null);
        context.Request.RouteValues.Clear();
        context.Request.Path = "/Home/Error";

        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            if (context.Response.HasStarted)
            {
                _logger.LogWarning(
                    exception,
                    "The MVC error response has already started; the exception cannot be rewritten. Rethrowing the original exception.");
                throw;
            }

            if (!context.Response.HasStarted)
            {
                context.Response.Clear();
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "text/plain; charset=utf-8";
                await context.Response.WriteAsync("An unexpected error occurred.");
            }
        }
    }

    private async Task HandleExceptionAsync(HttpResponse response, Exception exception)
    {
        response.ContentType = "application/problem+json";
        var httpExceptionHandler = new HttpExceptionHandler
        {
            Response = response
        };
        await httpExceptionHandler.HandleExceptionAsync(exception);
    }

    private static async Task WriteProblemDetailsAsync(HttpResponse response, ProblemDetails details)
    {
        response.StatusCode = details.Status ?? StatusCodes.Status500InternalServerError;
        response.ContentType = "application/problem+json";
        await response.WriteAsJsonAsync(details);
    }

    private static bool IsApiRequest(HttpContext context)
    {
        return context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsEnrollmentSettlementConflict(Exception exception)
    {
        if (exception is not DbUpdateException dbUpdateException)
            return false;

        for (var current = dbUpdateException.InnerException; current is not null; current = current.InnerException)
        {
            if (current is SqlException sqlException && sqlException.Number is 2601 or 2627)
            {
                // Match only the constraints owned by enrollment/settlement. The
                // database message is never sent to the client.
                return sqlException.Message.Contains("UX_Enrollments_Student_Course", StringComparison.OrdinalIgnoreCase)
                    || sqlException.Message.Contains("UX_Payments_Enrollment", StringComparison.OrdinalIgnoreCase)
                    || sqlException.Message.Contains("UX_Payments_IdempotencyKey", StringComparison.OrdinalIgnoreCase);
            }
        }

        return false;
    }

    private static bool IsFacilityNameConflict(Exception exception)
    {
        if (exception is not DbUpdateException dbUpdateException)
            return false;

        for (var current = dbUpdateException.InnerException; current is not null; current = current.InnerException)
        {
            if (current is SqlException sqlException && sqlException.Number is 2601 or 2627)
            {
                // Match only the Facility unique index. The database message is
                // never sent to the client.
                return sqlException.Message.Contains("UX_Facilities_Name_Active", StringComparison.OrdinalIgnoreCase);
            }
        }

        return false;
    }

    private void LogException(HttpContext context, Exception exception)
    {
        string user = context.User.Identity?.Name ?? "?";
        string path = context.Request.Path.Value ?? "Unknown";

        // Beklenen iş kuralı hataları Warning, beklenmeyen hatalar Error seviyesinde loglanır
        if (exception is BusinessException or ValidationException or AuthorizationException or NotFoundException)
        {
            _logger.LogWarning(
                "[Exception] {ExceptionType} - {Message} - Path: {Path} - User: {User}",
                exception.GetType().Name,
                exception.Message,
                path,
                user);
        }
        else
        {
            _logger.LogError(
                exception,
                "[Exception] {ExceptionType} - {Message} - Path: {Path} - User: {User}",
                exception.GetType().Name,
                exception.Message,
                path,
                user);
        }
    }
}
