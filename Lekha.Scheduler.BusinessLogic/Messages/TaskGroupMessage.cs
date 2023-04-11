using System;
using System.Collections.Generic;

namespace Lekha.Scheduler.BusinessLogic.Messages
{
    public class TaskGroupMessage
    {
        public string AccountId { get; set; }
        public string AccountName { get; set; }
        public string TaskGroupId { get; set; }
        public string TaskGroupName { get; set; }
        public DateTimeOffset? ScheduleBeginDate { get; set; }
        public DateTimeOffset? ScheduleEndDate { get; set; }
        public string ScheduleCronTabExpression { get; set; }
        public List<DateTimeOffset> ExcludedDays { get; set; }
    }
}
