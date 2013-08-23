//******************************************************************************************************
//  PIPBInputAdapter.cs - Gbtc
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
//  08/13/2012 - Ryan McCoy
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using GSF;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using PISDK;

namespace PIAdapters
{
    /// <summary>
    /// Retrieves PI data for non-real time temporal sessions. The real-time PI input adapter is preferred for real-time data due to use of event pipes.
    /// </summary>
    public class PIPBInputAdapter : InputAdapterBase
    {
        #region [ Members ]

        // Constants
        private const int QueryTimeSpan = 5;                        // Minutes of data to pull per query

        // Fields
        private string m_userName;                                  // username for PI connection string
        private string m_password;                                  // password for PI connection string
        private string m_servername;                                // server name where PI connection should be established for connection string
        private string m_tagFilter;                                 // caching the string used to filter down to PI points that can be provided from this adapter
        private PISDK.PISDK m_pisdk;                                // PI SDK object
        private Server m_server;                                    // PI server from which this adapter connects to get data
        private PointList m_points;                                 // List of points this adapter queries from PI
        private Dictionary<string, MeasurementKey> m_tagKeyMap;     // Provides quick look ups of openPDC keys by PI point tag
        private List<IMeasurement> m_measurements;                  // Queried measurements that are prepared to be published  
        private int m_processedMeasurements;                        // Track number of measurements queried from PI for statistics
        private DateTime m_publishTime = DateTime.MinValue;         // The timestamp that is currently being published
        private DateTime m_queryTime = DateTime.MinValue;           // The timestamp that is currently being queried from PI
        private Thread m_dataThread;                                // Thread to run queries
        private System.Timers.Timer m_publishTimer;                 // Timer to publish data
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="PIPBInputAdapter"/> with default values for members
        /// </summary>
        public PIPBInputAdapter()
        {
            m_userName = string.Empty;
            m_password = string.Empty;
            m_servername = string.Empty;
            m_tagKeyMap = new Dictionary<string, MeasurementKey>();
            m_pisdk = new PISDK.PISDK();
            m_measurements = new List<IMeasurement>();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Returns false to indicate that this adapter connects synchronously when AttemptConnection is called
        /// </summary>
        protected override bool UseAsyncConnect
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Returns true to indicate that this adapter can process temporal data
        /// </summary>
        public override bool SupportsTemporalProcessing
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Represents the measurements that this adapter is currently providing because they are being used in the framework
        /// </summary>
        public override MeasurementKey[] RequestedOutputMeasurementKeys
        {
            get
            {
                return base.RequestedOutputMeasurementKeys;
            }
            set
            {
                // only run the adapter in playback temporal constraints
                if (StartTimeConstraint != DateTime.MinValue)
                {
                    base.RequestedOutputMeasurementKeys = value;

                    if (value != null)
                        HandleNewMeasurementsRequest(value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of the PI server for the adapter's PI connection.
        /// </summary>
        [ConnectionStringParameter, Description("Defines the name of the PI server for the adapter's PI connection.")]
        public string ServerName
        {
            get
            {
                return m_servername;
            }
            set
            {
                m_servername = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the PI user ID for the adapter's PI connection.
        /// </summary>
        [ConnectionStringParameter, Description("Defines the name of the PI user ID for the adapter's PI connection.")]
        public string UserName
        {
            get
            {
                return m_userName;
            }
            set
            {
                m_userName = value;
            }
        }

        /// <summary>
        /// Gets or sets the password used for the adapter's PI connection.
        /// </summary>
        [ConnectionStringParameter, Description("Defines the password used for the adapter's PI connection.")]
        public string Password
        {
            get
            {
                return m_password;
            }
            set
            {
                m_password = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="PIPBInputAdapter"/> object and optionally releases the managed resources.
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
                        m_userName = null;
                        m_password = null;
                        m_servername = null;
                        m_tagFilter = null;

                        m_pisdk = null;
                        m_server = null;
                        m_points = null;
                        m_tagKeyMap.Clear();
                        m_tagKeyMap = null;
                        m_measurements.Clear();
                        m_measurements = null;

                        if (m_dataThread != null)
                        {
                            m_dataThread.Abort();
                            m_dataThread = null;
                        }

                        if (m_publishTimer != null)
                        {
                            m_publishTimer.Stop();
                            m_publishTimer.Elapsed -= m_publishTimer_Tick;
                            m_publishTimer.Dispose();
                            m_publishTimer = null;
                        }
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
        /// Reads parameters from the connection string
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            m_processedMeasurements = 0;

            Dictionary<string, string> settings = Settings;

            if (!settings.TryGetValue("servername", out m_servername))
                throw new InvalidOperationException("Server name is a required setting for PI connections. Please add a server in the format server=myservername to the connection string.");

            settings.TryGetValue("username", out m_userName);
            settings.TryGetValue("password", out m_password);

            m_measurements = new List<IMeasurement>();
        }

        /// <summary>
        /// Returns a 1-line status for openPDC console
        /// </summary>
        /// <param name="maxLength">Maximum number of characters in the status</param>
        /// <returns>status message</returns>
        public override string GetShortStatus(int maxLength)
        {
            return string.Format("Received {0} measurements from PI...", m_processedMeasurements).CenterText(maxLength);
        }

        /// <summary>
        /// Connects to the configured PI server.
        /// </summary>
        protected override void AttemptConnection()
        {
            m_server = m_pisdk.Servers[m_servername];

            // PI server only allows independent connections to the same PI server from STA threads.
            // We're spinning up a thread here to connect STA, since our current thread is MTA.
            ManualResetEvent connectionEvent = new ManualResetEvent(false);
            Thread connectThread = new Thread(() =>
            {
                try
                {
                    if (!string.IsNullOrEmpty(m_userName) && !string.IsNullOrEmpty(m_password))
                        m_server.Open(string.Format("UID={0};PWD={1}", m_userName, m_password));
                    else
                        m_server.Open();
                    connectionEvent.Set();
                }
                catch (Exception e)
                {
                    OnProcessException(e);
                }
            });

            connectThread.SetApartmentState(ApartmentState.STA);
            connectThread.Start();

            if (!connectionEvent.WaitOne(30000))
            {
                connectThread.Abort();
                throw new InvalidOperationException("Timeout occurred while connecting to configured PI server.");
            }
        }

        /// <summary>
        /// Disconnects from the configured PI server, if the server is currently connected
        /// </summary>
        protected override void AttemptDisconnection()
        {
            if (m_server != null && m_server.Connected)
                m_server.Close();
        }

        /// <summary>
        /// Prepares the adapter to query data for the points that have been requested for connect on demand. The adapter will
        /// look up PI tag names and start up the thread to start the timers and threads necessary to run.
        /// </summary>
        /// <param name="keys"></param>
        private void HandleNewMeasurementsRequest(MeasurementKey[] keys)
        {
            if (!IsConnected)
                AttemptConnection();

            StopGettingData();

            if (keys != null && keys.Length > 0)
            {
                var query = from row in DataSource.Tables["ActiveMeasurements"].AsEnumerable()
                            from key in keys
                            where row["ID"].ToString().Split(':')[1] == key.ID.ToString() && row["PROTOCOL"].ToString() == "PI"
                            select new
                            {
                                Key = key,
                                AlternateTag = row["ALTERNATETAG"].ToString(),
                                PointTag = row["POINTTAG"].ToString()
                            };

                StringBuilder whereClause = new StringBuilder();

                foreach (var result in query)
                {
                    string tagname = result.PointTag;
                    if (!String.IsNullOrWhiteSpace(result.AlternateTag))
                        tagname = result.AlternateTag;

                    if (!m_tagKeyMap.ContainsKey(tagname))
                        m_tagKeyMap.Add(tagname, result.Key);

                    if (whereClause.Length > 0)
                        whereClause.Append(" OR ");

                    whereClause.Append(string.Format("tag='{0}'", tagname));
                }

                // Store this filtering string for when the PointList is created during query
                m_tagFilter = whereClause.ToString();

                ThreadPool.QueueUserWorkItem(StartGettingData);
            }
        }

        private void StartGettingData(object state)
        {
            try
            {
                m_points = m_server.GetPoints(m_tagFilter);

                m_dataThread = new Thread(QueryData);
                m_dataThread.IsBackground = true;
                m_dataThread.Start();

                m_publishTimer = new System.Timers.Timer();
                m_publishTimer.Interval = ProcessingInterval;
                m_publishTimer.Elapsed += m_publishTimer_Tick;
                m_publishTimer.Start();
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception e)
            {
                OnProcessException(e);
            }
        }

        private void StopGettingData()
        {
            try
            {
                if (m_publishTimer != null)
                {
                    m_publishTimer.Elapsed -= m_publishTimer_Tick;
                    m_publishTimer.Stop();
                    m_publishTimer.Dispose();
                }
                m_publishTimer = null;


                if (m_dataThread != null)
                    m_dataThread.Abort();

                m_dataThread = null;

                m_points = null;
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                OnProcessException(ex);
            }
        }

        private void QueryData()
        {
            DateTime currentTime = StartTimeConstraint;

            while (currentTime <= StopTimeConstraint)
            {
                try
                {
                    DateTime endTime = currentTime.AddMinutes(QueryTimeSpan);

                    List<IMeasurement> measToAdd = new List<IMeasurement>();
                    foreach (PIPoint point in m_points)
                    {
                        PIValues values = point.Data.RecordedValues(currentTime.ToLocalTime(), endTime.ToLocalTime());
                        foreach (PIValue value in values)
                        {
                            if (!value.Value.GetType().IsCOMObject)
                            {
                                Measurement measurement = new Measurement();
                                measurement.Key = m_tagKeyMap[point.Name];
                                measurement.ID = measurement.Key.SignalID;
                                measurement.Value = (double)value.Value;
                                measurement.Timestamp = value.TimeStamp.LocalDate.ToUniversalTime();
                                measToAdd.Add(measurement);
                            }
                        }
                    }

                    if (measToAdd.Any())
                    {
                        lock (m_measurements)
                        {
                            foreach (IMeasurement meas in measToAdd)
                            {
                                m_measurements.Add(meas);
                                m_processedMeasurements++;
                            }
                        }
                    }

                    currentTime = endTime;
                    m_queryTime = currentTime;
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    OnProcessException(e);
                }

                Thread.Sleep(33);
            }
        }

        private void m_publishTimer_Tick(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (m_queryTime > m_publishTime)
                {
                    if (m_publishTime < StartTimeConstraint)
                        m_publishTime = StartTimeConstraint;

                    m_publishTime = m_publishTime.AddMilliseconds(33);

                    List<IMeasurement> publishMeasurements = new List<IMeasurement>();
                    foreach (IMeasurement measurement in m_measurements.ToArray())
                    {
                        if (measurement.Timestamp <= m_publishTime.Ticks)
                        {
                            publishMeasurements.Add(measurement);

                            lock (m_measurements)
                            {
                                m_measurements.Remove(measurement);
                            }
                        }
                    }

                    if (publishMeasurements.Count > 0)
                        OnNewMeasurements(publishMeasurements);
                }
            }
            catch (Exception ex)
            {
                OnProcessException(ex);
            }
        }

        #endregion
    }
}
