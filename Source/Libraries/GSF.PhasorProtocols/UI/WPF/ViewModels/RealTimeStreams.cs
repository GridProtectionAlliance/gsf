//******************************************************************************************************
//  RealTimeStreams.cs - Gbtc
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
//  08/18/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using GSF.Communication;
using GSF.Data;
using GSF.PhasorProtocols.UI.DataModels;
using GSF.ServiceProcess;
using GSF.TimeSeries;
using GSF.TimeSeries.Statistics;
using GSF.TimeSeries.Transport;
using GSF.TimeSeries.UI;

namespace GSF.PhasorProtocols.UI.ViewModels
{
    /// <summary>
    /// Class to hold bindable <see cref="RealTimeStream"/> collection and current selection information for UI.
    /// </summary>
    internal class RealTimeStreams : PagedViewModelBase<RealTimeStream, int>
    {
        #region [ Members ]

        // Fields
        private bool m_expanded;
        private bool m_restartConnectionCycle;
        private string m_lastRefresh;
        private ObservableCollection<StatisticMeasurement> m_statisticMeasurements;
        private Dictionary<Guid, RealTimeMeasurement> m_realTimeMeasurements; 
        private RealTimeStatistics m_statistics;
        private readonly int m_statisticRefreshInterval = 5;
        private bool m_temporalSupportEnabled;
        private string m_startTime = "*-10m";
        private string m_stopTime = "*"; 

        // Unsynchronized Subscription Fields.
        private DataSubscriber m_unsynchronizedSubscriber;
        private bool m_subscribedUnsynchronized;
        private string m_allSignalIDs;  // string of GUIDs used for subscription.
        private int m_processingUnsynchronizedMeasurements;
        private readonly int m_refreshInterval = 5;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates an instance of <see cref="RealTimeStreams"/>.
        /// </summary>
        /// <param name="itemsPerPage"></param>
        /// <param name="refreshInterval">Interval to refresh measurement in a tree.</param>
        /// <param name="autoSave"></param>        
        public RealTimeStreams(int itemsPerPage, int refreshInterval, bool autoSave = false)
            : base(itemsPerPage, autoSave)
        {
            // Perform initialization here. 
            m_refreshInterval = refreshInterval;
            InitializeUnsynchronizedSubscription();
            m_restartConnectionCycle = true;
            StatisticMeasurements = new ObservableCollection<StatisticMeasurement>();

            int.TryParse(IsolatedStorageManager.ReadFromIsolatedStorage("StatisticsDataRefreshInterval").ToString(), out m_statisticRefreshInterval);
            Statistics = new RealTimeStatistics(1, m_statisticRefreshInterval);

            CheckTemporalSupport();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets start time for this <see cref="RealTimeStream"/>.
        /// </summary>
        public string StartTime
        {
            get
            {
                return m_startTime;
            }
            set
            {
                m_startTime = value;
            }
        }

        /// <summary>
        /// Gets or sets stop for this <see cref="RealTimeStream"/>.
        /// </summary>
        public string StopTime
        {
            get
            {
                return m_stopTime;
            }
            set
            {
                m_stopTime = value;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if temporal support is enabled.
        /// </summary>
        public bool TemporalSupportEnabled
        {
            get
            {
                return m_temporalSupportEnabled;
            }
            set
            {
                m_temporalSupportEnabled = value;
                OnPropertyChanged("TemporalSupportEnabled");
            }
        }

        /// <summary>
        /// Gets flag that determines if <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/> is a new record.
        /// </summary>
        public override bool IsNewRecord
        {
            get
            {
                return CurrentItem.ID == 0;
            }
        }

        /// <summary>
        /// Gets or sets a boolean flag <see cref="RealTimeStreams"/> expanded flag.
        /// </summary>
        public bool Expanded
        {
            get
            {
                return m_expanded;
            }
            set
            {
                m_expanded = value;
                OnPropertyChanged("Expanded");
            }
        }

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
        /// Gets or sets a last refresh time to display on UI.
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

        /// <summary>
        /// Gets or sets <see cref="RealTimeStreams"/> StatisticMeasurements.
        /// </summary>
        public ObservableCollection<StatisticMeasurement> StatisticMeasurements
        {
            get
            {
                return m_statisticMeasurements;
            }
            set
            {
                m_statisticMeasurements = value;
                OnPropertyChanged("StatisticMeasurements");
            }
        }

        private RealTimeStatistics Statistics
        {
            get
            {
                return m_statistics;
            }
            set
            {
                if ((object)m_statistics != null)
                    m_statistics.NewMeasurements -= m_statistics_NewMeasurements;

                m_statistics = value;

                if ((object)m_statistics != null)
                    m_statistics.NewMeasurements += m_statistics_NewMeasurements;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Gets the primary key value of the <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/>.
        /// </summary>
        /// <returns>The primary key value of the <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/>.</returns>
        public override int GetCurrentItemKey()
        {
            return CurrentItem.ID;
        }

        /// <summary>
        /// Gets the string based named identifier of the <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/>.
        /// </summary>
        /// <returns>The string based named identifier of the <see cref="PagedViewModelBase{T1, T2}.CurrentItem"/>.</returns>
        public override string GetCurrentItemName()
        {
            return CurrentItem.Name;
        }

        /// <summary>
        /// Overrides Load() method from the base class to add aditional functionalities.
        /// </summary>
        public override void Load()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                base.Load();

                // Build a string of measurement GUIDs to pass into subscription request to retrieve data for tree.
                StringBuilder sb = new StringBuilder();

                foreach (RealTimeStream stream in ItemsSource)
                {
                    foreach (RealTimeDevice device in stream.DeviceList)
                    {
                        foreach (RealTimeMeasurement measurement in device.MeasurementList)
                        {
                            sb.Append(measurement.SignalID.ToString());
                            sb.Append(";");
                        }
                    }
                }

                m_allSignalIDs = sb.ToString();

                if (m_allSignalIDs.Length > 0)
                    m_allSignalIDs = m_allSignalIDs.Substring(0, m_allSignalIDs.Length - 1);
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
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        public void GetStatistics(Device device)
        {
            try
            {
                StatisticMeasurements.Clear();

                ObservableCollection<StatisticMeasurement> tempMeasurements = new ObservableCollection<StatisticMeasurement>(
                    RealTimeStatistic.GetStatisticMeasurements(null).Where(sm => sm.DeviceID == device.ID)
                );

                foreach (StatisticMeasurement measurement in tempMeasurements)
                {
                    StatisticMeasurement tempMeasurement;

                    if (RealTimeStatistic.StatisticMeasurements.TryGetValue(measurement.SignalID, out tempMeasurement))
                        StatisticMeasurements.Add(tempMeasurement);
                }

            }
            catch (Exception ex)
            {
                Popup("Failed to retrieve statistics." + Environment.NewLine + ex.Message, "ERROR! Get Statistics", MessageBoxImage.Error);
            }
        }

        private void m_statistics_NewMeasurements(object sender, EventArgs<ICollection<IMeasurement>> e)
        {
            if (0 == Interlocked.Exchange(ref m_processingUnsynchronizedMeasurements, 1))
            {
                try
                {
                    StreamStatistic streamStatistic;
                    long now = DateTime.UtcNow.Ticks;

                    // Since device health is handled by the data subscription,
                    // this subscription will only tell us whether devices have
                    // been disconnected -- colors can only turn red
                    foreach (RealTimeStream stream in ItemsSource)
                    {
                        if (stream.ID > 0 && RealTimeStatistic.InputStreamStatistics.TryGetValue(stream.ID, out streamStatistic))
                        {
                            // Stream has input stream statistics
                            // that determine its connectivity state
                            if (streamStatistic.StatusColor == "Red")
                                stream.StatusColor = "Red";
                        }

                        foreach (RealTimeDevice device in stream.DeviceList)
                        {
                            if (device.ID != null && device.ID > 0)
                            {
                                if (stream.StatusColor == "Red")
                                {
                                    // If a stream is disconnected,
                                    // its devices are also disconnected
                                    device.StatusColor = "Red";
                                }
                                else if (RealTimeStatistic.InputStreamStatistics.TryGetValue((int)device.ID, out streamStatistic))
                                {
                                    // The device is direct connected, so it has input stream
                                    // statistics that determine its connectivity state
                                    if (streamStatistic.StatusColor == "Red")
                                        device.StatusColor = "Red";
                                }
                            }
                            else
                            {
                                // This device is not configured, but rather its existence is inferred based on the signal references of its measurements.
                                // Its connectivity state is determined by the length of time since its measurements were last received
                                if (device.MeasurementList.All(measurement => Ticks.ToSeconds(now - measurement.LastUpdated) > m_refreshInterval * 2))
                                    device.StatusColor = "Red";
                            }

                            if (device.StatusColor == "Red")
                            {
                                // For devices which are now disconnected, change their measurements to a gray color
                                foreach (RealTimeMeasurement measurement in device.MeasurementList)
                                    measurement.Quality = "N/A";
                            }
                        }
                    }
                }
                finally
                {
                    Interlocked.Exchange(ref m_processingUnsynchronizedMeasurements, 0);
                }
            }
        }

        #region [ Unsynchronized Subscription ]

        private void m_unsynchronizedSubscriber_ConnectionTerminated(object sender, EventArgs e)
        {
            m_subscribedUnsynchronized = false;
            TerminateSubscription();

            if (RestartConnectionCycle)
                InitializeUnsynchronizedSubscription();
        }

        private void m_unsynchronizedSubscriber_NewMeasurements(object sender, EventArgs<ICollection<IMeasurement>> e)
        {
            if (0 == Interlocked.Exchange(ref m_processingUnsynchronizedMeasurements, 1))
            {
                try
                {
                    ObservableCollection<StatisticMeasurement> statisticMeasurements;
                    RealTimeMeasurement realTimeMeasurement;

                    HashSet<RealTimeStream> updatedStreams = new HashSet<RealTimeStream>();
                    HashSet<RealTimeDevice> updatedDevices = new HashSet<RealTimeDevice>();

                    // If it doesn't already exist, create lookup table to allow quick
                    // lookup of RealTimeMeasurements whose value has changed
                    if ((object)m_realTimeMeasurements == null)
                    {
                        m_realTimeMeasurements = ItemsSource
                            .SelectMany(stream => stream.DeviceList)
                            .SelectMany(device => device.MeasurementList)
                            .GroupBy(measurement => measurement.SignalID)
                            .Select(group => group.First())
                            .ToDictionary(measurement => measurement.SignalID);
                    }

                    // Update measurements that have changed
                    foreach (IMeasurement newMeasurement in e.Argument)
                    {
                        if (m_realTimeMeasurements.TryGetValue(newMeasurement.ID, out realTimeMeasurement))
                        {
                            RealTimeDevice parentDevice = realTimeMeasurement.Parent;
                            RealTimeStream parentStream = parentDevice.Parent;

                            if (newMeasurement.Timestamp > realTimeMeasurement.LastUpdated)
                            {
                                realTimeMeasurement.Quality = newMeasurement.ValueQualityIsGood() ? "GOOD" : "BAD";
                                realTimeMeasurement.TimeTag = newMeasurement.Timestamp.ToString("HH:mm:ss.fff");
                                realTimeMeasurement.Value = newMeasurement.AdjustedValue.ToString("0.###");
                                realTimeMeasurement.LastUpdated = newMeasurement.Timestamp;

                                // Maintain collection of updated devices and streams
                                updatedDevices.Add(parentDevice);

                                if ((object)parentStream != null)
                                    updatedStreams.Add(parentStream);
                            }
                        }
                    }

                    // Update status color of the updated devices
                    // NOTE: Color cannot be red since we just received data from the devices
                    foreach (RealTimeDevice device in updatedDevices)
                    {
                        // By default, determine color based on measurement quality
                        List<RealTimeMeasurement> measurementList = device.MeasurementList.Where(m => m.Value != "--").ToList();

                        if (measurementList.Count > 0)
                        {
                            if (measurementList.All(m => m.Quality != "BAD"))
                                device.StatusColor = "Green";
                            else
                                device.StatusColor = "Yellow";
                        }

                        if (device.ID != null && device.ID > 0)
                        {
                            // Check to see if device has statistic measurements which define the number of errors reported by the device
                            if (!RealTimeStatistic.InputStreamStatistics.ContainsKey((int)device.ID))
                            {
                                if (RealTimeStatistic.DevicesWithStatisticMeasurements.TryGetValue((int)device.ID, out statisticMeasurements))
                                {
                                    // If there are any reported errors, force color to yellow
                                    foreach (StatisticMeasurement statisticMeasurement in statisticMeasurements)
                                    {
                                        int value;

                                        if (StatisticsEngine.RegexMatch(statisticMeasurement.SignalReference, "PMU") && int.TryParse(statisticMeasurement.Value, out value) && value > 0)
                                            device.StatusColor = "Yellow";
                                    }
                                }
                            }
                        }
                    }

                    // Update status color of the updated streams
                    // NOTE: Color cannot be red since we just received data from the devices
                    foreach (RealTimeStream stream in updatedStreams)
                    {
                        // Streams with ID of 0 are placeholders
                        // and must remain transparent
                        if (stream.ID > 0)
                        {
                            if (stream.DeviceList.Any(device => device.StatusColor == "Green"))
                                stream.StatusColor = "Green";
                            else
                                stream.StatusColor = "Yellow";
                        }
                    }

                    LastRefresh = "Last Refresh: " + DateTime.UtcNow.ToString("HH:mm:ss.fff");
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
            CommonFunctions.LogException(null, "RealTimeStreams subscription", e.Argument);
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
                    m_unsynchronizedSubscriber.ConnectionString = database.DataPublisherConnectionString();
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
            SubscribeUnsynchronizedData(false);
        }

        public void SubscribeUnsynchronizedData(bool historical)
        {
            UnsynchronizedSubscriptionInfo info;

            if (m_unsynchronizedSubscriber == null)
                InitializeUnsynchronizedSubscription();

            if (m_subscribedUnsynchronized && !string.IsNullOrEmpty(m_allSignalIDs))
            {
                double lagTime;
                double leadTime;

                if (!double.TryParse(IsolatedStorageManager.ReadFromIsolatedStorage("LagTime")?.ToString(), out lagTime))
                    lagTime = 60.0D;

                if (!double.TryParse(IsolatedStorageManager.ReadFromIsolatedStorage("LagTime")?.ToString(), out leadTime))
                    leadTime = 60.0D;

                info = new UnsynchronizedSubscriptionInfo(true);

                info.UseCompactMeasurementFormat = true;
                info.FilterExpression = m_allSignalIDs;
                info.IncludeTime = true;
                info.UseLocalClockAsRealTime = IsolatedStorageManager.ReadFromIsolatedStorage("UseLocalClockAsRealTime").ToNonNullString("true").ParseBoolean();
                info.LagTime = lagTime;
                info.LeadTime = leadTime;
                info.PublishInterval = m_refreshInterval;

                if (historical)
                {
                    info.StartTime = StartTime;
                    info.StopTime = StopTime;
                    info.ProcessingInterval = m_refreshInterval * 1000;
                }

                m_unsynchronizedSubscriber.Subscribe(info);
            }

            if (m_statistics == null)
                Application.Current.Dispatcher.BeginInvoke(new Action(() => Statistics = new RealTimeStatistics(1, m_statisticRefreshInterval)));
        }

        /// <summary>
        /// Unsubscribes data from the service.
        /// </summary>
        public void TerminateSubscription()
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
                    if (m_statistics != null)
                    {
                        m_statistics.Stop();
                        Statistics = null;
                    }
                }
            }
            catch
            {
                m_unsynchronizedSubscriber = null;
            }
        }

        #endregion

        private void CheckTemporalSupport()
        {
            WindowsServiceClient windowsServiceClient = null;
            try
            {
                s_responseWaitHandle = new ManualResetEvent(false);

                windowsServiceClient = CommonFunctions.GetWindowsServiceClient();
                if (windowsServiceClient != null && windowsServiceClient.Helper != null &&
                   windowsServiceClient.Helper.RemotingClient != null && windowsServiceClient.Helper.RemotingClient.CurrentState == ClientState.Connected)
                {
                    windowsServiceClient.Helper.ReceivedServiceResponse += Helper_ReceivedServiceResponse;

                    CommonFunctions.SendCommandToService("TemporalSupport -system");

                    if (!s_responseWaitHandle.WaitOne(10000))
                    {
                        TemporalSupportEnabled = false;
                        //throw new ApplicationException("Response timeout occured. Waited 10 seconds for response.");
                    }
                }
                else
                {
                    //throw new ApplicationException("Connection timeout occured. Tried 10 times to connect to windows service.");
                }
            }
            catch (Exception ex)
            {
                Popup("ERROR: " + ex.Message, "Request Configuration", MessageBoxImage.Error);
            }
            finally
            {
                if (windowsServiceClient != null)
                {
                    windowsServiceClient.Helper.ReceivedServiceResponse -= Helper_ReceivedServiceResponse;
                }
            }
        }

        /// <summary>
        /// Handles ReceivedServiceResponse event.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void Helper_ReceivedServiceResponse(object sender, EventArgs<ServiceResponse> e)
        {
            bool temporalSupportEnabled = false;
            string sourceCommand;
            bool responseSuccess;

            if (ClientHelper.TryParseActionableResponse(e.Argument, out sourceCommand, out responseSuccess))
            {
                if (responseSuccess && bool.TryParse(e.Argument.Attachments[0].ToString(), out temporalSupportEnabled))
                    TemporalSupportEnabled = temporalSupportEnabled;

                s_responseWaitHandle.Set();
            }
        }

        #endregion

        #region [ Static ]

        // Fields

        private static ManualResetEvent s_responseWaitHandle;

        #endregion
    }
}
