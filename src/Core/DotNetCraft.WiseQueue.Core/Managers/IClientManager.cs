using System;
using System.Linq.Expressions;
using DotNetCraft.Common.Core.Domain.Management;
using DotNetCraft.WiseQueue.Core.Managers.Tasks;

namespace DotNetCraft.WiseQueue.Core.Managers
{
    public interface IClientManager: IBackgroundManager
    {
        /// <summary>
        /// Start new <c>task</c>.
        /// </summary>
        /// <param name="task">The <see cref="Expression"/> instance.</param>
        /// <returns>The task's identifier.</returns>
        int StartTask(Expression<Action> task);

        /// <summary>
        /// Start new <c>task</c>.
        /// </summary>
        /// <param name="task">The <see cref="Expression"/> instance.</param>
        /// <param name="scheduleData">Schedule</param>
        /// <returns>The task's identifier.</returns>
        int StartTask(Expression<Action> task, IScheduleData scheduleData);

        /// <summary>
        /// Start new <c>task</c>.
        /// </summary>
        /// <param name="queueName">The queue's name.</param>
        /// <param name="task">The <see cref="Expression"/> instance.</param>
        /// <returns>The task's identifier.</returns>
        int StartTask(string queueName, Expression<Action> task);

        /// <summary>
        /// Start new <c>task</c>.
        /// </summary>
        /// <param name="queueName">The queue's name.</param>
        /// <param name="task">The <see cref="Expression"/> instance.</param>
        /// <param name="scheduleData">Schedule</param>
        /// <returns>The task's identifier.</returns>
        int StartTask(string queueName, Expression<Action> task, IScheduleData scheduleData);

        /// <summary>
        /// Cancel a task that has been started.
        /// </summary>
        /// <param name="taskId">The task's identifier.</param>
        void StopTask(int taskId);
    }
}
