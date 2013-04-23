//******************************************************************************************************
//  PIRTInputAdapter.cs - Gbtc
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
//  08/19/2012 - Ryan McCoy
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using PISDK;
using TimeSeriesFramework;
using TimeSeriesFramework.Adapters;
using TVA;

namespace PIAdapters
{
    /// <summary>
    /// Uses PI event pipes to deliver real-time PI data to openPDC
    /// </summary>
    [Description("PI : Reads real-time measurements from a PI server using PI SDK.")]
    public class PIRTInputAdapter : InputAdapterBase
    {
        #region [ Members ]

        // Constants
        private readonly DateTime EPOCH = new DateTime(1970, 1, 1, 0, 0, 0);  // Date used to calculated delta on timestamps from PI

        // Fields
        private ConcurrentDictionary<string, MeasurementKey> m_tagKeyMap;     // Map PI tagnames to openPDC measurement keys
        private string m_userName;                                            // Username for PI connection string
        private string m_password;                                            // Password for PI connection string
        private string m_servername;                                          // Server for PI connection string
        private PISDK.PISDK m_pi;                                             // object for PI SDK access
        private Server m_server;                                              // server for PI connection
        private PointList m_points;                                           // PI pointlist of points to which subscription should be made
        private EventPipe m_pipe;                                             // event pipe object raises an event when a subscribed point is updated
        private int m_processedMeasurements = 0;                              // processed measurements for short status
        private bool m_autoAddOutput = false;                                 // whether or not to automatically add PI points
        private DateTime m_lastReceivedTimestamp;                             // last received timestamp from PI

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Receives real-time updates from PI
        /// </summary>
        public PIRTInputAdapter()
        {
            m_tagKeyMap = new ConcurrentDictionary<string, MeasurementKey>();
            m_pi = new PISDK.PISDK();

            m_userName = string.Empty;
            m_password = string.Empty;
            m_servername = string.Empty;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Returns false to indicate that this <see cref="PIRTInputAdapter"/> does NOT connect asynchronously
        /// </summary>
        protected override bool UseAsyncConnect
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Returns false to indicate that this <see cref="PIRTInputAdapter"/> does NOT support temporal processing. 
        /// Temporal processing is supported in a separate adapter that is not driven by event pipes.
        /// </summary>
        public override bool SupportsTemporalProcessing
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets or sets the measurements that this <see cref="PIRTInputAdapter"/> has been requested to provide
        /// </summary>
        public override MeasurementKey[] RequestedOutputMeasurementKeys
        {
            get
            {
                return base.RequestedOutputMeasurementKeys;
            }
            set
            {
                base.RequestedOutputMeasurementKeys = value;
                if (value != null && value.Any())
                    HandleNewMeasurementsRequest(value);
            }
        }

        /// <summary>
        /// Gets or sets the name of the PI server for the adapter's PI connection.
        /// </summary>
        [ConnectionStringParameter,
         Description("Defines the name of the PI server for the adapter's PI connection.")]
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

        /// <summary>
        /// Enables or disables adapter's ability to automatically set output measurements.
        /// </summary>
        [ConnectionStringParameter, Description("Enables or disables adapter's ability to automatically set output measurements."), DefaultValue(false)]
        public bool AutoAddOutput
        {
            get
            {
                return m_autoAddOutput;
            }
            set
            {
                m_autoAddOutput = value;
            }
        }

        /// <summary>
        /// Last timestamp received from PI
        /// </summary>
        public DateTime LastReceivedTimestamp
        {
            get
            {
                return m_lastReceivedTimestamp;
            }
        }

        public override string Status
        {
            get
            {
                string status = string.Format("   Last Received Timestamp: {0}", m_lastReceivedTimestamp.ToString("MM/dd/yyyy HH:mm:ss.fff")) + Environment.NewLine;
                status += string.Format("              Last Latency: {0}s", (DateTime.UtcNow - m_lastReceivedTimestamp).TotalSeconds) + Environment.NewLine;
                return status + base.Status;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Reads values from the connection string and prepares this <see cref="PIRTInputAdapter"/> for connecting to PI
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            Dictionary<string, string> settings = Settings;
            string setting;

            if (!settings.TryGetValue("ServerName", out m_servername))
                throw new InvalidOperationException("Server name is a required setting for PI connections. Please add a server in the format server=myservername to the connection string.");

            if (!settings.TryGetValue("UserName", out setting))
                m_userName = setting;
            else
                m_userName = null;

            if (!settings.TryGetValue("Password", out setting))
                m_password = setting;
            else
                m_password = null;

            if (!settings.TryGetValue("AutoAddOutput", out setting))
                AutoAddOutput = false;
            else
                AutoAddOutput = bool.Parse(setting);

            if (AutoAddOutput)
            {
                var measurements = from row in DataSource.Tables["ActiveMeasurements"].AsEnumerable()
                                   where row["PROTOCOL"].ToString() == "PI"
                                   select row;

                List<IMeasurement> outputMeasurements = new List<IMeasurement>();
                foreach (DataRow row in measurements)
                {
                    var measurement = new Measurement();
                    measurement.ID = new Guid(row["SIGNALID"].ToString());
                    measurement.Key = new MeasurementKey(measurement.ID, uint.Parse(row["ID"].ToString().Split(':')[1]), row["ID"].ToString().Split(':')[0]);
                    outputMeasurements.Add(measurement);
                }

                OutputMeasurements = outputMeasurements.ToArray();
            }
        }

        /// <summary>
        /// Connects to the configured PI server.
        /// </summary>
        protected override void AttemptConnection()
        {
            m_server = m_pi.Servers[m_servername];

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
        /// Disconnects from the configured PI server if a connection is open
        /// </summary>
        protected override void AttemptDisconnection()
        {
            if (m_server != null && m_server.Connected)
                m_server.Close();
        }

        /// <summary>
        /// Returns brief status with measurements processed
        /// </summary>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        public override string GetShortStatus(int maxLength)
        {
            return string.Format("Received {0} measurements from PI...", m_processedMeasurements).CenterText(maxLength);
        }

        private void HandleNewMeasurementsRequest(MeasurementKey[] Keys)
        {
            OnStatusMessage("Received request for {0} keys...", new object[] { Keys.Count() });

            if (!IsConnected)
                AttemptConnection();

            var query = from row in DataSource.Tables["ActiveMeasurements"].AsEnumerable()
                        from key in Keys
                        where row["ID"].ToString().Split(':')[1] == key.ID.ToString() && row["PROTOCOL"].ToString() == "PI"
                        select new
                        {
                            Key = key, AlternateTag = row["ALTERNATETAG"].ToString(), PointTag = row["POINTTAG"].ToString()
                        };

            StringBuilder tagFilter = new StringBuilder();
            foreach (var row in query)
            {
                string tagname = row.PointTag;
                if (!String.IsNullOrWhiteSpace(row.AlternateTag))
                    tagname = row.AlternateTag;

                if (!m_tagKeyMap.ContainsKey(tagname))
                {
                    m_tagKeyMap.AddOrUpdate(tagname, row.Key, (k, v) => row.Key);
                }

                if (tagFilter.Length > 0)
                    tagFilter.Append(" OR ");

                tagFilter.Append(string.Format("tag='{0}'", tagname));
            }

            m_points = m_server.GetPoints(tagFilter.ToString());

            if (m_pipe != null)
                ((_DEventPipeEvents_Event)m_pipe).OnNewValue -= (PISDK._DEventPipeEvents_OnNewValueEventHandler)PipeOnOnNewValue;

            m_pipe = m_points.Data.EventPipe;
            ((_DEventPipeEvents_Event)m_pipe).OnNewValue += (PISDK._DEventPipeEvents_OnNewValueEventHandler)PipeOnOnNewValue;
        }

        private void PipeOnOnNewValue()
        {
            List<IMeasurement> measurements = new List<IMeasurement>();
            PIEventObject eventobject;
            PointValue pointvalue;
            for (int i = 0; i < m_pipe.Count; i++)
            {
                eventobject = m_pipe.Take();

                // we will publish measurements for every action except deleted (possible dupes on updates)
                if (eventobject.Action != EventActionConstants.eaDelete)
                {
                    try
                    {
                        pointvalue = (PointValue)eventobject.EventData;

                        double value = Convert.ToDouble(pointvalue.PIValue.Value);
                        MeasurementKey key = m_tagKeyMap[pointvalue.PIPoint.Name];

                        Measurement measurement = new Measurement();
                        measurement.ID = key.SignalID;
                        measurement.Key = key;
                        measurement.Timestamp = pointvalue.PIValue.TimeStamp.LocalDate.ToUniversalTime();
                        measurement.Value = value;
                        measurement.StateFlags = MeasurementStateFlags.Normal;

                        if (measurement.Timestamp > m_lastReceivedTimestamp.Ticks)
                            m_lastReceivedTimestamp = measurement.Timestamp;

                        measurements.Add(measurement);
                    }
                    catch
                    {
                        /* squelch any errors on digital state data that can't be converted to a double */
                    }
                }

                eventobject = null;
            }

            if (measurements.Any())
            {
                OnNewMeasurements(measurements);
                m_processedMeasurements += measurements.Count;
            }
        }

        /// <summary>
        /// Disposes members for garbage collection
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (m_server != null && m_server.Connected)
                m_server.Close();
            m_server = null;

            if (m_tagKeyMap != null)
                m_tagKeyMap.Clear();
            m_tagKeyMap = null;

            m_pi = null;
            m_points = null;
            m_pipe = null;
        }

        #endregion
    }
}
