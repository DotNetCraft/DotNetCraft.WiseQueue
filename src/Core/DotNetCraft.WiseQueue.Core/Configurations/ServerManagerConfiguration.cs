using System;
using System.Collections.Generic;
using DotNetCraft.Common.Core.Domain.Management;

namespace DotNetCraft.WiseQueue.Core.Configurations
{
    public class ServerManagerConfiguration: IBackgroundManagerConfiguration
    {
        #region Implementation of IManagerConfiguration

        public string Name { get; set; }

        #endregion

        #region Implementation of IBackgroundManagerConfiguration

        public bool StartImmediately { get; set; }

        public TimeSpan SleepTime { get; set; }

        #endregion

        public TimeSpan ServerHeartbeat { get; set; }

        public List<string> Queues { get; set; }
    }
}
