using Jaeger;
using Jaeger.Samplers;
using Microsoft.Extensions.Logging;

namespace Jaeger_NETCore_Sample.Infrastructure
{
    public static class Tracing
    {
        public static Tracer Init(string serviceName, ILoggerFactory loggerFactory)
        {
            var samplerConfiguration = new Configuration.SamplerConfiguration(loggerFactory)
                .WithType(ConstSampler.Type)
                .WithParam(1);

            var senderConfiguration = new Configuration.SenderConfiguration(loggerFactory)
                .WithAgentHost("srvdocker2-t")
                .WithAgentPort(46831);

            var reporterConfiguration = new Configuration.ReporterConfiguration(loggerFactory)
                .WithSender(senderConfiguration)
                .WithLogSpans(true);

            return (Tracer)new Configuration(serviceName, loggerFactory)
                .WithSampler(samplerConfiguration)
                .WithReporter(reporterConfiguration)
                .GetTracer();
        }
    }
}
