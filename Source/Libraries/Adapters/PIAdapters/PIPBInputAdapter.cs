//******************************************************************************************************
//  PIPBInputAdapter.cs - Gbtc
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
//  08/13/2012 - Ryan McCoy
//       Generated original version of source code.
//  06/18/2014 - J. Ritchie Carroll
//       Updated code to use PIConnection instance.
//  12/17/2014 - J. Ritchie Carroll
//       Updated to use AF-SDK
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
using OSIsoft.AF.Asset;
using OSIsoft.AF.PI;
using OSIsoft.AF.Time;
using Timer = System.Timers.Timer;

namespace PIAdapters
{
    /// <summary>
    /// Retrieves historical OSI-PI data using AF-SDK.
    /// </summary>
    [Description("OSI-PI: Reads historical measurements from an OSI-PI server using AF-SDK.")]
    public class PIPBInputAdapter : InputAdapterBase
    {
        #region [ Members ]

        // Constants
        private const long DefaultPublicationInterval = 333333;

        // Fields
        private Timer m_readTimer;                                          // Archive data read timer
        private string m_serverName;                                        // Server name where PI connection should be established for connection string
        private string m_userName;                                          // Username for PI connection string
        private string m_password;                                          // Password for PI connection string
        private int m_connectTimeout;                                       // PI connection timeout
        private PIConnection m_connection;                                  // PI server connection
        private IEnumerator<AFValue> m_dataReader;                          // Data reader
        private PIPointList m_points;                                       // List of points this adapter queries from PI
        private readonly Dictionary<int, MeasurementKey> m_tagKeyMap;
        private string m_instanceName;
        private Ticks m_publicationInterval;
        private long m_publicationTime;
        private bool m_simulateTimestamp;
        private AFTime m_startTime;
        private AFTime m_stopTime;
        private bool m_autoRepeat;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="PIAdapters.PIPBInputAdapter"/>.
        /// </summary>
        public PIPBInputAdapter()
        {
            // Setup a read timer
            m_readTimer = new Timer();
            m_readTimer.Elapsed += m_readTimer_Elapsed;
            m_tagKeyMap = new Dictionary<int, MeasurementKey>();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets instance name defined for this <see cref="PIAdapters.PIPBInputAdapter"/>.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the instance name the archive to read. Leave this value blank to default to the adapter name."),
        DefaultValue("")]
        public string InstanceName
        {
            get
            {
                if (string.IsNullOrEmpty(m_instanceName))
                    return Name.ToLower();

                return m_instanceName;
            }
            set
            {
                m_instanceName = value;
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
        /// Gets or sets the publication interval for this <see cref="PIAdapters.PIPBInputAdapter"/>.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the publication time interval in 100-nanosecond tick intervals for reading historical data."),
        DefaultValue(DefaultPublicationInterval)]
        public long PublicationInterval
        {
            get
            {
                return m_publicationInterval;
            }
            set
            {
                m_publicationInterval = value;
            }
        }

        /// <summary>
        /// Gets the start time for reading data.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the start time for reading data into real-time session, or do not define to start reading from the beginning of the available data. Either StartTimeConstraint or StopTimeConstraint must be defined in order to start reading data into real-time session. Value should not be defined when using adapter for subscription based temporal session support."),
        DefaultValue("")]
        public new string StartTimeConstraint
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the stop time for reading data.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the stop time for reading data into real-time session, or do not define to keep reading until the end of the available data. Either StartTimeConstraint or StopTimeConstraint must be defined in order to start reading data into real-time session. Value should not be defined when using adapter for subscription based temporal session support."),
        DefaultValue("")]
        public new string StopTimeConstraint
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value that determines whether timestamps are
        /// simulated for the purposes of real-time concentration.
        /// </summary>
        [ConnectionStringParameter,
        Description("Indicate whether timestamps are simulated for real-time concentration."),
        DefaultValue(false)]
        public bool SimulateTimestamp
        {
            get
            {
                return m_simulateTimestamp;
            }
            set
            {
                m_simulateTimestamp = value;
            }
        }

        /// <summary>
        /// Gets or sets value that determines if the input data should be replayed repeatedly.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define if the input data should be replayed repeatedly."),
        DefaultValue(false)]
        public bool AutoRepeat
        {
            get
            {
                return m_autoRepeat;
            }
            set
            {
                m_autoRepeat = value;
            }
        }

        /// <summary>
        /// Gets the flag indicating if this adapter supports temporal processing.
        /// </summary>
        public override bool SupportsTemporalProcessing
        {
            get
            {
                // If the start/time constraints are defined in the connection string, it is expected that this adapter
                // will be used in real-time. For temporal sessions these properties will be defined via method call to
                // the SetTemporalConstraint function.
                Dictionary<string, string> settings = Settings;

                if ((object)settings != null && settings.Count > 0)
                    return !(settings.ContainsKey("startTimeConstraint") || settings.ContainsKey("stopTimeConstraint"));

                return false;
            }
        }

        /// <summary>
        /// Gets or sets the desired processing interval, in milliseconds, for the adapter.
        /// </summary>
        /// <remarks>
        /// With the exception of the values of -1 and 0, this value specifies the desired processing interval for data, i.e.,
        /// basically a delay, or timer interval, over which to process data. A value of -1 means to use the default processing
        /// interval while a value of 0 means to process data as fast as possible.
        /// </remarks>
        public override int ProcessingInterval
        {
            get
            {
                return base.ProcessingInterval;
            }
            set
            {
                base.ProcessingInterval = value;

                // Set read timer interval to the requested processing interval
                m_readTimer.Interval = value <= 0 ? 1 : value;
            }
        }

        /// <summary>
        /// Gets flag that determines if this <see cref="PIAdapters.PIPBInputAdapter"/> uses an asynchronous connection.
        /// </summary>
        protected override bool UseAsyncConnect
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Returns the detailed status of the data input source.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();
                status.Append(base.Status);

                status.AppendFormat("        OSI-PI server name: {0}\r\n", m_serverName);
                status.AppendFormat("       Connected to server: {0}\r\n", (object)m_connection == null ? "No" : m_connection.Connected ? "Yes" : "No");
                status.AppendFormat("             Instance name: {0}\r\n", m_instanceName);
                status.AppendFormat("      Publication interval: {0:#,##0}\r\n", m_publicationInterval);
                status.AppendFormat("               Auto-repeat: {0}\r\n", m_autoRepeat);
                status.AppendFormat("            Start time-tag: {0}\r\n", m_startTime);
                status.AppendFormat("             Stop time-tag: {0}\r\n", m_stopTime);

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="PIAdapters.PIPBInputAdapter"/> object and optionally releases the managed resources.
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
                        if ((object)m_readTimer != null)
                        {
                            m_readTimer.Elapsed -= m_readTimer_Elapsed;
                            m_readTimer.Dispose();
                            m_readTimer = null;
                        }

                        if ((object)m_connection != null)
                        {
                            m_connection.Dispose();
                            m_connection = null;
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
        /// Initializes this <see cref="PIAdapters.PIPBInputAdapter"/>.
        /// </summary>
        /// <exception cref="ArgumentException"><b>HistorianID</b>, <b>Server</b>, <b>Port</b>, <b>Protocol</b>, or <b>InitiateConnection</b> is missing from the <see cref="AdapterBase.Settings"/>.</exception>
        public override void Initialize()
        {
            const string errorMessage = "{0} is missing from settings - Example: instanceName=PPA; serverName=myServerName; publicationInterval=333333";

            base.Initialize();

            Dictionary<string, string> settings = Settings;
            string setting;

            // Validate settings.
            settings.TryGetValue("instanceName", out m_instanceName);

            if (((object)OutputSourceIDs == null || OutputSourceIDs.Length == 0) && string.IsNullOrEmpty(m_instanceName))
                throw new ArgumentException(string.Format(errorMessage, "instanceName"));

            if (!settings.TryGetValue("ServerName", out m_serverName))
                throw new InvalidOperationException("Server name is a required setting for PI connections. Please add a server in the format serverName=myServerName to the connection string.");

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

            if (!(settings.TryGetValue("publicationInterval", out setting) && Ticks.TryParse(setting, out m_publicationInterval)))
                m_publicationInterval = DefaultPublicationInterval;

            if (settings.TryGetValue("simulateTimestamp", out setting))
                m_simulateTimestamp = setting.ParseBoolean();

            if (settings.TryGetValue("autoRepeat", out setting))
                m_autoRepeat = setting.ParseBoolean();

            // Define output measurements this input adapter can support based on the instance name (if not already defined)
            if (string.IsNullOrEmpty(m_instanceName))
            {
                if ((object)OutputSourceIDs != null && OutputSourceIDs.Length > 0)
                    m_instanceName = OutputSourceIDs[0];
            }
            else
            {
                OutputSourceIDs = new[] { m_instanceName };
            }
        }

        /// <summary>
        /// Gets a short one-line status of this <see cref="PIAdapters.PIPBInputAdapter"/>.
        /// </summary>
        /// <param name="maxLength">Maximum length of the status message.</param>
        /// <returns>Text of the status message.</returns>
        public override string GetShortStatus(int maxLength)
        {
            if (Enabled && m_publicationTime > 0)
                return string.Format("Publishing data for {0}...", (new DateTime(m_publicationTime)).ToString("yyyy-MM-dd HH:mm:ss.fff")).CenterText(maxLength);

            return "Not currently publishing data".CenterText(maxLength);
        }

        /// <summary>
        /// Attempts to connect to this <see cref="PIAdapters.PIPBInputAdapter"/>.
        /// </summary>
        protected override void AttemptConnection()
        {
            // This adapter is only engaged for history, so we don't process any data unless a temporal constraint is defined
            if (this.TemporalConstraintIsDefined())
            {
                // Turn off read timer if it's active
                m_readTimer.Enabled = false;

                m_connection = new PIConnection
                {
                    ServerName = m_serverName,
                    UserName = m_userName,
                    Password = m_password,
                    ConnectTimeout = m_connectTimeout
                };

                m_connection.Open();

                // Start the data reader on its own thread so connection attempt can complete in a timely fashion...
                ThreadPool.QueueUserWorkItem(StartDataReader);
            }
        }

        /// <summary>
        /// Attempts to disconnect from this <see cref="PIAdapters.PIPBInputAdapter"/>.
        /// </summary>
        protected override void AttemptDisconnection()
        {
            if ((object)m_readTimer != null)
            {
                m_readTimer.Enabled = false;

                lock (m_readTimer)
                {
                    m_dataReader = null;
                }
            }

            if ((object)m_connection != null)
            {
                m_connection.Dispose();
                m_connection = null;
            }
        }

        private IEnumerable<AFValue> ReadData(AFTime startTime, AFTime endTime)
        {
            return new TimeSortedValueScanner
            {
                Points = m_points,
                StartTime = startTime,
                EndTime = endTime,
                DataReadExceptionHandler = OnProcessException
            }
            .Read();
        }

        // Kick start read process for historian
        private void StartDataReader(object state)
        {
            try
            {
                if (SupportsTemporalProcessing)
                {
                    if ((object)RequestedOutputMeasurementKeys != null)
                        OnStatusMessage("Replaying for requested output keys: {0:N0} defined measurements", RequestedOutputMeasurementKeys.Length);
                    else
                        OnStatusMessage("No measurements have been requested for playback - make sure \"; connectOnDemand=true\" is defined in the connection string for the reader.");
                }

                MeasurementKey[] requestedKeys = SupportsTemporalProcessing ? RequestedOutputMeasurementKeys : OutputMeasurements.MeasurementKeys().ToArray();

                if (Enabled && (object)m_connection != null && (object)requestedKeys != null && requestedKeys.Length > 0)
                {
                    HashSet<string> tagList = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    var query = from row in DataSource.Tables["ActiveMeasurements"].AsEnumerable()
                                from key in requestedKeys
                                where row["ID"].ToString() == key.ToString()
                                select new
                                {
                                    Key = key,
                                    AlternateTag = row["AlternateTag"].ToString(),
                                    PointTag = row["PointTag"].ToString()
                                };

                    string tagName;
                    m_points = new PIPointList();
                    PIPoint point;

                    foreach (var result in query)
                    {
                        tagName = result.PointTag;

                        if (!string.IsNullOrWhiteSpace(result.AlternateTag))
                            tagName = result.AlternateTag;

                        if (tagList.Add(tagName) && PIPoint.TryFindPIPoint(m_connection.Server, tagName, out point))
                        {
                            m_tagKeyMap[point.ID] = result.Key;
                            m_points.Add(point);
                        }
                    }

                    m_publicationTime = 0;

                    // Start data read from historian
                    lock (m_readTimer)
                    {
                        m_startTime = base.StartTimeConstraint < DateTime.MinValue ? DateTime.MinValue : base.StartTimeConstraint > DateTime.MaxValue ? DateTime.MaxValue : base.StartTimeConstraint;
                        m_stopTime = base.StopTimeConstraint < DateTime.MinValue ? DateTime.MinValue : base.StopTimeConstraint > DateTime.MaxValue ? DateTime.MaxValue : base.StopTimeConstraint;

                        m_dataReader = ReadData(m_startTime, m_stopTime).GetEnumerator();

                        m_readTimer.Enabled = m_dataReader.MoveNext();

                        if (m_readTimer.Enabled)
                        {
                            OnStatusMessage("Starting historical data read...");
                        }
                        else
                        {
                            OnStatusMessage("No historical data was available to read for given time frame.");
                            OnProcessingComplete();
                        }
                    }
                }
                else
                {
                    m_readTimer.Enabled = false;
                    OnStatusMessage("No measurement keys have been requested for reading, historian reader is idle.");
                    OnProcessingComplete();
                }
            }
            catch (Exception ex)
            {
                OnProcessException(new InvalidOperationException(string.Format("Could not start historical data read due to exception: {0}", ex.Message), ex));
            }
        }

        // Process next data read
        private void m_readTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            List<IMeasurement> measurements = new List<IMeasurement>();

            if (Monitor.TryEnter(m_readTimer))
            {
                try
                {
                    AFValue currentPoint = m_dataReader.Current;
                    long timestamp = currentPoint.Timestamp.UtcTime.Ticks;

                    if (m_publicationTime == 0)
                        m_publicationTime = timestamp;

                    // Set next reasonable publication time
                    while (m_publicationTime < timestamp)
                        m_publicationTime += m_publicationInterval;

                    do
                    {
                        try
                        {
                            // Add current measurement to the collection for publication
                            measurements.Add(new Measurement
                            {
                                Metadata = m_tagKeyMap[currentPoint.PIPoint.ID].Metadata,
                                Timestamp = m_simulateTimestamp ? DateTime.UtcNow.Ticks : timestamp,
                                Value = Convert.ToDouble(currentPoint.Value),
                                StateFlags = ConvertStatusFlags(currentPoint.Status)
                            });
                        }
                        catch (Exception ex)
                        {
                            OnProcessException(new InvalidOperationException("Failed to map AFValue to Measurement: " + ex.Message, ex));
                        }

                        // Attempt to move to next record
                        if (timestamp < m_stopTime.UtcTime.Ticks && m_dataReader.MoveNext())
                        {
                            // Read record value
                            currentPoint = m_dataReader.Current;
                            timestamp = currentPoint.Timestamp.UtcTime.Ticks;
                        }
                        else
                        {
                            if (timestamp < m_stopTime.UtcTime.Ticks && m_startTime.UtcTime.Ticks < timestamp)
                            {
                                // Could be attempting read with a future end time - in these cases attempt to re-read current data
                                // from now to end time in case any new data as been archived in the mean-time
                                m_startTime = new AFTime(timestamp + Ticks.PerMillisecond);
                                m_dataReader = ReadData(m_startTime, m_stopTime).GetEnumerator();

                                if (!m_dataReader.MoveNext())
                                {
                                    // Finished reading all available data
                                    m_readTimer.Enabled = false;

                                    if (m_autoRepeat)
                                        ThreadPool.QueueUserWorkItem(StartDataReader);
                                    else
                                        OnProcessingComplete();
                                }
                            }
                            else
                            {
                                // Finished reading all available data
                                m_readTimer.Enabled = false;

                                if (m_autoRepeat)
                                    ThreadPool.QueueUserWorkItem(StartDataReader);
                                else
                                    OnProcessingComplete();
                            }

                            break;
                        }
                    }
                    while (timestamp <= m_publicationTime);
                }
                catch (InvalidOperationException)
                {
                    // Pooled timer thread executed after last read, verify timer has stopped
                    m_readTimer.Enabled = false;
                }
                finally
                {
                    Monitor.Exit(m_readTimer);
                }
            }

            // Publish all measurements for this time interval
            if (measurements.Count > 0)
                OnNewMeasurements(measurements);
        }

        private MeasurementStateFlags ConvertStatusFlags(AFValueStatus status)
        {
            MeasurementStateFlags flags = MeasurementStateFlags.Normal;

            if ((status & AFValueStatus.Bad) > 0)
                flags |= MeasurementStateFlags.BadData;

            if ((status & AFValueStatus.Questionable) > 0)
                flags |= MeasurementStateFlags.SuspectData;

            if ((status & AFValueStatus.BadSubstituteValue) > 0)
                flags |= MeasurementStateFlags.CalculationError | MeasurementStateFlags.BadData;

            if ((status & AFValueStatus.UncertainSubstituteValue) > 0)
                flags |= MeasurementStateFlags.CalculationError | MeasurementStateFlags.SuspectData;

            if ((status & AFValueStatus.Substituted) > 0)
                flags |= MeasurementStateFlags.CalculatedValue;

            return flags;
        }

        #endregion
    }
}