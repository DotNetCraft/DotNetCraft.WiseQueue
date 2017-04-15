using DotNetCraft.Common.Core.DataAccessLayer;

namespace DotNetCraft.WiseQueue.Core.Configurations
{
    public class SqlSettings: IContextSettings
    {
        public string Name { get; set; }
        public string ConnectionString { get; set; }
    }
}
