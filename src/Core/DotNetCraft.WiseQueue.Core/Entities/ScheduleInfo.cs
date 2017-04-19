﻿using System;
using DotNetCraft.WiseQueue.Core.Managers.Tasks;
using DotNetCraft.WiseQueue.Core.Managers.Tasks.ScheduleStrategy;

namespace DotNetCraft.WiseQueue.Core.Entities
{
    public class ScheduleInfo: BaseWiseEntity
    {
        public ScheduleType ScheduleType { get; set; }

        public string ScheduleData { get; set; }
        public string ScheduleDataType { get; set; }
    }
}
