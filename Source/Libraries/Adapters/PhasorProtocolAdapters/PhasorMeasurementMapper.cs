//******************************************************************************************************
//  PhasorMeasurementMapper.cs - Gbtc
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
//  05/18/2009 - J. Ritchie Carroll
//       Generated original version of source code.
//  03/21/2010 - J. Ritchie Carroll
//       Added new connection string settings to accommodate new MultiProtocolFrameParser properties.
//  12/04/2012 - J. Ritchie Carroll
//       Migrated to PhasorProtocolAdapters project.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using GSF;
using GSF.Communication;
using GSF.Diagnostics;
using GSF.IO;
using GSF.Parsing;
using GSF.PhasorProtocols;
using GSF.PhasorProtocols.Anonymous;
using GSF.Threading;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using GSF.TimeSeries.Data;
using GSF.TimeSeries.Statistics;
using GSF.Units;
using GSF.Units.EE;
using TcpClient = GSF.Communication.TcpClient;

namespace PhasorProtocolAdapters
{
    /// <summary>
    /// Represents an <see cref="IInputAdapter"/> used to map measured values from a connection
    /// to a phasor measurement device to new <see cref="IMeasurement"/> values.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class PhasorMeasurementMapper : InputAdapterBase
    {
        #region [ Members ]

        // Nested Types

        // Represents a missing data monitor needed to better calculate
        // total missing data when there are redundant frames in a
        // packet of data (e.g., when using IEC 61850-90-5)
        private class MissingDataMonitor : ConcentratorBase
        {
            #region [ Members ]

            // Fields
            private long m_lastFrameTimestamp;
            private long m_missingData;
            private int m_redundantFramesPerPacket;

            #endregion

            #region [ Properties ]

            /// <summary>
            /// Gets or sets the redundant frames per packet.
            /// </summary>
            public int RedundantFramesPerPacket
            {
                set => m_redundantFramesPerPacket = value;
            }

            /// <summary>
            /// Gets the missing data count since the monitor was started.
            /// </summary>
            public long TotalMissingData
            {
                get
                {
                    long cutOff = Ticks.AlignToSubsecondDistribution(RealTime - LagTicks, FramesPerSecond, TimeResolution);
                    int missingFrameCount = (int)Math.Round((cutOff - m_lastFrameTimestamp) / TicksPerFrame);
                    
                    m_missingData += missingFrameCount > m_redundantFramesPerPacket ? missingFrameCount - m_redundantFramesPerPacket : 0;
                    m_lastFrameTimestamp = cutOff;

                    return m_missingData;
                }
            }

            #endregion

            #region [ Methods ]

            /// <summary>
            /// Starts the <see cref="MissingDataMonitor"/>.
            /// </summary>
            public override void Start()
            {
                base.Start();
                m_lastFrameTimestamp = Ticks.AlignToSubsecondDistribution(RealTime, FramesPerSecond, TimeResolution);
            }

            /// <summary>
            /// Called when a frame is published by the underlying concentrator engine.
            /// </summary>
            /// <param name="frame">The frame that is published.</param>
            /// <param name="index">The index of the frame from 0 to <see cref="ConcentratorBase.FramesPerSecond"/> - 1.</param>
            protected override void PublishFrame(IFrame frame, int index)
            {
                int missingFrameCount = (int)Math.Round((frame.Timestamp - m_lastFrameTimestamp) / TicksPerFrame);
                m_missingData += missingFrameCount > m_redundantFramesPerPacket ? missingFrameCount - m_redundantFramesPerPacket : 0;
                m_lastFrameTimestamp = frame.Timestamp;
            }

            #endregion
        }

        // Fields
        private MultiProtocolFrameParser m_frameParser;
        private IServer m_publishChannel;
        private TcpClient m_clientBasedPublishChannel;
        private IConfigurationFrame m_lastConfigurationFrame;
        private bool m_forwardOnly;
        private Dictionary<string, MeasurementKey> m_definedMeasurements;
        private ConcurrentDictionary<ushort, DeviceStatisticsHelper<ConfigurationCell>> m_definedDevices;
        private ConcurrentDictionary<string, DeviceStatisticsHelper<ConfigurationCell>> m_labelDefinedDevices;
        private readonly ConcurrentDictionary<string, long> m_undefinedDevices;
        private readonly ConcurrentDictionary<SignalKind, string[]> m_generatedSignalReferenceCache;
        private string[] m_serverList;
        private ushort[] m_accessIDList;
        private int m_serverIndex;
        private MissingDataMonitor m_missingDataMonitor;
        private SharedTimer m_dataStreamMonitor;
        private SharedTimer m_measurementCounter;
        private bool m_cachedConfigLoadAttempted;
        private readonly object m_configurationOperationLock;
        private readonly ConcurrentDictionary<Guid, string> m_connectionIDCache;
        private volatile IConfigurationFrame m_configurationFrame;
        private int m_lastConfigurationPublishMinute;
        private bool m_configurationFramePublished;
        private long m_receivedConfigurationFrames;
        private long m_totalLatency;
        private long m_minimumLatency;
        private long m_maximumLatency;
        private long m_latencyFrames;
        private string m_sharedMapping;
        private uint m_sharedMappingID;
        private bool m_forceLabelMapping;
        private bool m_receivedConfigFrame;
        private long m_bytesReceived;
        private double m_lagTime;
        private double m_leadTime;
        private long m_timeResolution;
        private bool m_countOnlyMappedMeasurements;
        private bool m_injectBadData;

        private long m_totalMeasurementsPerSecond;
        private long m_measurementsPerSecondCount;
        private long m_measurementsInSecond;
        private long m_lastSecondsSinceEpoch;
        private long m_lifetimeTotalLatency;
        private long m_lifetimeMinimumLatency;
        private long m_lifetimeMaximumLatency;
        private long m_lifetimeLatencyFrames;
        private MeasurementKey m_qualityFlagsKey;
        private int m_lastMeasurementMappedCount = 4;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="PhasorMeasurementMapper"/>.
        /// </summary>
        public PhasorMeasurementMapper()
        {
            // Create a cached signal reference dictionary for generated signal references
            m_generatedSignalReferenceCache = new ConcurrentDictionary<SignalKind, string[]>();

            // Create data stream monitoring timer
            m_dataStreamMonitor = CommonPhasorServices.TimerScheduler.CreateTimer();
            m_dataStreamMonitor.Elapsed += m_dataStreamMonitor_Elapsed;
            m_dataStreamMonitor.AutoReset = true;
            m_dataStreamMonitor.Enabled = false;

            m_undefinedDevices = new ConcurrentDictionary<string, long>();
            m_configurationOperationLock = new object();

            // Create a new connection ID cache
            m_connectionIDCache = new ConcurrentDictionary<Guid, string>();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets flag that determines if device being mapped is a concentrator (i.e., data from multiple
        /// devices combined together from the connected device).
        /// </summary>
        public bool IsConcentrator { get; private set; }

        /// <summary>
        /// Gets the access ID (a.k.a, ID code) for this device connection. Value is often necessary in order to make a connection to some phasor protocols.
        /// </summary>
        /// <remarks>
        /// This value can mutate when configured with multiple values, i.e., where one access ID code is specified for each target device connection, e.g.:
        /// <c>server=192.168.1.10:4712,192.168.1.12:4712; accessID=95,96</c>
        /// </remarks>
        public ushort AccessID
        {
            get
            {
                if (m_serverIndex >= 0 && m_serverIndex < m_accessIDList?.Length)
                    return m_accessIDList[m_serverIndex];

                if (m_accessIDList?.Length > 0)
                    return m_accessIDList[0];

                return (ushort)1;
            }
        }

        private IEnumerable<DeviceStatisticsHelper<ConfigurationCell>> StatisticsHelpers => m_labelDefinedDevices != null ? m_definedDevices.Values.Concat(m_labelDefinedDevices.Values) : m_definedDevices.Values;

        /// <summary>
        /// Gets an enumeration of all defined system devices (regardless of ID or label based definition)
        /// </summary>
        public IEnumerable<ConfigurationCell> DefinedDevices => StatisticsHelpers.Select(statisticsHelper => statisticsHelper.Device);

        /// <summary>
        /// Gets or sets flag that determines if use of cached configuration during initial connection is allowed when a configuration has not been received within the data loss interval.
        /// </summary>
        public bool AllowUseOfCachedConfiguration { get; set; }

        /// <summary>
        /// Gets the configuration cache file name, with path.
        /// </summary>
        public string ConfigurationCacheFileName => ConfigurationFrame.GetConfigurationCacheFileName(Name);

        /// <summary>
        /// Gets or sets time zone of this <see cref="PhasorMeasurementMapper"/>.
        /// </summary>
        /// <remarks>
        /// If time zone of clock of connected device is not set to UTC, assigning this property
        /// with proper time zone will allow proper adjustment.
        /// </remarks>
        public TimeZoneInfo TimeZone { get; set; }

        /// <summary>
        /// Gets or sets ticks used to manually adjust time of this <see cref="PhasorMeasurementMapper"/>.
        /// </summary>
        /// <remarks>
        /// This property will allow for precise time adjustments of connected devices should
        /// this be needed.
        /// </remarks>
        public Ticks TimeAdjustmentTicks { get; set; }

        /// <summary>
        /// Gets the number of missing frames of data, taking into account redundant frames.
        /// </summary>
        public long MissingData
        {
            get
            {
                if (m_frameParser.RedundantFramesPerPacket > 0 && m_missingDataMonitor != null)
                    return m_missingDataMonitor.TotalMissingData;

                return MissingFrames;
            }
        }

        /// <summary>
        /// Gets or set last report time for current mapper connection.
        /// </summary>
        public Ticks LastReportTime { get; private set; }

        /// <summary>
        /// Gets the the total number of frames that have been received by the current mapper connection.
        /// </summary>
        public long TotalFrames => m_frameParser?.TotalFramesReceived ?? 0;

        /// <summary>
        /// Gets the total number of frames that have been missed by the current mapper connection.
        /// </summary>
        public long MissingFrames => m_frameParser?.TotalMissingFrames ?? 0;

        /// <summary>
        /// Gets the total number of CRC errors that have been encountered by the the current mapper connection.
        /// </summary>
        public long CRCErrors => m_frameParser?.TotalCrcExceptions ?? 0;

        /// <summary>
        /// Gets the total number frames that came in out of order from the current mapper connection.
        /// </summary>
        public long OutOfOrderFrames { get; private set; }

        /// <summary>
        /// Gets the minimum latency in milliseconds over the last test interval.
        /// </summary>
        public int MinimumLatency => (int)Ticks.ToMilliseconds(m_minimumLatency);

        /// <summary>
        /// Gets the maximum latency in milliseconds over the last test interval.
        /// </summary>
        public int MaximumLatency => (int)Ticks.ToMilliseconds(m_maximumLatency);

        /// <summary>
        /// Gets the average latency in milliseconds over the last test interval.
        /// </summary>
        public int AverageLatency => m_latencyFrames == 0 ? -1 : (int)Ticks.ToMilliseconds(m_totalLatency / m_latencyFrames);

        /// <summary>
        /// Gets the total number of connection attempts.
        /// </summary>
        public long ConnectionAttempts { get; private set; }

        /// <summary>
        /// Gets the total number of received configurations.
        /// </summary>
        public long ConfigurationChanges { get; private set; }

        /// <summary>
        /// Gets the total number of received data frames.
        /// </summary>
        public long TotalDataFrames { get; private set; }

        /// <summary>
        /// Gets the total number of received configuration frames.
        /// </summary>
        public long TotalConfigurationFrames { get; private set; }

        /// <summary>
        /// Gets the total number of received header frames.
        /// </summary>
        public long TotalHeaderFrames { get; private set; }

        /// <summary>
        /// Gets the defined frame rate.
        /// </summary>
        public int DefinedFrameRate => m_frameParser?.ConfiguredFrameRate ?? 0;

        /// <summary>
        /// Gets the actual frame rate.
        /// </summary>
        public double ActualFrameRate => m_frameParser?.CalculatedFrameRate ?? 0.0D;

        /// <summary>
        /// Gets the actual data rate.
        /// </summary>
        public double ActualDataRate => m_frameParser?.ByteRate ?? 0.0D;

        /// <summary>
        /// Gets or sets acronym of other device for which to assume a shared mapping.
        /// </summary>
        /// <remarks>
        /// Assigning acronym to this property automatically looks up ID of associated device.
        /// </remarks>
        public string SharedMapping
        {
            get => m_sharedMapping;
            set
            {
                m_sharedMapping = value;
                m_sharedMappingID = 0;

                if (!string.IsNullOrWhiteSpace(m_sharedMapping))
                {
                    try
                    {
                        DataRow[] filteredRows = DataSource.Tables["InputAdapters"].Select($"AdapterName = '{m_sharedMapping}'");

                        if (filteredRows.Length > 0)
                        {
                            m_sharedMappingID = uint.Parse(filteredRows[0]["ID"].ToString());
                        }
                        else
                        {
                            OnProcessException(MessageLevel.Warning, new InvalidOperationException($"Failed to find input adapter ID for shared mapping \"{m_sharedMapping}\", mapping was not assigned."));
                            m_sharedMapping = null;
                        }
                    }
                    catch (Exception ex)
                    {
                        OnProcessException(MessageLevel.Warning, new InvalidOperationException($"Failed to find input adapter ID for shared mapping \"{m_sharedMapping}\" due to exception: {ex.Message} Mapping was not assigned.", ex));
                        m_sharedMapping = null;
                    }
                }
            }
        }

        /// <summary>
        /// Returns ID of associated device with shared mapping or <see cref="AdapterBase.ID"/> of this <see cref="PhasorMeasurementMapper"/> if no shared mapping is defined.
        /// </summary>
        public uint SharedMappingID => m_sharedMappingID == 0 ? ID : m_sharedMappingID;

        /// <summary>
        /// Gets flag that determines if the data input connects asynchronously.
        /// </summary>
        protected override bool UseAsyncConnect => true;

        /// <summary>
        /// Gets the flag indicating if this adapter supports temporal processing.
        /// </summary>
        /// <remarks>
        /// Since the phasor measurement mapper is designed to open sockets and connect to data streams,
        /// it is expected that this would not be desired in a temporal data streaming session.
        /// </remarks>
        public override bool SupportsTemporalProcessing => false;

        /// <summary>
        /// Gets total data packet bytes received during this session.
        /// </summary>
        public long TotalBytesReceived => m_frameParser?.TotalBytesReceived ?? 0;

        /// <summary>
        /// Gets the total number of measurements processed through this data publisher over the lifetime of the input stream.
        /// </summary>
        public long LifetimeMeasurements { get; private set; }

        /// <summary>
        /// Gets the minimum value of the measurements per second calculation.
        /// </summary>
        public long MinimumMeasurementsPerSecond { get; private set; }

        /// <summary>
        /// Gets the maximum value of the measurements per second calculation.
        /// </summary>
        public long MaximumMeasurementsPerSecond { get; private set; }

        /// <summary>
        /// Gets the average value of the measurements per second calculation.
        /// </summary>
        public long AverageMeasurementsPerSecond => m_measurementsPerSecondCount == 0L ? 0L : m_totalMeasurementsPerSecond / m_measurementsPerSecondCount;

        /// <summary>
        /// Gets the minimum latency calculated over the full lifetime of the input stream.
        /// </summary>
        public int LifetimeMinimumLatency => (int)Ticks.ToMilliseconds(m_lifetimeMinimumLatency);

        /// <summary>
        /// Gets the maximum latency calculated over the full lifetime of the input stream.
        /// </summary>
        public int LifetimeMaximumLatency => (int)Ticks.ToMilliseconds(m_lifetimeMaximumLatency);

        /// <summary>
        /// Gets the average latency calculated over the full lifetime of the input stream.
        /// </summary>
        public int LifetimeAverageLatency => m_lifetimeLatencyFrames == 0 ? -1 : (int)Ticks.ToMilliseconds(m_lifetimeTotalLatency / m_lifetimeLatencyFrames);

        /// <summary>
        /// When false, this adapter will not log any connection errors through OnProcessException. When true, all errors get logged.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Enable to allow this adapter to log connection errors. Disable to ignore connection errors.")]
        [DefaultValue(true)]
        public new bool EnableConnectionErrors
        {
            get => base.EnableConnectionErrors;
            set => base.EnableConnectionErrors = value;
        }

        /// <summary>
        /// Gets connection info for adapter, if any.
        /// </summary>
        public override string ConnectionInfo => m_frameParser?.ConnectionInfo;

        /// <summary>
        /// Gets or sets reference to <see cref="MultiProtocolFrameParser"/>, attaching and/or detaching to events as needed.
        /// </summary>
        protected MultiProtocolFrameParser FrameParser
        {
            get => m_frameParser;
            set
            {
                if (m_frameParser != null)
                {
                    // Detach from events on existing frame parser reference
                    m_frameParser.ConfigurationChanged -= m_frameParser_ConfigurationChanged;
                    m_frameParser.ConnectionAttempt -= m_frameParser_ConnectionAttempt;
                    m_frameParser.ConnectionEstablished -= m_frameParser_ConnectionEstablished;
                    m_frameParser.ConnectionException -= m_frameParser_ConnectionException;
                    m_frameParser.ConnectionTerminated -= m_frameParser_ConnectionTerminated;
                    m_frameParser.ExceededParsingExceptionThreshold -= m_frameParser_ExceededParsingExceptionThreshold;
                    m_frameParser.ParsingException -= m_frameParser_ParsingException;
                    m_frameParser.ReceivedConfigurationFrame -= m_frameParser_ReceivedConfigurationFrame;
                    m_frameParser.ReceivedDataFrame -= m_frameParser_ReceivedDataFrame;
                    m_frameParser.ReceivedHeaderFrame -= m_frameParser_ReceivedHeaderFrame;
                    m_frameParser.ReceivedFrameImage -= m_frameParser_ReceivedFrameImage;
                    m_frameParser.ReceivedFrameBufferImage -= m_frameParser_ReceivedFrameBufferImage;

                    if (m_frameParser != value)
                        m_frameParser.Dispose();
                }

                // Assign new frame parser reference
                m_frameParser = value;

                if (m_frameParser != null)
                {
                    // Attach to events on new frame parser reference
                    m_frameParser.ConfigurationChanged += m_frameParser_ConfigurationChanged;
                    m_frameParser.ConnectionAttempt += m_frameParser_ConnectionAttempt;
                    m_frameParser.ConnectionEstablished += m_frameParser_ConnectionEstablished;
                    m_frameParser.ConnectionException += m_frameParser_ConnectionException;
                    m_frameParser.ConnectionTerminated += m_frameParser_ConnectionTerminated;
                    m_frameParser.ExceededParsingExceptionThreshold += m_frameParser_ExceededParsingExceptionThreshold;
                    m_frameParser.ParsingException += m_frameParser_ParsingException;
                    m_frameParser.ReceivedConfigurationFrame += m_frameParser_ReceivedConfigurationFrame;
                    m_frameParser.ReceivedDataFrame += m_frameParser_ReceivedDataFrame;
                    m_frameParser.ReceivedHeaderFrame += m_frameParser_ReceivedHeaderFrame;
                    m_frameParser.ReceivedFrameImage += m_frameParser_ReceivedFrameImage;

                    // Only attach to full frame buffer reception event if data forwarding is enabled as attaching
                    // to this event engages an async queue to guarantee ordered delivery of buffer images
                    if (m_publishChannel != null || m_clientBasedPublishChannel != null)
                        m_frameParser.ReceivedFrameBufferImage += m_frameParser_ReceivedFrameBufferImage;
                }
            }
        }

        /// <summary>
        /// Gets or sets reference to <see cref="UdpServer"/> publication channel, attaching and/or detaching to events as needed.
        /// </summary>
        protected UdpServer UdpPublishChannel
        {
            get => m_publishChannel as UdpServer;
            set
            {
                UdpServer udpPublishChannel = m_publishChannel as UdpServer;

                if (m_publishChannel != null && udpPublishChannel == null)
                {
                    // Trying to dispose non-UDP publication channel - nothing to do...
                    if (value == null)
                        return;

                    // Publish channel is currently TCP, detach from TCP events
                    TcpPublishChannel = null;
                }

                if (udpPublishChannel != null)
                {
                    // Detach from events on existing data channel reference
                    udpPublishChannel.ClientConnectingException -= udpPublishChannel_ClientConnectingException;
                    udpPublishChannel.ReceiveClientDataComplete -= udpPublishChannel_ReceiveClientDataComplete;
                    udpPublishChannel.SendClientDataException -= udpPublishChannel_SendClientDataException;
                    udpPublishChannel.ServerStarted -= udpPublishChannel_ServerStarted;
                    udpPublishChannel.ServerStopped -= udpPublishChannel_ServerStopped;

                    if (udpPublishChannel != value)
                        udpPublishChannel.Dispose();
                }

                // Assign new UDP publish channel reference
                udpPublishChannel = value;

                if (udpPublishChannel != null)
                {
                    // Detach any existing client based publish channels
                    TcpClientPublishChannel = null;

                    // Attach to events on new data channel reference
                    udpPublishChannel.ClientConnectingException += udpPublishChannel_ClientConnectingException;
                    udpPublishChannel.ReceiveClientDataComplete += udpPublishChannel_ReceiveClientDataComplete;
                    udpPublishChannel.SendClientDataException += udpPublishChannel_SendClientDataException;
                    udpPublishChannel.ServerStarted += udpPublishChannel_ServerStarted;
                    udpPublishChannel.ServerStopped += udpPublishChannel_ServerStopped;
                }

                m_publishChannel = udpPublishChannel;
            }
        }

        /// <summary>
        /// Gets or sets reference to <see cref="TcpServer"/> publication channel, attaching and/or detaching to events as needed.
        /// </summary>
        protected TcpServer TcpPublishChannel
        {
            get => m_publishChannel as TcpServer;
            set
            {
                TcpServer tcpPublishChannel = m_publishChannel as TcpServer;

                if (m_publishChannel != null && tcpPublishChannel == null)
                {
                    // Trying to dispose non-TCP publication channel - nothing to do...
                    if (value == null)
                        return;

                    // Publish channel is currently UDP, detach from UDP events
                    UdpPublishChannel = null;
                }

                if (tcpPublishChannel != null)
                {
                    // Detach from events on existing command channel reference
                    tcpPublishChannel.ClientConnected -= tcpPublishChannel_ClientConnected;
                    tcpPublishChannel.ClientDisconnected -= tcpPublishChannel_ClientDisconnected;
                    tcpPublishChannel.ClientConnectingException -= tcpPublishChannel_ClientConnectingException;
                    tcpPublishChannel.ReceiveClientDataComplete -= tcpPublishChannel_ReceiveClientDataComplete;
                    tcpPublishChannel.SendClientDataException -= tcpPublishChannel_SendClientDataException;
                    tcpPublishChannel.ServerStarted -= tcpPublishChannel_ServerStarted;
                    tcpPublishChannel.ServerStopped -= tcpPublishChannel_ServerStopped;

                    if (tcpPublishChannel != value)
                        tcpPublishChannel.Dispose();
                }

                // Assign new TCP publish channel reference
                tcpPublishChannel = value;

                if (tcpPublishChannel != null)
                {
                    // Detach any existing client based publish channels
                    TcpClientPublishChannel = null;

                    // Attach to events on new command channel reference
                    tcpPublishChannel.ClientConnected += tcpPublishChannel_ClientConnected;
                    tcpPublishChannel.ClientDisconnected += tcpPublishChannel_ClientDisconnected;
                    tcpPublishChannel.ClientConnectingException += tcpPublishChannel_ClientConnectingException;
                    tcpPublishChannel.ReceiveClientDataComplete += tcpPublishChannel_ReceiveClientDataComplete;
                    tcpPublishChannel.SendClientDataException += tcpPublishChannel_SendClientDataException;
                    tcpPublishChannel.ServerStarted += tcpPublishChannel_ServerStarted;
                    tcpPublishChannel.ServerStopped += tcpPublishChannel_ServerStopped;
                }

                m_publishChannel = tcpPublishChannel;
            }
        }

        /// <summary>
        /// Gets or sets reference to <see cref="TcpClient"/> publication channel, attaching and/or detaching to events as needed.
        /// </summary>
        protected TcpClient TcpClientPublishChannel
        {
            get => m_clientBasedPublishChannel;
            set
            {
                if (m_clientBasedPublishChannel != null)
                {
                    // Detach from events on existing command channel reference
                    m_clientBasedPublishChannel.ConnectionEstablished -= tcpClientBasedPublishChannel_ConnectionEstablished;
                    m_clientBasedPublishChannel.ConnectionTerminated -= tcpClientBasedPublishChannel_ConnectionTerminated;
                    m_clientBasedPublishChannel.ConnectionException -= tcpClientBasedPublishChannel_ConnectionException;
                    m_clientBasedPublishChannel.ReceiveDataComplete -= tcpClientBasedPublishChannel_ReceiveDataComplete;
                    m_clientBasedPublishChannel.SendDataException -= tcpClientBasedPublishChannel_SendDataException;

                    if (m_clientBasedPublishChannel != value)
                        m_clientBasedPublishChannel.Dispose();
                }

                // Assign new TCP client based publish channel reference
                m_clientBasedPublishChannel = value;

                if (m_clientBasedPublishChannel != null)
                {
                    // Detach any existing server based publish channels
                    UdpPublishChannel = null;
                    TcpPublishChannel = null;

                    // Attach to events on new command channel reference
                    m_clientBasedPublishChannel.ConnectionEstablished += tcpClientBasedPublishChannel_ConnectionEstablished;
                    m_clientBasedPublishChannel.ConnectionTerminated += tcpClientBasedPublishChannel_ConnectionTerminated;
                    m_clientBasedPublishChannel.ConnectionException += tcpClientBasedPublishChannel_ConnectionException;
                    m_clientBasedPublishChannel.ReceiveDataComplete += tcpClientBasedPublishChannel_ReceiveDataComplete;
                    m_clientBasedPublishChannel.SendDataException += tcpClientBasedPublishChannel_SendDataException;
                }
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

                status.AppendFormat("    Source is concentrator: {0}", IsConcentrator);
                status.AppendLine();
                status.AppendFormat("Forwarding only connection: {0}", m_forwardOnly);
                status.AppendLine();

                if (!string.IsNullOrWhiteSpace(SharedMapping))
                {
                    status.AppendFormat("     Shared mapping source: {0}", SharedMapping);
                    status.AppendLine();
                }

                status.AppendFormat(" Source connection ID code: {0:N0} (index = {1:N0})", AccessID, m_serverIndex);
                status.AppendLine();
                status.AppendFormat("     Forcing label mapping: {0}", m_forceLabelMapping);
                status.AppendLine();
                status.AppendFormat("      Label mapped devices: {0:N0}", (object)m_labelDefinedDevices == null ? 0 : m_labelDefinedDevices.Count);
                status.AppendLine();
                status.AppendFormat("          Target time zone: {0}", TimeZone.Id);
                status.AppendLine();
                status.AppendFormat("    Manual time adjustment: {0:0.000} seconds", TimeAdjustmentTicks.ToSeconds());
                status.AppendLine();
                status.AppendFormat("Allow use of cached config: {0}", AllowUseOfCachedConfiguration);
                status.AppendLine();
                status.AppendFormat("No data reconnect interval: {0:0.000} seconds", Ticks.FromMilliseconds(m_dataStreamMonitor.Interval).ToSeconds());
                status.AppendLine();

                if (m_injectBadData)
                    status.AppendLine("   Injecting bad data flag: Yes - WARNING: Test mode enabled to override bad data flag");

                if (AllowUseOfCachedConfiguration)
                {
                    //                   123456789012345678901234567890
                    status.AppendFormat("   Cached config file name: {0}", FilePath.TrimFileName(ConfigurationCacheFileName, 51));
                    status.AppendLine();
                }

                status.AppendFormat("       Out of order frames: {0:N0}", OutOfOrderFrames);
                status.AppendLine();
                status.AppendFormat("           Minimum latency: {0:N0}ms over {1} tests", MinimumLatency, m_latencyFrames);
                status.AppendLine();
                status.AppendFormat("           Maximum latency: {0:N0}ms over {1} tests", MaximumLatency, m_latencyFrames);
                status.AppendLine();
                status.AppendFormat("           Average latency: {0:N0}ms over {1} tests", AverageLatency, m_latencyFrames);
                status.AppendLine();

                if (m_configurationFrame != null)
                {
                    status.AppendFormat("  Configuration frame size: {0:N0} bytes", m_configurationFrame.BinaryLength);
                    status.AppendLine();
                }

                if (m_frameParser != null)
                    status.Append(m_frameParser.Status);

                if (m_publishChannel != null)
                {
                    status.AppendLine();
                    status.AppendLine("Publication Channel Status".CenterText(50));
                    status.AppendLine("--------------------------".CenterText(50));
                    status.Append(m_publishChannel.Status);

                    if (m_publishChannel is TcpServer tcpPublishChannel)
                    {
                        Guid[] clientIDs = tcpPublishChannel.ClientIDs;

                        if (clientIDs != null && clientIDs.Length > 0)
                        {
                            status.AppendLine();
                            status.AppendFormat("TCP publish channel has {0} connected clients:\r\n\r\n", clientIDs.Length);

                            for (int i = 0; i < clientIDs.Length; i++)
                                status.AppendFormat("    {0}) {1}\r\n", i + 1, GetConnectionID(tcpPublishChannel, clientIDs[i]));

                            status.AppendLine();
                        }
                    }
                }

                if (m_clientBasedPublishChannel != null)
                {
                    status.AppendLine();
                    status.AppendLine("Publication Channel Status".CenterText(50));
                    status.AppendLine("--------------------------".CenterText(50));
                    status.Append(m_clientBasedPublishChannel.Status);
                }

                status.AppendLine();
                status.Append("Parsed Frame Quality Statistics".CenterText(78));
                status.AppendLine();
                status.AppendLine();
                //                      1         2         3         4         5         6         7
                //             123456789012345678901234567890123456789012345678901234567890123456789012345678
                status.Append("Device                  Bad Data   Bad Time    Frame      Total    Last Report");
                status.AppendLine();
                status.Append(" Name                    Frames     Frames     Errors     Frames      Time");
                status.AppendLine();
                //                      1         2            1          1          1          1          1
                //             1234567890123456789012 1234567890 1234567890 1234567890 1234567890 123456789012
                status.Append("---------------------- ---------- ---------- ---------- ---------- ------------");
                status.AppendLine();

                IConfigurationCell parsedDevice;
                string stationName;

                foreach (ConfigurationCell definedDevice in DefinedDevices)
                {
                    stationName = null;

                    // Attempt to lookup station name in configuration frame of connected device
                    if (m_frameParser?.ConfigurationFrame != null)
                    {
                        // Attempt to lookup by label (if defined), then by ID code
                        if (m_labelDefinedDevices != null && !string.IsNullOrWhiteSpace(definedDevice.StationName) &&
                            m_frameParser.ConfigurationFrame.Cells.TryGetByStationName(definedDevice.StationName, out parsedDevice) ||
                            m_frameParser.ConfigurationFrame.Cells.TryGetByIDCode(definedDevice.IDCode, out parsedDevice))
                            stationName = parsedDevice.StationName;
                    }

                    // We will default to defined name if parsed name is unavailable
                    if (string.IsNullOrWhiteSpace(stationName))
                        stationName = $"[{definedDevice.StationName.NotEmpty($"{definedDevice.IDLabel.NotEmpty("UNDEF")}:{definedDevice.IDCode}")}]";

                    status.Append(stationName.TruncateRight(22).PadRight(22));
                    status.Append(' ');
                    status.Append(definedDevice.DataQualityErrors.ToString("N0").CenterText(10));
                    status.Append(' ');
                    status.Append(definedDevice.TimeQualityErrors.ToString("N0").CenterText(10));
                    status.Append(' ');
                    status.Append(definedDevice.DeviceErrors.ToString("N0").CenterText(10));
                    status.Append(' ');
                    status.Append(definedDevice.TotalFrames.ToString("N0").CenterText(10));
                    status.Append(' ');
                    status.Append(definedDevice.LastReportTime.ToString("HH:mm:ss.fff"));
                    status.AppendLine();
                }

                status.AppendLine();
                status.AppendFormat("Undefined devices encountered: {0:N0}", m_undefinedDevices.Count);
                status.AppendLine();

                foreach (KeyValuePair<string, long> item in m_undefinedDevices)
                {
                    status.Append("    Device \"");
                    status.Append(item.Key);
                    status.Append("\" encountered ");
                    status.Append(item.Value);
                    status.Append(" times");
                    status.AppendLine();
                }

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="PhasorMeasurementMapper"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (m_disposed)
                return;

            try
            {
                if (!disposing)
                    return;

                // Detach from fowarding channel events and set any references to null
                TcpPublishChannel = null;
                UdpPublishChannel = null;
                TcpClientPublishChannel = null;

                // Detach from frame parser events and set reference to null
                FrameParser = null;

                if (m_dataStreamMonitor != null)
                {
                    m_dataStreamMonitor.Elapsed -= m_dataStreamMonitor_Elapsed;
                    m_dataStreamMonitor.Dispose();
                    m_dataStreamMonitor = null;
                }

                if (m_measurementCounter != null)
                {
                    m_measurementCounter.Elapsed -= m_measurementCounter_Elapsed;
                    m_measurementCounter.Dispose();
                    m_measurementCounter = null;
                }

                if (m_definedDevices != null)
                {
                    // Unregister each existing device from the statistics engine
                    foreach (ConfigurationCell device in DefinedDevices)
                        StatisticsEngine.Unregister(device);
                }

                if (m_missingDataMonitor != null)
                {
                    m_missingDataMonitor.Dispose();
                    m_missingDataMonitor = null;
                }
            }
            finally
            {
                m_disposed = true;          // Prevent duplicate dispose.
                base.Dispose(disposing);    // Call base class Dispose().
            }
        }

        /// <summary>
        /// Initializes <see cref="PhasorMeasurementMapper"/>.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            Dictionary<string, string> settings = Settings;

            // Load optional mapper specific connection parameters
            IsConcentrator = settings.TryGetValue("isConcentrator", out string setting) && setting.ParseBoolean();

            // Parse any defined server list, this assumes TCP connection since this is currently the only connection type that supports multiple end points
            if (settings.TryGetValue("server", out setting) && !string.IsNullOrWhiteSpace(setting))
                m_serverList = setting.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(server => $"tcp://{server.Trim()}".ToLower()).ToArray();
            else
                m_serverList = Array.Empty<string>();

            // Parse any defined access ID list, there must be one access ID for each defined server connection when multiple access IDs are specified
            if (settings.TryGetValue("accessID", out setting) && !string.IsNullOrWhiteSpace(setting))
            {
                List<ushort> accessIDList = new List<ushort>();
                string[] accessIDs = setting.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string accessID in accessIDs)
                {
                    if (ushort.TryParse(accessID.Trim(), out ushort id))
                        accessIDList.Add(id);
                }

                if (accessIDList.Count > 0 && accessIDList.Count == m_serverList.Length)
                {
                    m_accessIDList = accessIDList.ToArray();
                }
                else
                {
                    // Choose first value in list as access ID when there is a length mismatch
                    m_accessIDList = new[] { accessIDList.Count > 0 ? accessIDList[0] : (ushort)1 };

                    // Only display a warning when more than one access ID code is specified and there is a length mismatch with server connection list,
                    // the more common case is to define the same single access ID code for multiple server connections
                    if (accessIDList.Count > 1)
                    {
                        OnProcessException(MessageLevel.Warning, new InvalidOperationException(
                                $"Configured \"server\" connection list \"{string.Join(",", m_serverList)}\" with {m_serverList.Length:N0} entries does not match " +
                                $"configured \"accessID\" list \"{string.Join(",", accessIDList)}\" with {accessIDList.Count:N0} entries, as parsed. " +
                                $"Access ID used for all connections will be {m_accessIDList[0]:N0}."), nameof(Initialize));
                    }
                }
            }
            else
            {
                m_accessIDList = new[] { (ushort)1 };
            }

            m_forceLabelMapping = settings.TryGetValue("forceLabelMapping", out setting) && setting.ParseBoolean();

            // Commented out for security purposes related to Parent collection --
            // Replace with secure notification mechanism

            //if (settings.TryGetValue("primaryDataSource", out setting))
            //{
            //    uint primaryDataSourceID = 0;

            //    // Lookup adapter runtime ID of specified primary data source
            //    if (!string.IsNullOrWhiteSpace(setting))
            //    {
            //        try
            //        {
            //            DataRow[] filteredRows = DataSource.Tables["InputAdapters"].Select(string.Format("AdapterName = '{0}'", setting));

            //            if (filteredRows.Length > 0)
            //                primaryDataSourceID = uint.Parse(filteredRows[0]["ID"].ToString());
            //            else
            //                OnProcessException(new InvalidOperationException(string.Format("Failed to find input adapter ID for primary data source \"{0}\", data source was not established.", setting)));
            //        }
            //        catch (Exception ex)
            //        {
            //            OnProcessException(new InvalidOperationException(string.Format("Failed to find input adapter ID for primary data source \"{0}\" due to exception: {1} Data source was not established.", setting, ex.Message), ex));
            //        }
            //    }

            //    if (primaryDataSourceID > 0)
            //    {
            //        // Get matching data subscriber
            //        m_primaryDataSource = Parent.FirstOrDefault(adapter => adapter.ID == primaryDataSourceID && adapter is DataSubscriber) as DataSubscriber;

            //        if ((object)m_primaryDataSource == null)
            //        {
            //            OnProcessException(new InvalidOperationException(string.Format("Input adapter \"{0}\" specified to be the primary data source was not a DataSubscriber adapter, data source was not established.", setting)));
            //        }
            //        else
            //        {
            //            // Attach to connection events of primary source adapter since these will be used to control when back connection is established...
            //            m_primaryDataSource.ConnectionEstablished += m_primaryDataSource_ConnectionEstablished;
            //            m_primaryDataSource.ConnectionTerminated += m_primaryDataSource_ConnectionTerminated;
            //        }
            //    }
            //}

            SharedMapping = settings.TryGetValue("sharedMapping", out setting) ? setting.Trim() : null;

            if (settings.TryGetValue("timeZone", out setting) && !string.IsNullOrWhiteSpace(setting) && !setting.Trim().Equals("UTC", StringComparison.OrdinalIgnoreCase) && !setting.Trim().Equals("Coordinated Universal Time", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    TimeZone = TimeZoneInfo.FindSystemTimeZoneById(setting);
                }
                catch (Exception ex)
                {
                    OnProcessException(MessageLevel.Info, new InvalidOperationException($"Defaulting to UTC. Failed to find system time zone for ID \"{setting}\": {ex.Message}", ex));
                    TimeZone = TimeZoneInfo.Utc;
                }
            }
            else
            {
                TimeZone = TimeZoneInfo.Utc;
            }

            TimeAdjustmentTicks = settings.TryGetValue("timeAdjustmentTicks", out setting) ? long.Parse(setting) : 0L;

            m_dataStreamMonitor.Interval = settings.TryGetValue("dataLossInterval", out setting) ? (int)(double.Parse(setting) * 1000.0D) : 5000;

            if (settings.TryGetValue("delayedConnectionInterval", out setting))
            {
                double interval = double.Parse(setting) * 1000.0D;

                // Minimum delay is one millisecond
                if (interval < 1.0D)
                    interval = 1.0D;

                ConnectionAttemptInterval = interval;
            }
            else
            {
                ConnectionAttemptInterval = 1500.0D;
            }

            AllowUseOfCachedConfiguration = !settings.TryGetValue("allowUseOfCachedConfiguration", out setting) || setting.ParseBoolean();

            m_countOnlyMappedMeasurements = settings.TryGetValue("countOnlyMappedMeasurements", out setting) && setting.ParseBoolean();

            // Create a new phasor protocol frame parser for non-virtual connections
            MultiProtocolFrameParser frameParser = new MultiProtocolFrameParser
            {
                // Most of the parameters in the connection string will be for the data source in the frame parser
                // so we provide all of them, other parameters will simply be ignored
                ConnectionString = ConnectionString,

                // Since input adapter will automatically reconnect on connection exceptions, we need only to specify
                // that the frame parser try to connect once per connection attempt
                MaximumConnectionAttempts = 1
            };

            // For captured data simulations we will inject a simulated timestamp and auto-repeat file stream...
            if (frameParser.TransportProtocol == TransportProtocol.File)
            {
                frameParser.DefinedFrameRate = settings.TryGetValue("definedFrameRate", out setting) ? int.Parse(setting) : 30;
                frameParser.AutoRepeatCapturedPlayback = !settings.TryGetValue("autoRepeatFile", out setting) || setting.ParseBoolean();
                frameParser.UseHighResolutionInputTimer = settings.TryGetValue("useHighResolutionInputTimer", out setting) && setting.ParseBoolean();

                if (settings.TryGetValue("replayStartTime", out setting) && DateTime.TryParse(setting, out DateTime replayTime))
                    frameParser.ReplayStartTime = replayTime;
                else
                    frameParser.ReplayStartTime = DateTime.MinValue;

                if (settings.TryGetValue("replayStopTime", out setting) && DateTime.TryParse(setting, out replayTime))
                    frameParser.ReplayStopTime = replayTime;
                else
                    frameParser.ReplayStopTime = DateTime.MaxValue;
            }

            // Apply other settings as needed
            if (settings.TryGetValue("simulateTimestamp", out setting))
                frameParser.InjectSimulatedTimestamp = setting.ParseBoolean();
            else
                frameParser.InjectSimulatedTimestamp = frameParser.TransportProtocol == TransportProtocol.File;

            if (settings.TryGetValue("allowedParsingExceptions", out setting))
                frameParser.AllowedParsingExceptions = int.Parse(setting);

            if (settings.TryGetValue("parsingExceptionWindow", out setting))
                frameParser.ParsingExceptionWindow = Ticks.FromSeconds(double.Parse(setting));

            if (settings.TryGetValue("autoStartDataParsingSequence", out setting))
                frameParser.AutoStartDataParsingSequence = setting.ParseBoolean();

            if (settings.TryGetValue("skipDisableRealTimeData", out setting))
                frameParser.SkipDisableRealTimeData = setting.ParseBoolean();

            if (settings.TryGetValue("disableRealTimeDataOnStop", out setting))
                frameParser.DisableRealTimeDataOnStop = setting.ParseBoolean();

            if (!(settings.TryGetValue("lagTime", out setting) && double.TryParse(setting, out m_lagTime)))
                m_lagTime = 10.0D;

            if (!(settings.TryGetValue("leadTime", out setting) && double.TryParse(setting, out m_leadTime)))
                m_leadTime = 5.0D;

            if (!(settings.TryGetValue("timeResolution", out setting) && long.TryParse(setting, out m_timeResolution)))
                m_timeResolution = 10000L;

            // Provide access ID to frame parser as this may be necessary to make a phasor connection
            frameParser.DeviceID = AccessID;
            frameParser.SourceName = Name;

            // Check for forwarding settings which will establish a data forwarding channel (also supporting proxySettings parameter as used by stream splitter application)
            if ((settings.TryGetValue("forwardingSettings", out string forwardingSettings) || settings.TryGetValue("proxySettings", out forwardingSettings))  && !string.IsNullOrWhiteSpace(forwardingSettings))
            {
                if (forwardingSettings.ParseKeyValuePairs().TryGetValue("useClientPublishChannel", out setting) && setting.ParseBoolean())
                {
                    // Create a new client based publication channel (for reverse TCP connections)
                    TcpClientPublishChannel = ClientBase.Create(forwardingSettings) as TcpClient;

                    if (m_clientBasedPublishChannel != null)
                        m_clientBasedPublishChannel.MaxConnectionAttempts = -1;
                }
                else
                {
                    // Create a new server based publication channel
                    IServer publicationServer = ServerBase.Create(forwardingSettings);
                    TcpPublishChannel = publicationServer as TcpServer;
                    UdpPublishChannel = publicationServer as UdpServer;
                }
            }

            // Assign reference to frame parser for this connection and attach to needed events, this needs to
            // happen after establishing any forwarding operations so that frame parser can attach to needed
            // data parsing events
            FrameParser = frameParser;

            if (settings.TryGetValue("forwardOnly", out setting))
                m_forwardOnly = setting.ParseBoolean();

            // Load input devices associated with this connection
            LoadInputDevices();

            // Load active device measurements associated with this connection
            LoadDeviceMeasurements();

            // Register with the statistics engine
            StatisticsEngine.Register(this, "InputStream", "IS");
            StatisticsEngine.Calculated += (sender, args) => ResetLatencyCounters();
            StatisticsEngine.Calculated += (sender, args) => ResetMeasurementsPerSecondCounters();
        }

        // Load device list for this mapper connection
        private void LoadInputDevices()
        {
            ConfigurationCell definedDevice;
            string deviceName;

            if (m_definedDevices != null)
            {
                // Unregister each existing device from the statistics engine
                foreach (ConfigurationCell device in DefinedDevices)
                    StatisticsEngine.Unregister(device);
            }

            m_definedDevices = new ConcurrentDictionary<ushort, DeviceStatisticsHelper<ConfigurationCell>>();

            if (IsConcentrator)
            {
                StringBuilder deviceStatus = new StringBuilder();
                int index = 0;

                deviceStatus.AppendLine();
                deviceStatus.AppendLine();
                deviceStatus.Append("Loading expected concentrator device list...");
                deviceStatus.AppendLine();
                deviceStatus.AppendLine();

                // Making a connection to a concentrator that can support multiple devices
                foreach (DataRow row in DataSource.Tables["InputStreamDevices"].Select($"ParentID={SharedMappingID}"))
                {
                    // Create new configuration cell parsing needed ID code and label from input stream configuration
                    definedDevice = new ConfigurationCell(ushort.Parse(row["AccessID"].ToString()));
                    deviceName = row["Acronym"].ToNonNullString("[undefined]").Trim();

                    definedDevice.StationName = row["Name"].ToNonNullString(deviceName).Trim().TruncateRight(definedDevice.MaximumStationNameLength);
                    definedDevice.IDLabel = deviceName.TruncateRight(definedDevice.IDLabelLength);
                    definedDevice.Tag = uint.Parse(row["ID"].ToString());
                    definedDevice.Source = this;

                    bool deviceAdded = false;

                    if (m_forceLabelMapping)
                    {
                        // When forcing label mapping we always try to use label for unique lookup
                        if (m_labelDefinedDevices == null)
                            m_labelDefinedDevices = new ConcurrentDictionary<string, DeviceStatisticsHelper<ConfigurationCell>>(StringComparer.OrdinalIgnoreCase);

                        // See if label already exists in this collection
                        if (m_labelDefinedDevices.ContainsKey(definedDevice.StationName))
                        {
                            // For devices that do not have unique labels when forcing label mapping, we fall back on its ID code for unique lookup
                            if (m_definedDevices.ContainsKey(definedDevice.IDCode))
                            {
                                OnProcessException(MessageLevel.Error, new InvalidOperationException($"ERROR: Device ID \"{definedDevice.IDCode}\", labeled \"{definedDevice.StationName}\", was not unique in the {Name} input stream. Data from devices that are not distinctly defined by ID code or label will not be correctly parsed until uniquely identified."), flags: MessageFlags.UsageIssue);
                            }
                            else
                            {
                                m_definedDevices.TryAdd(definedDevice.IDCode, new DeviceStatisticsHelper<ConfigurationCell>(definedDevice));
                                RegisterStatistics(definedDevice, definedDevice.IDLabel, "Device", "PMU");
                                deviceAdded = true;
                            }
                        }
                        else
                        {
                            m_labelDefinedDevices.TryAdd(definedDevice.StationName, new DeviceStatisticsHelper<ConfigurationCell>(definedDevice));
                            RegisterStatistics(definedDevice, definedDevice.IDLabel, "Device", "PMU");
                            deviceAdded = true;
                        }
                    }
                    else
                    {
                        // See if key already exists in this collection
                        if (m_definedDevices.ContainsKey(definedDevice.IDCode))
                        {
                            // For devices that do not have unique ID codes, we fall back on its label for unique lookup
                            if (m_labelDefinedDevices == null)
                                m_labelDefinedDevices = new ConcurrentDictionary<string, DeviceStatisticsHelper<ConfigurationCell>>(StringComparer.OrdinalIgnoreCase);

                            if (m_labelDefinedDevices.ContainsKey(definedDevice.StationName))
                            {
                                OnProcessException(MessageLevel.Error, new InvalidOperationException($"Device ID \"{definedDevice.IDCode}\", labeled \"{definedDevice.StationName}\", was not unique in the {Name} input stream. Data from devices that are not distinctly defined by ID code or label will not be correctly parsed until uniquely identified."), flags: MessageFlags.UsageIssue);
                            }
                            else
                            {
                                m_labelDefinedDevices.TryAdd(definedDevice.StationName, new DeviceStatisticsHelper<ConfigurationCell>(definedDevice));
                                RegisterStatistics(definedDevice, definedDevice.IDLabel, "Device", "PMU");
                                deviceAdded = true;
                            }
                        }
                        else
                        {
                            m_definedDevices.TryAdd(definedDevice.IDCode, new DeviceStatisticsHelper<ConfigurationCell>(definedDevice));
                            RegisterStatistics(definedDevice, definedDevice.IDLabel, "Device", "PMU");
                            deviceAdded = true;
                        }
                    }

                    if (deviceAdded)
                    {
                        // Create status display string for expected device
                        deviceStatus.Append("   Device ");
                        deviceStatus.Append(index++.ToString("00"));
                        deviceStatus.Append(": ");
                        deviceStatus.Append(definedDevice.StationName);
                        deviceStatus.Append(" (");
                        deviceStatus.Append(definedDevice.IDCode);
                        deviceStatus.Append(')');
                        deviceStatus.AppendLine();
                    }
                }

                OnStatusMessage(MessageLevel.Info, deviceStatus.ToString());

                if (m_labelDefinedDevices != null)
                {
                    if (m_forceLabelMapping)
                        OnStatusMessage(MessageLevel.Info, $"{Name} has {m_labelDefinedDevices.Count:N0} defined input devices that are using the device label for identification since connection has been forced to use label mapping. This is not the optimal configuration.", flags: MessageFlags.UsageIssue);
                    else
                        OnStatusMessage(MessageLevel.Info, $"{Name} has {m_labelDefinedDevices.Count:N0} defined input devices that do not have unique ID codes (i.e., the AccessID), as a result system will use the device label for identification. This is not the optimal configuration.", flags: MessageFlags.UsageIssue);
                }
            }
            else
            {
                if (m_forceLabelMapping && m_accessIDList.Length > 1)
                    OnProcessException(MessageLevel.Warning, new InvalidOperationException($"Device configuration currently forces label mapping and has {m_accessIDList.Length:N0} access ID codes configured. Only the first access ID, {m_accessIDList[0]:N0}, will be used."), nameof(LoadInputDevices));

                // Making a connection to a single device, accommodating possible multiple access ID codes
                for (int i = 0; i < (m_forceLabelMapping ? 1 : m_accessIDList.Length); i++)
                {
                    definedDevice = new ConfigurationCell(m_accessIDList[i]);

                    // Used shared mapping name for single device connection if defined - this causes measurement mappings to be associated
                    // with alternate device by caching signal references associated with shared mapping acronym
                    deviceName = string.IsNullOrWhiteSpace(SharedMapping) ? Name.ToNonNullString("[undefined]").Trim() : SharedMapping;

                    definedDevice.StationName = deviceName.TruncateRight(definedDevice.MaximumStationNameLength);
                    definedDevice.IDLabel = deviceName.TruncateRight(definedDevice.IDLabelLength);
                    definedDevice.Tag = ID;
                    definedDevice.Source = this;

                    // When forcing label mapping we always try to use label for unique lookup instead of ID code
                    if (m_forceLabelMapping)
                    {
                        if (m_labelDefinedDevices == null)
                            m_labelDefinedDevices = new ConcurrentDictionary<string, DeviceStatisticsHelper<ConfigurationCell>>(StringComparer.OrdinalIgnoreCase);

                        m_labelDefinedDevices.TryAdd(definedDevice.StationName, new DeviceStatisticsHelper<ConfigurationCell>(definedDevice));
                        RegisterStatistics(definedDevice, definedDevice.IDLabel, "Device", "PMU");

                        OnStatusMessage(MessageLevel.Info, "Input device is using the device label for identification since connection has been forced to use label mapping. This is not the optimal configuration.");
                    }
                    else
                    {
                        m_definedDevices.TryAdd(definedDevice.IDCode, new DeviceStatisticsHelper<ConfigurationCell>(definedDevice));
                        RegisterStatistics(definedDevice, definedDevice.IDLabel, "Device", "PMU");
                    }
                }
            }
        }

        // Load active device measurements for this mapper connection
        private void LoadDeviceMeasurements()
        {
            Dictionary<string, MeasurementKey> definedMeasurements = new Dictionary<string, MeasurementKey>();

            foreach (DataRow row in DataSourceLookups.GetLookupCache(DataSource).ActiveMeasurements.LookupByDeviceID(SharedMappingID))
            {
                string signalReference = row["SignalReference"].ToString();
                string signalType = row["SignalType"].ToString();

                // Although statistics may be associated with device, it will not be this adapter producing them...
                if (string.IsNullOrWhiteSpace(signalReference) || string.Equals(signalType, "STAT", StringComparison.OrdinalIgnoreCase))
                    continue;

                try
                {
                    // Get measurement's signal ID
                    Guid signalID = new Guid(row["SignalID"].ToNonNullString(Guid.NewGuid().ToString()));

                    MeasurementKey key = MeasurementKey.LookUpOrCreate(signalID, row["ID"].ToString());

                    // Create a measurement with a reference associated with this adapter
                    MeasurementKey definedMeasurement = key;

                    // Add measurement to definition list keyed by signal reference
                    if (!definedMeasurements.ContainsKey(signalReference))
                        definedMeasurements.Add(signalReference, definedMeasurement);
                }
                catch (Exception ex)
                {
                    OnProcessException(MessageLevel.Warning, new InvalidOperationException($"Failed to load signal reference \"{signalReference}\" due to exception: {ex.Message}", ex), "Loading");
                }
            }

            m_definedMeasurements = definedMeasurements;

            // Cache signal reference name for connected device quality flags - in normal usage, name will not change for adapter lifetime
            definedMeasurements.TryGetValue(SignalReference.ToString(Name, SignalKind.Quality), out m_qualityFlagsKey);

            // Update output measurements that input adapter can provide such that it can participate in connect on demand
            OutputMeasurements = definedMeasurements.Count > 0 ? definedMeasurements.Values.Select(measurement => new Measurement { Metadata = measurement.Metadata } as IMeasurement).ToArray() : null;

            OnStatusMessage(MessageLevel.Info, $"Loaded {definedMeasurements.Count} active device measurements...", "Loading");
        }

        /// <summary>
        /// Toggles the flag that determines whether to inject the bad data state flag into the stream.
        /// </summary>
        [AdapterCommand("Toggles the flag that determines whether to inject the bad data state flag into the stream.", "Administrator", "Editor")]
        public void ToggleBadData()
        {
            m_injectBadData = !m_injectBadData;

            if (m_injectBadData)
                OnStatusMessage(MessageLevel.Warning, "TEST MODE ENABLED: Now overriding bad data flag, all published measurements for this device will now report bad data.");
            else
                OnStatusMessage(MessageLevel.Info, "TEST MODE DISABLED: Now publishing bad data flag as reported by device.");
        }

        /// <summary>
        /// Sends the specified <see cref="DeviceCommand"/> to the current device connection.
        /// </summary>
        /// <param name="command"><see cref="DeviceCommand"/> to send to connected device.</param>
        [AdapterCommand("Sends the specified command to connected phasor device.", "Administrator", "Editor")]
        public void SendCommand(DeviceCommand command)
        {
            if (m_frameParser != null)
            {
                if (m_frameParser.SendDeviceCommand(command) != null)
                    OnStatusMessage(MessageLevel.Info, $"Sent device command \"{command}\"...", "Command");
            }
            else
            {
                OnStatusMessage(MessageLevel.Info, $"Failed to send device command \"{command}\", no frame parser is defined.", "Command");
            }
        }

        /// <summary>
        /// Resets the statistics of all devices associated with this connection.
        /// </summary>
        [AdapterCommand("Resets the statistics of all devices associated with this connection.", "Administrator", "Editor")]
        public void ResetStatistics()
        {
            if (m_definedDevices != null)
            {
                foreach (ConfigurationCell definedDevice in DefinedDevices)
                {
                    definedDevice.DataQualityErrors = 0L;
                    definedDevice.DeviceErrors = 0L;
                    definedDevice.TotalFrames = 0L;
                    definedDevice.TimeQualityErrors = 0L;
                }

                OutOfOrderFrames = 0L;

                OnStatusMessage(MessageLevel.Info, "Statistics reset for all devices associated with this connection.", "Statistics");
            }
            else
            {
                OnStatusMessage(MessageLevel.Info, "Failed to reset statistics, no devices are defined.", "Statistics");
            }
        }

        /// <summary>
        /// Resets the statistics of the specified device associated with this connection.
        /// </summary>
        /// <param name="idCode">Integer ID code of device on which to reset statistics.</param>
        [AdapterCommand("Resets the statistics of the device with the specified ID code.", "Administrator", "Editor")]
        public void ResetDeviceStatistics(ushort idCode)
        {
            if (m_definedDevices != null)
            {
                if (m_definedDevices.TryGetValue(idCode, out DeviceStatisticsHelper<ConfigurationCell> statisticsHelper))
                {
                    ConfigurationCell definedDevice = statisticsHelper.Device;

                    definedDevice.DataQualityErrors = 0L;
                    definedDevice.DeviceErrors = 0L;
                    definedDevice.TotalFrames = 0L;
                    definedDevice.TimeQualityErrors = 0L;

                    OnStatusMessage(MessageLevel.Info, $"Statistics reset for device with ID code \"{idCode}\" associated with this connection.", "Statistics");
                }
                else
                {
                    OnStatusMessage(MessageLevel.Info, $"WARNING: Failed to find device with ID code \"{idCode}\" associated with this connection.", "Statistics");
                }
            }
            else
            {
                OnStatusMessage(MessageLevel.Info, "Failed to reset statistics, no devices are defined.", "Statistics");
            }
        }

        /// <summary>
        /// Resets the counters for the lifetime statistics without interrupting the adapter's operations.
        /// </summary>
        [AdapterCommand("Resets the counters for the lifetime statistics without interrupting the adapter's operations.", "Administrator", "Editor")]
        public virtual void ResetLifetimeCounters()
        {
            LifetimeMeasurements = 0L;
            m_lifetimeTotalLatency = 0L;
            m_lifetimeMinimumLatency = 0L;
            m_lifetimeMaximumLatency = 0L;
            m_lifetimeLatencyFrames = 0L;

            m_frameParser?.ResetTotalBytesReceived();
        }

        /// <summary>
        /// Resets counters related to latency calculations.
        /// </summary>
        [AdapterCommand("Resets the latency counters for the device without interrupting the adapter's operations.", "Administrator", "Editor")]
        public void ResetLatencyCounters()
        {
            m_minimumLatency = 0L;
            m_maximumLatency = 0L;
            m_totalLatency = 0L;
            m_latencyFrames = 0L;
        }

        /// <summary>
        /// Attempts to delete the last known good (i.e., cached) configuration.
        /// </summary>
        [AdapterCommand("Attempts to delete the last known good configuration.", "Administrator", "Editor")]
        public void DeleteCachedConfiguration()
        {
            lock (m_configurationOperationLock)
            {
                try
                {
                    ConfigurationFrame.DeleteCachedConfiguration(Name);
                    m_frameParser.ConfigurationFrame = null;
                    m_configurationFrame = null;
                    m_receivedConfigFrame = false;
                    OnStatusMessage(MessageLevel.Info, $"Cached configuration file \"{ConfigurationCacheFileName}\" was deleted.");
                    SendCommand(DeviceCommand.SendConfigurationFrame2);
                }
                catch (Exception ex)
                {
                    OnProcessException(MessageLevel.Info, new InvalidOperationException($"Failed to delete cached configuration \"{ConfigurationCacheFileName}\": {ex.Message}", ex));
                }
            }
        }

        /// <summary>
        /// Returns the current configuration frame to the caller.
        /// </summary>
        /// <returns>A <see cref="IConfigurationFrame"/> if successful, -or- <c>null</c> if request failed.</returns>
        [AdapterCommand("Requests the current configuration frame and returns it to the caller.", "Administrator", "Editor")]
        public IConfigurationFrame RequestCurrentConfiguration() => m_frameParser.ConfigurationFrame;

        /// <summary>
        /// Attempts to load the last known good (i.e., cached) configuration.
        /// </summary>
        [AdapterCommand("Attempts to load the last known good configuration.", "Administrator", "Editor")]
        public void LoadCachedConfiguration()
        {
            lock (m_configurationOperationLock)
            {
                try
                {
                    IConfigurationFrame configFrame = ConfigurationFrame.GetCachedConfiguration(Name, true);

                    // As soon as a configuration frame is made available to the frame parser, regardless of source,
                    // full parsing of data frames can begin...
                    if (configFrame != null)
                    {
                        m_frameParser.ConfigurationFrame = configFrame;
                        m_configurationFrame = configFrame;
                        m_receivedConfigFrame = true;

                        StartMeasurementCounter();
                        CheckForConfigurationChanges();
                    }
                    else
                    {
                        OnStatusMessage(MessageLevel.Info, $"NOTICE: Cannot load cached configuration, file \"{ConfigurationCacheFileName}\" does not exist.");
                    }
                }
                catch (Exception ex)
                {
                    OnProcessException(MessageLevel.Info, new InvalidOperationException($"Failed to load cached configuration \"{ConfigurationCacheFileName}\": {ex.Message}", ex));
                }
            }
        }

        /// <summary>
        /// Attempts to load the specified configuration.
        /// </summary>
        /// <param name="configurationFileName">Path and file name containing serialized configuration.</param>
        [AdapterCommand("Attempts to load the specified configuration.", "Administrator", "Editor")]
        public void LoadConfiguration(string configurationFileName)
        {
            if (string.IsNullOrWhiteSpace(configurationFileName))
                return;

            configurationFileName = configurationFileName.Trim();

            if (configurationFileName.Equals("LoadFromCache", StringComparison.OrdinalIgnoreCase))
            {
                LoadCachedConfiguration();
                return;
            }

            configurationFileName = FilePath.GetAbsolutePath(configurationFileName);

            lock (m_configurationOperationLock)
            {
                try
                {
                    IConfigurationFrame configFrame = ConfigurationFrame.GetCachedConfiguration(configurationFileName, false);

                    // As soon as a configuration frame is made available to the frame parser, regardless of source,
                    // full parsing of data frames can begin...
                    if (configFrame != null)
                    {
                        m_frameParser.ConfigurationFrame = configFrame;
                        m_configurationFrame = configFrame;
                        m_receivedConfigFrame = true;

                        try
                        {
                            // Cache this configuration frame since its being loaded as the new last known good configuration
                            ConfigurationFrame.Cache(configFrame, ex => OnProcessException(MessageLevel.Info, ex), Name);
                        }
                        catch (Exception ex)
                        {
                            // Process exception for logging
                            OnProcessException(MessageLevel.Warning, new InvalidOperationException($"Failed to queue caching of config frame due to exception: {ex.Message}", ex));
                        }

                        StartMeasurementCounter();
                        CheckForConfigurationChanges();
                    }
                    else
                    {
                        OnStatusMessage(MessageLevel.Info, $"NOTICE: Cannot load configuration, file \"{configurationFileName}\" does not exist.");
                    }
                }
                catch (Exception ex)
                {
                    OnProcessException(MessageLevel.Warning, new InvalidOperationException($"Failed to load configuration \"{configurationFileName}\": {ex.Message}", ex));
                }
            }
        }

        /// <summary>
        /// Gets a short one-line status of this <see cref="PhasorMeasurementMapper"/>.
        /// </summary>
        /// <param name="maxLength">Maximum number of available characters for display.</param>
        /// <returns>A short one-line summary of the current status of this <see cref="PhasorMeasurementMapper"/>.</returns>
        public override string GetShortStatus(int maxLength)
        {
            StringBuilder status = new StringBuilder();

            if (m_frameParser != null && m_frameParser.IsConnected)
            {
                if (LastReportTime > 0L)
                {
                    // Calculate total bad frames
                    long totalDataErrors = DefinedDevices.Sum(definedDevice => definedDevice.DataQualityErrors);

                    // Generate a short connect time
                    Time connectionTime = m_frameParser.ConnectionTime;
                    string upTime;

                    if (connectionTime.ToDays() < 1.0D)
                    {
                        if (connectionTime.ToHours() < 1.0D)
                            upTime = connectionTime.ToMinutes() < 1.0D ? $"{(int)connectionTime} seconds" : $"{connectionTime.ToMinutes():0.0} minutes";
                        else
                            upTime = $"{connectionTime.ToHours():0.00} hours";
                    }
                    else
                    {
                        upTime = $"{connectionTime.ToDays():0.00} days";
                    }

                    string uptimeStats = $"Up for {upTime}, {totalDataErrors} errors";
                    string runtimeStats = $" {LastReportTime:MM/dd/yyyy HH:mm:ss.fff} {m_frameParser.CalculatedFrameRate:0.00} fps";

                    uptimeStats = uptimeStats.TruncateRight(maxLength - runtimeStats.Length).PadLeft(maxLength - runtimeStats.Length, '\xA0');

                    status.Append(uptimeStats);
                    status.Append(runtimeStats);
                }
                else if (m_frameParser.ConfigurationFrame == null)
                {
                    status.AppendFormat("  >> Awaiting configuration frame - {0} bytes received", m_frameParser.TotalBytesReceived);
                }
                else
                {
                    status.AppendFormat("  ** No data parsed yet. Check mapping configuration - {0} bytes received", m_frameParser.TotalBytesReceived);
                }
            }
            else
            {
                status.Append("  ** Not connected");
            }

            return status.ToString();
        }

        /// <summary>
        /// Attempts to connect to data input source.
        /// </summary>
        protected override void AttemptConnection()
        {
            LastReportTime = 0L;
            m_bytesReceived = 0L;
            OutOfOrderFrames = 0L;
            m_receivedConfigFrame = false;
            m_cachedConfigLoadAttempted = false;

            // Load specific configuration file if one was specified
            if (Settings.TryGetValue("configurationFile", out string setting))
                LoadConfiguration(setting);

            // Start publication server
            m_publishChannel?.Start();
            m_clientBasedPublishChannel?.ConnectAsync();

            // Start frame parser
            m_frameParser?.Start();
        }

        /// <summary>
        /// Attempts to disconnect from data input source.
        /// </summary>
        protected override void AttemptDisconnection()
        {
            // Stop data stream monitor
            m_dataStreamMonitor.Enabled = false;

            // Stop frame parser
            m_frameParser?.Stop();

            // Stop publication server
            m_publishChannel?.Stop();
            m_clientBasedPublishChannel?.Disconnect();
            m_measurementCounter?.Stop();
        }

        /// <summary>
        /// Map parsed measurement value to defined measurement attributes (i.e., assign meta-data to parsed measured value).
        /// </summary>
        /// <param name="mappedMeasurements">Destination collection for the mapped measurement values.</param>
        /// <param name="metadata">The metadata to assign</param>
        /// <param name="parsedMeasurement">The parsed <see cref="IMeasurement"/> value.</param>
        protected void MapMeasurementAttributes(List<IMeasurement> mappedMeasurements, MeasurementMetadata metadata, IMeasurement parsedMeasurement)
        {
            if (metadata == MeasurementMetadata.Undefined)
                return;

            if (metadata == null)
                return;

            // Assign ID and other relevant attributes to the parsed measurement value
            parsedMeasurement.Metadata = metadata;

            // Add the updated measurement value to the destination measurement collection
            mappedMeasurements.Add(parsedMeasurement);
        }

        /// <summary>
        /// Extract frame measurements and add expose them via the <see cref="IInputAdapter.NewMeasurements"/> event.
        /// </summary>
        /// <param name="frame">Phasor data frame to extract measurements from.</param>
        protected void ExtractFrameMeasurements(IDataFrame frame)
        {
            const int AngleIndex = (int)CompositePhasorValue.Angle;
            const int MagnitudeIndex = (int)CompositePhasorValue.Magnitude;
            const int FrequencyIndex = (int)CompositeFrequencyValue.Frequency;
            const int DfDtIndex = (int)CompositeFrequencyValue.DfDt;

            List<IMeasurement> mappedMeasurements = new List<IMeasurement>(m_lastMeasurementMappedCount);
            List<IMeasurement> deviceMappedMeasurements = new List<IMeasurement>();

            // Adjust time to UTC based on source time zone
            if (!TimeZone.Equals(TimeZoneInfo.Utc))
                frame.Timestamp = TimeZoneInfo.ConvertTimeToUtc(frame.Timestamp, TimeZone);

            // We also allow "fine tuning" of time for fickle GPS clocks...
            if (TimeAdjustmentTicks.Value != 0)
                frame.Timestamp += TimeAdjustmentTicks;

            // Get adjusted timestamp of this frame
            Ticks timestamp = frame.Timestamp;

            // Track latest reporting time for mapper
            if (timestamp < LastReportTime)
                OutOfOrderFrames++;

            LastReportTime = timestamp;

            // Track latency statistics against system time - in order for these statistics
            // to be useful, the local clock must be fairly accurate
            long latency = frame.CreatedTimestamp - timestamp;

            // Throw out latencies that exceed one hour as invalid
            if (Math.Abs(latency) <= Time.SecondsPerHour * Ticks.PerSecond)
            {
                if (m_minimumLatency > latency || m_minimumLatency == 0)
                    m_minimumLatency = latency;

                if (m_maximumLatency < latency || m_maximumLatency == 0)
                    m_maximumLatency = latency;

                m_totalLatency += latency;
                m_latencyFrames++;

                if (m_lifetimeMinimumLatency > latency || m_lifetimeMinimumLatency == 0)
                    m_lifetimeMinimumLatency = latency;

                if (m_lifetimeMaximumLatency < latency || m_lifetimeMaximumLatency == 0)
                    m_lifetimeMaximumLatency = latency;

                m_lifetimeTotalLatency += latency;
                m_lifetimeLatencyFrames++;
            }

            // Map quality flags (QF) from device frame, if any
            MapMeasurementAttributes(mappedMeasurements, m_qualityFlagsKey?.Metadata, frame.GetQualityFlagsMeasurement());

            // Loop through each parsed device in the data frame
            foreach (IDataCell parsedDevice in frame.Cells)
            {
                try
                {
                    // Lookup device by its label (if needed), then by its ID code
                    if (m_labelDefinedDevices != null &&
                        m_labelDefinedDevices.TryGetValue(parsedDevice.StationName.ToNonNullString(), out DeviceStatisticsHelper<ConfigurationCell> statisticsHelper) ||
                        m_definedDevices.TryGetValue(parsedDevice.IDCode, out statisticsHelper))
                    {
                        ConfigurationCell definedDevice = statisticsHelper.Device;

                        // Track latest reporting time for this device
                        if (timestamp > definedDevice.LastReportTime && timestamp.UtcTimeIsValid(m_lagTime, m_leadTime))
                            definedDevice.LastReportTime = timestamp;

                        // Track quality statistics for this device
                        definedDevice.TotalFrames++;

                        if (m_injectBadData)
                            parsedDevice.DataIsValid = false;

                        if (!parsedDevice.DataIsValid)
                            definedDevice.DataQualityErrors++;

                        if (!parsedDevice.SynchronizationIsValid)
                            definedDevice.TimeQualityErrors++;

                        if (parsedDevice.DeviceError)
                            definedDevice.DeviceErrors++;

                        // Map status flags (SF) from device data cell itself
                        MapMeasurementAttributes(mappedMeasurements, definedDevice.GetMetadata(m_definedMeasurements, SignalKind.Status), parsedDevice.GetStatusFlagsMeasurement());

                        // Map phase angles (PAn) and magnitudes (PMn)
                        PhasorValueCollection phasors = parsedDevice.PhasorValues;
                        int count = phasors.Count;
                        IMeasurement[] measurements;

                        for (int x = 0; x < count; x++)
                        {
                            // Get composite phasor measurements
                            measurements = phasors[x].Measurements;

                            // Map angle
                            MapMeasurementAttributes(deviceMappedMeasurements, definedDevice.GetMetadata(m_definedMeasurements, SignalKind.Angle, x, count), measurements[AngleIndex]);

                            // Map magnitude
                            MapMeasurementAttributes(deviceMappedMeasurements, definedDevice.GetMetadata(m_definedMeasurements, SignalKind.Magnitude, x, count), measurements[MagnitudeIndex]);
                        }

                        // Map frequency (FQ) and dF/dt (DF)
                        measurements = parsedDevice.FrequencyValue.Measurements;

                        // Map frequency
                        MapMeasurementAttributes(deviceMappedMeasurements, definedDevice.GetMetadata(m_definedMeasurements, SignalKind.Frequency), measurements[FrequencyIndex]);

                        // Map dF/dt
                        MapMeasurementAttributes(deviceMappedMeasurements, definedDevice.GetMetadata(m_definedMeasurements, SignalKind.DfDt), measurements[DfDtIndex]);

                        // Map analog values (AVn)
                        AnalogValueCollection analogs = parsedDevice.AnalogValues;
                        count = analogs.Count;

                        // Map analog values
                        for (int x = 0; x < count; x++)
                            MapMeasurementAttributes(deviceMappedMeasurements, definedDevice.GetMetadata(m_definedMeasurements, SignalKind.Analog, x, count), analogs[x].Measurements[0]);

                        // Map digital values (DVn)
                        DigitalValueCollection digitals = parsedDevice.DigitalValues;
                        count = digitals.Count;

                        // Map digital values
                        for (int x = 0; x < count; x++)
                            MapMeasurementAttributes(deviceMappedMeasurements, definedDevice.GetMetadata(m_definedMeasurements, SignalKind.Digital, x, count), digitals[x].Measurements[0]);

                        // Track measurement count statistics for this device
                        if (m_countOnlyMappedMeasurements)
                        {
                            statisticsHelper.AddToMeasurementsReceived(CountMappedMeasurements(parsedDevice, deviceMappedMeasurements));
                            statisticsHelper.AddToMeasurementsWithError(CountMappedMeasurementsWithError(parsedDevice, deviceMappedMeasurements));
                        }
                        else
                        {
                            statisticsHelper.AddToMeasurementsReceived(CountParsedMeasurements(parsedDevice));
                            statisticsHelper.AddToMeasurementsWithError(CountParsedMeasurementsWithError(parsedDevice));
                        }

                        mappedMeasurements.AddRange(deviceMappedMeasurements);
                        deviceMappedMeasurements.Clear();
                    }
                    else
                    {
                        // Encountered an undefined device, track frame counts
                        if (m_undefinedDevices.TryGetValue(parsedDevice.StationName, out long frameCount))
                        {
                            frameCount++;
                            m_undefinedDevices[parsedDevice.StationName] = frameCount;
                        }
                        else
                        {
                            m_undefinedDevices.TryAdd(parsedDevice.StationName, 1);
                            OnStatusMessage(MessageLevel.Warning, $"Encountered an undefined device \"{parsedDevice.StationName}\"...");
                        }
                    }
                }
                catch (Exception ex)
                {
                    OnProcessException(MessageLevel.Warning, new InvalidOperationException($"Exception encountered while mapping \"{Name}\" data frame cell \"{parsedDevice.StationName.ToNonNullString("[undefined]")}\" elements to measurements: {ex.Message}", ex));
                }
            }

            m_lastMeasurementMappedCount = mappedMeasurements.Count;

            // Provide real-time measurements where needed
            OnNewMeasurements(mappedMeasurements);

            int measurementCount = mappedMeasurements.Count;
            LifetimeMeasurements += measurementCount;
            UpdateMeasurementsPerSecond(measurementCount);
        }

        private int CountMappedMeasurements(IDataCell parsedDevice, List<IMeasurement> mappedMeasurements)
        {
            IFrequencyValue frequencyValue = parsedDevice.FrequencyValue;

            // Ignore frequency measurements when
            // frequency value is zero - some PDCs use
            // zero for missing frequency values
            return frequencyValue.Frequency == 0.0D
                ? mappedMeasurements.Count(measurement => !double.IsNaN(measurement.Value) && frequencyValue.Measurements.All(freq => measurement.Key != freq.Key))
                : mappedMeasurements.Count(measurement => !double.IsNaN(measurement.Value));
        }

        private int CountMappedMeasurementsWithError(IDataCell parsedDevice, List<IMeasurement> mappedMeasurements)
        {
            const MeasurementStateFlags ErrorFlags = MeasurementStateFlags.BadData | MeasurementStateFlags.BadTime | MeasurementStateFlags.SystemError;
            bool hasError(MeasurementStateFlags stateFlags) => (stateFlags & ErrorFlags) != MeasurementStateFlags.Normal;
            IFrequencyValue frequencyValue = parsedDevice.FrequencyValue;

            // Ignore frequency measurements when
            // frequency value is zero - some PDCs use
            // zero for missing frequency values
            return frequencyValue.Frequency == 0.0D
                ? mappedMeasurements.Count(measurement => hasError(measurement.StateFlags) && !double.IsNaN(measurement.Value) && frequencyValue.Measurements.All(freq => measurement.Key != freq.Key))
                : mappedMeasurements.Count(measurement => hasError(measurement.StateFlags) && !double.IsNaN(measurement.Value));
        }

        // Count parsed measurements for this device
        private int CountParsedMeasurements(IDataCell parsedDevice)
        {
            if (parsedDevice == null)
                return 0;

            int parsedMeasurementCount = 0;

            // ReSharper disable once UselessBinaryOperation
            parsedMeasurementCount += parsedDevice.PhasorValues?.Sum(phasorValue => phasorValue.Measurements.Count(measurement => !double.IsNaN(measurement.Value))) ?? 0;
            parsedMeasurementCount += parsedDevice.DigitalValues?.Sum(digitalValue => digitalValue.Measurements.Count(measurement => !double.IsNaN(measurement.Value))) ?? 0;
            parsedMeasurementCount += parsedDevice.AnalogValues?.Sum(analogValue => analogValue.Measurements.Count(measurement => !double.IsNaN(measurement.Value))) ?? 0;

            // Ignore frequency measurements when
            // frequency value is zero - some PDCs use
            // zero for missing frequency values
            IFrequencyValue frequencyValue = parsedDevice.FrequencyValue;

            if ((frequencyValue?.Frequency ?? 0.0D) != 0.0D)
                parsedMeasurementCount += frequencyValue.Measurements.Count(measurement => !double.IsNaN(measurement.Value));

            return parsedMeasurementCount;
        }

        // Count parsed measurements with errors for this device
        private int CountParsedMeasurementsWithError(IDataCell parsedDevice)
        {
            if (parsedDevice == null)
                return 0;

            const MeasurementStateFlags ErrorFlags = MeasurementStateFlags.BadData | MeasurementStateFlags.BadTime | MeasurementStateFlags.SystemError;
            bool hasError(MeasurementStateFlags stateFlags) => (stateFlags & ErrorFlags) != MeasurementStateFlags.Normal;

            int parsedMeasurementCount = 0;

            // ReSharper disable once UselessBinaryOperation
            parsedMeasurementCount += parsedDevice.PhasorValues?.Sum(phasorValue => phasorValue.Measurements.Count(measurement => hasError(measurement.StateFlags) && !double.IsNaN(measurement.Value))) ?? 0;
            parsedMeasurementCount += parsedDevice.DigitalValues?.Sum(digitalValue => digitalValue.Measurements.Count(measurement => hasError(measurement.StateFlags) && !double.IsNaN(measurement.Value))) ?? 0;
            parsedMeasurementCount += parsedDevice.AnalogValues?.Sum(analogValue => analogValue.Measurements.Count(measurement => hasError(measurement.StateFlags) && !double.IsNaN(measurement.Value))) ?? 0;

            // Ignore frequency measurements when
            // frequency value is zero - some PDCs use
            // zero for missing frequency values
            IFrequencyValue frequencyValue = parsedDevice.FrequencyValue;

            if ((frequencyValue?.Frequency ?? 0.0D) != 0.0D)
                parsedMeasurementCount += frequencyValue.Measurements.Count(measurement => hasError(measurement.StateFlags) && !double.IsNaN(measurement.Value));

            return parsedMeasurementCount;
        }

        /// <summary>
        /// Get signal reference for specified <see cref="SignalKind"/>.
        /// </summary>
        /// <param name="type"><see cref="SignalKind"/> to request signal reference for.</param>
        /// <returns>Signal reference of given <see cref="SignalKind"/>.</returns>
        public string GetSignalReference(SignalKind type)
        {
            // We cache non-indexed signal reference strings so they don't need to be generated at each mapping call.

            // Look up synonym in dictionary based on signal type, if found return single element
            if (m_generatedSignalReferenceCache.TryGetValue(type, out string[] references))
                return references[0];

            // Create and cache new non-indexed signal reference (for single element)
            references = new[] { SignalReference.ToString($"{Name}!IS", type) };

            // Cache generated signal synonym
            m_generatedSignalReferenceCache.TryAdd(type, references);

            return references[0];
        }

        /// <summary>
        /// Get signal reference for specified <see cref="SignalKind"/> and <paramref name="index"/>.
        /// </summary>
        /// <param name="type"><see cref="SignalKind"/> to request signal reference for.</param>
        /// <param name="index">Index <see cref="SignalKind"/> to request signal reference for.</param>
        /// <param name="count">Number of signals defined for this <see cref="SignalKind"/>.</param>
        /// <returns>Signal reference of given <see cref="SignalKind"/> and <paramref name="index"/>.</returns>
        public string GetSignalReference(SignalKind type, int index, int count)
        {
            // We cache indexed signal reference strings so they don't need to be generated at each mapping call.
            // For speed purposes we intentionally do not validate that signalIndex falls within signalCount, be
            // sure calling procedures are very careful with parameters...

            // Look up synonym in dictionary based on signal type
            if (m_generatedSignalReferenceCache.TryGetValue(type, out string[] references))
            {
                // Verify signal count has not changed (we may have received new configuration from device)
                if (count == references.Length)
                {
                    // Create and cache new signal reference if it doesn't exist
                    if ((object)references[index] == null)
                        references[index] = SignalReference.ToString($"{Name}!IS", type, index + 1);

                    return references[index];
                }
            }

            // Create a new indexed signal reference array
            references = new string[count];

            // Create and cache new signal reference
            references[index] = SignalReference.ToString($"{Name}!IS", type, index + 1);

            // Cache generated signal synonym array
            m_generatedSignalReferenceCache.TryAdd(type, references);

            return references[index];
        }

        private void StartMeasurementCounter()
        {
            long now = DateTime.UtcNow.Ticks;

            if (!m_receivedConfigFrame)
            {
                // If this is the first time we've received the configuration frame,
                // we'll use it to calculate expected measurements per second for each device
                DataSet dataSource = DataSource;

                if (dataSource != null && dataSource.Tables.Contains("ActiveMeasurements"))
                {
                    DataTable measurementTable = dataSource.Tables["ActiveMeasurements"];
                    IConfigurationFrame configurationFrame = m_frameParser.ConfigurationFrame;

                    foreach (DeviceStatisticsHelper<ConfigurationCell> statisticsHelper in StatisticsHelpers)
                    {
                        int measurementsPerFrame = measurementTable.Select($"SignalReference LIKE '{statisticsHelper.Device.IDLabel}-%' AND SignalType <> 'FLAG' AND SignalType <> 'STAT'").Length;
                        
                        statisticsHelper.Device.MeasurementsDefined = measurementsPerFrame;
                        statisticsHelper.ExpectedMeasurementsPerSecond = configurationFrame.FrameRate * measurementsPerFrame;
                        statisticsHelper.Reset(now);
                    }
                }
            }

            // Create the measurement counter timer if it doesn't already exist
            if (m_measurementCounter == null)
            {
                m_measurementCounter = CommonPhasorServices.TimerScheduler.CreateTimer(1000);
                m_measurementCounter.Elapsed += m_measurementCounter_Elapsed;
            }

            // Start the measurement counter timer
            // to start gathering statistics
            m_measurementCounter.Start();
        }

        // Updates the measurements per second counters after receiving another set of measurements.
        private void UpdateMeasurementsPerSecond(int measurementCount)
        {
            long secondsSinceEpoch = DateTime.UtcNow.Ticks / Ticks.PerSecond;

            if (secondsSinceEpoch > m_lastSecondsSinceEpoch)
            {
                if (m_measurementsInSecond < MinimumMeasurementsPerSecond || MinimumMeasurementsPerSecond == 0L)
                    MinimumMeasurementsPerSecond = m_measurementsInSecond;

                if (m_measurementsInSecond > MaximumMeasurementsPerSecond || MaximumMeasurementsPerSecond == 0L)
                    MaximumMeasurementsPerSecond = m_measurementsInSecond;

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
            MinimumMeasurementsPerSecond = 0L;
            MaximumMeasurementsPerSecond = 0L;
            m_totalMeasurementsPerSecond = 0L;
            m_measurementsPerSecondCount = 0L;
        }

        // Replace with secure notification mechanism handlers for notification when subordinate phasor adapter needs to kick on when primary data feed goes offline (if feature is still deemed useful)

        //// Primary data source connection has terminated, engage backup connection
        //private void m_primaryDataSource_ConnectionTerminated(object sender, EventArgs e)
        //{
        //    OnStatusMessage("WARNING: Primary data source connection was terminated, attempting to engage backup connection...");
        //    Start();
        //}

        //// Primary data source connection has been reestablished, disengage backup connection
        //private void m_primaryDataSource_ConnectionEstablished(object sender, EventArgs e)
        //{
        //    if (Enabled)
        //        OnStatusMessage("Primary data source connection has been reestablished, disengaging backup connection...");

        //    Stop();
        //}

        /// <summary>
        /// Gets connection ID (i.e., IP:Port) for specified <paramref name="clientID"/>.
        /// </summary>
        /// <param name="server">Server connection of associated <paramref name="clientID"/>.</param>
        /// <param name="clientID">Guid of client for ID lookup.</param>
        /// <returns>Connection ID (i.e., IP:Port) for specified <paramref name="clientID"/>.</returns>
        protected virtual string GetConnectionID(IServer server, Guid clientID)
        {
            if ((server == null || clientID.Equals(Guid.Empty)) && m_clientBasedPublishChannel != null)
                return m_clientBasedPublishChannel.ServerUri;

            if (m_connectionIDCache.TryGetValue(clientID, out string connectionID))
                return connectionID;

            // Attempt to lookup remote connection identification for logging purposes
            try
            {
                IPEndPoint remoteEndPoint = null;

                switch (server)
                {
                    case TcpServer commandChannel when commandChannel.TryGetClient(clientID, out TransportProvider<Socket> tcpClient):
                        remoteEndPoint = tcpClient.Provider?.RemoteEndPoint as IPEndPoint;
                        break;
                    case UdpServer dataChannel when dataChannel.TryGetClient(clientID, out TransportProvider<EndPoint> udpClient):
                        remoteEndPoint = udpClient.Provider as IPEndPoint;
                        break;
                }

                if (remoteEndPoint != null)
                {
                    connectionID = remoteEndPoint.AddressFamily == AddressFamily.InterNetworkV6 ? $"[{remoteEndPoint.Address}]:{remoteEndPoint.Port}" : $"{remoteEndPoint.Address}:{remoteEndPoint.Port}";

                    // Cache value for future lookup
                    m_connectionIDCache.TryAdd(clientID, connectionID);
                    ThreadPool.QueueUserWorkItem(LookupHostName, new Tuple<Guid, string, IPEndPoint>(clientID, connectionID, remoteEndPoint));
                }
            }
            catch (Exception ex)
            {
                OnProcessException(MessageLevel.Warning, new InvalidOperationException($"Failed to lookup remote end-point connection information for client data transmission due to exception: {ex.Message}", ex));
            }

            if (string.IsNullOrEmpty(connectionID))
                connectionID = "unavailable";

            return connectionID;
        }

        private void LookupHostName(object state)
        {
            try
            {
                Tuple<Guid, string, IPEndPoint> parameters = (Tuple<Guid, string, IPEndPoint>)state;
                IPHostEntry ipHost = Dns.GetHostEntry(parameters.Item3.Address);

                if (!string.IsNullOrWhiteSpace(ipHost.HostName))
                    m_connectionIDCache[parameters.Item1] = $"{ipHost.HostName} ({parameters.Item2})";
            }
            catch (Exception ex)
            {
                // Just ignoring possible DNS lookup failures...
                Logger.SwallowException(ex, "DNS Lookup Failure");
            }
        }

        /// <summary>
        /// Handles incoming commands from devices connected over the command channel.
        /// </summary>
        /// <param name="clientID">Guid of client that sent the command.</param>
        /// <param name="connectionID">Remote client connection identification (i.e., IP:Port).</param>
        /// <param name="commandBuffer">Data buffer received from connected client device.</param>
        /// <param name="length">Valid length of data within the buffer.</param>
        protected virtual void DeviceCommandHandler(Guid clientID, string connectionID, byte[] commandBuffer, int length)
        {
            try
            {
                ICommandFrame commandFrame;

                // Attempt to interpret data received from a client as a command frame
                switch (m_frameParser.PhasorProtocol)
                {
                    case PhasorProtocol.IEEEC37_118V2:
                    case PhasorProtocol.IEEEC37_118V1:
                    case PhasorProtocol.IEEEC37_118D6:
                        commandFrame = new GSF.PhasorProtocols.IEEEC37_118.CommandFrame(commandBuffer, 0, length);
                        break;
                    case PhasorProtocol.IEEE1344:
                        commandFrame = new GSF.PhasorProtocols.IEEE1344.CommandFrame(commandBuffer, 0, length);
                        break;
                    case PhasorProtocol.SelFastMessage:
                        commandFrame = new GSF.PhasorProtocols.SelFastMessage.CommandFrame(commandBuffer, 0, length);
                        break;
                    case PhasorProtocol.IEC61850_90_5:
                        commandFrame = new GSF.PhasorProtocols.IEC61850_90_5.CommandFrame(commandBuffer, 0, length);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException($"Protocol \"{m_frameParser.PhasorProtocol}\" does not support commands.");
                }

                switch (commandFrame.Command)
                {
                    case DeviceCommand.SendConfigurationFrame1:
                    case DeviceCommand.SendConfigurationFrame2:
                        // Reset received configuration frame counter
                        m_receivedConfigurationFrames = 0;

                        if (m_configurationFrame != null)
                        {
                            if (m_publishChannel != null)
                                m_publishChannel.SendToAsync(clientID, m_configurationFrame.BinaryImage(), 0, m_configurationFrame.BinaryLength);
                            else
                                m_clientBasedPublishChannel?.SendAsync(m_configurationFrame.BinaryImage(), 0, m_configurationFrame.BinaryLength);
   
                            OnStatusMessage(MessageLevel.Info, $"Received request for \"{commandFrame.Command}\" from \"{connectionID}\" - frame was returned.");
                        }

                        break;
                    case DeviceCommand.EnableRealTimeData:
                    case DeviceCommand.DisableRealTimeData:
                        // We ignore these commands without message, these commands are normally sent by synchrophasor devices
                        // but we do not allow stream control in a proxy situation
                        break;
                    default:
                        OnStatusMessage(MessageLevel.Info, $"Request for \"{commandFrame.Command}\" from \"{connectionID}\" was ignored - device command is unsupported for data forwarding.");
                        break;
                }
            }
            catch (Exception ex)
            {
                OnProcessException(MessageLevel.Warning, new InvalidOperationException($"Remotely connected device \"{connectionID}\" sent an unrecognized data sequence to the data forwarding engine, no action was taken. Exception details: {ex.Message}", ex));
            }
        }

        // Thread procedure used to proxy data to the user implemented device command handler
        private void DeviceCommandHandlerProc(object state)
        {
            if (state is EventArgs<Guid, byte[], int> e)
                DeviceCommandHandler(e.Argument1, GetConnectionID(m_publishChannel, e.Argument1), e.Argument2, e.Argument3);
        }

        private void m_dataStreamMonitor_Elapsed(object sender, EventArgs<DateTime> e)
        {
            if (m_bytesReceived == 0 && (m_frameParser.DeviceSupportsCommands || m_frameParser.ConnectionIsMulticast || m_frameParser.ConnectionIsListener))
            {
                // If we've received no data in the last time-span, we restart connect cycle...
                m_dataStreamMonitor.Enabled = false;
                OnStatusMessage(MessageLevel.Info, $"\r\nNo data received in {m_dataStreamMonitor.Interval / 1000.0D:0.0} seconds, restarting connect cycle...\r\n", "Connection Issues");
                Start();
            }
            else if (!m_receivedConfigFrame && AllowUseOfCachedConfiguration)
            {
                // If data is being received but a configuration has yet to be loaded, attempt to load last known good configuration
                if (!m_cachedConfigLoadAttempted)
                {
                    OnStatusMessage(MessageLevel.Info, "Configuration frame has yet to be received, attempting to load cached configuration...");
                    m_cachedConfigLoadAttempted = true;
                    LoadCachedConfiguration();
                }
                else if (m_frameParser.DeviceSupportsCommands)
                {
                    OnStatusMessage(MessageLevel.Info, "\r\nConfiguration frame has yet to be received even after attempt to load from cache, restarting connect cycle...\r\n");
                    Start();
                }
            }

            m_bytesReceived = 0;
        }

        private void m_measurementCounter_Elapsed(object sender, EventArgs<DateTime> elapsedEventArgs)
        {
            long now = DateTime.UtcNow.Ticks;
            IEnumerable<DeviceStatisticsHelper<ConfigurationCell>> statisticsHelpers = StatisticsHelpers;

            foreach (DeviceStatisticsHelper<ConfigurationCell> statisticsHelper in statisticsHelpers)
                statisticsHelper.Update(now);
        }

        private void RegisterStatistics(object source, string sourceName, string sourceCategory, string sourceAcronym)
        {
            if (string.IsNullOrWhiteSpace(SharedMapping))
                StatisticsEngine.Register(source, sourceName, sourceCategory, sourceAcronym);
        }

        // Compare last configuration to new received configuration frame to see if there have been any changes
        private bool CheckForConfigurationChanges()
        {
            IConfigurationFrame newConfigurationFrame = m_frameParser?.ConfigurationFrame;
            bool configurationChanged = false;

            if (newConfigurationFrame == null)
                return false;

            if (m_lastConfigurationFrame == null)
            {
                ConfigurationChanges++;
                configurationChanged = true;
            }
            else
            {
                // Check binary image lengths first, if these are different there is already a change
                if (m_lastConfigurationFrame.BinaryLength != newConfigurationFrame.BinaryLength)
                {
                    ConfigurationChanges++;
                    configurationChanged = true;
                }
                else
                {
                    // Make sure timestamps are identical since this should not count against comparison
                    m_lastConfigurationFrame.Timestamp = newConfigurationFrame.Timestamp;

                    // Generate binary images for the configuration frames
                    byte[] lastConfigFrameBuffer = new byte[m_lastConfigurationFrame.BinaryLength];
                    byte[] newConfigFrameBuffer = new byte[newConfigurationFrame.BinaryLength];

                    try
                    {
                        m_lastConfigurationFrame.GenerateBinaryImage(lastConfigFrameBuffer, 0);
                    }
                    catch (Exception ex)
                    {
                        OnStatusMessage(MessageLevel.Info, $"WARNING: Failed to generate a binary image for cached configuration frame - clearing cache. Exception reported: {ex.Message}");
                        m_lastConfigurationFrame = null;
                    }

                    try
                    {
                        newConfigurationFrame.GenerateBinaryImage(newConfigFrameBuffer, 0);
                    }
                    catch (Exception ex)
                    {
                        OnStatusMessage(MessageLevel.Info, $"WARNING: Failed to generate a binary image for received configuration frame: {ex.Message}");
                        return false;
                    }

                    // Compare the binary images - if they are different, this counts as a configuration change
                    if (m_lastConfigurationFrame == null || !lastConfigFrameBuffer.SequenceEqual(newConfigFrameBuffer))
                    {
                        ConfigurationChanges++;
                        configurationChanged = true;
                    }
                }
            }

            m_lastConfigurationFrame = newConfigurationFrame;

            return configurationChanged;
        }

        #region [ Frame Parser Event Handlers ]

        // Redistribute received data.
        private void m_frameParser_ReceivedFrameBufferImage(object sender, EventArgs<FundamentalFrameType, byte[], int, int> e)
        {
            if (m_publishChannel == null && m_clientBasedPublishChannel == null)
                return;

            byte[] image;

            // Track total number of received configuration frames - data source could be auto-sending config frames every minute
            if (e.Argument1 == FundamentalFrameType.ConfigurationFrame)
                m_receivedConfigurationFrames++;

            // Send the configuration frame at the top of each minute if publication channel is UDP and source is not auto-sending configuration frames
            if (m_receivedConfigurationFrames < 2 && m_publishChannel is UdpServer && m_configurationFrame != null)
            {
                DateTime currentTime = DateTime.UtcNow;

                if (currentTime.Second == 0)
                {
                    if (currentTime.Minute != m_lastConfigurationPublishMinute)
                    {
                        m_lastConfigurationPublishMinute = currentTime.Minute;
                        m_configurationFramePublished = false;
                    }

                    if (!m_configurationFramePublished)
                    {
                        // Publish binary image for configuration frame
                        m_configurationFramePublished = true;
                        m_configurationFrame.Timestamp = currentTime;

                        image = m_configurationFrame.BinaryImage();

                        try
                        {
                            m_publishChannel.MulticastAsync(image, 0, image.Length);
                        }
                        catch (Exception ex)
                        {
                            OnProcessException(MessageLevel.Error, new InvalidOperationException($"Server based publication channel exception during data forwarding: {ex.Message}", ex));
                        }

                        // Sleep for a moment between config frame and data frame transmissions
                        Thread.Sleep(1);
                    }
                }
            }

            // Publish binary image for frame 
            image = e.Argument2;
            int offset = e.Argument3;
            int length = e.Argument4;

            // We republish exactly what we receive to the current destinations
            if (m_publishChannel != null)
            {
                try
                {
                    m_publishChannel.MulticastAsync(image, offset, length);
                }
                catch (Exception ex)
                {
                    OnProcessException(MessageLevel.Error, new InvalidOperationException($"Server based publication channel exception during data forwarding: {ex.Message}", ex));
                }
            }
            else if (m_clientBasedPublishChannel != null && m_clientBasedPublishChannel.CurrentState == ClientState.Connected)
            {
                try
                {
                    m_clientBasedPublishChannel.SendAsync(image, offset, length);
                }
                catch (Exception ex)
                {
                    OnProcessException(MessageLevel.Error, new InvalidOperationException($"TCP client based publication channel exception during data forwarding: {ex.Message}", ex));
                }
            }
        }

        private void m_frameParser_ReceivedDataFrame(object sender, EventArgs<IDataFrame> e)
        {
            // Do not process data frames when connection is setup to only forward data
            if (m_forwardOnly)
                return;

            ExtractFrameMeasurements(e.Argument);
            TotalDataFrames++;

            if (m_frameParser.RedundantFramesPerPacket > 0)
            {
                if (m_missingDataMonitor == null)
                {
                    m_missingDataMonitor = new MissingDataMonitor
                    {
                        LagTime = m_lagTime,
                        LeadTime = m_leadTime,
                        FramesPerSecond = m_frameParser.DefinedFrameRate,
                        TimeResolution = m_timeResolution,
                        UsePrecisionTimer = false
                    };
                }

                m_missingDataMonitor.RedundantFramesPerPacket = m_frameParser.RedundantFramesPerPacket;
                m_missingDataMonitor.SortMeasurements(e.Argument.Cells.Select(cell => cell.GetStatusFlagsMeasurement()));
            }
        }

        private void m_frameParser_ReceivedConfigurationFrame(object sender, EventArgs<IConfigurationFrame> e)
        {
            // Keep a reference to latest configuration frame that was received for fowarding
            m_configurationFrame = e.Argument;

            lock (m_configurationOperationLock)
            {
                bool configurationChanged = CheckForConfigurationChanges();

                if (!m_receivedConfigFrame || configurationChanged)
                {
                    OnStatusMessage(MessageLevel.Info, $"Received {(m_receivedConfigFrame ? "updated" : "initial")} configuration frame at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}");
                    m_receivedConfigFrame = true;

                    try
                    {
                        // Cache configuration on an independent thread in case this takes some time
                        ConfigurationFrame.Cache(e.Argument, ex => OnProcessException(MessageLevel.Info, ex), Name);
                    }
                    catch (Exception ex)
                    {
                        // Process exception for logging
                        OnProcessException(MessageLevel.Warning, new InvalidOperationException($"Failed to queue caching of config frame due to exception: {ex.Message}", ex));
                    }

                    StartMeasurementCounter();
                }
            }

            TotalConfigurationFrames++;
        }

        private void m_frameParser_ReceivedHeaderFrame(object sender, EventArgs<IHeaderFrame> e)
        {
            TotalHeaderFrames++;
        }

        private void m_frameParser_ReceivedFrameImage(object sender, EventArgs<FundamentalFrameType, int> e)
        {
            // We track bytes received so that connection can be restarted if data is not flowing
            m_bytesReceived += e.Argument2;
        }

        private void m_frameParser_ConnectionTerminated(object sender, EventArgs e)
        {
            OnDisconnected();

            if (!(m_frameParser?.Enabled ?? false))
                return;

            // Communications layer closed connection (close not initiated by system) - so we restart connection cycle...
            OnStatusMessage(MessageLevel.Info, "Connection closed by remote device, attempting reconnection...");
            Start();
        }

        private void m_frameParser_ConnectionEstablished(object sender, EventArgs e)
        {
            OnConnected();

            ResetStatistics();

            // Enable data stream monitor for connections that support commands
            m_dataStreamMonitor.Enabled = m_frameParser.DeviceSupportsCommands || AllowUseOfCachedConfiguration;

            // Reinitialize proxy connection if needed...
            if (Enabled && m_clientBasedPublishChannel != null && m_clientBasedPublishChannel.CurrentState != ClientState.Connected)
            {
                ThreadPool.QueueUserWorkItem(state =>
                {
                    m_clientBasedPublishChannel.Disconnect();
                    m_clientBasedPublishChannel.ConnectAsync();
                });
            }
        }

        private void m_frameParser_ConnectionException(object sender, EventArgs<Exception, int> e)
        {
            Exception ex = e.Argument1;

            if (EnableConnectionErrors)
                OnProcessException(MessageLevel.Info, new ConnectionException($"Connection attempt failed for {ConnectionInfo}: {ex.Message}", ex));

            // Check for multiple access IDs
            if (m_accessIDList.Length > 1 && m_frameParser.TransportProtocol == TransportProtocol.Tcp)
            {
                // Next server URI is selected by TCP client when a connection exception occurs
                string serverUri = m_frameParser.CommandChannelServerUri;

                if (!string.IsNullOrWhiteSpace(serverUri))
                {
                    m_serverIndex = 0;

                    // Lookup matching server URI
                    for (int i = 0; i < m_serverList.Length; i++)
                    {
                        if (m_serverList[i].Equals(serverUri, StringComparison.Ordinal))
                        {
                            m_serverIndex = i;
                            break;
                        }
                    }

                    // Set new access ID for frame parser
                    m_frameParser.DeviceID = AccessID;
                }
            }

            // So long as user hasn't requested to stop, keep trying connection
            if (Enabled)
                Start();
        }

        private void m_frameParser_ParsingException(object sender, EventArgs<Exception> e)
        {
            Exception ex = e.Argument;

            if (ex is ConnectionException && !EnableConnectionErrors)
                return;

            OnProcessException(MessageLevel.Info, ex, "Frame Parsing Exception");
        }

        private void m_frameParser_ExceededParsingExceptionThreshold(object sender, EventArgs e)
        {
            if (EnableConnectionErrors)
                OnStatusMessage(MessageLevel.Info, "\r\nConnection is being reset due to an excessive number of exceptions...\r\n");

            // So long as user hasn't already requested to stop, we restart connection
            if (Enabled)
                Start();
        }

        private void m_frameParser_ConnectionAttempt(object sender, EventArgs e)
        {
            OnStatusMessage(MessageLevel.Info, $"Initiating {m_frameParser.PhasorProtocol.GetFormattedProtocolName()} protocol connection...");
            ConnectionAttempts++;
        }

        private void m_frameParser_ConfigurationChanged(object sender, EventArgs e)
        {
            OnStatusMessage(MessageLevel.Info, "NOTICE: Configuration has changed, requesting new configuration frame...");
            m_receivedConfigFrame = false;
            SendCommand(DeviceCommand.SendConfigurationFrame2);
        }

        #endregion

        #region [ UDP Publish Channel Event Handlers ]

        private void udpPublishChannel_ClientConnectingException(object sender, EventArgs<Exception> e)
        {
            Exception ex = e.Argument;
            OnProcessException(MessageLevel.Error, new InvalidOperationException($"Exception occurred while client attempting to connect to UDP publication channel: {ex.Message}", ex));
        }

        private void udpPublishChannel_ReceiveClientDataComplete(object sender, EventArgs<Guid, byte[], int> e)
        {
            // Queue up device command handling on a different thread since this will often
            // engage sending data back on the same command channel and we want this async
            // thread to complete gracefully...
            if (m_publishChannel == null)
                ThreadPool.QueueUserWorkItem(DeviceCommandHandlerProc, e);
        }

        private void udpPublishChannel_SendClientDataException(object sender, EventArgs<Guid, Exception> e)
        {
            Exception ex = e.Argument2;

            if (ex is SocketException)
                OnProcessException(MessageLevel.Error, new InvalidOperationException($"Socket exception occurred on the UDP publication channel while attempting to send client data to \"{GetConnectionID(m_publishChannel, e.Argument1)}\": {ex.Message}", ex));
            else
                OnProcessException(MessageLevel.Error, new InvalidOperationException($"UDP publication channel exception occurred while sending client data to \"{GetConnectionID(m_publishChannel, e.Argument1)}\": {ex.Message}", ex));
        }

        private void udpPublishChannel_ServerStarted(object sender, EventArgs e)
        {
            OnStatusMessage(MessageLevel.Info, "UDP publication channel started.");
        }

        private void udpPublishChannel_ServerStopped(object sender, EventArgs e)
        {
            OnStatusMessage(MessageLevel.Info, "UDP publication channel stopped.");
        }

        #endregion

        #region [ TCP Publish Channel Event Handlers ]

        private void tcpPublishChannel_ClientConnected(object sender, EventArgs<Guid> e)
        {
            OnStatusMessage(MessageLevel.Info, "Client \"{0}\" connected to TCP publication channel.", GetConnectionID(m_publishChannel, e.Argument));
        }

        private void tcpPublishChannel_ClientDisconnected(object sender, EventArgs<Guid> e)
        {
            Guid clientID = e.Argument;

            OnStatusMessage(MessageLevel.Info, "Client \"{0}\" disconnected from TCP publication channel.", GetConnectionID(m_publishChannel, clientID));
            m_connectionIDCache.TryRemove(clientID, out _);
        }

        private void tcpPublishChannel_ClientConnectingException(object sender, EventArgs<Exception> e)
        {
            Exception ex = e.Argument;
            OnProcessException(MessageLevel.Error, new InvalidOperationException($"Socket exception occurred while client was attempting to connect to TCP publication channel: {ex.Message}", ex));
        }

        private void tcpPublishChannel_ReceiveClientDataComplete(object sender, EventArgs<Guid, byte[], int> e)
        {
            // Queue up derived class device command handling on a different thread
            ThreadPool.QueueUserWorkItem(DeviceCommandHandlerProc, e);
        }

        private void tcpPublishChannel_SendClientDataException(object sender, EventArgs<Guid, Exception> e)
        {
            Exception ex = e.Argument2;

            if (ex is SocketException)
                OnProcessException(MessageLevel.Error, new InvalidOperationException($"Socket exception occurred on the TCP publication channel while attempting to send client data to \"{GetConnectionID(m_publishChannel, e.Argument1)}\": {ex.Message}", ex));
            else
                OnProcessException(MessageLevel.Error, new InvalidOperationException($"TCP publication channel exception occurred while sending client data to \"{GetConnectionID(m_publishChannel, e.Argument1)}\": {ex.Message}", ex));
        }

        private void tcpPublishChannel_ServerStarted(object sender, EventArgs e)
        {
            OnStatusMessage(MessageLevel.Info, "TCP publication channel started.");
        }

        private void tcpPublishChannel_ServerStopped(object sender, EventArgs e)
        {
            OnStatusMessage(MessageLevel.Info, "TCP publication channel stopped.");
        }

        #endregion

        #region [ TCP Client Based Publish Channel Event Handlers ]

        private void tcpClientBasedPublishChannel_ConnectionEstablished(object sender, EventArgs e)
        {
            OnStatusMessage(MessageLevel.Info, "TCP publishing client connected to TCP listening server channel \"{0}\".", m_clientBasedPublishChannel.ServerUri);
        }

        private void tcpClientBasedPublishChannel_ConnectionTerminated(object sender, EventArgs e)
        {
            OnStatusMessage(MessageLevel.Info, "TCP publishing client disconnected from TCP listening server channel \"{0}\".", m_clientBasedPublishChannel.ServerUri);

            // Reinitialize client connection if it was just disconnected...
            if (Enabled && m_frameParser.IsConnected)
            {
                new Action(() =>
                {
                    if (m_clientBasedPublishChannel.CurrentState == ClientState.Disconnected)
                        m_clientBasedPublishChannel.ConnectAsync();
                })
                .DelayAndExecute(1000);
            }
        }

        private void tcpClientBasedPublishChannel_ConnectionException(object sender, EventArgs<Exception> e)
        {
            Exception ex = e.Argument;
            OnProcessException(MessageLevel.Error, new InvalidOperationException($"Socket exception occurred while TCP publishing client was attempting to connect to TCP listening server channel \"{m_clientBasedPublishChannel.ServerUri}\": {ex.Message}", ex));
        }

        private void tcpClientBasedPublishChannel_ReceiveDataComplete(object sender, EventArgs<byte[], int> e)
        {
            // Queue up derived class device command handling on a different thread
            ThreadPool.QueueUserWorkItem(DeviceCommandHandlerProc, new EventArgs<Guid, byte[], int>(Guid.Empty, e.Argument1, e.Argument2));
        }

        private void tcpClientBasedPublishChannel_SendDataException(object sender, EventArgs<Exception> e)
        {
            Exception ex = e.Argument;

            if (ex is SocketException)
                OnProcessException(MessageLevel.Error, new InvalidOperationException($"Socket exception occurred on the TCP publication client channel while attempting to send client data to TCP listening server \"{m_clientBasedPublishChannel.ServerUri}\": {ex.Message}", ex));
            else
                OnProcessException(MessageLevel.Error, new InvalidOperationException($"TCP publication client channel exception occurred while sending client data to TCP listening server \"{m_clientBasedPublishChannel.ServerUri}\": {ex.Message}", ex));
        }

        #endregion

        #endregion
    }
}
