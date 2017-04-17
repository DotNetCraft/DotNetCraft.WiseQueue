using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DotNetCraft.Common.Core.Utils.Logging;
using DotNetCraft.Common.Utils.Logging;
using DotNetCraft.WiseQueue.Core.Entities;
using DotNetCraft.WiseQueue.Core.Entities.Enums;
using DotNetCraft.WiseQueue.Core.Managers.Tasks;

namespace DotNetCraft.WiseQueue.Domain.Server.Tasks
{
    public class RunningTask: IRunningTask
    {
        private readonly ICommonLogger logger = LogManager.GetCurrentClassLogger();

        private readonly TaskInfo taskInfo;
        private readonly object instance;
        private readonly MethodInfo method;
        private readonly Type[] argumentTypes;
        private readonly object[] arguments;

        private CancellationTokenSource taskCancelTokenSource;

        public int TaskId
        {
            get
            {
                return taskInfo.Id;
            }
        }

        public RunningTask(TaskInfo taskInfo, object instance, MethodInfo method, Type[] argumentTypes, object[] arguments)
        {
            if (taskInfo == null) throw new ArgumentNullException(nameof(taskInfo));
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (method == null) throw new ArgumentNullException(nameof(method));
            if (argumentTypes == null) throw new ArgumentNullException(nameof(argumentTypes));
            if (arguments == null) throw new ArgumentNullException(nameof(arguments));

            this.taskInfo = taskInfo;
            this.instance = instance;
            this.method = method;
            this.argumentTypes = argumentTypes;
            this.arguments = arguments;

            taskCancelTokenSource = new CancellationTokenSource();
        }

        #region Implementation of IRunningTask
        
        public EventHandler<TaskInfo> OnCompletedEventHandler { get; set; }

        /// <summary>
        /// Execute task.
        /// </summary>
        public void Execute()
        {
            CancellationToken taskCancellationToken = taskCancelTokenSource.Token;

            try
            {
                Task.Run(() =>
                {
                    for (int i = 0; i < argumentTypes.Length; i++)
                    {
                        if (argumentTypes[i] == typeof(CancellationToken))
                        {
                            arguments[i] = taskCancellationToken;
                            break;
                        }
                    }
                    try
                    {
                        method.Invoke(instance, arguments);
                        if (OnCompletedEventHandler != null)
                        {
                            if (taskCancellationToken.IsCancellationRequested)
                            {
                                taskInfo.TaskState = TaskStates.Cancelled;
                            }
                            else
                            {
                                taskInfo.TaskState = TaskStates.Successed;
                            }
                            OnCompletedEventHandler(this, taskInfo);
                        }
                    }
                    catch (OperationCanceledException canceledException)
                    {
                        string msg = string.Format(
                            "The task has been canceled via OperationCanceledException: {0}.{1}.",
                            instance.GetType().FullName, method.Name);
                        logger.Error(msg, canceledException);
                        if (OnCompletedEventHandler != null)
                        {
                            taskInfo.TaskState = TaskStates.Cancelled;
                            OnCompletedEventHandler(this, taskInfo);
                        }
                    }
                    catch (Exception ex)
                    {
                        string msg = string.Format("There was an error during executing task: {0}.{1}.",
                            instance.GetType().FullName, method.Name);
                        logger.Error(msg, ex);
                        if (OnCompletedEventHandler != null)
                        {
                            taskInfo.RepeatCrashCount--;
                            if (taskInfo.RepeatCrashCount == 0)
                            {
                                taskInfo.TaskState = TaskStates.Failed;
                            }
                            else
                            {
                                taskInfo.ServerId = 0;
                                taskInfo.TaskState = TaskStates.New;
                            }
                            OnCompletedEventHandler(this, taskInfo);
                        }
                    }
                }, taskCancellationToken);
            }
            catch (Exception ex)
            {
                string msg = string.Format("There was an error during executing task: {0}.{1}.",
                            instance.GetType().FullName, method.Name);
                logger.Error(msg, ex);
                if (OnCompletedEventHandler != null)
                {
                    taskInfo.RepeatCrashCount--;
                    if (taskInfo.RepeatCrashCount == 0)
                    {
                        taskInfo.TaskState = TaskStates.Failed;
                    }
                    else
                    {
                        taskInfo.ServerId = 0;
                        taskInfo.TaskState = TaskStates.New;
                    }
                    OnCompletedEventHandler(this, taskInfo);
                }
            }
        }

        /// <summary>
        /// Cancel task execution.
        /// </summary>
        public void Cancel()
        {
            taskCancelTokenSource.Cancel();
        }

        #endregion
    }
}
