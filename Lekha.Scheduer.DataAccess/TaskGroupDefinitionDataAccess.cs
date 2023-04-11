using Lekha.Models;
using Lekha.Scheduer.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Lekha.Scheduer.DataAccess
{
    public class TaskGroupDefinitionDataAccess : ITaskGroupDefinitionDataAccess
    {
        public Task<PaginationResult<TaskGroupDefinition>> GetTaskGroupDefinitionsAsync(string accountId, PaginationRequest request, CancellationToken cancellationToken)
        {
            List<TaskGroupDefinition> items = new List<TaskGroupDefinition>();
            for (int i = 0; i < 10; i++)
            {
                items.Add(new TaskGroupDefinition
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = i.ToString(),

                });
            };

            return Task.FromResult(new PaginationResult<TaskGroupDefinition> { HasMoreItems = false, ContinuationToken = null, Items = items });
        }
        public Task<TaskGroupExecutionSchedule> GetExecutioScheduleAsync(string accountId, string taskGropDefinitionId, CancellationToken cancellationToken)
        {
            return Task.FromResult(new TaskGroupExecutionSchedule
            {
                Begin = DateTimeOffset.UtcNow.Date,
                End = DateTimeOffset.UtcNow.Date.AddDays(10),
                CrontabExpression = "DateTimeOffset.UtcNow.Date",
                ExcludeDays = null,
                RunTimeToLiveInSeconds = null
            });
        }
    }
}
