using Lekha.Infrastructure;
using Lekha.Models;
using Lekha.Uploader.ActionFilter;
using Lekha.Uploader.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Exporter;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System;
using System.IO;
using System.Reflection;

namespace Lekha.Uploader
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var applicationContext = new UploaderApplicationContext
            {
                AppName = Configuration["AppName"],
                Service = Configuration["ServiceName"]
            };
            services.AddSingleton(applicationContext);
            services.AddSingleton<ApplicationContext>(applicationContext);
            services.AddControllers(options => options.Filters.Add<HttpResponseExceptionFilter>());

            services.AddSingleton<IUploadService, UploadService>();
            services.AddSingleton<IBlobClientService<UploadDocument>, BlobClientService<UploadDocument>>();

            // Reference: https://docs.microsoft.com/en-us/dotnet/architecture/dapr-for-net-developers/getting-started
            // Troubleshooting - https://pomeroy.me/2020/09/solved-windows-10-forbidden-port-bind/
            //   Had to change the ports from 51000 / 52000 to 50080 and 50090 after referring to the above article
            //     in the individual components
            //     Running the "netsh int ipv4 show excludedportrange protocol=tcp"  showed that 51000 is in use
            services.AddDaprClient();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Lekha Uploader API",
                    Description = "API providing functionality for uploading documents",
                    TermsOfService = new Uri("https://github.com/yalamarti/Lekha"),
                    Contact = new OpenApiContact
                    {
                        Name = "Lekha on github",
                        Email = string.Empty,
                        Url = new Uri("https://github.com/yalamarti/Lekha/issues"),
                    }
                });
                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            //
            // Instrumentation
            //

            // Build a resource configuration action to set service information.
            Action<ResourceBuilder> configureResource = r => r.AddService(
                serviceName: applicationContext.Service,
                serviceVersion: typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown",
                serviceInstanceId: Environment.MachineName);

            // Create a service to expose ActivitySource, and Metric Instruments
            // for manual instrumentation
            services.AddSingleton<Instrumentation>();

            //// Reference: https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/examples/AspNetCore/Startup.cs
            //// Switch between Zipkin/Jaeger/OTLP by setting UseExporter in appsettings.json.
            //var tracingExporter = this.Configuration.GetValue<string>("UseTracingExporter").ToLowerInvariant();
            //switch (tracingExporter)
            //{
            //    case "jaeger":
            //        services.AddOpenTelemetryTracing((builder) => builder
            //            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(this.Configuration.GetValue<string>("Jaeger:ServiceName")))
            //            .AddAspNetCoreInstrumentation()
            //            .AddHttpClientInstrumentation()
            //            .AddJaegerExporter());

            //        services.Configure<JaegerExporterOptions>(this.Configuration.GetSection("Jaeger"));

            //        // Customize the HttpClient that will be used when JaegerExporter is configured for HTTP transport.
            //        services.AddHttpClient("JaegerExporter", configureClient: (client) => client.DefaultRequestHeaders.Add("X-MyCustomHeader", "value"));
            //        break;
            //    case "zipkin":
            //        services.AddOpenTelemetryTracing((builder) => builder
            //            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(this.Configuration.GetValue<string>("Zipkin:ServiceName")))
            //            .AddAspNetCoreInstrumentation()
            //            .AddHttpClientInstrumentation()
            //            .AddZipkinExporter());

            //        services.Configure<ZipkinExporterOptions>(this.Configuration.GetSection("Zipkin"));
            //        break;
            //    default:
            //        services.AddOpenTelemetryTracing((builder) => builder
            //            .AddAspNetCoreInstrumentation()
            //            .AddHttpClientInstrumentation()
            //            .AddConsoleExporter());

            //        // For options which can be bound from IConfiguration.
            //        services.Configure<AspNetCoreInstrumentationOptions>(this.Configuration.GetSection("AspNetCoreInstrumentation"));

            //        // For options which can be configured from code only.
            //        services.Configure<AspNetCoreInstrumentationOptions>(options =>
            //        {
            //            options.Filter = (req) =>
            //            {
            //                return req.Request.Host.HasValue;
            //            };
            //        });

            //        break;
            //}

            //
            // Configure OpenTelemetry tracing & metrics with auto-start using the
            // AddOpenTelemetry extension from OpenTelemetry.Extensions.Hosting.
            //

            // Switch between Zipkin/Jaeger/OTLP by setting UseExporter in appsettings.json.
            var tracingExporter = this.Configuration.GetValue<string>("UseTracingExporter").ToLowerInvariant();

            services.AddOpenTelemetry()
                .ConfigureResource(configureResource)
                .WithTracing(builder =>
                {
                    // Tracing

                    // Ensure the TracerProvider subscribes to any custom ActivitySources.
                    builder
                        .AddSource(Instrumentation.ActivitySourceName)
                        .SetSampler(new AlwaysOnSampler())
                        .AddHttpClientInstrumentation()
                        .AddAspNetCoreInstrumentation();

                    // Use IConfiguration binding for AspNetCore instrumentation options.
                    services.Configure<AspNetCoreInstrumentationOptions>(Configuration.GetSection("AspNetCoreInstrumentation"));

                    switch (tracingExporter)
                    {
                        case "jaeger":
                            builder.AddJaegerExporter();

                            builder.ConfigureServices(services =>
                            {
                                // Use IConfiguration binding for Jaeger exporter options.
                                services.Configure<JaegerExporterOptions>(Configuration.GetSection("Jaeger"));

                                // Customize the HttpClient that will be used when JaegerExporter is configured for HTTP transport.
                                services.AddHttpClient("JaegerExporter", configureClient: (client) => client.DefaultRequestHeaders.Add("X-MyCustomHeader", "value"));
                            });
                            break;

                        case "zipkin":
                            builder.AddZipkinExporter();

                            builder.ConfigureServices(services =>
                            {
                                // Use IConfiguration binding for Zipkin exporter options.
                                services.Configure<ZipkinExporterOptions>(Configuration.GetSection("Zipkin"));
                            });
                            break;

                        default:
                            builder.AddConsoleExporter();
                            break;
                    }
                })
                .WithMetrics(builder =>
                {
                    // Metrics
                    var metricsExporter = this.Configuration.GetValue<string>("UseMetricsExporter").ToLowerInvariant();


                    switch (metricsExporter)
                    {
                        case "prometheus":
                            builder.AddPrometheusExporter();
                            break;
                        default:
                            builder.AddConsoleExporter();
                            break;
                    }
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseStaticFiles();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            if (env.IsDevelopment())
            {
                app.UseExceptionHandler("/error-local-development");
            }
            else
            {
                app.UseExceptionHandler("/error");
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            
        }
    }


    // Task-In-Motion
    //    TIMO framework
    //    TASKIMO framework

    // Task Delivery Framework
    //    T-Del Framework


}
