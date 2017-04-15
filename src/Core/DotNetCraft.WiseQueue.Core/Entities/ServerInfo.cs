using System;
using System.Collections.Generic;

namespace DotNetCraft.WiseQueue.Core.Entities
{
    /// <summary>
    /// Server entity.
    /// </summary>
    public class ServerInfo: BaseWiseEntity
    {
        /// <summary>
        /// Server's name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Server's description.
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Date and time when server will be expired.
        /// </summary>
        public DateTime ServerExpiredTime { get; set; }

        /// <summary>
        /// List of queues that are supported by this server.
        /// </summary>
        public List<string> Queues { get; set; }
    }
}
