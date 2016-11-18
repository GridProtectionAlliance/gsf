//******************************************************************************************************
//  DataSubscriber.cs - Gbtc
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
//  08/20/2010 - J. Ritchie Carroll
//       Generated original version of source code.
//  02/07/2012 - Mehulbhai Thakkar
//       Modified SynchronizeMetadata to filter devices by original source and modified insert query
//       to populate OriginalSource value. Added to flag to optionally avoid meta-data synchronization.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using GSF.Collections;
using GSF.Communication;
using GSF.Configuration;
using GSF.Data;
using GSF.IO;
using GSF.Net.Security;
using GSF.Reflection;
using GSF.Security.Cryptography;
using GSF.Threading;
using GSF.TimeSeries.Adapters;
using GSF.TimeSeries.Data;
using GSF.TimeSeries.Statistics;
using GSF.Units;
using Random = GSF.Security.Cryptography.Random;
using TcpClient = GSF.Communication.TcpClient;
using UdpClient = GSF.Communication.UdpClient;

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

        private class SubscribedDevice : IDevice, IDisposable
        {
            #region [ Members ]

            // Fields
            private readonly string m_name;

            private Guid m_statusFlagsID;
            private Guid m_frequencyID;
            private Guid m_deltaFrequencyID;

            private long m_dataQualityErrors;
            private long m_timeQualityErrors;
            private long m_deviceErrors;
            private long m_measurementsReceived;
            private double m_measurementsExpected;

            private bool m_disposed;

            #endregion

            #region [ Constructors ]

            public SubscribedDevice(string name)
            {
                if ((object)name == null)
                    throw new ArgumentNullException(nameof(name));

                m_name = name;
                StatisticsEngine.Register(this, name, "Device", "PMU");
            }

            /// <summary>
            /// Releases the unmanaged resources before the <see cref="SubscribedDevice"/> object is reclaimed by <see cref="GC"/>.
            /// </summary>
            ~SubscribedDevice()
            {
                Unregister();
            }

            #endregion

            #region [ Properties ]

            public string Name => m_name;

            public Guid StatusFlagsID
            {
                get
                {
                    return m_statusFlagsID;
                }
                set
                {
                    m_statusFlagsID = value;
                }
            }

            public Guid FrequencyID
            {
                get
                {
                    return m_frequencyID;
                }
                set
                {
                    m_frequencyID = value;
                }
            }

            public Guid DeltaFrequencyID
            {
                get
                {
                    return m_deltaFrequencyID;
                }
                set
                {
                    m_deltaFrequencyID = value;
                }
            }

            public long DataQualityErrors
            {
                get
                {
                    return Interlocked.Read(ref m_dataQualityErrors);
                }
                set
                {
                    Interlocked.Exchange(ref m_dataQualityErrors, value);
                }
            }

            public long TimeQualityErrors
            {
                get
                {
                    return Interlocked.Read(ref m_timeQualityErrors);
                }
                set
                {
                    Interlocked.Exchange(ref m_timeQualityErrors, value);
                }
            }

            public long DeviceErrors
            {
                get
                {
                    return Interlocked.Read(ref m_deviceErrors);
                }
                set
                {
                    Interlocked.Exchange(ref m_deviceErrors, value);
                }
            }

            public long MeasurementsReceived
            {
                get
                {
                    return Interlocked.Read(ref m_measurementsReceived);
                }
                set
                {
                    Interlocked.Exchange(ref m_measurementsReceived, value);
                }
            }

            public long MeasurementsExpected
            {
                get
                {
                    return (long)Interlocked.CompareExchange(ref m_measurementsExpected, 0.0D, 0.0D);
                }
                set
                {
                    Interlocked.Exchange(ref m_measurementsExpected, value);
                }
            }

            #endregion

            #region [ Methods ]

            public override bool Equals(object obj)
            {
                SubscribedDevice subscribedDevice = obj as SubscribedDevice;
                return (object)subscribedDevice != null && m_name.Equals(subscribedDevice.m_name);
            }

            public override int GetHashCode()
            {
                return m_name.GetHashCode();
            }

            /// <summary>
            /// Releases all the resources used by the <see cref="SubscribedDevice"/> object.
            /// </summary>
            public void Dispose()
            {
                Unregister();
                GC.SuppressFinalize(this);
            }

            private void Unregister()
            {
                if (!m_disposed)
                {
                    try
                    {
                        StatisticsEngine.Unregister(this);
                    }
                    finally
                    {
                        m_disposed = true;  // Prevent duplicate dispose.
                    }
                }
            }

            #endregion
        }

        /// <summary>
        /// EventArgs implementation for handling user commands.
        /// </summary>
        public class UserCommandArgs : EventArgs
        {
            /// <summary>
            /// Creates a new instance of the <see cref="UserCommandArgs"/> class.
            /// </summary>
            /// <param name="command">The code for the user command.</param>
            /// <param name="response">The code for the server's response.</param>
            /// <param name="solicited">Indicates whether the response was solicited.</param>
            /// <param name="buffer">Buffer containing the message from the server.</param>
            /// <param name="startIndex">Index into the buffer used to skip the header.</param>
            /// <param name="length">The length of the message in the buffer, including the header.</param>
            public UserCommandArgs(ServerCommand command, ServerResponse response, bool solicited, byte[] buffer, int startIndex, int length)
            {
                Command = command;
                Response = response;
                Solicited = solicited;
                Buffer = buffer;
                StartIndex = startIndex;
                Length = length;
            }

            /// <summary>
            /// Gets the code for the user command.
            /// </summary>
            public ServerCommand Command { get; private set; }

            /// <summary>
            /// Gets the code for the server's response.
            /// </summary>
            public ServerResponse Response { get; private set; }

            /// <summary>
            /// Gets a flag indicating whether the response was solicited.
            /// </summary>
            public bool Solicited { get; private set; }

            /// <summary>
            /// Gets the buffer containing the message from the server.
            /// </summary>
            public byte[] Buffer { get; private set; }

            /// <summary>
            /// Gets the index into the buffer used to skip the header.
            /// </summary>
            public int StartIndex { get; private set; }

            /// <summary>
            /// Gets the length of the message in the buffer, including the header.
            /// </summary>
            public int Length { get; private set; }
        }

        // Constants

        /// <summary>
        /// Defines default value for <see cref="DataSubscriber.OperationalModes"/>.
        /// </summary>
        public const OperationalModes DefaultOperationalModes = OperationalModes.CompressMetadata | OperationalModes.CompressSignalIndexCache | OperationalModes.ReceiveInternalMetadata | OperationalModes.UseCommonSerializationFormat;

        /// <summary>
        /// Defines the default value for the <see cref="MetadataSynchronizationTimeout"/> property.
        /// </summary>
        public const int DefaultMetadataSynchronizationTimeout = 0;

        /// <summary>
        /// Defines the default value for the <see cref="UseTransactionForMetadata"/> property.
        /// </summary>
        public const bool DefaultUseTransactionForMetadata = true;

        /// <summary>
        /// Default value for <see cref="LoggingPath"/>.
        /// </summary>
        public const string DefaultLoggingPath = "ConfigurationCache";

        /// <summary>
        /// Specifies the default value for the <see cref="AllowedParsingExceptions"/> property.
        /// </summary>
        public const int DefaultAllowedParsingExceptions = 10;

        /// <summary>
        /// Specifies the default value for the <see cref="ParsingExceptionWindow"/> property.
        /// </summary>
        public const long DefaultParsingExceptionWindow = 50000000L; // 5 seconds

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
        /// Occurs when client receives response from the server.
        /// </summary>
        public event EventHandler<EventArgs<ServerResponse, ServerCommand>> ReceivedServerResponse;

        /// <summary>
        /// Occurs when client receives message from the server in response to a user command.
        /// </summary>
        public event EventHandler<UserCommandArgs> ReceivedUserCommandResponse;

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
        /// a finite amount of data, e.g., reading a historical range of data during temporal processing.
        /// </remarks>
        public new event EventHandler<EventArgs<string>> ProcessingComplete;

        /// <summary>
        /// Occurs when a notification has been received from the <see cref="DataPublisher"/>.
        /// </summary>
        public event EventHandler<EventArgs<string>> NotificationReceived;

        /// <summary>
        /// Occurs when the server has sent a notification that its configuration has changed, this
        /// can allow subscriber to request updated meta-data if desired.
        /// </summary>
        public event EventHandler ServerConfigurationChanged;

        /// <summary>
        /// Occurs when number of parsing exceptions exceed <see cref="AllowedParsingExceptions"/> during <see cref="ParsingExceptionWindow"/>.
        /// </summary>
        public event EventHandler ExceededParsingExceptionThreshold;

        // Fields
        private volatile Dictionary<Guid, DeviceStatisticsHelper<SubscribedDevice>> m_subscribedDevicesLookup;
        private volatile List<DeviceStatisticsHelper<SubscribedDevice>> m_statisticsHelpers;
        private readonly LongSynchronizedOperation m_registerStatisticsOperation;
        private IClient m_commandChannel;
        private UdpClient m_dataChannel;
        private bool m_useZeroMQChannel;
        private LocalConcentrator m_localConcentrator;
        private MeasurementDecompressionBlock m_decompressionBlock;
        private SharedTimer m_dataStreamMonitor;
        private long m_commandChannelConnectionAttempts;
        private long m_dataChannelConnectionAttempts;
        private volatile SignalIndexCache m_remoteSignalIndexCache;
        private volatile SignalIndexCache m_signalIndexCache;
        private volatile long[] m_baseTimeOffsets;
        private volatile int m_timeIndex;
        private volatile byte[][][] m_keyIVs;
        private volatile bool m_authenticated;
        private volatile bool m_subscribed;
        private volatile int m_lastBytesReceived;
        private long m_monitoredBytesReceived;
        private long m_totalBytesReceived;
        private long m_lastMissingCacheWarning;
        private Guid m_nodeID;
        private int m_gatewayProtocolID;
        private readonly List<ServerCommand> m_requests;
        private SecurityMode m_securityMode;
        private bool m_synchronizedSubscription;
        private bool m_useMillisecondResolution;
        private bool m_requestNaNValueFilter;
        private bool m_autoConnect;
        private string m_metadataFilters;
        private string m_sharedSecret;
        private string m_authenticationID;
        private string m_localCertificate;
        private string m_remoteCertificate;
        private SslPolicyErrors m_validPolicyErrors;
        private X509ChainStatusFlags m_validChainFlags;
        private bool m_checkCertificateRevocation;
        private bool m_internal;
        private bool m_includeTime;
        private bool m_autoSynchronizeMetadata;
        private bool m_useTransactionForMetadata;
        private bool m_useLocalClockAsRealTime;
        private bool m_metadataRefreshPending;
        private int m_metadataSynchronizationTimeout;
        private readonly LongSynchronizedOperation m_synchronizeMetadataOperation;
        private volatile DataSet m_receivedMetadata;
        private DataSet m_synchronizedMetadata;
        private DateTime m_lastMetaDataRefreshTime;
        private OperationalModes m_operationalModes;
        private Encoding m_encoding;
        private string m_loggingPath;
        private RunTimeLog m_runTimeLog;
        private bool m_dataGapRecoveryEnabled;
        private DataGapRecoverer m_dataGapRecoverer;
        private int m_parsingExceptionCount;
        private long m_lastParsingExceptionTime;
        private int m_allowedParsingExceptions;
        private Ticks m_parsingExceptionWindow;
        //private Ticks m_lastMeasurementCheck;
        //private Ticks m_minimumMissingMeasurementThreshold = 5;
        //private double m_transmissionDelayTimeAdjustment = 5.0;

        private readonly List<BufferBlockMeasurement> m_bufferBlockCache;
        private uint m_expectedBufferBlockSequenceNumber;

        private Ticks m_realTime;
        private Ticks m_lastStatisticsHelperUpdate;
        private SharedTimer m_subscribedDevicesTimer;

        private long m_lifetimeMeasurements;
        private long m_minimumMeasurementsPerSecond;
        private long m_maximumMeasurementsPerSecond;
        private long m_totalMeasurementsPerSecond;
        private long m_measurementsPerSecondCount;
        private long m_measurementsInSecond;
        private long m_lastSecondsSinceEpoch;
        private long m_lifetimeTotalLatency;
        private long m_lifetimeMinimumLatency;
        private long m_lifetimeMaximumLatency;
        private long m_lifetimeLatencyMeasurements;

        private long m_syncProgressTotalActions;
        private long m_syncProgressActionsCount;
        private long m_syncProgressUpdateInterval;
        private long m_syncProgressLastMessage;

        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="DataSubscriber"/>.
        /// </summary>
        public DataSubscriber()
        {
            m_registerStatisticsOperation = new LongSynchronizedOperation(HandleDeviceStatisticsRegistration)
            {
                IsBackground = true
            };

            m_requests = new List<ServerCommand>();

            m_synchronizeMetadataOperation = new LongSynchronizedOperation(SynchronizeMetadata)
            {
                IsBackground = true
            };

            m_encoding = Encoding.Unicode;
            m_operationalModes = DefaultOperationalModes;
            m_metadataSynchronizationTimeout = DefaultMetadataSynchronizationTimeout;
            m_allowedParsingExceptions = DefaultAllowedParsingExceptions;
            m_parsingExceptionWindow = DefaultParsingExceptionWindow;

            string loggingPath = FilePath.GetDirectoryName(FilePath.GetAbsolutePath(DefaultLoggingPath));

            if (Directory.Exists(loggingPath))
                m_loggingPath = loggingPath;

            // Default to not using transactions for meta-data on SQL server (helps avoid deadlocks)
            try
            {
                using (AdoDataConnection database = new AdoDataConnection("systemSettings"))
                {
                    m_useTransactionForMetadata = database.DatabaseType != DatabaseType.SQLServer;
                }
            }
            catch
            {
                m_useTransactionForMetadata = DefaultUseTransactionForMetadata;
            }

            DataLossInterval = 10.0D;

            m_bufferBlockCache = new List<BufferBlockMeasurement>();
            m_useLocalClockAsRealTime = true;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the security mode used for communications over the command channel.
        /// </summary>
        public SecurityMode SecurityMode
        {
            get
            {
                return m_securityMode;
            }
            set
            {
                m_securityMode = value;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if <see cref="DataPublisher"/> requires subscribers to authenticate before making data requests.
        /// </summary>
        public bool RequireAuthentication
        {
            get
            {
                return m_securityMode != SecurityMode.None;
            }
            set
            {
                m_securityMode = value ? SecurityMode.Gateway : SecurityMode.None;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if ZeroMQ should be used for command channel communications.
        /// </summary>
        public bool UseZeroMQChannel
        {
            get
            {
                return m_useZeroMQChannel;
            }
            set
            {
                m_useZeroMQChannel = value;
            }
        }

        /// <summary>
        /// Gets or sets logging path to be used to be runtime and outage logs of the subscriber which are required for
        /// automated data recovery.
        /// </summary>
        /// <remarks>
        /// Leave value blank for default path, i.e., installation folder. Can be a fully qualified path or a path that
        /// is relative to the installation folder, e.g., a value of "ConfigurationCache" might resolve to
        /// "C:\Program Files\MyTimeSeriespPp\ConfigurationCache\".
        /// </remarks>
        public string LoggingPath
        {
            get
            {
                return m_loggingPath;
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    string loggingPath = FilePath.GetDirectoryName(FilePath.GetAbsolutePath(value));

                    if (Directory.Exists(loggingPath))
                        value = loggingPath;
                }

                m_loggingPath = value;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if <see cref="DataSubscriber"/> should attempt to auto-connection to <see cref="DataPublisher"/> using defined connection settings.
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
        /// Gets or sets flag that determines if <see cref="DataSubscriber"/> should
        /// automatically request meta-data synchronization and synchronize publisher
        /// meta-data with its own database configuration.
        /// </summary>
        public bool AutoSynchronizeMetadata
        {
            get
            {
                return m_autoSynchronizeMetadata;
            }
            set
            {
                m_autoSynchronizeMetadata = value;
            }
        }

        /// <summary>
        /// Gets or sets requested meta-data filter expressions to be applied by <see cref="DataPublisher"/> before meta-data is sent.
        /// </summary>
        /// <remarks>
        /// Multiple meta-data filters, such filters for different data tables, should be separated by a semicolon. Specifying fields in the filter
        /// expression that do not exist in the data publisher's current meta-data set could cause filter expressions to not be applied and possibly
        /// result in no meta-data being received for the specified data table.
        /// </remarks>
        /// <example>
        /// FILTER MeasurementDetail WHERE SignalType &lt;&gt; 'STAT'; FILTER PhasorDetail WHERE Phase = '+'
        /// </example>
        public string MetadataFilters
        {
            get
            {
                return m_metadataFilters;
            }
            set
            {
                m_metadataFilters = value;
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
        public bool Authenticated => m_authenticated;

        /// <summary>
        /// Gets total data packet bytes received during this session.
        /// </summary>
        public long TotalBytesReceived => m_totalBytesReceived;

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
                        m_dataStreamMonitor = Common.TimerScheduler.CreateTimer();
                        m_dataStreamMonitor.Elapsed += m_dataStreamMonitor_Elapsed;
                        m_dataStreamMonitor.AutoReset = true;
                        m_dataStreamMonitor.Enabled = false;
                    }

                    // Set user specified interval
                    m_dataStreamMonitor.Interval = (int)(value * 1000.0D);
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
        /// Gets or sets the operational mode flag to compress meta-data.
        /// </summary>
        public bool CompressMetadata
        {
            get
            {
                return m_operationalModes.HasFlag(OperationalModes.CompressMetadata);
            }
            set
            {
                if (value)
                    m_operationalModes |= OperationalModes.CompressMetadata;
                else
                    m_operationalModes &= ~OperationalModes.CompressMetadata;
            }
        }

        /// <summary>
        /// Gets or sets the operational mode flag to compress the signal index cache.
        /// </summary>
        public bool CompressSignalIndexCache
        {
            get
            {
                return m_operationalModes.HasFlag(OperationalModes.CompressSignalIndexCache);
            }
            set
            {
                if (value)
                    m_operationalModes |= OperationalModes.CompressSignalIndexCache;
                else
                    m_operationalModes &= ~OperationalModes.CompressSignalIndexCache;
            }
        }

        /// <summary>
        /// Gets or sets the operational mode flag to compress data payloads.
        /// </summary>
        public bool CompressPayload
        {
            get
            {
                return m_operationalModes.HasFlag(OperationalModes.CompressPayloadData);
            }
            set
            {
                if (value)
                    m_operationalModes |= OperationalModes.CompressPayloadData;
                else
                    m_operationalModes &= ~OperationalModes.CompressPayloadData;
            }
        }

        /// <summary>
        /// Gets or sets the operational mode flag to receive internal meta-data.
        /// </summary>
        public bool ReceiveInternalMetadata
        {
            get
            {
                return m_operationalModes.HasFlag(OperationalModes.ReceiveInternalMetadata);
            }
            set
            {
                if (value)
                    m_operationalModes |= OperationalModes.ReceiveInternalMetadata;
                else
                    m_operationalModes &= ~OperationalModes.ReceiveInternalMetadata;
            }
        }

        /// <summary>
        /// Gets or sets the operational mode flag to receive external meta-data.
        /// </summary>
        public bool ReceiveExternalMetadata
        {
            get
            {
                return m_operationalModes.HasFlag(OperationalModes.ReceiveExternalMetadata);
            }
            set
            {
                if (value)
                    m_operationalModes |= OperationalModes.ReceiveExternalMetadata;
                else
                    m_operationalModes &= ~OperationalModes.ReceiveExternalMetadata;
            }
        }

        /// <summary>
        /// Gets or sets the operational mode flag to use the common serialization format.
        /// </summary>
        public bool UseCommonSerializationFormat
        {
            get
            {
                return m_operationalModes.HasFlag(OperationalModes.UseCommonSerializationFormat);
            }
            set
            {
                if (value)
                    m_operationalModes |= OperationalModes.UseCommonSerializationFormat;
                else
                    m_operationalModes &= ~OperationalModes.UseCommonSerializationFormat;
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
        /// Gets or sets the <see cref="CompressionModes"/> used by the subscriber and publisher.
        /// </summary>
        public CompressionModes CompressionModes
        {
            get
            {
                return (CompressionModes)(m_operationalModes & OperationalModes.CompressionModeMask);
            }
            set
            {
                m_operationalModes &= ~OperationalModes.CompressionModeMask;
                m_operationalModes |= (OperationalModes)value;

                if (value.HasFlag(CompressionModes.TSSC))
                    CompressPayload = true;
            }
        }

        /// <summary>
        /// Gets the version number of the protocol in use by this subscriber.
        /// </summary>
        public int Version => (int)(m_operationalModes & OperationalModes.VersionMask);

        /// <summary>
        /// Gets the flag indicating if this adapter supports temporal processing.
        /// </summary>
        /// <remarks>
        /// Although the data subscriber provisions support for temporal processing by receiving historical data from a remote source,
        /// the adapter opens sockets and does not need to be engaged within an actual temporal <see cref="IaonSession"/>, therefore
        /// this method returns <c>false</c> to make sure the adapter doesn't get instantiated within a temporal session.
        /// </remarks>
        public override bool SupportsTemporalProcessing => false;

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

                // Request server update the processing interval
                SendServerCommand(ServerCommand.UpdateProcessingInterval, BigEndian.GetBytes(value));
            }
        }

        /// <summary>
        /// Gets or sets the timeout used when executing database queries during meta-data synchronization.
        /// </summary>
        public int MetadataSynchronizationTimeout
        {
            get
            {
                return m_metadataSynchronizationTimeout;
            }
            set
            {
                m_metadataSynchronizationTimeout = value;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if meta-data synchronization should be performed within a transaction.
        /// </summary>
        public bool UseTransactionForMetadata
        {
            get
            {
                return m_useTransactionForMetadata;
            }
            set
            {
                m_useTransactionForMetadata = value;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines whether to use the local clock when calculating statistics.
        /// </summary>
        public bool UseLocalClockAsRealTime
        {
            get
            {
                return m_useLocalClockAsRealTime;
            }
            set
            {
                m_useLocalClockAsRealTime = value;
            }
        }

        /// <summary>
        /// Gets or sets number of parsing exceptions allowed during <see cref="ParsingExceptionWindow"/> before connection is reset.
        /// </summary>
        public int AllowedParsingExceptions
        {
            get
            {
                return m_allowedParsingExceptions;
            }
            set
            {
                m_allowedParsingExceptions = value;
            }
        }

        /// <summary>
        /// Gets or sets time duration, in <see cref="Ticks"/>, to monitor parsing exceptions.
        /// </summary>
        public Ticks ParsingExceptionWindow
        {
            get
            {
                return m_parsingExceptionWindow;
            }
            set
            {
                m_parsingExceptionWindow = value;
            }
        }

        /// <summary>
        /// Gets or sets <see cref="DataSet"/> based data source available to this <see cref="DataSubscriber"/>.
        /// </summary>
        public override DataSet DataSource
        {
            get
            {
                return base.DataSource;
            }
            set
            {
                base.DataSource = value;
                m_registerStatisticsOperation.RunOnce();

                // For automatic connections, when meta-data refresh is complete, update output measurements to see if any
                // points for subscription have changed after re-application of filter expressions and if so, resubscribe
                if ((object)m_commandChannel != null && m_commandChannel.Enabled && m_autoConnect && UpdateOutputMeasurements())
                {
                    OnStatusMessage("Meta-data received from publisher modified measurement availability, adjusting active subscription...");

                    // Updating subscription will restart data stream monitor upon successful resubscribe
                    SubscribeToOutputMeasurements(true);
                }
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
                status.AppendFormat("             Security mode: {0}", SecurityMode);
                status.AppendLine();
                status.AppendFormat("         Compression modes: {0}", CompressionModes);
                status.AppendLine();
                if ((object)m_dataChannel != null)
                {
                    status.AppendFormat("  UDP Data packet security: {0}", (object)m_keyIVs == null ? "Unencrypted" : "Encrypted");
                    status.AppendLine();
                }
                status.AppendFormat("      Data monitor enabled: {0}", (object)m_dataStreamMonitor != null && m_dataStreamMonitor.Enabled);
                status.AppendLine();
                status.AppendFormat("              Logging path: {0}", FilePath.TrimFileName(m_loggingPath.ToNonNullNorWhiteSpace(FilePath.GetAbsolutePath("")), 51));
                status.AppendLine();

                if (DataLossInterval > 0.0D)
                    status.AppendFormat("No data reconnect interval: {0} seconds", DataLossInterval.ToString("0.000"));
                else
                    status.Append("No data reconnect interval: disabled");

                status.AppendLine();

                status.AppendFormat("    Data gap recovery mode: {0}", m_dataGapRecoveryEnabled ? "Enabled" : "Disabled");
                status.AppendLine();

                if (m_dataGapRecoveryEnabled && (object)m_dataGapRecoverer != null)
                    status.Append(m_dataGapRecoverer.Status);

                if ((object)m_runTimeLog != null)
                {
                    status.AppendLine();
                    status.AppendLine("Run-Time Log Status".CenterText(50));
                    status.AppendLine("-------------------".CenterText(50));
                    status.AppendFormat(m_runTimeLog.Status);
                }

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
        protected override bool UseAsyncConnect => true;

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
                if ((object)m_dataChannel != null)
                {
                    // Detach from events on existing data channel reference
                    m_dataChannel.ConnectionException -= m_dataChannel_ConnectionException;
                    m_dataChannel.ConnectionAttempt -= m_dataChannel_ConnectionAttempt;
                    m_dataChannel.ReceiveData -= m_dataChannel_ReceiveData;
                    m_dataChannel.ReceiveDataException -= m_dataChannel_ReceiveDataException;

                    if ((object)m_dataChannel != value)
                        m_dataChannel.Dispose();
                }

                // Assign new data channel reference
                m_dataChannel = value;

                if ((object)m_dataChannel != null)
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
        /// Gets or sets reference to <see cref="Communication.TcpClient"/> command channel, attaching and/or detaching to events as needed.
        /// </summary>
        protected IClient CommandChannel
        {
            get
            {
                return m_commandChannel;
            }
            set
            {
                if ((object)m_commandChannel != null)
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

                if ((object)m_commandChannel != null)
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

        /// <summary>
        /// Gets the total number of measurements processed through this data publisher over the lifetime of the subscriber.
        /// </summary>
        public long LifetimeMeasurements => m_lifetimeMeasurements;

        /// <summary>
        /// Gets the minimum value of the measurements per second calculation.
        /// </summary>
        public long MinimumMeasurementsPerSecond => m_minimumMeasurementsPerSecond;

        /// <summary>
        /// Gets the maximum value of the measurements per second calculation.
        /// </summary>
        public long MaximumMeasurementsPerSecond => m_maximumMeasurementsPerSecond;

        /// <summary>
        /// Gets the average value of the measurements per second calculation.
        /// </summary>
        public long AverageMeasurementsPerSecond
        {
            get
            {
                if (m_measurementsPerSecondCount == 0L)
                    return 0L;

                return m_totalMeasurementsPerSecond / m_measurementsPerSecondCount;
            }
        }

        /// <summary>
        /// Gets the minimum latency calculated over the full lifetime of the subscriber.
        /// </summary>
        public int LifetimeMinimumLatency => (int)Ticks.ToMilliseconds(m_lifetimeMinimumLatency);

        /// <summary>
        /// Gets the maximum latency calculated over the full lifetime of the subscriber.
        /// </summary>
        public int LifetimeMaximumLatency => (int)Ticks.ToMilliseconds(m_lifetimeMaximumLatency);

        /// <summary>
        /// Gets the average latency calculated over the full lifetime of the subscriber.
        /// </summary>
        public int LifetimeAverageLatency
        {
            get
            {
                if (m_lifetimeLatencyMeasurements == 0)
                    return -1;

                return (int)Ticks.ToMilliseconds(m_lifetimeTotalLatency / m_lifetimeLatencyMeasurements);
            }
        }

        /// <summary>
        /// Gets real-time as determined by either the local clock or the latest measurement received.
        /// </summary>
        protected Ticks RealTime => m_useLocalClockAsRealTime ? (Ticks)DateTime.UtcNow.Ticks : m_realTime;

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

                        if ((object)m_dataGapRecoverer != null)
                        {
                            m_dataGapRecoverer.RecoveredMeasurements -= m_dataGapRecoverer_RecoveredMeasurements;
                            m_dataGapRecoverer.StatusMessage -= m_dataGapRecoverer_StatusMessage;
                            m_dataGapRecoverer.ProcessException -= m_dataGapRecoverer_ProcessException;
                            m_dataGapRecoverer.Dispose();
                            m_dataGapRecoverer = null;
                        }

                        if ((object)m_runTimeLog != null)
                        {
                            m_runTimeLog.ProcessException -= m_runTimeLog_ProcessException;
                            m_runTimeLog.Dispose();
                            m_runTimeLog = null;
                        }

                        if ((object)m_subscribedDevicesTimer != null)
                        {
                            m_subscribedDevicesTimer.Elapsed -= SubscribedDevicesTimer_Elapsed;
                            m_subscribedDevicesTimer.Dispose();
                            m_subscribedDevicesTimer = null;
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
        /// Initializes <see cref="DataSubscriber"/>.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            Dictionary<string, string> settings = Settings;
            string setting;

            OperationalModes operationalModes;
            CompressionModes compressionModes;
            int metadataSynchronizationTimeout;
            double interval;
            int bufferSize;

            // Setup connection to data publishing server with or without authentication required
            if (settings.TryGetValue("requireAuthentication", out setting))
                RequireAuthentication = setting.ParseBoolean();

            // See if user has opted for different operational modes
            if (settings.TryGetValue("operationalModes", out setting) && Enum.TryParse(setting, true, out operationalModes))
                OperationalModes = operationalModes;

            // Set the security mode if explicitly defined
            if (!settings.TryGetValue("securityMode", out setting) || !Enum.TryParse(setting, true, out m_securityMode))
                m_securityMode = SecurityMode.None;

            // Apply gateway compression mode to operational mode flags
            if (settings.TryGetValue("compressionModes", out setting) && Enum.TryParse(setting, true, out compressionModes))
                CompressionModes = compressionModes;

            if (settings.TryGetValue("useZeroMQChannel", out setting))
                m_useZeroMQChannel = setting.ParseBoolean();

            // TODO: Remove this exception when CURVE is enabled in GSF ZeroMQ library
            if (m_useZeroMQChannel && m_securityMode == SecurityMode.TLS)
                throw new ArgumentException("CURVE security settings are not yet available for GSF ZeroMQ client channel.");

            // Settings specific to Gateway security
            if (m_securityMode == SecurityMode.Gateway)
            {
                if (!settings.TryGetValue("sharedSecret", out m_sharedSecret) || string.IsNullOrWhiteSpace(m_sharedSecret))
                    throw new ArgumentException("The \"sharedSecret\" setting must be defined when using Gateway security mode.");

                if (!settings.TryGetValue("authenticationID", out m_authenticationID) || string.IsNullOrWhiteSpace(m_authenticationID))
                    throw new ArgumentException("The \"authenticationID\" setting must be defined when using Gateway security mode.");
            }

            // Settings specific to Transport Layer Security
            if (m_securityMode == SecurityMode.TLS)
            {
                if (!settings.TryGetValue("localCertificate", out m_localCertificate) || !File.Exists(m_localCertificate))
                    m_localCertificate = GetLocalCertificate();

                if (!settings.TryGetValue("remoteCertificate", out m_remoteCertificate) || !RemoteCertificateExists())
                    throw new ArgumentException("The \"remoteCertificate\" setting must be defined and certificate file must exist when using TLS security mode.");

                if (!settings.TryGetValue("validPolicyErrors", out setting) || !Enum.TryParse(setting, out m_validPolicyErrors))
                    m_validPolicyErrors = SslPolicyErrors.None;

                if (!settings.TryGetValue("validChainFlags", out setting) || !Enum.TryParse(setting, out m_validChainFlags))
                    m_validChainFlags = X509ChainStatusFlags.NoError;

                if (settings.TryGetValue("checkCertificateRevocation", out setting) && !string.IsNullOrWhiteSpace(setting))
                    m_checkCertificateRevocation = setting.ParseBoolean();
                else
                    m_checkCertificateRevocation = true;
            }

            // Check if measurements for this connection should be marked as "internal" - i.e., owned and allowed for proxy
            if (settings.TryGetValue("internal", out setting))
                m_internal = setting.ParseBoolean();

            // Check if user has explicitly defined the ReceiveInternalMetadata flag
            if (settings.TryGetValue("receiveInternalMetadata", out setting))
                ReceiveInternalMetadata = setting.ParseBoolean();

            // Check if user has explicitly defined the ReceiveExternalMetadata flag
            if (settings.TryGetValue("receiveExternalMetadata", out setting))
                ReceiveExternalMetadata = setting.ParseBoolean();

            // Check if user has defined a meta-data synchronization timeout
            if (settings.TryGetValue("metadataSynchronizationTimeout", out setting) && int.TryParse(setting, out metadataSynchronizationTimeout))
                m_metadataSynchronizationTimeout = metadataSynchronizationTimeout;

            // Check if user has defined a flag for using a transaction during meta-data synchronization
            if (settings.TryGetValue("useTransactionForMetadata", out setting))
                m_useTransactionForMetadata = setting.ParseBoolean();

            // Check if user wants to request that publisher use millisecond resolution to conserve bandwidth
            if (settings.TryGetValue("useMillisecondResolution", out setting))
                m_useMillisecondResolution = setting.ParseBoolean();

            // Check if user wants to request that publisher remove NaN from the data stream to conserve bandwidth
            if (settings.TryGetValue("requestNaNValueFilter", out setting))
                m_requestNaNValueFilter = setting.ParseBoolean();

            // Check if user has defined any meta-data filter expressions
            if (settings.TryGetValue("metadataFilters", out setting))
                m_metadataFilters = setting;

            // Define auto connect setting
            if (settings.TryGetValue("autoConnect", out setting))
            {
                m_autoConnect = setting.ParseBoolean();

                if (m_autoConnect)
                    m_autoSynchronizeMetadata = true;
            }

            // Define the maximum allowed exceptions before resetting the connection
            if (settings.TryGetValue("allowedParsingExceptions", out setting))
                m_allowedParsingExceptions = int.Parse(setting);

            // Define the window of time over which parsing exceptions are tolerated
            if (settings.TryGetValue("parsingExceptionWindow", out setting))
                m_parsingExceptionWindow = Ticks.FromSeconds(double.Parse(setting));

            // Check if synchronize meta-data is explicitly enabled or disabled
            if (settings.TryGetValue("synchronizeMetadata", out setting))
                m_autoSynchronizeMetadata = setting.ParseBoolean();

            // Define data loss interval
            if (settings.TryGetValue("dataLossInterval", out setting) && double.TryParse(setting, out interval))
                DataLossInterval = interval;

            // Define buffer size
            if (!settings.TryGetValue("bufferSize", out setting) || !int.TryParse(setting, out bufferSize))
                bufferSize = ClientBase.DefaultReceiveBufferSize;

            if (settings.TryGetValue("useLocalClockAsRealTime", out setting))
                m_useLocalClockAsRealTime = setting.ParseBoolean();

            if (m_autoConnect)
            {
                // Connect to local events when automatically engaging connection cycle
                ConnectionAuthenticated += DataSubscriber_ConnectionAuthenticated;
                MetaDataReceived += DataSubscriber_MetaDataReceived;

                // Update output measurements to include "subscribed" points
                UpdateOutputMeasurements(true);
            }
            else if (m_autoSynchronizeMetadata)
            {
                // Output measurements do not include "subscribed" points,
                // but should still be filtered if applicable
                TryFilterOutputMeasurements();
            }

            if (m_securityMode != SecurityMode.TLS)
            {
                if (m_useZeroMQChannel)
                {
                    // Create a new ZeroMQ Dealer
                    ZeroMQClient commandChannel = new ZeroMQClient();

                    // Initialize default settings
                    commandChannel.PersistSettings = false;
                    commandChannel.MaxConnectionAttempts = 1;
                    commandChannel.ReceiveBufferSize = bufferSize;
                    commandChannel.SendBufferSize = bufferSize;

                    // Assign command channel client reference and attach to needed events
                    CommandChannel = commandChannel;
                }
                else
                {
                    // Create a new TCP client
                    TcpClient commandChannel = new TcpClient();

                    // Initialize default settings
                    commandChannel.PayloadAware = true;
                    commandChannel.PersistSettings = false;
                    commandChannel.MaxConnectionAttempts = 1;
                    commandChannel.ReceiveBufferSize = bufferSize;
                    commandChannel.SendBufferSize = bufferSize;

                    // Assign command channel client reference and attach to needed events
                    CommandChannel = commandChannel;
                }
            }
            else
            {
                if (m_useZeroMQChannel)
                {
                    // Create a new ZeroMQ Dealer with CURVE security enabled
                    ZeroMQClient commandChannel = new ZeroMQClient();

                    // Initialize default settings
                    commandChannel.PersistSettings = false;
                    commandChannel.MaxConnectionAttempts = 1;
                    commandChannel.ReceiveBufferSize = bufferSize;
                    commandChannel.SendBufferSize = bufferSize;

                    // TODO: Parse certificate and pass keys to ZeroMQClient for CURVE security

                    // Assign command channel client reference and attach to needed events
                    CommandChannel = commandChannel;
                }
                else
                {
                    // Create a new TLS client and certificate checker
                    TlsClient commandChannel = new TlsClient();
                    SimpleCertificateChecker certificateChecker = new SimpleCertificateChecker();

                    // Set up certificate checker
                    certificateChecker.TrustedCertificates.Add(new X509Certificate2(FilePath.GetAbsolutePath(m_remoteCertificate)));
                    certificateChecker.ValidPolicyErrors = m_validPolicyErrors;
                    certificateChecker.ValidChainFlags = m_validChainFlags;

                    // Initialize default settings
                    commandChannel.PayloadAware = true;
                    commandChannel.PersistSettings = false;
                    commandChannel.MaxConnectionAttempts = 1;
                    commandChannel.CertificateFile = FilePath.GetAbsolutePath(m_localCertificate);
                    commandChannel.CheckCertificateRevocation = m_checkCertificateRevocation;
                    commandChannel.CertificateChecker = certificateChecker;
                    commandChannel.ReceiveBufferSize = bufferSize;
                    commandChannel.SendBufferSize = bufferSize;

                    // Assign command channel client reference and attach to needed events
                    CommandChannel = commandChannel;
                }
            }

            // Get proper connection string - either from specified command channel or from base connection string
            if (settings.TryGetValue("commandChannel", out setting))
                m_commandChannel.ConnectionString = setting;
            else
                m_commandChannel.ConnectionString = ConnectionString;

            // Get logging path, if any has been defined
            if (settings.TryGetValue("loggingPath", out setting))
            {
                setting = FilePath.GetDirectoryName(FilePath.GetAbsolutePath(setting));

                if (Directory.Exists(setting))
                    m_loggingPath = setting;
                else
                    OnStatusMessage("WARNING: Logging path \"{0}\" not found, defaulting to \"{1}\"...", setting, FilePath.GetAbsolutePath(""));
            }

            // Initialize data gap recovery processing, if requested
            if (settings.TryGetValue("dataGapRecovery", out setting))
            {
                // Make sure setting exists to allow user to by-pass phasor data source validation at startup
                ConfigurationFile configFile = ConfigurationFile.Current;
                CategorizedSettingsElementCollection systemSettings = configFile.Settings["systemSettings"];
                CategorizedSettingsElement dataGapRecoveryEnabledSetting = systemSettings["DataGapRecoveryEnabled"];

                // See if this node should process phasor source validation
                if ((object)dataGapRecoveryEnabledSetting == null || dataGapRecoveryEnabledSetting.ValueAsBoolean())
                {
                    // Example connection string for data gap recovery:
                    //  dataGapRecovery={enabled=true; recoveryStartDelay=10.0; minimumRecoverySpan=0.0; maximumRecoverySpan=3600.0}
                    Dictionary<string, string> dataGapSettings = setting.ParseKeyValuePairs();

                    if (dataGapSettings.TryGetValue("enabled", out setting) && setting.ParseBoolean())
                    {
                        // Remove dataGapRecovery connection setting from command channel connection string, if defined there.
                        // This will prevent any recursive data gap recovery operations from being established:
                        Dictionary<string, string> connectionSettings = m_commandChannel.ConnectionString.ParseKeyValuePairs();
                        connectionSettings.Remove("dataGapRecovery");
                        connectionSettings.Remove("autoConnect");
                        connectionSettings.Remove("synchronizeMetadata");
                        connectionSettings.Remove("outputMeasurements");

                        // Note that the data gap recoverer will connect on the same command channel port as
                        // the real-time subscriber (TCP only)
                        m_dataGapRecoveryEnabled = true;
                        m_dataGapRecoverer = new DataGapRecoverer();
                        m_dataGapRecoverer.SourceConnectionName = Name;
                        m_dataGapRecoverer.DataSource = DataSource;
                        m_dataGapRecoverer.ConnectionString = string.Join("; ", $"autoConnect=false; synchronizeMetadata=false{(string.IsNullOrWhiteSpace(m_loggingPath) ? "" : "; loggingPath=" + m_loggingPath)}", dataGapSettings.JoinKeyValuePairs(), connectionSettings.JoinKeyValuePairs());
                        m_dataGapRecoverer.FilterExpression = this.OutputMeasurementKeys().Select(key => key.SignalID.ToString()).ToDelimitedString(';');
                        m_dataGapRecoverer.RecoveredMeasurements += m_dataGapRecoverer_RecoveredMeasurements;
                        m_dataGapRecoverer.StatusMessage += m_dataGapRecoverer_StatusMessage;
                        m_dataGapRecoverer.ProcessException += m_dataGapRecoverer_ProcessException;
                        m_dataGapRecoverer.Initialize();
                    }
                    else
                    {
                        m_dataGapRecoveryEnabled = false;
                    }
                }
            }
            else
            {
                m_dataGapRecoveryEnabled = false;
            }

            // Register subscriber with the statistics engine
            StatisticsEngine.Register(this, "Subscriber", "SUB");
            StatisticsEngine.Calculated += (sender, args) => ResetMeasurementsPerSecondCounters();

            Initialized = true;
        }

        // Gets the path to the local certificate from the configuration file
        private string GetLocalCertificate()
        {
            CategorizedSettingsElement localCertificateElement = ConfigurationFile.Current.Settings["systemSettings"]["LocalCertificate"];
            string localCertificate = null;

            if ((object)localCertificateElement != null)
                localCertificate = localCertificateElement.Value;

            if ((object)localCertificate == null || !File.Exists(FilePath.GetAbsolutePath(localCertificate)))
                throw new InvalidOperationException("Unable to find local certificate. Local certificate file must exist when using TLS security mode.");

            return localCertificate;
        }

        // Checks if the specified certificate exists
        private bool RemoteCertificateExists()
        {
            string fullPath = FilePath.GetAbsolutePath(m_remoteCertificate);
            CategorizedSettingsElement remoteCertificateElement;

            if (!File.Exists(fullPath))
            {
                remoteCertificateElement = ConfigurationFile.Current.Settings["systemSettings"]["RemoteCertificatesPath"];

                if ((object)remoteCertificateElement != null)
                {
                    m_remoteCertificate = Path.Combine(remoteCertificateElement.Value, m_remoteCertificate);
                    fullPath = FilePath.GetAbsolutePath(m_remoteCertificate);
                }
            }

            return File.Exists(fullPath);
        }

        // Initialize (or reinitialize) the output measurements associated with the data subscriber.
        // Returns true if output measurements were updated, otherwise false if they remain the same.
        private bool UpdateOutputMeasurements(bool initialCall = false)
        {
            IMeasurement[] originalOutputMeasurements = OutputMeasurements;

            // Reapply output measurements if reinitializing - this way filter expressions and/or sourceIDs
            // will be reapplied. This can be important after a meta-data refresh which may have added new
            // measurements that could now be applicable as desired output measurements.
            if (!initialCall)
            {
                string setting;

                if (Settings.TryGetValue("outputMeasurements", out setting))
                    OutputMeasurements = ParseOutputMeasurements(DataSource, true, setting);

                OutputSourceIDs = OutputSourceIDs;
            }

            // If active measurements are defined, attempt to defined desired subscription points from there
            if ((object)DataSource != null && DataSource.Tables.Contains("ActiveMeasurements"))
            {
                try
                {
                    // Filter to points associated with this subscriber that have been requested for subscription, are enabled and not owned locally
                    DataRow[] filteredRows = DataSource.Tables["ActiveMeasurements"].Select("Subscribed <> 0");
                    List<IMeasurement> subscribedMeasurements = new List<IMeasurement>();
                    Guid signalID;

                    foreach (DataRow row in filteredRows)
                    {
                        // Create a new measurement for the provided field level information
                        Measurement measurement = new Measurement();

                        // Parse primary measurement identifier
                        signalID = row["SignalID"].ToNonNullString(Guid.Empty.ToString()).ConvertToType<Guid>();

                        // Set measurement key if defined
                        MeasurementKey key = MeasurementKey.LookUpOrCreate(signalID, row["ID"].ToString());
                        key.SetDataSourceCommonValues(row["PointTag"].ToNonNullString(), double.Parse(row["Adder"].ToString()), double.Parse(row["Multiplier"].ToString()));
                        measurement.CommonMeasurementFields = key.DataSourceCommonValues;

                        subscribedMeasurements.Add(measurement);
                    }

                    if (subscribedMeasurements.Count > 0)
                    {
                        // Combine subscribed output measurement with any existing output measurement and return unique set
                        if ((object)OutputMeasurements == null)
                            OutputMeasurements = subscribedMeasurements.ToArray();
                        else
                            OutputMeasurements = subscribedMeasurements.Concat(OutputMeasurements).Distinct().ToArray();
                    }
                }
                catch (Exception ex)
                {
                    // Errors here may not be catastrophic, this simply limits the auto-assignment of input measurement keys desired for subscription
                    OnProcessException(new InvalidOperationException($"Failed to apply subscribed measurements to subscription filter: {ex.Message}", ex));
                }
            }

            // Ensure that we are not attempting to subscribe to
            // measurements that we know cannot be published
            TryFilterOutputMeasurements();

            // Determine if output measurements have changed
            return originalOutputMeasurements.CompareTo(OutputMeasurements, false) != 0;
        }

        // When synchronizing meta-data, the publisher sends meta-data for all possible signals we can subscribe to.
        // Here we check each signal defined in OutputMeasurements to determine whether that signal was defined in
        // the published meta-data rather than blindly attempting to subscribe to all signals.
        private void TryFilterOutputMeasurements()
        {
            IEnumerable<Guid> measurementIDs;
            ISet<Guid> measurementIDSet;
            Guid signalID = Guid.Empty;

            try
            {
                if ((object)OutputMeasurements != null && (object)DataSource != null && DataSource.Tables.Contains("ActiveMeasurements"))
                {
                    // Have to use a Convert expression for DeviceID column in Select function
                    // here since SQLite doesn't report data types for COALESCE based columns
                    measurementIDs = DataSource.Tables["ActiveMeasurements"]
                        .Select($"Convert(DeviceID, 'System.String') = '{ID}'")
                        .Where(row => Guid.TryParse(row["SignalID"].ToNonNullString(), out signalID))
                        .Select(row => signalID);

                    measurementIDSet = new HashSet<Guid>(measurementIDs);

                    OutputMeasurements = OutputMeasurements.Where(measurement => measurementIDSet.Contains(measurement.ID)).ToArray();
                }
            }
            catch (Exception ex)
            {
                OnProcessException(new InvalidOperationException($"Error when filtering output measurements by device ID: {ex.Message}", ex));
            }
        }

        /// <summary>
        /// Authenticates subscriber to a data publisher.
        /// </summary>
        /// <param name="sharedSecret">Shared secret used to look up private crypto key and initialization vector.</param>
        /// <param name="authenticationID">Authentication ID that publisher will use to validate subscriber identity.</param>
        /// <returns><c>true</c> if authentication transmission was successful; otherwise <c>false</c>.</returns>
        public virtual bool Authenticate(string sharedSecret, string authenticationID)
        {
            if (!string.IsNullOrWhiteSpace(authenticationID))
            {
                try
                {
                    using (BlockAllocatedMemoryStream buffer = new BlockAllocatedMemoryStream())
                    {
                        byte[] salt = new byte[DataPublisher.CipherSaltLength];
                        byte[] bytes;

                        // Generate some random prefix data to make sure auth key transmission is always unique
                        Random.GetBytes(salt);

                        // Get encoded bytes of authentication key
                        bytes = salt.Combine(m_encoding.GetBytes(authenticationID));

                        // Encrypt authentication key
                        bytes = bytes.Encrypt(sharedSecret, CipherStrength.Aes256);

                        // Write encoded authentication key length into buffer
                        buffer.Write(BigEndian.GetBytes(bytes.Length), 0, 4);

                        // Encode encrypted authentication key into buffer
                        buffer.Write(bytes, 0, bytes.Length);

                        // Send authentication command to server with associated command buffer
                        return SendServerCommand(ServerCommand.Authenticate, buffer.ToArray());
                    }
                }
                catch (Exception ex)
                {
                    OnProcessException(new InvalidOperationException("Exception occurred while trying to authenticate publisher subscription: " + ex.Message, ex));
                }
            }
            else
                OnProcessException(new InvalidOperationException("Cannot authenticate subscription without a connection string."));

            return false;
        }

        /// <summary>
        /// Subscribes (or re-subscribes) to a data publisher for a set of data points.
        /// </summary>
        /// <param name="info">Configuration object that defines the subscription.</param>
        /// <returns><c>true</c> if subscribe transmission was successful; otherwise <c>false</c>.</returns>
        public bool Subscribe(SubscriptionInfo info)
        {
            SynchronizedSubscriptionInfo synchronizedSubscriptionInfo = info as SynchronizedSubscriptionInfo;

            if ((object)synchronizedSubscriptionInfo != null)
                return SynchronizedSubscribe(synchronizedSubscriptionInfo);

            UnsynchronizedSubscriptionInfo unsynchronizedSubscriptionInfo = info as UnsynchronizedSubscriptionInfo;

            if ((object)unsynchronizedSubscriptionInfo != null)
                return UnsynchronizedSubscribe(unsynchronizedSubscriptionInfo);

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
                connectionString.AppendFormat("requestNaNValueFilter={0};", info.RequestNaNValueFilter);
                connectionString.AppendFormat("downsamplingMethod={0};", info.DownsamplingMethod);
                connectionString.AppendFormat("processingInterval={0};", info.ProcessingInterval);
                connectionString.AppendFormat("assemblyInfo={{source={0};version={1}.{2}.{3};buildDate={4}}};", assemblyInfo.Name, assemblyInfo.Version.Major, assemblyInfo.Version.Minor, assemblyInfo.Version.Build, assemblyInfo.BuildDate.ToString("yyyy-MM-dd HH:mm:ss"));

                if (!string.IsNullOrWhiteSpace(info.FilterExpression))
                    connectionString.AppendFormat("inputMeasurementKeys={{{0}}};", info.FilterExpression);

                if (info.UdpDataChannel)
                    connectionString.AppendFormat("dataChannel={{localport={0}}};", info.DataChannelLocalPort);

                if (!string.IsNullOrWhiteSpace(info.StartTime))
                    connectionString.AppendFormat("startTimeConstraint={0};", info.StartTime);

                if (!string.IsNullOrWhiteSpace(info.StopTime))
                    connectionString.AppendFormat("stopTimeConstraint={0};", info.StopTime);

                if (!string.IsNullOrWhiteSpace(info.ConstraintParameters))
                    connectionString.AppendFormat("timeConstraintParameters={0};", info.ConstraintParameters);

                if (!string.IsNullOrWhiteSpace(info.ExtraConnectionStringParameters))
                    connectionString.AppendFormat("{0};", info.ExtraConnectionStringParameters);

                return Subscribe(true, info.UseCompactMeasurementFormat, connectionString.ToString());
            }

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
                DateTime startTimeConstraint = !string.IsNullOrWhiteSpace(info.StartTime) ? ParseTimeTag(info.StartTime) : DateTime.MinValue;
                DateTime stopTimeConstraint = !string.IsNullOrWhiteSpace(info.StopTime) ? ParseTimeTag(info.StopTime) : DateTime.MaxValue;

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
            connectionString.AppendFormat("requestNaNValueFilter={0};", info.RequestNaNValueFilter);
            connectionString.AppendFormat("assemblyInfo={{source={0};version={1}.{2}.{3};buildDate={4}}};", assemblyInfo.Name, assemblyInfo.Version.Major, assemblyInfo.Version.Minor, assemblyInfo.Version.Build, assemblyInfo.BuildDate.ToString("yyyy-MM-dd HH:mm:ss"));

            if (!string.IsNullOrWhiteSpace(info.FilterExpression))
                connectionString.AppendFormat("inputMeasurementKeys={{{0}}};", info.FilterExpression);

            if (info.UdpDataChannel)
                connectionString.AppendFormat("dataChannel={{localport={0}}};", info.DataChannelLocalPort);

            if (!string.IsNullOrWhiteSpace(info.StartTime))
                connectionString.AppendFormat("startTimeConstraint={0};", info.StartTime);

            if (!string.IsNullOrWhiteSpace(info.StopTime))
                connectionString.AppendFormat("stopTimeConstraint={0};", info.StopTime);

            if (!string.IsNullOrWhiteSpace(info.ConstraintParameters))
                connectionString.AppendFormat("timeConstraintParameters={0};", info.ConstraintParameters);

            if (!string.IsNullOrWhiteSpace(info.ExtraConnectionStringParameters))
                connectionString.AppendFormat("{0};", info.ExtraConnectionStringParameters);

            // Make sure not to monitor for data loss any faster than down-sample time on throttled connections - additionally
            // you will want to make sure data stream monitor is twice lag-time to allow time for initial points to arrive.
            if (info.Throttled && (object)m_dataStreamMonitor != null && m_dataStreamMonitor.Interval / 1000.0D < info.LagTime)
                m_dataStreamMonitor.Interval = (int)(2.0D * info.LagTime * 1000.0D);

            // Set millisecond resolution member variable for compact measurement parsing
            m_useMillisecondResolution = info.UseMillisecondResolution;

            return Subscribe(false, info.UseCompactMeasurementFormat, connectionString.ToString());
        }

        /// <summary>
        /// Subscribes (or re-subscribes) to a data publisher for a remotely synchronized set of data points.
        /// </summary>
        /// <param name="compactFormat">Boolean value that determines if the compact measurement format should be used. Set to <c>false</c> for full fidelity measurement serialization; otherwise set to <c>true</c> for bandwidth conservation.</param>
        /// <param name="framesPerSecond">The desired number of data frames per second.</param>
        /// <param name="lagTime">Allowed past time deviation tolerance, in seconds (can be sub-second).</param>
        /// <param name="leadTime">Allowed future time deviation tolerance, in seconds (can be sub-second).</param>
        /// <param name="filterExpression">Filtering expression that defines the measurements that are being subscribed.</param>
        /// <param name="dataChannel">Desired UDP return data channel connection string to use for data packet transmission. Set to <c>null</c> to use TCP channel for data transmission.</param>
        /// <param name="useLocalClockAsRealTime">Boolean value that determines whether or not to use the local clock time as real-time.</param>
        /// <param name="ignoreBadTimestamps">Boolean value that determines if bad timestamps (as determined by measurement's timestamp quality) should be ignored when sorting measurements.</param>
        /// <param name="allowSortsByArrival"> Gets or sets flag that determines whether or not to allow incoming measurements with bad timestamps to be sorted by arrival time.</param>
        /// <param name="timeResolution">Gets or sets the maximum time resolution, in ticks, to use when sorting measurements by timestamps into their proper destination frame.</param>
        /// <param name="allowPreemptivePublishing">Gets or sets flag that allows system to preemptively publish frames assuming all expected measurements have arrived.</param>
        /// <param name="downsamplingMethod">Gets the total number of down-sampled measurements processed by the concentrator.</param>
        /// <param name="startTime">Defines a relative or exact start time for the temporal constraint to use for historical playback.</param>
        /// <param name="stopTime">Defines a relative or exact stop time for the temporal constraint to use for historical playback.</param>
        /// <param name="constraintParameters">Defines any temporal parameters related to the constraint to use for historical playback.</param>
        /// <param name="processingInterval">Defines the desired processing interval milliseconds, i.e., historical play back speed, to use when temporal constraints are defined.</param>
        /// <param name="waitHandleNames">Comma separated list of wait handle names used to establish external event wait handles needed for inter-adapter synchronization.</param>
        /// <param name="waitHandleTimeout">Maximum wait time for external events, in milliseconds, before proceeding.</param>
        /// <returns><c>true</c> if subscribe transmission was successful; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// <para>
        /// When the <paramref name="startTime"/> or <paramref name="stopTime"/> temporal processing constraints are defined (i.e., not <c>null</c>), this
        /// specifies the start and stop time over which the subscriber session will process data. Passing in <c>null</c> for the <paramref name="startTime"/>
        /// and <paramref name="stopTime"/> specifies the subscriber session will process data in standard, i.e., real-time, operation.
        /// </para>
        /// <para>
        /// With the exception of the values of -1 and 0, the <paramref name="processingInterval"/> value specifies the desired historical playback data
        /// processing interval in milliseconds. This is basically a delay, or timer interval, over which to process data. Setting this value to -1 means
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
        public virtual bool RemotelySynchronizedSubscribe(bool compactFormat, int framesPerSecond, double lagTime, double leadTime, string filterExpression, string dataChannel = null, bool useLocalClockAsRealTime = false, bool ignoreBadTimestamps = false, bool allowSortsByArrival = true, long timeResolution = Ticks.PerMillisecond, bool allowPreemptivePublishing = true, DownsamplingMethod downsamplingMethod = DownsamplingMethod.LastReceived, string startTime = null, string stopTime = null, string constraintParameters = null, int processingInterval = -1, string waitHandleNames = null, int waitHandleTimeout = 0)
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

            if (!string.IsNullOrWhiteSpace(waitHandleNames))
            {
                connectionString.AppendFormat("; waitHandleNames={0}", waitHandleNames);
                connectionString.AppendFormat("; waitHandleTimeout={0}", waitHandleTimeout);
            }

            return Subscribe(true, compactFormat, connectionString.ToString());
        }

        /// <summary>
        /// Subscribes (or re-subscribes) to a data publisher for a locally synchronized set of data points.
        /// </summary>
        /// <param name="compactFormat">Boolean value that determines if the compact measurement format should be used. Set to <c>false</c> for full fidelity measurement serialization; otherwise set to <c>true</c> for bandwidth conservation.</param>
        /// <param name="framesPerSecond">The desired number of data frames per second.</param>
        /// <param name="lagTime">Allowed past time deviation tolerance, in seconds (can be sub-second).</param>
        /// <param name="leadTime">Allowed future time deviation tolerance, in seconds (can be sub-second).</param>
        /// <param name="filterExpression">Filtering expression that defines the measurements that are being subscribed.</param>
        /// <param name="dataChannel">Desired UDP return data channel connection string to use for data packet transmission. Set to <c>null</c> to use TCP channel for data transmission.</param>
        /// <param name="useLocalClockAsRealTime">Boolean value that determines whether or not to use the local clock time as real-time.</param>
        /// <param name="ignoreBadTimestamps">Boolean value that determines if bad timestamps (as determined by measurement's timestamp quality) should be ignored when sorting measurements.</param>
        /// <param name="allowSortsByArrival"> Gets or sets flag that determines whether or not to allow incoming measurements with bad timestamps to be sorted by arrival time.</param>
        /// <param name="timeResolution">Gets or sets the maximum time resolution, in ticks, to use when sorting measurements by timestamps into their proper destination frame.</param>
        /// <param name="allowPreemptivePublishing">Gets or sets flag that allows system to preemptively publish frames assuming all expected measurements have arrived.</param>
        /// <param name="downsamplingMethod">Gets the total number of down-sampled measurements processed by the concentrator.</param>
        /// <param name="startTime">Defines a relative or exact start time for the temporal constraint to use for historical playback.</param>
        /// <param name="stopTime">Defines a relative or exact stop time for the temporal constraint to use for historical playback.</param>
        /// <param name="constraintParameters">Defines any temporal parameters related to the constraint to use for historical playback.</param>
        /// <param name="processingInterval">Defines the desired processing interval milliseconds, i.e., historical play back speed, to use when temporal constraints are defined.</param>
        /// <param name="waitHandleNames">Comma separated list of wait handle names used to establish external event wait handles needed for inter-adapter synchronization.</param>
        /// <param name="waitHandleTimeout">Maximum wait time for external events, in milliseconds, before proceeding.</param>
        /// <returns><c>true</c> if subscribe transmission was successful; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// <para>
        /// When the <paramref name="startTime"/> or <paramref name="stopTime"/> temporal processing constraints are defined (i.e., not <c>null</c>), this
        /// specifies the start and stop time over which the subscriber session will process data. Passing in <c>null</c> for the <paramref name="startTime"/>
        /// and <paramref name="stopTime"/> specifies the subscriber session will process data in standard, i.e., real-time, operation.
        /// </para>
        /// <para>
        /// With the exception of the values of -1 and 0, the <paramref name="processingInterval"/> value specifies the desired historical playback data
        /// processing interval in milliseconds. This is basically a delay, or timer interval, over which to process data. Setting this value to -1 means
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
        public virtual bool LocallySynchronizedSubscribe(bool compactFormat, int framesPerSecond, double lagTime, double leadTime, string filterExpression, string dataChannel = null, bool useLocalClockAsRealTime = false, bool ignoreBadTimestamps = false, bool allowSortsByArrival = true, long timeResolution = Ticks.PerMillisecond, bool allowPreemptivePublishing = true, DownsamplingMethod downsamplingMethod = DownsamplingMethod.LastReceived, string startTime = null, string stopTime = null, string constraintParameters = null, int processingInterval = -1, string waitHandleNames = null, int waitHandleTimeout = 0)
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
            DateTime startTimeConstraint = !string.IsNullOrWhiteSpace(startTime) ? ParseTimeTag(startTime) : DateTime.MinValue;
            DateTime stopTimeConstraint = !string.IsNullOrWhiteSpace(stopTime) ? ParseTimeTag(stopTime) : DateTime.MaxValue;

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

            if (!string.IsNullOrWhiteSpace(waitHandleNames))
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

        /// <summary>
        /// Subscribes (or re-subscribes) to a data publisher for an unsynchronized set of data points.
        /// </summary>
        /// <param name="compactFormat">Boolean value that determines if the compact measurement format should be used. Set to <c>false</c> for full fidelity measurement serialization; otherwise set to <c>true</c> for bandwidth conservation.</param>
        /// <param name="throttled">Boolean value that determines if data should be throttled at a set transmission interval or sent on change.</param>
        /// <param name="filterExpression">Filtering expression that defines the measurements that are being subscribed.</param>
        /// <param name="dataChannel">Desired UDP return data channel connection string to use for data packet transmission. Set to <c>null</c> to use TCP channel for data transmission.</param>
        /// <param name="includeTime">Boolean value that determines if time is a necessary component in streaming data.</param>
        /// <param name="lagTime">When <paramref name="throttled"/> is <c>true</c>, defines the data transmission speed in seconds (can be sub-second).</param>
        /// <param name="leadTime">When <paramref name="throttled"/> is <c>true</c>, defines the allowed time deviation tolerance to real-time in seconds (can be sub-second).</param>
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
        /// When the <paramref name="startTime"/> or <paramref name="stopTime"/> temporal processing constraints are defined (i.e., not <c>null</c>), this
        /// specifies the start and stop time over which the subscriber session will process data. Passing in <c>null</c> for the <paramref name="startTime"/>
        /// and <paramref name="stopTime"/> specifies the subscriber session will process data in standard, i.e., real-time, operation.
        /// </para>
        /// <para>
        /// With the exception of the values of -1 and 0, the <paramref name="processingInterval"/> value specifies the desired historical playback data
        /// processing interval in milliseconds. This is basically a delay, or timer interval, over which to process data. Setting this value to -1 means
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
        public virtual bool UnsynchronizedSubscribe(bool compactFormat, bool throttled, string filterExpression, string dataChannel = null, bool includeTime = true, double lagTime = 10.0D, double leadTime = 5.0D, bool useLocalClockAsRealTime = false, string startTime = null, string stopTime = null, string constraintParameters = null, int processingInterval = -1, string waitHandleNames = null, int waitHandleTimeout = 0)
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
            connectionString.AppendFormat("requestNaNValueFilter={0}; ", m_requestNaNValueFilter);
            connectionString.AppendFormat("assemblyInfo={{source={0}; version={1}.{2}.{3}; buildDate={4}}}", assemblyInfo.Name, assemblyInfo.Version.Major, assemblyInfo.Version.Minor, assemblyInfo.Version.Build, assemblyInfo.BuildDate.ToString("yyyy-MM-dd HH:mm:ss"));

            if (!string.IsNullOrWhiteSpace(waitHandleNames))
            {
                connectionString.AppendFormat("; waitHandleNames={0}", waitHandleNames);
                connectionString.AppendFormat("; waitHandleTimeout={0}", waitHandleTimeout);
            }

            // Make sure not to monitor for data loss any faster than down-sample time on throttled connections - additionally
            // you will want to make sure data stream monitor is twice lag-time to allow time for initial points to arrive.
            if (throttled && (object)m_dataStreamMonitor != null && m_dataStreamMonitor.Interval / 1000.0D < lagTime)
                m_dataStreamMonitor.Interval = (int)(2.0D * lagTime * 1000.0D);

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

            if (!string.IsNullOrWhiteSpace(connectionString))
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

                    if (!string.IsNullOrWhiteSpace(setting))
                    {
                        dataChannel = new UdpClient(setting);

                        dataChannel.ReceiveBufferSize = ushort.MaxValue;
                        dataChannel.MaxConnectionAttempts = -1;
                        dataChannel.ConnectAsync();
                    }

                    // Assign data channel client reference and attach to needed events
                    DataChannel = dataChannel;

                    // Setup subscription packet
                    using (BlockAllocatedMemoryStream buffer = new BlockAllocatedMemoryStream())
                    {
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
                        buffer.Write(BigEndian.GetBytes(bytes.Length), 0, 4);

                        // Encode connection string into buffer
                        buffer.Write(bytes, 0, bytes.Length);

                        // Cache subscribed synchronization state
                        m_synchronizedSubscription = remotelySynchronized;

                        // Send subscribe server command with associated command buffer
                        success = SendServerCommand(ServerCommand.Subscribe, buffer.ToArray());
                    }
                }
                catch (Exception ex)
                {
                    OnProcessException(new InvalidOperationException("Exception occurred while trying to make publisher subscription: " + ex.Message, ex));
                }
            }
            else
                OnProcessException(new InvalidOperationException("Cannot make publisher subscription without a connection string."));

            // Reset decompressor on successful resubscription
            if (success && (object)m_decompressionBlock != null)
                m_decompressionBlock.Reset();

            return success;
        }

        /// <summary>
        /// Unsubscribes from a data publisher.
        /// </summary>
        /// <returns><c>true</c> if unsubscribe transmission was successful; otherwise <c>false</c>.</returns>
        public virtual bool Unsubscribe()
        {
            // Send unsubscribe server command
            return SendServerCommand(ServerCommand.Unsubscribe);
        }

        /// <summary>
        /// Returns the measurements signal IDs that were authorized after the last successful subscription request.
        /// </summary>
        [AdapterCommand("Gets authorized signal IDs from last subscription request.", "Administrator", "Editor", "Viewer")]
        public virtual Guid[] GetAuthorizedSignalIDs()
        {
            if ((object)m_signalIndexCache != null)
                return m_signalIndexCache.AuthorizedSignalIDs;

            return new Guid[0];
        }

        /// <summary>
        /// Returns the measurements signal IDs that were unauthorized after the last successful subscription request.
        /// </summary>
        [AdapterCommand("Gets unauthorized signal IDs from last subscription request.", "Administrator", "Editor", "Viewer")]
        public virtual Guid[] GetUnauthorizedSignalIDs()
        {
            if ((object)m_signalIndexCache != null)
                return m_signalIndexCache.UnauthorizedSignalIDs;

            return new Guid[0];
        }

        /// <summary>
        /// Resets the counters for the lifetime statistics without interrupting the adapter's operations.
        /// </summary>
        [AdapterCommand("Resets the counters for the lifetime statistics without interrupting the adapter's operations.", "Administrator", "Editor")]
        public virtual void ResetLifetimeCounters()
        {
            m_lifetimeMeasurements = 0L;
            m_totalBytesReceived = 0L;
            m_lifetimeTotalLatency = 0L;
            m_lifetimeMinimumLatency = 0L;
            m_lifetimeMaximumLatency = 0L;
            m_lifetimeLatencyMeasurements = 0L;
        }

        /// <summary>
        /// Initiate a meta-data refresh.
        /// </summary>
        [AdapterCommand("Initiates a meta-data refresh.", "Administrator", "Editor")]
        public virtual void RefreshMetadata()
        {
            SendServerCommand(ServerCommand.MetaDataRefresh, m_metadataFilters);
        }

        /// <summary>
        /// Gets the status of the temporal <see cref="DataSubscriber"/> used by the data gap recovery module.
        /// </summary>
        /// <returns>Status of the temporal <see cref="DataSubscriber"/> used by the data gap recovery module.</returns>
        [AdapterCommand("Gets the status of the temporal subscription used by the data gap recovery module.", "Administrator", "Editor", "Viewer")]
        public virtual string GetDataGapRecoverySubscriptionStatus()
        {
            if (m_dataGapRecoveryEnabled && (object)m_dataGapRecoverer != null)
                return m_dataGapRecoverer.TemporalSubscriptionStatus;

            return "Data gap recovery not enabled";
        }

        /// <summary>
        /// Spawn meta-data synchronization.
        /// </summary>
        /// <param name="metadata"><see cref="DataSet"/> to use for synchronization.</param>
        /// <remarks>
        /// This method makes sure only one meta-data synchronization happens at a time.
        /// </remarks>
        public void SynchronizeMetadata(DataSet metadata)
        {
            try
            {
                m_receivedMetadata = metadata;
                m_synchronizeMetadataOperation.RunOnceAsync();
            }
            catch (Exception ex)
            {
                // Process exception for logging
                OnProcessException(new InvalidOperationException("Failed to queue meta-data synchronization: " + ex.Message, ex));
            }
        }

        /// <summary>
        /// Sends a server command to the publisher connection with associated <paramref name="message"/> data.
        /// </summary>
        /// <param name="commandCode"><see cref="ServerCommand"/> to send.</param>
        /// <param name="message">String based command data to send to server.</param>
        /// <returns><c>true</c> if <paramref name="commandCode"/> transmission was successful; otherwise <c>false</c>.</returns>
        public virtual bool SendServerCommand(ServerCommand commandCode, string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                using (BlockAllocatedMemoryStream buffer = new BlockAllocatedMemoryStream())
                {
                    byte[] bytes = m_encoding.GetBytes(message);

                    buffer.Write(BigEndian.GetBytes(bytes.Length), 0, 4);
                    buffer.Write(bytes, 0, bytes.Length);

                    return SendServerCommand(commandCode, buffer.ToArray());
                }
            }

            return SendServerCommand(commandCode);
        }

        /// <summary>
        /// Sends a server command to the publisher connection.
        /// </summary>
        /// <param name="commandCode"><see cref="ServerCommand"/> to send.</param>
        /// <param name="data">Optional command data to send.</param>
        /// <returns><c>true</c> if <paramref name="commandCode"/> transmission was successful; otherwise <c>false</c>.</returns>
        public virtual bool SendServerCommand(ServerCommand commandCode, byte[] data = null)
        {
            if ((object)m_commandChannel != null && m_commandChannel.CurrentState == ClientState.Connected)
            {
                try
                {
                    using (BlockAllocatedMemoryStream commandPacket = new BlockAllocatedMemoryStream())
                    {
                        // Write command code into command packet
                        commandPacket.WriteByte((byte)commandCode);

                        // Write command buffer into command packet
                        if ((object)data != null && data.Length > 0)
                            commandPacket.Write(data, 0, data.Length);

                        // Send command packet to publisher
                        m_commandChannel.SendAsync(commandPacket.ToArray(), 0, (int)commandPacket.Length);
                        m_metadataRefreshPending = (commandCode == ServerCommand.MetaDataRefresh);
                    }

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
                    OnProcessException(new InvalidOperationException($"Exception occurred while trying to send server command \"{commandCode}\" to publisher: {ex.Message}", ex));
                }
            }
            else
                OnProcessException(new InvalidOperationException($"Subscriber is currently unconnected. Cannot send server command \"{commandCode}\" to publisher."));

            return false;
        }

        /// <summary>
        /// Attempts to connect to this <see cref="DataSubscriber"/>.
        /// </summary>
        protected override void AttemptConnection()
        {
            long now = m_useLocalClockAsRealTime ? DateTime.UtcNow.Ticks : 0L;
            List<DeviceStatisticsHelper<SubscribedDevice>> statisticsHelpers = m_statisticsHelpers;

            m_registerStatisticsOperation.RunOnceAsync();
            m_expectedBufferBlockSequenceNumber = 0u;
            m_commandChannelConnectionAttempts = 0;
            m_dataChannelConnectionAttempts = 0;

            m_authenticated = (m_securityMode == SecurityMode.TLS);
            m_subscribed = false;
            m_keyIVs = null;
            m_totalBytesReceived = 0L;
            m_monitoredBytesReceived = 0L;
            m_lastBytesReceived = 0;

            m_commandChannel.ConnectAsync();

            if (m_useLocalClockAsRealTime && (object)m_subscribedDevicesTimer == null)
            {
                m_subscribedDevicesTimer = Common.TimerScheduler.CreateTimer(1000);
                m_subscribedDevicesTimer.Elapsed += SubscribedDevicesTimer_Elapsed;
            }

            if ((object)statisticsHelpers != null)
            {
                m_realTime = 0L;
                m_lastStatisticsHelperUpdate = 0L;

                foreach (DeviceStatisticsHelper<SubscribedDevice> statisticsHelper in statisticsHelpers)
                    statisticsHelper.Reset(now);
            }

            if (m_useLocalClockAsRealTime)
                m_subscribedDevicesTimer.Start();
        }

        /// <summary>
        /// Attempts to disconnect from this <see cref="DataSubscriber"/>.
        /// </summary>
        protected override void AttemptDisconnection()
        {
            // Unregister device statistics
            m_registerStatisticsOperation.RunOnceAsync();

            // Stop data stream monitor
            if ((object)m_dataStreamMonitor != null)
                m_dataStreamMonitor.Enabled = false;

            // Disconnect command channel
            if ((object)m_commandChannel != null)
                m_commandChannel.Disconnect();

            if ((object)m_subscribedDevicesTimer != null)
                m_subscribedDevicesTimer.Stop();

            m_metadataRefreshPending = false;
        }

        /// <summary>
        /// Gets a short one-line status of this <see cref="DataSubscriber"/>.
        /// </summary>
        /// <param name="maxLength">Maximum length of the status message.</param>
        /// <returns>Text of the status message.</returns>
        public override string GetShortStatus(int maxLength)
        {
            if ((object)m_commandChannel != null && m_commandChannel.CurrentState == ClientState.Connected)
                return $"Subscriber is connected and receiving {(m_synchronizedSubscription ? "synchronized" : "unsynchronized")} data points".CenterText(maxLength);

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
                    Dictionary<Guid, DeviceStatisticsHelper<SubscribedDevice>> subscribedDevicesLookup;
                    DeviceStatisticsHelper<SubscribedDevice> statisticsHelper;

                    ServerResponse responseCode = (ServerResponse)buffer[0];
                    ServerCommand commandCode = (ServerCommand)buffer[1];
                    int responseLength = BigEndian.ToInt32(buffer, 2);
                    int responseIndex = DataPublisher.ClientResponseHeaderSize;
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

                    if (!IsUserCommand(commandCode))
                        OnReceivedServerResponse(responseCode, commandCode);
                    else
                        OnReceivedUserCommandResponse(commandCode, responseCode, solicited, buffer, responseIndex, length);

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
                                        m_metadataRefreshPending = false;
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
                                        m_metadataRefreshPending = false;
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

                            if (commandCode == ServerCommand.MetaDataRefresh)
                                m_metadataRefreshPending = false;
                            break;
                        case ServerResponse.DataPacket:
                            long now = DateTime.UtcNow.Ticks;

                            // Deserialize data packet
                            List<IMeasurement> measurements = new List<IMeasurement>();
                            DataPacketFlags flags;
                            Ticks timestamp = 0;
                            int count;

                            if (m_totalBytesReceived == 0)
                            {
                                // At the point when data is being received, data monitor should be enabled
                                if ((object)m_dataStreamMonitor != null && !m_dataStreamMonitor.Enabled)
                                    m_dataStreamMonitor.Enabled = true;

                                // Establish run-time log for subscriber
                                if (m_autoConnect || m_dataGapRecoveryEnabled)
                                {
                                    if ((object)m_runTimeLog == null)
                                    {
                                        m_runTimeLog = new RunTimeLog();
                                        m_runTimeLog.FileName = GetLoggingPath(Name + "_RunTimeLog.txt");
                                        m_runTimeLog.ProcessException += m_runTimeLog_ProcessException;
                                        m_runTimeLog.Initialize();
                                    }
                                    else
                                    {
                                        // Mark the start of any data transmissions
                                        m_runTimeLog.StartTime = DateTimeOffset.UtcNow;
                                        m_runTimeLog.Enabled = true;
                                    }
                                }

                                // The duration between last disconnection and start of data transmissions
                                // represents a gap in data - if data gap recovery is enabled, we log
                                // this as a gap for recovery:
                                if (m_dataGapRecoveryEnabled && (object)m_dataGapRecoverer != null)
                                    m_dataGapRecoverer.LogDataGap(m_runTimeLog.StopTime, DateTimeOffset.UtcNow);
                            }

                            // Track total data packet bytes received from any channel
                            m_totalBytesReceived += m_lastBytesReceived;
                            m_monitoredBytesReceived += m_lastBytesReceived;

                            // Get data packet flags
                            flags = (DataPacketFlags)buffer[responseIndex];
                            responseIndex++;

                            bool synchronizedMeasurements = ((byte)(flags & DataPacketFlags.Synchronized) > 0);
                            bool compactMeasurementFormat = ((byte)(flags & DataPacketFlags.Compact) > 0);
                            bool compressedPayload = ((byte)(flags & DataPacketFlags.Compressed) > 0);
                            int cipherIndex = (flags & DataPacketFlags.CipherIndex) > 0 ? 1 : 0;

                            // Decrypt data packet payload if keys are available
                            if ((object)m_keyIVs != null)
                            {
                                // Get a local copy of volatile keyIVs reference since this can change at any time
                                keyIVs = m_keyIVs;

                                // Decrypt payload portion of data packet
                                buffer = Common.SymmetricAlgorithm.Decrypt(buffer, responseIndex, responseLength - 1, keyIVs[cipherIndex][0], keyIVs[cipherIndex][1]);
                                responseIndex = 0;
                                responseLength = buffer.Length;
                            }

                            // Synchronized packets contain a frame level timestamp
                            if (synchronizedMeasurements)
                            {
                                timestamp = BigEndian.ToInt64(buffer, responseIndex);
                                responseIndex += 8;
                            }

                            // Deserialize number of measurements that follow
                            count = BigEndian.ToInt32(buffer, responseIndex);
                            responseIndex += 4;

                            if (compressedPayload)
                            {
                                if ((object)m_signalIndexCache == null && m_lastMissingCacheWarning + MissingCacheWarningInterval < now)
                                {
                                    if (m_lastMissingCacheWarning != 0L)
                                    {
                                        // Warning message for missing signal index cache
                                        OnStatusMessage("WARNING: Signal index cache has not arrived. No compact measurements can be parsed.");
                                    }

                                    m_lastMissingCacheWarning = now;
                                }
                                else
                                {
                                    try
                                    {
                                        if (CompressionModes.HasFlag(CompressionModes.TSSC))
                                        {
                                            // Use TSSC compression to decompress measurements                                            
                                            if ((object)m_decompressionBlock == null)
                                                m_decompressionBlock = new MeasurementDecompressionBlock();

                                            MemoryStream bufferStream = new MemoryStream(buffer, responseIndex, responseLength - responseIndex + DataPublisher.ClientResponseHeaderSize);
                                            bool eos = false;

                                            while (!eos)
                                            {
                                                Measurement measurement;
                                                Tuple<Guid, string, uint> tuple;
                                                ushort id;
                                                long time;
                                                uint quality;
                                                float value;
                                                byte command;

                                                switch (m_decompressionBlock.GetMeasurement(out id, out time, out quality, out value, out command))
                                                {
                                                    case DecompressionExitCode.EndOfStreamOccured:
                                                        if (bufferStream.Position != bufferStream.Length)
                                                            m_decompressionBlock.Fill(bufferStream);
                                                        else
                                                            eos = true;
                                                        break;
                                                    case DecompressionExitCode.CommandRead:
                                                        break;
                                                    case DecompressionExitCode.MeasurementRead:
                                                        // Attempt to restore signal identification
                                                        if (m_signalIndexCache.Reference.TryGetValue(id, out tuple))
                                                        {
                                                            measurement = new Measurement();
                                                            measurement.CommonMeasurementFields = MeasurementKey.LookUpOrCreate(tuple.Item1, tuple.Item2, tuple.Item3).DataSourceCommonValues;
                                                            measurement.Timestamp = time;
                                                            measurement.StateFlags = (MeasurementStateFlags)quality;
                                                            measurement.Value = value;
                                                            measurements.Add(measurement);
                                                        }
                                                        break;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // Decompress compact measurements from payload
                                            measurements.AddRange(buffer.DecompressPayload(m_signalIndexCache, responseIndex, responseLength - responseIndex + DataPublisher.ClientResponseHeaderSize, count, m_includeTime, flags));
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        OnProcessException(new InvalidOperationException("WARNING: Decompression failure: " + ex.Message, ex));
                                    }
                                }
                            }
                            else
                            {
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
                            }

                            // Calculate statistics
                            subscribedDevicesLookup = m_subscribedDevicesLookup;
                            statisticsHelper = null;

                            if ((object)subscribedDevicesLookup != null)
                            {
                                IEnumerable<IGrouping<DeviceStatisticsHelper<SubscribedDevice>, IMeasurement>> deviceGroups = measurements
                                    .Where(measurement => subscribedDevicesLookup.TryGetValue(measurement.ID, out statisticsHelper))
                                    .Select(measurement => Tuple.Create(statisticsHelper, measurement))
                                    .ToList()
                                    .GroupBy(tuple => tuple.Item1, tuple => tuple.Item2);

                                foreach (IGrouping<DeviceStatisticsHelper<SubscribedDevice>, IMeasurement> deviceGroup in deviceGroups)
                                {
                                    statisticsHelper = deviceGroup.Key;

                                    foreach (IGrouping<Ticks, IMeasurement> frame in deviceGroup.GroupBy(measurement => measurement.Timestamp))
                                    {
                                        // Determine the number of measurements received with valid values
                                        int measurementsReceived = frame.Count(measurement => !double.IsNaN(measurement.Value));

                                        IMeasurement statusFlags = null;
                                        IMeasurement frequency = null;
                                        IMeasurement deltaFrequency = null;

                                        // Attempt to update real-time
                                        if (!m_useLocalClockAsRealTime && frame.Key > m_realTime)
                                            m_realTime = frame.Key;

                                        // Search the frame for status flags, frequency, and delta frequency
                                        foreach (IMeasurement measurement in frame)
                                        {
                                            if (measurement.ID == statisticsHelper.Device.StatusFlagsID)
                                                statusFlags = measurement;
                                            else if (measurement.ID == statisticsHelper.Device.FrequencyID)
                                                frequency = measurement;
                                            else if (measurement.ID == statisticsHelper.Device.DeltaFrequencyID)
                                                deltaFrequency = measurement;
                                        }

                                        // If we are receiving status flags for this device,
                                        // count the data quality, time quality, and device errors
                                        if ((object)statusFlags != null)
                                        {
                                            uint commonStatusFlags = (uint)statusFlags.Value;

                                            if ((commonStatusFlags & (uint)Bits.Bit19) > 0)
                                                statisticsHelper.Device.DataQualityErrors++;

                                            if ((commonStatusFlags & (uint)Bits.Bit18) > 0)
                                                statisticsHelper.Device.TimeQualityErrors++;

                                            if ((commonStatusFlags & (uint)Bits.Bit16) > 0)
                                                statisticsHelper.Device.DeviceErrors++;

                                            measurementsReceived--;
                                        }

                                        // Zero is not a valid value for frequency.
                                        // If frequency is zero, invalidate both frequency and delta frequency
                                        if ((object)frequency != null && frequency.Value == 0.0D)
                                        {
                                            if ((object)deltaFrequency != null)
                                                measurementsReceived -= 2;
                                            else
                                                measurementsReceived--;
                                        }

                                        // Track the number of measurements received
                                        statisticsHelper.AddToMeasurementsReceived(measurementsReceived);
                                    }
                                }
                            }

                            // Provide new measurements to local concentrator, if defined, otherwise directly expose them to the consumer
                            if ((object)m_localConcentrator != null)
                                m_localConcentrator.SortMeasurements(measurements);
                            else
                                OnNewMeasurements(measurements);

                            // Gather statistics on received data
                            DateTime timeReceived = RealTime;

                            if (!m_useLocalClockAsRealTime && timeReceived.Ticks - m_lastStatisticsHelperUpdate > Ticks.PerSecond)
                            {
                                UpdateStatisticsHelpers();
                                m_lastStatisticsHelperUpdate = m_realTime;
                            }

                            m_lifetimeMeasurements += measurements.Count;
                            UpdateMeasurementsPerSecond(timeReceived, measurements.Count);

                            for (int x = 0; x < measurements.Count; x++)
                            {
                                long latency = timeReceived.Ticks - (long)measurements[x].Timestamp;

                                // Throw out latencies that exceed one hour as invalid
                                if (Math.Abs(latency) > Time.SecondsPerHour * Ticks.PerSecond)
                                    continue;

                                if (m_lifetimeMinimumLatency > latency || m_lifetimeMinimumLatency == 0)
                                    m_lifetimeMinimumLatency = latency;

                                if (m_lifetimeMaximumLatency < latency || m_lifetimeMaximumLatency == 0)
                                    m_lifetimeMaximumLatency = latency;

                                m_lifetimeTotalLatency += latency;
                                m_lifetimeLatencyMeasurements++;
                            }
                            break;
                        case ServerResponse.BufferBlock:
                            // Buffer block received - wrap as a buffer block measurement and expose back to consumer
                            uint sequenceNumber = BigEndian.ToUInt32(buffer, responseIndex);
                            int cacheIndex = (int)(sequenceNumber - m_expectedBufferBlockSequenceNumber);
                            BufferBlockMeasurement bufferBlockMeasurement;
                            Tuple<Guid, string, uint> measurementKey;
                            ushort signalIndex;

                            // Check if this buffer block has already been processed (e.g., mistaken retransmission due to timeout)
                            if (cacheIndex >= 0 && (cacheIndex >= m_bufferBlockCache.Count || (object)m_bufferBlockCache[cacheIndex] == null))
                            {
                                // Send confirmation that buffer block is received
                                SendServerCommand(ServerCommand.ConfirmBufferBlock, buffer.BlockCopy(responseIndex, 4));

                                // Get measurement key from signal index cache
                                signalIndex = BigEndian.ToUInt16(buffer, responseIndex + 4);

                                if (!m_signalIndexCache.Reference.TryGetValue(signalIndex, out measurementKey))
                                    throw new InvalidOperationException("Failed to find associated signal identification for runtime ID " + signalIndex);

                                // Skip the sequence number and signal index when creating the buffer block measurement
                                bufferBlockMeasurement = new BufferBlockMeasurement(buffer, responseIndex + 6, responseLength - 6)
                                {
                                    CommonMeasurementFields = MeasurementKey.LookUpOrCreate(measurementKey.Item1, measurementKey.Item2, measurementKey.Item3).DataSourceCommonValues
                                };

                                // Determine if this is the next buffer block in the sequence
                                if (sequenceNumber == m_expectedBufferBlockSequenceNumber)
                                {
                                    List<IMeasurement> bufferBlockMeasurements = new List<IMeasurement>();
                                    int i;

                                    // Add the buffer block measurement to the list of measurements to be published
                                    bufferBlockMeasurements.Add(bufferBlockMeasurement);
                                    m_expectedBufferBlockSequenceNumber++;

                                    // Add cached buffer block measurements to the list of measurements to be published
                                    for (i = 1; i < m_bufferBlockCache.Count; i++)
                                    {
                                        if ((object)m_bufferBlockCache[i] == null)
                                            break;

                                        bufferBlockMeasurements.Add(m_bufferBlockCache[i]);
                                        m_expectedBufferBlockSequenceNumber++;
                                    }

                                    // Remove published measurements from the buffer block queue
                                    if (m_bufferBlockCache.Count > 0)
                                        m_bufferBlockCache.RemoveRange(0, i);

                                    // Publish measurements
                                    OnNewMeasurements(bufferBlockMeasurements);
                                }
                                else
                                {
                                    // Ensure that the list has at least as many
                                    // elements as it needs to cache this measurement
                                    for (int i = m_bufferBlockCache.Count; i <= cacheIndex; i++)
                                        m_bufferBlockCache.Add(null);

                                    // Insert this buffer block into the proper location in the list
                                    m_bufferBlockCache[cacheIndex] = bufferBlockMeasurement;
                                }
                            }

                            m_lifetimeMeasurements += 1;
                            UpdateMeasurementsPerSecond(DateTime.UtcNow, 1);
                            break;
                        case ServerResponse.DataStartTime:
                            // Raise data start time event
                            OnDataStartTime(BigEndian.ToInt64(buffer, responseIndex));
                            break;
                        case ServerResponse.ProcessingComplete:
                            // Raise input processing completed event
                            OnProcessingComplete(InterpretResponseMessage(buffer, responseIndex, responseLength));
                            break;
                        case ServerResponse.UpdateSignalIndexCache:
                            // Deserialize new signal index cache
                            m_remoteSignalIndexCache = DeserializeSignalIndexCache(buffer.BlockCopy(responseIndex, responseLength));
                            m_signalIndexCache = new SignalIndexCache(DataSource, m_remoteSignalIndexCache);
                            FixExpectedMeasurementCounts();
                            break;
                        case ServerResponse.UpdateBaseTimes:
                            // Get active time index
                            m_timeIndex = BigEndian.ToInt32(buffer, responseIndex);
                            responseIndex += 4;

                            // Deserialize new base time offsets
                            m_baseTimeOffsets = new[] { BigEndian.ToInt64(buffer, responseIndex), BigEndian.ToInt64(buffer, responseIndex + 8) };
                            break;
                        case ServerResponse.UpdateCipherKeys:
                            // Move past active cipher index (not currently used anywhere else)
                            responseIndex++;

                            // Extract remaining response
                            byte[] bytes = buffer.BlockCopy(responseIndex, responseLength - 1);

                            // Decrypt response payload if subscription is authenticated
                            if (m_authenticated)
                                bytes = bytes.Decrypt(m_sharedSecret, CipherStrength.Aes256);

                            // Deserialize new cipher keys
                            keyIVs = new byte[2][][];
                            keyIVs[EvenKey] = new byte[2][];
                            keyIVs[OddKey] = new byte[2][];

                            int index = 0;
                            int bufferLen;

                            // Read even key size
                            bufferLen = BigEndian.ToInt32(bytes, index);
                            index += 4;

                            // Read even key
                            keyIVs[EvenKey][KeyIndex] = new byte[bufferLen];
                            Buffer.BlockCopy(bytes, index, keyIVs[EvenKey][KeyIndex], 0, bufferLen);
                            index += bufferLen;

                            // Read even initialization vector size
                            bufferLen = BigEndian.ToInt32(bytes, index);
                            index += 4;

                            // Read even initialization vector
                            keyIVs[EvenKey][IVIndex] = new byte[bufferLen];
                            Buffer.BlockCopy(bytes, index, keyIVs[EvenKey][IVIndex], 0, bufferLen);
                            index += bufferLen;

                            // Read odd key size
                            bufferLen = BigEndian.ToInt32(bytes, index);
                            index += 4;

                            // Read odd key
                            keyIVs[OddKey][KeyIndex] = new byte[bufferLen];
                            Buffer.BlockCopy(bytes, index, keyIVs[OddKey][KeyIndex], 0, bufferLen);
                            index += bufferLen;

                            // Read odd initialization vector size
                            bufferLen = BigEndian.ToInt32(bytes, index);
                            index += 4;

                            // Read odd initialization vector
                            keyIVs[OddKey][IVIndex] = new byte[bufferLen];
                            Buffer.BlockCopy(bytes, index, keyIVs[OddKey][IVIndex], 0, bufferLen);
                            //index += bufferLen;

                            // Exchange keys
                            m_keyIVs = keyIVs;

                            OnStatusMessage("Successfully established new cipher keys for data packet transmissions.");
                            break;
                        case ServerResponse.Notify:
                            // Skip the 4-byte hash
                            string message = m_encoding.GetString(buffer, responseIndex + 4, responseLength - 4);

                            // Display notification
                            OnStatusMessage("NOTIFICATION: {0}", message);
                            OnNotificationReceived(message);

                            // Send confirmation of receipt of the notification
                            SendServerCommand(ServerCommand.ConfirmNotification, buffer.BlockCopy(responseIndex, 4));
                            break;
                        case ServerResponse.ConfigurationChanged:
                            OnStatusMessage("Received notification from publisher that configuration has changed.");
                            OnServerConfigurationChanged();

                            // Initiate meta-data refresh when publisher configuration has changed - we only do this
                            // for automatic connections since API style connections have to manually initiate a
                            // meta-data refresh. API style connection should attach to server configuration changed
                            // event and request meta-data refresh to complete automated cycle.
                            if (m_autoConnect && m_autoSynchronizeMetadata)
                                SendServerCommand(ServerCommand.MetaDataRefresh, m_metadataFilters);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    OnProcessException(new InvalidOperationException("Failed to process publisher response packet due to exception: " + ex.Message, ex));
                }
            }
        }

        private bool IsUserCommand(ServerCommand command)
        {
            ServerCommand[] userCommands =
            {
                ServerCommand.UserCommand00,
                ServerCommand.UserCommand01,
                ServerCommand.UserCommand02,
                ServerCommand.UserCommand03,
                ServerCommand.UserCommand04,
                ServerCommand.UserCommand05,
                ServerCommand.UserCommand06,
                ServerCommand.UserCommand07,
                ServerCommand.UserCommand08,
                ServerCommand.UserCommand09,
                ServerCommand.UserCommand10,
                ServerCommand.UserCommand11,
                ServerCommand.UserCommand12,
                ServerCommand.UserCommand13,
                ServerCommand.UserCommand14,
                ServerCommand.UserCommand15
            };

            return userCommands.Contains(command);
        }

        // Handles auto-connection subscription initialization
        private void StartSubscription()
        {
            SubscribeToOutputMeasurements(!m_autoSynchronizeMetadata);

            // Initiate meta-data refresh
            if (m_autoSynchronizeMetadata)
                SendServerCommand(ServerCommand.MetaDataRefresh, m_metadataFilters);
        }

        private void SubscribeToOutputMeasurements(bool metaDataRefreshCompleted)
        {
            StringBuilder filterExpression = new StringBuilder();
            string dataChannel = null;

            // If TCP command channel is defined separately, then base connection string defines data channel
            if (Settings.ContainsKey("commandChannel"))
                dataChannel = ConnectionString;

            if ((object)OutputMeasurements != null && OutputMeasurements.Length > 0)
            {
                foreach (IMeasurement measurement in OutputMeasurements)
                {
                    if (filterExpression.Length > 0)
                        filterExpression.Append(';');

                    // Subscribe by associated Guid...
                    filterExpression.Append(measurement.ID);
                }

                // Start unsynchronized subscription
#pragma warning disable 0618
                UnsynchronizedSubscribe(true, false, filterExpression.ToString(), dataChannel);
            }
            else if (metaDataRefreshCompleted)
            {
                OnStatusMessage("WARNING: No measurements are currently defined for subscription.");
            }
        }

        /// <summary>
        /// Handles meta-data synchronization to local system.
        /// </summary>
        /// <remarks>
        /// This function should only be initiated from call to <see cref="SynchronizeMetadata(DataSet)"/> to make
        /// sure only one meta-data synchronization happens at once. Users can override this method to customize
        /// process of meta-data synchronization.
        /// </remarks>
        protected virtual void SynchronizeMetadata()
        {
            bool dataMonitoringEnabled = false;

            // TODO: This function is complex and very closely tied to the current time-series data schema - perhaps it should be moved outside this class and referenced
            // TODO: as a delegate that can be assigned and called to allow other schemas as well. DataPublisher is already very flexible in what data it can deliver.
            try
            {
                DataSet metadata = m_receivedMetadata;

                // Only perform database synchronization if meta-data has changed since last update
                if (!SynchronizedMetadataChanged(metadata))
                    return;

                if ((object)metadata == null)
                {
                    OnStatusMessage("WARNING: Meta-data synchronization was not performed, deserialized dataset was empty.");
                    return;
                }

                // Reset data stream monitor while meta-data synchronization is in progress
                if ((object)m_dataStreamMonitor != null && m_dataStreamMonitor.Enabled)
                {
                    m_dataStreamMonitor.Enabled = false;
                    dataMonitoringEnabled = true;
                }

                // Track total meta-data synchronization process time
                Ticks startTime = DateTime.UtcNow.Ticks;
                DateTime updateTime;
                DateTime latestUpdateTime = DateTime.MinValue;

                // Open the configuration database using settings found in the config file
                using (AdoDataConnection database = new AdoDataConnection("systemSettings"))
                using (IDbCommand command = database.Connection.CreateCommand())
                {
                    IDbTransaction transaction = null;

                    if (m_useTransactionForMetadata)
                        transaction = database.Connection.BeginTransaction(database.DefaultIsloationLevel);

                    try
                    {
                        if ((object)transaction != null)
                            command.Transaction = transaction;

                        // Query the actual record ID based on the known run-time ID for this subscriber device
                        int parentID = Convert.ToInt32(command.ExecuteScalar($"SELECT SourceID FROM Runtime WHERE ID = {ID} AND SourceTable='Device'", m_metadataSynchronizationTimeout));

                        // Validate that the subscriber device is marked as a concentrator (we are about to associate children devices with it)
                        if (!command.ExecuteScalar($"SELECT IsConcentrator FROM Device WHERE ID = {parentID}", m_metadataSynchronizationTimeout).ToString().ParseBoolean())
                            command.ExecuteNonQuery($"UPDATE Device SET IsConcentrator = 1 WHERE ID = {parentID}", m_metadataSynchronizationTimeout);

                        // Get any historian associated with the subscriber device
                        object historianID = command.ExecuteScalar($"SELECT HistorianID FROM Device WHERE ID = {parentID}", m_metadataSynchronizationTimeout);

                        // Determine the active node ID - we cache this since this value won't change for the lifetime of this class
                        if (m_nodeID == Guid.Empty)
                            m_nodeID = Guid.Parse(command.ExecuteScalar($"SELECT NodeID FROM IaonInputAdapter WHERE ID = {(int)ID}", m_metadataSynchronizationTimeout).ToString());

                        // Determine the protocol record auto-inc ID value for the gateway transport protocol (GEP) - this value is also cached since it shouldn't change for the lifetime of this class
                        if (m_gatewayProtocolID == 0)
                            m_gatewayProtocolID = int.Parse(command.ExecuteScalar("SELECT ID FROM Protocol WHERE Acronym='GatewayTransport'", m_metadataSynchronizationTimeout).ToString());

                        // Ascertain total number of actions required for all meta-data synchronization so some level feed back can be provided on progress
                        InitSyncProgress(metadata.Tables.Cast<DataTable>().Select(dataTable => (long)dataTable.Rows.Count).Sum() + 3);

                        // Prefix all children devices with the name of the parent since the same device names could appear in different connections (helps keep device names unique)
                        string sourcePrefix = Name + "!";
                        Dictionary<string, int> deviceIDs = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                        string deviceAcronym, signalTypeAcronym;
                        decimal longitude, latitude;
                        decimal? location;
                        object originalSource;
                        int deviceID;

                        // Check to see if data for the "DeviceDetail" table was included in the meta-data
                        if (metadata.Tables.Contains("DeviceDetail"))
                        {
                            DataTable deviceDetail = metadata.Tables["DeviceDetail"];
                            List<Guid> uniqueIDs = new List<Guid>();
                            DataRow[] deviceRows;

                            // Define SQL statement to query if this device is already defined (this should always be based on the unique guid-based device ID)
                            string deviceExistsSql = database.ParameterizedQueryString("SELECT COUNT(*) FROM Device WHERE UniqueID = {0}", "uniqueID");

                            // Define SQL statement to insert new device record
                            string insertDeviceSql = database.ParameterizedQueryString("INSERT INTO Device(NodeID, ParentID, HistorianID, Acronym, Name, ProtocolID, FramesPerSecond, OriginalSource, AccessID, Longitude, Latitude, ContactList, IsConcentrator, Enabled) " +
                                                                                       "VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, 0, 1)", "nodeID", "parentID", "historianID", "acronym", "name", "protocolID", "framesPerSecond", "originalSource", "accessID", "longitude", "latitude", "contactList");

                            // Define SQL statement to update device's guid-based unique ID after insert
                            string updateDeviceUniqueIDSql = database.ParameterizedQueryString("UPDATE Device SET UniqueID = {0} WHERE Acronym = {1}", "uniqueID", "acronym");

                            // Define SQL statement to query if a device can be safely updated
                            string deviceIsUpdatableSql = database.ParameterizedQueryString("SELECT COUNT(*) FROM Device WHERE UniqueID = {0} AND (ParentID <> {1} OR ParentID IS NULL)", "uniqueID", "parentID");

                            // Define SQL statement to update existing device record
                            string updateDeviceSql = database.ParameterizedQueryString("UPDATE Device SET Acronym = {0}, Name = {1}, OriginalSource = {2}, ProtocolID = {3}, FramesPerSecond = {4}, HistorianID = {5}, AccessID = {6}, Longitude = {7}, Latitude = {8}, ContactList = {9} WHERE UniqueID = {10}",
                                                                                       "acronym", "name", "originalSource", "protocolID", "framesPerSecond", "historianID", "accessID", "longitude", "latitude", "contactList", "uniqueID");

                            // Define SQL statement to retrieve device's auto-inc ID based on its unique guid-based ID
                            string queryDeviceIDSql = database.ParameterizedQueryString("SELECT ID FROM Device WHERE UniqueID = {0}", "uniqueID");

                            // Define SQL statement to retrieve all unique device ID's for the current parent to check for mismatches
                            string queryUniqueDeviceIDsSql = database.ParameterizedQueryString("SELECT UniqueID FROM Device WHERE ParentID = {0}", "parentID");

                            // Define SQL statement to remove device records that no longer exist in the meta-data
                            string deleteDeviceSql = database.ParameterizedQueryString("DELETE FROM Device WHERE UniqueID = {0}", "uniqueID");

                            // Determine which device rows should be synchronized based on operational mode flags
                            if (ReceiveInternalMetadata && ReceiveExternalMetadata)
                                deviceRows = deviceDetail.Select();
                            else if (ReceiveInternalMetadata)
                                deviceRows = deviceDetail.Select("OriginalSource IS NULL");
                            else if (ReceiveExternalMetadata)
                                deviceRows = deviceDetail.Select("OriginalSource IS NOT NULL");
                            else
                                deviceRows = new DataRow[0];

                            // Check existence of optional meta-data fields
                            DataColumnCollection deviceDetailColumns = deviceDetail.Columns;
                            bool accessIDFieldExists = deviceDetailColumns.Contains("AccessID");
                            bool longitudeFieldExists = deviceDetailColumns.Contains("Longitude");
                            bool latitudeFieldExists = deviceDetailColumns.Contains("Latitude");
                            bool companyAcronymFieldExists = deviceDetailColumns.Contains("CompanyAcronym");
                            bool protocolNameFieldExists = deviceDetailColumns.Contains("ProtocolName");
                            bool vendorAcronymFieldExists = deviceDetailColumns.Contains("VendorAcronym");
                            bool vendorDeviceNameFieldExists = deviceDetailColumns.Contains("VendorDeviceName");
                            bool interconnectionNameFieldExists = deviceDetailColumns.Contains("InterconnectionName");
                            bool updatedOnFieldExists = deviceDetailColumns.Contains("UpdatedOn");

                            // Older versions of GEP did not include the AccessID field, so this is treated as optional
                            int accessID = 0;

                            foreach (DataRow row in deviceRows)
                            {
                                Guid uniqueID = Guid.Parse(row.Field<object>("UniqueID").ToString());
                                bool recordNeedsUpdating;

                                // Track unique device Guids in this meta-data session, we'll need to remove any old associated devices that no longer exist
                                uniqueIDs.Add(uniqueID);

                                // Determine if record has changed since last synchronization
                                if (updatedOnFieldExists)
                                {
                                    try
                                    {
                                        updateTime = Convert.ToDateTime(row["UpdatedOn"]);
                                        recordNeedsUpdating = updateTime > m_lastMetaDataRefreshTime;

                                        if (updateTime > latestUpdateTime)
                                            latestUpdateTime = updateTime;
                                    }
                                    catch
                                    {
                                        recordNeedsUpdating = true;
                                    }
                                }
                                else
                                {
                                    recordNeedsUpdating = true;
                                }

                                // We will synchronize meta-data only if the source owns this device and it's not defined as a concentrator (these should normally be filtered by publisher - but we check just in case).
                                if (!row["IsConcentrator"].ToNonNullString("0").ParseBoolean())
                                {
                                    if (accessIDFieldExists)
                                        accessID = row.ConvertField<int>("AccessID");

                                    // Get longitude and latitude values if they are defined
                                    longitude = 0M;
                                    latitude = 0M;

                                    if (longitudeFieldExists)
                                    {
                                        location = row.ConvertNullableField<decimal>("Longitude");

                                        if (location.HasValue)
                                            longitude = location.Value;
                                    }

                                    if (latitudeFieldExists)
                                    {
                                        location = row.ConvertNullableField<decimal>("Latitude");

                                        if (location.HasValue)
                                            latitude = location.Value;
                                    }

                                    // Save any reported extraneous values from device meta-data in connection string formatted contact list - all fields are considered optional
                                    Dictionary<string, string> contactList = new Dictionary<string, string>();

                                    if (companyAcronymFieldExists)
                                        contactList["companyAcronym"] = row.Field<string>("CompanyAcronym") ?? string.Empty;

                                    if (protocolNameFieldExists)
                                        contactList["protocolName"] = row.Field<string>("ProtocolName") ?? string.Empty;

                                    if (vendorAcronymFieldExists)
                                        contactList["vendorAcronym"] = row.Field<string>("VendorAcronym") ?? string.Empty;

                                    if (vendorDeviceNameFieldExists)
                                        contactList["vendorDeviceName"] = row.Field<string>("VendorDeviceName") ?? string.Empty;

                                    if (interconnectionNameFieldExists)
                                        contactList["interconnectionName"] = row.Field<string>("InterconnectionName") ?? string.Empty;

                                    // Determine if device record already exists
                                    if (Convert.ToInt32(command.ExecuteScalar(deviceExistsSql, m_metadataSynchronizationTimeout, database.Guid(uniqueID))) == 0)
                                    {
                                        // Insert new device record
                                        command.ExecuteNonQuery(insertDeviceSql, m_metadataSynchronizationTimeout, database.Guid(m_nodeID), parentID, historianID, sourcePrefix + row.Field<string>("Acronym"), row.Field<string>("Name"), m_gatewayProtocolID, row.ConvertField<int>("FramesPerSecond"),
                                                                m_internal ? (object)DBNull.Value : string.IsNullOrEmpty(row.Field<string>("ParentAcronym")) ? sourcePrefix + row.Field<string>("Acronym") : sourcePrefix + row.Field<string>("ParentAcronym"), accessID, longitude, latitude, contactList.JoinKeyValuePairs());

                                        // Guids are normally auto-generated during insert - after insertion update the Guid so that it matches the source data. Most of the database
                                        // scripts have triggers that support properly assigning the Guid during an insert, but this code ensures the Guid will always get assigned.
                                        command.ExecuteNonQuery(updateDeviceUniqueIDSql, m_metadataSynchronizationTimeout, database.Guid(uniqueID), sourcePrefix + row.Field<string>("Acronym"));
                                    }
                                    else if (recordNeedsUpdating)
                                    {
                                        // Perform safety check to preserve device records which are not safe to overwrite
                                        if (Convert.ToInt32(command.ExecuteScalar(deviceIsUpdatableSql, m_metadataSynchronizationTimeout, database.Guid(uniqueID), parentID)) > 0)
                                            continue;

                                        // Gateway is assuming ownership of the device records when the "internal" flag is true - this means the device's measurements can be forwarded to another party. From a device record perspective,
                                        // ownership is inferred by setting 'OriginalSource' to null. When gateway doesn't own device records (i.e., the "internal" flag is false), this means the device's measurements can only be consumed
                                        // locally - from a device record perspective this means the 'OriginalSource' field is set to the acronym of the PDC or PMU that generated the source measurements. This field allows a mirrored source
                                        // restriction to be implemented later to ensure all devices in an output protocol came from the same original source connection, if desired.
                                        originalSource = m_internal ? (object)DBNull.Value : string.IsNullOrEmpty(row.Field<string>("ParentAcronym")) ? sourcePrefix + row.Field<string>("Acronym") : sourcePrefix + row.Field<string>("ParentAcronym");

                                        // Update existing device record
                                        command.ExecuteNonQuery(updateDeviceSql, m_metadataSynchronizationTimeout, sourcePrefix + row.Field<string>("Acronym"), row.Field<string>("Name"), originalSource, m_gatewayProtocolID, row.ConvertField<int>("FramesPerSecond"), historianID, accessID, longitude, latitude, contactList.JoinKeyValuePairs(), database.Guid(uniqueID));
                                    }
                                }

                                // Capture local device ID auto-inc value for measurement association
                                deviceIDs[row.Field<string>("Acronym")] = Convert.ToInt32(command.ExecuteScalar(queryDeviceIDSql, m_metadataSynchronizationTimeout, database.Guid(uniqueID)));

                                // Periodically notify user about synchronization progress
                                UpdateSyncProgress();
                            }

                            // Remove any device records associated with this subscriber that no longer exist in the meta-data
                            if (uniqueIDs.Count > 0)
                            {
                                // Sort unique ID list so that binary search can be used for quick lookups
                                uniqueIDs.Sort();

                                DataTable deviceUniqueIDs = command.RetrieveData(database.AdapterType, queryUniqueDeviceIDsSql, m_metadataSynchronizationTimeout, parentID);
                                Guid uniqueID;

                                foreach (DataRow deviceRow in deviceUniqueIDs.Rows)
                                {
                                    uniqueID = database.Guid(deviceRow, "UniqueID");

                                    // Remove any devices in the database that are associated with the parent device and do not exist in the meta-data
                                    if (uniqueIDs.BinarySearch(uniqueID) < 0)
                                        command.ExecuteNonQuery(deleteDeviceSql, m_metadataSynchronizationTimeout, database.Guid(uniqueID));
                                }
                                UpdateSyncProgress();
                            }
                        }

                        // Check to see if data for the "MeasurementDetail" table was included in the meta-data
                        if (metadata.Tables.Contains("MeasurementDetail"))
                        {
                            DataTable measurementDetail = metadata.Tables["MeasurementDetail"];
                            List<Guid> signalIDs = new List<Guid>();
                            DataRow[] measurementRows;

                            // Define SQL statement to query if this measurement is already defined (this should always be based on the unique signal ID Guid)
                            string measurementExistsSql = database.ParameterizedQueryString("SELECT COUNT(*) FROM Measurement WHERE SignalID = {0}", "signalID");

                            // Define SQL statement to insert new measurement record
                            string insertMeasurementSql = database.ParameterizedQueryString("INSERT INTO Measurement(DeviceID, HistorianID, PointTag, AlternateTag, SignalTypeID, PhasorSourceIndex, SignalReference, Description, Internal, Subscribed, Enabled) " +
                                                                                            "VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, 0, 1)", "deviceID", "historianID", "pointTag", "alternateTag", "signalTypeID", "phasorSourceIndex", "signalReference", "description", "internal");

                            // Define SQL statement to update measurement's signal ID after insert
                            string updateMeasurementSignalIDSql = database.ParameterizedQueryString("UPDATE Measurement SET SignalID = {0}, AlternateTag = NULL WHERE AlternateTag = {1}", "signalID", "alternateTag");

                            // Define SQL statement to update existing measurement record
                            string updateMeasurementSql = database.ParameterizedQueryString("UPDATE Measurement SET HistorianID = {0}, PointTag = {1}, SignalTypeID = {2}, PhasorSourceIndex = {3}, SignalReference = {4}, Description = {5}, Internal = {6} WHERE SignalID = {7}",
                                                                                            "historianID", "pointTag", "signalTypeID", "phasorSourceIndex", "signalReference", "description", "internal", "signalID");

                            // Define SQL statement to retrieve all measurement signal ID's for the current parent to check for mismatches - note that we use the ActiveMeasurements view
                            // since it associates measurements with their top-most parent runtime device ID, this allows us to easily query all measurements for the parent device
                            string queryMeasurementSignalIDsSql = database.ParameterizedQueryString("SELECT SignalID FROM ActiveMeasurement WHERE DeviceID = {0}", "deviceID");

                            // Define SQL statement to retrieve measurement's associated device ID, i.e., actual record ID, based on measurement's signal ID
                            string queryMeasurementDeviceIDSql = database.ParameterizedQueryString("SELECT DeviceID FROM Measurement WHERE SignalID = {0}", "signalID");

                            // Define SQL statement to remove device records that no longer exist in the meta-data
                            string deleteMeasurementSql = database.ParameterizedQueryString("DELETE FROM Measurement WHERE SignalID = {0}", "signalID");

                            // Load signal type ID's from local database associated with their acronym for proper signal type translation
                            Dictionary<string, int> signalTypeIDs = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                            foreach (DataRow row in command.RetrieveData(database.AdapterType, "SELECT ID, Acronym FROM SignalType").Rows)
                            {
                                signalTypeAcronym = row.Field<string>("Acronym");

                                if (!string.IsNullOrWhiteSpace(signalTypeAcronym))
                                    signalTypeIDs[signalTypeAcronym] = row.ConvertField<int>("ID");
                            }

                            // Determine which measurement rows should be synchronized based on operational mode flags
                            if (ReceiveInternalMetadata && ReceiveExternalMetadata)
                                measurementRows = measurementDetail.Select();
                            else if (ReceiveInternalMetadata)
                                measurementRows = measurementDetail.Select("Internal <> 0");
                            else if (ReceiveExternalMetadata)
                                measurementRows = measurementDetail.Select("Internal = 0");
                            else
                                measurementRows = new DataRow[0];

                            // Check existence of optional meta-data fields
                            DataColumnCollection measurementDetailColumns = measurementDetail.Columns;
                            bool phasorSourceIndexFieldExists = measurementDetailColumns.Contains("PhasorSourceIndex");
                            bool updatedOnFieldExists = measurementDetailColumns.Contains("UpdatedOn");

                            object phasorSourceIndex = DBNull.Value;

                            foreach (DataRow row in measurementRows)
                            {
                                bool recordNeedsUpdating;

                                // Determine if record has changed since last synchronization
                                if (updatedOnFieldExists)
                                {
                                    try
                                    {
                                        updateTime = Convert.ToDateTime(row["UpdatedOn"]);
                                        recordNeedsUpdating = updateTime > m_lastMetaDataRefreshTime;

                                        if (updateTime > latestUpdateTime)
                                            latestUpdateTime = updateTime;
                                    }
                                    catch
                                    {
                                        recordNeedsUpdating = true;
                                    }
                                }
                                else
                                {
                                    recordNeedsUpdating = true;
                                }

                                // Get device and signal type acronyms
                                deviceAcronym = row.Field<string>("DeviceAcronym") ?? string.Empty;
                                signalTypeAcronym = row.Field<string>("SignalAcronym") ?? string.Empty;

                                // Get phasor source index if field is defined
                                if (phasorSourceIndexFieldExists)
                                {
                                    // Using ConvertNullableField extension since publisher could use SQLite database in which case
                                    // all integers would arrive in data set as longs and need to be converted back to integers
                                    int? index = row.ConvertNullableField<int>("PhasorSourceIndex");
                                    phasorSourceIndex = index.HasValue ? (object)index.Value : (object)DBNull.Value;
                                }

                                // Make sure we have an associated device and signal type already defined for the measurement
                                if (!string.IsNullOrWhiteSpace(deviceAcronym) && deviceIDs.ContainsKey(deviceAcronym) && !string.IsNullOrWhiteSpace(signalTypeAcronym) && signalTypeIDs.ContainsKey(signalTypeAcronym))
                                {
                                    Guid signalID = Guid.Parse(row.Field<object>("SignalID").ToString());

                                    // Track unique measurement signal Guids in this meta-data session, we'll need to remove any old associated measurements that no longer exist
                                    signalIDs.Add(signalID);


                                    // Prefix the tag name with the "updated" device name
                                    string pointTag = sourcePrefix + row.Field<string>("PointTag");

                                    // Look up associated device ID (local DB auto-inc)
                                    deviceID = deviceIDs[deviceAcronym];

                                    // Determine if measurement record already exists
                                    if (Convert.ToInt32(command.ExecuteScalar(measurementExistsSql, m_metadataSynchronizationTimeout, database.Guid(signalID))) == 0)
                                    {
                                        string alternateTag = Guid.NewGuid().ToString();

                                        // Insert new measurement record
                                        command.ExecuteNonQuery(insertMeasurementSql, m_metadataSynchronizationTimeout, deviceID, historianID, pointTag, alternateTag, signalTypeIDs[signalTypeAcronym], phasorSourceIndex, sourcePrefix + row.Field<string>("SignalReference"), row.Field<string>("Description") ?? string.Empty, database.Bool(m_internal));

                                        // Guids are normally auto-generated during insert - after insertion update the Guid so that it matches the source data. Most of the database
                                        // scripts have triggers that support properly assigning the Guid during an insert, but this code ensures the Guid will always get assigned.
                                        command.ExecuteNonQuery(updateMeasurementSignalIDSql, m_metadataSynchronizationTimeout, database.Guid(signalID), alternateTag);
                                    }
                                    else if (recordNeedsUpdating)
                                    {
                                        // Update existing measurement record. Note that this update assumes that measurements will remain associated with a static source device.
                                        command.ExecuteNonQuery(updateMeasurementSql, m_metadataSynchronizationTimeout, historianID, pointTag, signalTypeIDs[signalTypeAcronym], phasorSourceIndex, sourcePrefix + row.Field<string>("SignalReference"), row.Field<string>("Description") ?? string.Empty, database.Bool(m_internal), database.Guid(signalID));
                                    }
                                }

                                // Periodically notify user about synchronization progress
                                UpdateSyncProgress();
                            }

                            // Remove any measurement records associated with existing devices in this session but no longer exist in the meta-data
                            if (signalIDs.Count > 0)
                            {
                                // Sort signal ID list so that binary search can be used for quick lookups
                                signalIDs.Sort();

                                // Query all the guid-based signal ID's for all measurement records associated with the parent device using run-time ID
                                DataTable measurementSignalIDs = command.RetrieveData(database.AdapterType, queryMeasurementSignalIDsSql, m_metadataSynchronizationTimeout, (int)ID);
                                Guid signalID;

                                // Walk through each database record and see if the measurement exists in the provided meta-data
                                foreach (DataRow measurementRow in measurementSignalIDs.Rows)
                                {
                                    signalID = database.Guid(measurementRow, "SignalID");

                                    // Remove any measurements in the database that are associated with received devices and do not exist in the meta-data
                                    if (signalIDs.BinarySearch(signalID) < 0)
                                    {
                                        // Measurement was not in the meta-data, get the measurement's actual record based ID for its associated device
                                        object measurementDeviceID = command.ExecuteScalar(queryMeasurementDeviceIDSql, m_metadataSynchronizationTimeout, database.Guid(signalID));

                                        // If the unknown measurement is directly associated with a device that exists in the meta-data it is assumed that this measurement
                                        // was removed from the publishing system and no longer exists therefore we remove it from the local measurement cache. If the user
                                        // needs custom local measurements associated with a remote device, they should be associated with the parent device only.
                                        if (measurementDeviceID != null && !(measurementDeviceID is DBNull) && deviceIDs.ContainsValue(Convert.ToInt32(measurementDeviceID)))
                                            command.ExecuteNonQuery(deleteMeasurementSql, m_metadataSynchronizationTimeout, database.Guid(signalID));
                                    }
                                }

                                UpdateSyncProgress();
                            }
                        }

                        // Check to see if data for the "PhasorDetail" table was included in the meta-data
                        if (metadata.Tables.Contains("PhasorDetail"))
                        {
                            Dictionary<int, List<int>> definedSourceIndicies = new Dictionary<int, List<int>>();

                            // Phasor data is normally only needed so that the user can properly generate a mirrored IEEE C37.118 output stream from the source data.
                            // This is necessary since, in this protocol, the phasors are described (i.e., labeled) as a unit (i.e., as a complex number) instead of
                            // as two distinct angle and magnitude measurements.

                            // Define SQL statement to query if phasor record is already defined (no Guid is defined for these simple label records)
                            string phasorExistsSql = database.ParameterizedQueryString("SELECT COUNT(*) FROM Phasor WHERE DeviceID = {0} AND SourceIndex = {1}", "deviceID", "sourceIndex");

                            // Define SQL statement to insert new phasor record
                            string insertPhasorSql = database.ParameterizedQueryString("INSERT INTO Phasor(DeviceID, Label, Type, Phase, SourceIndex) VALUES ({0}, {1}, {2}, {3}, {4})", "deviceID", "label", "type", "phase", "sourceIndex");

                            // Define SQL statement to update existing phasor record
                            string updatePhasorSql = database.ParameterizedQueryString("UPDATE Phasor SET Label = {0}, Type = {1}, Phase = {2} WHERE DeviceID = {3} AND SourceIndex = {4}", "label", "type", "phase", "deviceID", "sourceIndex");

                            // Define SQL statement to delete a phasor record
                            string deletePhasorSql = database.ParameterizedQueryString("DELETE FROM Phasor WHERE DeviceID = {0}", "deviceID");

                            foreach (DataRow row in metadata.Tables["PhasorDetail"].Rows)
                            {
                                // Get device acronym
                                deviceAcronym = row.Field<string>("DeviceAcronym") ?? string.Empty;

                                // Make sure we have an associated device already defined for the phasor record
                                if (!string.IsNullOrWhiteSpace(deviceAcronym) && deviceIDs.ContainsKey(deviceAcronym))
                                {
                                    bool recordNeedsUpdating;

                                    // Determine if record has changed since last synchronization
                                    try
                                    {
                                        updateTime = Convert.ToDateTime(row["UpdatedOn"]);
                                        recordNeedsUpdating = updateTime > m_lastMetaDataRefreshTime;

                                        if (updateTime > latestUpdateTime)
                                            latestUpdateTime = updateTime;
                                    }
                                    catch
                                    {
                                        recordNeedsUpdating = true;
                                    }

                                    deviceID = deviceIDs[deviceAcronym];

                                    // Determine if phasor record already exists
                                    if (Convert.ToInt32(command.ExecuteScalar(phasorExistsSql, m_metadataSynchronizationTimeout, deviceID, row.ConvertField<int>("SourceIndex"))) == 0)
                                    {
                                        // Insert new phasor record
                                        command.ExecuteNonQuery(insertPhasorSql, m_metadataSynchronizationTimeout, deviceID, row.Field<string>("Label") ?? "undefined", (row.Field<string>("Type") ?? "V").TruncateLeft(1), (row.Field<string>("Phase") ?? "+").TruncateLeft(1), row.ConvertField<int>("SourceIndex"));
                                    }
                                    else if (recordNeedsUpdating)
                                    {
                                        // Update existing phasor record
                                        command.ExecuteNonQuery(updatePhasorSql, m_metadataSynchronizationTimeout, row.Field<string>("Label") ?? "undefined", (row.Field<string>("Type") ?? "V").TruncateLeft(1), (row.Field<string>("Phase") ?? "+").TruncateLeft(1), deviceID, row.ConvertField<int>("SourceIndex"));
                                    }

                                    // Track defined phasors for each device
                                    definedSourceIndicies.GetOrAdd(deviceID, id => new List<int>()).Add(row.ConvertField<int>("SourceIndex"));
                                }

                                // Periodically notify user about synchronization progress
                                UpdateSyncProgress();
                            }

                            // Remove any phasor records associated with existing devices in this session but no longer exist in the meta-data
                            foreach (int id in deviceIDs.Values)
                            {
                                List<int> sourceIndicies;

                                if (definedSourceIndicies.TryGetValue(id, out sourceIndicies))
                                    command.ExecuteNonQuery(deletePhasorSql + $" AND SourceIndex NOT IN ({string.Join(",", sourceIndicies)})", m_metadataSynchronizationTimeout, id);
                                else
                                    command.ExecuteNonQuery(deletePhasorSql, m_metadataSynchronizationTimeout, id);
                            }
                        }

                        if ((object)transaction != null)
                            transaction.Commit();

                        // Update local in-memory synchronized meta-data cache
                        m_synchronizedMetadata = metadata;
                    }
                    catch (Exception ex)
                    {
                        OnProcessException(new InvalidOperationException("Failed to synchronize meta-data to local cache: " + ex.Message, ex));

                        if ((object)transaction != null)
                        {
                            try
                            {
                                transaction.Rollback();
                            }
                            catch (Exception rollbackException)
                            {
                                OnProcessException(new InvalidOperationException("Failed to roll back database transaction due to exception: " + rollbackException.Message, rollbackException));
                            }
                        }

                        return;
                    }
                    finally
                    {
                        if ((object)transaction != null)
                            transaction.Dispose();
                    }
                }

                // New signals may have been defined, take original remote signal index cache and apply changes
                if (m_remoteSignalIndexCache != null)
                    m_signalIndexCache = new SignalIndexCache(DataSource, m_remoteSignalIndexCache);

                m_lastMetaDataRefreshTime = latestUpdateTime > DateTime.MinValue ? latestUpdateTime : DateTime.UtcNow;

                OnStatusMessage("Meta-data synchronization completed successfully in {0}", (DateTime.UtcNow.Ticks - startTime).ToElapsedTimeString(2));

                // Send notification that system configuration has changed
                OnConfigurationChanged();
            }
            catch (Exception ex)
            {
                OnProcessException(new InvalidOperationException("Failed to synchronize meta-data to local cache: " + ex.Message, ex));
            }
            finally
            {
                // Restart data stream monitor after meta-data synchronization if it was originally enabled
                if (dataMonitoringEnabled && (object)m_dataStreamMonitor != null)
                    m_dataStreamMonitor.Enabled = true;
            }
        }

        private void InitSyncProgress(long totalActions)
        {
            m_syncProgressTotalActions = totalActions;
            m_syncProgressActionsCount = 0;

            // We update user on progress with 5 messages or every 15 seconds
            m_syncProgressUpdateInterval = (long)(totalActions * 0.2D);
            m_syncProgressLastMessage = DateTime.UtcNow.Ticks;
        }

        private void UpdateSyncProgress()
        {
            m_syncProgressActionsCount++;

            if (m_syncProgressActionsCount % m_syncProgressUpdateInterval == 0 || DateTime.UtcNow.Ticks - m_syncProgressLastMessage > 150000000)
            {
                OnStatusMessage("Meta-data synchronization is {0:0.0%} complete...", m_syncProgressActionsCount / (double)m_syncProgressTotalActions);
                m_syncProgressLastMessage = DateTime.UtcNow.Ticks;
            }
        }

        private SignalIndexCache DeserializeSignalIndexCache(byte[] buffer)
        {
            CompressionModes compressionModes = (CompressionModes)(m_operationalModes & OperationalModes.CompressionModeMask);
            bool useCommonSerializationFormat = (m_operationalModes & OperationalModes.UseCommonSerializationFormat) > 0;
            bool compressSignalIndexCache = (m_operationalModes & OperationalModes.CompressSignalIndexCache) > 0;

            SignalIndexCache deserializedCache;

            GZipStream inflater = null;

            if (compressSignalIndexCache && compressionModes.HasFlag(CompressionModes.GZip))
            {
                try
                {
                    using (MemoryStream compressedData = new MemoryStream(buffer))
                    {
                        inflater = new GZipStream(compressedData, CompressionMode.Decompress, true);
                        buffer = inflater.ReadStream();
                    }
                }
                finally
                {
                    if ((object)inflater != null)
                        inflater.Close();
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
                deserializedCache = Serialization.Deserialize<SignalIndexCache>(buffer, SerializationFormat.Binary);
            }

            return deserializedCache;
        }

        private DataSet DeserializeMetadata(byte[] buffer)
        {
            CompressionModes compressionModes = (CompressionModes)(m_operationalModes & OperationalModes.CompressionModeMask);
            bool useCommonSerializationFormat = (m_operationalModes & OperationalModes.UseCommonSerializationFormat) > 0;
            bool compressMetadata = (m_operationalModes & OperationalModes.CompressMetadata) > 0;
            Ticks startTime = DateTime.UtcNow.Ticks;

            DataSet deserializedMetadata;
            GZipStream inflater = null;

            if (compressMetadata && compressionModes.HasFlag(CompressionModes.GZip))
            {
                try
                {
                    // Insert compressed data into compressed buffer
                    using (MemoryStream compressedData = new MemoryStream(buffer))
                    {
                        inflater = new GZipStream(compressedData, CompressionMode.Decompress, true);
                        buffer = inflater.ReadStream();
                    }
                }
                finally
                {
                    if ((object)inflater != null)
                        inflater.Close();
                }
            }

            if (useCommonSerializationFormat)
            {
                // Copy decompressed data into encoded buffer
                using (MemoryStream encodedData = new MemoryStream(buffer))
                using (XmlTextReader xmlReader = new XmlTextReader(encodedData))
                {
                    // Read encoded data into data set as XML
                    deserializedMetadata = new DataSet();
                    deserializedMetadata.ReadXml(xmlReader, XmlReadMode.ReadSchema);
                }
            }
            else
            {
                deserializedMetadata = Serialization.Deserialize<DataSet>(buffer, SerializationFormat.Binary);
            }

            long rowCount = deserializedMetadata.Tables.Cast<DataTable>().Select(dataTable => (long)dataTable.Rows.Count).Sum();

            if (rowCount > 0)
            {
                Time elapsedTime = (DateTime.UtcNow.Ticks - startTime).ToSeconds();
                OnStatusMessage("Received a total of {0:N0} records spanning {1:N0} tables of meta-data that was {2}deserialized in {3}...", rowCount, deserializedMetadata.Tables.Count, compressMetadata ? "uncompressed and " : "", elapsedTime.ToString(2));
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
                    throw new InvalidOperationException($"Unsupported encoding detected: {operationalEncoding}");
            }

            return encoding;
        }

        // Socket exception handler
        private bool HandleSocketException(Exception ex)
        {
            SocketException socketException = ex as SocketException;

            if ((object)socketException != null)
            {
                // WSAECONNABORTED and WSAECONNRESET are common errors after a client disconnect,
                // if they happen for other reasons, make sure disconnect procedure is handled
                if (socketException.ErrorCode == 10053 || socketException.ErrorCode == 10054)
                {
                    DisconnectClient();
                    return true;
                }
            }

            if ((object)ex != null)
                HandleSocketException(ex.InnerException);

            return false;
        }

        // Disconnect client, restarting if disconnect was not intentional
        private void DisconnectClient()
        {
            // Mark end of any data transmission in run-time log
            if ((object)m_runTimeLog != null && m_runTimeLog.Enabled)
            {
                m_runTimeLog.StopTime = DateTimeOffset.UtcNow;
                m_runTimeLog.Enabled = false;
            }

            // Stop data gap recovery operations
            if (m_dataGapRecoveryEnabled && (object)m_dataGapRecoverer != null)
            {
                try
                {
                    m_dataGapRecoverer.Enabled = false;
                    m_dataGapRecoverer.FlushLogAsync();
                }
                catch (Exception ex)
                {
                    OnProcessException(new InvalidOperationException($"Exception while attempting to flush data gap recoverer log: {ex.Message}", ex));
                }
            }

            DataChannel = null;
            m_metadataRefreshPending = false;

            // If user didn't initiate disconnect, restart the connection
            if (Enabled)
                Start();
        }

        private void HandleDeviceStatisticsRegistration()
        {
            if (Enabled)
                RegisterDeviceStatistics();
            else
                UnregisterDeviceStatistics();
        }

        private void RegisterDeviceStatistics()
        {
            long now = m_useLocalClockAsRealTime ? DateTime.UtcNow.Ticks : 0L;

            Dictionary<Guid, DeviceStatisticsHelper<SubscribedDevice>> subscribedDevicesLookup;
            List<DeviceStatisticsHelper<SubscribedDevice>> subscribedDevices;
            ISet<string> subscribedDeviceNames;
            ISet<string> definedDeviceNames;

            DataSet dataSource;
            Guid signalID;

            try
            {
                dataSource = DataSource;

                if ((object)dataSource == null || !dataSource.Tables.Contains("InputStreamDevices"))
                {
                    if ((object)m_statisticsHelpers != null)
                    {
                        foreach (DeviceStatisticsHelper<SubscribedDevice> statisticsHelper in m_statisticsHelpers)
                            statisticsHelper.Device.Dispose();
                    }

                    m_statisticsHelpers = new List<DeviceStatisticsHelper<SubscribedDevice>>();
                    m_subscribedDevicesLookup = new Dictionary<Guid, DeviceStatisticsHelper<SubscribedDevice>>();
                }
                else
                {
                    subscribedDevicesLookup = new Dictionary<Guid, DeviceStatisticsHelper<SubscribedDevice>>();
                    subscribedDevices = new List<DeviceStatisticsHelper<SubscribedDevice>>();
                    subscribedDeviceNames = new HashSet<string>();
                    definedDeviceNames = new HashSet<string>();

                    foreach (DataRow deviceRow in dataSource.Tables["InputStreamDevices"].Select($"ParentID = {ID}"))
                        definedDeviceNames.Add($"LOCAL${deviceRow["Acronym"].ToNonNullString()}");

                    if ((object)m_statisticsHelpers != null)
                    {
                        foreach (DeviceStatisticsHelper<SubscribedDevice> statisticsHelper in m_statisticsHelpers)
                        {
                            if (definedDeviceNames.Contains(statisticsHelper.Device.Name))
                            {
                                subscribedDevices.Add(statisticsHelper);
                                subscribedDeviceNames.Add(statisticsHelper.Device.Name);
                            }
                            else
                            {
                                statisticsHelper.Device.Dispose();
                            }
                        }
                    }

                    foreach (string definedDeviceName in definedDeviceNames)
                    {
                        if (!subscribedDeviceNames.Contains(definedDeviceName))
                        {
                            DeviceStatisticsHelper<SubscribedDevice> statisticsHelper = new DeviceStatisticsHelper<SubscribedDevice>(new SubscribedDevice(definedDeviceName));
                            subscribedDevices.Add(statisticsHelper);
                            statisticsHelper.Reset(now);
                        }
                    }

                    if (dataSource.Tables.Contains("ActiveMeasurements"))
                    {
                        ActiveMeasurementsTableLookup measurementLookup = DataSourceLookups.ActiveMeasurements(dataSource);

                        foreach (DeviceStatisticsHelper<SubscribedDevice> statisticsHelper in subscribedDevices)
                        {
                            string deviceName = Regex.Replace(statisticsHelper.Device.Name, @"^LOCAL\$", "");

                            foreach (DataRow measurementRow in measurementLookup.LookupByDeviceNameNoStat(deviceName))
                            {
                                if (Guid.TryParse(measurementRow["SignalID"].ToNonNullString(), out signalID))
                                {
                                    // In some rare cases duplicate signal ID's have been encountered (likely bad configuration),
                                    // as a result we use a GetOrAdd instead of an Add
                                    subscribedDevicesLookup.GetOrAdd(signalID, statisticsHelper);

                                    switch (measurementRow["SignalType"].ToNonNullString())
                                    {
                                        case "FLAG":
                                            statisticsHelper.Device.StatusFlagsID = signalID;
                                            break;

                                        case "FREQ":
                                            statisticsHelper.Device.FrequencyID = signalID;
                                            break;

                                        case "DFDT":
                                            statisticsHelper.Device.DeltaFrequencyID = signalID;
                                            break;
                                    }
                                }
                            }
                        }
                    }

                    m_subscribedDevicesLookup = subscribedDevicesLookup;
                    m_statisticsHelpers = subscribedDevices;
                }

                FixExpectedMeasurementCounts();
            }
            catch (Exception ex)
            {
                OnProcessException(new InvalidOperationException($"Unable to register device statistics due to exception: {ex.Message}", ex));
            }
        }

        private void UnregisterDeviceStatistics()
        {
            try
            {
                if ((object)m_statisticsHelpers != null)
                {
                    foreach (DeviceStatisticsHelper<SubscribedDevice> statisticsHelper in m_statisticsHelpers)
                        statisticsHelper.Device.Dispose();

                    m_statisticsHelpers = new List<DeviceStatisticsHelper<SubscribedDevice>>();
                    m_subscribedDevicesLookup = new Dictionary<Guid, DeviceStatisticsHelper<SubscribedDevice>>();
                }
            }
            catch (Exception ex)
            {
                OnProcessException(new InvalidOperationException($"Unable to unregister device statistics due to exception: {ex.Message}", ex));
            }
        }

        private void FixExpectedMeasurementCounts()
        {
            Dictionary<Guid, DeviceStatisticsHelper<SubscribedDevice>> subscribedDevicesLookup = m_subscribedDevicesLookup;
            List<DeviceStatisticsHelper<SubscribedDevice>> statisticsHelpers = m_statisticsHelpers;
            DeviceStatisticsHelper<SubscribedDevice> statisticsHelper;

            SignalIndexCache signalIndexCache = m_signalIndexCache;
            DataSet dataSource = DataSource;

            DataTable measurementTable;
            IEnumerable<IGrouping<DeviceStatisticsHelper<SubscribedDevice>, Guid>> groups;

            try
            {
                if ((object)statisticsHelpers == null || (object)subscribedDevicesLookup == null)
                    return;

                if ((object)signalIndexCache == null)
                    return;

                if ((object)dataSource == null || !dataSource.Tables.Contains("ActiveMeasurements"))
                    return;

                measurementTable = dataSource.Tables["ActiveMeasurements"];

                if (!measurementTable.Columns.Contains("FramesPerSecond"))
                    return;

                // Get expected measurement counts
                groups = signalIndexCache.AuthorizedSignalIDs
                    .Where(signalID => subscribedDevicesLookup.TryGetValue(signalID, out statisticsHelper))
                    .Select(signalID => Tuple.Create(subscribedDevicesLookup[signalID], signalID))
                    .ToList()
                    .GroupBy(tuple => tuple.Item1, tuple => tuple.Item2);

                foreach (IGrouping<DeviceStatisticsHelper<SubscribedDevice>, Guid> group in groups)
                {
                    group.Key.ExpectedMeasurementsPerSecond = group
                        .Select(signalID => GetFramesPerSecond(measurementTable, signalID))
                        .Sum();
                }
            }
            catch (Exception ex)
            {
                OnProcessException(new InvalidOperationException($"Unable to set expected measurement counts for gathering statistics due to exception: {ex.Message}", ex));
            }
        }

        private int GetFramesPerSecond(DataTable measurementTable, Guid signalID)
        {
            DataRow row = measurementTable.Select($"SignalID = '{signalID}'").FirstOrDefault();

            if ((object)row != null)
            {
                switch (row.Field<string>("SignalType").ToUpperInvariant())
                {
                    case "FLAG":
                    case "STAT":
                        return 0;

                    default:
                        return row.ConvertField<int>("FramesPerSecond");
                }
            }

            return 0;
        }

        // This method is called when connection has been authenticated
        private void DataSubscriber_ConnectionAuthenticated(object sender, EventArgs e)
        {
            if (m_autoConnect)
                StartSubscription();
        }

        // This method is called then new meta-data has been received
        private void DataSubscriber_MetaDataReceived(object sender, EventArgs<DataSet> e)
        {
            try
            {
                // We handle synchronization on a separate thread since this process may be lengthy
                if (m_autoSynchronizeMetadata)
                    SynchronizeMetadata(e.Argument);
            }
            catch (Exception ex)
            {
                // Process exception for logging
                OnProcessException(new InvalidOperationException("Failed to queue meta-data synchronization due to exception: " + ex.Message, ex));
            }
        }

        /// <summary>
        /// Raises the <see cref="ConnectionEstablished"/> event.
        /// </summary>
        protected void OnConnectionEstablished()
        {
            try
            {
                ConnectionEstablished?.Invoke(this, EventArgs.Empty);
                m_lastMissingCacheWarning = 0L;
            }
            catch (Exception ex)
            {
                // We protect our code from consumer thrown exceptions
                OnProcessException(new InvalidOperationException($"Exception in consumer handler for ConnectionEstablished event: {ex.Message}", ex));
            }
        }

        /// <summary>
        /// Raises the <see cref="ConnectionTerminated"/> event.
        /// </summary>
        protected void OnConnectionTerminated()
        {
            try
            {
                ConnectionTerminated?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                // We protect our code from consumer thrown exceptions
                OnProcessException(new InvalidOperationException($"Exception in consumer handler for ConnectionTerminated event: {ex.Message}", ex));
            }
        }

        /// <summary>
        /// Raises the <see cref="ConnectionAuthenticated"/> event.
        /// </summary>
        protected void OnConnectionAuthenticated()
        {
            try
            {
                ConnectionAuthenticated?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                // We protect our code from consumer thrown exceptions
                OnProcessException(new InvalidOperationException($"Exception in consumer handler for ConnectionAuthenticated event: {ex.Message}", ex));
            }
        }

        /// <summary>
        /// Raises the <see cref="ReceivedServerResponse"/> event.
        /// </summary>
        /// <param name="responseCode">Response received from the server.</param>
        /// <param name="commandCode">Command that the server responded to.</param>
        protected void OnReceivedServerResponse(ServerResponse responseCode, ServerCommand commandCode)
        {
            try
            {
                ReceivedServerResponse?.Invoke(this, new EventArgs<ServerResponse, ServerCommand>(responseCode, commandCode));
            }
            catch (Exception ex)
            {
                // We protect our code from consumer thrown exceptions
                OnProcessException(new InvalidOperationException($"Exception in consumer handler for ReceivedServerResponse event: {ex.Message}", ex));
            }
        }

        /// <summary>
        /// Raises the <see cref="ReceivedUserCommandResponse"/> event.
        /// </summary>
        /// <param name="command">The code for the user command.</param>
        /// <param name="response">The code for the server's response.</param>
        /// <param name="solicited">Indicates whether the response was solicited.</param>
        /// <param name="buffer">Buffer containing the message from the server.</param>
        /// <param name="startIndex">Index into the buffer used to skip the header.</param>
        /// <param name="length">The length of the message in the buffer, including the header.</param>
        protected void OnReceivedUserCommandResponse(ServerCommand command, ServerResponse response, bool solicited, byte[] buffer, int startIndex, int length)
        {
            try
            {
                UserCommandArgs args = new UserCommandArgs(command, response, solicited, buffer, startIndex, length);
                ReceivedUserCommandResponse?.Invoke(this, args);
            }
            catch (Exception ex)
            {
                // We protect our code from consumer thrown exceptions
                OnProcessException(new InvalidOperationException($"Exception in consumer handler for UserCommandResponse event: {ex.Message}", ex));
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
                MetaDataReceived?.Invoke(this, new EventArgs<DataSet>(metadata));
            }
            catch (Exception ex)
            {
                // We protect our code from consumer thrown exceptions
                OnProcessException(new InvalidOperationException($"Exception in consumer handler for MetaDataReceived event: {ex.Message}", ex));
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
                DataStartTime?.Invoke(this, new EventArgs<Ticks>(startTime));
            }
            catch (Exception ex)
            {
                // We protect our code from consumer thrown exceptions
                OnProcessException(new InvalidOperationException($"Exception in consumer handler for DataStartTime event: {ex.Message}", ex));
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
                ProcessingComplete?.Invoke(this, new EventArgs<string>(source));

                // Also raise base class event in case this event has been subscribed
                OnProcessingComplete();
            }
            catch (Exception ex)
            {
                // We protect our code from consumer thrown exceptions
                OnProcessException(new InvalidOperationException($"Exception in consumer handler for ProcessingComplete event: {ex.Message}", ex));
            }
        }

        /// <summary>
        /// Raises the <see cref="NotificationReceived"/> event.
        /// </summary>
        /// <param name="message">Message for the notification.</param>
        protected void OnNotificationReceived(string message)
        {
            try
            {
                NotificationReceived?.Invoke(this, new EventArgs<string>(message));
            }
            catch (Exception ex)
            {
                // We protect our code from consumer thrown exceptions
                OnProcessException(new InvalidOperationException($"Exception in consumer handler for NotificationReceived event: {ex.Message}", ex));
            }
        }

        /// <summary>
        /// Raises the <see cref="ServerConfigurationChanged"/> event.
        /// </summary>
        protected void OnServerConfigurationChanged()
        {
            try
            {
                ServerConfigurationChanged?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                // We protect our code from consumer thrown exceptions
                OnProcessException(new InvalidOperationException($"Exception in consumer handler for ServerConfigurationChanged event: {ex.Message}", ex));
            }
        }

        /// <summary>
        /// Raises <see cref="AdapterBase.ProcessException"/> event.
        /// </summary>
        /// <param name="ex">Processing <see cref="Exception"/>.</param>
        protected override void OnProcessException(Exception ex)
        {
            base.OnProcessException(ex);

            if (DateTime.UtcNow.Ticks - m_lastParsingExceptionTime > m_parsingExceptionWindow)
            {
                // Exception window has passed since last exception, so we reset counters
                m_lastParsingExceptionTime = DateTime.UtcNow.Ticks;
                m_parsingExceptionCount = 0;
            }

            m_parsingExceptionCount++;

            if (m_parsingExceptionCount > m_allowedParsingExceptions)
            {
                try
                {
                    // When the parsing exception threshold has been exceeded, connection is restarted
                    Start();
                }
                catch (Exception restartException)
                {
                    string message = $"Error while restarting subscriber connection due to excessive exceptions: {restartException.Message}";
                    base.OnProcessException(new InvalidOperationException(message, restartException));
                }
                finally
                {
                    // Notify consumer of parsing exception threshold deviation
                    OnExceededParsingExceptionThreshold();
                    m_lastParsingExceptionTime = 0;
                    m_parsingExceptionCount = 0;
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="ExceededParsingExceptionThreshold"/> event.
        /// </summary>
        private void OnExceededParsingExceptionThreshold()
        {
            ExceededParsingExceptionThreshold?.Invoke(this, EventArgs.Empty);
        }

        // Updates the measurements per second counters after receiving another set of measurements.
        private void UpdateMeasurementsPerSecond(DateTime now, int measurementCount)
        {
            long secondsSinceEpoch = now.Ticks / Ticks.PerSecond;

            if (secondsSinceEpoch > m_lastSecondsSinceEpoch)
            {
                if (m_measurementsInSecond < m_minimumMeasurementsPerSecond || m_minimumMeasurementsPerSecond == 0L)
                    m_minimumMeasurementsPerSecond = m_measurementsInSecond;

                if (m_measurementsInSecond > m_maximumMeasurementsPerSecond || m_maximumMeasurementsPerSecond == 0L)
                    m_maximumMeasurementsPerSecond = m_measurementsInSecond;

                m_totalMeasurementsPerSecond += m_measurementsInSecond;
                m_measurementsPerSecondCount++;
                m_measurementsInSecond = 0L;

                m_lastSecondsSinceEpoch = secondsSinceEpoch;
            }

            m_measurementsInSecond += measurementCount;
        }

        // Resets the measurements per second counters after reading the values from the last calculation interval.
        private void ResetMeasurementsPerSecondCounters()
        {
            m_minimumMeasurementsPerSecond = 0L;
            m_maximumMeasurementsPerSecond = 0L;
            m_totalMeasurementsPerSecond = 0L;
            m_measurementsPerSecondCount = 0L;
        }

        private void UpdateStatisticsHelpers()
        {
            long now = RealTime;
            List<DeviceStatisticsHelper<SubscribedDevice>> statisticsHelpers = m_statisticsHelpers;

            foreach (DeviceStatisticsHelper<SubscribedDevice> statisticsHelper in statisticsHelpers)
            {
                statisticsHelper.Update(now);

                // TODO: Missing data detection could be complex. For example, no need to continue logging data outages for devices that are offline - but how to detect?
                //// If data channel is UDP, measurements are missing for time span and data gap recovery enabled, request missing
                //if ((object)m_dataChannel != null && m_dataGapRecoveryEnabled && (object)m_dataGapRecoverer != null && m_lastMeasurementCheck > 0 &&
                //    statisticsHelper.Device.MeasurementsExpected - statisticsHelper.Device.MeasurementsReceived > m_minimumMissingMeasurementThreshold)
                //    m_dataGapRecoverer.LogDataGap(m_lastMeasurementCheck - Ticks.FromSeconds(m_transmissionDelayTimeAdjustment), now);
            }

            //m_lastMeasurementCheck = now;
        }

        private void SubscribedDevicesTimer_Elapsed(object sender, EventArgs<DateTime> elapsedEventArgs)
        {
            UpdateStatisticsHelpers();
        }

        private bool SynchronizedMetadataChanged(DataSet newSynchronizedMetadata)
        {
            try
            {
                return !DataSetEqualityComparer.Default.Equals(m_synchronizedMetadata, newSynchronizedMetadata);
            }
            catch
            {
                return true;
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

        /// <summary>
        /// Gets file path for any defined logging path.
        /// </summary>
        /// <param name="filePath">Path to acquire within logging path.</param>
        /// <returns>File path within any defined logging path.</returns>
        protected string GetLoggingPath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(m_loggingPath))
                return FilePath.GetAbsolutePath(filePath);

            return Path.Combine(m_loggingPath, filePath);
        }

        private void m_localConcentrator_ProcessException(object sender, EventArgs<Exception> e)
        {
            // Make sure any exceptions reported by local concentrator get exposed as needed
            OnProcessException(e.Argument);
        }

        private void m_dataStreamMonitor_Elapsed(object sender, EventArgs<DateTime> e)
        {
            bool dataReceived = m_monitoredBytesReceived > 0;

            if ((object)m_dataChannel == null && m_metadataRefreshPending)
                dataReceived = (DateTime.UtcNow - m_commandChannel.Statistics.LastReceive).Seconds < DataLossInterval;

            if (!dataReceived)
            {
                // If we've received no data in the last time-span, we restart connect cycle...
                m_dataStreamMonitor.Enabled = false;
                OnStatusMessage("\r\nNo data received in {0} seconds, restarting connect cycle...\r\n", (m_dataStreamMonitor.Interval / 1000.0D).ToString("0.0"));
                ThreadPool.QueueUserWorkItem(state => Restart());
            }

            // Reset bytes received bytes being monitored
            m_monitoredBytesReceived = 0L;
        }

        private void m_runTimeLog_ProcessException(object sender, EventArgs<Exception> e)
        {
            OnProcessException(e.Argument);
        }

        private void m_dataGapRecoverer_RecoveredMeasurements(object sender, EventArgs<ICollection<IMeasurement>> e)
        {
            OnNewMeasurements(e.Argument);
        }

        private void m_dataGapRecoverer_StatusMessage(object sender, EventArgs<string> e)
        {
            OnStatusMessage("[DataGapRecoverer] " + e.Argument);
        }

        private void m_dataGapRecoverer_ProcessException(object sender, EventArgs<Exception> e)
        {
            OnProcessException(new InvalidOperationException("[DataGapRecoverer] " + e.Argument.Message, e.Argument.InnerException));
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
            SendServerCommand(ServerCommand.DefineOperationalModes, BigEndian.GetBytes((uint)m_operationalModes));

            // Notify input adapter base that asynchronous connection succeeded
            OnConnected();

            // Notify consumer that connection was successfully established
            OnConnectionEstablished();

            OnStatusMessage("Data subscriber command channel connection to publisher was established.");

            if (m_autoConnect)
            {
                // Attempt authentication if required, remaining steps will happen on successful authentication
                if (m_securityMode == SecurityMode.Gateway)
                    Authenticate(m_sharedSecret, m_authenticationID);
                else
                    StartSubscription();
            }

            if (m_dataGapRecoveryEnabled && (object)m_dataGapRecoverer != null)
                m_dataGapRecoverer.Enabled = true;
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

            if (!HandleSocketException(ex) && !(ex is ObjectDisposedException))
                OnProcessException(new InvalidOperationException("Data subscriber encountered an exception while sending command channel data to publisher connection: " + ex.Message, ex));
        }

        private void m_commandChannel_ReceiveData(object sender, EventArgs<int> e)
        {
            try
            {
                int length = e.Argument;
                byte[] buffer = new byte[length];

                m_lastBytesReceived = length;

                m_commandChannel.Read(buffer, 0, length);
                ProcessServerResponse(buffer, length);
            }
            catch (Exception ex)
            {
                OnProcessException(ex);
            }
        }

        private void m_commandChannel_ReceiveDataException(object sender, EventArgs<Exception> e)
        {
            Exception ex = e.Argument;

            if (!HandleSocketException(ex) && !(ex is ObjectDisposedException))
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
                int length = e.Argument;
                byte[] buffer = new byte[length];

                m_lastBytesReceived = length;

                m_dataChannel.Read(buffer, 0, length);
                ProcessServerResponse(buffer, length);
            }
            catch (Exception ex)
            {
                OnProcessException(ex);
            }
        }

        private void m_dataChannel_ReceiveDataException(object sender, EventArgs<Exception> e)
        {
            Exception ex = e.Argument;

            if (!HandleSocketException(ex) && !(ex is ObjectDisposedException))
                OnProcessException(new InvalidOperationException("Data subscriber encountered an exception while receiving UDP data from publisher connection: " + ex.Message, ex));
        }

        #endregion

        #endregion
    }
}
