using System.Text.Json;
using Fundo.Applications.WebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace Fundo.Applications.WebApi.Middleware
{
    /// <summary>
    /// Converts unhandled domain and runtime exceptions into consistent
    /// ProblemDetails JSON responses, without leaking stack traces to clients.
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
            catch (LoanNotFoundException ex)
            {
                await WriteProblemAsync(context, StatusCodes.Status404NotFound, "Loan not found", ex.Message);
            }
            catch (InvalidPaymentException ex)
            {
                await WriteProblemAsync(context, StatusCodes.Status400BadRequest, "Invalid payment", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception processing {Method} {Path}",
                    context.Request.Method, context.Request.Path);
                await WriteProblemAsync(context, StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred", "An unexpected error occurred while processing your request.");
            }
        }

        private static async Task WriteProblemAsync(HttpContext context, int statusCode, string title, string detail)
        {
            if (context.Response.HasStarted)
            {
                return;
            }

            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = context.Request.Path
            };

            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
        }
    }
}
