namespace DotNetCraft.WiseQueue.Core.Configurations
{
    public class SystemConfiguration
    {
        public SqlSettings SqlSettings { get; set; }

        public ServerManagerConfiguration ServerManagerConfiguration { get; set; }

        public TaskManagerConfiguration TaskManagerConfiguration { get; set; }

        public ClientManagerConfiguration ClientManagerConfiguration { get; set; }        
    }
}
