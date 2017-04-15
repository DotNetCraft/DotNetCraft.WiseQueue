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
using DotNetCraft.WiseQueue.Core.Managers;
using DotNetCraft.WiseQueue.Core.Repositories;
using DotNetCraft.WiseQueue.Core.ServiceMessages;
using DotNetCraft.WiseQueue.DataAccessLayer.Specifications;

namespace DotNetCraft.WiseQueue.Domain.Server
{
    public class TaskManager: BaseBackgroundManager<TaskManagerConfiguration>, ITaskManager, IServiceMessageHandler
    {
        private readonly ITaskRepository taskRepository;
        private readonly IServiceMessageProcessor serviceMessageProcessor;
        private readonly IUnitOfWorkFactory unitOfWorkFactory;
        private readonly IContextSettings contextSettings;
        private int serverId;

        private readonly object syncObject = new object();

        public Guid ServiceMessageHandlerId { get; }

        public TaskManager(ITaskRepository taskRepository, IServiceMessageProcessor serviceMessageProcessor, IUnitOfWorkFactory unitOfWorkFactory, IContextSettings contextSettings, TaskManagerConfiguration managerConfiguration) : base(managerConfiguration)
        {
            if (taskRepository == null)
                throw new ArgumentNullException(nameof(taskRepository));
            if (serviceMessageProcessor == null)
                throw new ArgumentNullException(nameof(serviceMessageProcessor));
            if (unitOfWorkFactory == null)
                throw new ArgumentNullException(nameof(unitOfWorkFactory));
            if (contextSettings == null)
                throw new ArgumentNullException(nameof(contextSettings));

            this.taskRepository = taskRepository;
            this.serviceMessageProcessor = serviceMessageProcessor;
            this.unitOfWorkFactory = unitOfWorkFactory;
            this.contextSettings = contextSettings;

            ServiceMessageHandlerId = Guid.NewGuid();
            serviceMessageProcessor.RegisteredWaitHandle(this);
        }

        #region Overrides of BaseBackgroundManager<TaskManagerConfiguration>

        protected override void OnBackroundExecution()
        {
            if (serverId == 0)
                return;
            
            int currentServerId;
            lock (syncObject)
            {
                currentServerId = serverId;
            }

            using (IUnitOfWork unitOfWork = unitOfWorkFactory.CreateUnitOfWork(contextSettings))
            {
                ISpecificationRequest<TaskInfo> specification = new SimpleSpecificationRequest<TaskInfo>();
                specification.Take = managerConfiguration.MaxProcessingTasks;
                specification.Specification = new NewTaskSpecification();
                ICollection<TaskInfo> tasks = taskRepository.GetBySpecification(specification);

                foreach (TaskInfo taskInfo in tasks)
                {
                    IServiceMessage message = new TaskReceivedMessage(taskInfo, this);
                    bool isReceived = serviceMessageProcessor.SendMessage(message);
                    if (isReceived)
                    {
                        taskInfo.ServerId = currentServerId;
                        taskInfo.TaskState = TaskStates.Running;
                        unitOfWork.Update(taskInfo);
                    }
                }

                unitOfWork.Commit();
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
                    serverId = serverRegistrationMessage.ServerId;
                }
                return true;
            }

            return false;
        }        

        #endregion
    }
}
