using Azure.Data.Tables;
using Lekha.Scheduer.DataAccess;
using Lekha.Scheduler.BusinessLogic;
using Lekha.Scheduler.BusinessLogic.Consumers;
using MassTransit;
using MassTransit.Definition;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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

                    services.AddMassTransitHostedService(true);

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