using System;
using DotNetCraft.WiseQueue.Core.Entities.Enums;

namespace DotNetCraft.WiseQueue.Core.Managers.Tasks.ScheduleStrategy
{
    /// <summary>
    /// Interface shows that object is a schedule strategy.
    /// </summary>
    public interface IScheduleStrategy
    {
        /// <summary>
        /// Schedule strategy type
        /// </summary>
        ScheduleType ScheduleType { get; }

        /// <summary>
        /// Get next execution time.
        /// </summary>
        /// <param name="currentExecutionTime">The date and time when this task has been executed.</param>
        /// <param name="taskState">Current task state</param>
        /// <returns>The datetime when task should be executed.</returns>
        DateTime GetNextExecutionTime(DateTime currentExecutionTime, TaskStates taskState);
    }
}
