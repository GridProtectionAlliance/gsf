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
        private readonly int m_statisticDataRefreshInterval = 5;
        private string m_lastRefresh;
        private bool m_restartConnectionCycle;

        // Unsynchronized Subscription Fields.
        private DataSubscriber m_unsynchronizedSubscriber;
        private bool m_subscribedUnsynchronized;
        private string m_allSignalIDs;  // string of GUIDs used for subscription.
        private int m_processingUnsynchronizedMeasurements;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets a boolean flag indicating if connection to backend windows service needs to be reestablished upon disconnection.
        /// </summary>
        public bool RestartConnectionCycle
        {
            get
            {
                return m_restartConnectionCycle;
            }
            set
            {
                m_restartConnectionCycle = value;
            }
        }

        /// <summary>
        /// Gets flag that determines if <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/> is a new record.
        /// </summary>
        public override bool IsNewRecord
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets or sets when data refreshed last time.
        /// </summary>
        public string LastRefresh
        {
            get
            {
                return m_lastRefresh;
            }
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
            m_restartConnectionCycle = true;

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
            if (0 == Interlocked.Exchange(ref m_processingUnsynchronizedMeasurements, 1))
            {
                try
                {
                    foreach (IMeasurement newMeasurement in e.Argument)
                    {
                        StatisticMeasurement measurement;

                        if (RealTimeStatistic.StatisticMeasurements.TryGetValue(newMeasurement.ID, out measurement))
                        {
                            if (!string.IsNullOrEmpty(measurement.DisplayFormat) && !string.IsNullOrEmpty(measurement.DataType))
                            {
                                measurement.Quality = newMeasurement.ValueQualityIsGood() ? "GOOD" : "BAD";
                                measurement.Value = string.Format(measurement.DisplayFormat, ConvertValueToType(newMeasurement.AdjustedValue.ToString(), measurement.DataType));
                                measurement.TimeTag = newMeasurement.Timestamp.ToString("HH:mm:ss.fff");

                                StreamStatistic streamStatistic;
                                if (measurement.ConnectedState) //if measurement defines connection state.
                                {
                                    if ((measurement.Source == "System" && RealTimeStatistic.SystemStatistics.TryGetValue(measurement.DeviceID, out streamStatistic)) ||
                                        (measurement.Source == "InputStream" && RealTimeStatistic.InputStreamStatistics.TryGetValue(measurement.DeviceID, out streamStatistic)) ||
                                        (measurement.Source == "OutputStream" && RealTimeStatistic.OutputStreamStatistics.TryGetValue(measurement.DeviceID, out streamStatistic)) ||
                                        (measurement.Source == "Publisher" && RealTimeStatistic.DataPublisherStatistics.TryGetValue(measurement.DeviceID, out streamStatistic)) ||
                                        (measurement.Source == "Subscriber" && RealTimeStatistic.InputStreamStatistics.TryGetValue(measurement.DeviceID, out streamStatistic)))
                                    {
                                        if (Convert.ToBoolean(newMeasurement.AdjustedValue))
                                            streamStatistic.StatusColor = "Green";
                                        else
                                            streamStatistic.StatusColor = "Red";

                                        // We do extra validation on the input stream since devices can be technically connected and not receiving data (e.g., UDP)
                                        if (measurement.Source == "InputStream")
                                        {
                                            StatisticMeasurement totalFramesStat = RealTimeStatistic.StatisticMeasurements.Values.FirstOrDefault(stat => string.Compare(stat.SignalReference.ToNonNullString().Trim(), string.Format("{0}!IS-ST1", streamStatistic.Acronym.ToNonNullString().Trim()), true) == 0);

                                            if ((object)totalFramesStat != null)
                                            {
                                                IMeasurement totalFramesStatMeasurement = e.Argument.FirstOrDefault(m => m.ID == totalFramesStat.SignalID);

                                                if ((object)totalFramesStatMeasurement != null && totalFramesStatMeasurement.AdjustedValue <= 0.0D)
                                                    streamStatistic.StatusColor = "Red";
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    LastRefresh = "Last Refresh: " + DateTime.UtcNow.ToString("HH:mm:ss.fff");

                    if ((object)NewMeasurements != null)
                        NewMeasurements(this, e);
                }
                finally
                {
                    Interlocked.Exchange(ref m_processingUnsynchronizedMeasurements, 0);
                }
            }
        }

        private void m_unsynchronizedSubscriber_ConnectionEstablished(object sender, EventArgs e)
        {
            m_subscribedUnsynchronized = true;
            SubscribeUnsynchronizedData();
        }

        private void m_unsynchronizedSubscriber_ProcessException(object sender, EventArgs<Exception> e)
        {
        }

        private void m_unsynchronizedSubscriber_StatusMessage(object sender, EventArgs<string> e)
        {
        }

        private void InitializeUnsynchronizedSubscription()
        {
            try
            {
                using (AdoDataConnection database = new AdoDataConnection(CommonFunctions.DefaultSettingsCategory))
                {
                    m_unsynchronizedSubscriber = new DataSubscriber();
                    m_unsynchronizedSubscriber.StatusMessage += m_unsynchronizedSubscriber_StatusMessage;
                    m_unsynchronizedSubscriber.ProcessException += m_unsynchronizedSubscriber_ProcessException;
                    m_unsynchronizedSubscriber.ConnectionEstablished += m_unsynchronizedSubscriber_ConnectionEstablished;
                    m_unsynchronizedSubscriber.NewMeasurements += m_unsynchronizedSubscriber_NewMeasurements;
                    m_unsynchronizedSubscriber.ConnectionTerminated += m_unsynchronizedSubscriber_ConnectionTerminated;

                    // Statistics move slowly, typically every 10 seconds, so we reduce data loss interval to every 20 seconds
                    m_unsynchronizedSubscriber.ConnectionString = "dataLossInterval = 20.0; " + database.DataPublisherConnectionString();

                    m_unsynchronizedSubscriber.Initialize();
                    m_unsynchronizedSubscriber.Start();
                }
            }
            catch (Exception ex)
            {
                Popup("Failed to initialize subscription." + Environment.NewLine + ex.Message, "Failed to Subscribe", MessageBoxImage.Error);
            }
        }

        private void StopUnsynchronizedSubscription()
        {
            if (m_unsynchronizedSubscriber != null)
            {
                m_unsynchronizedSubscriber.StatusMessage -= m_unsynchronizedSubscriber_StatusMessage;
                m_unsynchronizedSubscriber.ProcessException -= m_unsynchronizedSubscriber_ProcessException;
                m_unsynchronizedSubscriber.ConnectionEstablished -= m_unsynchronizedSubscriber_ConnectionEstablished;
                m_unsynchronizedSubscriber.NewMeasurements -= m_unsynchronizedSubscriber_NewMeasurements;
                m_unsynchronizedSubscriber.ConnectionTerminated -= m_unsynchronizedSubscriber_ConnectionTerminated;
                m_unsynchronizedSubscriber.Stop();
                m_unsynchronizedSubscriber.Dispose();
                m_unsynchronizedSubscriber = null;
            }
        }

        private void SubscribeUnsynchronizedData()
        {
            UnsynchronizedSubscriptionInfo info;

            if (m_unsynchronizedSubscriber == null)
                InitializeUnsynchronizedSubscription();

            if (m_subscribedUnsynchronized && !string.IsNullOrEmpty(m_allSignalIDs))
            {
                info = new UnsynchronizedSubscriptionInfo(false);

                info.UseCompactMeasurementFormat = true;
                info.FilterExpression = m_allSignalIDs;
                info.IncludeTime = true;
                info.LagTime = 60.0D;
                info.LeadTime = 60.0D;
                info.PublishInterval = m_statisticDataRefreshInterval;

                m_unsynchronizedSubscriber.UnsynchronizedSubscribe(info);
            }
        }

        /// <summary>
        /// Unsubscribes data from the service.
        /// </summary>
        public void UnsubscribeUnsynchronizedData()
        {
            try
            {
                if (m_unsynchronizedSubscriber != null)
                {
                    m_unsynchronizedSubscriber.Unsubscribe();
                    StopUnsynchronizedSubscription();
                }
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
        public override int GetCurrentItemKey()
        {
            return 0;
        }

        /// <summary>
        /// Gets the string based named identifier of the <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/>.
        /// </summary>
        /// <returns>The string based named identifier of the <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/>.</returns>
        public override string GetCurrentItemName()
        {
            return "";
        }

        public override void Load()
        {
            try
            {
                base.Load();

                StringBuilder sb = new StringBuilder();

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
                if (ex.InnerException != null)
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

        private object ConvertValueToType(string xmlValue, string xmlDataType)
        {
            Type dataType = Type.GetType(xmlDataType);
            float value;

            if (float.TryParse(xmlValue, out value))
            {
                switch (xmlDataType)
                {
                    case "System.DateTime":
                        return new DateTime((long)value);
                    default:
                        return Convert.ChangeType(value, dataType);
                }
            }

            return "".ConvertToType<object>(dataType);
        }

        public void Stop()
        {
            UnsubscribeUnsynchronizedData();
        }

        #endregion
    }
}
