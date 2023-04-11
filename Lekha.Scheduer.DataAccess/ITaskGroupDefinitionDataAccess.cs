using Lekha.Models;
using Lekha.Scheduer.DataAccess.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Lekha.Scheduer.DataAccess
{
    public interface ITaskGroupDefinitionDataAccess
    {
        Task<PaginationResult<TaskGroupDefinition>> GetTaskGroupDefinitionsAsync(string accountId, PaginationRequest request, CancellationToken cancellationToken);
        Task<TaskGroupExecutionSchedule> GetExecutioScheduleAsync(string accountId, string taskGropDefinitionId, CancellationToken cancellationToken);
    }
}
