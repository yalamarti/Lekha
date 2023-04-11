using Lekha.Models;
using Lekha.Scheduer.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Lekha.Scheduer.DataAccess
{
    public class TaskletDataAccess : ITaskletDataAccess
    {
        public Task<PaginationResult<TaskletList>> GetTaskLetListsAsync(string accountId, string taskGropDefinitionId, PaginationRequest request, CancellationToken cancellationToken)
        {
            List<TaskletList> items = new List<TaskletList>();
            for (int i = 0; i < 10; i++)
            {
                items.Add(new TaskletList
                {
                    Id = Guid.NewGuid().ToString(),
                    TaskGroupDefinitionId = taskGropDefinitionId,

                });
            };

            return Task.FromResult(new PaginationResult<TaskletList> { HasMoreItems = false, ContinuationToken = null, Items = items });
        }
    }
}
