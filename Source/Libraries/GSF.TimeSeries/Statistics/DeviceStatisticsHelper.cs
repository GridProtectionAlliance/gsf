//******************************************************************************************************
//  DeviceWrapper.cs - Gbtc
//
//  Copyright © 2014, Grid Protection Alliance.  All Rights Reserved.
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
//  02/24/2014 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GSF.TimeSeries.Statistics
{
    internal static class GlobalDeviceStatistics
    {
        private class LatestDeviceTime
        {
            public long Ticks;
        }

        private static readonly ConcurrentDictionary<IDevice, LatestDeviceTime> s_latestDeviceTimes;
        private static Dictionary<IDevice, long> s_deviceTimesSnapshot;
        private static long s_snapshotTime;

        static GlobalDeviceStatistics()
        {
            s_latestDeviceTimes = new ConcurrentDictionary<IDevice, LatestDeviceTime>();
            StatisticsEngine.SourceRegistered += StatisticsEngine_SourceRegistered;
            StatisticsEngine.SourceUnregistered += StatisticsEngine_SourceUnregistered;
            StatisticsEngine.BeforeCalculate += StatisticsEngine_BeforeCalculate;
        }

        public static long AverageTime { get; private set; }

        public static long MinimumTime { get; private set; }

        public static long MaximumTime { get; private set; }

        public static long GetDeviceTimeDeviationFromAverage(IDevice device) => 
            s_deviceTimesSnapshot.TryGetValue(device, out long currentDeviceTime) ? AverageTime - currentDeviceTime : 0L;

        public static long GetLocalClockTimeDeviationFromAverage() => 
            AverageTime - s_snapshotTime;

        public static void MarkDeviceTimestamp(IDevice device, long ticks)
        {
            if (s_latestDeviceTimes.TryGetValue(device, out LatestDeviceTime latestDeviceTime))
                latestDeviceTime.Ticks = ticks;
        }

        private static void StatisticsEngine_SourceRegistered(object sender, EventArgs<object> e)
        {
            if (e.Argument is IDevice device)
                s_latestDeviceTimes[device] = new LatestDeviceTime();
        }

        private static void StatisticsEngine_SourceUnregistered(object sender, EventArgs<object> e)
        {
            if (e.Argument is IDevice device)
                s_latestDeviceTimes.TryRemove(device, out _);
        }

        private static void StatisticsEngine_BeforeCalculate(object sender, EventArgs e)
        {
            s_deviceTimesSnapshot = s_latestDeviceTimes.ToArray().ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Ticks);
            s_snapshotTime = DateTime.UtcNow.Ticks;
            
            long[] deviceTimes = s_deviceTimesSnapshot.Select(kvp => kvp.Value).Where(ticks => ticks > 0L).ToArray();

            AverageTime = (long)deviceTimes.Average();
            MinimumTime = deviceTimes.Min();
            MaximumTime = deviceTimes.Max();
        }
    }

    /// <summary>
    /// Helper class for calculating device statistics.
    /// </summary>
    /// <typeparam name="T">The type of the devices whose statistics are to be calculated.</typeparam>
    public class DeviceStatisticsHelper<T> where T : class, IDevice
    {
        #region [ Members ]

        // Fields
        private long m_lastUpdateTicks;
        private int m_measurementsInSecond;
        private int m_errorsInSecond;
        private double m_expectedMeasurements;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="DeviceStatisticsHelper{T}"/> class.
        /// </summary>
        /// <param name="device">The device whose statistics are to be calculated using this helper.</param>
        public DeviceStatisticsHelper(T device)
        {
            Device = device;
            m_lastUpdateTicks = DateTime.UtcNow.Ticks;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the device whose statistics are being calculated using this helper.
        /// </summary>
        public T Device { get; }

        /// <summary>
        /// Gets or sets the number of measurements expected to be received from the device per second.
        /// </summary>
        public int ExpectedMeasurementsPerSecond { get; set; }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Increases the count of the number of measurements received from the device.
        /// </summary>
        /// <param name="count">The number of measurements received from the device since the last time this method was called.</param>
        /// <remarks>
        /// Call this each time measurements have been received by the device in order
        /// to properly track the <see cref="IDevice.MeasurementsReceived"/> statistic.
        /// </remarks>
        public void AddToMeasurementsReceived(int count) => 
            Interlocked.Add(ref m_measurementsInSecond, count);

        /// <summary>
        /// Increases the count of the number of measurements received while the device is reporting errors.
        /// </summary>
        /// <param name="count">The number of measurements received while the device is reporting errors since the last time this method was called.</param>
        /// <remarks>
        /// Call this each time measurements with errors have been received by the device in order
        /// to properly track the <see cref="IDevice.MeasurementsWithError"/> statistic.
        /// </remarks>
        public void AddToMeasurementsWithError(int count) => 
            Interlocked.Add(ref m_errorsInSecond, count);

        /// <summary>
        /// Marks the latest timestamp of device measurements used to calculate global timestamp statistics.
        /// </summary>
        /// <param name="ticks">Latest timestamp of device measurements.</param>
        public void MarkDeviceTimestamp(long ticks) => 
            GlobalDeviceStatistics.MarkDeviceTimestamp(Device, ticks);

        /// <summary>
        /// Updates the statistics for the number of measurements received and the number
        /// of measurements expected in the <see cref="IDevice"/> wrapped by this helper.
        /// </summary>
        /// <remarks>
        /// Call this periodically, preferably on a slow timer (e.g., once per second),
        /// in order to update the <see cref="IDevice.MeasurementsReceived"/>,
        /// <see cref="IDevice.MeasurementsWithError"/>, and
        /// <see cref="IDevice.MeasurementsExpected"/> statistics.
        /// </remarks>
        public void Update() => 
            Update(DateTime.UtcNow.Ticks);

        /// <summary>
        /// Updates the statistics for the number of measurements received and the number
        /// of measurements expected in the <see cref="IDevice"/> wrapped by this helper.
        /// </summary>
        /// <param name="nowTicks">The current time, in ticks.</param>
        /// <remarks>
        /// Call this periodically, preferably on a slow timer (e.g., once per second),
        /// in order to update the <see cref="IDevice.MeasurementsReceived"/>,
        /// <see cref="IDevice.MeasurementsWithError"/>, and
        /// <see cref="IDevice.MeasurementsExpected"/> statistics. This method is preferred
        /// when tracking statistics for multiple <see cref="IDevice"/>s to reduce the number
        /// of calls to <see cref="DateTime.UtcNow"/>.
        /// </remarks>
        public void Update(long nowTicks)
        {
            int measurementsInSecond = Interlocked.Exchange(ref m_measurementsInSecond, 0);
            int errorsInSecond = Interlocked.Exchange(ref m_errorsInSecond, 0);

            if (m_lastUpdateTicks > 0L)
            {
                double diff = (nowTicks - m_lastUpdateTicks) / (double)Ticks.PerSecond;
                m_expectedMeasurements += diff * ExpectedMeasurementsPerSecond;
                long expectedMeasurementsInSecond = (long)m_expectedMeasurements;

                Device.MeasurementsReceived += measurementsInSecond;
                Device.MeasurementsWithError += errorsInSecond;
                Device.MeasurementsExpected += expectedMeasurementsInSecond;

                m_expectedMeasurements -= expectedMeasurementsInSecond;
            }

            m_lastUpdateTicks = nowTicks;
        }

        /// <summary>
        /// Resets the member variables used to track
        /// statistics for the device wrapped by this helper.
        /// </summary>
        /// <remarks>
        /// Call this when the connection to a device has been
        /// reset to mark a starting point for statistics gathering.
        /// Since the <see cref="Update(long)"/> method keeps track
        /// of the time it was last called, it is necessary to call
        /// this method during long periods of downtime where Update
        /// was not being called in order to avoid momentary, unexpectedly
        /// large values to be calculated for measurements expected.
        /// </remarks>
        public void Reset() => 
            Reset(DateTime.UtcNow.Ticks);

        /// <summary>
        /// Resets the member variables used to track
        /// statistics for the device wrapped by this helper.
        /// </summary>
        /// <param name="nowTicks">The current time, in ticks.</param>
        /// <remarks>
        /// Call this when the connection to a device has been
        /// reset to mark a starting point for statistics gathering.
        /// Since the <see cref="Update(long)"/> method keeps track
        /// of the time it was last called, it is necessary to call
        /// this method during long periods of downtime where Update
        /// was not being called in order to avoid momentary, unexpectedly
        /// large values to be calculated for measurements expected.
        /// This method is preferred when tracking statistics for multiple
        /// <see cref="IDevice"/>s to reduce the number of calls to
        /// <see cref="DateTime.UtcNow"/>.
        /// </remarks>
        public void Reset(long nowTicks)
        {
            Interlocked.Exchange(ref m_measurementsInSecond, 0);
            m_expectedMeasurements = 0.0D;
            m_lastUpdateTicks = nowTicks;
        }

        #endregion
    }
}
