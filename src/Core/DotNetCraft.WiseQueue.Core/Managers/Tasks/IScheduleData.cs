using System;

namespace DotNetCraft.WiseQueue.Core.Managers.Tasks
{
    public enum ScheduleType
    {
        Immediately = 1,
        ExecuteAt = 2,
        Interval = 3
    }

    public interface IScheduleData
    {
        ScheduleType ScheduleType { get; }

        DateTime GetNextExecutionTime();
    }
}
