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
//  06/18/2014 - J. Ritchie Carroll
//       Updated code to use PIConnection instance.
//  12/17/2014 - J. Ritchie Carroll
//       Updated to use AF-SDK
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
using System.Timers;
using GSF;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using OSIsoft.AF.Asset;
using OSIsoft.AF.Data;
using OSIsoft.AF.PI;
using OSIsoft.AF.Time;

namespace PIAdapters
{
    /// <summary>
    /// Uses PI event pipes to deliver real-time PI data to GSF host
    /// </summary>
    [Description("OSI-PI: Reads real-time measurements from an OSI-PI server using AF-SDK.")]
    public class PIRTInputAdapter : InputAdapterBase
    {
        #region [ Members ]

        // Fields
        private ConcurrentDictionary<string, MeasurementKey> m_tagKeyMap;     // Map PI tag names to GSFSchema measurement keys
        private PIConnection m_connection;                                    // PI server connection
        private string m_serverName;                                          // Server name for PI connection string
        private string m_userName;                                            // Username for PI connection string
        private string m_password;                                            // Password for PI connection string
        private int m_connectTimeout;                                         // PI connection timeout
        private PIPointList m_points;                                         // PI point list of points to which subscription should be made
        private AFDataPipe m_pipe;                                            // event pipe object raises an event when a subscribed point is updated
        private int m_processedMeasurements;                                  // processed measurements for short status
        private bool m_autoAddOutput;                                         // whether or not to automatically add PI points
        private DateTime m_lastReceivedTimestamp;                             // last received timestamp from PI event pipe
        private bool m_useEventPipes = true;                                  // whether or not to use event pipes for real-time data
        private List<IMeasurement> m_measurements;                            // Queried measurements that are prepared to be published  
        private int m_queryTimeSpan = 30;                                     // Minutes of data to pull per query
        private DateTime m_publishTime = DateTime.MinValue;                   // The timestamp that is currently being published
        private DateTime m_queryTime = DateTime.MinValue;                     // The timestamp that is currently being queried from PI
        Thread m_dataThread;                                                  // Thread to run queries
        System.Timers.Timer m_publishTimer;                                   // last received timestamp from PI

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Receives real-time updates from PI
        /// </summary>
        public PIRTInputAdapter()
        {
            m_tagKeyMap = new ConcurrentDictionary<string, MeasurementKey>();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets a value for whether the adapter will use event pipes. If event pipes are disabled, the adapter will use polling.
        /// </summary>
        [ConnectionStringParameter, Description("Gets or sets a value for whether the adapter will use event pipes. If event pipes are disabled, the adapter will use polling."), DefaultValue(true)]
        public bool UseEventPipes
        {
            get
            {
                return m_useEventPipes;
            }
            set
            {
                m_useEventPipes = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of seconds to query when the adapter is polling instead of using event pipes.
        /// </summary>
        [ConnectionStringParameter, Description("Gets or sets the number of seconds to query when the adapter is polling instead of using event pipes."), DefaultValue(5)]
        public int QueryTimeSpan
        {
            get
            {
                return m_queryTimeSpan;
            }
            set
            {
                m_queryTimeSpan = value;
            }
        }

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
                if ((object)value != null && value.Any())
                    HandleNewMeasurementsRequest(value);
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

        /// <summary>
        /// Returns the status of the adapter
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();
                status.AppendFormat("   Last Received Timestamp: {0}\r\n", m_lastReceivedTimestamp.ToString("MM/dd/yyyy HH:mm:ss.fff"));
                status.AppendFormat("              Last Latency: {0}s\r\n", (DateTime.UtcNow - m_lastReceivedTimestamp).TotalSeconds);
                status.AppendFormat("         Using Event Pipes: {0}\r\n", m_useEventPipes);
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

            m_measurements = new List<IMeasurement>();

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

            if (!settings.TryGetValue("AutoAddOutput", out setting))
                AutoAddOutput = false;
            else
                AutoAddOutput = bool.Parse(setting);

            if (settings.TryGetValue("UseEventPipes", out setting))
                UseEventPipes = bool.Parse(setting);
            else
                UseEventPipes = true;

            if (settings.TryGetValue("QueryTimeSpan", out setting))
                QueryTimeSpan = Convert.ToInt32(setting);
            else
                QueryTimeSpan = 5;


            if (AutoAddOutput)
            {
                var measurements = from row in DataSource.Tables["ActiveMeasurements"].AsEnumerable()
                                   where row["PROTOCOL"].ToString() == "PI"
                                   select row;

                List<IMeasurement> outputMeasurements = new List<IMeasurement>();
                foreach (DataRow row in measurements)
                {
                    var measurement = new Measurement();
                    var signalID = new Guid(row["SIGNALID"].ToString());
                    measurement.Key = MeasurementKey.LookUpOrCreate(signalID, row["ID"].ToString());
                    outputMeasurements.Add(measurement);
                }

                OutputMeasurements = outputMeasurements.ToArray();
                OnOutputMeasurementsUpdated();
            }
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
        /// Disconnects from the configured PI server if a connection is open
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
                        where row["ID"].ToString().Split(':')[1] == key.ID.ToString()
                        select new
                        {
                            Key = key,
                            AlternateTag = row["ALTERNATETAG"].ToString(),
                            PointTag = row["POINTTAG"].ToString()
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

            m_points = new PIPointList(PIPoint.FindPIPoints(m_connection.Server, tagFilter.ToString(), true));

            // TODO: Re-enable event pipe functionality in AF-SDK
            //bool useEventPipes;

            //// event pipes are only applicable if enabled in connection string and this is a real time session, not playback
            //useEventPipes = m_useEventPipes && StartTimeConstraint == DateTime.MinValue && StopTimeConstraint == DateTime.MaxValue;

            //if (useEventPipes)
            //{
            //    try
            //    {
            //        if (m_pipe != null)
            //            ((_DEventPipeEvents_Event)m_pipe).OnNewValue -= (_DEventPipeEvents_OnNewValueEventHandler)PipeOnOnNewValue;

            //        m_connection.Execute(server => m_pipe = m_points.Data.EventPipe);

            //        ((_DEventPipeEvents_Event)m_pipe).OnNewValue += (_DEventPipeEvents_OnNewValueEventHandler)PipeOnOnNewValue;
            //    }
            //    catch (ThreadAbortException)
            //    {
            //        throw;
            //    }
            //    catch (Exception e)
            //    {
            //        useEventPipes = false; // try to run with polling instead of event pipes;
            //        OnProcessException(e);
            //    }
            //}

            //if (!useEventPipes)
            //{
            // warn that we are going to use a different configuration here...
            if (m_useEventPipes)
                OnStatusMessage("WARNING: PI adapter switching from event pipes to polling due to error or start/stop time constraints.");

            // set up a new thread to do some long calls to PI and set up threads, timers, etc for polling
            StopGettingData();
            ThreadPool.QueueUserWorkItem(StartGettingData, tagFilter);
            //}

            //m_useEventPipes = useEventPipes;
        }

        /// <summary>
        /// Starts threads and timers to poll the PI server for data
        /// </summary>
        /// <param name="state">Filter string which will get the desired points from PI</param>
        private void StartGettingData(object state)
        {
            try
            {
                string tagFilter = state.ToString();

                m_points = new PIPointList(PIPoint.FindPIPoints(m_connection.Server, tagFilter, true));

                m_dataThread = new Thread(QueryData);
                m_dataThread.IsBackground = true;
                m_dataThread.Start();

                m_publishTimer = new System.Timers.Timer();
                m_publishTimer.Interval = ProcessingInterval > 0 ? ProcessingInterval : 33;
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

        /// <summary>
        /// Runs a constant loop to pull data from PI
        /// </summary>
        private void QueryData()
        {
            DateTime currentTime = StartTimeConstraint;
            if (currentTime == DateTime.MinValue) // handle real-time IAON session
                currentTime = DateTime.UtcNow;

            while (currentTime <= StopTimeConstraint)
            {
                try
                {
                    DateTime endTime = currentTime.AddSeconds(m_queryTimeSpan);

                    if (endTime <= DateTime.UtcNow)
                    {
                        DateTime localCurrentTime = currentTime.ToLocalTime();
                        DateTime localEndTime = endTime.ToLocalTime();
                        List<IMeasurement> measToAdd = new List<IMeasurement>();

                        foreach (PIPoint point in m_points)
                        {
                            AFValues values = point.RecordedValues(new AFTimeRange(new AFTime(localCurrentTime), new AFTime(localEndTime)), AFBoundaryType.Inside, null, false);

                            foreach (AFValue value in values)
                            {
                                Measurement measurement = new Measurement();
                                measurement.Key = m_tagKeyMap[point.Name];
                                measurement.Value = Convert.ToDouble(value.Value);
                                measurement.Timestamp = value.Timestamp.UtcTime;
                                measToAdd.Add(measurement);
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

        // Publishes data that was queried from PI using polling
        private void m_publishTimer_Tick(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (m_queryTime > m_publishTime)
                {
                    DateTime minPublishTime = StartTimeConstraint;
                    if (minPublishTime == DateTime.MinValue)
                        minPublishTime = DateTime.UtcNow;

                    if (m_publishTime < minPublishTime)
                        m_publishTime = minPublishTime;

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

                    OnNewMeasurements(publishMeasurements);
                }
            }
            catch (Exception ex)
            {
                OnProcessException(ex);
            }
        }

        // TODO: Updated code for handling piped-events in AF-SDK
        //private void PipeOnOnNewValue()
        //{
        //    List<IMeasurement> measurements = new List<IMeasurement>();

        //    m_connection.Execute(server =>
        //    {
        //        PIEventObject eventobject;
        //        PointValue pointvalue;

        //        for (int i = 0; i < m_pipe.Count; i++)
        //        {
        //            eventobject = m_pipe.Take();

        //            // we will publish measurements for every action except deleted (possible dupes on updates)
        //            if (eventobject.Action != EventActionConstants.eaDelete)
        //            {
        //                try
        //                {
        //                    pointvalue = (PointValue)eventobject.EventData;

        //                    double value = Convert.ToDouble(pointvalue.PIValue.Value);
        //                    MeasurementKey key = m_tagKeyMap[pointvalue.PIPoint.Name];

        //                    Measurement measurement = new Measurement();
        //                    measurement.Key = key;
        //                    measurement.Timestamp = pointvalue.PIValue.TimeStamp.LocalDate.ToUniversalTime();
        //                    measurement.Value = value;
        //                    measurement.StateFlags = MeasurementStateFlags.Normal;

        //                    if (measurement.Timestamp > m_lastReceivedTimestamp.Ticks)
        //                        m_lastReceivedTimestamp = measurement.Timestamp;

        //                    measurements.Add(measurement);
        //                }
        //                catch
        //                {
        //                    /* squelch any errors on digital state data that can't be converted to a double */
        //                }
        //            }
        //        }
        //    });

        //    if (measurements.Any())
        //    {
        //        OnNewMeasurements(measurements);
        //        m_processedMeasurements += measurements.Count;
        //    }
        //}

        /// <summary>
        /// Disposes members for garbage collection
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (m_tagKeyMap != null)
                m_tagKeyMap.Clear();
            m_tagKeyMap = null;

            if ((object)m_connection != null)
                m_connection.Dispose();

            m_connection = null;
            m_points = null;
            m_pipe = null;

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

        #endregion
    }
}
