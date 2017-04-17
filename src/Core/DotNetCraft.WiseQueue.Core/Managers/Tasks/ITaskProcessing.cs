using System;
using DotNetCraft.WiseQueue.Core.Entities;

namespace DotNetCraft.WiseQueue.Core.Managers.Tasks
{
    public interface ITaskProcessing
    {
        int Slots { get; }

        void RunTask(IRunningTask taskInfo);

        void CancelTask(int taskId);

        EventHandler<TaskInfo> OnTaskProcessed { get; set; }        
    }
}
