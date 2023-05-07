using Lekha.GrainSiloClient;

namespace Lekha.Client.ConsoleApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var siloClient = new App();
            await siloClient.Start();
        }
    }
}