using DotNetCraft.Common.Core.DataAccessLayer.Repositories;
using DotNetCraft.WiseQueue.Core.Entities;

namespace DotNetCraft.WiseQueue.Core.Repositories
{
    public interface ITaskRepository : IIntRepository<TaskInfo>
    {
    }
}
