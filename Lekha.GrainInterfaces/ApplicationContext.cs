using System;
using System.Collections.Generic;

namespace Lekha.GrainInterfaces
{

    /// <summary>
    /// Container for managing Accounts
    /// Billing etc at this level
    /// </summary>
    [GenerateSerializer]
    public class Organization
    {
        /// <summary>
        /// Id
        /// System generated
        /// Used in API references
        /// </summary>
        [Id(0)]
        public string Id { get; set; }
        /// <summary>
        /// Name
        /// User provided - cannot be changed - Friendly name for Id
        /// </summary>
        [Id(1)]
        public string Name { get; set; }
    }

    /// <summary>
    /// Container for managing users and workspaces
    /// Has one primary owner, and multiple users associated 
    /// </summary>
    [GenerateSerializer]
    public class Account
    {
        /// <summary>
        /// Id
        /// System generated
        /// Used in API references
        /// </summary>
        [Id(0)] 
        public string Id { get; set; }
        [Id(1)] 
        public string OrganizationId { get; set; }
        /// <summary>
        /// Name
        /// User provided - cannot be changed - Friendly name for Id
        /// </summary>
        [Id(3)]
        public string Name { get; set; }
        [Id(4)]
        public string Title { get; set; }
    }

    /// <summary>
    /// Container for managing Task definitions, Task Schedule definitions, task outcomes and other
    ///   task configurations
    /// </summary>
    [GenerateSerializer]
    public class TaskGroupDefinition
    {
        /// <summary>
        /// Id
        /// System generated
        /// Used in API references
        /// </summary>
        [Id(0)]
        public string Id { get; set; }
        [Id(1)]
        public string Organizationid { get; set; }
        [Id(2)]
        public string AccountId { get; set; }
        /// <summary>
        /// Name
        /// User provided - cannot be changed - Friendly name for Id
        /// </summary>
        [Id(3)]
        public string Name { get; set; }
        [Id(4)]
        public string Description { get; set; }
    }



    [GenerateSerializer]
    public class DateTimeRange
    {
        public DateTimeOffset BeginTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
    }

}
