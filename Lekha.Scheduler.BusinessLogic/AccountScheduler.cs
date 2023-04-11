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
    public class AccountScheduler
    {
        private readonly IScheduleDomain scheduleDomain;
        private readonly IEventManager eventManager;
        private readonly IConfiguration configuration;
        private readonly IReadOnlyPolicyRegistry<string> readOnlyPolicyRegistry;
        private readonly ILogger<AccountScheduler> logger;

        public AccountScheduler(IScheduleDomain scheduleDomain,
                         IEventManager eventManager,
                         IConfiguration configuration,
                         IReadOnlyPolicyRegistry<string> readOnlyPolicyRegistry,
                         ILogger<AccountScheduler> logger)
        {
            this.scheduleDomain = scheduleDomain;
            this.eventManager = eventManager;
            this.configuration = configuration;
            this.readOnlyPolicyRegistry = readOnlyPolicyRegistry;
            this.logger = logger;
        }

        private async Task<PaginationResult<Account>> GetPaginatedAccounts(PaginationRequest paginationRequest, CancellationToken cancellationToken)
        {
            Context context = new Context(nameof(AccountScheduler))
                .WithLogger(logger);
            var policyResult = await readOnlyPolicyRegistry.Get<IAsyncPolicy>(PollyPolicies.DataRetrievalRetryPolicy)
                    .ExecuteAndCaptureAsync((context, cancellationToken) =>
                    {
                        return scheduleDomain.GetAccountsAsync(paginationRequest, cancellationToken);
                    }, context, cancellationToken);
            if (policyResult.Outcome != OutcomeType.Successful)
            {
                if (policyResult.FinalException != null)
                {
                    throw new Exception("Failed to retrieve accounts", policyResult.FinalException);
                }
                else
                {
                    throw new Exception("Failed to retrieve accounts - unknown error");
                }
            }

            return policyResult.Result;
        }
        
        /// <summary>
        /// Start triggering scheduling of TaskGroups for all accounts
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task Start(CancellationToken cancellationToken)
        {
            IList<Account> accounts = null;

            //
            // Process page of Accounts at a time
            //
            Context context = new Context(nameof(AccountScheduler))
                .WithLogger(logger);

            bool hasMoreItems = true;
            var paginationRequest = new PaginationRequest
            {
                PageSize = configuration.GetValue<int>(ConfigurationNames.AccountRetrievalPageSize, 100)
            };
            int? count = 0;
            while (hasMoreItems)
            {
                //
                // Retry querying for accounts if needed
                //
                logger.LogInformation("Retrieving next page of accounts");

                var paginationResult = await GetPaginatedAccounts(paginationRequest, cancellationToken);
                accounts = paginationResult.Items;

                logger.LogInformation($"Retrieved next page of {accounts?.Count} accounts.");

                hasMoreItems = paginationResult.HasMoreItems;
                paginationRequest.ContinuationToken = paginationResult.ContinuationToken;

                //var policyResult = await readOnlyPolicyRegistry.Get<IAsyncPolicy>(PollyPolicies.DataRetrievalRetryPolicy)
                //    .ExecuteAndCaptureAsync((context, cancellationToken) =>
                //    {
                //        return scheduleDomain.GetAccountsAsync(paginationRequest, cancellationToken);
                //    }, context, cancellationToken);
                //if (policyResult.Outcome == OutcomeType.Successful)
                //{
                //    accounts = policyResult.Result.Items;
                //    logger.LogInformation($"Retrieved next page of {accounts?.Count} accounts.");
                //}
                //else
                //{
                //    if (policyResult.FinalException != null)
                //    {
                //        throw new Exception("Failed to retrieve accounts", policyResult.FinalException);
                //    }
                //    else
                //    {
                //        throw new Exception("Failed to retrieve accounts - unknown error");
                //    }
                //}
                //hasMoreItems = policyResult.Result.HasMoreItems;
                //paginationRequest.ContinuationToken = policyResult.Result.ContinuationToken;

                //
                // Begin triggering of scheduling of the Task Groups related to the retrieved Account
                //
                var taskList = new List<Task>();
                foreach (var account in accounts)
                {
                    var task = Start(account.Id, account.Name, cancellationToken);
                    taskList.Add(task);
                }
                logger.LogInformation($"Begun waiting for {taskList.Count} triggering of scheduling accounts.  Total accounts processed so far: {count}");

                try
                {
                    await Task.WhenAll(taskList);
                    count += accounts?.Count;
                    logger.LogInformation($"End waiting for {taskList.Count} triggering of scheduling accounts.  Total accounts processed so far: {count}");
                }
                catch (Exception ex)
                {
                    var exceptions = taskList.Where(t => t.Exception != null)
                                          .Select(t => t.Exception);
                    throw new Exception($"Attempted {taskList.Count} - but failed to start triggering of scheduling for {exceptions.Count()} accounts", ex);
                }
            }
        }

        /// <summary>
        /// Start triggering of scheduling of TaskGroups for specified account
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="accountName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task Start(string accountId, string accountName, CancellationToken cancellationToken)
        {
            // Trace begin operation
            var data = new AccountMessage
            {
                AccountId = accountId,
                AccountName = accountName
            };

            logger.LogInformation("Starting triggering of scheduling for {@account}", data);

            Context context = new Context(nameof(AccountScheduler))
                .WithLogger(logger)
                .WithData(PollyContextKeys.AccountDataName, data);

            var policy = await readOnlyPolicyRegistry.Get<IAsyncPolicy>(PollyPolicies.PublishMessageRetryPolicy)
                .ExecuteAndCaptureAsync<bool>((context, cancellationToken) =>
                  {
                      return eventManager.Publish(EventSources.ScheduleAccount, data, cancellationToken);
                  }, context, cancellationToken);

            if (policy.Outcome == OutcomeType.Successful)
            {
                logger.LogInformation("Started triggering of scheduling for {@account}", data);
            }
            else
            {
                logger.LogError("Error starting triggering of scheduling for {@account}", data);
                if (policy.FinalException != null)
                {
                    throw new Exception($"Failed to start triggering of scheduling for {accountId}/{accountName}", policy.FinalException);
                }
                else
                {
                    throw new Exception($"Failed to start triggering of scheduling - unknown error - for {accountId}/{accountName}");
                }
            }
        }
    }
}
