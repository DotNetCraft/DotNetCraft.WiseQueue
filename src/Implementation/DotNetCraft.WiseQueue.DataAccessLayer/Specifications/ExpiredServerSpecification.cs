using System;
using System.Linq.Expressions;
using DotNetCraft.Common.Core.DataAccessLayer.Specifications;
using DotNetCraft.WiseQueue.Core.Entities;

namespace DotNetCraft.WiseQueue.DataAccessLayer.Specifications
{
    public class ExpiredServerSpecification: ISpecification<ServerInfo>
    {
        #region Implementation of ISpecification<ServerInfo>

        public Expression<Func<ServerInfo, bool>> IsSatisfiedBy()
        {
            return x => x.ServerExpiredTime < DateTime.UtcNow;
        }

        #endregion
    }
}
