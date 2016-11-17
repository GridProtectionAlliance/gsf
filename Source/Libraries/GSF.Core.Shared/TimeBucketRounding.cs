using System;
using System.Collections.Generic;

namespace GSF
{
    /// <summary>
    /// Responsible for rounding measurements to their nearest time bucket.
    /// For example. a 30 sample per second value of 5.1666766 would round to 5.1666667
    /// </summary>
    public class DateTimeRounding
    {
        private readonly int m_ticksPerInterval;
        private readonly int m_ticksPerHalfInterval;
        private readonly int[] m_distribution;

        private DateTimeRounding(int samplesPerSecond)
        {
            m_distribution = new int[samplesPerSecond + 1];
            double ticksPerInterval = (TimeSpan.TicksPerSecond / (double)samplesPerSecond);
            for (int i = 0; i < samplesPerSecond; i++)
                m_distribution[i] = (int)(Math.Round(i * ticksPerInterval));
            m_distribution[samplesPerSecond] = (int)TimeSpan.TicksPerSecond;

            m_ticksPerInterval = (int)(ticksPerInterval);
            m_ticksPerHalfInterval = m_ticksPerInterval / 2;
        }

        public DateTime Round(DateTime sourceTime)
        {
            long ticks = sourceTime.Ticks;
            //Time rounded down to nearest second
            long ticksTruncatedToSeconds = ticks - ticks % TimeSpan.TicksPerSecond;
            //Compute the frame by taking the ticks after a second, adding 1/2 the interval value, then integer rounding
            int frame = (int)(ticks - ticksTruncatedToSeconds + m_ticksPerHalfInterval) / m_ticksPerInterval;
            return new DateTime(ticksTruncatedToSeconds + m_distribution[frame]);
        }

        public long Round(long sourceTime)
        {
            long baseTicks = sourceTime - sourceTime % TimeSpan.TicksPerSecond;
            int frame = (int)(sourceTime - baseTicks + m_ticksPerHalfInterval) / m_ticksPerInterval;
            return baseTicks + m_distribution[frame];
        }

        private static Dictionary<int, DateTimeRounding> s_cache = new Dictionary<int, DateTimeRounding>();

        public static DateTimeRounding Create(int samplesPerSecond)
        {
            lock (s_cache)
            {
                DateTimeRounding rv;
                if (!s_cache.TryGetValue(samplesPerSecond, out rv))
                {
                    rv = new DateTimeRounding(samplesPerSecond);
                    s_cache.Add(samplesPerSecond, rv);
                }
                return rv;
            }
        }
    }
}
