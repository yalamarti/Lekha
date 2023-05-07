namespace Lekha.Server.ConsoleApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var silo = new GrainSilo.App();
            await silo.Start(args);
        }
    }
}