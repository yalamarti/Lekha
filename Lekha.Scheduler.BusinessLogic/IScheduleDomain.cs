using Lekha.Models;
using Lekha.Scheduer.DataAccess.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Lekha.Scheduler.BusinessLogic
{
    public interface IScheduleDomain
    {
        Task<PaginationResult<Account>> GetAccountsAsync(PaginationRequest paginationRequest, CancellationToken cancellationToken);
        Task<PaginationResult<TaskGroupDefinition>> GetTaskGroupDefinitionsAsync(string accountId, PaginationRequest paginationRequest, CancellationToken cancellationToken);
        Task<PaginationResult<TaskletList>> GetTaskLetListsAsync(string accountId, string taskGropDefinitionId, PaginationRequest paginationRequest, CancellationToken cancellationToken);
        Task<TaskGroupExecutionSchedule> GetExecutioScheduleAsync(string accountId, string taskGropDefinitionId, CancellationToken cancellationToken);
    }
}
