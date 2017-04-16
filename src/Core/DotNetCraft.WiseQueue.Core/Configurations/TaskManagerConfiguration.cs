using System;
using DotNetCraft.Common.Core.Domain.Management;

namespace DotNetCraft.WiseQueue.Core.Configurations
{
    public class TaskManagerConfiguration : IBackgroundManagerConfiguration
    {
        #region Implementation of IManagerConfiguration

        public string Name { get; set; }

        #endregion

        #region Implementation of IBackgroundManagerConfiguration

        public bool StartImmediately { get; set; }

        public TimeSpan SleepTime { get; set; }

        #endregion        

        public TaskProcessingConfiguration TaskProcessingConfiguration { get; set; }
    }
}
