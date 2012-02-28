//******************************************************************************************************
//  PerformanceStatistics.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
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
//  02/24/2012 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using TVA.Diagnostics;

namespace TimeSeriesFramework.Statistics
{
    internal static class PerformanceStatistics
    {
        #region [ CPU Usage ]

        private static double GetSystemStatistic_CPUUsage(object source, string arguments)
        {
            double statistic = 0.0;
            PerformanceMonitor perfMon = source as PerformanceMonitor;

            if ((object)perfMon != null)
                statistic = perfMon.CPUUsage.LastValue;

            return statistic;
        }

        private static double GetSystemStatistic_AverageCPUUsage(object source, string arguments)
        {
            double statistic = 0.0;
            PerformanceMonitor perfMon = source as PerformanceMonitor;

            if ((object)perfMon != null)
                statistic = perfMon.CPUUsage.AverageValue;

            return statistic;
        }

        #endregion

        #region [ Memory Usage ]

        private static double GetSystemStatistic_MemoryUsage(object source, string arguments)
        {
            double statistic = 0.0;
            PerformanceMonitor perfMon = source as PerformanceMonitor;

            if ((object)perfMon != null)
                statistic = perfMon.MemoryUsage.LastValue;

            return statistic;
        }

        private static double GetSystemStatistic_AverageMemoryUsage(object source, string arguments)
        {
            double statistic = 0.0;
            PerformanceMonitor perfMon = source as PerformanceMonitor;

            if ((object)perfMon != null)
                statistic = perfMon.MemoryUsage.AverageValue;

            return statistic;
        }

        #endregion

        #region [ Thread Count ]

        private static double GetSystemStatistic_ThreadCount(object source, string arguments)
        {
            double statistic = 0.0;
            PerformanceMonitor perfMon = source as PerformanceMonitor;

            if ((object)perfMon != null)
                statistic = perfMon.ThreadCount.LastValue;

            return statistic;
        }

        private static double GetSystemStatistic_AverageThreadCount(object source, string arguments)
        {
            double statistic = 0.0;
            PerformanceMonitor perfMon = source as PerformanceMonitor;

            if ((object)perfMon != null)
                statistic = perfMon.ThreadCount.AverageValue;

            return statistic;
        }

        #endregion

        #region [ Threading Contention Rate ]

        private static double GetSystemStatistic_ThreadingContentionRate(object source, string arguments)
        {
            double statistic = 0.0;
            PerformanceMonitor perfMon = source as PerformanceMonitor;

            if ((object)perfMon != null)
                statistic = perfMon.ThreadingContentionRate.LastValue;

            return statistic;
        }

        private static double GetSystemStatistic_AverageThreadingContentionRate(object source, string arguments)
        {
            double statistic = 0.0;
            PerformanceMonitor perfMon = source as PerformanceMonitor;

            if ((object)perfMon != null)
                statistic = perfMon.ThreadingContentionRate.AverageValue;

            return statistic;
        }

        #endregion

        #region [ IO Usage ]

        private static double GetSystemStatistic_IOUsage(object source, string arguments)
        {
            double statistic = 0.0;
            PerformanceMonitor perfMon = source as PerformanceMonitor;

            if ((object)perfMon != null)
                statistic = perfMon.IOUsage.LastValue;

            return statistic;
        }

        private static double GetSystemStatistic_AverageIOUsage(object source, string arguments)
        {
            double statistic = 0.0;
            PerformanceMonitor perfMon = source as PerformanceMonitor;

            if ((object)perfMon != null)
                statistic = perfMon.IOUsage.AverageValue;

            return statistic;
        }

        #endregion

        #region [ Datagram Send Rate ]

        private static double GetSystemStatistic_DatagramSendRate(object source, string arguments)
        {
            double statistic = 0.0;
            PerformanceMonitor perfMon = source as PerformanceMonitor;

            if ((object)perfMon != null)
                statistic = perfMon.DatagramSendRate.LastValue;

            return statistic;
        }

        private static double GetSystemStatistic_AverageDatagramSendRate(object source, string arguments)
        {
            double statistic = 0.0;
            PerformanceMonitor perfMon = source as PerformanceMonitor;

            if ((object)perfMon != null)
                statistic = perfMon.DatagramSendRate.AverageValue;

            return statistic;
        }

        #endregion

        #region [ Datagram Receive Rate ]

        private static double GetSystemStatistic_DatagramReceiveRate(object source, string arguments)
        {
            double statistic = 0.0;
            PerformanceMonitor perfMon = source as PerformanceMonitor;

            if ((object)perfMon != null)
                statistic = perfMon.DatagramReceiveRate.LastValue;

            return statistic;
        }

        private static double GetSystemStatistic_AverageDatagramReceiveRate(object source, string arguments)
        {
            double statistic = 0.0;
            PerformanceMonitor perfMon = source as PerformanceMonitor;

            if ((object)perfMon != null)
                statistic = perfMon.DatagramReceiveRate.AverageValue;

            return statistic;
        }

        #endregion
    }
}
