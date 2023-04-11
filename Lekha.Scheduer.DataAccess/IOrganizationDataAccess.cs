using Lekha.Models;
using Lekha.Scheduer.DataAccess.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Lekha.Scheduer.DataAccess
{
    public interface IOrganizationDataAccess
    {
        Task<PaginationResult<Organization>> GetOrganizationsAsync(PaginationRequest request, CancellationToken cancellationToken);
    }
}
