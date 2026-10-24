using Microsoft.AspNetCore.Http;

namespace LoanManagementSystem.API.Middleware
{
    /// <summary>
    /// Passes requests through without logging potentially sensitive request bodies.
    /// </summary>
    public class RequestBodyLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        public RequestBodyLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);
        }
    }
}
