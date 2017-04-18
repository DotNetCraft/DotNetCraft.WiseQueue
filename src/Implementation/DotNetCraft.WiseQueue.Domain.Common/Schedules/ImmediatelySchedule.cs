using System;
using DotNetCraft.WiseQueue.Core.Managers.Tasks;

namespace DotNetCraft.WiseQueue.Domain.Common.Schedules
{
    public class ImmediatelySchedule : IScheduleData
    {
        #region Implementation of IScheduleData

        public ScheduleType ScheduleType { get; private set; }

        public ImmediatelySchedule()
        {
            ScheduleType = ScheduleType.Immediately;
        }

        public DateTime GetNextExecutionTime()
        {
            return DateTime.UtcNow;
        }

        #endregion
    }
}
