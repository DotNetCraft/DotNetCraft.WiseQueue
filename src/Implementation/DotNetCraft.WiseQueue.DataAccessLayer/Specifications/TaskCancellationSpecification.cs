using System;
using System.Linq.Expressions;
using DotNetCraft.Common.Core.DataAccessLayer.Specofications;
using DotNetCraft.WiseQueue.Core.Entities;
using DotNetCraft.WiseQueue.Core.Entities.Enums;

namespace DotNetCraft.WiseQueue.DataAccessLayer.Specifications
{
    public class TaskCancellationSpecification : ISpecification<TaskInfo>
    {
        private int serverId;

        public TaskCancellationSpecification(int serverId)
        {
            if (serverId <= 0)
                throw new ArgumentOutOfRangeException(nameof(serverId));

            this.serverId = serverId;
        }

        #region Implementation of ISpecification<TaskInfo>

        public Expression<Func<TaskInfo, bool>> IsSatisfiedBy()
        {
            return x => x.ServerId == serverId && x.TaskState == TaskStates.Cancelling;
        }

        #endregion
    }
}
