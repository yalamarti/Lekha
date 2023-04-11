using Lekha.Models;
using Lekha.Scheduer.DataAccess;
using Lekha.Scheduer.DataAccess.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lekha.Scheduler.BusinessLogic
{
    public class ScheduleDomain : IScheduleDomain
    {
        private readonly IOrganizationDataAccess organizationDataAccess;
        private readonly IAccountDataAccess accountDataAccess;
        private readonly ITaskGroupDefinitionDataAccess taskGroupDefinitionDataAccess;
        private readonly ITaskletDataAccess taskletDataAccess;

        public ScheduleDomain(IOrganizationDataAccess organizationDataAccess,
                              IAccountDataAccess accountDataAccess,
                              ITaskGroupDefinitionDataAccess taskGroupDefinitionDataAccess,
                              ITaskletDataAccess taskletDataAccess)
        {
            this.organizationDataAccess = organizationDataAccess ?? throw new ArgumentNullException(nameof(organizationDataAccess));
            this.accountDataAccess = accountDataAccess;
            this.taskGroupDefinitionDataAccess = taskGroupDefinitionDataAccess ?? throw new ArgumentNullException(nameof(taskGroupDefinitionDataAccess));
            this.taskletDataAccess = taskletDataAccess;
        }

        public Task<PaginationResult<Account>> GetAccountsAsync(PaginationRequest paginationRequest, CancellationToken cancellationToken)
        {
            return accountDataAccess.GetAccountsAsync(paginationRequest, cancellationToken);
        }
        public Task<PaginationResult<TaskGroupDefinition>> GetTaskGroupDefinitionsAsync(string accountId, PaginationRequest paginationRequest, CancellationToken cancellationToken)
        {
            return taskGroupDefinitionDataAccess.GetTaskGroupDefinitionsAsync(accountId, paginationRequest, cancellationToken);
        }

        public Task<PaginationResult<TaskletList>> GetTaskLetListsAsync(string accountId, string taskGropDefinitionId, PaginationRequest paginationRequest, CancellationToken cancellationToken)
        {
            return taskletDataAccess.GetTaskLetListsAsync(accountId, taskGropDefinitionId, paginationRequest, cancellationToken);
        }
        public Task<TaskGroupExecutionSchedule> GetExecutioScheduleAsync(string accountId, string taskGropDefinitionId, CancellationToken cancellationToken)
        {
            return taskGroupDefinitionDataAccess.GetExecutioScheduleAsync(accountId, taskGropDefinitionId, cancellationToken);
        }
    }

}
