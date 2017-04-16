using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DotNetCraft.Common.Core.DataAccessLayer.Specofications;
using DotNetCraft.WiseQueue.Core.Entities;
using DotNetCraft.WiseQueue.Core.Entities.Enums;

namespace DotNetCraft.WiseQueue.DataAccessLayer.Specifications
{
    public class NewTaskSpecification: ISpecification<TaskInfo>
    {
        private readonly IReadOnlyCollection<string> queueNames;

        public NewTaskSpecification(IReadOnlyCollection<string> queueNames)
        {
            if (queueNames == null)
                throw new ArgumentNullException(nameof(queueNames));

            this.queueNames = queueNames;
        }

        #region Implementation of ISpecification<TaskInfo>

        public Expression<Func<TaskInfo, bool>> IsSatisfiedBy()
        {
            return x => x.TaskState == TaskStates.New && queueNames.Contains(x.QueueName);
        }

        #endregion
    }
}
