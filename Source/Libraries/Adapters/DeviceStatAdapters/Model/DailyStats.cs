using GSF.Data.Model;
using System;

namespace DeviceStatAdapters.Model
{
    public class DailyStats
    {
        [PrimaryKey(true)]
        public int ID { get; set; }

        public int DeviceID{ get; set; }

        public DateTime Timestamp { get; set; }

        public long ReceivedCount { get; set; }

        public long DataErrorCount { get; set; }

        public long TimeErrorCount { get; set; }

        public int MinLatency { get; set; }

        public int MaxLatency { get; set; }

        public int AvgLatency { get; set; }
    }
}
