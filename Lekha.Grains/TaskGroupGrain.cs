namespace Lekha.Grains
{
    using GrainInterfaces;
    using MassTransit;
    using Microsoft.Extensions.Logging;

    public class TaskGroupGrain : Grain, ITaskGroupGrain
    {
        private readonly IGrainFactory _grainFactory;
        private readonly IBus _bus;
        private readonly ILogger _logger;

        public TaskGroupGrain(IGrainFactory grainFactory, IBus bus, ILogger<TaskGroupGrain> logger)
        {
            _grainFactory = grainFactory;
            this._bus = bus;
            _logger = logger;
        }

        async ValueTask<BeginTaskGroupExecutionResponse> ITaskGroupGrain.BeginExecution(BeginTaskGroupExecutionRequest beginTaskGroupExecutionRequest)
        {
            await Task.Delay(200);

            //await _bus.Publish("This is a text string");
            await _bus.Publish(new TestMessage 
            { 
                id = beginTaskGroupExecutionRequest.TaskGroupDefinition.Id, 
                name = beginTaskGroupExecutionRequest.TaskGroupDefinition.Name 
            });

            return await ValueTask.FromResult(new BeginTaskGroupExecutionResponse
            {
                Result = $"""
            '=> Execution Completed
                  Organization: {beginTaskGroupExecutionRequest.Organization.Id}/{beginTaskGroupExecutionRequest.Organization.Name}  
                  Account: {beginTaskGroupExecutionRequest.Account.Id}/{beginTaskGroupExecutionRequest.Account.Name}
                  TaskGroup: {beginTaskGroupExecutionRequest.TaskGroupDefinition.Id}/{beginTaskGroupExecutionRequest.TaskGroupDefinition.Name}
                  Grain Id is:  {this.IdentityString}/{this.RuntimeIdentity}
            """
            });
        }
    }
}