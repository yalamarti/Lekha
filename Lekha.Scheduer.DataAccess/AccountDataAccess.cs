using Lekha.Models;
using Lekha.Scheduer.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Lekha.Scheduer.DataAccess
{
    public class AccountDataAccess : IAccountDataAccess
    {
        public Task<PaginationResult<Account>> GetAccountsAsync(PaginationRequest request, CancellationToken cancellationToken)
        {
            var accounts = new List<Account>();
            for (int i = 0; i < 10; i++)
            {
                accounts.Add(new Account
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = i.ToString()
                });
            };
            return Task.FromResult(new PaginationResult<Account> { HasMoreItems = false, ContinuationToken = null, Items = accounts });
        }
    }
}
