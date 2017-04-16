using System;
using System.Collections.Generic;

namespace DotNetCraft.WiseQueue.Core.Models
{
    public class ServerDetails
    {
        public int ServerId { get; private set; }
        public IReadOnlyCollection<string> QueueNames { get; private set; }

        public bool IsOnline
        {
            get { return ServerId > 0; }
        }

        public ServerDetails(int serverId, List<string> queueNames)
        {
            if (queueNames == null)
                throw new ArgumentNullException(nameof(queueNames));
            if (serverId < 0)
                throw new ArgumentOutOfRangeException(nameof(serverId));

            ServerId = serverId;
            QueueNames = queueNames;
        }
    }
}
