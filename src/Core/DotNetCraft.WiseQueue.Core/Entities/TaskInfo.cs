﻿using System;
using System.ComponentModel.DataAnnotations;
using DotNetCraft.WiseQueue.Core.Entities.Enums;

namespace DotNetCraft.WiseQueue.Core.Entities
{
    /// <summary>
    /// Class contains all fields that are using to store information about a task.
    /// </summary>
    public class TaskInfo : BaseWiseEntity
    {
        #region Properties...

        /// <summary>
        /// The queue's identifier where the task will be put.
        /// </summary>
        public string QueueName { get; set; }

        /// <summary>
        /// The server identifier.
        /// </summary>
        public int ServerId { get; set; }

        /// <summary>
        /// Task's state.
        /// </summary>
        public TaskStates TaskState { get; set; }

        /// <summary>
        /// Information about class that will be used for executing method.
        /// </summary>
        public string InstanceType { get; set; }

        /// <summary>
        /// Information about method that will be executed.
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// Parameters' types that are using in the method.
        /// </summary>
        public string ParametersTypes { get; set; }

        /// <summary>
        /// Arguments' values that are using in the method.
        /// </summary>
        public string Arguments { get; set; }

        /// <summary>
        /// Count of attempts that will be used for reruning this task after its crashed.
        /// </summary>
        public int RepeatCrashCount { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime LastModified { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        public int ScheduleInfoId { get; set; }
        public DateTime ExecuteAt { get; set; }

        #endregion

        #region Overrides of Object

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return string.Format("Id: {0}; TaskState: {1}; QueueName: {2}; taskActivationDetails: {3}", Id, TaskState,
                QueueName, "TODO:"); //TODO: ToString().
        }

        #endregion
    }
}