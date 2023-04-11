using System.Collections.Generic;

namespace Lekha.Scheduer.DataAccess.Models
{
    public class PaginationResult<T>
    {
        /// <summary>
        /// Indicates there are more items - query again to get the next page of results
        /// </summary>
        public bool HasMoreItems { get; set; }

        /// <summary>
        /// Provides the token necessary for querying the next 'page', in case there are more itmes as indicated by 'HasMoreItems' property
        /// Refer to https://docs.microsoft.com/en-us/azure/cosmos-db/sql/sql-query-pagination
        /// Refer to https://github.com/Azure/azure-cosmos-dotnet-v3/blob/master/Microsoft.Azure.Cosmos.Samples/Usage/Queries/Program.cs#L230
        /// </summary>
        public string ContinuationToken { get; set; }

        /// <summary>
        /// Page of actual items retrieved
        /// </summary>
        public IList<T> Items { get; set; }
    }
}
