using DotNetCraft.Common.Domain.ServiceMessenger;
using DotNetCraft.WiseQueue.Core.Entities;

namespace DotNetCraft.WiseQueue.Core.ServiceMessages
{
    public class TaskReceivedMessage: BaseServiceMessage
    {
        public TaskInfo TaskInfo { get; private set; }

        public TaskReceivedMessage(TaskInfo taskInfo, object sender) : base(sender)
        {
            TaskInfo = taskInfo;
        }
    }
}
