using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using OpenTracing;

namespace Jaeger_NETCore_Sample.Infrastructure
{
    public class TracerFilter : IAsyncActionFilter
    {
        private ITracer _tracer;

        public TracerFilter(
            ITracer tracer)
        {
            this._tracer = tracer;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var controller = "";
            context.ActionDescriptor.RouteValues.TryGetValue("controller", out controller);
            var action = "";
            context.ActionDescriptor.RouteValues.TryGetValue("action", out action);
            var executedContext = (await next()).Result;

            if (executedContext is ObjectResult result)
            {
                using (var scope = this._tracer.BuildSpan($"{controller} - {action}").StartActive(true))
                {
                    scope.Span.SetTag("id", context.HttpContext.TraceIdentifier);
                    scope.Span.Log(new Dictionary<string, object>
                    {
                        [LogFields.Event] = $"{controller}-{action}",
                        ["input"] = JsonConvert.SerializeObject(context.ActionArguments),
                        ["output"] = JsonConvert.SerializeObject(result.Value)
                    });
                }
            }
        }
    }
}
