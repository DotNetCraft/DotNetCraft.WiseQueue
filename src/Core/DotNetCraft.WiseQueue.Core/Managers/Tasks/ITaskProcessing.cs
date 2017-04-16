using System;
using DotNetCraft.WiseQueue.Core.Entities;

namespace DotNetCraft.WiseQueue.Core.Managers.Tasks
{
    public interface ITaskProcessing
    {
        int Slots { get; }

        void RunTask(TaskInfo taskInfo);

        void CancelTask(TaskInfo taskInfo);

        EventHandler<TaskInfo> OnTaskProcessed { get; set; }        
    }
}
