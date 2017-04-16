using System;
using System.Linq.Expressions;
using DotNetCraft.Common.Core.DataAccessLayer.Specofications;
using DotNetCraft.WiseQueue.Core.Entities;
using DotNetCraft.WiseQueue.Core.Entities.Enums;

namespace DotNetCraft.WiseQueue.DataAccessLayer.Specifications
{
    public class RunningTaskByServerIdSpecification : ISpecification<TaskInfo>
    {
        private readonly int serverId;

        public RunningTaskByServerIdSpecification(int serverId)
        {
            this.serverId = serverId;
        }

        #region Implementation of ISpecification<TaskInfo>

        public Expression<Func<TaskInfo, bool>> IsSatisfiedBy()
        {
            return x => x.ServerId == serverId && x.TaskState == TaskStates.Running;
        }

        #endregion
    }
}
