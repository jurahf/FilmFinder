using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EsConsultationApi.Infrastructure;

/// <summary>
/// Переводит исключения приложения в ProblemDetails с понятным статусом.
/// </summary>
public sealed class ApiExceptionMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogger<ApiExceptionMiddleware> logger;

    public ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
    {
        this.next = next;
        this.logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            if (context.Response.HasStarted)
                throw;

            var (status, title) = Map(exception);

            if (status >= StatusCodes.Status500InternalServerError)
                logger.LogError(exception, "Необработанная ошибка API консультаций.");
            else
                logger.LogInformation(exception, "Ошибка API консультаций: {Status}.", status);

            context.Response.Clear();
            context.Response.StatusCode = status;
            context.Response.ContentType = "application/problem+json";

            var problem = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = exception.Message,
                Instance = context.Request.Path
            };

            problem.Extensions["traceId"] = context.TraceIdentifier;

            await context.Response.WriteAsJsonAsync(
                problem,
                options: null,
                contentType: "application/problem+json");
        }
    }

    private static (int Status, string Title) Map(Exception exception)
    {
        return exception switch
        {
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Не найдено"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Некорректный запрос"),
            InvalidOperationException => (StatusCodes.Status409Conflict, "Конфликт состояния консультации"),
            DbUpdateException => (StatusCodes.Status409Conflict, "Конфликт данных"),
            _ => (StatusCodes.Status500InternalServerError, "Внутренняя ошибка сервера")
        };
    }
}
