using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

#nullable disable

namespace CloudStorage.WebApi.Shared.Infrastructure
{
    class CustomErrorResponseMiddleware
    {
        private readonly ILogger<CustomErrorResponseMiddleware> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly RequestDelegate _next;

        public CustomErrorResponseMiddleware(RequestDelegate next, ILogger<CustomErrorResponseMiddleware> logger,
            IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task Invoke(HttpContext context /* other scoped dependencies */)
        {
            var exceptionDetails = context.Features.Get<IExceptionHandlerFeature>();
            var ex = exceptionDetails?.Error;

            var includeDetails = _env.IsDevelopment();

            // Should always exist, but best to be safe!
            if (ex != null)
            {
                // ProblemDetails has it's own content type
                context.Response.ContentType = "application/problem+json";

                // Get the details to display, depending on whether we want to expose the raw exception
                var title = includeDetails ? "An error occured: " + ex.Message : "An error occured";
                var details = includeDetails ? ex.Message : null;

                var problem = new Microsoft.AspNetCore.Mvc.ProblemDetails
                {
                    Status = 500,
                    Title = title,
                    Detail = details
                };

                // This is often very handy information for tracing the specific request
                var traceId = System.Diagnostics.Activity.Current?.Id ?? context?.TraceIdentifier;
                if (traceId != null)
                {
                    problem.Extensions["traceId"] = traceId;
                }

                //Serialize the problem details object to the Response as JSON (using System.Text.Json)
                await context.Response.WriteAsync(JsonConvert.SerializeObject(problem));
            }
        }
    }
}
