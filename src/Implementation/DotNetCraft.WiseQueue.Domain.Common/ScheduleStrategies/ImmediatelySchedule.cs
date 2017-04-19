using System;
using DotNetCraft.WiseQueue.Core.Entities.Enums;
using DotNetCraft.WiseQueue.Core.Managers.Tasks.ScheduleStrategy;

namespace DotNetCraft.WiseQueue.Domain.Common.ScheduleStrategies
{
    /// <summary>
    /// Use this strategy if you want to your task be executed immediately.
    /// </summary>
    public class ImmediatelySchedule : IScheduleStrategy
    {
        /// <summary>
        /// Schedule strategy type
        /// </summary>
        /// <remarks>ScheduleType.Immediately</remarks>
        public ScheduleType ScheduleType
        {
            get { return ScheduleType.Immediately; }
        }

        #region Implementation of IScheduleStrategy

        /// <summary>
        /// Get next execution time.
        /// </summary>
        /// <param name="currentExecutionTime">The date and time when this task has been executed.</param>
        /// <param name="taskState">Current task state</param>
        /// <returns>The datetime when task should be executed.</returns>
        public DateTime GetNextExecutionTime(DateTime currentExecutionTime, TaskStates taskState)
        {
            return DateTime.UtcNow;
        }

        #endregion
    }
}
