using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jaeger;
using Jaeger_NETCore_Sample.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTracing;
using OpenTracing.Util;

namespace Jaeger_NETCore_Sample
{
    public class Startup
    {
        private static readonly ILoggerFactory LoggerFactory = new LoggerFactory().AddConsole();
        private static readonly Tracer Tracer = Tracing.Init("Sample.Services", LoggerFactory);


        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddEvertrustMessageHandler(option =>
            {
                option.DefaultVersion = "v1";
                option.Domain = "sample";
            });

            services.AddMvc(option =>
            {
                option.Filters.Add<TracerFilter>();
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
    }
}
