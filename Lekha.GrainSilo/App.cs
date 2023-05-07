using Microsoft.Extensions.Hosting;
using System.Net.NetworkInformation;

namespace Lekha.GrainSilo
{
    using Lekha.GrainInterfaces;
    using MassTransit;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Orleans.Hosting;
    using System.Reflection;
    using System.Text.Json;

    public class App
    {
        public async Task<int> Start(string[] args)
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            var taskGroupDataFileName = Path.Combine(path, "SampleData/Organization.json");

            switch (args.Length)
            {
                default:
                    Console.WriteLine("*** Invalid command line arguments.");
                    return -1;
                case 0:
                    break;
                case 1:
                    taskGroupDataFileName = args[0];
                    break;
            }

            if (!File.Exists(taskGroupDataFileName))
            {
                Console.WriteLine("*** File not found: {0}", taskGroupDataFileName);
                return -2;
            }

            try
            {
                using IHost host = await App.StartTaskGroupSiloAsync(taskGroupDataFileName);
                Console.WriteLine("\n\n Press Enter to terminate...\n\n");
                Console.ReadLine();

                await host.StopAsync();

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return 1;
            }
        }

        static async Task<IHost> StartTaskGroupSiloAsync(string organizationDataFileName)
        {
            var builder = new HostBuilder()
                .UseOrleans(siloBuilder =>
                {
                    siloBuilder.UseLocalhostClustering()
                        .ConfigureLogging(logging => logging.AddConsole());
                    siloBuilder.AddAzureTableGrainStorage(
                        name: "organizationStore",
                        configureOptions: options =>
                        {
                            // Configure the storage connection key
                            options.ConfigureTableServiceClient(
                                "UseDevelopmentStorage=true");
                        });
                    siloBuilder.AddAzureBlobGrainStorage(
                            name: "accountStore",
                            configureOptions: options =>
                            {
                                // Configure the storage connection key
                                options.ConfigureBlobServiceClient(
                                    "UseDevelopmentStorage=true");
                            });

                    siloBuilder.AddAzureBlobGrainStorage(
                            name: "taskGroupStore",
                            configureOptions: options =>
                            {
                                // Configure the storage connection key
                                options.ConfigureBlobServiceClient(
                                    "UseDevelopmentStorage=true");
                            });
                })
                .ConfigureLogging(logging => logging.AddConsole());

            builder.ConfigureServices(services =>
            {
                services.AddMassTransit(x =>
                {
                    x.UsingRabbitMq((context, cfg) =>
                    {
                        cfg.ConfigureEndpoints(context);
                        cfg.Publish<TestMessage>();
                    });
                });

            });

            var host = builder.Build();
            await host.StartAsync();

            Console.WriteLine("Task Groups Data file name is '{0}'.", organizationDataFileName);
            Console.WriteLine("Setting up Lekha Task Grups, please wait ...");


            // Initialize the Task Group App 
            var client = host.Services.GetRequiredService<IGrainFactory>();
            var logger = host.Services.GetRequiredService<ILogger<App>>();

            var adventure = new TaskGroupApp(client, logger);
            await adventure.Configure(organizationDataFileName);

            Console.WriteLine("Setup completed.");
            Console.WriteLine("Now you can launch the client.");

            // Exit when any key is pressed
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
            await host.StopAsync();


            return host;
        }
    }


}