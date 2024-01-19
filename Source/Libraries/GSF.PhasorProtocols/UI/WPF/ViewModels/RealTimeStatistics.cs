//******************************************************************************************************
//  RealTimeStatistics.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
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
//  09/29/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using GSF.Data;
using GSF.PhasorProtocols.UI.DataModels;
using GSF.TimeSeries;
using GSF.TimeSeries.Transport;
using GSF.TimeSeries.UI;

// ReSharper disable UnusedParameter.Local
// ReSharper disable VirtualMemberCallInConstructor
namespace GSF.PhasorProtocols.UI.ViewModels
{
    internal class RealTimeStatistics : PagedViewModelBase<RealTimeStatistic, int>
    {
        #region [ Members ]

        // Events

        /// <summary>
        /// When new measurements are received from the unsynchronized subscriber.
        /// </summary>
        public event EventHandler<EventArgs<ICollection<IMeasurement>>> NewMeasurements;

        // Fields
        private readonly int m_statisticDataRefreshInterval;
        private string m_lastRefresh;
        private readonly Dictionary<int, long> m_lastDeviceReportTimes = new();

        // Unsynchronized Subscription Fields.
        private DataSubscriber m_unsynchronizedSubscriber;
        private bool m_subscribedUnsynchronized;
        private string m_allSignalIDs;  // string of GUIDs used for subscription.
        private int m_processingUnsynchronizedMeasurements;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets a boolean flag indicating if connection to back-end windows service needs to be reestablished upon disconnection.
        /// </summary>
        public bool RestartConnectionCycle { get; set; }

        /// <summary>
        /// Gets flag that determines if <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/> is a new record.
        /// </summary>
        public override bool IsNewRecord => false;

        /// <summary>
        /// Gets or sets when data refreshed last time.
        /// </summary>
        public string LastRefresh
        {
            get => m_lastRefresh;
            set
            {
                m_lastRefresh = value;
                OnPropertyChanged("LastRefresh");
            }
        }

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates an instance of <see cref="RealTimeStatistics"/>.
        /// </summary>
        /// <param name="itemsPerPage">Number of items to be displayed on page.</param>
        /// <param name="refreshInterval">Interval at which data will be refreshed.</param>
        /// <param name="autoSave">Boolean flag indicating if changed records to be auto saved.</param>
        public RealTimeStatistics(int itemsPerPage, int refreshInterval, bool autoSave = false)
            : base(0, autoSave)
        {
            m_statisticDataRefreshInterval = refreshInterval;
            RestartConnectionCycle = true;

            Load();
        }

        #endregion

        #region [ Methods ]

        #region [ Unsynchronized Subscription ]

        private void m_unsynchronizedSubscriber_ConnectionTerminated(object sender, EventArgs e)
        {
            m_subscribedUnsynchronized = false;

            UnsubscribeUnsynchronizedData();

            if (RestartConnectionCycle)
                InitializeUnsynchronizedSubscription();
        }

        private void m_unsynchronizedSubscriber_NewMeasurements(object sender, EventArgs<ICollection<IMeasurement>> e)
        {
            if (Interlocked.Exchange(ref m_processingUnsynchronizedMeasurements, 1) != 0)
                return;

            HashSet<int> deviceIDs = new();

            try
            {
                ICollection<IMeasurement> measurements = e.Argument;

                foreach (IMeasurement newMeasurement in measurements)
                {
                    if (!RealTimeStatistic.StatisticMeasurements.TryGetValue(newMeasurement.ID, out StatisticMeasurement measurement))
                        continue;

                    if (string.IsNullOrEmpty(measurement.DisplayFormat) || string.IsNullOrEmpty(measurement.DataType))
                        continue;

                    measurement.Quality = newMeasurement.ValueQualityIsGood() ? "GOOD" : "BAD";
                    measurement.Value = string.Format(measurement.DisplayFormat, ConvertValueToType(newMeasurement.AdjustedValue, measurement.DataType));
                    measurement.TimeTag = newMeasurement.Timestamp.ToString("HH:mm:ss.fff");

                    // Check if measurement defines connection state
                    if (!measurement.ConnectedState)
                        continue;

                    if ((measurement.Source != "System" || !RealTimeStatistic.SystemStatistics.TryGetValue(measurement.DeviceID, out StreamStatistic streamStatistic)) &&
                        (measurement.Source != "InputStream" || !RealTimeStatistic.InputStreamStatistics.TryGetValue(measurement.DeviceID, out streamStatistic)) &&
                        (measurement.Source != "OutputStream" || !RealTimeStatistic.OutputStreamStatistics.TryGetValue(measurement.DeviceID, out streamStatistic)) &&
                        (measurement.Source != "Publisher" || !RealTimeStatistic.DataPublisherStatistics.TryGetValue(measurement.DeviceID, out streamStatistic)) &&
                        (measurement.Source != "Subscriber" || !RealTimeStatistic.InputStreamStatistics.TryGetValue(measurement.DeviceID, out streamStatistic)))
                        continue;

                    // Only process following statistic operations per device
                    if (measurement.DeviceID > 0 && !deviceIDs.Add(measurement.DeviceID))
                        continue;

                    if (measurement.Source == "InputStream")
                    {
                        // Get last report time for device
                        StatisticMeasurement lastReportTimeStat = RealTimeStatistic.StatisticMeasurements.Values.FirstOrDefault(stat => string.Compare(stat.SignalReference.ToNonNullString().Trim(), $"{streamStatistic.Acronym.ToNonNullString().Trim()}!IS-ST2", StringComparison.OrdinalIgnoreCase) == 0);

                        if (lastReportTimeStat is null)
                            return;

                        IMeasurement lastReportTimeStatMeasurement = measurements.FirstOrDefault(m => m.ID == lastReportTimeStat.SignalID);

                        if (lastReportTimeStatMeasurement is null)
                            return;

                        long lastReportTime = (long)lastReportTimeStatMeasurement.AdjustedValue;

                        if (lastReportTime <= 0)
                            return;

                        // Only updating device statuses after we are sure device has been connected for five seconds
                        if (m_lastDeviceReportTimes.TryGetValue(measurement.DeviceID, out long lastReportTimeForDevice) && lastReportTimeForDevice > 0)
                        {
                            if (lastReportTimeForDevice > lastReportTime)
                            {
                                // Last report time for device rolled over, it only has hour accuracy
                                m_lastDeviceReportTimes[measurement.DeviceID] = lastReportTime;
                                return;
                            }

                            // Only check further stats after five seconds - the frame rate timer only updates every five seconds so
                            // we need to wait for count to be updated on an initial connection before checking for total frames
                            if (lastReportTime - lastReportTimeForDevice < 5 * Ticks.PerSecond)
                                return;
                        }
                        else
                        {
                            m_lastDeviceReportTimes.Add(measurement.DeviceID, lastReportTime);
                            return;
                        }
                    }

                    // Connected stat primarily drives the status color
                    streamStatistic.StatusColor = Convert.ToBoolean(newMeasurement.AdjustedValue) ? "Green" : "Red";

                    // We do extra validation on the input stream since devices can be technically connected and not receiving data (e.g., UDP)
                    if (measurement.Source != "InputStream")
                        continue;

                    // Check if measurement defines total frames received
                    ThreadPool.QueueUserWorkItem(_ =>
                    {
                        StatisticMeasurement totalFramesStat = RealTimeStatistic.StatisticMeasurements.Values.FirstOrDefault(stat => string.Compare(stat.SignalReference.ToNonNullString().Trim(), $"{streamStatistic.Acronym.ToNonNullString().Trim()}!IS-ST1", StringComparison.OrdinalIgnoreCase) == 0);

                        if (totalFramesStat is null)
                            return;

                        IMeasurement totalFramesStatMeasurement = measurements.FirstOrDefault(m => m.ID == totalFramesStat.SignalID);

                        if (totalFramesStatMeasurement is { AdjustedValue: <= 0.0D })
                            streamStatistic.StatusColor = "Red";
                    });

                    // Check if measurement defines configuration out of sync state
                    ThreadPool.QueueUserWorkItem(_ =>
                    {
                        StatisticMeasurement configurationOutOfSyncStat = RealTimeStatistic.StatisticMeasurements.Values.FirstOrDefault(stat => string.Compare(stat.SignalReference.ToNonNullString().Trim(), $"{streamStatistic.Acronym.ToNonNullString().Trim()}!IS-ST29", StringComparison.OrdinalIgnoreCase) == 0);

                        if (configurationOutOfSyncStat is null)
                            return;

                        IMeasurement configurationOutOfSyncStatMeasurement = measurements.FirstOrDefault(m => m.ID == configurationOutOfSyncStat.SignalID);

                        if (configurationOutOfSyncStatMeasurement is not null)
                            streamStatistic.ConfigurationOutOfSync = configurationOutOfSyncStatMeasurement.AdjustedValue != 0.0D;
                    });
                }

                LastRefresh = "Last Refresh: " + DateTime.UtcNow.ToString("HH:mm:ss.fff");
                NewMeasurements?.Invoke(this, e);
            }
            finally
            {
                Interlocked.Exchange(ref m_processingUnsynchronizedMeasurements, 0);
            }
        }

        private void m_unsynchronizedSubscriber_ConnectionEstablished(object sender, EventArgs e)
        {
            m_subscribedUnsynchronized = true;
            SubscribeUnsynchronizedData();
        }

        private void InitializeUnsynchronizedSubscription()
        {
            try
            {
                using AdoDataConnection database = new(CommonFunctions.DefaultSettingsCategory);

                m_unsynchronizedSubscriber = new DataSubscriber();
                m_unsynchronizedSubscriber.ConnectionEstablished += m_unsynchronizedSubscriber_ConnectionEstablished;
                m_unsynchronizedSubscriber.NewMeasurements += m_unsynchronizedSubscriber_NewMeasurements;
                m_unsynchronizedSubscriber.ConnectionTerminated += m_unsynchronizedSubscriber_ConnectionTerminated;

                // Statistics move slowly, typically every 10 seconds, so we reduce data loss interval to every 20 seconds
                m_unsynchronizedSubscriber.ConnectionString = "dataLossInterval = 20.0; " + database.DataPublisherConnectionString();

                m_unsynchronizedSubscriber.Initialize();
                m_unsynchronizedSubscriber.Start();
            }
            catch (Exception ex)
            {
                Popup("Failed to initialize subscription." + Environment.NewLine + ex.Message, "Failed to Subscribe", MessageBoxImage.Error);
            }
        }

        private void StopUnsynchronizedSubscription()
        {
            if (m_unsynchronizedSubscriber is null)
                return;

            m_unsynchronizedSubscriber.ConnectionEstablished -= m_unsynchronizedSubscriber_ConnectionEstablished;
            m_unsynchronizedSubscriber.NewMeasurements -= m_unsynchronizedSubscriber_NewMeasurements;
            m_unsynchronizedSubscriber.ConnectionTerminated -= m_unsynchronizedSubscriber_ConnectionTerminated;
            m_unsynchronizedSubscriber.Stop();
            m_unsynchronizedSubscriber.Dispose();

            m_unsynchronizedSubscriber = null;
        }

        private void SubscribeUnsynchronizedData()
        {
            if (m_unsynchronizedSubscriber is null)
                InitializeUnsynchronizedSubscription();

            if (!m_subscribedUnsynchronized || string.IsNullOrEmpty(m_allSignalIDs))
                return;

            UnsynchronizedSubscriptionInfo info = new(false)
            {
                UseCompactMeasurementFormat = true,
                FilterExpression = m_allSignalIDs,
                IncludeTime = true,
                LagTime = 60.0D,
                LeadTime = 60.0D,
                PublishInterval = m_statisticDataRefreshInterval
            };


            m_unsynchronizedSubscriber?.UnsynchronizedSubscribe(info);
        }

        /// <summary>
        /// Unsubscribes data from the service.
        /// </summary>
        public void UnsubscribeUnsynchronizedData()
        {
            try
            {
                if (m_unsynchronizedSubscriber is null)
                    return;

                m_unsynchronizedSubscriber.Unsubscribe();
                StopUnsynchronizedSubscription();
            }
            catch
            {
                m_unsynchronizedSubscriber = null;
            }
        }

        #endregion

        /// <summary>
        /// Gets the primary key value of the <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/>.
        /// </summary>
        /// <returns>The primary key value of the <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/>.</returns>
        public override int GetCurrentItemKey() => 0;

        /// <summary>
        /// Gets the string based named identifier of the <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/>.
        /// </summary>
        /// <returns>The string based named identifier of the <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/>.</returns>
        public override string GetCurrentItemName() => string.Empty;

        public override void Load()
        {
            try
            {
                base.Load();

                StringBuilder sb = new();

                foreach (KeyValuePair<Guid, StatisticMeasurement> measurement in RealTimeStatistic.StatisticMeasurements)
                {
                    sb.Append(measurement.Key);
                    sb.Append(";");
                }

                m_allSignalIDs = sb.ToString();

                if (m_allSignalIDs.Length > 0)
                    m_allSignalIDs = m_allSignalIDs.Substring(0, m_allSignalIDs.Length - 1);

                InitializeUnsynchronizedSubscription();
            }
            catch (Exception ex)
            {
                if (ex.InnerException is not null)
                {
                    Popup(ex.Message + Environment.NewLine + "Inner Exception: " + ex.InnerException.Message, "Load " + DataModelName + " Exception:", MessageBoxImage.Error);
                    CommonFunctions.LogException(null, "Load " + DataModelName, ex.InnerException);
                }
                else
                {
                    Popup(ex.Message, "Load " + DataModelName + " Exception:", MessageBoxImage.Error);
                    CommonFunctions.LogException(null, "Load " + DataModelName, ex);
                }
            }
        }

        private static object ConvertValueToType(double value, string dataType)
        {
            return dataType switch
            {
                "System.Double" => value,
                "System.DateTime" => new DateTime((long)value),
                "GSF.UnixTimeTag" => new UnixTimeTag((decimal)value),
                _ => Convert.ChangeType(value, Type.GetType(dataType) ?? typeof(double))
            };
        }

        public void Stop() =>
            UnsubscribeUnsynchronizedData();

        #endregion
    }
}
