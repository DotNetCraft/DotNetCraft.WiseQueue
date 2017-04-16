﻿using System;
using System.Collections.Generic;
using DotNetCraft.Common.Core.Attributes;
using DotNetCraft.Common.Core.Domain.Management;

namespace DotNetCraft.WiseQueue.Core.Configurations
{
    public class ServerManagerConfiguration: IBackgroundManagerConfiguration
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

        public List<string> Queues { get; set; }
    }
}
