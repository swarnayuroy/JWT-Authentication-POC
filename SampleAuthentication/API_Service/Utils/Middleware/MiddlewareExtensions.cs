namespace API_Service.Utils.Middleware
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseApiMiddleware(this IApplicationBuilder app) 
        {
            return app
                .UseMiddleware<SetCorrelationId>()
                .UseMiddleware<RequestLogging>()
                .UseMiddleware<ExceptionHandling>();
        }
    }
}
