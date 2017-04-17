using System;
using System.Collections.Generic;
using System.Threading;
using DotNetCraft.WiseQueue.Core.Configurations;
using DotNetCraft.WiseQueue.Core.Entities;
using DotNetCraft.WiseQueue.Core.Managers.Tasks;

namespace DotNetCraft.WiseQueue.Domain.Server.Tasks
{
    public class TaskProcessing: ITaskProcessing
    {
        private readonly ITaskBuilder taskBuilder;
        private readonly Dictionary<int, IRunningTask> RunningTasks;

        private int slots;

        public int Slots { get { return slots; } }

        public TaskProcessing(ITaskBuilder taskBuilder, TaskProcessingConfiguration taskProcessingConfiguration)
        {
            if (taskBuilder == null) throw new ArgumentNullException(nameof(taskBuilder));
            if (taskProcessingConfiguration == null)
                throw new ArgumentNullException(nameof(taskProcessingConfiguration));

            this.taskBuilder = taskBuilder;
            slots = taskProcessingConfiguration.MaxProcessingTasks;
            RunningTasks = new Dictionary<int, IRunningTask>();
        }

        #region Implementation of ITaskProcessing

        
        public void RunTask(IRunningTask runningTask)
        {
            lock (RunningTasks)
            {
                RunningTasks.Add(runningTask.TaskId, runningTask);
            }

            runningTask.OnCompletedEventHandler += OnCompletedEventHandler;
            Interlocked.Decrement(ref slots);
            runningTask.Execute();
        }

        public void CancelTask(int taskId)
        {
            IRunningTask task;
            lock (RunningTasks)
            {
                if (RunningTasks.TryGetValue(taskId, out task))
                    RunningTasks.Remove(taskId);
            }

            if (task != null)
                task.Cancel();
        }

        private void OnCompletedEventHandler(object sender, TaskInfo taskInfo)
        {
            IRunningTask runningTask = (IRunningTask) sender;
            if (runningTask == null)
                return;
                
            runningTask.OnCompletedEventHandler -= OnCompletedEventHandler;
            lock (RunningTasks)
            {
                if (RunningTasks.ContainsKey(taskInfo.Id))
                    RunningTasks.Remove(taskInfo.Id);
            }

            if (OnTaskProcessed != null)
                OnTaskProcessed(this, taskInfo);
            Interlocked.Increment(ref slots);
        }

        public EventHandler<TaskInfo> OnTaskProcessed { get; set; }

        #endregion
    }
}
