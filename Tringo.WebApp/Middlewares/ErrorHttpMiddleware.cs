using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Tringo.WebApp.Middlewares
{
    public class ErrorHttpMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHttpMiddleware> _logger;

        public ErrorHttpMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = loggerFactory?.CreateLogger<ErrorHttpMiddleware>()
                    ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next.Invoke(context);
            }
            catch (Exception e)
            {
                HandleError(e, context);
                context.Response.Clear();
                context.Response.StatusCode = 500;
                context.Response.ContentType = @"text/plain";
                await context.Response.WriteAsync("Unknown Error");
            }
        }

        private void HandleError(Exception exception, HttpContext context)
        {
            _logger.LogError(exception.ToString());
        }
    }
}
