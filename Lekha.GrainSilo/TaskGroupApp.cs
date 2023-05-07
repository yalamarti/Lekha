using Lekha.GrainInterfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Lekha.GrainSilo
{
    internal class TaskGroupApp
    {
        private readonly IGrainFactory _client;
        private readonly ILogger _logger;

        public TaskGroupApp(IGrainFactory client, ILogger logger)
        {
            _client = client;
            _logger = logger;
        }

        internal async Task Configure(string organizationDataFileName)
        {
            // Read the contents of the game file and deserialize it
            var jsonData = await File.ReadAllTextAsync(organizationDataFileName);
            var organizations = JsonSerializer.Deserialize<List<Organization>>(jsonData, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            })!;

            // Initialize the game world using the game data
            var tasks = new List<Task<BeginOrganizationExecutionResponse>>();
            foreach (var organization in organizations)
            {
                
                var roomGrain = _client.GetGrain<IOrganizationGrain>(organization.Id);
                var task = roomGrain.BeginTaskGroupExecution(new BeginOrganizationExecutionRequest
                {
                    Organization = organization
                });
                tasks.Add(task.AsTask());
            }

            await Task.WhenAll(tasks);
            foreach (var task in tasks)
            {
                _logger.LogInformation($"Response received: {task.Result.Result}.");
            }
            _logger.LogInformation($"Completed Organization Level execution.");
        }
    }
}
