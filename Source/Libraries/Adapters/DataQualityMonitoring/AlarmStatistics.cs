//******************************************************************************************************
//  AlarmStatistics.cs - Gbtc
//
//  Copyright © 2014, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  10/14/2014 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Collections.Generic;
using GSF.Collections;
using GSF.TimeSeries;
using GSF.TimeSeries.Statistics;

namespace DataQualityMonitoring
{
    /// <summary>
    /// Class used to gather statistics related to alarms and provide them to the statistics engine.
    /// </summary>
    public class AlarmStatistics
    {
        #region [ Members ]

        // Fields
        private Dictionary<int, long> m_countBySeverity;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="AlarmStatistics"/> class.
        /// </summary>
        public AlarmStatistics()
        {
            m_countBySeverity = new Dictionary<int, long>();
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Increments the counter for the given alarm.
        /// </summary>
        /// <param name="alarm">The alarm that was raised.</param>
        public void IncrementCounter(Alarm alarm)
        {
            m_countBySeverity.AddOrUpdate((int)alarm.Severity, severity => 1L, (severity, count) => count + 1L);
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static StatisticValueStateCache s_statisticValueCache = new StatisticValueStateCache();

        // Static Methods

        // Gets the alarm count for a specific signal at a specific severity.
        private static double GetPointStatistic_MeasurementCountForSeverity(object source, string arguments)
        {
            double statistic = 0.0D;

            AlarmStatistics alarmStatistics = source as AlarmStatistics;
            int severity;
            long count;

            if ((object)alarmStatistics != null && int.TryParse(arguments, out severity) && alarmStatistics.m_countBySeverity.TryGetValue(severity, out count))
                statistic = s_statisticValueCache.GetDifference(alarmStatistics, count, arguments);

            return statistic;
        }

        #endregion
    }
}
