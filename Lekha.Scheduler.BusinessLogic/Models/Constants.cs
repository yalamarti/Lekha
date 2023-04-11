namespace Lekha.Scheduler.BusinessLogic.Models
{
    public class ConfigurationNames
    {
        public const string PublishRetryRetryCount = "PublishRetryRetryCount";
        public const string DataRetrievalRetryCount = "DataRetrievalRetryCount";
        public const string AccountRetrievalPageSize = "AccountRetrievalPageSize";
        public const string TaskGroupRetrievalPageSize = "TaskGroupRetrievalPageSize";

        public const string DataRetrievalDefaultBackoff = "DataRetrievalDefaultBackoff";
        public const string DataRetrievalBackoffMin = "DataRetrievalBackoffMin";
        public const string DataRetrievalBackoffMax = "DataRetrievalBackoffMax";

        public const string PublishDefaultBackoff = "PublishDefaultBackoff";
        public const string PublishBackoffMin = "PublishBackoffMin";
        public const string PublishBackoffMax = "PublishBackoffMax";
    }

    public class PollyPolicies
    {
        public const string DataRetrievalRetryPolicy = "DataRetrievalRetryPolicy";
        public const string PublishMessageRetryPolicy = "PublishMessageRetryPolicy";
    }
    public class EventSources
    {
        public const string ScheduleAccount = "ScheduleAccount";
        public const string ScheduleTaskGroup = "ScheduleTaskGroup";
    }
    public class PollyContextKeys
    {
        public const string AccountDataName = "account";
        public const string TaskGroupDataName = "taskGroup";
    }
}
