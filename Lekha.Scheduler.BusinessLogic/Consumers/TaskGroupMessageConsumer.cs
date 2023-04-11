using Lekha.Scheduler.BusinessLogic.Messages;
using Lekha.Scheduler.BusinessLogic.Models;
using Lekha.Scheduler.Extensions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Registry;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lekha.Scheduler.BusinessLogic.Consumers
{
    public class TaskGroupMessageConsumer :
        IConsumer<TaskGroupMessage>
    {
        private readonly IEventManager eventManager;
        private readonly IReadOnlyPolicyRegistry<string> readOnlyPolicyRegistry;
        readonly ILogger<TaskGroupMessageConsumer> logger;

        public TaskGroupMessageConsumer(IEventManager eventManager, IReadOnlyPolicyRegistry<string> readOnlyPolicyRegistry, ILogger<TaskGroupMessageConsumer> logger)
        {
            this.eventManager = eventManager;
            this.readOnlyPolicyRegistry = readOnlyPolicyRegistry;
            this.logger = logger;
        }

        public async Task Consume(ConsumeContext<TaskGroupMessage> context1)
        {
            logger.LogInformation("TaskGroupConsumer received : {@taskGroup}", context1.Message);

            // Get schedule info
            var nextRun = DateTime.UtcNow + TimeSpan.FromSeconds(30);

            var data = new TaskGroupExecutionMessage
            {
                AccountId = context1.Message.AccountId,
                AccountName = context1.Message.AccountName,
                TaskGroupId = context1.Message.TaskGroupId,
                TaskGroupName = context1.Message.TaskGroupName
            };
            CancellationTokenSource cts = new CancellationTokenSource();

            Context pollyContext = new Context(nameof(TaskGroupMessageConsumer))
                .WithLogger(logger)
                .WithData(PollyContextKeys.TaskGroupDataName, data);

            var policyResult = await readOnlyPolicyRegistry.Get<IAsyncPolicy>(PollyPolicies.PublishMessageRetryPolicy)
                    .ExecuteAndCaptureAsync((pollyContext1, cancellationToken) =>
                    {
                        return context1.SchedulePublish(nextRun, data, cancellationToken);

                        //return eventManager.SchedulePublish<TaskGroupExecutionMessage>(nextRun, data, cancellationToken);
                    }, pollyContext, cts.Token);
            if (policyResult.Outcome == OutcomeType.Successful)
            {
                logger.LogInformation("Scheduled taskgroup execution for {scheduedExecutionTime}UTC for {@taskGroupExecution}", nextRun, data);
            }
            else
            {
                if (policyResult.FinalException == null)
                {
                    throw new Exception($"Failed to scheduled taskgroup execution for {nextRun}UTC for {data.AccountId}/{data.AccountName}/{data.TaskGroupId}/{data.TaskGroupName}");
                }
                else
                {
                    throw new Exception($"Failed to scheduled taskgroup execution for {nextRun}UTC for {data.AccountId}/{data.AccountName}/{data.TaskGroupId}/{data.TaskGroupName}", policyResult.FinalException);
                }
            }
        }
    }
}
