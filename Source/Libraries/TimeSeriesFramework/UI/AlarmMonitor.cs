//******************************************************************************************************
//  AlarmMonitor.cs - Gbtc
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
//  02/16/2012 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using GSF.Data;
using GSF.TimeSeriesFramework.UI.DataModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Xml.Linq;

namespace GSF.TimeSeriesFramework.UI
{
    /// <summary>
    /// Represents a monitor that checks a web service in order to
    /// keep an updated list of raised alarms defined by that service.
    /// </summary>
    public class AlarmMonitor : IDisposable
    {
        #region [ Members ]

        // Constants
        private const int DefaultRefreshInterval = 10;
        private const string DefaultSoundFile = "";
        private const int DefaultAlertSeverity = 850;
        private const string DefaultUrl = "http://localhost:5018/alarmservices";

        // Events

        /// <summary>
        /// Event raised when alarms are refreshed.
        /// </summary>
        public event EventHandler<EventArgs> RefreshedAlarms;

        // Fields
        private int m_refreshInterval;
        private string m_url;

        private object m_alarmLock;
        private List<RaisedAlarm> m_alarmList;
        private System.Timers.Timer m_refreshTimer;
        private object m_currentNodeID;

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

            m_alarmLock = new object();
            m_alarmList = new List<RaisedAlarm>();
            m_refreshTimer = new System.Timers.Timer(m_refreshInterval * 1000);
            m_refreshTimer.Elapsed += RefreshTimer_Elapsed;

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
                m_refreshTimer.Interval = value;
                m_refreshInterval = value;
            }
        }

        /// <summary>
        /// Gets or sets the node ID currently
        /// in use by the alarm monitor.
        /// </summary>
        private object CurrentNodeID
        {
            get
            {
                return m_currentNodeID;
            }
            set
            {
                m_currentNodeID = value;
                RefreshUrl();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Starts the refresh timer that checks alarm status.
        /// </summary>
        public void Start()
        {
            if (!m_refreshTimer.Enabled)
            {
                RefreshAlarms();
                m_refreshTimer.Start();
            }
        }

        /// <summary>
        /// Stops the refresh timer.
        /// </summary>
        public void Stop()
        {
            if (m_refreshTimer.Enabled)
                m_refreshTimer.Stop();
        }

        /// <summary>
        /// Gets the current list of raised alarms.
        /// </summary>
        /// <returns></returns>
        public ObservableCollection<RaisedAlarm> GetAlarmList()
        {
            lock (m_alarmLock)
            {
                return new ObservableCollection<RaisedAlarm>(m_alarmList);
            }
        }

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
                    }
                }
                finally
                {
                    m_disposed = true;          // Prevent duplicate dispose.
                }
            }
        }

        // Processing for the Elapsed event of the refresh timer.
        private void RefreshTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            RefreshAlarms();
        }

        // Contacts the web service and refreshes the list of alarms.
        private void RefreshAlarms()
        {
            AdoDataConnection database;

            HttpWebResponse response = null;
            StreamReader reader = null;
            XElement root = null;

            List<RaisedAlarm> newAlarmList;

            try
            {
                // Ensure that the current node ID is the currently selected node ID
                using (database = new AdoDataConnection(CommonFunctions.DefaultSettingsCategory))
                {
                    if (m_currentNodeID != database.CurrentNodeID())
                        CurrentNodeID = database.CurrentNodeID();
                }

                // Make the web request and parse the response as XML
                using (response = MakeWebRequest())
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        reader = new StreamReader(response.GetResponseStream());
                        root = XElement.Parse(reader.ReadToEnd());
                    }
                }

                if (root != null)
                {
                    // Build the new alarm list from the XML data
                    newAlarmList = new List<RaisedAlarm>();

                    foreach (XElement element in root.Element("Alarms").Elements("Alarm"))
                    {
                        newAlarmList.Add(new RaisedAlarm()
                        {
                            ID = int.Parse(element.Element("ID").Value),
                            Severity = int.Parse(element.Element("Severity").Value),
                            TimeRaised = element.Element("TimeRaised").Value,
                            TagName = element.Element("TagName").Value,
                            Description = element.Element("Description").Value,
                            Value = double.Parse(element.Element("ValueAtTimeRaised").Value)
                        });
                    }

                    // Update the alarm list
                    lock (m_alarmLock)
                    {
                        m_alarmList = newAlarmList;
                    }

                    // Notify that the alarms have been updated
                    OnRefreshedAlarms();
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    CommonFunctions.LogException(null, "Refresh Alarms ", ex.InnerException);
                else
                    CommonFunctions.LogException(null, "Refresh Alarms ", ex);
            }
        }

        // Makes a web request, making sure to go into the database
        // and update the alarm URL if the current URL doesn't work.
        private HttpWebResponse MakeWebRequest()
        {
            HttpWebRequest request;

            try
            {
                request = WebRequest.Create(m_url) as HttpWebRequest;
                return request.GetResponse() as HttpWebResponse;
            }
            catch (WebException)
            {
                // Assume the connection could not be made and
                // attempt to get a new URL in case it changed.
                RefreshUrl();

                request = WebRequest.Create(m_url) as HttpWebRequest;
                return request.GetResponse() as HttpWebResponse;
            }
        }

        // Updates the alarm service URL based on the alarm
        // adapter's connection string in the database.
        private void RefreshUrl()
        {
            const string queryFormat = "SELECT Settings FROM Node WHERE ID = '{0}'";

            AdoDataConnection database = null;
            object nodeSettingsConnectionString;
            Dictionary<string, string> nodeSettings;

            try
            {
                // Create database connection and get the node settings
                database = new AdoDataConnection(CommonFunctions.DefaultSettingsCategory);
                nodeSettingsConnectionString = database.Connection.ExecuteScalar(string.Format(queryFormat, m_currentNodeID));

                // Parse the connection string
                nodeSettings = nodeSettingsConnectionString.ToNonNullString().ParseKeyValuePairs();

                // Get the service endpoints
                if (!nodeSettings.TryGetValue("AlarmServiceUrl", out m_url))
                    m_url = DefaultUrl;

                m_url += "/raisedalarms/severe/xml";
            }
            finally
            {
                if ((object)database != null)
                    database.Dispose();
            }
        }

        // Triggers the RefreshedAlarms event.
        private void OnRefreshedAlarms()
        {
            if (RefreshedAlarms != null)
                RefreshedAlarms(this, new EventArgs());
        }

        #endregion

        #region [ Static ]

        /// <summary>
        /// Gets or sets the global reference to a singleton
        /// of the <see cref="AlarmMonitor"/> class.
        /// </summary>
        public static AlarmMonitor Default { get; set; }

        #endregion
    }
}
