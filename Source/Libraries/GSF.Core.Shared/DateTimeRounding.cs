//******************************************************************************************************
//  DateTimeRounding.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  11/17/2016 - Steven E. Chisholm
//       Generated original version of source code. 
//
//******************************************************************************************************

using System;
using System.Collections.Generic;

namespace GSF
{
    /// <summary>
    /// Responsible for rounding measurements to their nearest time bucket.
    /// For example: a 30 sample per second value of 5.1666766 would round to 5.1666667
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

        /// <summary>
        /// Rounds the specified timestamp
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public DateTime Round(DateTime time)
        {
            long ticks = time.Ticks;
            //Time rounded down to nearest second
            long ticksTruncatedToSeconds = ticks - ticks % TimeSpan.TicksPerSecond;
            //Compute the frame by taking the ticks after a second, adding 1/2 the interval value, then integer rounding
            int frame = (int)(ticks - ticksTruncatedToSeconds + m_ticksPerHalfInterval) / m_ticksPerInterval;
            return new DateTime(ticksTruncatedToSeconds + m_distribution[frame]);
        }

        /// <summary>
        /// Rounds the specified timestamp
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public long Round(long time)
        {
            long baseTicks = time - time % TimeSpan.TicksPerSecond;
            int frame = (int)(time - baseTicks + m_ticksPerHalfInterval) / m_ticksPerInterval;
            return baseTicks + m_distribution[frame];
        }

        private static Dictionary<int, DateTimeRounding> s_cache = new Dictionary<int, DateTimeRounding>();

        /// <summary>
        /// Creates a <see cref="DateTimeRounding"/>
        /// </summary>
        /// <param name="samplesPerSecond">the samples per second for the rounding algorithm</param>
        /// <returns></returns>
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
