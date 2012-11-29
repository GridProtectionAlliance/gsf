//******************************************************************************************************
//  DataSubscriber.cs - Gbtc
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
//  08/20/2010 - J. Ritchie Carroll
//       Generated original version of source code.
//  02/07/2012 - Mehulbhai Thakkar
//       Modified SynchronizeMetadata to filter devices by original source and modified insert query
//       to populate OriginalSource value. Added to flag to optionally avoid metadata synchronization.
//
//******************************************************************************************************

using GSF.Collections;
using GSF.Communication;
//using GSF.Data;
using GSF.IO;
using GSF.Reflection;
//using GSF.Security.Cryptography;
using GSF.TimeSeries.Adapters;
//using GSF.TimeSeries.Statistics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;

namespace GSF.TimeSeries.Transport
{
    /// <summary>
    /// Represents a data subscribing client that will connect to a data publisher for a data subscription.
    /// </summary>
    [Description("DataSubscriber: client that subscribes to a publishing server for a streaming data.")]
    [EditorBrowsable(EditorBrowsableState.Advanced)] // Normally defined as an input device protocol
    public class DataSubscriber : InputAdapterBase
    {
        #region [ Members ]

        // Nested Types

        // Local measurement concentrator
        private class LocalConcentrator : ConcentratorBase
        {
            #region [ Members ]

            // Fields
            private DataSubscriber m_parent;
            private bool m_disposed;

            #endregion

            #region [ Constructors ]

            /// <summary>
            /// Creates a new local concentrator.
            /// </summary>
            public LocalConcentrator(DataSubscriber parent)
            {
                m_parent = parent;
            }

            #endregion

            #region [ Methods ]

            /// <summary>
            /// Releases the unmanaged resources used by the <see cref="LocalConcentrator"/> object and optionally releases the managed resources.
            /// </summary>
            /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
            protected override void Dispose(bool disposing)
            {
                if (!m_disposed)
                {
                    try
                    {
                        if (disposing)
                            m_parent = null;
                    }
                    finally
                    {
                        m_disposed = true;          // Prevent duplicate dispose.
                        base.Dispose(disposing);    // Call base class Dispose().
                    }
                }
            }

            /// <summary>
            /// Publish <see cref="IFrame"/> of time-aligned collection of <see cref="IMeasurement"/> values that arrived within the
            /// concentrator's defined <see cref="ConcentratorBase.LagTime"/>.
            /// </summary>
            /// <param name="frame"><see cref="IFrame"/> of measurements with the same timestamp that arrived within <see cref="ConcentratorBase.LagTime"/> that are ready for processing.</param>
            /// <param name="index">Index of <see cref="IFrame"/> within a second ranging from zero to <c><see cref="ConcentratorBase.FramesPerSecond"/> - 1</c>.</param>
            protected override void PublishFrame(IFrame frame, int index)
            {
                // Publish locally sorted measurements
                if ((object)m_parent != null)
                    m_parent.OnNewMeasurements(frame.Measurements.Values);
            }

            #endregion
        }

        // Constants
        private const int EvenKey = 0;      // Even key/IV index
        private const int OddKey = 1;       // Odd key/IV index
        private const int KeyIndex = 0;     // Index of cipher key component in keyIV array
        private const int IVIndex = 1;      // Index of initialization vector component in keyIV array

        private const long MissingCacheWarningInterval = 20000000;

        // Events

        /// <summary>
        /// Occurs when client connection to the data publication server is established.
        /// </summary>
        public event EventHandler ConnectionEstablished;

        /// <summary>
        /// Occurs when client connection to the data publication server is terminated.
        /// </summary>
        public event EventHandler ConnectionTerminated;

        /// <summary>
        /// Occurs when client connection to the data publication server has successfully authenticated.
        /// </summary>
        public event EventHandler ConnectionAuthenticated;

        /// <summary>
        /// Occurs when client receives requested meta-data transmitted by data publication server.
        /// </summary>
        public event EventHandler<EventArgs<DataSet>> MetaDataReceived;

        /// <summary>
        /// Occurs when first measurement is transmitted by data publication server.
        /// </summary>
        public event EventHandler<EventArgs<Ticks>> DataStartTime;

        /// <summary>
        /// Indicates that processing for an input adapter (via temporal session) has completed.
        /// </summary>
        /// <remarks>
        /// This event is expected to only be raised when an input adapter has been designed to process
        /// a finite amount of data, e.g., reading a historical range of data during temporal procesing.
        /// </remarks>
        public new event EventHandler<EventArgs<string>> ProcessingComplete;

        // Fields
        private TcpClient m_commandChannel;
        private UdpClient m_dataChannel;
        private LocalConcentrator m_localConcentrator;
        private System.Timers.Timer m_dataStreamMonitor;
        private long m_commandChannelConnectionAttempts;
        private long m_dataChannelConnectionAttempts;
        private volatile SignalIndexCache m_remoteSignalIndexCache;
        private volatile SignalIndexCache m_signalIndexCache;
        private volatile long[] m_baseTimeOffsets;
        private volatile int m_timeIndex;
        private volatile byte[][][] m_keyIVs;
        private volatile int m_cipherIndex;
        private volatile bool m_authenticated;
        private volatile bool m_subscribed;
        private volatile int m_lastBytesReceived;
        private long m_monitoredBytesReceived;
        private long m_totalBytesReceived;
        private long m_lastMissingCacheWarning;
        private Guid m_nodeID;
        private int m_gatewayProtocolID;
        private List<ServerCommand> m_requests;
        private bool m_synchronizedSubscription;
        private bool m_requireAuthentication;
        private bool m_useMillisecondResolution;
        private bool m_autoConnect;
        private string m_sharedSecret;
        private string m_authenticationID;
        private bool m_includeTime;
        private bool m_synchronizeMetadata;
        private bool m_internal;
        private OperationalModes m_operationalModes;
        private Encoding m_encoding;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="DataSubscriber"/>.
        /// </summary>
        public DataSubscriber()
        {
            m_requests = new List<ServerCommand>();
            m_encoding = Encoding.Unicode;
            DataLossInterval = 10.0D;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets flag that determines if <see cref="DataPublisher"/> requires subscribers to authenticate before making data requests.
        /// </summary>
        public bool RequireAuthentication
        {
            get
            {
                return m_requireAuthentication;
            }
            set
            {
                m_requireAuthentication = value;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if <see cref="DataSubscriber"/> should attempt to autoconnection to <see cref="DataPublisher"/> using defined connection settings.
        /// </summary>
        public bool AutoConnect
        {
            get
            {
                return m_autoConnect;
            }
            set
            {
                m_autoConnect = value;
            }
        }

        /// <summary>
        /// Gets or sets flag that informs publisher if base time-offsets can use millisecond resolution to conserve bandwidth.
        /// </summary>
        [Obsolete("SubscriptionInfo object defines this parameter.", false)]
        public bool UseMillisecondResolution
        {
            get
            {
                return m_useMillisecondResolution;
            }
            set
            {
                m_useMillisecondResolution = value;
            }
        }

        /// <summary>
        /// Gets flag that determines if this <see cref="DataSubscriber"/> has successfully authenticated with the <see cref="DataPublisher"/>.
        /// </summary>
        public bool Authenticated
        {
            get
            {
                return m_authenticated;
            }
        }

        /// <summary>
        /// Gets total data packet bytes received during this session.
        /// </summary>
        public long TotalBytesReceived
        {
            get
            {
                return m_totalBytesReceived;
            }
        }

        /// <summary>
        /// Gets or sets data loss monitoring interval, in seconds. Set to zero to disable monitoring.
        /// </summary>
        public double DataLossInterval
        {
            get
            {
                if ((object)m_dataStreamMonitor != null)
                    return m_dataStreamMonitor.Interval / 1000.0D;

                return 0.0D;
            }
            set
            {
                if (value > 0.0D)
                {
                    if ((object)m_dataStreamMonitor == null)
                    {
                        // Create data stream monitoring timer
                        m_dataStreamMonitor = new System.Timers.Timer();
                        m_dataStreamMonitor.Elapsed += m_dataStreamMonitor_Elapsed;
                        m_dataStreamMonitor.AutoReset = true;
                        m_dataStreamMonitor.Enabled = false;
                    }
                    // Set user specified interval
                    m_dataStreamMonitor.Interval = value * 1000.0D;
                }
                else
                {
                    // Disable data monitor
                    if ((object)m_dataStreamMonitor != null)
                    {
                        m_dataStreamMonitor.Elapsed -= m_dataStreamMonitor_Elapsed;
                        m_dataStreamMonitor.Dispose();
                    }
                    m_dataStreamMonitor = null;
                }
            }
        }

        /// <summary>
        /// Gets or sets a set of flags that define ways in
        /// which the subscriber and publisher communicate.
        /// </summary>
        public OperationalModes OperationalModes
        {
            get
            {
                return m_operationalModes;
            }
            set
            {
                OperationalEncoding operationalEncoding;

                m_operationalModes = value;
                operationalEncoding = (OperationalEncoding)(value & OperationalModes.EncodingMask);
                m_encoding = GetCharacterEncoding(operationalEncoding);
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="OperationalEncoding"/> used by the subscriber and publisher.
        /// </summary>
        public OperationalEncoding OperationalEncoding
        {
            get
            {
                return (OperationalEncoding)(m_operationalModes & OperationalModes.EncodingMask);
            }
            set
            {
                m_operationalModes &= ~OperationalModes.EncodingMask;
                m_operationalModes |= (OperationalModes)value;
                m_encoding = GetCharacterEncoding(value);
            }
        }

        /// <summary>
        /// Gets the flag indicating if this adapter supports temporal processing.
        /// </summary>
        /// <remarks>
        /// Although the data subscriber provisions support for temporal processing by receiving historical data from a remote source,
        /// the adapter opens sockets and does not need to be engaged within an actual temporal <see cref="IaonSession"/>, therefore
        /// this method returns <c>false</c> to make sure the adapter doesn't get instantiated within a temporal session.
        /// </remarks>
        public override bool SupportsTemporalProcessing
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets or sets the desired processing interval, in milliseconds, for the adapter.
        /// </summary>
        /// <remarks>
        /// With the exception of the values of -1 and 0, this value specifies the desired processing interval for data, i.e.,
        /// basically a delay, or timer interval, overwhich to process data. A value of -1 means to use the default processing
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

                // Request server update the processing interval
                SendServerCommand(ServerCommand.UpdateProcessingInterval, EndianOrder.BigEndian.GetBytes(value));
            }
        }

        /// <summary>
        /// Gets the status of this <see cref="DataSubscriber"/>.
        /// </summary>
        /// <remarks>
        /// Derived classes should provide current status information about the adapter for display purposes.
        /// </remarks>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.AppendFormat("         Subscription mode: {0}", m_synchronizedSubscription ? "Remotely Synchronized" : (object)m_localConcentrator == null ? "Unsynchronized" : "Locally Synchronized");
                status.AppendLine();
                status.AppendFormat("  Pending command requests: {0}", m_requests.Count);
                status.AppendLine();
                status.AppendFormat("             Authenticated: {0}", m_authenticated);
                status.AppendLine();
                status.AppendFormat("                Subscribed: {0}", m_subscribed);
                status.AppendLine();
                status.AppendFormat("      Data packet security: {0}", m_keyIVs == null ? "unencrypted" : "encrypted");
                status.AppendLine();

                if (DataLossInterval > 0.0D)
                    status.AppendFormat("No data reconnect interval: {0} seconds", DataLossInterval.ToString("0.000"));
                else
                    status.Append("No data reconnect interval: disabled");

                status.AppendLine();

                if ((object)m_dataChannel != null)
                {
                    status.AppendLine();
                    status.AppendLine("Data Channel Status".CenterText(50));
                    status.AppendLine("-------------------".CenterText(50));
                    status.Append(m_dataChannel.Status);
                }

                if ((object)m_commandChannel != null)
                {
                    status.AppendLine();
                    status.AppendLine("Command Channel Status".CenterText(50));
                    status.AppendLine("----------------------".CenterText(50));
                    status.Append(m_commandChannel.Status);
                }

                if ((object)m_localConcentrator != null)
                {
                    status.AppendLine();
                    status.AppendLine("Local Concentrator Status".CenterText(50));
                    status.AppendLine("-------------------------".CenterText(50));
                    status.Append(m_localConcentrator.Status);
                }

                status.Append(base.Status);

                return status.ToString();
            }
        }

        /// <summary>
        /// Gets a flag that determines if this <see cref="DataSubscriber"/> uses an asynchronous connection.
        /// </summary>
        protected override bool UseAsyncConnect
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets or sets reference to <see cref="UdpClient"/> data channel, attaching and/or detaching to events as needed.
        /// </summary>
        protected UdpClient DataChannel
        {
            get
            {
                return m_dataChannel;
            }
            set
            {
                if (m_dataChannel != null)
                {
                    // Detach from events on existing data channel reference
                    m_dataChannel.ConnectionException -= m_dataChannel_ConnectionException;
                    m_dataChannel.ConnectionAttempt -= m_dataChannel_ConnectionAttempt;
                    m_dataChannel.ReceiveData -= m_dataChannel_ReceiveData;
                    m_dataChannel.ReceiveDataException -= m_dataChannel_ReceiveDataException;

                    if (m_dataChannel != value)
                        m_dataChannel.Dispose();
                }

                // Assign new data channel reference
                m_dataChannel = value;

                if (m_dataChannel != null)
                {
                    // Attach to desired events on new data channel reference
                    m_dataChannel.ConnectionException += m_dataChannel_ConnectionException;
                    m_dataChannel.ConnectionAttempt += m_dataChannel_ConnectionAttempt;
                    m_dataChannel.ReceiveData += m_dataChannel_ReceiveData;
                    m_dataChannel.ReceiveDataException += m_dataChannel_ReceiveDataException;
                }
            }
        }

        /// <summary>
        /// Gets or sets reference to <see cref="TcpClient"/> command channel, attaching and/or detaching to events as needed.
        /// </summary>
        protected TcpClient CommandChannel
        {
            get
            {
                return m_commandChannel;
            }
            set
            {
                if (m_commandChannel != null)
                {
                    // Detach from events on existing command channel reference
                    m_commandChannel.ConnectionAttempt -= m_commandChannel_ConnectionAttempt;
                    m_commandChannel.ConnectionEstablished -= m_commandChannel_ConnectionEstablished;
                    m_commandChannel.ConnectionException -= m_commandChannel_ConnectionException;
                    m_commandChannel.ConnectionTerminated -= m_commandChannel_ConnectionTerminated;
                    m_commandChannel.ReceiveData -= m_commandChannel_ReceiveData;
                    m_commandChannel.ReceiveDataException -= m_commandChannel_ReceiveDataException;
                    m_commandChannel.SendDataException -= m_commandChannel_SendDataException;

                    if (m_commandChannel != value)
                        m_commandChannel.Dispose();
                }

                // Assign new command channel reference
                m_commandChannel = value;

                if (m_commandChannel != null)
                {
                    // Attach to desired events on new command channel reference
                    m_commandChannel.ConnectionAttempt += m_commandChannel_ConnectionAttempt;
                    m_commandChannel.ConnectionEstablished += m_commandChannel_ConnectionEstablished;
                    m_commandChannel.ConnectionException += m_commandChannel_ConnectionException;
                    m_commandChannel.ConnectionTerminated += m_commandChannel_ConnectionTerminated;
                    m_commandChannel.ReceiveData += m_commandChannel_ReceiveData;
                    m_commandChannel.ReceiveDataException += m_commandChannel_ReceiveDataException;
                    m_commandChannel.SendDataException += m_commandChannel_SendDataException;
                }
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="DataSubscriber"/> object and optionally releases the managed resources.
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
                        DataLossInterval = 0.0D;
                        CommandChannel = null;
                        DataChannel = null;
                        DisposeLocalConcentrator();
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
        /// Initializes <see cref="DataSubscriber"/>.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            Dictionary<string, string> settings = Settings;
            string setting;
            double interval;

            // Setup connection to data publishing server with or without authentication required
            if (settings.TryGetValue("requireAuthentication", out setting))
                m_requireAuthentication = setting.ParseBoolean();
            else
                m_requireAuthentication = false;

            if (m_requireAuthentication)
            {
                if (!settings.TryGetValue("sharedSecret", out m_sharedSecret) && !m_sharedSecret.IsNullOrWhiteSpace())
                    throw new ArgumentException("The \"sharedSecret\" setting must defined when authentication is required.");

                if (!settings.TryGetValue("authenticationID", out m_authenticationID) && !m_authenticationID.IsNullOrWhiteSpace())
                    throw new ArgumentException("The \"authenticationID\" setting must defined when authentication is required.");
            }

            // Check if synchronize metadata is disabled.
            if (settings.TryGetValue("synchronizeMetadata", out setting))
                m_synchronizeMetadata = setting.ParseBoolean();
            else
                m_synchronizeMetadata = true;   // by default, we will always perform this.

            // Check if measurements for this connection should be marked as "internal" - i.e., owned and allowed for proxy
            if (settings.TryGetValue("internal", out setting))
                m_internal = setting.ParseBoolean();

            // Check if we should be using the alternate binary format for communications with the publisher
            if (settings.TryGetValue("operationalModes", out setting))
                m_operationalModes = (OperationalModes)uint.Parse(setting);

            // Check if user wants to request that publisher use millisecond resolution to conserve bandwidth
            if (settings.TryGetValue("useMillisecondResolution", out setting))
                m_useMillisecondResolution = setting.ParseBoolean();

            // Define auto connect setting
            if (settings.TryGetValue("autoConnect", out setting))
                m_autoConnect = setting.ParseBoolean();

            // Define data loss interval
            if (settings.TryGetValue("dataLossInterval", out setting) && double.TryParse(setting, out interval))
                DataLossInterval = interval;

            if (m_autoConnect)
            {
                // Connect to local events when automatically engaging connection cycle
                ConnectionAuthenticated += DataSubscriber_ConnectionAuthenticated;
                MetaDataReceived += DataSubscriber_MetaDataReceived;

                // If active measurements are defined, attempt to defined desired subscription points from there
                if (DataSource != null && (object)DataSource.Tables != null && DataSource.Tables.Contains("ActiveMeasurements"))
                {
                    try
                    {
                        // Filter to points associated with this subscriber that have been requested for subscription, are enabled and not owned locally
                        DataRow[] filteredRows = DataSource.Tables["ActiveMeasurements"].Select("Subscribed <> 0");
                        List<IMeasurement> subscribedMeasurements = new List<IMeasurement>();
                        MeasurementKey key;
                        Guid signalID;

                        foreach (DataRow row in filteredRows)
                        {
                            // Create a new measurement for the provided field level information
                            Measurement measurement = new Measurement();

                            // Parse primary measurement identifier
                            signalID = row["SignalID"].ToNonNullString(Guid.Empty.ToString()).ConvertToType<Guid>();

                            // Set measurement key if defined
                            if (MeasurementKey.TryParse(row["ID"].ToString(), signalID, out key))
                                measurement.Key = key;

                            // Assign other attributes
                            measurement.ID = signalID;
                            measurement.TagName = row["PointTag"].ToNonNullString();
                            measurement.Multiplier = double.Parse(row["Multiplier"].ToString());
                            measurement.Adder = double.Parse(row["Adder"].ToString());

                            subscribedMeasurements.Add(measurement);
                        }

                        if (subscribedMeasurements.Count > 0)
                        {
                            // Combine subscribed output measurement with any existing output measurement and return unique set
                            if (OutputMeasurements == null)
                                OutputMeasurements = subscribedMeasurements.ToArray();
                            else
                                OutputMeasurements = subscribedMeasurements.Concat(OutputMeasurements).Distinct().ToArray();
                        }
                    }
                    catch (Exception ex)
                    {
                        // Errors here may not be catastrophic, this simply limits the auto-assignment of input measurement keys desired for subscription
                        OnProcessException(new InvalidOperationException(string.Format("Failed to define subscribed measurements: {0}", ex.Message), ex));
                    }
                }
            }

            // Create a new TCP client
            TcpClient commandChannel = new TcpClient();

            // Initialize default settings
            commandChannel.ConnectionString = "server=localhost:6165";
            commandChannel.PayloadAware = true;
            commandChannel.PersistSettings = false;
            commandChannel.MaxConnectionAttempts = 1;

            // Assign command channel client reference and attach to needed events
            this.CommandChannel = commandChannel;

            // Get proper connection string - either from specified command channel
            // or from base connection string
            if (settings.TryGetValue("commandChannel", out setting))
                commandChannel.ConnectionString = setting;
            else
                commandChannel.ConnectionString = ConnectionString;

            // Register subscriber with the statistics engine
            //StatisticsEngine.Register(this, "Subscriber", "SUB");

            Initialized = true;
        }

        /// <summary>
        /// Authenticates subcriber to a data publisher.
        /// </summary>
        /// <param name="sharedSecret">Shared secret used to look up private crypto key and initialization vector.</param>
        /// <param name="authenticationID">Authentication ID that publisher will use to validate subscriber identity.</param>
        /// <returns><c>true</c> if authentication transmission was successful; otherwise <c>false</c>.</returns>
        public virtual bool Authenticate(string sharedSecret, string authenticationID)
        {
            return false;
//            if (!authenticationID.IsNullOrWhiteSpace())
//            {
//                try
//                {
//                    MemoryStream buffer = new MemoryStream();
//                    byte[] salt = new byte[DataPublisher.CipherSaltLength];
//                    byte[] bytes;
//
//                    // Generate some random prefix data to make sure auth key transmission is always unique
//                    GSF.Security.Cryptography.Random.GetBytes(salt);
//
//                    // Get encoded bytes of authentication key
//                    bytes = salt.Combine(m_encoding.GetBytes(authenticationID));
//
//                    // Encrypt authentication key
//                    bytes = bytes.Encrypt(sharedSecret, CipherStrength.Aes256);
//
//                    // Write encoded authentication key length into buffer
//                    buffer.Write(EndianOrder.BigEndian.GetBytes(bytes.Length), 0, 4);
//
//                    // Encode encrypted authentication key into buffer
//                    buffer.Write(bytes, 0, bytes.Length);
//
//                    // Send authentication command to server with associated command buffer
//                    return SendServerCommand(ServerCommand.Authenticate, buffer.ToArray());
//                }
//                catch (Exception ex)
//                {
//                    OnProcessException(new InvalidOperationException("Exception occurred while trying to authenticate publisher subscription: " + ex.Message, ex));
//                }
//            }
//            else
//                OnProcessException(new InvalidOperationException("Cannot authenticate subscription without a connection string."));
//
//            return false;
        }

        /// <summary>
        /// Subscribes (or re-subscribes) to a data publisher for a set of data points.
        /// </summary>
        /// <param name="info">Configuration object that defines the subscription.</param>
        /// <returns><c>true</c> if subscribe transmission was successful; otherwise <c>false</c>.</returns>
        public bool Subscribe(SubscriptionInfo info)
        {
            if (info is SynchronizedSubscriptionInfo)
                return SynchronizedSubscribe((SynchronizedSubscriptionInfo)info);

            if (info is UnsynchronizedSubscriptionInfo)
                return UnsynchronizedSubscribe((UnsynchronizedSubscriptionInfo)info);

            throw new NotSupportedException("Type of subscription used is not supported");
        }

        /// <summary>
        /// Subscribes (or re-subscribes) to a data publisher for a synchronized set of data points.
        /// </summary>
        /// <param name="info">Configuration object that defines the subscription.</param>
        /// <returns><c>true</c> if subscribe transmission was successful; otherwise <c>false</c>.</returns>
        public bool SynchronizedSubscribe(SynchronizedSubscriptionInfo info)
        {
            StringBuilder connectionString = new StringBuilder();
            AssemblyInfo assemblyInfo = AssemblyInfo.ExecutingAssembly;

            // Dispose of any previously established local concentrator
            DisposeLocalConcentrator();

            if (info.RemotelySynchronized)
            {
                connectionString.AppendFormat("framesPerSecond={0};", info.FramesPerSecond);
                connectionString.AppendFormat("lagTime={0};", info.LagTime);
                connectionString.AppendFormat("leadTime={0};", info.LeadTime);
                connectionString.AppendFormat("includeTime=false;");
                connectionString.AppendFormat("useLocalClockAsRealTime={0};", info.UseLocalClockAsRealTime);
                connectionString.AppendFormat("ignoreBadTimestamps={0};", info.IgnoreBadTimestamps);
                connectionString.AppendFormat("allowSortsByArrival={0};", info.AllowSortsByArrival);
                connectionString.AppendFormat("timeResolution={0};", info.TimeResolution);
                connectionString.AppendFormat("allowPreemptivePublishing={0};", info.AllowPreemptivePublishing);
                connectionString.AppendFormat("downsamplingMethod={0};", info.DownsamplingMethod.ToString());
                connectionString.AppendFormat("processingInterval={0};", info.ProcessingInterval);
                connectionString.AppendFormat("assemblyInfo={{source={0};version={1}.{2}.{3};buildDate={4}}};", assemblyInfo.Name, assemblyInfo.Version.Major, assemblyInfo.Version.Minor, assemblyInfo.Version.Build, assemblyInfo.BuildDate.ToString("yyyy-MM-dd HH:mm:ss"));

                if (!info.FilterExpression.IsNullOrWhiteSpace())
                    connectionString.AppendFormat("inputMeasurementKeys={{{0}}};", info.FilterExpression);

                if (info.UdpDataChannel)
                    connectionString.AppendFormat("dataChannel={{localport={0}}};", info.DataChannelLocalPort);

                if (!info.StartTime.IsNullOrWhiteSpace())
                    connectionString.AppendFormat("startTimeConstraint={0};", info.StartTime);

                if (!info.StopTime.IsNullOrWhiteSpace())
                    connectionString.AppendFormat("stopTimeConstraint={0};", info.StopTime);

                if (!info.ConstraintParameters.IsNullOrWhiteSpace())
                    connectionString.AppendFormat("timeConstraintParameters={0};", info.ConstraintParameters);

                if (!info.WaitHandleNames.IsNullOrWhiteSpace())
                {
                    connectionString.AppendFormat("waitHandleNames={0};", info.WaitHandleNames);
                    connectionString.AppendFormat("waitHandleTimeout={0};", info.WaitHandleTimeout);
                }

                if (!info.ExtraConnectionStringParameters.IsNullOrWhiteSpace())
                    connectionString.AppendFormat("{0};", info.ExtraConnectionStringParameters);

                return Subscribe(true, info.UseCompactMeasurementFormat, connectionString.ToString());
            }
            else
            {
                // Locally concentrated subscription simply uses an unsynchronized subscription and concentrates the
                // measurements on the subscriber side
                if (Subscribe(FromLocallySynchronizedInfo(info)))
                {
                    // Establish a local concentrator to synchronize received measurements
                    LocalConcentrator localConcentrator = new LocalConcentrator(this);
                    localConcentrator.ProcessException += m_localConcentrator_ProcessException;
                    localConcentrator.FramesPerSecond = info.FramesPerSecond;
                    localConcentrator.LagTime = info.LagTime;
                    localConcentrator.LeadTime = info.LeadTime;
                    localConcentrator.UseLocalClockAsRealTime = info.UseLocalClockAsRealTime;
                    localConcentrator.IgnoreBadTimestamps = info.IgnoreBadTimestamps;
                    localConcentrator.AllowSortsByArrival = info.AllowSortsByArrival;
                    localConcentrator.TimeResolution = info.TimeResolution;
                    localConcentrator.AllowPreemptivePublishing = info.AllowPreemptivePublishing;
                    localConcentrator.DownsamplingMethod = info.DownsamplingMethod;
                    localConcentrator.UsePrecisionTimer = false;

                    // Parse time constraints, if defined
                    DateTime startTimeConstraint = !info.StartTime.IsNullOrWhiteSpace() ? AdapterBase.ParseTimeTag(info.StartTime) : DateTime.MinValue;
                    DateTime stopTimeConstraint = !info.StopTime.IsNullOrWhiteSpace() ? AdapterBase.ParseTimeTag(info.StopTime) : DateTime.MaxValue;

                    // When processing historical data, timestamps should not be evaluated for reasonability
                    if (startTimeConstraint != DateTime.MinValue || stopTimeConstraint != DateTime.MaxValue)
                    {
                        localConcentrator.PerformTimestampReasonabilityCheck = false;
                        localConcentrator.LeadTime = double.MaxValue;
                    }

                    // Assign alternate processing interval, if defined
                    if (info.ProcessingInterval != -1)
                        localConcentrator.ProcessingInterval = info.ProcessingInterval;

                    // Start local concentrator
                    localConcentrator.Start();

                    // Move concentrator to member variable
                    Interlocked.Exchange(ref m_localConcentrator, localConcentrator);

                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Subscribes (or re-subscribes) to a data publisher for an unsynchronized set of data points.
        /// </summary>
        /// <param name="info">Configuration object that defines the subscription.</param>
        /// <returns><c>true</c> if subscribe transmission was successful; otherwise <c>false</c>.</returns>
        public bool UnsynchronizedSubscribe(UnsynchronizedSubscriptionInfo info)
        {
            // Dispose of any previously established local concentrator
            DisposeLocalConcentrator();

            StringBuilder connectionString = new StringBuilder();
            AssemblyInfo assemblyInfo = AssemblyInfo.ExecutingAssembly;

            connectionString.AppendFormat("trackLatestMeasurements={0};", info.Throttled);
            connectionString.AppendFormat("publishInterval={0};", info.PublishInterval);
            connectionString.AppendFormat("includeTime={0};", info.IncludeTime);
            connectionString.AppendFormat("lagTime={0};", info.LagTime);
            connectionString.AppendFormat("leadTime={0};", info.LeadTime);
            connectionString.AppendFormat("useLocalClockAsRealTime={0};", info.UseLocalClockAsRealTime);
            connectionString.AppendFormat("processingInterval={0};", info.ProcessingInterval);
            connectionString.AppendFormat("useMillisecondResolution={0};", info.UseMillisecondResolution);
            connectionString.AppendFormat("assemblyInfo={{source={0};version={1}.{2}.{3};buildDate={4}}};", assemblyInfo.Name, assemblyInfo.Version.Major, assemblyInfo.Version.Minor, assemblyInfo.Version.Build, assemblyInfo.BuildDate.ToString("yyyy-MM-dd HH:mm:ss"));

            if (!info.FilterExpression.IsNullOrWhiteSpace())
                connectionString.AppendFormat("inputMeasurementKeys={{{0}}};", info.FilterExpression);

            if (info.UdpDataChannel)
                connectionString.AppendFormat("dataChannel={{localport={0}}};", info.DataChannelLocalPort);

            if (!info.StartTime.IsNullOrWhiteSpace())
                connectionString.AppendFormat("startTimeConstraint={0};", info.StartTime);

            if (!info.StopTime.IsNullOrWhiteSpace())
                connectionString.AppendFormat("stopTimeConstraint={0};", info.StopTime);

            if (!info.ConstraintParameters.IsNullOrWhiteSpace())
                connectionString.AppendFormat("timeConstraintParameters={0};", info.ConstraintParameters);

            if (!info.WaitHandleNames.IsNullOrWhiteSpace())
            {
                connectionString.AppendFormat("waitHandleNames={0};", info.WaitHandleNames);
                connectionString.AppendFormat("waitHandleTimeout={0};", info.WaitHandleTimeout);
            }

            if (!info.ExtraConnectionStringParameters.IsNullOrWhiteSpace())
                connectionString.AppendFormat("{0};", info.ExtraConnectionStringParameters);

            // Make sure not to monitor for data loss any faster than downsample time on throttled connections - additionally
            // you will want to make sure data stream monitor is twice lagtime to allow time for initial points to arrive.
            if (info.Throttled && (object)m_dataStreamMonitor != null && m_dataStreamMonitor.Interval / 1000.0D < info.LagTime)
                m_dataStreamMonitor.Interval = 2.0D * info.LagTime * 1000.0D;

            // Set millisecond resolution member variable for compact measurement parsing
            m_useMillisecondResolution = info.UseMillisecondResolution;

            return Subscribe(false, info.UseCompactMeasurementFormat, connectionString.ToString());
        }
  
        public bool RemotelySynchronizedSubscribe(bool compactFormat, int framesPerSecond, double lagTime, double leadTime, string filterExpression)
        {
            return RemotelySynchronizedSubscribe(compactFormat, framesPerSecond, lagTime, leadTime, filterExpression, null, false, false, true, Ticks.PerMillisecond, true, DownsamplingMethod.LastReceived, null, null, null,  -1, null, 0);
        }
        
        /// <summary>
        /// Subscribes (or re-subscribes) to a data publisher for a remotely synchronized set of data points.
        /// </summary>
        /// <param name="compactFormat">Boolean value that determines if the compact measurement format should be used. Set to <c>false</c> for full fidelity measurement serialization; otherwise set to <c>true</c> for bandwidth conservation.</param>
        /// <param name="framesPerSecond">The desired number of data frames per second.</param>
        /// <param name="lagTime">Allowed past time deviation tolerance, in seconds (can be subsecond).</param>
        /// <param name="leadTime">Allowed future time deviation tolerance, in seconds (can be subsecond).</param>
        /// <param name="filterExpression">Filtering expression that defines the measurements that are being subscribed.</param>
        /// <param name="dataChannel">Desired UDP return data channel connection string to use for data packet transmission. Set to <c>null</c> to use TCP channel for data transmission.</param>
        /// <param name="useLocalClockAsRealTime">Boolean value that determines whether or not to use the local clock time as real-time.</param>
        /// <param name="ignoreBadTimestamps">Boolean value that determines if bad timestamps (as determined by measurement's timestamp quality) should be ignored when sorting measurements.</param>
        /// <param name="allowSortsByArrival"> Gets or sets flag that determines whether or not to allow incoming measurements with bad timestamps to be sorted by arrival time.</param>
        /// <param name="timeResolution">Gets or sets the maximum time resolution, in ticks, to use when sorting measurements by timestamps into their proper destination frame.</param>
        /// <param name="allowPreemptivePublishing">Gets or sets flag that allows system to preemptively publish frames assuming all expected measurements have arrived.</param>
        /// <param name="downsamplingMethod">Gets the total number of downsampled measurements processed by the concentrator.</param>
        /// <param name="startTime">Defines a relative or exact start time for the temporal constraint to use for historical playback.</param>
        /// <param name="stopTime">Defines a relative or exact stop time for the temporal constraint to use for historical playback.</param>
        /// <param name="constraintParameters">Defines any temporal parameters related to the constraint to use for historical playback.</param>
        /// <param name="processingInterval">Defines the desired processing interval milliseconds, i.e., historical play back speed, to use when temporal constraints are defined.</param>
        /// <param name="waitHandleNames">Comma separated list of wait handle names used to establish external event wait handles needed for inter-adapter synchronization.</param>
        /// <param name="waitHandleTimeout">Maximum wait time for external events, in milliseconds, before proceeding.</param>
        /// <returns><c>true</c> if subscribe transmission was successful; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// <para>
        /// When the <paramref name="startTime"/> or <paramref name="stopTime"/> temporal processing contraints are defined (i.e., not <c>null</c>), this
        /// specifies the start and stop time over which the subscriber session will process data. Passing in <c>null</c> for the <paramref name="startTime"/>
        /// and <paramref name="stopTime"/> specifies the the subscriber session will process data in standard, i.e., real-time, operation.
        /// </para>
        /// <para>
        /// With the exception of the values of -1 and 0, the <paramref name="processingInterval"/> value specifies the desired historical playback data
        /// processing interval in milliseconds. This is basically a delay, or timer interval, overwhich to process data. Setting this value to -1 means
        /// to use the default processing interval while setting the value to 0 means to process data as fast as possible.
        /// </para>
        /// <para>
        /// The <paramref name="startTime"/> and <paramref name="stopTime"/> parameters can be specified in one of the
        /// following formats:
        /// <list type="table">
        ///     <listheader>
        ///         <term>Time Format</term>
        ///         <description>Format Description</description>
        ///     </listheader>
        ///     <item>
        ///         <term>12-30-2000 23:59:59.033</term>
        ///         <description>Absolute date and time.</description>
        ///     </item>
        ///     <item>
        ///         <term>*</term>
        ///         <description>Evaluates to <see cref="DateTime.UtcNow"/>.</description>
        ///     </item>
        ///     <item>
        ///         <term>*-20s</term>
        ///         <description>Evaluates to 20 seconds before <see cref="DateTime.UtcNow"/>.</description>
        ///     </item>
        ///     <item>
        ///         <term>*-10m</term>
        ///         <description>Evaluates to 10 minutes before <see cref="DateTime.UtcNow"/>.</description>
        ///     </item>
        ///     <item>
        ///         <term>*-1h</term>
        ///         <description>Evaluates to 1 hour before <see cref="DateTime.UtcNow"/>.</description>
        ///     </item>
        ///     <item>
        ///         <term>*-1d</term>
        ///         <description>Evaluates to 1 day before <see cref="DateTime.UtcNow"/>.</description>
        ///     </item>
        /// </list>
        /// </para>
        /// </remarks>
        [Obsolete("Preferred method uses SubscriptionInfo object to subscribe.", false)]
        public virtual bool RemotelySynchronizedSubscribe(bool compactFormat, int framesPerSecond, double lagTime, double leadTime, string filterExpression, string dataChannel, bool useLocalClockAsRealTime, bool ignoreBadTimestamps, bool allowSortsByArrival, long timeResolution, bool allowPreemptivePublishing, DownsamplingMethod downsamplingMethod, string startTime, string stopTime, string constraintParameters, int processingInterval, string waitHandleNames, int waitHandleTimeout)
        {
            // Dispose of any previously established local concentrator
            DisposeLocalConcentrator();

            StringBuilder connectionString = new StringBuilder();
            AssemblyInfo assemblyInfo = AssemblyInfo.ExecutingAssembly;

            connectionString.AppendFormat("framesPerSecond={0}; ", framesPerSecond);
            connectionString.AppendFormat("lagTime={0}; ", lagTime);
            connectionString.AppendFormat("leadTime={0}; ", leadTime);
            connectionString.AppendFormat("inputMeasurementKeys={{{0}}}; ", filterExpression.ToNonNullString());
            connectionString.AppendFormat("dataChannel={{{0}}}; ", dataChannel.ToNonNullString());
            connectionString.AppendFormat("includeTime=false; ");
            connectionString.AppendFormat("useLocalClockAsRealTime={0}; ", useLocalClockAsRealTime);
            connectionString.AppendFormat("ignoreBadTimestamps={0}; ", ignoreBadTimestamps);
            connectionString.AppendFormat("allowSortsByArrival={0}; ", allowSortsByArrival);
            connectionString.AppendFormat("timeResolution={0}; ", (long)timeResolution);
            connectionString.AppendFormat("allowPreemptivePublishing={0}; ", allowPreemptivePublishing);
            connectionString.AppendFormat("downsamplingMethod={0}; ", downsamplingMethod.ToString());
            connectionString.AppendFormat("startTimeConstraint={0}; ", startTime.ToNonNullString());
            connectionString.AppendFormat("stopTimeConstraint={0}; ", stopTime.ToNonNullString());
            connectionString.AppendFormat("timeConstraintParameters={0}; ", constraintParameters.ToNonNullString());
            connectionString.AppendFormat("processingInterval={0}; ", processingInterval);
            connectionString.AppendFormat("assemblyInfo={{source={0}; version={1}.{2}.{3}; buildDate={4}}}", assemblyInfo.Name, assemblyInfo.Version.Major, assemblyInfo.Version.Minor, assemblyInfo.Version.Build, assemblyInfo.BuildDate.ToString("yyyy-MM-dd HH:mm:ss"));

            if (!waitHandleNames.IsNullOrWhiteSpace())
            {
                connectionString.AppendFormat("; waitHandleNames={0}", waitHandleNames);
                connectionString.AppendFormat("; waitHandleTimeout={0}", waitHandleTimeout);
            }

            return Subscribe(true, compactFormat, connectionString.ToString());
        }
  
        public bool LocallySynchronizedSubscribe(bool compactFormat, int framesPerSecond, double lagTime, double leadTime, string filterExpression)
        {
            return LocallySynchronizedSubscribe(compactFormat, framesPerSecond, lagTime, leadTime, filterExpression, null, false, false, true, Ticks.PerMillisecond, true, DownsamplingMethod.LastReceived, null, null, null, -1, null, 0);
        }
        
        /// <summary>
        /// Subscribes (or re-subscribes) to a data publisher for a locally synchronized set of data points.
        /// </summary>
        /// <param name="compactFormat">Boolean value that determines if the compact measurement format should be used. Set to <c>false</c> for full fidelity measurement serialization; otherwise set to <c>true</c> for bandwidth conservation.</param>
        /// <param name="framesPerSecond">The desired number of data frames per second.</param>
        /// <param name="lagTime">Allowed past time deviation tolerance, in seconds (can be subsecond).</param>
        /// <param name="leadTime">Allowed future time deviation tolerance, in seconds (can be subsecond).</param>
        /// <param name="filterExpression">Filtering expression that defines the measurements that are being subscribed.</param>
        /// <param name="dataChannel">Desired UDP return data channel connection string to use for data packet transmission. Set to <c>null</c> to use TCP channel for data transmission.</param>
        /// <param name="useLocalClockAsRealTime">Boolean value that determines whether or not to use the local clock time as real-time.</param>
        /// <param name="ignoreBadTimestamps">Boolean value that determines if bad timestamps (as determined by measurement's timestamp quality) should be ignored when sorting measurements.</param>
        /// <param name="allowSortsByArrival"> Gets or sets flag that determines whether or not to allow incoming measurements with bad timestamps to be sorted by arrival time.</param>
        /// <param name="timeResolution">Gets or sets the maximum time resolution, in ticks, to use when sorting measurements by timestamps into their proper destination frame.</param>
        /// <param name="allowPreemptivePublishing">Gets or sets flag that allows system to preemptively publish frames assuming all expected measurements have arrived.</param>
        /// <param name="downsamplingMethod">Gets the total number of downsampled measurements processed by the concentrator.</param>
        /// <param name="startTime">Defines a relative or exact start time for the temporal constraint to use for historical playback.</param>
        /// <param name="stopTime">Defines a relative or exact stop time for the temporal constraint to use for historical playback.</param>
        /// <param name="constraintParameters">Defines any temporal parameters related to the constraint to use for historical playback.</param>
        /// <param name="processingInterval">Defines the desired processing interval milliseconds, i.e., historical play back speed, to use when temporal constraints are defined.</param>
        /// <param name="waitHandleNames">Comma separated list of wait handle names used to establish external event wait handles needed for inter-adapter synchronization.</param>
        /// <param name="waitHandleTimeout">Maximum wait time for external events, in milliseconds, before proceeding.</param>
        /// <returns><c>true</c> if subscribe transmission was successful; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// <para>
        /// When the <paramref name="startTime"/> or <paramref name="stopTime"/> temporal processing contraints are defined (i.e., not <c>null</c>), this
        /// specifies the start and stop time over which the subscriber session will process data. Passing in <c>null</c> for the <paramref name="startTime"/>
        /// and <paramref name="stopTime"/> specifies the the subscriber session will process data in standard, i.e., real-time, operation.
        /// </para>
        /// <para>
        /// With the exception of the values of -1 and 0, the <paramref name="processingInterval"/> value specifies the desired historical playback data
        /// processing interval in milliseconds. This is basically a delay, or timer interval, overwhich to process data. Setting this value to -1 means
        /// to use the default processing interval while setting the value to 0 means to process data as fast as possible.
        /// </para>
        /// <para>
        /// The <paramref name="startTime"/> and <paramref name="stopTime"/> parameters can be specified in one of the
        /// following formats:
        /// <list type="table">
        ///     <listheader>
        ///         <term>Time Format</term>
        ///         <description>Format Description</description>
        ///     </listheader>
        ///     <item>
        ///         <term>12-30-2000 23:59:59.033</term>
        ///         <description>Absolute date and time.</description>
        ///     </item>
        ///     <item>
        ///         <term>*</term>
        ///         <description>Evaluates to <see cref="DateTime.UtcNow"/>.</description>
        ///     </item>
        ///     <item>
        ///         <term>*-20s</term>
        ///         <description>Evaluates to 20 seconds before <see cref="DateTime.UtcNow"/>.</description>
        ///     </item>
        ///     <item>
        ///         <term>*-10m</term>
        ///         <description>Evaluates to 10 minutes before <see cref="DateTime.UtcNow"/>.</description>
        ///     </item>
        ///     <item>
        ///         <term>*-1h</term>
        ///         <description>Evaluates to 1 hour before <see cref="DateTime.UtcNow"/>.</description>
        ///     </item>
        ///     <item>
        ///         <term>*-1d</term>
        ///         <description>Evaluates to 1 day before <see cref="DateTime.UtcNow"/>.</description>
        ///     </item>
        /// </list>
        /// </para>
        /// </remarks>
        [Obsolete("Preferred method uses SubscriptionInfo object to subscribe.", false)]
        public virtual bool LocallySynchronizedSubscribe(bool compactFormat, int framesPerSecond, double lagTime, double leadTime, string filterExpression, string dataChannel, bool useLocalClockAsRealTime, bool ignoreBadTimestamps, bool allowSortsByArrival, long timeResolution, bool allowPreemptivePublishing, DownsamplingMethod downsamplingMethod, string startTime, string stopTime, string constraintParameters, int processingInterval, string waitHandleNames, int waitHandleTimeout)
        {
            // Dispose of any previously established local concentrator
            DisposeLocalConcentrator();

            // Establish a local concentrator to synchronize received measurements
            m_localConcentrator = new LocalConcentrator(this);
            m_localConcentrator.ProcessException += m_localConcentrator_ProcessException;
            m_localConcentrator.FramesPerSecond = framesPerSecond;
            m_localConcentrator.LagTime = lagTime;
            m_localConcentrator.LeadTime = leadTime;
            m_localConcentrator.UseLocalClockAsRealTime = useLocalClockAsRealTime;
            m_localConcentrator.IgnoreBadTimestamps = ignoreBadTimestamps;
            m_localConcentrator.AllowSortsByArrival = allowSortsByArrival;
            m_localConcentrator.TimeResolution = timeResolution;
            m_localConcentrator.AllowPreemptivePublishing = allowPreemptivePublishing;
            m_localConcentrator.DownsamplingMethod = downsamplingMethod;
            m_localConcentrator.UsePrecisionTimer = false;

            // Parse time constraints, if defined
            DateTime startTimeConstraint = !startTime.IsNullOrWhiteSpace() ? AdapterBase.ParseTimeTag(startTime) : DateTime.MinValue;
            DateTime stopTimeConstraint = !stopTime.IsNullOrWhiteSpace() ? AdapterBase.ParseTimeTag(stopTime) : DateTime.MaxValue;

            // When processing historical data, timestamps should not be evaluated for reasonability
            if (startTimeConstraint != DateTime.MinValue || stopTimeConstraint != DateTime.MaxValue)
            {
                m_localConcentrator.PerformTimestampReasonabilityCheck = false;
                m_localConcentrator.LeadTime = double.MaxValue;
            }

            // Assign alternate processing interval, if defined
            if (processingInterval != -1)
                m_localConcentrator.ProcessingInterval = processingInterval;

            // Initiate unsynchronized subscribe
            StringBuilder connectionString = new StringBuilder();
            AssemblyInfo assemblyInfo = AssemblyInfo.ExecutingAssembly;

            connectionString.AppendFormat("trackLatestMeasurements={0}; ", false);
            connectionString.AppendFormat("inputMeasurementKeys={{{0}}}; ", filterExpression.ToNonNullString());
            connectionString.AppendFormat("dataChannel={{{0}}}; ", dataChannel.ToNonNullString());
            connectionString.AppendFormat("includeTime={0}; ", true);
            connectionString.AppendFormat("lagTime={0}; ", 10.0D);
            connectionString.AppendFormat("leadTime={0}; ", 5.0D);
            connectionString.AppendFormat("useLocalClockAsRealTime={0}; ", false);
            connectionString.AppendFormat("startTimeConstraint={0}; ", startTime.ToNonNullString());
            connectionString.AppendFormat("stopTimeConstraint={0}; ", stopTime.ToNonNullString());
            connectionString.AppendFormat("timeConstraintParameters={0}; ", constraintParameters.ToNonNullString());
            connectionString.AppendFormat("processingInterval={0}; ", processingInterval);
            connectionString.AppendFormat("useMillisecondResolution={0}; ", m_useMillisecondResolution);
            connectionString.AppendFormat("assemblyInfo={{source={0}; version={1}.{2}.{3}; buildDate={4}}}", assemblyInfo.Name, assemblyInfo.Version.Major, assemblyInfo.Version.Minor, assemblyInfo.Version.Build, assemblyInfo.BuildDate.ToString("yyyy-MM-dd HH:mm:ss"));

            if (!waitHandleNames.IsNullOrWhiteSpace())
            {
                connectionString.AppendFormat("; waitHandleNames={0}", waitHandleNames);
                connectionString.AppendFormat("; waitHandleTimeout={0}", waitHandleTimeout);
            }

            // Start subscription process
            if (Subscribe(false, compactFormat, connectionString.ToString()))
            {
                // If subscription succeeds, start local concentrator
                m_localConcentrator.Start();
                return true;
            }

            return false;
        }
  
        public bool UnsynchronizedSubscribe(bool compactFormat, bool throttled, string filterExpression)
        {
            return UnsynchronizedSubscribe(compactFormat, throttled, filterExpression, null, true, 10.0D, 5.0D, false, null, null, null, -1, null, 0);
        }
        
        /// <summary>
        /// Subscribes (or re-subscribes) to a data publisher for an unsynchronized set of data points.
        /// </summary>
        /// <param name="compactFormat">Boolean value that determines if the compact measurement format should be used. Set to <c>false</c> for full fidelity measurement serialization; otherwise set to <c>true</c> for bandwidth conservation.</param>
        /// <param name="throttled">Boolean value that determines if data should be throttled at a set transmission interval or sent on change.</param>
        /// <param name="filterExpression">Filtering expression that defines the measurements that are being subscribed.</param>
        /// <param name="dataChannel">Desired UDP return data channel connection string to use for data packet transmission. Set to <c>null</c> to use TCP channel for data transmission.</param>
        /// <param name="includeTime">Boolean value that determines if time is a necessary component in streaming data.</param>
        /// <param name="lagTime">When <paramref name="throttled"/> is <c>true</c>, defines the data transmission speed in seconds (can be subsecond).</param>
        /// <param name="leadTime">When <paramref name="throttled"/> is <c>true</c>, defines the allowed time deviation tolerance to real-time in seconds (can be subsecond).</param>
        /// <param name="useLocalClockAsRealTime">When <paramref name="throttled"/> is <c>true</c>, defines boolean value that determines whether or not to use the local clock time as real-time. Set to <c>false</c> to use latest received measurement timestamp as real-time.</param>
        /// <param name="startTime">Defines a relative or exact start time for the temporal constraint to use for historical playback.</param>
        /// <param name="stopTime">Defines a relative or exact stop time for the temporal constraint to use for historical playback.</param>
        /// <param name="constraintParameters">Defines any temporal parameters related to the constraint to use for historical playback.</param>
        /// <param name="processingInterval">Defines the desired processing interval milliseconds, i.e., historical play back speed, to use when temporal constraints are defined.</param>
        /// <param name="waitHandleNames">Comma separated list of wait handle names used to establish external event wait handles needed for inter-adapter synchronization.</param>
        /// <param name="waitHandleTimeout">Maximum wait time for external events, in milliseconds, before proceeding.</param>
        /// <returns><c>true</c> if subscribe transmission was successful; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// <para>
        /// When the <paramref name="startTime"/> or <paramref name="stopTime"/> temporal processing contraints are defined (i.e., not <c>null</c>), this
        /// specifies the start and stop time over which the subscriber session will process data. Passing in <c>null</c> for the <paramref name="startTime"/>
        /// and <paramref name="stopTime"/> specifies the the subscriber session will process data in standard, i.e., real-time, operation.
        /// </para>
        /// <para>
        /// With the exception of the values of -1 and 0, the <paramref name="processingInterval"/> value specifies the desired historical playback data
        /// processing interval in milliseconds. This is basically a delay, or timer interval, overwhich to process data. Setting this value to -1 means
        /// to use the default processing interval while setting the value to 0 means to process data as fast as possible.
        /// </para>
        /// <para>
        /// The <paramref name="startTime"/> and <paramref name="stopTime"/> parameters can be specified in one of the
        /// following formats:
        /// <list type="table">
        ///     <listheader>
        ///         <term>Time Format</term>
        ///         <description>Format Description</description>
        ///     </listheader>
        ///     <item>
        ///         <term>12-30-2000 23:59:59.033</term>
        ///         <description>Absolute date and time.</description>
        ///     </item>
        ///     <item>
        ///         <term>*</term>
        ///         <description>Evaluates to <see cref="DateTime.UtcNow"/>.</description>
        ///     </item>
        ///     <item>
        ///         <term>*-20s</term>
        ///         <description>Evaluates to 20 seconds before <see cref="DateTime.UtcNow"/>.</description>
        ///     </item>
        ///     <item>
        ///         <term>*-10m</term>
        ///         <description>Evaluates to 10 minutes before <see cref="DateTime.UtcNow"/>.</description>
        ///     </item>
        ///     <item>
        ///         <term>*-1h</term>
        ///         <description>Evaluates to 1 hour before <see cref="DateTime.UtcNow"/>.</description>
        ///     </item>
        ///     <item>
        ///         <term>*-1d</term>
        ///         <description>Evaluates to 1 day before <see cref="DateTime.UtcNow"/>.</description>
        ///     </item>
        /// </list>
        /// </para>
        /// </remarks>
        [Obsolete("Preferred method uses SubscriptionInfo object to subscribe.", false)]
        public virtual bool UnsynchronizedSubscribe(bool compactFormat, bool throttled, string filterExpression, string dataChannel, bool includeTime, double lagTime, double leadTime, bool useLocalClockAsRealTime, string startTime, string stopTime, string constraintParameters, int processingInterval, string waitHandleNames, int waitHandleTimeout)
        {
            // Dispose of any previously established local concentrator
            DisposeLocalConcentrator();

            StringBuilder connectionString = new StringBuilder();
            AssemblyInfo assemblyInfo = AssemblyInfo.ExecutingAssembly;

            connectionString.AppendFormat("trackLatestMeasurements={0}; ", throttled);
            connectionString.AppendFormat("inputMeasurementKeys={{{0}}}; ", filterExpression.ToNonNullString());
            connectionString.AppendFormat("dataChannel={{{0}}}; ", dataChannel.ToNonNullString());
            connectionString.AppendFormat("includeTime={0}; ", includeTime);
            connectionString.AppendFormat("lagTime={0}; ", lagTime);
            connectionString.AppendFormat("leadTime={0}; ", leadTime);
            connectionString.AppendFormat("useLocalClockAsRealTime={0}; ", useLocalClockAsRealTime);
            connectionString.AppendFormat("startTimeConstraint={0}; ", startTime.ToNonNullString());
            connectionString.AppendFormat("stopTimeConstraint={0}; ", stopTime.ToNonNullString());
            connectionString.AppendFormat("timeConstraintParameters={0}; ", constraintParameters.ToNonNullString());
            connectionString.AppendFormat("processingInterval={0}; ", processingInterval);
            connectionString.AppendFormat("useMillisecondResolution={0}; ", m_useMillisecondResolution);
            connectionString.AppendFormat("assemblyInfo={{source={0}; version={1}.{2}.{3}; buildDate={4}}}", assemblyInfo.Name, assemblyInfo.Version.Major, assemblyInfo.Version.Minor, assemblyInfo.Version.Build, assemblyInfo.BuildDate.ToString("yyyy-MM-dd HH:mm:ss"));

            if (!waitHandleNames.IsNullOrWhiteSpace())
            {
                connectionString.AppendFormat("; waitHandleNames={0}", waitHandleNames);
                connectionString.AppendFormat("; waitHandleTimeout={0}", waitHandleTimeout);
            }

            // Make sure not to monitor for data loss any faster than downsample time on throttled connections - additionally
            // you will want to make sure data stream monitor is twice lagtime to allow time for initial points to arrive.
            if (throttled && (object)m_dataStreamMonitor != null && m_dataStreamMonitor.Interval / 1000.0D < lagTime)
                m_dataStreamMonitor.Interval = 2.0D * lagTime * 1000.0D;

            return Subscribe(false, compactFormat, connectionString.ToString());
        }

        /// <summary>
        /// Subscribes (or re-subscribes) to a data publisher for a set of data points.
        /// </summary>
        /// <param name="remotelySynchronized">Boolean value that determines if subscription should be remotely synchronized - note that data publisher may not allow remote synchronization.</param>
        /// <param name="compactFormat">Boolean value that determines if the compact measurement format should be used. Set to <c>false</c> for full fidelity measurement serialization; otherwise set to <c>true</c> for bandwidth conservation.</param>
        /// <param name="connectionString">Connection string that defines required and optional parameters for the subscription.</param>
        /// <returns><c>true</c> if subscribe transmission was successful; otherwise <c>false</c>.</returns>
        public virtual bool Subscribe(bool remotelySynchronized, bool compactFormat, string connectionString)
        {
            bool success = false;

            if (!connectionString.IsNullOrWhiteSpace())
            {
                try
                {
                    // Parse connection string to see if it contains a data channel definition
                    Dictionary<string, string> settings = connectionString.ParseKeyValuePairs();
                    UdpClient dataChannel = null;
                    string setting;

                    // Track specified time inclusion for later deserialization
                    if (settings.TryGetValue("includeTime", out setting))
                        m_includeTime = setting.ParseBoolean();
                    else
                        m_includeTime = true;

                    settings.TryGetValue("dataChannel", out setting);

                    if (!setting.IsNullOrWhiteSpace())
                    {
                        dataChannel = new UdpClient(setting);

                        dataChannel.ReceiveBufferSize = ushort.MaxValue;
                        dataChannel.MaxConnectionAttempts = -1;
                        dataChannel.ConnectAsync();
                    }

                    // Assign data channel client reference and attach to needed events
                    this.DataChannel = dataChannel;

                    // Setup subcription packet
                    MemoryStream buffer = new MemoryStream();
                    DataPacketFlags flags = DataPacketFlags.NoFlags;
                    byte[] bytes;

                    if (remotelySynchronized)
                        flags |= DataPacketFlags.Synchronized;

                    if (compactFormat)
                        flags |= DataPacketFlags.Compact;

                    // Write data packet flags into buffer
                    buffer.WriteByte((byte)flags);

                    // Get encoded bytes of connection string
                    bytes = m_encoding.GetBytes(connectionString);

                    // Write encoded connection string length into buffer
                    buffer.Write(EndianOrder.BigEndian.GetBytes(bytes.Length), 0, 4);

                    // Encode connection string into buffer
                    buffer.Write(bytes, 0, bytes.Length);

                    // Cache subscribed synchronization state
                    m_synchronizedSubscription = remotelySynchronized;

                    // Send subscribe server command with associated command buffer
                    success = SendServerCommand(ServerCommand.Subscribe, buffer.ToArray());
                }
                catch (Exception ex)
                {
                    OnProcessException(new InvalidOperationException("Exception occurred while trying to make publisher subscription: " + ex.Message, ex));
                }
            }
            else
                OnProcessException(new InvalidOperationException("Cannot make publisher subscription without a connection string."));

            return success;
        }

        /// <summary>
        /// Unsubscribes from a data publisher.
        /// </summary>
        /// <returns><c>true</c> if unsubscribe transmission was successful; otherwise <c>false</c>.</returns>
        public virtual bool Unsubscribe()
        {
            // Send unsubscribe server command
            return SendServerCommand(ServerCommand.Unsubscribe, null);
        }

        /// <summary>
        /// Returns the measurements signal IDs that were authorized after the last successful subscription request.
        /// </summary>
        [AdapterCommand("Gets authorized signal IDs from last subscription request.")]
        public virtual Guid[] GetAuthorizedSignalIDs()
        {
            if (m_signalIndexCache != null)
                return m_signalIndexCache.AuthorizedSignalIDs;

            return new Guid[0];
        }

        /// <summary>
        /// Returns the measurements signal IDs that were unauthorized after the last successful subscription request.
        /// </summary>
        [AdapterCommand("Gets unauthorized signal IDs from last subscription request.")]
        public virtual Guid[] GetUnauthorizedSignalIDs()
        {
            if (m_signalIndexCache != null)
                return m_signalIndexCache.UnauthorizedSignalIDs;

            return new Guid[0];
        }

        /// <summary>
        /// Initiate a metadata refresh.
        /// </summary>
        [AdapterCommand("Initiates a metadata refresh.")]
        public virtual void RefreshMetadata()
        {
            SendServerCommand(ServerCommand.MetaDataRefresh);
        }
  
        public bool SendServerCommand(ServerCommand commandCode)
        {
            return SendServerCommand(commandCode, null);
        }
        
        /// <summary>
        /// Sends a server command to the publisher connection.
        /// </summary>
        /// <param name="commandCode"><see cref="ServerCommand"/> to send.</param>
        /// <param name="data">Command data to send.</param>
        /// <returns><c>true</c> if <paramref name="commandCode"/> transmission was successful; otherwise <c>false</c>.</returns>
        public virtual bool SendServerCommand(ServerCommand commandCode, byte[] data)
        {
            if (m_commandChannel != null && m_commandChannel.CurrentState == ClientState.Connected)
            {
                try
                {
                    MemoryStream commandPacket = new MemoryStream();

                    // Write command code into command packet
                    commandPacket.WriteByte((byte)commandCode);

                    // Write command buffer into command packet
                    if (data != null && data.Length > 0)
                        commandPacket.Write(data, 0, data.Length);

                    // Send command packet to publisher
                    m_commandChannel.SendAsync(commandPacket.ToArray());

                    // Track server command in pending request queue
                    lock (m_requests)
                    {
                        // Make sure a pending request does not already exist
                        int index = m_requests.BinarySearch(commandCode);

                        if (index < 0)
                        {
                            // Add the new server command to the request list
                            m_requests.Add(commandCode);

                            // Make sure requests are sorted to allow for binary searching
                            m_requests.Sort();
                        }
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    OnProcessException(new InvalidOperationException(string.Format("Exception occurred while trying to send server command \"{0}\" to publisher: {1}", commandCode, ex.Message), ex));
                }
            }
            else
                OnProcessException(new InvalidOperationException(string.Format("Subscriber is currently unconnected. Cannot send server command \"{0}\" to publisher.", commandCode)));

            return false;
        }

        /// <summary>
        /// Attempts to connect to this <see cref="DataSubscriber"/>.
        /// </summary>
        protected override void AttemptConnection()
        {
            m_commandChannelConnectionAttempts = 0;
            m_dataChannelConnectionAttempts = 0;
            m_commandChannel.ConnectAsync();
            m_authenticated = false;
            m_subscribed = false;
            m_keyIVs = null;
            m_totalBytesReceived = 0L;
            m_monitoredBytesReceived = 0L;
            m_lastBytesReceived = 0;
        }

        /// <summary>
        /// Attempts to disconnect from this <see cref="DataSubscriber"/>.
        /// </summary>
        protected override void AttemptDisconnection()
        {
            // Stop data stream monitor
            if ((object)m_dataStreamMonitor != null)
                m_dataStreamMonitor.Enabled = false;

            // Disconnect command channel
            if ((object)m_commandChannel != null)
                m_commandChannel.Disconnect();
        }

        /// <summary>
        /// Gets a short one-line status of this <see cref="DataSubscriber"/>.
        /// </summary>
        /// <param name="maxLength">Maximum length of the status message.</param>
        /// <returns>Text of the status message.</returns>
        public override string GetShortStatus(int maxLength)
        {
            if (m_commandChannel != null && m_commandChannel.CurrentState == ClientState.Connected)
                return string.Format("Subscriber is connected and receiving {0} data points", m_synchronizedSubscription ? "synchronized" : "unsynchronized").CenterText(maxLength);

            return "Subscriber is not connected.".CenterText(maxLength);
        }

        /// <summary>
        /// Get message from string based response.
        /// </summary>
        /// <param name="buffer">Response buffer.</param>
        /// <param name="startIndex">Start index of response message.</param>
        /// <param name="length">Length of response message.</param>
        /// <returns>Decoded response string.</returns>
        protected string InterpretResponseMessage(byte[] buffer, int startIndex, int length)
        {
            return m_encoding.GetString(buffer, startIndex, length);
        }

        // Restarts the subscriber.
        private void Restart()
        {
            try
            {
                base.Start();
            }
            catch (Exception ex)
            {
                OnProcessException(ex);
            }
        }

        private void ProcessServerResponse(byte[] buffer, int length)
        {
            // Currently this work is done on the async socket completion thread, make sure work to be done is timely and if the response processing
            // is coming in via the command channel and needs to send a command back to the server, it should be done on a separate thread...
            if (buffer != null && length > 0)
            {
                try
                {
                    ServerResponse responseCode = (ServerResponse)buffer[0];
                    ServerCommand commandCode = (ServerCommand)buffer[1];
                    int responseLength = EndianOrder.BigEndian.ToInt32(buffer, 2);
                    int responseIndex = 6;
                    bool solicited = false;
                    byte[][][] keyIVs;

                    // See if this was a solicited response to a requested server command
                    if (responseCode.IsSolicited())
                    {
                        lock (m_requests)
                        {
                            int index = m_requests.BinarySearch(commandCode);

                            if (index >= 0)
                            {
                                solicited = true;
                                m_requests.RemoveAt(index);
                            }
                        }

                        // Disconnect any established UDP data channel upon successful unsubscribe
                        if (solicited && commandCode == ServerCommand.Unsubscribe && responseCode == ServerResponse.Succeeded)
                            DataChannel = null;
                    }

                    switch (responseCode)
                    {
                        case ServerResponse.Succeeded:
                            if (solicited)
                            {
                                switch (commandCode)
                                {
                                    case ServerCommand.Authenticate:
                                        OnStatusMessage("Success code received in response to server command \"{0}\": {1}", commandCode, InterpretResponseMessage(buffer, responseIndex, responseLength));
                                        m_authenticated = true;
                                        OnConnectionAuthenticated();
                                        break;
                                    case ServerCommand.Subscribe:
                                        OnStatusMessage("Success code received in response to server command \"{0}\": {1}", commandCode, InterpretResponseMessage(buffer, responseIndex, responseLength));
                                        m_subscribed = true;
                                        if ((object)m_dataStreamMonitor != null)
                                            m_dataStreamMonitor.Enabled = true;
                                        break;
                                    case ServerCommand.Unsubscribe:
                                        OnStatusMessage("Success code received in response to server command \"{0}\": {1}", commandCode, InterpretResponseMessage(buffer, responseIndex, responseLength));
                                        m_subscribed = false;
                                        if ((object)m_dataStreamMonitor != null)
                                            m_dataStreamMonitor.Enabled = false;
                                        break;
                                    case ServerCommand.RotateCipherKeys:
                                        OnStatusMessage("Success code received in response to server command \"{0}\": {1}", commandCode, InterpretResponseMessage(buffer, responseIndex, responseLength));
                                        break;
                                    case ServerCommand.MetaDataRefresh:
                                        OnStatusMessage("Success code received in response to server command \"{0}\": latest meta-data received.", commandCode);
                                        OnMetaDataReceived(DeserializeMetadata(buffer.BlockCopy(responseIndex, responseLength)));
                                        break;
                                }
                            }
                            else
                            {
                                switch (commandCode)
                                {
                                    case ServerCommand.MetaDataRefresh:
                                        // Meta-data refresh may be unsolicited
                                        OnStatusMessage("Received server confirmation for unsolicited request to \"{0}\" command: latest meta-data received.", commandCode);
                                        OnMetaDataReceived(DeserializeMetadata(buffer.BlockCopy(responseIndex, responseLength)));
                                        break;
                                    case ServerCommand.RotateCipherKeys:
                                        // Key rotation may be unsolicited
                                        OnStatusMessage("Received server confirmation for unsolicited request to \"{0}\" command: {1}", commandCode, InterpretResponseMessage(buffer, responseIndex, responseLength));
                                        break;
                                    case ServerCommand.Subscribe:
                                        OnStatusMessage("Received unsolicited response to \"{0}\" command: {1}", commandCode, InterpretResponseMessage(buffer, responseIndex, responseLength));
                                        break;
                                    default:
                                        OnProcessException(new InvalidOperationException("Publisher sent a success code for an unsolicited server command: " + commandCode));
                                        break;
                                }
                            }
                            break;
                        case ServerResponse.Failed:
                            if (solicited)
                                OnStatusMessage("Failure code received in response to server command \"{0}\": {1}", commandCode, InterpretResponseMessage(buffer, responseIndex, responseLength));
                            else
                                OnProcessException(new InvalidOperationException("Publisher sent a failed code for an unsolicited server command: " + commandCode));
                            break;
                        case ServerResponse.DataPacket:
                            long now = DateTime.UtcNow.Ticks;

                            // Deserialize data packet
                            List<IMeasurement> measurements = new List<IMeasurement>();
                            DataPacketFlags flags;
                            Ticks timestamp = 0;
                            int count;

                            // Track total data packet bytes received from any channel
                            m_totalBytesReceived += m_lastBytesReceived;
                            m_monitoredBytesReceived += m_lastBytesReceived;

                            // Get data packet flags
                            flags = (DataPacketFlags)buffer[responseIndex];
                            responseIndex++;

                            bool synchronizedMeasurements = ((byte)(flags & DataPacketFlags.Synchronized) > 0);
                            bool compactMeasurementFormat = ((byte)(flags & DataPacketFlags.Compact) > 0);
                            int cipherIndex = (flags & DataPacketFlags.CipherIndex) > 0 ? 1 : 0;

                            // Decrypt data packet payload if keys are available
//                            if (m_keyIVs != null)
//                            {
//                                // Get a local copy of volatile keyIVs reference since this can change at any time
//                                keyIVs = m_keyIVs;
//
//                                // Decrypt payload portion of data packet
//                                buffer = Common.SymmetricAlgorithm.Decrypt(buffer, responseIndex, responseLength - 1, keyIVs[cipherIndex][0], keyIVs[cipherIndex][1]);
//                                responseIndex = 0;
//                                responseLength = buffer.Length;
//                            }

                            // Synchronized packets contain a frame level timestamp
                            if (synchronizedMeasurements)
                            {
                                timestamp = EndianOrder.BigEndian.ToInt64(buffer, responseIndex);
                                responseIndex += 8;
                            }

                            // Deserialize number of measurements that follow
                            count = EndianOrder.BigEndian.ToInt32(buffer, responseIndex);
                            responseIndex += 4;

                            // Deserialize measurements
                            for (int i = 0; i < count; i++)
                            {
                                if (!compactMeasurementFormat)
                                {
                                    // Deserialize full measurement format
                                    SerializableMeasurement measurement = new SerializableMeasurement(m_encoding);
                                    responseIndex += measurement.ParseBinaryImage(buffer, responseIndex, responseLength - responseIndex);
                                    measurements.Add(measurement);
                                }
                                else if ((object)m_signalIndexCache != null)
                                {
                                    // Deserialize compact measurement format
                                    CompactMeasurement measurement = new CompactMeasurement(m_signalIndexCache, m_includeTime, m_baseTimeOffsets, m_timeIndex, m_useMillisecondResolution);
                                    responseIndex += measurement.ParseBinaryImage(buffer, responseIndex, responseLength - responseIndex);

                                    // Apply timestamp from frame if not included in transmission
                                    if (!measurement.IncludeTime)
                                        measurement.Timestamp = timestamp;

                                    measurements.Add(measurement);
                                }
                                else if (m_lastMissingCacheWarning + MissingCacheWarningInterval < now)
                                {
                                    if (m_lastMissingCacheWarning != 0L)
                                    {
                                        // Warning message for missing signal index cache
                                        OnStatusMessage("WARNING: Signal index cache has not arrived. No compact measurements can be parsed.");
                                    }

                                    m_lastMissingCacheWarning = now;
                                }
                            }

                            // Provide new measurements to local concentrator, if defined, otherwise directly expose them to the consumer
                            if ((object)m_localConcentrator != null)
                                m_localConcentrator.SortMeasurements(measurements);
                            else
                                OnNewMeasurements(measurements);
                            break;
                        case ServerResponse.DataStartTime:
                            // Raise data start time event
                            OnDataStartTime(EndianOrder.BigEndian.ToInt64(buffer, responseIndex));
                            break;
                        case ServerResponse.ProcessingComplete:
                            // Raise input processing completed event
                            OnProcessingComplete(InterpretResponseMessage(buffer, responseIndex, responseLength));
                            break;
                        case ServerResponse.UpdateSignalIndexCache:
                            // Deserialize new signal index cache
                            m_remoteSignalIndexCache = DeserializeSignalIndexCache(buffer.BlockCopy(responseIndex, responseLength));
                            m_signalIndexCache = new SignalIndexCache(DataSource, m_remoteSignalIndexCache);
                            break;
                        case ServerResponse.UpdateBaseTimes:
                            // Get active time index
                            m_timeIndex = EndianOrder.BigEndian.ToInt32(buffer, responseIndex);
                            responseIndex += 4;

                            // Deserialize new base time offsets
                            m_baseTimeOffsets = new long[] { EndianOrder.BigEndian.ToInt64(buffer, responseIndex), EndianOrder.BigEndian.ToInt64(buffer, responseIndex + 8) };
                            break;
                        case ServerResponse.UpdateCipherKeys:
                            // Get active cipher index
                            m_cipherIndex = buffer[responseIndex++];

                            // Extract remaining response
                            byte[] bytes = buffer.BlockCopy(responseIndex, responseLength - 1);

//                            // Decrypt response payload if subscription is authenticated
//                            if (m_authenticated)
//                                bytes = bytes.Decrypt(m_sharedSecret, CipherStrength.Aes256);

                            // Deserialize new cipher keys
                            keyIVs = new byte[2][][];
                            keyIVs[EvenKey] = new byte[2][];
                            keyIVs[OddKey] = new byte[2][];

                            int index = 0;
                            int bufferLen;

                            // Read even key size
                            bufferLen = EndianOrder.BigEndian.ToInt32(bytes, index);
                            index += 4;

                            // Read even key
                            keyIVs[EvenKey][KeyIndex] = new byte[bufferLen];
                            Buffer.BlockCopy(bytes, index, keyIVs[EvenKey][KeyIndex], 0, bufferLen);
                            index += bufferLen;

                            // Read even initialization vector size
                            bufferLen = EndianOrder.BigEndian.ToInt32(bytes, index);
                            index += 4;

                            // Read even initialization vector
                            keyIVs[EvenKey][IVIndex] = new byte[bufferLen];
                            Buffer.BlockCopy(bytes, index, keyIVs[EvenKey][IVIndex], 0, bufferLen);
                            index += bufferLen;

                            // Read odd key size
                            bufferLen = EndianOrder.BigEndian.ToInt32(bytes, index);
                            index += 4;

                            // Read odd key
                            keyIVs[OddKey][KeyIndex] = new byte[bufferLen];
                            Buffer.BlockCopy(bytes, index, keyIVs[OddKey][KeyIndex], 0, bufferLen);
                            index += bufferLen;

                            // Read odd initialization vector size
                            bufferLen = EndianOrder.BigEndian.ToInt32(bytes, index);
                            index += 4;

                            // Read odd initialization vector
                            keyIVs[OddKey][IVIndex] = new byte[bufferLen];
                            Buffer.BlockCopy(bytes, index, keyIVs[OddKey][IVIndex], 0, bufferLen);
                            index += bufferLen;

                            // Exchange keys
                            m_keyIVs = keyIVs;

                            OnStatusMessage("Successfully established new cipher keys for data packet transmissions.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    OnProcessException(new InvalidOperationException("Failed to process publisher response packet due to exception: " + ex.Message, ex));
                }
            }
        }

        // Handles auto-connection subscription initialization
        private void StartSubscription()
        {
            StringBuilder filterExpression = new StringBuilder();
            string dataChannel = null;

            // If TCP command channel is defined separately, then base connection string defines data channel
            if (Settings.ContainsKey("commandChannel"))
                dataChannel = ConnectionString;

            if (OutputMeasurements != null)
            {
                foreach (IMeasurement measurement in OutputMeasurements)
                {
                    if (filterExpression.Length > 0)
                        filterExpression.Append(';');

                    // Subscribe by associated Guid...
                    filterExpression.Append(measurement.ID.ToString());
                }

                // Start unsynchronized subscription
                UnsynchronizedSubscribe(true, false, filterExpression.ToString(), dataChannel, true, 10.0D, 5.0D, false, null, null, null, -1, null, 0);
            }
            else
            {
                OnStatusMessage("WARNING: No measurements are currently defined for subscription.");
            }

            // Initiate meta-data refresh
            if (m_synchronizeMetadata)
                SendServerCommand(ServerCommand.MetaDataRefresh);
        }

        /// <summary>
        /// Handles auto-connection metadata synchronization to local system. 
        /// </summary>
        /// <param name="state"><see cref="DataSet"/> metadata collection passed into state parameter.</param>
        /// <remarks>
        /// This function is normally called from thread pool since synchronization can take some time.
        /// </remarks>
        protected virtual void SynchronizeMetadata(object state)
        {
//            try
//            {
//                DataSet metadata = state as DataSet;
//
//                if (metadata != null)
//                {
//                    // Track total meta-data synchronization process time
//                    Ticks startTime = DateTime.UtcNow.Ticks;
//
//                    // Open the configuration database using settings found in the config file
//                    AdoDataConnection database = new AdoDataConnection("systemSettings");
//                    IDbConnection connection = database.Connection;
//                    string guidPrefix = database.DatabaseType == DatabaseType.Access ? "{" : "'";
//                    string guidSuffix = database.DatabaseType == DatabaseType.Access ? "}" : "'";
//
//                    // Query the actual record ID based on the known run-time ID for this subscriber device
//                    int parentID = Convert.ToInt32(connection.ExecuteScalar(string.Format("SELECT SourceID FROM Runtime WHERE ID = {0} AND SourceTable='Device'", ID)));
//
//                    // Validate that the subscriber device is marked as a concentrator (we are about to associate children devices with it)
//                    if (!connection.ExecuteScalar(string.Format("SELECT IsConcentrator FROM Device WHERE ID = {0}", parentID)).ToString().ParseBoolean())
//                        connection.ExecuteNonQuery(string.Format("UPDATE Device SET IsConcentrator = 1 WHERE ID = {0}", parentID));
//
//                    // Determine the active node ID - we cache this since this value won't change for the lifetime of this class
//                    if (m_nodeID == Guid.Empty)
//                        m_nodeID = new Guid(connection.ExecuteScalar(string.Format("SELECT NodeID FROM IaonInputAdapter WHERE ID = {0}", ID)).ToString());
//
//                    // Determine the protocol record auto-inc ID value for the gateway transport protocol (GEP) - this value is also cached since it shouldn't change for the lifetime of this class
//                    if (m_gatewayProtocolID == 0)
//                        m_gatewayProtocolID = int.Parse(connection.ExecuteScalar("SELECT ID FROM Protocol WHERE Acronym='GatewayTransport'").ToString());
//
//                    // Prefix all children devices with the name of the parent since the same device names could appear in different connections (helps keep device names unique)
//                    string sourcePrefix = Name + "!";
//                    Dictionary<string, int> deviceIDs = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
//                    string selectSql, insertSql, updateSql, deleteSql, deviceAcronym, signalTypeAcronym;
//                    int deviceID;
//
//                    // Check to see if data for the "DeviceDetail" table was included in the meta-data
//                    if (metadata.Tables.Contains("DeviceDetail"))
//                    {
//                        List<Guid> uniqueIDs = new List<Guid>();
//
//                        foreach (DataRow row in metadata.Tables["DeviceDetail"].Rows)
//                        {
//                            Guid uniqueID = new Guid(row.Field<object>("UniqueID").ToString()); // adoDatabase.Guid(row, "UniqueID"); // row.Field<Guid>("UniqueID");
//
//                            // Track unique device guid's in this meta-data session, we'll need to remove any old associated devices that no longer exist
//                            uniqueIDs.Add(uniqueID);
//
//                            // We will synchronize metadata only if the source owns this device and it's not defined as a concentrator (these should normally be filtered by publisher - but we check just in case).
//                            if (row.Field<object>("OriginalSource") == null && !row["IsConcentrator"].ToNonNullString("0").ParseBoolean())
//                            {
//                                // Define query to determine if this device is already defined (this should always be based on the unique device Guid)
//                                selectSql = database.ParameterizedQueryString("SELECT COUNT(*) FROM Device WHERE UniqueID = {0}", "deviceGuid");
//
//                                if (Convert.ToInt32(connection.ExecuteScalar(selectSql, database.Guid(uniqueID))) == 0)
//                                {
//                                    // Insert new device record
//                                    insertSql = database.ParameterizedQueryString("INSERT INTO Device(NodeID, ParentID, Acronym, Name, ProtocolID, IsConcentrator, Enabled, OriginalSource) " +
//                                        "VALUES ({0}, {1}, {2}, {3}, {4}, 0, 1, {5})", "nodeID", "parentID", "acronym", "name", "protocolID", "originalSource");
//
//                                    connection.ExecuteNonQuery(insertSql, database.Guid(m_nodeID), parentID, sourcePrefix + row.Field<string>("Acronym"), row.Field<string>("Name"), m_gatewayProtocolID,
//                                        m_internal == true ? (object)DBNull.Value : string.IsNullOrEmpty(row.Field<string>("ParentAcronym")) ? sourcePrefix + row.Field<string>("Acronym") : sourcePrefix + row.Field<string>("ParentAcronym"));
//
//                                    // Guids are normally auto-generated during insert - after insertion update the guid so that it matches the source data. Most of the database
//                                    // scripts have triggers that support properly assigning the Guid during an insert, but this code ensures the Guid will always get assigned.
//                                    updateSql = database.ParameterizedQueryString("UPDATE Device SET UniqueID = {0} WHERE Acronym = {1}", "uniqueID", "acronym");
//                                    connection.ExecuteNonQuery(updateSql, database.Guid(uniqueID), sourcePrefix + row.Field<string>("Acronym"));
//                                }
//                                else
//                                {
//                                    // Update existing device record
//                                    if (m_internal)
//                                    {
//                                        // Gateway is assuming ownership of the device records when the "interal" flag is true - this means the device's measurements can be forwarded to another party.
//                                        // From a device record perspective, ownership is inferred by setting 'OriginalSource' to null.
//                                        updateSql = database.ParameterizedQueryString("UPDATE Device SET Acronym = {0}, Name = {1}, OriginalSource = {2}, ProtocolID = {3} WHERE UniqueID = {4}", "acronym", "name", "originalSource", "protocolID", "uniqueID");
//                                        connection.ExecuteNonQuery(updateSql, sourcePrefix + row.Field<string>("Acronym"), row.Field<string>("Name"), (object)DBNull.Value, m_gatewayProtocolID, database.Guid(uniqueID));
//                                    }
//                                    else
//                                    {
//                                        // When gateway doesn't own device records (i.e., the "interal" flag is false), this means the device's measurements can only be consumed locally. From a device
//                                        // record perspective this means the 'OriginalSource' field is set to the acronym of the PDC or PMU that generated the source measurments. This field allows a
//                                        // mirrored source restriction to be implemented later to ensure all devices in an output protocol came from the same original source connection.
//                                        updateSql = database.ParameterizedQueryString("UPDATE Device SET Acronym = {0}, Name = {1}, ProtocolID = {2} WHERE UniqueID = {3}", "acronym", "name", "protocolID", "uniqueID");
//                                        connection.ExecuteNonQuery(updateSql, sourcePrefix + row.Field<string>("Acronym"), row.Field<string>("Name"), m_gatewayProtocolID, database.Guid(uniqueID));
//                                    }
//                                }
//                            }
//
//                            // Capture local device ID auto-inc value for measurement association
//                            selectSql = database.ParameterizedQueryString("SELECT ID FROM Device WHERE UniqueID = {0}", "deviceGuid");
//                            deviceIDs[row.Field<string>("Acronym")] = Convert.ToInt32(connection.ExecuteScalar(selectSql, database.Guid(uniqueID)));
//                        }
//
//                        // Remove any device records associated with this subscriber that no longer exist in the meta-data
//                        if (uniqueIDs.Count > 0)
//                        {
//                            deleteSql = string.Format("DELETE FROM Device WHERE ParentID = {0} AND UniqueID NOT IN ({1})", parentID, uniqueIDs.Select(uniqueID => guidPrefix + uniqueID.ToString().ToLower() + guidSuffix).ToDelimitedString(", "));
//                            connection.ExecuteNonQuery(deleteSql);
//                        }
//                    }
//
//                    // Check to see if data for the "MeasurementDetail" table was included in the meta-data
//                    if (metadata.Tables.Contains("MeasurementDetail"))
//                    {
//                        List<Guid> signalIDs = new List<Guid>();
//
//                        // Load signal type ID's from local database associated with their acronym for proper signal type translation
//                        Dictionary<string, int> signalTypeIDs = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
//
//                        foreach (DataRow row in connection.RetrieveData(database.AdapterType, "SELECT ID, Acronym FROM SignalType").Rows)
//                        {
//                            signalTypeAcronym = row.Field<string>("Acronym");
//
//                            if (!signalTypeAcronym.IsNullOrWhiteSpace())
//                                signalTypeIDs[signalTypeAcronym] = row.ConvertField<int>("ID");
//                        }
//
//                        foreach (DataRow row in metadata.Tables["MeasurementDetail"].Rows)
//                        {
//                            // Verify that this measurement record is internal to the publisher (these should be filtered by the publisher, but we still check since we won't be able to subscribe to any measurements it doesn't own)
//                            if (row["Internal"].ToNonNullString("1").ParseBoolean())
//                            {
//                                // Get device and signal type acronyms
//                                deviceAcronym = row.Field<string>("DeviceAcronym") ?? string.Empty;
//                                signalTypeAcronym = row.Field<string>("SignalAcronym") ?? string.Empty;
//
//                                // Make sure we have an associated device and signal type already defined for the measurement
//                                if (!deviceAcronym.IsNullOrWhiteSpace() && deviceIDs.ContainsKey(deviceAcronym) && !signalTypeAcronym.IsNullOrWhiteSpace() && signalTypeIDs.ContainsKey(signalTypeAcronym))
//                                {
//                                    // Prefix the tag name with the "updated" device name
//                                    deviceID = deviceIDs[deviceAcronym];
//                                    string pointTag = sourcePrefix + row.Field<string>("PointTag") ?? string.Empty;
//                                    Guid signalID = new Guid(row.Field<object>("SignalID").ToString()); // adoDatabase.Guid(row, "SignalID");  // row.Field<Guid>("SignalID");
//
//                                    // Track unique measurement signal guid's in this meta-data session, we'll need to remove any old associated measurements that no longer exist
//                                    signalIDs.Add(signalID);
//
//                                    // Define query to determine if this measurement is already defined (this should always be based on the unique signal ID Guid)
//                                    selectSql = database.ParameterizedQueryString("SELECT COUNT(*) FROM Measurement WHERE SignalID = {0}", "signalID");
//
//                                    if (Convert.ToInt32(connection.ExecuteScalar(selectSql, database.Guid(signalID))) == 0)
//                                    {
//                                        // Insert new measurement record
//                                        insertSql = database.ParameterizedQueryString("INSERT INTO Measurement (DeviceID, PointTag, SignalTypeID, SignalReference, Description, Internal, Subscribed, Enabled) VALUES ({0}, {1}, {2}, {3}, {4}, {5}, 0, 1)", "deviceID", "pointTag", "signalTypeID", "signalReference", "description", "internal");
//                                        connection.ExecuteNonQuery(insertSql, 30, deviceID, pointTag, signalTypeIDs[signalTypeAcronym], sourcePrefix + row.Field<string>("SignalReference"), row.Field<string>("Description") ?? string.Empty, database.Bool(m_internal));
//
//                                        // Guids are normally auto-generated during insert - after insertion update the guid so that it matches the source data. Most of the database
//                                        // scripts have triggers that support properly assigning the Guid during an insert, but this code ensures the Guid will always get assigned.
//                                        updateSql = database.ParameterizedQueryString("UPDATE Measurement SET SignalID = {0} WHERE PointTag = {1}", "signalID", "pointTag");
//                                        connection.ExecuteNonQuery(updateSql, database.Guid(signalID), pointTag);
//                                    }
//                                    else
//                                    {
//                                        // Update existing measurement record. Note that this update assumes that measurements will remain associated with a static source device.
//                                        updateSql = database.ParameterizedQueryString("UPDATE Measurement SET PointTag = {0}, SignalTypeID = {1}, SignalReference = {2}, Description = {3}, Internal = {4} WHERE SignalID = {5}", "pointTag", "signalTypeID", "signalReference", "description", "internal", "signalID");
//                                        connection.ExecuteNonQuery(updateSql, pointTag, signalTypeIDs[signalTypeAcronym], sourcePrefix + row.Field<string>("SignalReference"), row.Field<string>("Description") ?? string.Empty, database.Bool(m_internal), database.Guid(signalID));
//                                    }
//                                }
//                            }
//                        }
//
//                        // Remove any measurement records associated with existing devices in this session but no longer exist in the meta-data
//                        if (deviceIDs.Count > 0 && signalIDs.Count > 0)
//                        {
//                            deleteSql = string.Format("DELETE FROM Measurement WHERE DeviceID IN ({0}) AND SignalID NOT IN ({1})", deviceIDs.Values.ToDelimitedString(", "), signalIDs.Select(uniqueID => guidPrefix + uniqueID.ToString() + guidSuffix).ToDelimitedString(", "));
//                            connection.ExecuteNonQuery(deleteSql);
//                        }
//                    }
//
//                    // Check to see if data for the "PhasorDetail" table was included in the meta-data
//                    if (metadata.Tables.Contains("PhasorDetail"))
//                    {
//                        Dictionary<int, int> maxSourceIndicies = new Dictionary<int, int>();
//                        int sourceIndex;
//
//                        // Phasor data is normally only needed so that the user can property generate a mirrored IEEE C37.118 output stream from the source data.
//                        // This is necessary since, in this protocol, the phasors are described (i.e., labeled) as a unit (i.e., as a complex number) instead of
//                        // as two distinct angle and magnitude measurements.
//
//                        foreach (DataRow row in metadata.Tables["PhasorDetail"].Rows)
//                        {
//                            // Get device acronym
//                            deviceAcronym = row.Field<string>("DeviceAcronym") ?? string.Empty;
//
//                            // Make sure we have an associated device already defined for the phasor record
//                            if (!deviceAcronym.IsNullOrWhiteSpace() && deviceIDs.ContainsKey(deviceAcronym))
//                            {
//                                deviceID = deviceIDs[deviceAcronym];
//
//                                // Define query to determine if this phasor record is already defined, this is no Guid for these simple label records
//                                selectSql = database.ParameterizedQueryString("SELECT COUNT(*) FROM Phasor WHERE DeviceID = {0} AND SourceIndex = {1}", "deviceID", "sourceIndex");
//
//                                if (Convert.ToInt32(connection.ExecuteScalar(selectSql, 30, deviceID, row.ConvertField<int>("SourceIndex"))) == 0)
//                                {
//                                    // Insert new phasor record
//                                    insertSql = database.ParameterizedQueryString("INSERT INTO Phasor (DeviceID, Label, Type, Phase, SourceIndex) VALUES ({0}, {1}, {2}, {3}, {4})", "deviceID", "label", "type", "phase", "sourceIndex");
//                                    connection.ExecuteNonQuery(insertSql, 30, deviceID, row.Field<string>("Label") ?? "undefined", (row.Field<string>("Type") ?? "V").TruncateLeft(1), (row.Field<string>("Phase") ?? "+").TruncateLeft(1), row.ConvertField<int>("SourceIndex"));
//                                }
//                                else
//                                {
//                                    // Update existing phasor record
//                                    updateSql = database.ParameterizedQueryString("UPDATE Phasor SET Label = {0}, Type = {1}, Phase = {2} WHERE DeviceID = {3} AND SourceIndex = {4}", "label", "type", "phase", "deviceID", "sourceIndex");
//                                    connection.ExecuteNonQuery(updateSql, row.Field<string>("Label") ?? "undefined", (row.Field<string>("Type") ?? "V").TruncateLeft(1), (row.Field<string>("Phase") ?? "+").TruncateLeft(1), deviceID, row.ConvertField<int>("SourceIndex"));
//                                }
//
//                                // Track largest source index for each device
//                                maxSourceIndicies.TryGetValue(deviceID, out sourceIndex);
//
//                                if (row.ConvertField<int>("SourceIndex") > sourceIndex)
//                                    maxSourceIndicies[deviceID] = row.ConvertField<int>("SourceIndex");
//                            }
//                        }
//
//                        // Remove any phasor records associated with existing devices in this session but no longer exist in the meta-data
//                        if (maxSourceIndicies.Count > 0)
//                        {
//                            foreach (KeyValuePair<int, int> deviceIndexPair in maxSourceIndicies)
//                            {
//                                deleteSql = string.Format("DELETE FROM Phasor WHERE DeviceID = {0} AND SourceIndex > {1}", deviceIndexPair.Key, deviceIndexPair.Value);
//                                connection.ExecuteNonQuery(deleteSql);
//                            }
//                        }
//                    }
//
//                    // New signals may have been defined, take original remote signal index cache and apply changes
//                    if (m_remoteSignalIndexCache != null)
//                        m_signalIndexCache = new SignalIndexCache(DataSource, m_remoteSignalIndexCache);
//
//                    OnStatusMessage("Meta-data synchronization completed successfully in {0}", (DateTime.UtcNow.Ticks - startTime).ToElapsedTimeString(3));
//                }
//                else
//                {
//                    OnStatusMessage("WARNING: Meta-data synchronization was not performed, deserialized dataset was empty.");
//                }
//            }
//            catch (Exception ex)
//            {
//                OnProcessException(new InvalidOperationException("Failed to synchronize meta-data to local cache: " + ex.Message, ex));
//            }
        }

        private SignalIndexCache DeserializeSignalIndexCache(byte[] buffer)
        {
            GatewayCompressionMode gatewayCompressionMode = (GatewayCompressionMode)(m_operationalModes & OperationalModes.CompressionModeMask);
            bool useCommonSerializationFormat = (m_operationalModes & OperationalModes.UseCommonSerializationFormat) > 0;
            bool compressSignalIndexCache = (m_operationalModes & OperationalModes.CompressSignalIndexCache) > 0;

            SignalIndexCache deserializedCache;

            MemoryStream compressedData = null;
            GZipStream inflater = null;

            if (compressSignalIndexCache && gatewayCompressionMode == GatewayCompressionMode.GZip)
            {
                try
                {
                    compressedData = new MemoryStream(buffer);
                    inflater = new GZipStream(compressedData, CompressionMode.Decompress);
                    buffer = inflater.ReadStream();
                }
                finally
                {
                    if ((object)inflater != null)
                        inflater.Close();

                    if ((object)compressedData != null)
                        compressedData.Close();
                }
            }

            if (useCommonSerializationFormat)
            {
                deserializedCache = new SignalIndexCache();
                deserializedCache.Encoding = m_encoding;
                deserializedCache.ParseBinaryImage(buffer, 0, buffer.Length);
            }
            else
            {
                deserializedCache = Serialization.Deserialize<SignalIndexCache>(buffer, GSF.SerializationFormat.Binary);
            }

            return deserializedCache;
        }

        private DataSet DeserializeMetadata(byte[] buffer)
        {
            GatewayCompressionMode gatewayCompressionMode = (GatewayCompressionMode)(m_operationalModes & OperationalModes.CompressionModeMask);
            bool useCommonSerializationFormat = (m_operationalModes & OperationalModes.UseCommonSerializationFormat) > 0;
            bool compressMetadata = (m_operationalModes & OperationalModes.CompressMetadata) > 0;

            DataSet deserializedMetadata;

            MemoryStream compressedData = null;
            GZipStream inflater = null;

            MemoryStream encodedData = null;
            XmlTextReader unicodeReader = null;

            if (compressMetadata && gatewayCompressionMode == GatewayCompressionMode.GZip)
            {
                try
                {
                    // Insert compressed data into compressed buffer
                    compressedData = new MemoryStream(buffer);
                    inflater = new GZipStream(compressedData, CompressionMode.Decompress);
                    buffer = inflater.ReadStream();
                }
                finally
                {
                    if ((object)inflater != null)
                        inflater.Close();

                    if ((object)compressedData != null)
                        compressedData.Close();
                }
            }

            if (useCommonSerializationFormat)
            {
                try
                {
                    // Copy decompressed data into encoded buffer
                    encodedData = new MemoryStream(buffer);

                    // Read encoded data into data set as XML
                    unicodeReader = new XmlTextReader(encodedData);
                    deserializedMetadata = new DataSet();
                    deserializedMetadata.ReadXml(unicodeReader, XmlReadMode.ReadSchema);
                }
                finally
                {
                    if ((object)unicodeReader != null)
                        unicodeReader.Close();

                    if ((object)encodedData != null)
                        encodedData.Close();
                }
            }
            else
            {
                deserializedMetadata = Serialization.Deserialize<DataSet>(buffer, GSF.SerializationFormat.Binary);
            }

            return deserializedMetadata;
        }

        private UnsynchronizedSubscriptionInfo FromLocallySynchronizedInfo(SynchronizedSubscriptionInfo info)
        {
            return new UnsynchronizedSubscriptionInfo(false)
            {
                FilterExpression = info.FilterExpression,
                UseCompactMeasurementFormat = info.UseCompactMeasurementFormat,
                UdpDataChannel = info.UdpDataChannel,
                DataChannelLocalPort = info.DataChannelLocalPort,
                LagTime = info.LagTime,
                LeadTime = info.LeadTime,
                UseLocalClockAsRealTime = false,
                UseMillisecondResolution = info.UseMillisecondResolution,
                StartTime = info.StartTime,
                StopTime = info.StopTime,
                ConstraintParameters = info.ConstraintParameters,
                ProcessingInterval = info.ProcessingInterval,
                WaitHandleNames = info.WaitHandleNames,
                WaitHandleTimeout = info.WaitHandleTimeout,
                ExtraConnectionStringParameters = info.ExtraConnectionStringParameters
            };
        }

        private Encoding GetCharacterEncoding(OperationalEncoding operationalEncoding)
        {
            Encoding encoding;

            switch (operationalEncoding)
            {
                case OperationalEncoding.Unicode:
                    encoding = Encoding.Unicode;
                    break;
                case OperationalEncoding.BigEndianUnicode:
                    encoding = Encoding.BigEndianUnicode;
                    break;
                case OperationalEncoding.UTF8:
                    encoding = Encoding.UTF8;
                    break;
                case OperationalEncoding.ANSI:
                    encoding = Encoding.Default;
                    break;
                default:
                    throw new InvalidOperationException(string.Format("Unsupported encoding detected: {0}", operationalEncoding));
            }

            return encoding;
        }

        // Socket exception handler
        private bool HandleSocketException(System.Net.Sockets.SocketException ex)
        {
            if ((object)ex != null)
            {
                // WSAECONNABORTED and WSAECONNRESET are common errors after a client disconnect,
                // if they happen for other reasons, make sure disconnect procedure is handled
                if (ex.ErrorCode == 10053 || ex.ErrorCode == 10054)
                {
                    DisconnectClient();
                    return true;
                }
            }

            return false;
        }

        // Disconnect client, restarting if disconnect was not intentional
        private void DisconnectClient()
        {
            DataChannel = null;

            // If user didn't initiate disconnect, restart the connection
            if (Enabled)
                Start();
        }

        // This method is called when connection has been authenticated
        private void DataSubscriber_ConnectionAuthenticated(object sender, EventArgs e)
        {
            if (m_autoConnect)
                StartSubscription();
        }

        // This method is called then new metadata has been received
        private void DataSubscriber_MetaDataReceived(object sender, EventArgs<DataSet> e)
        {
            try
            {
                // We handle synchronization on a seperate thread since this process may be lengthy
                if (m_synchronizeMetadata)
                    ThreadPool.QueueUserWorkItem(SynchronizeMetadata, e.Argument);
            }
            catch (Exception ex)
            {
                // Process exception for logging
                OnProcessException(new InvalidOperationException("Failed to queue metadata synchronization due to exception: " + ex.Message, ex));
            }
        }

        /// <summary>
        /// Raises the <see cref="ConnectionEstablished"/> event.
        /// </summary>
        protected void OnConnectionEstablished()
        {
            try
            {
                if (ConnectionEstablished != null)
                    ConnectionEstablished(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                // We protect our code from consumer thrown exceptions
                OnProcessException(new InvalidOperationException(string.Format("Exception in consumer handler for ConnectionEstablished event: {0}", ex.Message), ex));
            }
        }

        /// <summary>
        /// Raises the <see cref="ConnectionTerminated"/> event.
        /// </summary>
        protected void OnConnectionTerminated()
        {
            try
            {
                if (ConnectionTerminated != null)
                    ConnectionTerminated(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                // We protect our code from consumer thrown exceptions
                OnProcessException(new InvalidOperationException(string.Format("Exception in consumer handler for ConnectionTerminated event: {0}", ex.Message), ex));
            }
        }

        /// <summary>
        /// Raises the <see cref="ConnectionAuthenticated"/> event.
        /// </summary>
        protected void OnConnectionAuthenticated()
        {
            try
            {
                if (ConnectionAuthenticated != null)
                    ConnectionAuthenticated(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                // We protect our code from consumer thrown exceptions
                OnProcessException(new InvalidOperationException(string.Format("Exception in consumer handler for ConnectionAuthenticated event: {0}", ex.Message), ex));
            }
        }

        /// <summary>
        /// Raises the <see cref="MetaDataReceived"/> event.
        /// </summary>
        /// <param name="metadata">Meta-data <see cref="DataSet"/> instance to send to client subscription.</param>
        protected void OnMetaDataReceived(DataSet metadata)
        {
            try
            {
                if (MetaDataReceived != null)
                    MetaDataReceived(this, new EventArgs<DataSet>(metadata));
            }
            catch (Exception ex)
            {
                // We protect our code from consumer thrown exceptions
                OnProcessException(new InvalidOperationException(string.Format("Exception in consumer handler for MetaDataReceived event: {0}", ex.Message), ex));
            }
        }

        /// <summary>
        /// Raises the <see cref="DataStartTime"/> event.
        /// </summary>
        /// <param name="startTime">Start time, in <see cref="Ticks"/>, of first measurement transmitted.</param>
        protected void OnDataStartTime(Ticks startTime)
        {
            try
            {
                if (DataStartTime != null)
                    DataStartTime(this, new EventArgs<Ticks>(startTime));
            }
            catch (Exception ex)
            {
                // We protect our code from consumer thrown exceptions
                OnProcessException(new InvalidOperationException(string.Format("Exception in consumer handler for DataStartTime event: {0}", ex.Message), ex));
            }
        }

        /// <summary>
        /// Raises the <see cref="ProcessingComplete"/> event.
        /// </summary>
        /// <param name="source">Type name of adapter that sent the processing completed notification.</param>
        protected void OnProcessingComplete(string source)
        {
            try
            {
                if (ProcessingComplete != null)
                    ProcessingComplete(this, new EventArgs<string>(source));

                // Also raise base class event in case this event has been subscribed
                OnProcessingComplete();
            }
            catch (Exception ex)
            {
                // We protect our code from consumer thrown exceptions
                OnProcessException(new InvalidOperationException(string.Format("Exception in consumer handler for ProcessingComplete event: {0}", ex.Message), ex));
            }
        }

        /// <summary>
        /// Disposes of any previously defined local concentrator.
        /// </summary>
        protected internal void DisposeLocalConcentrator()
        {
            if ((object)m_localConcentrator != null)
            {
                m_localConcentrator.ProcessException -= m_localConcentrator_ProcessException;
                m_localConcentrator.Dispose();
            }

            m_localConcentrator = null;
        }

        private void m_localConcentrator_ProcessException(object sender, EventArgs<Exception> e)
        {
            // Make sure any exceptions reported by local concentrator get exposed as needed
            OnProcessException(e.Argument);
        }

        private void m_dataStreamMonitor_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (m_monitoredBytesReceived == 0)
            {
                // If we've received no data in the last timespan, we restart connect cycle...
                m_dataStreamMonitor.Enabled = false;
                OnStatusMessage("\r\nNo data received in {0} seconds, restarting connect cycle...\r\n", (m_dataStreamMonitor.Interval / 1000.0D).ToString("0.0"));
                ThreadPool.QueueUserWorkItem(state => Restart());
            }

            // Reset bytes received bytes being monitored
            m_monitoredBytesReceived = 0L;
        }

        #region [ Command Channel Event Handlers ]

        private void m_commandChannel_ConnectionEstablished(object sender, EventArgs e)
        {
            // Make sure no existing requests are queued for a new publisher connection
            lock (m_requests)
            {
                m_requests.Clear();
            }

            // Define operational modes as soon as possible
            SendServerCommand(ServerCommand.DefineOperationalModes, EndianOrder.BigEndian.GetBytes((uint)m_operationalModes));

            // Notify input adapter base that asynchronous connection succeeded
            OnConnected();

            // Notify consumer that connection was sucessfully established
            OnConnectionEstablished();

            OnStatusMessage("Data subscriber command channel connection to publisher was established.");

            if (m_autoConnect)
            {
                // Attempt authentication if required, remaining steps will happen on successful authentication
                if (m_requireAuthentication)
                    Authenticate(m_sharedSecret, m_authenticationID);
                else
                    StartSubscription();
            }
        }

        private void m_commandChannel_ConnectionTerminated(object sender, EventArgs e)
        {
            OnConnectionTerminated();
            OnStatusMessage("Data subscriber command channel connection to publisher was terminated.");
            DisconnectClient();
        }

        private void m_commandChannel_ConnectionException(object sender, EventArgs<Exception> e)
        {
            Exception ex = e.Argument;
            OnProcessException(new InvalidOperationException("Data subscriber encountered an exception while attempting command channel publisher connection: " + ex.Message, ex));
        }

        private void m_commandChannel_ConnectionAttempt(object sender, EventArgs e)
        {
            // Inject a short delay between multiple connection attempts
            if (m_commandChannelConnectionAttempts > 0)
                Thread.Sleep(2000);

            OnStatusMessage("Attempting command channel connection to publisher...");
            m_commandChannelConnectionAttempts++;
        }

        private void m_commandChannel_SendDataException(object sender, EventArgs<Exception> e)
        {
            Exception ex = e.Argument;

            if (!HandleSocketException(ex as System.Net.Sockets.SocketException) && !(ex is ObjectDisposedException))
                OnProcessException(new InvalidOperationException("Data subscriber encountered an exception while sending command channel data to publisher connection: " + ex.Message, ex));
        }

        private void m_commandChannel_ReceiveData(object sender, EventArgs<int> e)
        {
            try
            {
                byte[] buffer;
                int length = e.Argument;

                m_lastBytesReceived = length;

                buffer = BufferPool.TakeBuffer(length);

                try
                {
                    m_commandChannel.Read(buffer, 0, length);
                    ProcessServerResponse(buffer, length);
                }
                finally
                {
                    if (buffer != null)
                        BufferPool.ReturnBuffer(buffer);
                }
            }
            catch (Exception ex)
            {
                OnProcessException(ex);
            }
        }

        private void m_commandChannel_ReceiveDataException(object sender, EventArgs<Exception> e)
        {
            Exception ex = e.Argument;

            if (!HandleSocketException(ex as System.Net.Sockets.SocketException) && !(ex is ObjectDisposedException))
                OnProcessException(new InvalidOperationException("Data subscriber encountered an exception while receiving command channel data from publisher connection: " + ex.Message, ex));
        }

        #endregion

        #region [ Data Channel Event Handlers ]

        private void m_dataChannel_ConnectionException(object sender, EventArgs<Exception> e)
        {
            Exception ex = e.Argument;
            OnProcessException(new InvalidOperationException("Data subscriber encountered an exception while attempting to establish UDP data channel connection: " + ex.Message, ex));
        }

        private void m_dataChannel_ConnectionAttempt(object sender, EventArgs e)
        {
            // Inject a short delay between multiple connection attempts
            if (m_dataChannelConnectionAttempts > 0)
                Thread.Sleep(2000);

            OnStatusMessage("Attempting to establish data channel connection to publisher...");
            m_dataChannelConnectionAttempts++;
        }

        private void m_dataChannel_ReceiveData(object sender, EventArgs<int> e)
        {
            try
            {
                byte[] buffer;
                int length = e.Argument;

                m_lastBytesReceived = length;

                buffer = BufferPool.TakeBuffer(length);

                try
                {
                    m_dataChannel.Read(buffer, 0, length);
                    ProcessServerResponse(buffer, length);
                }
                finally
                {
                    if (buffer != null)
                        BufferPool.ReturnBuffer(buffer);
                }
            }
            catch (Exception ex)
            {
                OnProcessException(ex);
            }
        }

        private void m_dataChannel_ReceiveDataException(object sender, EventArgs<Exception> e)
        {
            Exception ex = e.Argument;

            if (!HandleSocketException(ex as System.Net.Sockets.SocketException) && !(ex is ObjectDisposedException))
                OnProcessException(new InvalidOperationException("Data subscriber encountered an exception while receiving UDP data from publisher connection: " + ex.Message, ex));
        }

        #endregion

        #endregion
    }
}
