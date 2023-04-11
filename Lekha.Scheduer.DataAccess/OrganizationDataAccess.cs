using Lekha.Models;
using Lekha.Scheduer.DataAccess.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lekha.Scheduer.DataAccess
{
    public class OrganizationDataAccess : IOrganizationDataAccess
    {
        public Task<PaginationResult<Organization>> GetOrganizationsAsync(PaginationRequest request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
