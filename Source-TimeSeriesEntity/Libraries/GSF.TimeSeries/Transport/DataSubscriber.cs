//******************************************************************************************************
//  DataSubscriber.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//       to populate OriginalSource value. Added to flag to optionally avoid meta-data synchronization.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Timers;
using System.Xml;
using GSF.Communication;
using GSF.IO;
using GSF.Net.Security;
using GSF.Reflection;
using GSF.Security.Cryptography;
using GSF.TimeSeries.Adapters;
using Random = GSF.Security.Cryptography.Random;
using TcpClient = GSF.Communication.TcpClient;
using Timer = System.Timers.Timer;
using UdpClient = GSF.Communication.UdpClient;

namespace GSF.TimeSeries.Transport
{
    /// <summary>
    /// Represents a data subscribing client that will connect to a data publisher for a data subscription.
    /// </summary>
    public class DataSubscriber : IDisposable
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
                    m_parent.OnNewEntities(frame.Entities.Values);
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
        /// Occurs when client receives response from the server.
        /// </summary>
        public event EventHandler<EventArgs<ServerResponse, ServerCommand>> ReceivedServerResponse;

        /// <summary>
        /// Occurs when client receives requested meta-data transmitted by data publication server.
        /// </summary>
        public event EventHandler<EventArgs<DataSet>> MetaDataReceived;

        /// <summary>
        /// Occurs when first time-series entity is transmitted by data publication server.
        /// </summary>
        public event EventHandler<EventArgs<Ticks>> DataStartTime;

        /// <summary>
        /// Occurs when new time-series entities are received from the data publisher.
        /// </summary>
        public event EventHandler<EventArgs<ICollection<ITimeSeriesEntity>>> NewEntities; 

        /// <summary>
        /// Indicates that processing for an input adapter (via temporal session) has completed.
        /// </summary>
        /// <remarks>
        /// This event is expected to only be raised when an input adapter has been designed to process
        /// a finite amount of data, e.g., reading a historical range of data during temporal processing.
        /// </remarks>
        public event EventHandler<EventArgs<string>> ProcessingComplete;

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
        /// Occurs when the data subscriber has a message which
        /// provides information about the status of its operations.
        /// </summary>
        public event EventHandler<EventArgs<string>> StatusMessage; 

        /// <summary>
        /// Occurs when the data subscriber encounters an exception.
        /// </summary>
        public event EventHandler<EventArgs<Exception>> ProcessException;

        // Constants

        /// <summary>
        /// Defines default value for <see cref="DataSubscriber.OperationalModes"/>.
        /// </summary>
        public const OperationalModes DefaultOperationalModes = OperationalModes.CompressMetadata | OperationalModes.CompressSignalIndexCache | OperationalModes.CompressPayloadData | OperationalModes.ReceiveInternalMetadata | OperationalModes.UseCommonSerializationFormat;

        // Fields
        private IClient m_commandChannel;
        private UdpClient m_dataChannel;
        private string m_connectionString;
        private volatile bool m_reinitializeCommandChannel;

        private long m_commandChannelConnectionAttempts;
        private long m_dataChannelConnectionAttempts;

        private volatile bool m_enabled;
        private volatile bool m_isConnected;
        private volatile bool m_authenticated;
        private volatile bool m_subscribed;

        private int m_processingInterval;

        private LocalConcentrator m_localConcentrator;
        private Timer m_dataStreamMonitor;

        private SecurityMode m_securityMode;
        private string m_sharedSecret;
        private string m_localCertificateFilePath;
        private string m_remoteCertificateFilePath;
        private SslPolicyErrors m_validPolicyErrors;
        private X509ChainStatusFlags m_validChainFlags;
        private bool m_checkCertificateRevocation;

        private OperationalModes m_operationalModes;
        private Encoding m_encoding;
        private int m_bufferSize;

        private readonly List<ServerCommand> m_requests;
        private bool m_synchronizedSubscription;

        private volatile SignalIndexCache m_signalIndexCache;
        private long m_lastMissingCacheWarning;

        private bool m_useMillisecondResolution;
        private bool m_includeTime;

        private volatile long[] m_baseTimeOffsets;
        private volatile int m_timeIndex;
        private volatile byte[][][] m_keyIVs;

        private readonly List<TimeSeriesBuffer> m_bufferBlockCache;
        private uint m_expectedBufferBlockSequenceNumber;

        private volatile int m_lastBytesReceived;
        private long m_monitoredBytesReceived;
        private long m_totalBytesReceived;

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
            m_operationalModes = DefaultOperationalModes;
            DataLossInterval = 10.0D;

            m_bufferBlockCache = new List<TimeSeriesBuffer>();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the connection string which defines connection properties for the data subscriber.
        /// </summary>
        public string ConnectionString
        {
            get
            {
                return m_connectionString;
            }
            set
            {
                m_connectionString = value;
                m_reinitializeCommandChannel = true;
            }
        }

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
                m_reinitializeCommandChannel = true;
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
        /// Gets or sets the desired processing interval, in milliseconds, for the adapter.
        /// </summary>
        /// <remarks>
        /// With the exception of the values of -1 and 0, this value specifies the desired processing interval for data, i.e.,
        /// basically a delay, or timer interval, over which to process data. A value of -1 means to use the default processing
        /// interval while a value of 0 means to process data as fast as possible.
        /// </remarks>
        public int ProcessingInterval
        {
            get
            {
                return m_processingInterval;
            }
            set
            {
                m_processingInterval = value;

                // Request server update the processing interval
                SendServerCommand(ServerCommand.UpdateProcessingInterval, EndianOrder.BigEndian.GetBytes(value));
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
                        m_dataStreamMonitor = new Timer();
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
        /// Gets or sets the size of the buffers used to send and receive data in the transport layer.
        /// </summary>
        public int BufferSize
        {
            get
            {
                return m_bufferSize;
            }
            set
            {
                m_bufferSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the file path to the local certificate used
        /// to authenticate the data subscriber with the data publisher.
        /// </summary>
        public string LocalCertificateFilePath
        {
            get
            {
                return m_localCertificateFilePath;
            }
            set
            {
                m_localCertificateFilePath = value;
                m_reinitializeCommandChannel = true;
            }
        }

        /// <summary>
        /// Gets or sets the file path to the remote
        /// certificate used to identify the data publisher.
        /// </summary>
        public string RemoteCertificateFilePath
        {
            get
            {
                return m_remoteCertificateFilePath;
            }
            set
            {
                m_remoteCertificateFilePath = value;
                m_reinitializeCommandChannel = true;
            }
        }

        /// <summary>
        /// Gets or sets the set of policy errors we expect to
        /// encounter when validating the data publisher's identity.
        /// </summary>
        public SslPolicyErrors ValidPolicyErrors
        {
            get
            {
                return m_validPolicyErrors;
            }
            set
            {
                m_validPolicyErrors = value;
                m_reinitializeCommandChannel = true;
            }
        }

        /// <summary>
        /// Gets or sets the set of chain flags we expect to
        /// encounter when validating the data publisher's identity.
        /// </summary>
        public X509ChainStatusFlags ValidChainFlags
        {
            get
            {
                return m_validChainFlags;
            }
            set
            {
                m_validChainFlags = value;
            }
        }

        /// <summary>
        /// Gets or sets the flag that determines whether we should check that the
        /// data publisher's certificate was revoked by the certificate authority.
        /// </summary>
        public bool CheckCertificateRevocation
        {
            get
            {
                return m_checkCertificateRevocation;
            }
            set
            {
                m_checkCertificateRevocation = value;
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
        /// Gets or sets the <see cref="GatewayCompressionMode"/> used by the subscriber and publisher.
        /// </summary>
        public GatewayCompressionMode GatewayCompressionMode
        {
            get
            {
                return (GatewayCompressionMode)(m_operationalModes & OperationalModes.CompressionModeMask);
            }
            set
            {
                m_operationalModes &= ~OperationalModes.CompressionModeMask;
                m_operationalModes |= (OperationalModes)value;
            }
        }

        /// <summary>
        /// Gets the version number of the protocol in use by this subscriber.
        /// </summary>
        public int Version
        {
            get
            {
                return (int)(m_operationalModes & OperationalModes.VersionMask);
            }
        }

        /// <summary>
        /// Gets flag that indicates whether this data subscriber
        /// has successfully connected to the data publisher.
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return m_isConnected;
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
        /// Gets or sets reference to <see cref="Communication.TcpClient"/> command channel, attaching and/or detaching to events as needed.
        /// </summary>
        private IClient CommandChannel
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
        /// Gets or sets reference to <see cref="UdpClient"/> data channel, attaching and/or detaching to events as needed.
        /// </summary>
        private UdpClient DataChannel
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
        /// Gets the status of this <see cref="DataSubscriber"/>.
        /// </summary>
        /// <remarks>
        /// Derived classes should provide current status information about the adapter for display purposes.
        /// </remarks>
        public string Status
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
                status.AppendFormat("      Data packet security: {0}", (object)m_keyIVs == null ? "Unencrypted" : "Encrypted");
                status.AppendLine();
                status.AppendFormat("      Data monitor enabled: {0}", (object)m_dataStreamMonitor != null && m_dataStreamMonitor.Enabled);
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

                return status.ToString();
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
        /// Gets the total number of measurements processed through this data publisher over the lifetime of the subscriber.
        /// </summary>
        public long LifetimeMeasurements
        {
            get
            {
                return m_lifetimeMeasurements;
            }
        }

        /// <summary>
        /// Gets the minimum value of the measurements per second calculation.
        /// </summary>
        public long MinimumMeasurementsPerSecond
        {
            get
            {
                return m_minimumMeasurementsPerSecond;
            }
        }

        /// <summary>
        /// Gets the maximum value of the measurements per second calculation.
        /// </summary>
        public long MaximumMeasurementsPerSecond
        {
            get
            {
                return m_maximumMeasurementsPerSecond;
            }
        }

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
        public int LifetimeMinimumLatency
        {
            get
            {
                return (int)Ticks.ToMilliseconds(m_lifetimeMinimumLatency);
            }
        }

        /// <summary>
        /// Gets the maximum latency calculated over the full lifetime of the subscriber.
        /// </summary>
        public int LifetimeMaximumLatency
        {
            get
            {
                return (int)Ticks.ToMilliseconds(m_lifetimeMaximumLatency);
            }
        }

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

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Attempts to connect to this <see cref="DataSubscriber"/>.
        /// </summary>
        public void Start()
        {
            if (m_enabled)
                throw new InvalidOperationException("Unable to start data subscriber because it is already running.");

            if (string.IsNullOrEmpty(m_connectionString))
                throw new InvalidOperationException("Unable to start data subscriber because the connection string is missing.");

            m_expectedBufferBlockSequenceNumber = 0u;
            m_commandChannelConnectionAttempts = 0;
            m_dataChannelConnectionAttempts = 0;

            if (m_reinitializeCommandChannel)
                InitializeCommandChannel();

            m_commandChannel.ConnectAsync();

            m_authenticated = (m_securityMode == SecurityMode.TLS);
            m_subscribed = false;
            m_keyIVs = null;

            m_totalBytesReceived = 0L;
            m_monitoredBytesReceived = 0L;
            m_lastBytesReceived = 0;

            m_enabled = true;
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
                    m_sharedSecret = sharedSecret;

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
                        buffer.Write(EndianOrder.BigEndian.GetBytes(bytes.Length), 0, 4);

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
            {
                OnProcessException(new InvalidOperationException("Cannot authenticate subscription without a connection string."));
            }

            return false;
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

                    buffer.Write(EndianOrder.BigEndian.GetBytes(bytes.Length), 0, 4);
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
                    OnProcessException(new InvalidOperationException(string.Format("Exception occurred while trying to send server command \"{0}\" to publisher: {1}", commandCode, ex.Message), ex));
                }
            }
            else
                OnProcessException(new InvalidOperationException(string.Format("Subscriber is currently unconnected. Cannot send server command \"{0}\" to publisher.", commandCode)));

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

                // TODO: Setup proper filter function based
                localConcentrator.FilterFunction = null; //info.DownsamplingMethod;

                localConcentrator.UsePrecisionTimer = false;

                // Parse time constraints, if defined
                DateTime startTimeConstraint = !string.IsNullOrWhiteSpace(info.StartTime) ? AdapterBase.ParseTimeTag(info.StartTime) : DateTime.MinValue;
                DateTime stopTimeConstraint = !string.IsNullOrWhiteSpace(info.StopTime) ? AdapterBase.ParseTimeTag(info.StopTime) : DateTime.MaxValue;

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
                m_dataStreamMonitor.Interval = 2.0D * info.LagTime * 1000.0D;

            // Set millisecond resolution member variable for compact measurement parsing
            m_useMillisecondResolution = info.UseMillisecondResolution;

            return Subscribe(false, info.UseCompactMeasurementFormat, connectionString.ToString());
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
                        buffer.Write(EndianOrder.BigEndian.GetBytes(bytes.Length), 0, 4);

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

            return success;
        }

        /// <summary>
        /// Returns the measurements signal IDs that were authorized after the last successful subscription request.
        /// </summary>
        public virtual Guid[] GetAuthorizedSignalIDs()
        {
            if ((object)m_signalIndexCache != null)
                return m_signalIndexCache.AuthorizedSignalIDs;

            return new Guid[0];
        }

        /// <summary>
        /// Returns the measurements signal IDs that were unauthorized after the last successful subscription request.
        /// </summary>
        public virtual Guid[] GetUnauthorizedSignalIDs()
        {
            if ((object)m_signalIndexCache != null)
                return m_signalIndexCache.UnauthorizedSignalIDs;

            return new Guid[0];
        }

        /// <summary>
        /// Resets the counters for the lifetime statistics without interrupting the adapter's operations.
        /// </summary>
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
        /// Unsubscribes from a data publisher.
        /// </summary>
        /// <returns><c>true</c> if unsubscribe transmission was successful; otherwise <c>false</c>.</returns>
        public virtual bool Unsubscribe()
        {
            // Send unsubscribe server command
            return SendServerCommand(ServerCommand.Unsubscribe);
        }

        /// <summary>
        /// Gets a short one-line status of this <see cref="DataSubscriber"/>.
        /// </summary>
        /// <param name="maxLength">Maximum length of the status message.</param>
        /// <returns>Text of the status message.</returns>
        public string GetShortStatus(int maxLength)
        {
            if ((object)m_commandChannel != null && m_commandChannel.CurrentState == ClientState.Connected)
                return string.Format("Subscriber is connected and receiving {0} data points", m_synchronizedSubscription ? "synchronized" : "unsynchronized").CenterText(maxLength);

            return "Subscriber is not connected.".CenterText(maxLength);
        }

        /// <summary>
        /// Attempts to disconnect from this <see cref="DataSubscriber"/>.
        /// </summary>
        public void Stop()
        {
            m_enabled = false;

            // Stop data stream monitor
            if ((object)m_dataStreamMonitor != null)
                m_dataStreamMonitor.Enabled = false;

            // Disconnect command channel
            if ((object)m_commandChannel != null)
                m_commandChannel.Disconnect();
        }

        /// <summary>
        /// Releases all the resources used by the <see cref="DataSubscriber"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="DataSubscriber"/> object and optionally releases the managed resources.
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
                        DataLossInterval = 0.0D;
                        CommandChannel = null;
                        DataChannel = null;
                        DisposeLocalConcentrator();
                    }
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Disposes of any previously defined local concentrator.
        /// </summary>
        internal void DisposeLocalConcentrator()
        {
            if ((object)m_localConcentrator != null)
            {
                m_localConcentrator.ProcessException -= m_localConcentrator_ProcessException;
                m_localConcentrator.Dispose();
            }

            m_localConcentrator = null;
        }

        private void InitializeCommandChannel()
        {
            Dictionary<string, string> settings;
            string setting;

            if (m_securityMode != SecurityMode.TLS)
            {
                // Create a new TCP client
                TcpClient commandChannel = new TcpClient();

                // Initialize default settings
                commandChannel.PayloadAware = true;
                commandChannel.PersistSettings = false;
                commandChannel.MaxConnectionAttempts = 1;
                commandChannel.ReceiveBufferSize = m_bufferSize;
                commandChannel.SendBufferSize = m_bufferSize;

                // Assign command channel client reference and attach to needed events
                CommandChannel = commandChannel;
            }
            else
            {
                // Create a new TLS client and certificate checker
                TlsClient commandChannel = new TlsClient();
                SimpleCertificateChecker certificateChecker = new SimpleCertificateChecker();

                // Set up certificate checker
                certificateChecker.TrustedCertificates.Add(new X509Certificate2(FilePath.GetAbsolutePath(m_remoteCertificateFilePath)));
                certificateChecker.ValidPolicyErrors = m_validPolicyErrors;
                certificateChecker.ValidChainFlags = m_validChainFlags;

                // Initialize default settings
                commandChannel.PayloadAware = true;
                commandChannel.PersistSettings = false;
                commandChannel.MaxConnectionAttempts = 1;
                commandChannel.CertificateFile = FilePath.GetAbsolutePath(m_localCertificateFilePath);
                commandChannel.CheckCertificateRevocation = m_checkCertificateRevocation;
                commandChannel.CertificateChecker = certificateChecker;
                commandChannel.ReceiveBufferSize = m_bufferSize;
                commandChannel.SendBufferSize = m_bufferSize;

                // Assign command channel client reference and attach to needed events
                CommandChannel = commandChannel;
            }

            // Get proper connection string - either from specified command channel
            // or from base connection string
            settings = m_connectionString.ParseKeyValuePairs();

            if (settings.TryGetValue("commandChannel", out setting))
                m_commandChannel.ConnectionString = setting;
            else
                m_commandChannel.ConnectionString = ConnectionString;

            m_reinitializeCommandChannel = false;
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

                    OnReceivedServerResponse(responseCode, commandCode);

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
                            List<ITimeSeriesEntity> measurements = new List<ITimeSeriesEntity>();
                            DataPacketFlags flags;
                            Ticks timestamp = 0;
                            int count;

                            // At the point when data is being received, data monitor should be enabled
                            if (m_totalBytesReceived == 0 && (object)m_dataStreamMonitor != null && !m_dataStreamMonitor.Enabled)
                                m_dataStreamMonitor.Enabled = true;

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
                                timestamp = EndianOrder.BigEndian.ToInt64(buffer, responseIndex);
                                responseIndex += 8;
                            }

                            // Deserialize number of measurements that follow
                            count = EndianOrder.BigEndian.ToInt32(buffer, responseIndex);
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
                                        // Decompress compact measurements from payload
                                        measurements.AddRange(buffer.DecompressPayload(m_signalIndexCache, responseIndex, responseLength - responseIndex + DataPublisher.ClientResponseHeaderSize, count, m_includeTime, flags));
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

                            // Provide new measurements to local concentrator, if defined, otherwise directly expose them to the consumer
                            if ((object)m_localConcentrator != null)
                                m_localConcentrator.SortEntities(measurements);
                            else
                                OnNewEntities(measurements);

                            // Gather statistics on received data
                            long timeReceived = DateTime.UtcNow.Ticks;
                            int measurementCount = measurements.Count();

                            m_lifetimeMeasurements += measurementCount;
                            UpdateMeasurementsPerSecond(measurementCount);

                            for (int x = 0; x < measurements.Count; x++)
                            {
                                long latency = timeReceived - (long)measurements[x].Timestamp;
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
                            uint sequenceNumber = EndianOrder.BigEndian.ToUInt32(buffer, responseIndex);
                            int cacheIndex = (int)(sequenceNumber - m_expectedBufferBlockSequenceNumber);
                            TimeSeriesBuffer timeSeriesBuffer;
                            Tuple<Guid, string, uint> measurementKey;
                            ushort signalIndex;

                            // Check if this buffer block has already been processed (e.g., mistaken retransmission due to timeout)
                            if (cacheIndex >= 0 && (cacheIndex >= m_bufferBlockCache.Count || (object)m_bufferBlockCache[cacheIndex] == null))
                            {
                                // Send confirmation that buffer block is received
                                SendServerCommand(ServerCommand.ConfirmBufferBlock, buffer.BlockCopy(responseIndex, 4));

                                // Get measurement key from signal index cache
                                signalIndex = EndianOrder.BigEndian.ToUInt16(buffer, responseIndex + 4);

                                if (!m_signalIndexCache.Reference.TryGetValue(signalIndex, out measurementKey))
                                    throw new InvalidOperationException("Failed to find associated signal identification for runtime ID " + signalIndex);

                                // Skip the sequence number and signal index when creating the buffer block measurement
                                timeSeriesBuffer = new TimeSeriesBuffer(measurementKey.Item1, DateTime.UtcNow.Ticks, buffer, responseIndex + 6, responseLength - 6);

                                // Determine if this is the next buffer block in the sequence
                                if (sequenceNumber == m_expectedBufferBlockSequenceNumber)
                                {
                                    List<ITimeSeriesEntity> bufferBlockMeasurements = new List<ITimeSeriesEntity>();
                                    int i;

                                    // Add the buffer block measurement to the list of measurements to be published
                                    bufferBlockMeasurements.Add(timeSeriesBuffer);
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
                                    OnNewEntities(bufferBlockMeasurements);
                                }
                                else
                                {
                                    // Ensure that the list has at least as many
                                    // elements as it needs to cache this measurement
                                    for (int i = m_bufferBlockCache.Count; i <= cacheIndex; i++)
                                        m_bufferBlockCache.Add(null);

                                    // Insert this buffer block into the proper location in the list
                                    m_bufferBlockCache[cacheIndex] = timeSeriesBuffer;
                                }
                            }

                            m_lifetimeMeasurements += 1;
                            UpdateMeasurementsPerSecond(1);
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
                            m_signalIndexCache = DeserializeSignalIndexCache(buffer.BlockCopy(responseIndex, responseLength));
                            break;
                        case ServerResponse.UpdateBaseTimes:
                            // Get active time index
                            m_timeIndex = EndianOrder.BigEndian.ToInt32(buffer, responseIndex);
                            responseIndex += 4;

                            // Deserialize new base time offsets
                            m_baseTimeOffsets = new[] { EndianOrder.BigEndian.ToInt64(buffer, responseIndex), EndianOrder.BigEndian.ToInt64(buffer, responseIndex + 8) };
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
                            break;
                    }
                }
                catch (Exception ex)
                {
                    OnProcessException(new InvalidOperationException("Failed to process publisher response packet due to exception: " + ex.Message, ex));
                }
            }
        }

        /// <summary>
        /// Get message from string based response.
        /// </summary>
        /// <param name="buffer">Response buffer.</param>
        /// <param name="startIndex">Start index of response message.</param>
        /// <param name="length">Length of response message.</param>
        /// <returns>Decoded response string.</returns>
        private string InterpretResponseMessage(byte[] buffer, int startIndex, int length)
        {
            return m_encoding.GetString(buffer, startIndex, length);
        }

        private SignalIndexCache DeserializeSignalIndexCache(byte[] buffer)
        {
            GatewayCompressionMode gatewayCompressionMode = (GatewayCompressionMode)(m_operationalModes & OperationalModes.CompressionModeMask);
            bool useCommonSerializationFormat = (m_operationalModes & OperationalModes.UseCommonSerializationFormat) > 0;
            bool compressSignalIndexCache = (m_operationalModes & OperationalModes.CompressSignalIndexCache) > 0;

            SignalIndexCache deserializedCache;

            GZipStream inflater = null;

            if (compressSignalIndexCache && gatewayCompressionMode == GatewayCompressionMode.GZip)
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
                deserializedCache = GSF.Serialization.Deserialize<SignalIndexCache>(buffer, SerializationFormat.Binary);
            }

            return deserializedCache;
        }

        private DataSet DeserializeMetadata(byte[] buffer)
        {
            GatewayCompressionMode gatewayCompressionMode = (GatewayCompressionMode)(m_operationalModes & OperationalModes.CompressionModeMask);
            bool useCommonSerializationFormat = (m_operationalModes & OperationalModes.UseCommonSerializationFormat) > 0;
            bool compressMetadata = (m_operationalModes & OperationalModes.CompressMetadata) > 0;
            Ticks startTime = DateTime.UtcNow.Ticks;

            DataSet deserializedMetadata;
            GZipStream inflater = null;

            if (compressMetadata && gatewayCompressionMode == GatewayCompressionMode.GZip)
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
                deserializedMetadata = GSF.Serialization.Deserialize<DataSet>(buffer, SerializationFormat.Binary);
            }

            long rowCount = deserializedMetadata.Tables.Cast<DataTable>().Select(dataTable => (long)dataTable.Rows.Count).Sum();

            if (rowCount > 0)
            {
                double elapsedTime = (DateTime.UtcNow.Ticks - startTime).ToSeconds();
                OnStatusMessage("Received a total of {0:N0} records spanning {1:N0} tables of meta-data that was {2}deserialized in {3}...", rowCount, deserializedMetadata.Tables.Count, compressMetadata ? "uncompressed and " : "", elapsedTime < 0.01D ? "less than a second" : elapsedTime.ToString("0.00") + " seconds");
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

        // Restarts the subscriber.
        private void Restart()
        {
            try
            {
                if (m_enabled)
                    Stop();

                Start();
            }
            catch (Exception ex)
            {
                OnProcessException(ex);
            }
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

        // Updates the measurements per second counters after receiving another set of measurements.
        private void UpdateMeasurementsPerSecond(int measurementCount)
        {
            long secondsSinceEpoch = DateTime.UtcNow.Ticks / Ticks.PerSecond;

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

        // Disconnect client, restarting if disconnect was not intentional
        private void DisconnectClient()
        {
            DataChannel = null;

            // If user didn't initiate disconnect, restart the connection
            if (m_enabled)
                Restart();
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

        /// <summary>
        /// Raises the <see cref="ConnectionEstablished"/> event.
        /// </summary>
        protected void OnConnectionEstablished()
        {
            try
            {
                if ((object)ConnectionEstablished != null)
                    ConnectionEstablished(this, EventArgs.Empty);

                m_lastMissingCacheWarning = 0L;
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
                if ((object)ConnectionTerminated != null)
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
                if ((object)ConnectionAuthenticated != null)
                    ConnectionAuthenticated(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                // We protect our code from consumer thrown exceptions
                OnProcessException(new InvalidOperationException(string.Format("Exception in consumer handler for ConnectionAuthenticated event: {0}", ex.Message), ex));
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
                if ((object)ReceivedServerResponse != null)
                    ReceivedServerResponse(this, new EventArgs<ServerResponse, ServerCommand>(responseCode, commandCode));
            }
            catch (Exception ex)
            {
                // We protect our code from consumer thrown exceptions
                OnProcessException(new InvalidOperationException(string.Format("Exception in consumer handler for ReceivedServerResponse event: {0}", ex.Message), ex));
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
                if ((object)MetaDataReceived != null)
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
                if ((object)DataStartTime != null)
                    DataStartTime(this, new EventArgs<Ticks>(startTime));
            }
            catch (Exception ex)
            {
                // We protect our code from consumer thrown exceptions
                OnProcessException(new InvalidOperationException(string.Format("Exception in consumer handler for DataStartTime event: {0}", ex.Message), ex));
            }
        }

        /// <summary>
        /// Raises the <see cref="NewEntities"/> event.
        /// </summary>
        /// <param name="entities">Collection of entities received from the data publisher.</param>
        private void OnNewEntities(ICollection<ITimeSeriesEntity> entities)
        {
            if ((object)NewEntities != null)
                NewEntities(this, new EventArgs<ICollection<ITimeSeriesEntity>>(entities));
        }

        /// <summary>
        /// Raises the <see cref="ProcessingComplete"/> event.
        /// </summary>
        /// <param name="source">Type name of adapter that sent the processing completed notification.</param>
        protected void OnProcessingComplete(string source)
        {
            try
            {
                if ((object)ProcessingComplete != null)
                    ProcessingComplete(this, new EventArgs<string>(source));
            }
            catch (Exception ex)
            {
                // We protect our code from consumer thrown exceptions
                OnProcessException(new InvalidOperationException(string.Format("Exception in consumer handler for ProcessingComplete event: {0}", ex.Message), ex));
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
                if ((object)NotificationReceived != null)
                    NotificationReceived(this, new EventArgs<string>(message));
            }
            catch (Exception ex)
            {
                // We protect our code from consumer thrown exceptions
                OnProcessException(new InvalidOperationException(string.Format("Exception in consumer handler for NotificationReceived event: {0}", ex.Message), ex));
            }
        }

        /// <summary>
        /// Raises the <see cref="ServerConfigurationChanged"/> event.
        /// </summary>
        protected void OnServerConfigurationChanged()
        {
            try
            {
                if ((object)ServerConfigurationChanged != null)
                    ServerConfigurationChanged(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                // We protect our code from consumer thrown exceptions
                OnProcessException(new InvalidOperationException(string.Format("Exception in consumer handler for ServerConfigurationChanged event: {0}", ex.Message), ex));
            }
        }

        /// <summary>
        /// Raises the <see cref="StatusMessage"/> event.
        /// </summary>
        private void OnStatusMessage(string message)
        {
            if ((object)StatusMessage != null)
                StatusMessage(this, new EventArgs<string>(message));
        }

        /// <summary>
        /// Raises the <see cref="StatusMessage"/> event.
        /// </summary>
        private void OnStatusMessage(string format, params object[] formatArgs)
        {
            if ((object)StatusMessage != null)
                StatusMessage(this, new EventArgs<string>(string.Format(format, formatArgs)));
        }

        /// <summary>
        /// Raises the <see cref="ProcessException"/> event.
        /// </summary>
        private void OnProcessException(Exception ex)
        {
            if ((object)ProcessException != null)
                ProcessException(this, new EventArgs<Exception>(ex));
        }

        private void m_localConcentrator_ProcessException(object sender, EventArgs<Exception> e)
        {
            // Make sure any exceptions reported by local concentrator get exposed as needed
            OnProcessException(e.Argument);
        }

        private void m_dataStreamMonitor_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (m_monitoredBytesReceived == 0)
            {
                // If we've received no data in the last time-span, we restart connect cycle...
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
            m_isConnected = true;

            // Make sure no existing requests are queued for a new publisher connection
            lock (m_requests)
            {
                m_requests.Clear();
            }

            // Define operational modes as soon as possible
            SendServerCommand(ServerCommand.DefineOperationalModes, EndianOrder.BigEndian.GetBytes((uint)m_operationalModes));

            // Notify consumer that connection was successfully established
            OnConnectionEstablished();

            OnStatusMessage("Data subscriber command channel connection to publisher was established.");
        }

        private void m_commandChannel_ConnectionTerminated(object sender, EventArgs e)
        {
            m_isConnected = false;
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
                    if ((object)buffer != null)
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
                    if ((object)buffer != null)
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

            if (!HandleSocketException(ex) && !(ex is ObjectDisposedException))
                OnProcessException(new InvalidOperationException("Data subscriber encountered an exception while receiving UDP data from publisher connection: " + ex.Message, ex));
        }

        #endregion

        #endregion
    }
}
