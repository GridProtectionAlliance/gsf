//******************************************************************************************************
//  TimestampTest.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  12/18/2009 - Stephen C. Wills
//       Generated original version of source code.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modifed Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Timers;
using DataQualityMonitoring.Services;
using GSF;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;

namespace DataQualityMonitoring
{
    /// <summary>
    /// Tests measurements to determine whether their timestamps are good or bad.
    /// </summary>
    [Description("Timestamp Test: notifies when the timestamps of incoming measurements are bad")]
    public class TimestampTest : FacileActionAdapterBase
    {
        #region [ Members ]

        // Fields
        private readonly Dictionary<Ticks, LinkedList<IMeasurement>> m_badTimestampMeasurements;
        private IActionAdapter m_discardingAdapter;
        private int m_totalBadTimestampMeasurements;
        private Ticks m_timeToPurge;
        private Ticks m_warnInterval;
        private Timer m_purgeTimer;
        private Timer m_warningTimer;
        private TimestampService m_timestampService;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="TimestampTest"/> class.
        /// </summary>
        public TimestampTest()
        {
            m_badTimestampMeasurements = new Dictionary<Ticks, LinkedList<IMeasurement>>();
            m_timeToPurge = Ticks.FromSeconds(1.0);
            m_warnInterval = Ticks.FromSeconds(4.0);
            m_purgeTimer = new Timer();
            m_warningTimer = new Timer();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the age, in seconds, at which measurements with bad timestamps are purged from the adapter.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the age, in seconds, at which measurements with bad timestamps are purged from the adapter."),
        DefaultValue(1.0)]
        public double TimeToPurge
        {
            get
            {
                return m_timeToPurge.ToSeconds();
            }
            set
            {
                m_timeToPurge = Ticks.FromSeconds(value);
            }
        }

        /// <summary>
        /// Gets or sets the amount of time, in seconds, between console updates.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the amount of time, in seconds, between console updates."),
        DefaultValue(4.0)]
        public double WarnInterval
        {
            get
            {
                return m_warnInterval.ToSeconds();
            }
            set
            {
                m_warnInterval = Ticks.FromSeconds(value);
            }
        }

        /// <summary>
        /// Gets or sets the name of the concentrator whose measurements will be checked for bad timestamps.
        /// Modifying this property will cause this adapter to search for an action adapter with the name
        /// that was supplied to the property. If one is found, it will detach from the current adapter and
        /// attach to the other. If one is not found, nothing will happen.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the name of the concentrator whose measurements will be checked for bad timestamps.")]
        public string ConcentratorName
        {
            get
            {
                return m_discardingAdapter.Name;
            }
            set
            {
                // Detach to the current adapter's discarding measurements and disposed events.
                m_discardingAdapter.DiscardingMeasurements -= m_discardingAdapter_DiscardingMeasurements;
                m_discardingAdapter.Disposed -= m_discardingAdapter_Disposed;

                // Find the adapter whose name matches the specified concentratorName
                foreach (IAdapter adapter in Parent)
                {
                    IActionAdapter concentrator = adapter as IActionAdapter;

                    if (concentrator != null && string.Compare(adapter.Name, value, true) == 0)
                    {
                        m_discardingAdapter = concentrator;
                        break;
                    }
                }

                // Attach to adapter's discarding measurements and disposed events
                m_discardingAdapter.DiscardingMeasurements += m_discardingAdapter_DiscardingMeasurements;
                m_discardingAdapter.Disposed += m_discardingAdapter_Disposed;
            }
        }

        /// <summary>
        /// Gets the flag indicating if this adapter supports temporal processing.
        /// </summary>
        public override bool SupportsTemporalProcessing
        {
            get
            {
                return false;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Returns a short, one-line status message about the adapter.
        /// </summary>
        /// <param name="maxLength">The maximum length of the message to be returned.</param>
        /// <returns>A short, one-line status message.</returns>
        public override string GetShortStatus(int maxLength)
        {
            return string.Format("Detected {0} measurements with bad timestamps", m_totalBadTimestampMeasurements).CenterText(maxLength);
        }

        /// <summary>
        /// Initializes <see cref="TimestampTest"/>.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            string errorMessage = "{0} is missing from Settings - Example: concentratorName=TESTSTREAM";
            Dictionary<string, string> settings = Settings;
            string setting;

            // Load optional parameters
            if (settings.TryGetValue("timeToPurge", out setting))
                m_timeToPurge = Ticks.FromSeconds(double.Parse(setting));

            if (settings.TryGetValue("warnInterval", out setting))
                m_warnInterval = Ticks.FromSeconds(double.Parse(setting));

            // Load required parameters
            string concentratorName;

            if (!settings.TryGetValue("concentratorName", out concentratorName))
                throw new ArgumentException(string.Format(errorMessage, "concentratorName"));

            m_discardingAdapter = null;

            // Find the adapter whose name matches the specified concentratorName
            foreach (IAdapter adapter in Parent)
            {
                IActionAdapter concentrator = adapter as IActionAdapter;

                if (concentrator != null && string.Compare(adapter.Name, concentratorName, true) == 0)
                {
                    m_discardingAdapter = concentrator;
                    break;
                }
            }

            if (m_discardingAdapter == null)
                throw new ArgumentException(string.Format("Concentrator {0} not found.", concentratorName));

            // Wait for associated adapter to initialize
            int timeout = m_discardingAdapter.InitializationTimeout;
            m_discardingAdapter.WaitForInitialize(timeout);

            if (!m_discardingAdapter.Initialized)
                throw new TimeoutException(string.Format("Timeout waiting for concentrator {0} to initialize.", concentratorName));

            // Attach to adapter's discarding measurements and disposed events
            m_discardingAdapter.DiscardingMeasurements += m_discardingAdapter_DiscardingMeasurements;
            m_discardingAdapter.Disposed += m_discardingAdapter_Disposed;

            m_purgeTimer.Interval = m_timeToPurge.ToMilliseconds();
            m_purgeTimer.Elapsed += m_purgeTimer_Elapsed;

            m_warningTimer.Interval = m_warnInterval.ToMilliseconds();
            m_warningTimer.Elapsed += m_warningTimer_Elapsed;

            m_timestampService = new TimestampService(this);
            m_timestampService.ServiceProcessException += m_timestampService_ServiceProcessException;
            m_timestampService.SettingsCategory = base.Name + m_timestampService.SettingsCategory;
            m_timestampService.Initialize();
        }

        /// <summary>
        /// Starts this <see cref="TimestampTest"/>.
        /// </summary>
        public override void Start()
        {
            base.Start();
            m_purgeTimer.Start();
            m_warningTimer.Start();
        }

        /// <summary>
        /// Stops this <see cref="TimestampTest"/>.
        /// </summary>
        public override void Stop()
        {
            base.Stop();
            m_purgeTimer.Stop();
            m_warningTimer.Stop();
        }

        /// <summary>
        /// Queues a collection of measurements for processing.
        /// </summary>
        /// <param name="measurements">Collection of measurements to queue for processing.</param>
        /// <remarks>
        /// The <see cref="TimestampTest"/> adapter doesn't process any measurements.
        /// </remarks>
        public override void QueueMeasurementsForProcessing(IEnumerable<IMeasurement> measurements)
        {
            // The timestamp test doesn't have a need to process any measurements
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="TimestampTest"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                    {
                        // Dispose timestamp service.
                        if (m_timestampService != null)
                        {
                            m_timestampService.ServiceProcessException -= m_timestampService_ServiceProcessException;
                            m_timestampService.Dispose();
                        }
                        m_timestampService = null;

                        // Dispose purge timer.
                        if (m_purgeTimer != null)
                        {
                            m_purgeTimer.Elapsed -= m_purgeTimer_Elapsed;
                            m_purgeTimer.Dispose();
                        }
                        m_purgeTimer = null;

                        // Dispose warning timer.
                        if (m_warningTimer != null)
                        {
                            m_warningTimer.Elapsed -= m_warningTimer_Elapsed;
                            m_warningTimer.Dispose();
                        }
                        m_warningTimer = null;

                        // Dispose discarding adapter.
                        if (m_discardingAdapter != null)
                        {
                            m_discardingAdapter.DiscardingMeasurements -= m_discardingAdapter_DiscardingMeasurements;
                            m_discardingAdapter.Disposed -= m_discardingAdapter_Disposed;
                        }
                        m_discardingAdapter = null; 
                    }
                }
                finally
                {
                    m_disposed = true;          // Prevent duplicate dispose.
                    base.Dispose(disposing);    // Call base class Dispose().
                }
            }
        }

        /// <summary>
        /// Gets all the measurements with bad timestamps.
        /// </summary>
        /// <returns>A dictionary where the keys are arrival times and the values are <see cref="LinkedList{IMeasurement}"/>s of measurements that arrived at the corresponding time.</returns>
        public Dictionary<Ticks, LinkedList<IMeasurement>> GetMeasurementsWithBadTimestamps()
        {
            lock (m_badTimestampMeasurements)
            {
                PurgeOldMeasurements();
                return new Dictionary<Ticks, LinkedList<IMeasurement>>(m_badTimestampMeasurements);
            }
        }

        /// <summary>
        /// Gets the number of measurements with bad timestamps received by this <see cref="TimestampTest"/>.
        /// </summary>
        /// <returns>The number of measurements with bad timestamps.</returns>
        public int GetBadTimestampMeasurementCount()
        {
            Dictionary<Ticks, LinkedList<IMeasurement>> badTimestampMeasurements = GetMeasurementsWithBadTimestamps();
            int count = 0;

            foreach (LinkedList<IMeasurement> measurementList in badTimestampMeasurements.Values)
            {
                count += measurementList.Count;
            }

            return count;
        }

        private void m_discardingAdapter_DiscardingMeasurements(object sender, EventArgs<IEnumerable<IMeasurement>> e)
        {
            if (Enabled)
            {
                Ticks currentTime = DateTime.UtcNow.Ticks;

                lock (m_badTimestampMeasurements)
                {
                    foreach (IMeasurement measurement in e.Argument)
                    {
                        Ticks distance = currentTime - measurement.Timestamp;
                        AddMeasurementWithBadTimestamp(currentTime, measurement);
                    }
                }
            }
        }

        private void AddMeasurementWithBadTimestamp(Ticks timeArrived, IMeasurement measurement)
        {
            lock (m_badTimestampMeasurements)
            {
                LinkedList<IMeasurement> measurementList;

                if (!m_badTimestampMeasurements.TryGetValue(timeArrived, out measurementList))
                {
                    measurementList = new LinkedList<IMeasurement>();
                    m_badTimestampMeasurements.Add(timeArrived, measurementList);
                }

                measurementList.AddLast(measurement);
                m_totalBadTimestampMeasurements++;
            }
        }

        private void PurgeOldMeasurements()
        {
            Ticks currentTime = DateTime.UtcNow.Ticks;

            lock (m_badTimestampMeasurements)
            {
                List<Ticks> arrivalTimes = new List<Ticks>(m_badTimestampMeasurements.Keys);

                foreach (Ticks timeArrived in arrivalTimes)
                {
                    Ticks distance = currentTime - timeArrived;

                    if (distance > m_timeToPurge)
                        m_badTimestampMeasurements.Remove(timeArrived);
                }
            }
        }

        // Periodically purge measurements to speed up retrieval of data.
        private void m_purgeTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            PurgeOldMeasurements();
        }

        // Periodically send updates to the console about any measurements with bad timestamps.
        private void m_warningTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            int count = GetBadTimestampMeasurementCount();

            if (count > 0)
                OnStatusMessage("Received {0} measurements with bad timestamps within the last {1} seconds.", count, (int)m_timeToPurge.ToSeconds());
        }

        private void m_discardingAdapter_Disposed(object sender, EventArgs e)
        {
            m_discardingAdapter.DiscardingMeasurements -= m_discardingAdapter_DiscardingMeasurements;
            m_discardingAdapter.Disposed -= m_discardingAdapter_Disposed;
            m_discardingAdapter = null;
        }

        private void m_timestampService_ServiceProcessException(object sender, EventArgs<Exception> e)
        {
            OnProcessException(e.Argument);
        }

        #endregion
    }
}
