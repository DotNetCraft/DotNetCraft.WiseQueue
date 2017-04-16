using System;
using System.Collections.Generic;
using DotNetCraft.Common.Domain.ServiceMessenger;
using DotNetCraft.WiseQueue.Core.Models;

namespace DotNetCraft.WiseQueue.Core.ServiceMessages
{
    public class ServerRegistrationMessage: BaseServiceMessage
    {
        public ServerDetails ServerDetails { get; private set; }

        public ServerRegistrationMessage(ServerDetails serverDetails,  object sender) : base(sender)
        {
            if (serverDetails == null)
                throw new ArgumentNullException(nameof(serverDetails));

            ServerDetails = serverDetails;
        }
    }
}
