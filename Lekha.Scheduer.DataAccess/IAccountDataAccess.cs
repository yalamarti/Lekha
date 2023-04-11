using Lekha.Models;
using Lekha.Scheduer.DataAccess.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Lekha.Scheduer.DataAccess
{
    public interface IAccountDataAccess
    {
        Task<PaginationResult<Account>> GetAccountsAsync(PaginationRequest request, CancellationToken cancellationToken);
    }
}
