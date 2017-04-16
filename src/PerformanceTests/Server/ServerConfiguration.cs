using DotNetCraft.WiseQueue.Core.Configurations;

namespace Server
{
    class ServerConfiguration
    {
        public SqlSettings SqlSettings { get; set; }

        public ServerManagerConfiguration ServerManagerConfiguration { get; set; }

        public TaskManagerConfiguration TaskManagerConfiguration { get; set; }
    }
}
