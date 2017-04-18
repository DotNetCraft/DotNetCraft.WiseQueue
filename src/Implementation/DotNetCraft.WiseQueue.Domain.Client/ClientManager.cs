using System;
using System.Linq.Expressions;
using DotNetCraft.Common.Core.DataAccessLayer;
using DotNetCraft.Common.Core.DataAccessLayer.UnitOfWorks.Simple;
using DotNetCraft.Common.Domain.Management;
using DotNetCraft.WiseQueue.Core.Configurations;
using DotNetCraft.WiseQueue.Core.Converters;
using DotNetCraft.WiseQueue.Core.Entities;
using DotNetCraft.WiseQueue.Core.Entities.Enums;
using DotNetCraft.WiseQueue.Core.Managers;
using DotNetCraft.WiseQueue.Core.Managers.Tasks;
using DotNetCraft.WiseQueue.Core.Repositories;
using DotNetCraft.WiseQueue.Domain.Common;
using DotNetCraft.WiseQueue.Domain.Common.Schedules;

namespace DotNetCraft.WiseQueue.Domain.Client
{
    public class ClientManager: BaseBackgroundManager<ClientManagerConfiguration>, IClientManager
    {
        private const string defaultQueueName = "default";

        private readonly ITaskRepository taskRepository;
        private readonly IExpressionConverter expressionConverter;
        private readonly IJsonConverter jsonConverter;
        private readonly IUnitOfWorkFactory unitOfWorkFactory;
        private readonly IContextSettings contextSettings;        

        public ClientManager(ITaskRepository taskRepository, IExpressionConverter expressionConverter, IJsonConverter jsonConverter, IUnitOfWorkFactory unitOfWorkFactory, IContextSettings contextSettings, ClientManagerConfiguration managerConfiguration) : base(managerConfiguration)
        {
            if (taskRepository == null) throw new ArgumentNullException(nameof(taskRepository));
            if (expressionConverter == null) throw new ArgumentNullException(nameof(expressionConverter));
            if (jsonConverter == null) throw new ArgumentNullException(nameof(jsonConverter));
            if (unitOfWorkFactory == null)
                throw new ArgumentNullException(nameof(unitOfWorkFactory));
            if (contextSettings == null)
                throw new ArgumentNullException(nameof(contextSettings));

            this.taskRepository = taskRepository;
            this.expressionConverter = expressionConverter;
            this.jsonConverter = jsonConverter;
            this.unitOfWorkFactory = unitOfWorkFactory;
            this.contextSettings = contextSettings;
        }

        #region Implementation of IClientManager

        /// <summary>
        /// Start new <c>task</c>.
        /// </summary>
        /// <param name="task">The <see cref="Expression"/> instance.</param>
        /// <returns>The task's identifier.</returns>
        public int StartTask(Expression<Action> task)
        {
            return StartTask(defaultQueueName, task);
        }

        /// <summary>
        /// Start new <c>task</c>.
        /// </summary>
        /// <param name="task">The <see cref="Expression"/> instance.</param>
        /// <param name="scheduleData">Schedule</param>
        /// <returns>The task's identifier.</returns>
        public int StartTask(Expression<Action> task, IScheduleData scheduleData)
        {
            return StartTask(defaultQueueName, task, scheduleData);
        }

        /// <summary>
        /// Start new <c>task</c>.
        /// </summary>
        /// <param name="queueName">The queue's name.</param>
        /// <param name="task">The <see cref="Expression"/> instance.</param>
        /// <returns>The task's identifier.</returns>
        public int StartTask(string queueName, Expression<Action> task)
        {
            return StartTask(defaultQueueName, task, new ImmediatelySchedule());
        }

        /// <summary>
        /// Start new <c>task</c>.
        /// </summary>
        /// <param name="queueName">The queue's name.</param>
        /// <param name="task">The <see cref="Expression"/> instance.</param>
        /// <param name="scheduleData">Schedule</param>
        /// <returns>The task's identifier.</returns>
        public int StartTask(string queueName, Expression<Action> task, IScheduleData scheduleData)
        {
            using (IUnitOfWork unitOfWork = unitOfWorkFactory.CreateUnitOfWork(contextSettings))
            {
                ActivationData activationData = expressionConverter.Convert(task);

                string[] args = expressionConverter.SerializeArguments(activationData.Arguments);

                TaskInfo entity = new TaskInfo
                {
                    QueueName = queueName,
                    InstanceType = jsonConverter.ConvertToJson(activationData.InstanceType),
                    Method = activationData.Method.Name,
                    ParametersTypes = jsonConverter.ConvertToJson(activationData.ArgumentTypes),
                    Arguments = jsonConverter.ConvertToJson(args),
                    TaskState = TaskStates.New,
                    CreatedAt = DateTime.UtcNow,
                    LastModified = DateTime.UtcNow,
                    ExecuteAt = DateTime.UtcNow,
                    RepeatCrashCount = 3 //TODO: Settings
                };

                switch (scheduleData.ScheduleType)
                {
                    case ScheduleType.Immediately:
                    case ScheduleType.ExecuteAt:
                    {
                        entity.ScheduleInfoId = -1;
                        break;
                    }
                    default:
                    {
                        ScheduleInfo scheduleInfo = new ScheduleInfo
                        {
                            ScheduleType = scheduleData.ScheduleType,
                            ScheduleData = jsonConverter.ConvertToJson(scheduleData),
                            ScheduleDataType = jsonConverter.ConvertToJson(scheduleData.GetType())
                        };
                        unitOfWork.Insert(scheduleInfo);
                        entity.ScheduleInfoId = scheduleInfo.Id;
                        break;
                    }
                }

                unitOfWork.Insert(entity);
                unitOfWork.Commit();

                return entity.Id;
            }
        }

        /// <summary>
        /// Cancel a task that has been started.
        /// </summary>
        /// <param name="taskId">The task's identifier.</param>
        public void StopTask(int taskId)
        {
            using (IUnitOfWork unitOfWork = unitOfWorkFactory.CreateUnitOfWork(contextSettings))
            {
                int attemts = 3;
                while (attemts-- > 0)
                {
                    try
                    {
                        TaskInfo taskInfo = taskRepository.Get(taskId);
                        taskInfo.TaskState = TaskStates.Cancelling;
                        unitOfWork.Update(taskInfo);
                        unitOfWork.Commit();
                        break;
                    }
                    catch (Exception exception)
                    {
                    }
                }
            }
        }

        #endregion
    }
}
