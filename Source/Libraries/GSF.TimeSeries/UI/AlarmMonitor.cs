//******************************************************************************************************
//  AlarmMonitor.cs - Gbtc
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
//  02/16/2012 - Stephen C. Wills
//       Generated original version of source code.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Timers;
using GSF.Data;
using GSF.TimeSeries.Transport;
using GSF.TimeSeries.UI.DataModels;
using Timer = System.Timers.Timer;

namespace GSF.TimeSeries.UI
{
    /// <summary>
    /// Represents an alarm monitor, for use inside a management UI application, that gets initial state of raised alarms
    /// from an invoke based console connection command then maintains state with a data subscription to all alarms.
    /// </summary>
    public class AlarmMonitor : IDisposable
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Default refresh interval for alarm monitor.
        /// </summary>
        public const int DefaultRefreshInterval = 10;

        // For future use we may want an alarm sound for a given severity
        //private const string DefaultSoundFile = "";
        //private const int DefaultAlertSeverity = 850;

        // Events

        /// <summary>
        /// Event raised when alarms are refreshed.
        /// </summary>
        public event EventHandler RefreshedAlarms;

        // Fields
        private AlarmStatusQuery m_alarmStatusQuery;
        private DataSubscriber m_dataSubscriber;
        private readonly UnsynchronizedSubscriptionInfo m_subscriptionInfo;
        private volatile Dictionary<int, Alarm> m_definedAlarmsByID;
        private volatile Dictionary<Guid, Alarm> m_definedAlarmsByMeasurement;
        private readonly object m_currentAlarmsLock;
        private readonly ISet<Alarm> m_currentAlarms;
        private int m_alarmsChanged;
        private Timer m_refreshTimer;
        private int m_refreshInterval;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="AlarmMonitor"/> class.
        /// </summary>
        /// <param name="singleton">Indicates whether this instance should update the global reference to become the singleton.</param>
        public AlarmMonitor(bool singleton = false)
        {
            object refreshInterval = IsolatedStorageManager.ReadFromIsolatedStorage("AlarmStatusRefreshInterval") ?? DefaultRefreshInterval;

            if (!int.TryParse(refreshInterval.ToString(), out m_refreshInterval))
                m_refreshInterval = DefaultRefreshInterval;

            m_currentAlarmsLock = new object();
            m_currentAlarms = new HashSet<Alarm>();

            m_refreshTimer = new Timer(m_refreshInterval * 1000);
            m_refreshTimer.Elapsed += RefreshTimer_Elapsed;

            m_alarmStatusQuery = new AlarmStatusQuery();
            m_alarmStatusQuery.RaisedAlarmStates += m_alarmStatusQuery_RaisedAlarmStates;
            m_alarmStatusQuery.ProcessException += m_alarmStatusQuery_ProcessException;

            // Load all alarms defined in the database
            UpdateDefinedAlarms();

            // Setup subscription to subscribe to all alarm measurements
            m_subscriptionInfo = new UnsynchronizedSubscriptionInfo(false)
            {
                FilterExpression = "FILTER ActiveMeasurements WHERE SignalType='ALRM'"
            };

            m_dataSubscriber = new DataSubscriber();
            m_dataSubscriber.ConnectionEstablished += m_dataSubscriber_ConnectionEstablished;
            m_dataSubscriber.ReceivedServerResponse += m_dataSubscriber_ReceivedServerResponse;
            m_dataSubscriber.NewMeasurements += m_dataSubscriber_NewMeasurements;
            m_dataSubscriber.ProcessException += m_dataSubscriber_ProcessException;
            m_dataSubscriber.ConnectionString = GetDataPublisherConnectionString();
            m_dataSubscriber.OperationalModes |= OperationalModes.UseCommonSerializationFormat | OperationalModes.CompressSignalIndexCache;
            m_dataSubscriber.DataLossInterval = -1;
            m_dataSubscriber.Initialize();

            if (singleton)
                Default = this;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the interval, in seconds,
        /// between refreshing the alarm list.
        /// </summary>
        public int RefreshInterval
        {
            get
            {
                return m_refreshInterval;
            }
            set
            {
                m_refreshTimer.Interval = value * 1000;
                m_refreshInterval = value;
            }
        }

        /// <summary>
        /// Gets the current list of raised alarms.
        /// </summary>
        /// <returns>Current <see cref="RaisedAlarm"/> list.</returns>
        public ObservableCollection<RaisedAlarm> GetAlarmList()
        {
            IEnumerable<RaisedAlarm> currentHighestSeverityAlarms;

            lock (m_currentAlarmsLock)
            {
                currentHighestSeverityAlarms = m_currentAlarms
                    .GroupBy(alarm => alarm.SignalID)
                    .SelectMany(GetHighestSeverityAlarms)
                    .Select(CreateRaisedAlarm);

                return new ObservableCollection<RaisedAlarm>(currentHighestSeverityAlarms);
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or
        /// resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="AlarmMonitor"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                    {
                        if ((object)m_refreshTimer != null)
                        {
                            m_refreshTimer.Dispose();
                            m_refreshTimer = null;
                        }

                        if ((object)m_alarmStatusQuery != null)
                        {
                            m_alarmStatusQuery.RaisedAlarmStates -= m_alarmStatusQuery_RaisedAlarmStates;
                            m_alarmStatusQuery.ProcessException -= m_alarmStatusQuery_ProcessException;
                            m_alarmStatusQuery.Dispose();
                            m_alarmStatusQuery = null;
                        }

                        if ((object)m_dataSubscriber != null)
                        {
                            m_dataSubscriber.ConnectionEstablished -= m_dataSubscriber_ConnectionEstablished;
                            m_dataSubscriber.ReceivedServerResponse -= m_dataSubscriber_ReceivedServerResponse;
                            m_dataSubscriber.NewMeasurements -= m_dataSubscriber_NewMeasurements;
                            m_dataSubscriber.ProcessException -= m_dataSubscriber_ProcessException;
                            m_dataSubscriber.Dispose();
                            m_dataSubscriber = null;
                        }
                    }
                }
                finally
                {
                    m_disposed = true;          // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Starts the refresh timer that notifies consumer about the current alarm status.
        /// </summary>
        public void Start()
        {
            if (!m_refreshTimer.Enabled)
            {
                // Start subscriber connection cycle
                m_dataSubscriber.Start();

                // Start the refresh timer
                m_refreshTimer.Start();
            }
        }

        /// <summary>
        /// Updates the collection of defined system alarms.
        /// </summary>
        public void UpdateDefinedAlarms()
        {
            IEnumerable<Alarm> definedAlarms = LoadDefinedAlarms();
            List<Alarm> oldCurrentAlarms;
            Alarm newCurrentAlarm;

            // Initialize lookup table for defined alarms by ID
            m_definedAlarmsByID = definedAlarms.ToDictionary(alarm => alarm.ID);

            // Initialize lookup table for defined alarms by associated measurement ID
            m_definedAlarmsByMeasurement = definedAlarms
                .Where(alarm => alarm.AssociatedMeasurementID.HasValue)
                .GroupBy(alarm => alarm.AssociatedMeasurementID.GetValueOrDefault())
                .ToDictionary(group => group.Key, group => group.First());

            lock (m_currentAlarmsLock)
            {
                // Copy current alarms set to a list so we can iterate while removing
                oldCurrentAlarms = new List<Alarm>(m_currentAlarms);

                foreach (Alarm oldCurrentAlarm in oldCurrentAlarms)
                {
                    // Remove the old alarm from the set of current alarms
                    m_currentAlarms.Remove(oldCurrentAlarm);

                    // If the old alarm is still defined in the updated alarms table,
                    // add the equivalent new defined alarm to the set of current alarms
                    if (m_definedAlarmsByID.TryGetValue(oldCurrentAlarm.ID, out newCurrentAlarm))
                    {
                        newCurrentAlarm.TimeRaised = oldCurrentAlarm.TimeRaised;
                        m_currentAlarms.Add(newCurrentAlarm);
                    }
                }
            }

            // Set alarms changed state to false
            Interlocked.Exchange(ref m_alarmsChanged, 0);

            // Notify that the alarms have been updated
            OnRefreshedAlarms();
        }

        /// <summary>
        /// Stops the refresh timer.
        /// </summary>
        public void Stop()
        {
            if (m_refreshTimer.Enabled)
            {
                // Stop the refresh timer
                m_refreshTimer.Stop();

                // Stop data subscriber
                m_dataSubscriber.Stop();
            }
        }

        // Processing for the Elapsed event of the refresh timer.
        private void RefreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // No need to raise event unless current alarm state has changed
            if (Interlocked.CompareExchange(ref m_alarmsChanged, 0, 1) != 0)
                OnRefreshedAlarms();
        }

        // Gets the current data publisher connection string
        private string GetDataPublisherConnectionString()
        {
            AdoDataConnection database = null;

            try
            {
                // Create database connection to get the current data publisher connection string
                database = new AdoDataConnection(CommonFunctions.DefaultSettingsCategory);
                return database.DataPublisherConnectionString();
            }
            finally
            {
                if ((object)database != null)
                    database.Dispose();
            }
        }

        // Loads existing alarms defined in the database
        private IEnumerable<Alarm> LoadDefinedAlarms()
        {
            // Create alarms using definitions from the database
            AdoDataConnection database = null;

            try
            {
                // Create database connection to get currently defined alarms
                database = new AdoDataConnection(CommonFunctions.DefaultSettingsCategory);
                string query = database.ParameterizedQueryString("SELECT * FROM Alarm WHERE NodeID = {0} AND Enabled <> 0", "nodeID");
                DataTable alarms = database.Connection.RetrieveData(database.AdapterType, query, database.CurrentNodeID());

                return alarms.Rows.Cast<DataRow>()
                    .Select(CreateAlarm)
                    .ToList();
            }
            finally
            {
                if ((object)database != null)
                    database.Dispose();
            }
        }

        // Creates an alarm using a data row queried from the database
        private Alarm CreateAlarm(DataRow row)
        {
            object associatedMeasurementId = row.Field<object>("AssociatedMeasurementID");

            return new Alarm
            {
                ID = row.ConvertField<int>("ID"),
                TagName = row.Field<object>("TagName").ToString(),
                SignalID = Guid.Parse(row.Field<object>("SignalID").ToString()),
                AssociatedMeasurementID = ((object)associatedMeasurementId != null) ? Guid.Parse(associatedMeasurementId.ToString()) : (Guid?)null,
                Description = row.Field<object>("Description").ToNonNullString(),
                Severity = row.ConvertField<int>("Severity").GetEnumValueOrDefault<AlarmSeverity>(AlarmSeverity.None),
                Operation = row.ConvertField<int>("Operation").GetEnumValueOrDefault<AlarmOperation>(AlarmOperation.Equal),
                SetPoint = row.ConvertNullableField<double>("SetPoint"),
                Tolerance = row.ConvertNullableField<double>("Tolerance"),
                Delay = row.ConvertNullableField<double>("Delay"),
                Hysteresis = row.ConvertNullableField<double>("Hysteresis")
            };
        }

        // Creates a data model based "RaisedAlarm" from an alarm
        private RaisedAlarm CreateRaisedAlarm(Alarm alarm)
        {
            return new RaisedAlarm
            {
                ID = alarm.ID,
                Severity = (int)alarm.Severity,
                TimeRaised = ((DateTime)alarm.TimeRaised).ToString("MM/dd/yyyy HH:mm:ss.fff"),
                TagName = alarm.TagName,
                Description = alarm.Description
            };
        }

        // Get the defined alarm for the given alarm event
        private Alarm GetDefinedAlarm(IMeasurement alarmEvent)
        {
            Dictionary<Guid, Alarm> definedAlarmsByMeasurement;
            Alarm definedAlarm;

            definedAlarmsByMeasurement = m_definedAlarmsByMeasurement;

            if (definedAlarmsByMeasurement.TryGetValue(alarmEvent.ID, out definedAlarm))
                definedAlarm.TimeRaised = alarmEvent.Timestamp;

            return definedAlarm;
        }

        // Get all the highest severity alarms in a list of alarms
        private IEnumerable<Alarm> GetHighestSeverityAlarms(IEnumerable<Alarm> alarms)
        {
            AlarmSeverity highestSeverity = alarms.Max(alarm => alarm.Severity);
            return alarms.Where(alarm => alarm.Severity == highestSeverity);
        }

        // Triggers the RefreshedAlarms event.
        private void OnRefreshedAlarms()
        {
            if (RefreshedAlarms != null)
                RefreshedAlarms(this, EventArgs.Empty);
        }

        private void m_alarmStatusQuery_RaisedAlarmStates(object sender, EventArgs<ICollection<Alarm>> e)
        {
            Dictionary<int, Alarm> definedAlarmsByID;
            Alarm definedAlarm;

            definedAlarmsByID = m_definedAlarmsByID;

            // Received the initial set of alarms - cache them as the "current" raised alarm state
            lock (m_currentAlarmsLock)
            {
                foreach (Alarm raisedAlarm in e.Argument)
                {
                    // We can only add defined alarms to the set of current
                    // alarms to allow for fast set-based operations later
                    if (definedAlarmsByID.TryGetValue(raisedAlarm.ID, out definedAlarm))
                    {
                        definedAlarm.TimeRaised = raisedAlarm.TimeRaised;
                        m_currentAlarms.Add(definedAlarm);
                    }
                }
            }

            // Set alarms changed state to false
            Interlocked.Exchange(ref m_alarmsChanged, 0);

            // Notify that the alarms have been updated
            OnRefreshedAlarms();
        }

        private void m_alarmStatusQuery_ProcessException(object sender, EventArgs<Exception> e)
        {
            CommonFunctions.LogException(null, "AlarmMonitor", e.Argument);
        }

        private void m_dataSubscriber_ConnectionEstablished(object sender, EventArgs e)
        {
            lock (m_currentAlarmsLock)
            {
                m_currentAlarms.Clear();
            }

            m_dataSubscriber.Subscribe(m_subscriptionInfo);
        }

        private void m_dataSubscriber_ReceivedServerResponse(object sender, EventArgs<ServerResponse, ServerCommand> e)
        {
            // Request the latest alarm states after subscription is established to ensure all alarm states are up-to-date
            if (e.Argument1 == ServerResponse.Succeeded && e.Argument2 == ServerCommand.Subscribe)
                m_alarmStatusQuery.RequestRaisedAlarmStates();
        }

        private void m_dataSubscriber_NewMeasurements(object sender, EventArgs<ICollection<IMeasurement>> e)
        {
            Dictionary<Guid, Alarm> definedAlarmsByMeasurement = m_definedAlarmsByMeasurement;
            ICollection<IMeasurement> latestMeasurements;
            Alarm definedAlarm = null;

            IEnumerable<Alarm> clearedAlarms;
            IEnumerable<Alarm> raisedAlarms;

            // Get the most recent measurement for each alarm signal
            latestMeasurements = e.Argument
                .GroupBy(measurement => measurement.ID)
                .Select(group => group.Last())
                .ToList();

            // Get alarms for measurements that have been cleared (i.e., value == 0)
            clearedAlarms = latestMeasurements
                .Where(measurement => measurement.AdjustedValue == 0.0D)
                .Where(measurement => definedAlarmsByMeasurement.TryGetValue(measurement.ID, out definedAlarm))
                .Select(measurement => definedAlarm)
                .ToList();

            // Get alarms for measurements that have been raised (i.e., value == 1)
            raisedAlarms = latestMeasurements
                .Where(measurement => measurement.AdjustedValue != 0.0D)
                .Select(GetDefinedAlarm)
                .Where(alarm => (object)alarm != null)
                .ToList();

            // Handle any changes to current raised alarm list
            if (raisedAlarms.Any() || clearedAlarms.Any())
            {
                // Update current alarms - removing then adding alarms
                lock (m_currentAlarmsLock)
                {
                    m_currentAlarms.ExceptWith(clearedAlarms);
                    m_currentAlarms.UnionWith(raisedAlarms);
                }

                // Set alarms changed state to true
                Interlocked.Exchange(ref m_alarmsChanged, 1);
            }
        }

        private void m_dataSubscriber_ProcessException(object sender, EventArgs<Exception> e)
        {
            bool logException = true;
            SocketException ex = e.Argument as SocketException;

            if ((object)ex == null && (object)e.Argument.InnerException != null)
                ex = e.Argument.InnerException as SocketException;

            if ((object)ex != null)
                logException = (ex.SocketErrorCode != SocketError.ConnectionRefused);

            if (logException)
                CommonFunctions.LogException(null, "AlarmMonitor", e.Argument);
        }

        #region [ Old Code ]

        //// Contacts the web service and refreshes the list of alarms.
        //private void RefreshAlarms()
        //{
        //    AdoDataConnection database;

        //    HttpWebResponse response = null;
        //    StreamReader reader = null;
        //    XElement root = null;

        //    List<RaisedAlarm> newAlarmList;

        //    try
        //    {
        //        // Ensure that the current node ID is the currently selected node ID
        //        using (database = new AdoDataConnection(CommonFunctions.DefaultSettingsCategory))
        //        {
        //            if (m_currentNodeID != database.CurrentNodeID())
        //                CurrentNodeID = database.CurrentNodeID();
        //        }

        //        // Make the web request and parse the response as XML
        //        using (response = MakeWebRequest())
        //        {
        //            if (response.StatusCode == HttpStatusCode.OK)
        //            {
        //                reader = new StreamReader(response.GetResponseStream());
        //                root = XElement.Parse(reader.ReadToEnd());
        //            }
        //        }

        //        if (root != null)
        //        {
        //            // Build the new alarm list from the XML data
        //            newAlarmList = new List<RaisedAlarm>();

        //            foreach (XElement element in root.Element("Alarms").Elements("Alarm"))
        //            {
        //                newAlarmList.Add(new RaisedAlarm
        //                {
        //                    ID = int.Parse(element.Element("ID").Value),
        //                    Severity = int.Parse(element.Element("Severity").Value),
        //                    TimeRaised = element.Element("TimeRaised").Value,
        //                    TagName = element.Element("TagName").Value,
        //                    Description = element.Element("Description").Value,
        //                    Value = double.Parse(element.Element("ValueAtTimeRaised").Value)
        //                });
        //            }

        //            // Update the alarm list
        //            lock (m_alarmLock)
        //            {
        //                m_alarmList = newAlarmList;
        //            }

        //            // Notify that the alarms have been updated
        //            OnRefreshedAlarms();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex.InnerException != null)
        //            CommonFunctions.LogException(null, "Refresh Alarms ", ex.InnerException);
        //        else
        //            CommonFunctions.LogException(null, "Refresh Alarms ", ex);
        //    }
        //}

        //// Makes a web request, making sure to go into the database
        //// and update the alarm URL if the current URL doesn't work.
        //private HttpWebResponse MakeWebRequest()
        //{
        //    HttpWebRequest request;

        //    try
        //    {
        //        request = WebRequest.Create(m_url) as HttpWebRequest;
        //        return request.GetResponse() as HttpWebResponse;
        //    }
        //    catch (WebException)
        //    {
        //        // Assume the connection could not be made and
        //        // attempt to get a new URL in case it changed.
        //        RefreshUrl();

        //        request = WebRequest.Create(m_url) as HttpWebRequest;
        //        return request.GetResponse() as HttpWebResponse;
        //    }
        //}

        //// Updates the alarm service URL based on the alarm
        //// adapter's connection string in the database.
        //private void RefreshUrl()
        //{
        //    const string queryFormat = "SELECT Settings FROM Node WHERE ID = '{0}'";

        //    AdoDataConnection database = null;
        //    object nodeSettingsConnectionString;
        //    Dictionary<string, string> nodeSettings;

        //    try
        //    {
        //        // Create database connection and get the node settings
        //        database = new AdoDataConnection(CommonFunctions.DefaultSettingsCategory);
        //        nodeSettingsConnectionString = database.Connection.ExecuteScalar(string.Format(queryFormat, m_currentNodeID));

        //        // Parse the connection string
        //        nodeSettings = nodeSettingsConnectionString.ToNonNullString().ParseKeyValuePairs();

        //        // Get the service endpoints
        //        if (!nodeSettings.TryGetValue("AlarmServiceUrl", out m_url))
        //            m_url = DefaultUrl;

        //        m_url += "/raisedalarms/severe/xml";
        //    }
        //    finally
        //    {
        //        if ((object)database != null)
        //            database.Dispose();
        //    }
        //}

        #endregion

        #endregion

        #region [ Static ]

        /// <summary>
        /// Gets or sets the global reference to a singleton
        /// of the <see cref="AlarmMonitor"/> class.
        /// </summary>
        public static AlarmMonitor Default
        {
            get;
            set;
        }

        #endregion
    }
}
