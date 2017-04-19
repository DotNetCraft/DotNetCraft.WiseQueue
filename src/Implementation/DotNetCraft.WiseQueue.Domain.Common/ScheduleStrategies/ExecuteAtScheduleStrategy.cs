using System;
using DotNetCraft.WiseQueue.Core.Entities.Enums;
using DotNetCraft.WiseQueue.Core.Managers.Tasks.ScheduleStrategy;

namespace DotNetCraft.WiseQueue.Domain.Common.ScheduleStrategies
{
    /// <summary>
    /// Use this strategy if you want to define your task's execution time.
    /// </summary>
    public class ExecuteAtScheduleStrategy: IScheduleStrategy
    {
        /// <summary>
        /// Date and time when the task should be executed.
        /// </summary>
        private readonly DateTime executeAt;

        /// <summary>
        /// Schedule strategy type
        /// </summary>
        /// <remarks>ScheduleType.ExecuteAt</remarks>
        public ScheduleType ScheduleType
        {
            get {return ScheduleType.ExecuteAt;}
        }

        #region Constructors...
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="executeAt"> Date and time when the task should be executed.</param>
        public ExecuteAtScheduleStrategy(DateTime executeAt)
        {
            this.executeAt = executeAt;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <remarks>This constructor for deserialization purposes only.</remarks>
        private ExecuteAtScheduleStrategy() { }
        #endregion

        #region Implementation of IScheduleStrategy

        /// <summary>
        /// Get next execution time.
        /// </summary>
        /// <param name="currentExecutionTime">The date and time when this task has been executed.</param>
        /// <param name="taskState">Current task state</param>
        /// <returns>The datetime when task should be executed.</returns>
        public DateTime GetNextExecutionTime(DateTime currentExecutionTime, TaskStates taskState)
        {
            return executeAt;
        }

        #endregion
    }
}
