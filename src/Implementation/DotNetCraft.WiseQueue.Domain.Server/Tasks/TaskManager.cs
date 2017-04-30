using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using DotNetCraft.Common.Core.DataAccessLayer;
using DotNetCraft.Common.Core.DataAccessLayer.Specifications;
using DotNetCraft.Common.Core.DataAccessLayer.UnitOfWorks.Simple;
using DotNetCraft.Common.Core.Domain.ServiceMessenger;
using DotNetCraft.Common.DataAccessLayer.Specifications;
using DotNetCraft.Common.DataAccessLayer.UnitOfWorks;
using DotNetCraft.Common.Domain.Management;
using DotNetCraft.WiseQueue.Core.Configurations;
using DotNetCraft.WiseQueue.Core.Converters;
using DotNetCraft.WiseQueue.Core.Entities;
using DotNetCraft.WiseQueue.Core.Entities.Enums;
using DotNetCraft.WiseQueue.Core.Managers.Tasks;
using DotNetCraft.WiseQueue.Core.Managers.Tasks.ScheduleStrategy;
using DotNetCraft.WiseQueue.Core.Models;
using DotNetCraft.WiseQueue.Core.Repositories;
using DotNetCraft.WiseQueue.Core.ServiceMessages;
using DotNetCraft.WiseQueue.DataAccessLayer.Specifications;

namespace DotNetCraft.WiseQueue.Domain.Server.Tasks
{
    public class TaskManager: BaseBackgroundManager<TaskManagerConfiguration>, ITaskManager, IServiceMessageHandler
    {
        private readonly IScheduleRepository scheduleRepository;
        private readonly ITaskRepository taskRepository;
        private readonly IJsonConverter jsonConverter;
        private readonly ITaskBuilder taskBuilder;
        private readonly ITaskProcessing taskProcessing;
        private readonly IUnitOfWorkFactory unitOfWorkFactory;
        private readonly IContextSettings contextSettings;
        private ServerDetails serverDetails;

        private readonly ConcurrentQueue<TaskInfo> readyTasks = new ConcurrentQueue<TaskInfo>();

        private readonly object syncObject = new object();

        public Guid ServiceMessageHandlerId { get; }

        public TaskManager(IScheduleRepository scheduleRepository, ITaskRepository taskRepository, IJsonConverter jsonConverter, ITaskBuilder taskBuilder, ITaskProcessing taskProcessing, IServiceMessageProcessor serviceMessageProcessor, IUnitOfWorkFactory unitOfWorkFactory, IContextSettings contextSettings, TaskManagerConfiguration managerConfiguration) : base(managerConfiguration)
        {
            if (scheduleRepository == null) throw new ArgumentNullException(nameof(scheduleRepository));
            if (taskRepository == null)
                throw new ArgumentNullException(nameof(taskRepository));
            if (jsonConverter == null) throw new ArgumentNullException(nameof(jsonConverter));
            if (taskBuilder == null) throw new ArgumentNullException(nameof(taskBuilder));
            if (taskProcessing == null) throw new ArgumentNullException(nameof(taskProcessing));
            if (serviceMessageProcessor == null)
                throw new ArgumentNullException(nameof(serviceMessageProcessor));
            if (unitOfWorkFactory == null)
                throw new ArgumentNullException(nameof(unitOfWorkFactory));
            if (contextSettings == null)
                throw new ArgumentNullException(nameof(contextSettings));

            this.scheduleRepository = scheduleRepository;
            this.taskRepository = taskRepository;
            this.jsonConverter = jsonConverter;
            this.taskBuilder = taskBuilder;
            this.taskProcessing = taskProcessing;
            this.unitOfWorkFactory = unitOfWorkFactory;
            this.contextSettings = contextSettings;

            ServiceMessageHandlerId = Guid.NewGuid();
            serviceMessageProcessor.RegisteredServiceMessageHandler(this);

            taskProcessing.OnTaskProcessed += OnTaskProcessed;
        }

        private void OnTaskProcessed(object sender, TaskInfo taskInfo)
        {
            if (taskInfo == null)
                return;
            readyTasks.Enqueue(taskInfo);
            base.ForceRun("New task has been completed");
        }

        #region Overrides of BaseBackgroundManager<TaskManagerConfiguration>

        private void ProcessTaskCancelation(int currentServerId)
        {
            Queue<int> cancelQueueTasks = new Queue<int>();
            using (IUnitOfWork unitOfWork = unitOfWorkFactory.CreateUnitOfWork(contextSettings))
            {
                IDataRequest<TaskInfo> specification = new DataRequest<TaskInfo>(new TaskCancellationSpecification(currentServerId));
                //Retrieving tasks for cancellation...
                specification.Take = 10;
                ICollection<TaskInfo> tasks = taskRepository.GetBySpecification(specification);
                foreach (TaskInfo taskInfo in tasks)
                {
                    try
                    {
                        cancelQueueTasks.Enqueue(taskInfo.Id);
                    }
                    catch (Exception ex)
                    {
                        taskInfo.TaskState = TaskStates.Failed;
                        unitOfWork.Update(taskInfo);
                    }
                }
                if (cancelQueueTasks.Count > 0)
                    unitOfWork.Commit();
            }

            while (cancelQueueTasks.Count > 0)
            {
                int taskId = cancelQueueTasks.Dequeue();
                taskProcessing.CancelTask(taskId);
            }
        }

        private void ProcessCompletedTasks(int currentServerId)
        {
            using (IUnitOfWork unitOfWork = unitOfWorkFactory.CreateUnitOfWork(contextSettings))
            {
                while (readyTasks.Count > 0)
                {
                    TaskInfo taskInfo;
                    if (readyTasks.TryDequeue(out taskInfo) == false)
                    {
                        continue;
                    }
                    taskInfo.ServerId = currentServerId;
                    unitOfWork.Update(taskInfo);

                    if (taskInfo.ScheduleInfoId > 0)
                    {
                        var scheduleInfo = scheduleRepository.Get(taskInfo.ScheduleInfoId);
                        Type type = jsonConverter.ConvertFromJson<Type>(scheduleInfo.ScheduleDataType);
                        IScheduleStrategy scheduleStrategy = (IScheduleStrategy)jsonConverter.ConvertFromJson(scheduleInfo.ScheduleData, type);

                        TaskInfo entity = new TaskInfo
                        {
                            QueueName = taskInfo.QueueName,
                            InstanceType = taskInfo.InstanceType,
                            Method = taskInfo.Method,
                            ParametersTypes = taskInfo.ParametersTypes,
                            Arguments = taskInfo.Arguments,
                            TaskState = TaskStates.New,
                            ScheduleInfoId = taskInfo.ScheduleInfoId,
                            ExecuteAt = scheduleStrategy.GetNextExecutionTime(taskInfo.ExecuteAt, taskInfo.TaskState),
                            RepeatCrashCount = 3 //TODO: Settings
                        };
                        unitOfWork.Insert(entity, false);
                    }
                }
                unitOfWork.Commit();
            }
        }

        private void ProcessNewTasks(int currentServerId)
        {
            if (taskProcessing.Slots > 0)
            {
                Queue<IRunningTask> runningTasks = new Queue<IRunningTask>();
                using (IUnitOfWork unitOfWork = unitOfWorkFactory.CreateUnitOfWork(contextSettings))
                {
                    //TODO: Queues
                    ICollection<TaskInfo> tasks = unitOfWork.ExecuteQuery<TaskInfo>(@"
                                       UPDATE TOP (@MaxRows) [DotNetCraftWiseQueue].[dbo].[TaskInfoes]
			                                SET [TaskState] = @UpdatedTaskState,
				                                [ServerId] = @ServerId
			                                OUTPUT inserted.*
			                                Where ( [ServerId] = 0 
					                                AND [TaskState] = @NewTask
					                                AND [QueueName] in (@QueueName)
					                                AND [ExecuteAt] <= @ExecuteAt 
					                                AND [RepeatCrashCount] > 0);",
                                                    new DataBaseParameter("@MaxRows", taskProcessing.Slots),
                                                    new DataBaseParameter("@UpdatedTaskState", TaskStates.Running),
                                                    new DataBaseParameter("@ServerId", currentServerId),
                                                    new DataBaseParameter("@NewTask", TaskStates.New),
                                                    new DataBaseParameter("@ExecuteAt", DateTime.UtcNow),
                                                    new DataBaseParameter("@QueueName", "default"));

                    foreach (TaskInfo taskInfo in tasks)
                    {
                        try
                        {
                            IRunningTask runningTask = taskBuilder.Build(taskInfo);
                            runningTasks.Enqueue(runningTask);
                            taskInfo.ServerId = currentServerId;
                            taskInfo.TaskState = TaskStates.Running;
                        }
                        catch (Exception ex)
                        {
                            taskInfo.ServerId = currentServerId;
                            taskInfo.TaskState = TaskStates.Failed;
                        }

                        if (taskInfo.ScheduleInfoId > 0)
                        {
                            ScheduleInfo scheduleInfo = scheduleRepository.Get(taskInfo.ScheduleInfoId);
                            Type type = jsonConverter.ConvertFromJson<Type>(scheduleInfo.ScheduleDataType);
                            IScheduleStrategy scheduleStrategy = (IScheduleStrategy)jsonConverter.ConvertFromJson(scheduleInfo.ScheduleData, type);
                            taskInfo.ExecuteAt = scheduleStrategy.GetNextExecutionTime(taskInfo.ExecuteAt, taskInfo.TaskState);
                        }                        

                        unitOfWork.Update(taskInfo);
                    }

                    if (runningTasks.Count > 0)
                        unitOfWork.Commit();
                }

                while (runningTasks.Count > 0)
                {
                    IRunningTask runningTask = runningTasks.Dequeue();
                    taskProcessing.RunTask(runningTask);
                }
            }
        }

        protected override void OnBackroundExecution()
        {
            if (serverDetails == null || serverDetails.IsOnline == false)
                return;

            int currentServerId;
            lock (syncObject)
            {
                currentServerId = serverDetails.ServerId;
            }

            ProcessTaskCancelation(currentServerId);
            ProcessNewTasks(currentServerId);
            ProcessCompletedTasks(currentServerId);
        }

        #endregion

        #region Implementation of IServiceMessageHandler

        public bool HandleMessage(IServiceMessage message)
        {
            if (message is ServerRegistrationMessage)
            {
                lock (syncObject)
                {
                    ServerRegistrationMessage serverRegistrationMessage = (ServerRegistrationMessage) message;
                    serverDetails = serverRegistrationMessage.ServerDetails;
                }
                return true;
            }

            return false;
        }        

        #endregion
    }
}
