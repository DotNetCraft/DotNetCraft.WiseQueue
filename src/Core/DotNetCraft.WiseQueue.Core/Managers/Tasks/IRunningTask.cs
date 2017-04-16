using System;
using DotNetCraft.WiseQueue.Core.Entities;

namespace DotNetCraft.WiseQueue.Core.Managers.Tasks
{
    public interface IRunningTask
    {
        EventHandler<TaskInfo> OnCompletedEventHandler { get; set; }

        /// <summary>
        /// Execute task.
        /// </summary>
        void Execute();

        /// <summary>
        /// Cancel task execution.
        /// </summary>
        void Cancel();
    }
}
