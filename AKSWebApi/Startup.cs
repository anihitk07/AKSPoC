using AKSWebApi.Attributes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace AKSWebApi
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
            services.AddAntiforgery(options => options.SuppressXFrameOptionsHeader = true);
            services.AddHealthChecks()
               .AddCheck("self", () => HealthCheckResult.Healthy());
            ConfigureSwagger(services);
            services.AddApiVersioning(o =>
            {
                o.ReportApiVersions = true;
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.DefaultApiVersion = new ApiVersion(1, 0);
            });

            services.AddRouting(options => options.LowercaseUrls = true);
            services.AddControllers(mvc =>
            {
                mvc.Conventions.Add(new ControllerNameAttributeConvention());
            });
            services.AddResponseCompression();
            services.Configure<GzipCompressionProviderOptions>
            (options =>
            {
                options.Level = CompressionLevel.Fastest;
            });
            services.AddStartupTask<WarmupServicesStartupTask>().TryAddSingleton(services);

            services.AddHostedService<ApplicationLifetimeService>();
            services.Configure<HostOptions>(opts => opts.ShutdownTimeout = TimeSpan.FromSeconds(45));
        }

        private void ConfigureSwagger(IServiceCollection services)
        {
            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Weather Service", Version = "v1", Description = "Weather Service" });
                
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "basicAuth" }
                        },
                        Array.Empty<string>()
                    }
                });
                c.CustomSchemaIds(type => type.FullName);
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
            });
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
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts(options => options.MaxAge(days: 30).IncludeSubdomains());
            }
            app.UseSwagger(c =>
            {
                c.RouteTemplate = "/aks-api/{documentName}/swagger.json";
            });
            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = "aks-api";
                c.SwaggerEndpoint("./v1/swagger.json", "Weather Service v1");
                c.EnableValidator();
            });
            app.UseHttpsRedirection();

            app.UseRouting();


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                ConfigureHealthCheck(app);
            });
            ConfigureSecurity(app);
        }

        private void ConfigureSecurity(IApplicationBuilder app)
        {
            app.UseXContentTypeOptions();
            app.UseReferrerPolicy(opts => opts.NoReferrer());
            app.UseXXssProtection(opts => opts.EnabledWithBlockMode());
            app.UseXfo(options => options.Deny());
            app.UseRedirectValidation(opts =>
            {
                opts.AllowSameHostRedirectsToHttps();
            });
            app.UseCsp(opts => opts.
                        BlockAllMixedContent()
                        .StyleSources(s => s.None())
                        .FontSources(f => f.None())
                        .FrameSources(f => f.None())
                        .FormActions(f => f.None())
                        .FrameAncestors(f => f.None())
                        .ImageSources(i => i.None())
                        .ChildSources(c => c.None())
                        .BaseUris(b => b.None())
                        .ConnectSources(c => c.None())
                        .DefaultSources(d => d.None())
                        .ManifestSources(m => m.None())
                        .MediaSources(m => m.None())
                        .ObjectSources(o => o.None())
                        .Sandbox()
                        .UpgradeInsecureRequests()
                        .WorkerSources(w => w.None())
                        .ScriptSources(s => s.None()));
        }


        private void ConfigureHealthCheck(IApplicationBuilder app)
        {
            app.UseHealthChecks("/aks-api/self", new HealthCheckOptions
            {
                Predicate = r => r.Name.Contains("self")
            });
            app.UseHealthChecks(path: "/aks-api/hc", new HealthCheckOptions()
            {
                Predicate = _ => true
            });
        }

    }


    public class WarmupServicesStartupTask : IStartupTask
    {
        private readonly IServiceCollection _services;
        private readonly IServiceProvider _provider;
        public WarmupServicesStartupTask(IServiceCollection services, IServiceProvider provider)
        {
            _services = services;
            _provider = provider;
        }

        public Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            using (var scope = _provider.CreateScope())
            {
                foreach (var singleton in GetServices(_services))
                {
                    scope.ServiceProvider.GetServices(singleton);
                }
            }
            return Task.CompletedTask;
        }

        private static IEnumerable<Type> GetServices(IServiceCollection services)
        {
            return services
                .Where(descriptor => descriptor.ImplementationType != typeof(WarmupServicesStartupTask))
                .Where(descriptor => !descriptor.ServiceType.ContainsGenericParameters)
                .Select(descriptor => descriptor.ServiceType)
                .Distinct();
        }
    }

    public interface IStartupTask
    {
        Task ExecuteAsync(CancellationToken cancellationToken = default);
    }

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddStartupTask<T>(this IServiceCollection services)
            where T : class, IStartupTask
        {
            return services.AddTransient<IStartupTask, T>();
        }
    }

    public class ApplicationLifetimeService : IHostedService
    {
        readonly ILogger _logger;
        readonly IHostApplicationLifetime _applicationLifetime;

        public ApplicationLifetimeService(IHostApplicationLifetime applicationLifetime, ILogger<ApplicationLifetimeService> logger)
        {
            _applicationLifetime = applicationLifetime;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // register a callback that sleeps for 30 seconds
            _applicationLifetime.ApplicationStopping.Register(() =>
            {
                _logger.LogInformation("SIGTERM received, waiting for 30 seconds");
                Thread.Sleep(30000);
                _logger.LogInformation("Termination delay complete, continuing stopping process");
            });
            return Task.CompletedTask;
        }

        // Required to satisfy interface
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
