using System;
using System.Collections.Generic;
using DotNetCraft.Common.Core.DataAccessLayer;
using DotNetCraft.Common.Core.DataAccessLayer.Specofications;
using DotNetCraft.Common.Core.DataAccessLayer.UnitOfWorks.Simple;
using DotNetCraft.Common.Core.Domain.ServiceMessenger;
using DotNetCraft.Common.Domain.Management;
using DotNetCraft.WiseQueue.Core.Configurations;
using DotNetCraft.WiseQueue.Core.Entities;
using DotNetCraft.WiseQueue.Core.Entities.Enums;
using DotNetCraft.WiseQueue.Core.Managers.Tasks;
using DotNetCraft.WiseQueue.Core.Models;
using DotNetCraft.WiseQueue.Core.Repositories;
using DotNetCraft.WiseQueue.Core.ServiceMessages;
using DotNetCraft.WiseQueue.DataAccessLayer.Specifications;

namespace DotNetCraft.WiseQueue.Domain.Server.Tasks
{
    public class TaskManager: BaseBackgroundManager<TaskManagerConfiguration>, ITaskManager, IServiceMessageHandler
    {
        private readonly ITaskRepository taskRepository;
        private readonly ITaskProcessing taskProcessing;
        private readonly IUnitOfWorkFactory unitOfWorkFactory;
        private readonly IContextSettings contextSettings;
        private ServerDetails serverDetails;

        private readonly object syncObject = new object();

        public Guid ServiceMessageHandlerId { get; }

        public TaskManager(ITaskRepository taskRepository, ITaskProcessing taskProcessing, IServiceMessageProcessor serviceMessageProcessor, IUnitOfWorkFactory unitOfWorkFactory, IContextSettings contextSettings, TaskManagerConfiguration managerConfiguration) : base(managerConfiguration)
        {
            if (taskRepository == null)
                throw new ArgumentNullException(nameof(taskRepository));
            if (taskProcessing == null) throw new ArgumentNullException(nameof(taskProcessing));
            if (serviceMessageProcessor == null)
                throw new ArgumentNullException(nameof(serviceMessageProcessor));
            if (unitOfWorkFactory == null)
                throw new ArgumentNullException(nameof(unitOfWorkFactory));
            if (contextSettings == null)
                throw new ArgumentNullException(nameof(contextSettings));

            this.taskRepository = taskRepository;
            this.taskProcessing = taskProcessing;
            this.unitOfWorkFactory = unitOfWorkFactory;
            this.contextSettings = contextSettings;

            ServiceMessageHandlerId = Guid.NewGuid();
            serviceMessageProcessor.RegisteredWaitHandle(this);

            taskProcessing.OnTaskProcessed += OnTaskProcessed;
        }

        private void OnTaskProcessed(object sender, TaskInfo taskInfo)
        {
            lock (syncObject)
            {
                using (IUnitOfWork unitOfWork = unitOfWorkFactory.CreateUnitOfWork(contextSettings))
                {
                    unitOfWork.Update(taskInfo);
                    unitOfWork.Commit();
                }
            }
        }

        #region Overrides of BaseBackgroundManager<TaskManagerConfiguration>

        protected override void OnBackroundExecution()
        {
            if (serverDetails == null || serverDetails.IsOnline == false)
                return;

            int currentServerId;
            lock (syncObject)
            {
                currentServerId = serverDetails.ServerId;
            }

            lock (syncObject)
            {
                using (IUnitOfWork unitOfWork = unitOfWorkFactory.CreateUnitOfWork(contextSettings))
                {
                    bool needToSave = false;
                    ISpecificationRequest<TaskInfo> specification = new SimpleSpecificationRequest<TaskInfo>();
                    //Retrieving tasks for cancellation...
                    specification.Take = 10;
                    specification.Specification = new TaskCancellationSpecification(currentServerId);
                    ICollection<TaskInfo> tasks = taskRepository.GetBySpecification(specification);
                    foreach (TaskInfo taskInfo in tasks)
                    {
                        try
                        {
                            taskProcessing.CancelTask(taskInfo);
                            taskInfo.ServerId = currentServerId;
                            taskInfo.TaskState = TaskStates.Cancelled;
                        }
                        catch (Exception ex)
                        {
                            taskInfo.TaskState = TaskStates.Failed;
                        }
                        unitOfWork.Update(taskInfo);
                        needToSave = true;
                    }

                    if (taskProcessing.Slots > 0)
                    {
                        //Retrieving new tasks...
                        specification.Take = taskProcessing.Slots;
                        specification.Specification = new NewTaskSpecification(serverDetails.QueueNames);
                        tasks = taskRepository.GetBySpecification(specification);

                        foreach (TaskInfo taskInfo in tasks)
                        {
                            try
                            {
                                taskProcessing.RunTask(taskInfo);
                                taskInfo.ServerId = currentServerId;
                                taskInfo.TaskState = TaskStates.Running;
                            }
                            catch (Exception ex)
                            {
                                taskInfo.ServerId = currentServerId;
                                taskInfo.TaskState = TaskStates.Failed;
                            }
                            unitOfWork.Update(taskInfo);
                            needToSave = true;
                        }
                    }

                    if (needToSave)
                        unitOfWork.Commit();
                }
            }
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
