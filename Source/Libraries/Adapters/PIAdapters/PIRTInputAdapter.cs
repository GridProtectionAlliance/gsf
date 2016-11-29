//******************************************************************************************************
//  PIRTInputAdapter.cs - Gbtc
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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using GSF;
using GSF.Collections;
using GSF.Diagnostics;
using GSF.Threading;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using OSIsoft.AF;
using OSIsoft.AF.Asset;
using OSIsoft.AF.Data;
using OSIsoft.AF.PI;

namespace PIAdapters
{
    /// <summary>
    /// Uses PI event pipes to deliver real-time PI data to GSF host
    /// </summary>
    [Description("OSI-PI: Reads real-time measurements from an OSI-PI server using AF-SDK.")]
    public class PIRTInputAdapter : InputAdapterBase
    {
        #region [ Members ]

        // Nested Types
        //private class DataUpdateObserver : IObserver<AFDataPipeEvent>
        //{
        //    public EventHandler<EventArgs<AFValue>> DataUpdated;
        //    public EventHandler Completed;
        //    private Action<Exception> m_exceptionHandler;

        //    public DataUpdateObserver(Action<Exception> exceptionHandler)
        //    {
        //        m_exceptionHandler = exceptionHandler;
        //    }

        //    public void OnCompleted() => Completed?.Invoke(this, EventArgs.Empty);

        //    public void OnError(Exception error) => m_exceptionHandler?.Invoke(error);

        //    public void OnNext(AFDataPipeEvent value)
        //    {
        //        if (value.Action != AFDataPipeAction.Delete)
        //            DataUpdated?.Invoke(this, new EventArgs<AFValue>(value.Value));
        //    }
        //}

        // Fields
        private readonly ConcurrentDictionary<int, MeasurementKey> m_tagKeyMap; // Map PI tag ID to GSFSchema measurement keys
        private readonly ShortSynchronizedOperation m_restartConnection;        // Restart connection operation
        private readonly ShortSynchronizedOperation m_readEvents;               // Reads PI events
        private readonly System.Timers.Timer m_eventTimer;                      // Timer to trigger event reader
        private PIConnection m_connection;                                      // PI server connection
        private PIDataPipe m_dataPipe;                                          // PI data pipe
        private List<PIPoint> m_dataPoints;                                     // Last list of subscribed data points
        private string m_serverName;                                            // Server name for PI connection string
        private string m_userName;                                              // Username for PI connection string
        private string m_password;                                              // Password for PI connection string
        //private readonly DataUpdateObserver m_dataUpdateObserver;               // Custom observer class for handling point updates
        private int m_connectTimeout;                                           // PI connection timeout
        private Ticks m_lastReceivedTimestamp;                                  // Last received timestamp from PI event pipe
        private double m_lastReceivedValue;
        private List<IMeasurement> m_measurements;                              // Queried measurements that are prepared to be published
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Receives real-time updates from PI
        /// </summary>
        public PIRTInputAdapter()
        {
            m_tagKeyMap = new ConcurrentDictionary<int, MeasurementKey>();
            m_restartConnection = new ShortSynchronizedOperation(Start);
            m_readEvents = new ShortSynchronizedOperation(ReadEvents);
            //m_dataUpdateObserver = new DataUpdateObserver(OnProcessException);
            //m_dataUpdateObserver.DataUpdated += m_dataUpdateObserver_DataUpdated;
            m_eventTimer = new System.Timers.Timer(1000.0D);
            m_eventTimer.Elapsed += m_eventTimer_Elapsed;
            m_eventTimer.AutoReset = true;
            m_eventTimer.Enabled = false;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Returns false to indicate that this <see cref="PIRTInputAdapter"/> does NOT connect asynchronously
        /// </summary>
        protected override bool UseAsyncConnect => false;

        /// <summary>
        /// Returns false to indicate that this <see cref="PIRTInputAdapter"/> does NOT support temporal processing. 
        /// Temporal processing is supported in a separate adapter that is not driven by event pipes.
        /// </summary>
        public override bool SupportsTemporalProcessing => false;

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
                    SubscribeToPointUpdates(value);
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
        [ConnectionStringParameter, Description("Defines the name of the PI user ID for the adapter's PI connection."), DefaultValue("")]
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
        [ConnectionStringParameter, Description("Defines the password used for the adapter's PI connection."), DefaultValue("")]
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
        /// Last timestamp received from PI
        /// </summary>
        public DateTime LastReceivedTimestamp => m_lastReceivedTimestamp;

        /// <summary>
        /// Returns the status of the adapter
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.Append(base.Status);

                status.AppendFormat("   Last Received Timestamp: {0}", m_lastReceivedTimestamp.ToString("MM/dd/yyyy HH:mm:ss.fff"));
                status.AppendLine();
                status.AppendFormat("       Last Received Value: {0:N3}", m_lastReceivedValue);
                status.AppendLine();
                status.AppendFormat("  Latency of Last Received: {0}", (DateTime.UtcNow.Ticks - m_lastReceivedTimestamp).ToElapsedTimeString(3));
                status.AppendLine();

                if ((object)m_dataPoints != null)
                {
                    status.AppendFormat("       PI Data Point Count: {0:N0}: {1}...", m_dataPoints.Count, m_dataPoints.Select(p => p.ID.ToString()).Take(5).ToDelimitedString(", "));
                    status.AppendLine();
                }

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="PIRTInputAdapter"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed")]
        protected override void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                    {
                        if ((object)m_connection != null)
                        {
                            m_connection.Disconnected -= m_connection_Disconnected;
                            m_connection.Dispose();
                            m_connection = null;
                        }

                        m_dataPipe?.Dispose();

                        if ((object)m_eventTimer != null)
                        {
                            m_eventTimer.Elapsed -= m_eventTimer_Elapsed;
                            m_eventTimer.Dispose();
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
        }

        /// <summary>
        /// Connects to the configured PI server.
        /// </summary>
        protected override void AttemptConnection()
        {
            m_connection = new PIConnection
            {
                ServerName = this.ServerName,
                UserName = this.UserName,
                Password = this.Password,
                ConnectTimeout = this.ConnectTimeout
            };

            m_connection.Disconnected += m_connection_Disconnected;
            m_connection.Open();

            m_dataPipe = new PIDataPipe(AFDataPipeType.Snapshot);
            //m_dataPipe.Subscribe(m_dataUpdateObserver);

            if (AutoStart && (object)OutputMeasurements != null && OutputMeasurements.Any())
                SubscribeToPointUpdates(this.OutputMeasurementKeys());
        }

        /// <summary>
        /// Disconnects from the configured PI server if a connection is open
        /// </summary>
        protected override void AttemptDisconnection()
        {
            m_eventTimer.Enabled = false;

            if ((object)m_dataPoints != null)
            {
                m_dataPoints.Clear();
                m_dataPoints = null;
            }

            if ((object)m_dataPipe != null)
            {
                m_dataPipe.Dispose();
                m_dataPipe = null;
            }

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
        public override string GetShortStatus(int maxLength) => $"Received {ProcessedMeasurements} measurements from PI...".CenterText(maxLength);

        private void SubscribeToPointUpdates(MeasurementKey[] keys)
        {
            OnStatusMessage(MessageLevel.Info, "Subscribing to updates for {0} measurements...", keys.Length);

            var query = from row in DataSource.Tables["ActiveMeasurements"].AsEnumerable()
                        from key in keys
                        where row["ID"].ToString() == key.ToString()
                        select new
                        {
                            Key = key,
                            AlternateTag = row["AlternateTag"].ToString(),
                            PointTag = row["PointTag"].ToString()
                        };

            List<PIPoint> dataPoints = new List<PIPoint>();

            foreach (var row in query)
            {
                string tagName = row.PointTag;

                if (!string.IsNullOrWhiteSpace(row.AlternateTag))
                    tagName = row.AlternateTag;

                OnStatusMessage(MessageLevel.Debug, "DEBUG: Looking up point tag '{0}'...", tagName);

                PIPoint point = GetPIPoint(m_connection.Server, tagName);

                if ((object)point != null)
                {
                    OnStatusMessage(MessageLevel.Debug, "DEBUG: Found point tag '{0}'...", tagName);
                    dataPoints.Add(point);
                    m_tagKeyMap[point.ID] = row.Key;
                }
                else
                {
                    OnStatusMessage(MessageLevel.Debug, "DEBUG: Failed to find point tag '{0}'...", tagName);
                }
            }

            // Remove sign-ups for any existing point list
            if ((object)m_dataPoints != null)
                m_dataPipe.RemoveSignups(m_dataPoints);

            // Sign up for updates on selected points
            AFListResults<PIPoint, AFDataPipeEvent> initialEvents = m_dataPipe.AddSignupsWithInitEvents(dataPoints);

            OnStatusMessage(MessageLevel.Debug, $"DEBUG: Initial event count = {initialEvents.Results.Count}...");

            foreach (AFDataPipeEvent item in initialEvents.Results)
            {
                OnStatusMessage(MessageLevel.Debug, "DEBUG: Found initial event for action...");

                if (item.Action != AFDataPipeAction.Delete)
                    m_dataUpdateObserver_DataUpdated(this, new EventArgs<AFValue>(item.Value));
            }

            m_dataPoints = dataPoints;

            m_eventTimer.Enabled = true;
        }

        private PIPoint GetPIPoint(PIServer server, string tagName)
        {
            PIPoint point;
            PIPoint.TryFindPIPoint(server, tagName, out point);
            return point;
        }

        private void m_eventTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            OnStatusMessage(MessageLevel.Debug, "DEBUG: Timer elapsed...");
            m_readEvents.TryRunOnceAsync();
        }

        private void ReadEvents()
        {
            if ((object)m_dataPipe == null)
                return;

            OnStatusMessage(MessageLevel.Debug, "DEBUG: Data pipe called for next 100 GetUpdateEvents...");

            AFListResults<PIPoint, AFDataPipeEvent> updateEvents = m_dataPipe.GetUpdateEvents(100);

            OnStatusMessage(MessageLevel.Debug, $"DEBUG: Update event count = {updateEvents.Count}...");

            foreach (AFDataPipeEvent item in updateEvents.Results)
            {
                OnStatusMessage(MessageLevel.Debug, "DEBUG: Found update event for action...");

                if (item.Action != AFDataPipeAction.Delete)
                    m_dataUpdateObserver_DataUpdated(this, new EventArgs<AFValue>(item.Value));
            }
        }

        // PI data updated handler
        private void m_dataUpdateObserver_DataUpdated(object sender, EventArgs<AFValue> e)
        {
            OnStatusMessage(MessageLevel.Debug, $"DEBUG: Data observer event handler called with a new value: {Convert.ToDouble(e.Argument.Value):N3}...");
            AFValue value = e.Argument;
            MeasurementKey key;

            OnStatusMessage(MessageLevel.Debug, $"DEBUG: Data observer event handler looking up point ID {value.PIPoint.ID:N0} in table...");

            if ((object)value != null && m_tagKeyMap.TryGetValue(value.PIPoint.ID, out key))
            {
                OnStatusMessage(MessageLevel.Debug, $"DEBUG: Data observer event handler found point ID {value.PIPoint.ID:N0} in table: {key}...");
                Measurement measurement = new Measurement();

                measurement.Metadata = key.Metadata;
                measurement.Value = Convert.ToDouble(value.Value);
                measurement.Timestamp = value.Timestamp.UtcTime;

                OnNewMeasurements(new[] { measurement });

                m_lastReceivedTimestamp = measurement.Timestamp;
                m_lastReceivedValue = measurement.Value;
            }
            else
            {
                OnStatusMessage(MessageLevel.Debug, $"DEBUG: Data observer event handler did not find point ID {value.PIPoint.ID:N0} in table...");
            }
        }

        private void m_connection_Disconnected(object sender, EventArgs e)
        {
            // Since we may get a plethora of these requests, we use a synchronized operation to restart once
            m_restartConnection.RunOnceAsync();
        }

        #endregion
    }
}
