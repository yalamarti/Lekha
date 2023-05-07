namespace Lekha.Grains
{
    using GrainInterfaces;
    using Microsoft.Extensions.Logging;
    using Orleans.Runtime;
    using System.Reflection;
    using System.Text.Json;

    public class OrganizationGrain : Grain, IOrganizationGrain
    {
        public int SomeValue { get; set; }

        private readonly IGrainFactory _grainFactory;
        private readonly ILogger _logger;
        private readonly IPersistentState<OrganizationState> _organization;

        public OrganizationGrain(IGrainFactory grainFactory, ILogger<OrganizationGrain> logger,
        [PersistentState("organization", "organizationStore")] IPersistentState<OrganizationState> organization)
        {
            _grainFactory = grainFactory;
            _logger = logger;
            _organization = organization;
        }

        async ValueTask<BeginOrganizationExecutionResponse> IOrganizationGrain.BeginTaskGroupExecution(BeginOrganizationExecutionRequest beginOrganizationExecutionRequest)
        {
            _logger.LogInformation(
                "BeginTaskGroupExecutionRequest received for Organization: '{Organization}'", beginOrganizationExecutionRequest.Organization.Id);

            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            var accountDataFileName = Path.Combine(path, "SampleData/Account.json");

            var jsonData = await File.ReadAllTextAsync(accountDataFileName);
            var accounts = JsonSerializer.Deserialize<List<Account>>(jsonData, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            })!;

            var tasks = new List<Task<BeginAccountExecutionResponse>>();
            foreach (var account in accounts)
            {

                var accountGrain = _grainFactory.GetGrain<IAccountGrain>($"{beginOrganizationExecutionRequest.Organization.Id}/{account.Id}");
                var task = accountGrain.BeginTaskGroupExecution(new BeginAccountExecutionRequest
                {
                    Organization = beginOrganizationExecutionRequest.Organization,
                    Account = account
                });
                tasks.Add(task.AsTask());
            }

            await Task.WhenAll(tasks);

            foreach (var task in tasks)
            {
                _logger.LogInformation($"Response received from Account Grain: {task.Result.Result}.");
                await IncrementAccountsAsync(beginOrganizationExecutionRequest.Organization.Id);
            }

            return await ValueTask.FromResult(new BeginOrganizationExecutionResponse
            {
                Result = $"""
            '=> Execution Completed
                  Organization: {beginOrganizationExecutionRequest.Organization.Id}/{beginOrganizationExecutionRequest.Organization.Name}!  
                  Grain Id is:  {this.IdentityString}/{this.RuntimeIdentity}
                  Number Of Accounts:   {_organization.State.SomeValue}
            """
            });
        }

        private async Task IncrementAccountsAsync(string id)
        {
            if (id != "1")
            {
                _organization.State.SomeValue++;
                await _organization.WriteStateAsync();
            }
        }
    }
}