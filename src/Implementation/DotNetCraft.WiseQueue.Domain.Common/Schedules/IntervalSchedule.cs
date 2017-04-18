using System;
using DotNetCraft.WiseQueue.Core.Managers.Tasks;

namespace DotNetCraft.WiseQueue.Domain.Common.Schedules
{
    public class IntervalSchedule: IScheduleData
    {
        #region Implementation of IScheduleData

        public ScheduleType ScheduleType { get; }
        public DateTime GetNextExecutionTime()
        {
            return DateTime.UtcNow.AddMinutes(1);
        }

        #endregion
    }
}
