# Jaeger MVC

#### WebApiConfig.cs

```C#
private static readonly ILoggerFactory LoggerFactory = new LoggerFactory().AddConsole();
private static readonly Tracer Tracer = Tracing.Init("{project name}", LoggerFactory);

public static void Register(HttpConfiguration config)
{
   config.Filters.Add(new TracingFilter(Tracer));
}
```

#### Tracing.cs

```C#
public static class Tracing
{
    public static Tracer Init(string serviceName, ILoggerFactory loggerFactory)
    {
        var samplerConfiguration = new Configuration.SamplerConfiguration(loggerFactory)
            .WithType(ConstSampler.Type)
            .WithParam(1);

        var senderConfiguration = new Configuration.SenderConfiguration(loggerFactory)
            .WithAgentHost("{jaeger agent domain}")
            .WithAgentPort({jaeger agent port});

        var reporterConfiguration = new Configuration.ReporterConfiguration(loggerFactory)
            .WithSender(senderConfiguration)
            .WithLogSpans(true);

        return (Tracer)new Configuration(serviceName, loggerFactory)
            .WithSampler(samplerConfiguration)
            .WithReporter(reporterConfiguration)
            .GetTracer();
    }
}
```

#### TracingFilter.cs

```C#
public class TracingFilter : ActionFilterAttribute
{
    private ITracer _tracer;

    /// <summary>
    /// Initializes a new instance of the <see cref="TracingFilter"/> class.
    /// </summary>
    /// <param name="tracer">The tracer.</param>
    public TracingFilter(ITracer tracer)
    {
        this._tracer = tracer;
    }

    /// <summary>
    /// Called when [action executed asynchronous].
    /// </summary>
    /// <param name="actionExecutedContext">The action executed context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    public override async Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
    {
        var controller = actionExecutedContext.ActionContext.ControllerContext.ControllerDescriptor.ControllerName;
        var action = actionExecutedContext.ActionContext.ActionDescriptor.ActionName;
        var content = JObject.Parse(await actionExecutedContext.Response.Content.ReadAsStringAsync());
        content.TryGetValue("id", StringComparison.CurrentCultureIgnoreCase, out var id);

        using (var scope = this._tracer.BuildSpan($"{controller} - {action}").StartActive(true))
        {
            scope.Span.SetTag("id", (string)id);
            scope.Span.Log(new Dictionary<string, object>
            {
                [LogFields.Event] = $"{controller}-{action}",
                ["input"] = JsonConvert.SerializeObject(actionExecutedContext.ActionContext.ActionArguments),
                ["output"] = JsonConvert.SerializeObject(content)
            });

            await base.OnActionExecutedAsync(actionExecutedContext, cancellationToken);
        }
    }
}
```

