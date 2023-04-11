using System;
using System.Collections.Generic;

namespace Lekha.Models
{
    public class ApplicationContext
    {
        public string AppName { get; set; }
        public string Service { get; set; }
        public string ManagedIdentityClientId { get; set; }
    }

    public class Claim
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
    public class User
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public List<Claim> Claims { get; set; }
    }


    //
    // Cosmos DB References:
    //   - Cosmose DB Factory pattern: https://medium.com/swlh/best-design-pattern-for-azure-cosmos-db-containers-factory-pattern-addff5628f8a
    //   - Cosmos DB Best Practices: https://docs.microsoft.com/en-us/azure/cosmos-db/sql/best-practice-dotnet
    //   - Cosmos DB - Modeling data: https://docs.microsoft.com/en-us/azure/cosmos-db/sql/modeling-data

    /// <summary>
    /// Container for managing Accounts
    /// Billing etc at this level
    /// </summary>
    public class Organization
    {
        /// <summary>
        /// Id
        /// System generated
        /// Used in API references
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Name
        /// User provided - cannot be changed - Friendly name for Id
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// List of all users in an organization
        /// </summary>
        public List<User> Users { get; set; }
        public List<Account> Accounts { get; set; }
    }

    /// <summary>
    /// Container for managing Task definitions, Task Schedule definitions, task outcomes and other
    ///   task configurations
    /// </summary>
    public class TaskGroupDefinition
    {
        /// <summary>
        /// Id
        /// System generated
        /// Used in API references
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Name
        /// User provided - cannot be changed - Friendly name for Id
        /// </summary>
        public string Name { get; set; }
        public string Description { get; set; }
        public TaskletDefinition TaskletDefinition { get; set; }
        public TaskExecutionStrategy ExecutionStrategy { get; set; }
        public TaskGroupExecutionSchedule ExecutionSchedule { get; set; }
    }


    /// <summary>
    /// Container for managing users and workspaces
    /// Has one primary owner, and multiple users associated 
    /// </summary>
    public class Account
    {
        /// <summary>
        /// Id
        /// System generated
        /// Used in API references
        /// </summary>
        public string Id { get; set; }
        public string OrganizationId { get; set; }
        /// <summary>
        /// Name
        /// User provided - cannot be changed - Friendly name for Id
        /// </summary>
        public string Name { get; set; }
        public string Title { get; set; }
        /// <summary>
        /// Users in an organization that have access to this account
        /// </summary>
        public List<User> Users { get; set; }
        public List<TaskGroupDefinition> TaskGroups { get; set; }
    }

    public class TaskGroupExecutionSchedule
    {
        public DateTimeOffset Begin { get; set; }
        public DateTimeOffset? End { get; set; }
        public List<DateTimeOffset> ExcludeDays { get; set; }
        public string CrontabExpression { get; set; }
        /// <summary>
        /// How long to continue a single scheduled execution run.
        /// null indicates 'until end of execution of all tasklets in a group or until stopped/aborted'.
        /// 
        /// </summary>
        public int? RunTimeToLiveInSeconds { get; set; }
    }

    /// <summary>
    /// Task definition
    /// </summary>
    public class TaskletDefinition
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public IEnumerable<FieldDefinition> PayloadFieldDefinitions { get; set; }

        /// <summary>
        /// Tasklet Payload fields that need to be used during task execution.
        /// E.g., Use field named 'Phone1' as part of this execution strategy
        /// </summary>
        public IEnumerable<string> TaskletPayloadKeyFieldNames { get; set; }
    }

    /// <summary>
    /// Field limit constants
    /// </summary>
    public struct FieldLimits
    {
        public const int MaximumLength = 2048;
    }

    /// <summary>
    /// Represents the configuration of a field within Tasklet payload
    /// </summary>
    public class FieldDefinition
    {
        /// <summary>
        /// Name of the field.  
        /// Optional. 
        /// When not specified, implies name from HeaderRecords or a system generated name.
        /// Case insensitive: (meaning case of the letters in the name doesn't matter when comparing with, 
        /// say, comparing the field name to a corresponding field header in the CSV data)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A displayable title for the field.  Optional. Default: Same as Name value.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Data type of the field.
        /// Valid values: number, unsigned-number, decimal, date, datetime, time, string.
        /// Optional.
        /// Default: string
        ///   https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/floating-point-numeric-types: 
        ///      Number:   long      System.Int64    Size: Signed 64-bit integer
        ///         Range: -9,223,372,036,854,775,808 to 9,223,372,036,854,775,807
        ///         
        ///      Unsigned-Number:   ulong      System.UInt64    Size: Unsigned Signed 64-bit integer
        ///         Range: 0 to 18,446,744,073,709,551,615
        ///         
        ///      Decimal : decimal 	System.Decimal 	Size: 16 bytes 	
        ///         Precision: - 28-29 decimal places 	(28-29: includes significant digits and decimal places)
        ///         Range:     +-1.0 x 10 power 28 to +-7.9 x 10 power 28
        ///         
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// Format of the date, datetime or time field value.  
        /// Optional.
        /// Default: For date - 'yyyy/MM/dd'.  
        /// Default: For time: 'HH\:mm'.  For datetime: 'yyyy/MM/dd HH:mm' 
        /// </summary>
        public string DateTimeFormat { get; set; }

        /// <summary>
        /// Indicates if the field value can be empty or blank/spaces
        /// Optional.
        /// Default: false
        /// </summary>
        public bool AllowEmptyField { get; set; }

        /// <summary>
        /// Indicates if the field value has to be specified as part of the record.
        /// Optional.
        /// Applies when a header record is specified.
        /// When value is 'true', in case the field value is missing in a record, the record will be marked as 'in error'
        /// Default: false
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// Maximum allowed length of the field
        /// Default: Refer to FieldLimits.MaximumLength
        /// </summary>
        public int AllowedMaximumLength { get; set; } = FieldLimits.MaximumLength;

        /// <summary>
        /// Indicates if the value of the field can be part of exception/logging reporting
        /// Default: falsel
        /// </summary>
        public bool ExposeableToPublic { get; set; }
    }

    public class DateTimeRange
    {
        public DateTimeOffset BeginTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
    }

    public class TaskExecutionStrategy
    {
        public string Id { get; set; }
        public string Title { get; set; }

        /// <summary>
        /// Time window when the execution should take place
        /// </summary>
        public DateTimeRange ExecutionTimeWindow { get; set; }

        public ExecutionOutcomeProfile OutcomeProfile { get; set; }
    }

    public class ExecutionOutcomeDefinition
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public OutcomeAction Action { get; set; }
        /// <summary>
        /// Number of time to retry executing the action.
        /// Applicable only when Action is OutocmeAction.Retry
        /// </summary>
        public int? RetryCount { get; set; }
    }

    public enum OutcomeAction
    {
        Complete,
        Retry
    }

    public class ExecutionOutcomeProfile
    {
        public string Id { get; set; }
        public string Title { get; set; }

        public IEnumerable<ExecutionOutcomeDefinition> OutcomeDefinitions { get; set; }
    }


    public class Tasklet
    {
        public string Id { get; set; }
        public string TaskGroupId { get; set; }
        public string Payload { get; set; }
    }

    public class TaskletList
    {
        public string Id { get; set; }
        public string TaskGroupDefinitionId { get; set; }
    }
}
