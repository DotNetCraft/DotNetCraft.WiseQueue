using System;
using DotNetCraft.Common.Domain.ServiceMessenger;

namespace DotNetCraft.WiseQueue.Core.ServiceMessages
{
    public class ServerRegistrationMessage: BaseServiceMessage
    {
        public int ServerId { get; private set; }

        public ServerRegistrationMessage(int serverId, object sender) : base(sender)
        {
            if (serverId <= 0)
                throw new ArgumentOutOfRangeException(nameof(serverId));

            ServerId = serverId;
        }
    }
}
