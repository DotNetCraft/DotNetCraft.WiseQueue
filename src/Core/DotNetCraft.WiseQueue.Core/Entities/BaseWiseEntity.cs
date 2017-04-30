using System;
using DotNetCraft.Common.Core.BaseEntities;
using DotNetCraft.Common.DataAccessLayer.BaseEntities;

namespace DotNetCraft.WiseQueue.Core.Entities
{
    public abstract class BaseWiseEntity: BaseIntEntity
    {
        DateTime CreatedDate { get; set; }

        protected BaseWiseEntity()
        {
            CreatedDate = DateTime.UtcNow;
        }
    }
}
