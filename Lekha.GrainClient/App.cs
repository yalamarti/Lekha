using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

using Lekha.GrainInterfaces;

namespace Lekha.GrainSiloClient
{
    public class App
    {
        public async Task<int> Start()
        {

            try
            {
                using IHost host = await StartClientAsync();
                var client = host.Services.GetRequiredService<IClusterClient>();

                await DoClientWorkAsync(client);
                Console.ReadKey();

                await host.StopAsync();

                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine($$"""
        Exception while trying to run client: {{e.Message}}
        Make sure the silo the client is trying to connect to is running.
        Press any key to exit.
        """);

                Console.ReadKey();
                return 1;
            }

        }

        static async Task<IHost> StartClientAsync()
        {
            var builder = new HostBuilder()
                .UseOrleansClient(client =>
                {
                    client.UseLocalhostClustering();
                })
                .ConfigureLogging(logging => logging.AddConsole());

            var host = builder.Build();
            await host.StartAsync();

            Console.WriteLine("Client successfully connected to silo host \n");

            return host;
        }

        static async Task DoClientWorkAsync(IClusterClient client)
        {
            var appGrain = client.GetGrain<IOrganizationGrain>("CustomerJohnDoesAppInstance");
            var response = await appGrain.BeginTaskGroupExecution(new BeginOrganizationExecutionRequest
            {
                
            });

            Console.WriteLine($"\n\n{response.Result}\n\n");
        }
    }

}