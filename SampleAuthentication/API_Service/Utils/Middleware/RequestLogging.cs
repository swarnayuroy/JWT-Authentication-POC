using System.Diagnostics;

namespace API_Service.Utils.Middleware
{
    public sealed class RequestLogging
    {
        private readonly RequestDelegate _next;
        private readonly LoggerService<RequestLogging> _logger;

        public RequestLogging(RequestDelegate next, ILogger<RequestLogging> logger)
        {
            _next = next;
            _logger = new LoggerService<RequestLogging>(logger);
        }

        public async Task InvokeAsync(HttpContext context) 
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                
                var logLevel = context.Response.StatusCode >= StatusCodes.Status500InternalServerError? LogLevel.Error : LogLevel.Information;
                string message = $"HTTP {context.Request.Method} {context.Request.Path} completed with {context.Response.StatusCode} in {stopwatch.ElapsedMilliseconds} ms. CorrelationId: {context.TraceIdentifier}";

                _logger.LogDetails(logLevel == LogLevel.Error ? LogType.ERROR : LogType.INFO, message);
            }
        }
    }
}
