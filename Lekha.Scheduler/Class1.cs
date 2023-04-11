using System;
using System.Collections.Generic;

namespace Lekha.Scheduler
{
    public interface ITasklet
    {
        Guid Id { get; set; }
    }

    public interface ISchedule
    {
        DateTimeOffset Begin { get; set; }
        DateTimeOffset End { get; set; }
    }

    public class Tasklet<T> : ITasklet
    {
        public Guid Id { get; set; }
        public T Payload { get; set; }

        public Tasklet()
        {
            this.Id = Guid.NewGuid();
        }

        public Tasklet(Guid id, T payload)
        {
            this.Id = id;
            this.Payload = payload;
        }
    }

    public class ISkill
    {
        public string Name { get; set; }
    }
    public interface ITaskExecutor
    {
        Guid Id { get; set; }
        IEnumerable<ISkill> Skills { get; set; }
    }


    public class TaskExecutor : ITaskExecutor
    {
        public Guid Id { get; set; }
        public IEnumerable<ISkill> Skills { get; set; }
        public TaskExecutor()
        {
            this.Id = Guid.NewGuid();
        }

        public TaskExecutor(Guid id, IEnumerable<ISkill> skills)
        {
            this.Id = id;
            this.Skills = skills;
        }
    }
}
