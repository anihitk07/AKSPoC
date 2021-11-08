using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AKSWebApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            RegisterAppInsights(services);
            services.AddRazorPages();
        }

        private void RegisterAppInsights(IServiceCollection services)
        {
            ApplicationInsightsServiceOptions options = new ApplicationInsightsServiceOptions
            {
                EnableDebugLogger = false,
                InstrumentationKey = "200988dd-4f99-4079-94d3-435f5b34dfa6"
            };
            services.AddApplicationInsightsTelemetry(options);

            services.AddApplicationInsightsKubernetesEnricher();

            var dependencyCounterService = services.FirstOrDefault<ServiceDescriptor>(t => t.ImplementationType == typeof(DependencyTrackingTelemetryModule));
            if (dependencyCounterService != null)
            {
                services.Remove(dependencyCounterService);
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
