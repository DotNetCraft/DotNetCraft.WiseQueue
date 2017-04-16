using DotNetCraft.WiseQueue.Core.Entities;

namespace DotNetCraft.WiseQueue.Core.Managers.Tasks
{
    public interface ITaskBuilder
    {
        IRunningTask Build(TaskInfo taskInfo);
    }
}
