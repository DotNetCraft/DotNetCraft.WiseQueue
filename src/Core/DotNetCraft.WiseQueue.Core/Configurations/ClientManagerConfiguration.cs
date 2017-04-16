using System;
using DotNetCraft.Common.Core.Attributes;
using DotNetCraft.Common.Core.Domain.Management;

namespace DotNetCraft.WiseQueue.Core.Configurations
{
    public class ClientManagerConfiguration : IBackgroundManagerConfiguration
    {
        #region Implementation of IManagerConfiguration

        [FieldToString]
        public string Name { get; set; }

        #endregion

        #region Implementation of IBackgroundManagerConfiguration

        [FieldToString]
        public bool StartImmediately { get; set; }

        [FieldToString]
        public TimeSpan SleepTime { get; set; }

        #endregion

        [FieldToString]
        public TimeSpan ServerHeartbeat { get; set; }
    }
}
