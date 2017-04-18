using System;
using DotNetCraft.WiseQueue.Core.Managers.Tasks;

namespace DotNetCraft.WiseQueue.Domain.Common.Schedules
{
    public class ExecuteAtSchedule: IScheduleData
    {
        private readonly DateTime executeAt;

        #region Implementation of IScheduleData

        public ScheduleType ScheduleType { get; }

        public ExecuteAtSchedule(DateTime executeAt)
        {
            this.executeAt = executeAt;
        }

        private ExecuteAtSchedule() { }

        public DateTime GetNextExecutionTime()
        {
            return executeAt;
        }

        #endregion
    }
}
