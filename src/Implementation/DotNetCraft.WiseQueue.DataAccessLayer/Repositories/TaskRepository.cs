using DotNetCraft.Common.Core.DataAccessLayer;
using DotNetCraft.Common.DataAccessLayer.Repositories.Simple;
using DotNetCraft.WiseQueue.Core.Entities;
using DotNetCraft.WiseQueue.Core.Repositories;

namespace DotNetCraft.WiseQueue.DataAccessLayer.Repositories
{
    public class TaskRepository: BaseIntRepository<TaskInfo>, ITaskRepository
    {
        public TaskRepository(IContextSettings contextSettings, IDataContextFactory dataContextFactory) : base(contextSettings, dataContextFactory)
        {
        }
    }
}
