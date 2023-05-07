namespace Lekha.Grains
{
    using GrainInterfaces;
    using Microsoft.Extensions.Logging;
    using System.Reflection;
    using System.Text.Json;

    public class AccountGrain : Grain, IAccountGrain
    {
        private readonly IGrainFactory _grainFactory;
        private readonly ILogger _logger;

        public AccountGrain(IGrainFactory grainFactory, ILogger<AccountGrain> logger)
        {
            _grainFactory = grainFactory;
            _logger = logger;
        }
        async ValueTask<BeginAccountExecutionResponse> IAccountGrain.BeginTaskGroupExecution(BeginAccountExecutionRequest beginAccountExecutionRequest)
        {

            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            var dataFileName = Path.Combine(path, "SampleData/TaskGroup.json");

            // Read the contents of the game file and deserialize it
            var jsonData = await File.ReadAllTextAsync(dataFileName);
            var taskGroupDefinitions = JsonSerializer.Deserialize<List<TaskGroupDefinition>>(jsonData, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            })!;

            var tasks = new List<Task<BeginTaskGroupExecutionResponse>>();
            foreach (var taskGroupDefinition in taskGroupDefinitions.Where(i => i.AccountId == beginAccountExecutionRequest.Account.Id))
            {
                var grain = _grainFactory.GetGrain<ITaskGroupGrain>(taskGroupDefinition.Id);
                var task = grain.BeginExecution(new BeginTaskGroupExecutionRequest
                {
                    Organization = beginAccountExecutionRequest.Organization,
                    Account = beginAccountExecutionRequest.Account,
                    TaskGroupDefinition = taskGroupDefinition,
                });
                tasks.Add(task.AsTask());
            }

            await Task.WhenAll(tasks);

            foreach (var task in tasks)
            {
                _logger.LogInformation($"Response received from TaskGroup Grain: {task.Result.Result}.");
            }

            await Task.Delay(200);

            return await ValueTask.FromResult(new BeginAccountExecutionResponse
            {
                Result = $"""
            '=> Execution Completed
                  Organization: {beginAccountExecutionRequest.Organization.Id}/{beginAccountExecutionRequest.Organization.Name}!  
                  Account: {beginAccountExecutionRequest.Account.Id}/{beginAccountExecutionRequest.Account.Name}!  
                  Grain Id is:  {this.IdentityString}/{this.RuntimeIdentity}
            """
            });
        }
    }
}