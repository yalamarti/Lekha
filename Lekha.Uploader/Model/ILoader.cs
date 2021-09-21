using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Lekha.Uploader.Model
{
    public class Claim
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
    public class User
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public List<Claim> Claims { get; set; }
    }

    public class TaskGroupExecutionSchedule
    {
        public DateTimeOffset Begin { get; set; }
        public DateTimeOffset End { get; set; }
        public List<DateTimeOffset> ExcludeDays { get; set; }
    }

    public class TaskExecutionScheduleStrategy
    {
        public DateTimeOffset Begin { get; set; }
        public DateTimeOffset End { get; set; }
    }

    public class WorkFlow
    {
        public string Name { get; set; }
    }

    public class TaskGroup
    {
        public string Name { get; set; }
        public TaskGroupExecutionSchedule Schedule { get; set; }
        public WorkFlow WorkFlow { get; set; }
    }

    public class Project
    {
        public string Name { get; set; }
        public List<TaskGroup> TaskGroups { get; set; }
    }

    public class Account
    {
        public List<User> Users { get; set; }
        public List<Project> Projects { get; set; }
    }

    public interface ILoader
    {
        // Load transformed data to the target data store for later retrieval
        Task<StringBuilder> Load(Stream stream);
    }
}
