using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace LoanManagementSystem.API.Middleware
{
    /// <summary>
    /// Logs the raw request body for debugging purposes. Enables buffering so the request
    /// can still be read by MVC model binders.
    /// </summary>
    public class RequestBodyLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestBodyLoggingMiddleware> _logger;

        public RequestBodyLoggingMiddleware(RequestDelegate next, ILogger<RequestBodyLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Only log for requests with bodies and for debug scenarios
            if (context.Request.ContentLength > 0 &&
                context.Request.Body != null)
            {
                try
                {
                    context.Request.EnableBuffering();

                    using var reader = new StreamReader(
                        context.Request.Body,
                        Encoding.UTF8,
                        detectEncodingFromByteOrderMarks: false,
                        bufferSize: 1024,
                        leaveOpen: true);

                    var body = await reader.ReadToEndAsync();
                    context.Request.Body.Position = 0;

                    if (!string.IsNullOrWhiteSpace(body))
                    {
                        // Mask potential sensitive values (very basic)
                        var masked = body.Replace("\"KeySecret\":\"your_secret_here\"", "\"KeySecret\":\"****\"");
                        _logger.LogInformation("Incoming Request {Method} {Path} Body: {Body}",
                            context.Request.Method, context.Request.Path, masked);
                    }
                }
                catch (System.Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to read request body for logging");
                    context.Request.Body.Position = 0;
                }
            }

            await _next(context);
        }
    }
}
