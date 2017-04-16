using DotNetCraft.WiseQueue.Core.Configurations;

namespace Client
{
    class ClientConfiguration
    {
        public SqlSettings SqlSettings { get; set; }
        public ClientManagerConfiguration ClientManagerConfiguration { get; set; }
    }
}
