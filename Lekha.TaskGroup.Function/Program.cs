using Lekha.TaskGroup.Function;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MassTransit;
using Lekha.GrainInterfaces;
using Lekha.TaskGroup.Function.Consumers;

var hostBuilder = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults();

hostBuilder.ConfigureServices(services =>
{
    services
        .AddScoped<TaskGroupDefinitionFunctions>()
        
        .AddMassTransit(cfg =>
        {
            cfg.AddConsumersFromNamespaceContaining<ConsumerNamespace>();
            cfg.AddRequestClient<TaskGroupDefinition>(new Uri("queue:task-group-definition"));
            cfg.AddRequestClient<TestMessage>(new Uri("queue:test-queue"));
            
            cfg.UsingRabbitMq((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });
        });
});

var host = hostBuilder.Build();

host.Run();
