using System;
using DotNetCraft.Common.Core.BaseEntities;

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
