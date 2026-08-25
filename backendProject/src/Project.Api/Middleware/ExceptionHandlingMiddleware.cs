using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Project.Api.Errors;
using Project.Domain;
using Project.Domain.Exceptions;
using ValidationException = Project.Application.Common.Exceptions.ValidationException;

namespace Project.Api.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private static readonly IReadOnlyDictionary<string, HttpStatusCode> StatusByCode =
            new Dictionary<string, HttpStatusCode>
            {
                [ErrorCodes.ValidationError] = HttpStatusCode.BadRequest,
                [ErrorCodes.EmployeeRateNotFound] = HttpStatusCode.BadRequest,
                [ErrorCodes.DateOutOfProjectPeriod] = HttpStatusCode.BadRequest,

                [ErrorCodes.DailyHoursLimitExceeded] = HttpStatusCode.Conflict,
                [ErrorCodes.PeriodClosed] = HttpStatusCode.Conflict,
                [ErrorCodes.ConcurrencyConflict] = HttpStatusCode.Conflict,
                [ErrorCodes.PeriodAlreadyClosed] = HttpStatusCode.Conflict,

                [ErrorCodes.NotFound] = HttpStatusCode.NotFound
            };

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

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
            catch (ValidationException exception)
            {
                await WriteAsync(
                    context,
                    HttpStatusCode.BadRequest,
                    new ApiError(
                        ErrorCodes.ValidationError,
                        exception.Message,
                        exception.Errors.Select(e => new ApiFieldError(e.Field, e.Message)).ToList()));
            }
            catch (BusinessRuleException exception)
            {
                var status = StatusByCode.TryGetValue(exception.Code, out var mapped)
                    ? mapped
                    : HttpStatusCode.BadRequest;

                _logger.LogInformation(
                    "Отклонено бизнес-правилом: {Code} — {Message}",
                    exception.Code,
                    exception.Message);

                await WriteAsync(context, status, new ApiError(exception.Code, exception.Message));
            }
            catch (ArgumentException exception)
            {
                await WriteAsync(
                    context,
                    HttpStatusCode.BadRequest,
                    new ApiError(ErrorCodes.ValidationError, exception.Message));
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Необработанная ошибка при обработке запроса {Path}", context.Request.Path);

                await WriteAsync(
                    context,
                    HttpStatusCode.InternalServerError,
                    new ApiError(
                        ErrorCodes.InternalError,
                        "Внутренняя ошибка сервера. Обратитесь к администратору."));
            }
        }

        private static async Task WriteAsync(HttpContext context, HttpStatusCode status, ApiError error)
        {
            if (context.Response.HasStarted)
                return;

            context.Response.Clear();
            context.Response.StatusCode = (int)status;
            context.Response.ContentType = "application/json; charset=utf-8";

            await context.Response.WriteAsync(JsonSerializer.Serialize(error, JsonOptions));
        }
    }
}
