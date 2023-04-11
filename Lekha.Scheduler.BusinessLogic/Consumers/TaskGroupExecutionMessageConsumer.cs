using Lekha.Scheduler.BusinessLogic.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;
using Polly.Registry;
using System.Threading.Tasks;

namespace Lekha.Scheduler.BusinessLogic.Consumers
{
    public class TaskGroupExecutionMessageConsumer :
        IConsumer<TaskGroupExecutionMessage>
    {
        private readonly IReadOnlyPolicyRegistry<string> readOnlyPolicyRegistry;
        readonly ILogger<TaskGroupExecutionMessageConsumer> logger;

        public TaskGroupExecutionMessageConsumer(IReadOnlyPolicyRegistry<string> readOnlyPolicyRegistry, ILogger<TaskGroupExecutionMessageConsumer> logger)
        {
            this.readOnlyPolicyRegistry = readOnlyPolicyRegistry;
            this.logger = logger;
        }

        public Task Consume(ConsumeContext<TaskGroupExecutionMessage> context)
        {
            logger.LogInformation("TaskGroupExecutionMessageConsumer received : {@taskGroupExecution}", context.Message);

            //
            // Is TaskGroup current execution status 'InProgress'
            //    Yes
            //        Is current execution still fits into the Schedule?
            //             Continue Tasklet execution
            //             Return
            //        Else Is misfire policy to executenow? 
            //             Continue Tasklet execution
            //             Return
            //        Else
            //             Mark as incomplete execution
            // Is current execution still fits into the Schedule ?
            //    Start Tasklet execution
            // Else
            //    Get next scheduled run and schedule it
            //
            return Task.FromResult(true);
        }
    }
}
