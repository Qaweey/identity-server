using Microsoft.AspNetCore.Builder;

namespace settl.identityserver.Domain.Shared.Middlewares
{
    public static class MiddlewareHelper
    {
        public static IApplicationBuilder UseRequestResponseLoggingMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RequestResponseLoggingMiddleware>();
        }
    }
}