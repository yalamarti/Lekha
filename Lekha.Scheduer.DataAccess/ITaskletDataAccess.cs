using Lekha.Models;
using Lekha.Scheduer.DataAccess.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Lekha.Scheduer.DataAccess
{
    public interface ITaskletDataAccess
    {
        Task<PaginationResult<TaskletList>> GetTaskLetListsAsync(string accountId, string taskGropDefinitionId, PaginationRequest request, CancellationToken cancellationToken);
    }
}
