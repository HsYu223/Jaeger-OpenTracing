# Jaeger NET Core

#### Startup.cs

```C#
private static readonly ILoggerFactory LoggerFactory = new LoggerFactory().AddConsole();
private static readonly Tracer Tracer = Tracing.Init("Setting.Services", LoggerFactory);

public void ConfigureServices(IServiceCollection services)
{
    services.AddEvertrustMessageHandler(option =>
    {
        option.DefaultVersion = "v1";
        option.Domain = "sample";
    });

    services.AddMvc(option =>
    {
        var service = services.BuildServiceProvider();
        option.Filters.Add(new TracerFilter(service.GetService<ITracer>()));
        option.AddEvertrustMessageFilters();
        option.AddEvertrustValidation();
    }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

    GlobalTracer.Register(Tracer);
    services.AddOpenTracing(); 
}

// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseEvertrustMessageHandler();
    app.UseMvc();
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
```