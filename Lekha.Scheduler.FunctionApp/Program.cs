using Azure.Data.Tables;
using Lekha.Scheduer.DataAccess;
using Lekha.Scheduler.BusinessLogic;
using Lekha.Scheduler.BusinessLogic.Consumers;
using MassTransit;
using MassTransit.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace Lekha.Scheduler.FunctionApp
{
    public class Program
    {
        public static void Main()
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices((ctx, services) =>
                {
                    services.AddScoped<IEventManager, EventManager>();
                    services.AddScoped<IScheduleDomain, ScheduleDomain>();
                    services.AddScoped<IOrganizationDataAccess, OrganizationDataAccess>();
                    services.AddScoped<IAccountDataAccess, AccountDataAccess>();
                    services.AddScoped<ITaskGroupDefinitionDataAccess, TaskGroupDefinitionDataAccess>();
                    services.AddScoped<ITaskletDataAccess, TaskletDataAccess>();

                    services.AddScoped<AccountScheduler>();
                    services.AddScoped<TaskGroupScheduler>();

                    services.ConfigurePollyPolicies(ctx.Configuration);

                    services.AddMassTransit(x =>
                    {
                        // https://stackoverflow.com/questions/67356795/in-memory-masstransit-scheduled-message
                        //     AddDelayedMessageScheduler is for all transports - not for just in-memory
                        x.AddDelayedMessageScheduler();

                        x.AddConsumer<AccountMessageConsumer>();
                        x.AddConsumer<TaskGroupMessageConsumer>();
                        x.AddConsumer<TaskGroupExecutionMessageConsumer>();

                        //x.SetKebabCaseEndpointNameFormatter();

                        x.UsingInMemory((context, cfg) =>
                        {
                            // https://stackoverflow.com/questions/67356795/in-memory-masstransit-scheduled-message
                            //     UseDelayedMessageScheduler is for all transports - not for just in-memory
                            cfg.UseDelayedMessageScheduler();

                            cfg.ConfigureEndpoints(context, new CustomEndpointNameFormatter(ctx.Configuration));
                        });
                    });

                    //
                    // Reference: https://masstransit.io/support/upgrade
                    // AddMassTransitHostedService(deprecated)
                    // Previous versions of MassTransit required the use of the MassTransit.AspNetCore package to
                    //   support registration of MassTransit's hosted service.
                    //   This package is no longer required, and MassTransit will automatically add an IHostedService for MassTransit.
                    //

                    //   The host can be configured using IOptions configuration support, such as shown below:

                    services.Configure<MassTransitHostOptions>(options =>
                    {
                        options.WaitUntilStarted = true;
                        options.StartTimeout = TimeSpan.FromSeconds(30);
                        options.StopTimeout = TimeSpan.FromMinutes(1);
                    });

                    // The.NET Generic Host has its own internal timers for shutdown, etc.that may also need to be adjusted.
                    //   For MassTransit, configure the Generic Host options as shown.
                    // services.Configure<HostOptions>(opts => opts.ShutdownTimeout = TimeSpan.FromMinutes(1));

                    var connectionString = ctx.Configuration.GetConnectionString("LekhaTables");
                    services.AddSingleton(new TableClient(connectionString, "TaskGroupExecution"));
                })
                .ConfigureLogging((ctx, logging) =>
                {
                    logging.AddConsole();
                })
                .Build();

            host.Run();
        }
    }

    public class CustomEndpointNameFormatter :
        DefaultEndpointNameFormatter
    {
        private readonly IConfiguration configuration;

        public CustomEndpointNameFormatter(IConfiguration configuration)
            : base(includeNamespace: false)
        {
            this.configuration = configuration;
        }

        protected CustomEndpointNameFormatter()
        {
        }

        public new static IEndpointNameFormatter Instance { get; } = new CustomEndpointNameFormatter();

        public override string SanitizeName(string name)
        {
            return configuration.GetValue<string>($"{name}MassTransitEndpoint", name);
        }
    }
}