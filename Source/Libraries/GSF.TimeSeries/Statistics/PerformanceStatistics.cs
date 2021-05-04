//******************************************************************************************************
//  PerformanceStatistics.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  02/24/2012 - Stephen C. Wills
//       Generated original version of source code.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using GSF.Diagnostics;
using GSF.IO;
using static GSF.Common;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local
namespace GSF.TimeSeries.Statistics
{
    internal static class PerformanceStatistics
    {
        private static readonly double s_totalPhysicalMemory;

        // Run-time log used to calculate system run-time
        internal static RunTimeLog SystemRunTimeLog;

        static PerformanceStatistics()
        {
            try
            {
                // Total physical memory not expected to change over process lifetime
                s_totalPhysicalMemory = GetTotalPhysicalMemory();
            }
            catch (Exception ex)
            {
                Logger.SwallowException(ex);
                s_totalPhysicalMemory = double.NaN;
            }
        }

        #region [ CPU Usage ]

        private static double GetSystemStatistic_CPUUsage(object source, string _) => 
            source is PerformanceMonitor perfMon ? perfMon.CPUUsage?.LastValue ?? double.NaN : double.NaN;

        private static double GetSystemStatistic_AverageCPUUsage(object source, string _) =>
            source is PerformanceMonitor perfMon ? perfMon.CPUUsage?.AverageValue ?? double.NaN : double.NaN;

        private static double GetSystemStatistic_SystemCPUUsage(object source, string _) =>
            SystemPerformanceMonitor.Default?.CPUUsage?.LastValue ?? double.NaN;

        private static double GetSystemStatistic_AverageSystemCPUUsage(object source, string _) =>
            SystemPerformanceMonitor.Default?.CPUUsage?.AverageValue ?? double.NaN;

        #endregion

        #region [ Memory Usage ]

        private static double GetSystemStatistic_MemoryUsage(object source, string _) =>
            source is PerformanceMonitor perfMon ? perfMon.MemoryUsage?.LastValue ?? double.NaN : double.NaN;

        private static double GetSystemStatistic_AverageMemoryUsage(object source, string _) =>
            source is PerformanceMonitor perfMon ? perfMon.MemoryUsage?.AverageValue ?? double.NaN : double.NaN;

        private static double GetSystemStatistic_AvailableSystemMemory(object source, string _) => 
            SystemPerformanceMonitor.Default?.AvailableMemory?.LastValue ?? double.NaN;

        private static double GetSystemStatistic_AverageAvailableSystemMemory(object source, string _) =>
            SystemPerformanceMonitor.Default?.AvailableMemory?.AverageValue ?? double.NaN;

        private static double GetSystemStatistic_SystemMemoryUsage(object source, string _)
        {
            try
            {
                return (s_totalPhysicalMemory - GetAvailablePhysicalMemory()) / s_totalPhysicalMemory * 100.0D;
            }
            catch (Exception ex)
            {
                Logger.SwallowException(ex);
                return double.NaN;
            }
        }

        #endregion

        #region [ Thread Count ]

        private static double GetSystemStatistic_ThreadCount(object source, string _) =>
            source is PerformanceMonitor perfMon ? perfMon.ThreadCount?.LastValue ?? double.NaN : double.NaN;

        private static double GetSystemStatistic_AverageThreadCount(object source, string _) =>
            source is PerformanceMonitor perfMon ? perfMon.ThreadCount?.AverageValue ?? double.NaN : double.NaN;

        #endregion

        #region [ Threading Contention Rate ]

        private static double GetSystemStatistic_ThreadingContentionRate(object source, string _) =>
            source is PerformanceMonitor perfMon ? perfMon.ThreadingContentionRate?.LastValue ?? double.NaN : double.NaN;

        private static double GetSystemStatistic_AverageThreadingContentionRate(object source, string _) =>
            source is PerformanceMonitor perfMon ? perfMon.ThreadingContentionRate?.AverageValue ?? double.NaN : double.NaN;

        #endregion

        #region [ IO Usage ]

        private static double GetSystemStatistic_IOUsage(object source, string _) =>
            source is PerformanceMonitor perfMon ? perfMon.IOUsage?.LastValue ?? double.NaN : double.NaN;

        private static double GetSystemStatistic_AverageIOUsage(object source, string _) =>
            source is PerformanceMonitor perfMon ? perfMon.IOUsage?.AverageValue ?? double.NaN : double.NaN;

        #endregion

        #region [ IP Data Send Rate ]

        private static double GetSystemStatistic_IPDataSendRate(object source, string _) =>
            source is PerformanceMonitor perfMon ? perfMon.IPDataSendRate?.LastValue ?? double.NaN : double.NaN;

        // Historical name - function may be used by older database configurations
        private static double GetSystemStatistic_DatagramSendRate(object source, string arguments) =>
            GetSystemStatistic_IPDataSendRate(source, arguments);

        private static double GetSystemStatistic_AverageIPDataSendRate(object source, string _) =>
            source is PerformanceMonitor perfMon ? perfMon.IPDataSendRate?.AverageValue ?? double.NaN : double.NaN;

        // Historical name - function may be used by older database configurations
        private static double GetSystemStatistic_AverageDatagramSendRate(object source, string arguments) => 
            GetSystemStatistic_AverageIPDataSendRate(source, arguments);

        #endregion

        #region [ IP Data Receive Rate ]

        private static double GetSystemStatistic_IPDataReceiveRate(object source, string _) =>
            source is PerformanceMonitor perfMon ? perfMon.IPDataReceiveRate?.LastValue ?? double.NaN : double.NaN;

        // Historical name - function may be used by older database configurations
        private static double GetSystemStatistic_DatagramReceiveRate(object source, string arguments) =>
            GetSystemStatistic_IPDataReceiveRate(source, arguments);

        private static double GetSystemStatistic_AverageIPDataReceiveRate(object source, string _) =>
            source is PerformanceMonitor perfMon ? perfMon.IPDataReceiveRate?.AverageValue ?? double.NaN : double.NaN;

        // Historical name - function may be used by older database configurations
        private static double GetSystemStatistic_AverageDatagramReceiveRate(object source, string arguments) =>
            GetSystemStatistic_AverageIPDataReceiveRate(source, arguments);

        #endregion

        #region [ System Times ]

        private static double GetSystemStatistic_UpTime(object source, string _) => 
            !(SystemRunTimeLog?.IsDisposed ?? true) ? SystemRunTimeLog.UpTime.TotalSeconds : double.NaN;

        private static double GetSystemStatistic_AverageDeviceTime(object source, string _) =>
            (double)new UnixTimeTag(GlobalDeviceStatistics.AverageTime).Value;

        private static double GetSystemStatistic_MinimumDeviceTime(object source, string _) =>
            (double)new UnixTimeTag(GlobalDeviceStatistics.MinimumTime).Value;

        private static double GetSystemStatistic_MaximumDeviceTime(object source, string _) =>
            (double)new UnixTimeTag(GlobalDeviceStatistics.MaximumTime).Value;

        private static double GetSystemStatistic_SystemTimeDeviationFromAverage(object source, string _) =>
            new Ticks(GlobalDeviceStatistics.GetSystemTimeDeviationFromAverage()).ToSeconds();

        #endregion
    }
}
