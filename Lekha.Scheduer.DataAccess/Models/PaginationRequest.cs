using System;

namespace Lekha.Scheduer.DataAccess.Models
{
    public class PaginationRequest
    {
        /// <summary>
        /// Suggested page size - returned list could have less than or equal items
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Effective date of the items - say items 'updated' on or before this date/time
        /// </summary>
        public DateTimeOffset? EffectiveDateTime { get; set; }

        /// <summary>
        /// Token used for querying next 'page' - for first pagination request of a query, this would be null
        /// </summary>
        public string ContinuationToken { get; set; }
    }
}
