using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

#nullable disable

namespace CloudStorage.WebApi.Shared.Infrastructure
{
    /// <summary>
    /// 
    /// </summary>
    public class ErrorLoggingMiddleware
    {
        private readonly ILogger<ErrorLoggingMiddleware> _logger;

        private readonly RequestDelegate _next;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="next"></param>
        /// <param name="logger"></param>
        public ErrorLoggingMiddleware(RequestDelegate next, ILogger<ErrorLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context /* other scoped dependencies */)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }
    }
}
