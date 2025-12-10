using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

namespace HRSystem.API.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var requestId = Guid.NewGuid().ToString();
            var stopwatch = Stopwatch.StartNew();

            _logger.LogInformation(
                "HTTP {Method} {Path} started. RequestId: {RequestId}, UserAgent: {UserAgent}, RemoteIP: {RemoteIP}",
                context.Request.Method,
                context.Request.Path,
                requestId,
                context.Request.Headers["User-Agent"].ToString(),
                context.Connection.RemoteIpAddress);

            try
            {
                await _next(context);
                stopwatch.Stop();

                var logLevel = context.Response.StatusCode >= 400 ? LogLevel.Warning : LogLevel.Information;
                _logger.Log(logLevel,
                    "HTTP {Method} {Path} completed. RequestId: {RequestId}, StatusCode: {StatusCode}, Duration: {Duration}ms",
                    context.Request.Method,
                    context.Request.Path,
                    requestId,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex,
                    "Unhandled exception in HTTP {Method} {Path}. RequestId: {RequestId}, Duration: {Duration}ms",
                    context.Request.Method,
                    context.Request.Path,
                    requestId,
                    stopwatch.ElapsedMilliseconds);

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync($"{{\"error\":\"An error occurred processing your request.\",\"requestId\":\"{requestId}\"}}");
            }
        }
    }
}