using Microsoft.AspNetCore.Mvc;

namespace API_Service.Utils.Middleware
{
    public sealed class ExceptionHandling
    {
        private readonly RequestDelegate _next;
        private readonly LoggerService<ExceptionHandling> _logger;

        public ExceptionHandling(RequestDelegate next, ILogger<ExceptionHandling> logger)
        {
            _next = next;
            _logger = new LoggerService<ExceptionHandling>(logger);
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
            string errorMessage = $"Unhandled exception for request {context.Request.Method} {context.Request.Path}. CorrelationId: {context.TraceIdentifier} - {exception.Message}";
            _logger.LogDetails(LogType.ERROR, errorMessage);

            if (context.Response.HasStarted)
            {
                throw exception;
            }
            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred.",
                Detail = "Use the correlation ID when contacting support.",
                Instance = context.Request.Path
            };

            await context.Response.WriteAsJsonAsync(problemDetails);
        }
    }
}
