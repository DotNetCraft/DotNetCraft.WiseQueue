using System;
using DotNetCraft.WiseQueue.Core.Entities.Enums;
using DotNetCraft.WiseQueue.Core.Managers.Tasks.ScheduleStrategy;

namespace DotNetCraft.WiseQueue.Domain.Common.ScheduleStrategies
{
    public class IntervalSchedule : IScheduleStrategy
    {
        /// <summary>
        /// Interval that will be used for the task's execution.
        /// </summary>
        public TimeSpan Interval { get; private set; }

        /// <summary>
        /// Schedule strategy type
        /// </summary>
        /// <remarks>ScheduleType.Interval;</remarks>
        public ScheduleType ScheduleType
        {
            get { return ScheduleType.Interval; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="interval">Interval that will be used for the task's execution.</param>
        public IntervalSchedule(TimeSpan interval)
        {
            Interval = interval;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <remarks>This constructor for deserialization purposes only.</remarks>
        private IntervalSchedule() { }

        #region Implementation of IScheduleStrategy

        /// <summary>
        /// Get next execution time.
        /// </summary>
        /// <param name="currentExecutionTime">The date and time when this task has been executed.</param>
        /// <param name="taskState">Current task state</param>
        /// <returns>The datetime when task should be executed.</returns>
        public DateTime GetNextExecutionTime(DateTime currentExecutionTime, TaskStates taskState)
        {
            return currentExecutionTime.Add(Interval);
        }

        #endregion
    }
}
