using Microsoft.EntityFrameworkCore;
using System.Net;

namespace ProductCatalog.API.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Resource not found.");
                await HandleNotFoundAsync(context, ex);
            }
            catch (DbUpdateConcurrencyException)
            {
                await HandleConflictAsync(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleNotFoundAsync(HttpContext context, KeyNotFoundException ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;

            var result = new { message = "Resource not found.", detail = ex.Message };
            return context.Response.WriteAsJsonAsync(result);
        }
        
        private static Task HandleConflictAsync(HttpContext context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Conflict;
            return context.Response.WriteAsync("Conflict: The resource was updated by another user.");
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var result = new
            {
                message = "An internal server error occurred.",
                detail = ex.Message
            };
            return context.Response.WriteAsJsonAsync(result);
        }
    }
}
