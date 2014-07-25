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
//  06/18/2014 - J. Ritchie Carroll
//       Updated code to use PIConnection instance.
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
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class PIPBInputAdapter : InputAdapterBase
    {
        #region [ Members ]

        // Constants
        private const int QueryTimeSpan = 5;                        // Minutes of data to pull per query

        // Fields
        private PIConnection m_connection;                          // PI server connection from which this adapter connects to get data
        private string m_serverName;                                // server name where PI connection should be established for connection string
        private string m_userName;                                  // username for PI connection string
        private string m_password;                                  // password for PI connection string
        private int m_connectTimeout;                               // PI connection timeout
        private string m_tagFilter;                                 // caching the string used to filter down to PI points that can be provided from this adapter
        private PointList m_points;                                 // List of points this adapter queries from PI
        private Dictionary<string, MeasurementKey> m_tagKeyMap;     // Provides quick look ups of GSFSchema keys by PI point tag
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
            m_tagKeyMap = new Dictionary<string, MeasurementKey>();
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

                    if ((object)value != null)
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
                return m_serverName;
            }
            set
            {
                m_serverName = value;
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

        /// <summary>
        /// Gets or sets the timeout interval (in milliseconds) for the adapter's connection
        /// </summary>
        [ConnectionStringParameter, Description("Defines the timeout interval (in milliseconds) for the adapter's connection"), DefaultValue(PIConnection.DefaultConnectTimeout)]
        public int ConnectTimeout
        {
            get
            {
                return m_connectTimeout;
            }
            set
            {
                m_connectTimeout = value;
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
                        m_serverName = null;
                        m_tagFilter = null;

                        if ((object)m_connection != null)
                            m_connection.Dispose();

                        m_connection = null;
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

            Dictionary<string, string> settings = Settings;
            string setting;

            if (!settings.TryGetValue("ServerName", out m_serverName))
                throw new InvalidOperationException("Server name is a required setting for PI connections. Please add a server in the format servername=myservername to the connection string.");

            if (settings.TryGetValue("UserName", out setting))
                m_userName = setting;
            else
                m_userName = null;

            if (settings.TryGetValue("Password", out setting))
                m_password = setting;
            else
                m_password = null;

            if (settings.TryGetValue("ConnectTimeout", out setting))
                m_connectTimeout = Convert.ToInt32(setting);
            else
                m_connectTimeout = PIConnection.DefaultConnectTimeout;

            m_measurements = new List<IMeasurement>();
        }

        /// <summary>
        /// Returns a 1-line status for time-series host console
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
            m_processedMeasurements = 0;

            m_connection = new PIConnection
            {
                ServerName = m_serverName,
                UserName = m_userName,
                Password = m_password,
                ConnectTimeout = m_connectTimeout
            };

            m_connection.Open();
        }

        /// <summary>
        /// Disconnects from the configured PI server, if the server is currently connected
        /// </summary>
        protected override void AttemptDisconnection()
        {
            if ((object)m_connection != null)
            {
                m_connection.Dispose();
                m_connection = null;
            }
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
                m_connection.Execute(server => m_points = server.GetPoints(m_tagFilter));

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

                    DateTime localCurrentTime = currentTime.ToLocalTime();
                    DateTime localEndTime = endTime.ToLocalTime();
                    List<IMeasurement> measToAdd = new List<IMeasurement>();

                    m_connection.Execute(server =>
                    {
                        foreach (PIPoint point in m_points)
                        {
                            PIValues values = point.Data.RecordedValues(localCurrentTime, localEndTime);

                            foreach (PIValue value in values)
                            {
                                if (!value.Value.GetType().IsCOMObject)
                                {
                                    Measurement measurement = new Measurement();
                                    measurement.Key = m_tagKeyMap[point.Name];
                                    measurement.Value = (double)value.Value;
                                    measurement.Timestamp = value.TimeStamp.LocalDate.ToUniversalTime();
                                    measToAdd.Add(measurement);
                                }
                            }
                        }
                    });

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
