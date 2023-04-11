namespace Lekha.Scheduler.BusinessLogic.Messages
{
    public class TaskGroupExecutionMessage
    {
        public string AccountId { get; set; }
        public string AccountName { get; set; }
        public string TaskGroupId { get; set; }
        public string TaskGroupName { get; set; }
    }
}
