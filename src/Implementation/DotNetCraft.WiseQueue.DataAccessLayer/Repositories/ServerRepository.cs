using DotNetCraft.Common.Core.DataAccessLayer;
using DotNetCraft.Common.DataAccessLayer.Repositories.Simple;
using DotNetCraft.WiseQueue.Core.Entities;
using DotNetCraft.WiseQueue.Core.Repositories;

namespace DotNetCraft.WiseQueue.DataAccessLayer.Repositories
{
    public class ServerRepository: BaseIntRepository<ServerInfo>, IServerRepository
    {
        public ServerRepository(IContextSettings contextSettings, IDataContextFactory dataContextFactory) : base(contextSettings, dataContextFactory)
        {
        }        
    }
}
