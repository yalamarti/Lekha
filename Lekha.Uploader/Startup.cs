using Lekha.Infrastructure;
using Lekha.Models;
using Lekha.Uploader.ActionFilter;
using Lekha.Uploader.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
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
}
