using Lekha.Models;
using Lekha.Scheduer.DataAccess.Models;
using Lekha.Scheduler.BusinessLogic.Messages;
using Lekha.Scheduler.BusinessLogic.Models;
using Lekha.Scheduler.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Registry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Lekha.Scheduler.BusinessLogic
{
    /// <summary>
    /// Provides functions for triggering scheduling of TaskGroups for all accounts in the system
    /// </summary>
    public class TaskGroupScheduler
    {
        private readonly IScheduleDomain scheduleDomain;
        private readonly IEventManager eventManager;
        private readonly IConfiguration configuration;
        private readonly IReadOnlyPolicyRegistry<string> readOnlyPolicyRegistry;
        private readonly ILogger<TaskGroupScheduler> logger;

        public TaskGroupScheduler(IScheduleDomain scheduleDomain,
                         IEventManager eventManager,
                         IConfiguration configuration,
                         IReadOnlyPolicyRegistry<string> readOnlyPolicyRegistry,
                         ILogger<TaskGroupScheduler> logger)
        {
            this.scheduleDomain = scheduleDomain;
            this.eventManager = eventManager;
            this.configuration = configuration;
            this.readOnlyPolicyRegistry = readOnlyPolicyRegistry;
            this.logger = logger;
        }

        /// <summary>
        /// Start triggering scheduling of TaskGroups for all accounts
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task Start(string accountId, string accountName, CancellationToken cancellationToken)
        {
            var data = new AccountMessage
            {
                AccountId = accountId,
                AccountName = accountName
            };

            IList<TaskGroupDefinition> taskGroups = null;

            //
            // Process page of TaskGroups at a time
            //
            Context context = new Context(nameof(TaskGroupScheduler))
                .WithLogger(logger)
                .WithData(PollyContextKeys.AccountDataName, data);

            bool hasMoreItems = true;
            var paginationRequest = new PaginationRequest
            {
                PageSize = configuration.GetValue<int>(ConfigurationNames.TaskGroupRetrievalPageSize, 100)
            };
            int? count = 0;
            while (hasMoreItems)
            {
                //
                // Retry querying for accounts if needed
                //
                logger.LogInformation("Retrieving next page of task groups for {@account} account", data);

                var policyResult = await readOnlyPolicyRegistry.Get<IAsyncPolicy>(PollyPolicies.DataRetrievalRetryPolicy)
                    .ExecuteAndCaptureAsync((context, cancellationToken) =>
                    {
                        return scheduleDomain.GetTaskGroupDefinitionsAsync(accountId, paginationRequest, cancellationToken);
                    }, context, cancellationToken);
                if (policyResult.Outcome == OutcomeType.Successful)
                {
                    taskGroups = policyResult.Result.Items;
                    logger.LogInformation("Retrieved next page of {iterationCount} task groups for {@account}", taskGroups?.Count, data);
                }
                else
                {
                    if (policyResult.FinalException != null)
                    {
                        throw new Exception($"Failed to retrieve task groups for account {accountId}/{accountName}", policyResult.FinalException);
                    }
                    else
                    {
                        throw new Exception($"Failed to retrieve task groups - unknown error - for account {accountId}/{accountName}");
                    }
                }
                hasMoreItems = policyResult.Result.HasMoreItems;
                paginationRequest.ContinuationToken = policyResult.Result.ContinuationToken;

                //
                // Begin triggering of scheduling task groups
                //
                var taskList = new List<Task>();
                foreach (var taskGroup in taskGroups)
                {
                    var task = Start(accountId, accountName, taskGroup.Id, taskGroup.Name, cancellationToken);
                    taskList.Add(task);
                }
                logger.LogInformation("Begun waiting for {iterationCount} triggering of scheduling task groups.  Total task groups processed for {@account} so far: {cumulativecount}", taskList.Count, data, count);

                try
                {
                    await Task.WhenAll(taskList);
                    count += taskGroups?.Count;
                    logger.LogInformation("End waiting for {iterationCount} triggering of scheduling task groups.  Total task groups processed for {@account} so far: {cumulativecount}", taskList.Count, data, count);
                }
                catch (Exception ex)
                {
                    var exceptions = taskList.Where(t => t.Exception != null)
                                          .Select(t => t.Exception);
                    throw new Exception($"Attempted {taskList.Count} - but failed to start triggering of scheduling for {exceptions.Count()} task groups", ex);
                }
            }
        }

        /// <summary>
        /// Start triggering scheduling of TaskGroups for specified account
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="accountName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task Start(string accountId, string accountName, string taskGroupId, string taskGroupName, CancellationToken cancellationToken)
        {
            // Trace begin operation
            var data = new TaskGroupMessage
            {
                AccountId = accountId,
                AccountName = accountName,
                TaskGroupId = taskGroupId,
                TaskGroupName = taskGroupName
            };

            logger.LogInformation("Starting triggering of scheduling for {@taskGroup} taskgroup", data);

            Context context = new Context(nameof(TaskGroupScheduler))
                .WithLogger(logger)
                .WithData(PollyContextKeys.TaskGroupDataName, data);

            var policy = await readOnlyPolicyRegistry.Get<IAsyncPolicy>(PollyPolicies.PublishMessageRetryPolicy)
                .ExecuteAndCaptureAsync<bool>((context, cancellationToken) =>
                {
                    return eventManager.Publish(EventSources.ScheduleTaskGroup, data, cancellationToken);
                }, context, cancellationToken);

            if (policy.Outcome == OutcomeType.Successful)
            {
                logger.LogInformation("Started triggering of scheduling for {@taskGroup} taskgroup", data);
            }
            else
            {
                logger.LogError("Error starting triggering of scheduling for {@taskGroup} taskgroup", data);
                if (policy.FinalException != null)
                {
                    throw new Exception($"Failed to start triggering of scheduling for taskgroup {accountId}/{accountName}/{taskGroupId}/{taskGroupName}", policy.FinalException);
                }
                else
                {
                    throw new Exception($"Failed to start triggering of scheduling - unknown error - for taskgroup {accountId}/{accountName}/{taskGroupId}/{taskGroupName}");
                }
            }
        }
    }
}

