using Lekha.Models;
using Lekha.Scheduer.DataAccess.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Lekha.Scheduler.BusinessLogic
{
    public interface IScheduleManager
    {
        Task<PaginationResult<Account>> GetAccountsAsync(PaginationRequest paginationRequest, CancellationToken cancellationToken);
        Task<PaginationResult<TaskGroupDefinition>> GetTaskGroupsAsync(string accountId, PaginationRequest paginationRequest, CancellationToken cancellationToken);
    }

}
