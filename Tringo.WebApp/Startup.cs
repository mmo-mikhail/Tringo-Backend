using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.AzureAppServices;
using Newtonsoft.Json;
using Tringo.FlightsService;
using Tringo.FlightsService.Impls;
using Tringo.WebApp.HealthChecks;
using Tringo.WebApp.Middlewares;

namespace Tringo.WebApp
{
    public class Startup
    {
        private readonly ILogger _logger;

        private readonly string[] _corsPolicyOrigins;
        private const string DevCors = "DevCors";

        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            Configuration = configuration;
            _corsPolicyOrigins = configuration.GetSection("CorsPolicyOrigins:AllowedOrigins").Get<string[]>();
            _logger = logger;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(DevCors,
                builder =>
                {
                    builder.WithOrigins( _corsPolicyOrigins);
                });
            });

            services.AddMvc()
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddSingleton<IFlightsService>(new MockFlightsService());
            services.AddSingleton<IDestinationsFilter>(new DestinationsFilter());

            //Health Checks
            services.AddHealthChecks()
                .AddCheck<MainHealthCheck>(
                    nameof(MainHealthCheck),
                    failureStatus: HealthStatus.Degraded,
                    tags: new[] { "initial" });

            // TODO Polly. Will be set up later
            services.AddHttpClient();

            // Configure logging (text files) to Azure FileSystem
            services.Configure<AzureFileLoggerOptions>(options =>
            {
                options.FileName = "azure-diagnostics-";
                options.FileSizeLimit = 50 * 1024;
                options.RetainedFileCountLimit = 5;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            //The order is critical for security, performance, and functionality.
            //The following Startup.Configure method adds middleware components for common app scenarios:
            //1 Exception / error handling
            //2 HTTP Strict Transport Security Protocol
            //3 HTTPS redirection
            //4 Static file server
            //5 Cookie policy enforcement
            //6 Authentication
            //7 Session
            //8 MVC

            app.UseMiddleware<ErrorHttpMiddleware>();

            if (env.IsDevelopment())
            {
                _logger.LogInformation("Starting Up In Development Environment");
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                //app.UseHsts();
            }

            app.UseCors(DevCors);

            //app.UseHttpsRedirection();
            app.UseMvc();

            app.UseHealthChecks("/health", new HealthCheckOptions()
            {
                Predicate = (check) => check.Tags.Contains("initial"),
                AllowCachingResponses = false,
            });
        }
    }
}
