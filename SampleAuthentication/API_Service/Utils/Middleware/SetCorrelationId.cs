namespace API_Service.Utils.Middleware
{
    public sealed class SetCorrelationId
    {
        public const string HeaderName = "X-Correlation-ID";
        private readonly RequestDelegate _next;
        private readonly LoggerService<SetCorrelationId> _logger;

        public SetCorrelationId(RequestDelegate next, ILogger<SetCorrelationId> logger)
        {
            this._next = next;
            this._logger = new LoggerService<SetCorrelationId>(logger);
        }
        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = GetCorrelationId(context);
            context.TraceIdentifier = correlationId;
            context.Response.Headers[HeaderName] = correlationId;

            try
            {
                await _next(context);
            }
            finally
            {
                _logger.LogDetails(LogType.INFO, $"Set CorrelationId: {correlationId} for request {context.Request.Method} {context.Request.Path} {context.Response.StatusCode}");
            }            
        }

        private static string GetCorrelationId(HttpContext context)
        {
            var suppliedId = context.Request.Headers[HeaderName].FirstOrDefault();
            return !string.IsNullOrWhiteSpace(suppliedId) ? suppliedId : context.TraceIdentifier;
        }
    }
}
