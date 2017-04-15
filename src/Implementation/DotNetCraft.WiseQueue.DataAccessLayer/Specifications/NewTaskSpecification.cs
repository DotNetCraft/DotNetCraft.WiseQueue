using System;
using System.Linq.Expressions;
using DotNetCraft.Common.Core.DataAccessLayer.Specofications;
using DotNetCraft.WiseQueue.Core.Entities;
using DotNetCraft.WiseQueue.Core.Entities.Enums;

namespace DotNetCraft.WiseQueue.DataAccessLayer.Specifications
{
    public class NewTaskSpecification: ISpecification<TaskInfo>
    {
        #region Implementation of ISpecification<TaskInfo>

        public Expression<Func<TaskInfo, bool>> IsSatisfiedBy()
        {
            return x => x.TaskState == TaskStates.New;
        }

        #endregion
    }
}
