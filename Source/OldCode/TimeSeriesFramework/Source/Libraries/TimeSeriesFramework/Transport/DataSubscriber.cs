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

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using TimeSeriesFramework.Adapters;
using TVA;
using TVA.Communication;
using TVA.Data;
using TVA.IO.Compression;
using TVA.Security.Cryptography;

namespace TimeSeriesFramework.Transport
{
    /// <summary>
    /// Represents a data subscribing client that will connect to a data publisher for a data subscription.
    /// </summary>
    public class DataSubscriber : InputAdapterBase
    {
        #region [ Members ]

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
        private long m_connectionAttempts;
        private volatile SignalIndexCache m_remoteSignalIndexCache;
        private volatile SignalIndexCache m_signalIndexCache;
        private volatile long[] m_baseTimeOffsets;
        private volatile byte[][][] m_keyIVs;
        private volatile int m_cipherIndex;
        private volatile bool m_authenticated;
        private volatile bool m_subscribed;
        private volatile int m_lastBytesReceived;
        private long m_totalBytesReceived;
        private Guid m_nodeID;
        private List<ServerCommand> m_requests;
        private bool m_synchronizedSubscription;
        private bool m_requireAuthentication;
        private bool m_autoConnect;
        private string m_sharedSecret;
        private string m_authenticationID;
        private bool m_includeTime;
        private bool m_synchronizeMetadata;
        private bool m_internal;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="DataSubscriber"/>.
        /// </summary>
        public DataSubscriber()
        {
            m_requests = new List<ServerCommand>();
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
                return Interlocked.Read(ref m_totalBytesReceived);
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

                status.AppendFormat("         Subscription mode: {0}", m_synchronizedSubscription ? "Synchronized" : "Unsynchronized");
                status.AppendLine();
                status.AppendFormat("  Pending command requests: {0}", m_requests.Count);
                status.AppendLine();
                status.AppendFormat("             Authenticated: {0}", m_authenticated);
                status.AppendLine();
                status.AppendFormat("                Subscribed: {0}", m_subscribed);
                status.AppendLine();

                if (m_dataChannel != null)
                {
                    status.AppendLine();
                    status.AppendLine("Data Channel Status".CenterText(50));
                    status.AppendLine("-------------------".CenterText(50));
                    status.Append(m_dataChannel.Status);
                }

                if (m_commandChannel != null)
                {
                    status.AppendLine();
                    status.AppendLine("Command Channel Status".CenterText(50));
                    status.AppendLine("----------------------".CenterText(50));
                    status.Append(m_commandChannel.Status);
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
                return false;
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
                    m_dataChannel.ReceiveData -= m_dataChannel_ReceiveData;
                    m_dataChannel.ReceiveDataException -= m_dataChannel_ReceiveDataException;
                    m_dataChannel.ReceiveDataTimeout -= m_dataChannel_ReceiveDataTimeout;

                    if (m_dataChannel != value)
                        m_dataChannel.Dispose();
                }

                // Assign new data channel reference
                m_dataChannel = value;

                if (m_dataChannel != null)
                {
                    // Attach to desired events on new data channel reference
                    m_dataChannel.ConnectionException += m_dataChannel_ConnectionException;
                    m_dataChannel.ReceiveData += m_dataChannel_ReceiveData;
                    m_dataChannel.ReceiveDataException += m_dataChannel_ReceiveDataException;
                    m_dataChannel.ReceiveDataTimeout += m_dataChannel_ReceiveDataTimeout;
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
                    m_commandChannel.HandshakeProcessTimeout -= m_commandChannel_HandshakeProcessTimeout;
                    m_commandChannel.HandshakeProcessUnsuccessful -= m_commandChannel_HandshakeProcessUnsuccessful;
                    m_commandChannel.ReceiveData -= m_commandChannel_ReceiveData;
                    m_commandChannel.ReceiveDataException -= m_commandChannel_ReceiveDataException;
                    m_commandChannel.ReceiveDataTimeout -= m_commandChannel_ReceiveDataTimeout;
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
                    m_commandChannel.HandshakeProcessTimeout += m_commandChannel_HandshakeProcessTimeout;
                    m_commandChannel.HandshakeProcessUnsuccessful += m_commandChannel_HandshakeProcessUnsuccessful;
                    m_commandChannel.ReceiveData += m_commandChannel_ReceiveData;
                    m_commandChannel.ReceiveDataException += m_commandChannel_ReceiveDataException;
                    m_commandChannel.ReceiveDataTimeout += m_commandChannel_ReceiveDataTimeout;
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
                        CommandChannel = null;
                        DataChannel = null;
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

            // Setup data publishing server with or without required authentication 
            if (settings.TryGetValue("requireAuthentication", out setting))
                m_requireAuthentication = setting.ParseBoolean();
            else
                m_requireAuthentication = false;

            if (m_requireAuthentication)
            {
                if (!settings.TryGetValue("sharedSecret", out m_sharedSecret) && !string.IsNullOrWhiteSpace(m_sharedSecret))
                    throw new ArgumentException("The \"sharedSecret\" setting must defined when authentication is required.");

                if (!settings.TryGetValue("authenticationID", out m_authenticationID) && !string.IsNullOrWhiteSpace(m_authenticationID))
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

            // Define auto connect setting
            if (settings.TryGetValue("autoConnect", out setting))
                m_autoConnect = setting.ParseBoolean();

            // Connect to local events when automatically engaging connection cycle
            if (m_autoConnect)
            {
                ConnectionAuthenticated += DataSubscriber_ConnectionAuthenticated;
                MetaDataReceived += DataSubscriber_MetaDataReceived;

                // If active measurements are defined, attempt to defined desired subscription points from there
                if (DataSource != null && DataSource.Tables != null && DataSource.Tables.Contains("ActiveMeasurements"))
                {
                    try
                    {
                        // Filter to points associated with this subscriber that have been requested for subscription, are enabled and not owned locally
                        DataRow[] filteredRows = DataSource.Tables["ActiveMeasurements"].Select("Internal = 0 AND Subscribed <> 0 AND DeviceID = " + ID.ToString());
                        List<IMeasurement> subscribedMeasurements = new List<IMeasurement>();

                        foreach (DataRow row in filteredRows)
                        {
                            // Create a new measurement for the provided field level information
                            Measurement measurement = new Measurement()
                            {
                                Key = MeasurementKey.Parse(row["ID"].ToNonNullString("_:0"), row["SignalID"].ToNonNullString(Guid.Empty.ToString()).ConvertToType<Guid>())
                            };

                            measurement.ID = row["SignalID"].ToNonNullString(Guid.Empty.ToString()).ConvertToType<Guid>();
                            measurement.TagName = row["PointTag"].ToNonNullString();

                            // Attempt to update empty signal ID if available
                            if (measurement.Key.SignalID == Guid.Empty)
                                measurement.Key.UpdateSignalID(measurement.ID);

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
                    catch
                    {
                        // Errors here are not catastrophic, this simply limits the auto-assignment of input measurement keys desired for subscription
                    }
                }
            }

            // Create a new TCP client
            TcpClient commandChannel = new TcpClient();

            // Initialize default settings
            commandChannel.ConnectionString = "server=localhost:6165";
            commandChannel.PayloadAware = true;
            commandChannel.Compression = CompressionStrength.NoCompression;
            commandChannel.PersistSettings = false;

            // Assign command channel client reference and attach to needed events
            this.CommandChannel = commandChannel;

            // Get proper connection string - either from specified command channel
            // or from base connection string
            if (settings.TryGetValue("commandChannel", out setting))
                commandChannel.ConnectionString = setting;
            else
                commandChannel.ConnectionString = ConnectionString;

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
            if (!string.IsNullOrWhiteSpace(authenticationID))
            {
                try
                {
                    MemoryStream buffer = new MemoryStream();
                    byte[] salt = new byte[DataPublisher.CipherSaltLength];
                    byte[] bytes;

                    // Generate some random prefix data to make sure auth key transmission is always unique
                    TVA.Security.Cryptography.Random.GetBytes(salt);

                    // Get encoded bytes of authentication key
                    bytes = salt.Combine(Encoding.Unicode.GetBytes(authenticationID));

                    // Encrypt authentication key
                    bytes = bytes.Encrypt(sharedSecret, CipherStrength.Aes256);

                    // Write encoded authentication key length into buffer
                    buffer.Write(EndianOrder.BigEndian.GetBytes(bytes.Length), 0, 4);

                    // Encode encrypted authentication key into buffer
                    buffer.Write(bytes, 0, bytes.Length);

                    // Send authentication command to server with associated command buffer
                    return SendServerCommand(ServerCommand.Authenticate, buffer.ToArray());
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
        /// Subscribes (or re-subscribes) to a data publisher for a synchronized set of data points.
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
        public virtual bool SynchronizedSubscribe(bool compactFormat, int framesPerSecond, double lagTime, double leadTime, string filterExpression, string dataChannel = null, bool useLocalClockAsRealTime = false, bool ignoreBadTimestamps = false, bool allowSortsByArrival = true, long timeResolution = Ticks.PerMillisecond, bool allowPreemptivePublishing = true, DownsamplingMethod downsamplingMethod = DownsamplingMethod.LastReceived, string startTime = null, string stopTime = null, string constraintParameters = null, int processingInterval = -1, string waitHandleNames = null, int waitHandleTimeout = 0)
        {
            StringBuilder connectionString = new StringBuilder();

            connectionString.Append("usePrecisionTimer=false; ");
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
            connectionString.AppendFormat("processingInterval={0}", processingInterval);

            if (!string.IsNullOrWhiteSpace(waitHandleNames))
            {
                connectionString.AppendFormat("; waitHandleNames={0}", waitHandleNames);
                connectionString.AppendFormat("; waitHandleTimeout={0}", waitHandleTimeout);
            }

            return Subscribe(true, compactFormat, connectionString.ToString());
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
        public virtual bool UnsynchronizedSubscribe(bool compactFormat, bool throttled, string filterExpression, string dataChannel = null, bool includeTime = true, double lagTime = 10.0D, double leadTime = 5.0D, bool useLocalClockAsRealTime = false, string startTime = null, string stopTime = null, string constraintParameters = null, int processingInterval = -1, string waitHandleNames = null, int waitHandleTimeout = 0)
        {
            StringBuilder connectionString = new StringBuilder();

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
            connectionString.AppendFormat("processingInterval={0}", processingInterval);

            if (!string.IsNullOrWhiteSpace(waitHandleNames))
            {
                connectionString.AppendFormat("; waitHandleNames={0}", waitHandleNames);
                connectionString.AppendFormat("; waitHandleTimeout={0}", waitHandleTimeout);
            }

            return Subscribe(false, compactFormat, connectionString.ToString());
        }

        /// <summary>
        /// Subscribes (or re-subscribes) to a data publisher for a set of data points.
        /// </summary>
        /// <param name="synchronized">Boolean value that determines if subscription should be synchronized.</param>
        /// <param name="compactFormat">Boolean value that determines if the compact measurement format should be used. Set to <c>false</c> for full fidelity measurement serialization; otherwise set to <c>true</c> for bandwidth conservation.</param>
        /// <param name="connectionString">Connection string that defines required and optional parameters for the subscription.</param>
        /// <returns><c>true</c> if subscribe transmission was successful; otherwise <c>false</c>.</returns>
        public virtual bool Subscribe(bool synchronized, bool compactFormat, string connectionString)
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

                        // Parse data channel sub-settings to check for compression setting
                        settings = setting.ParseKeyValuePairs();
                        bool compressionEnabled = false;

                        if (settings.TryGetValue("compression", out setting))
                            compressionEnabled = setting.ParseBoolean();

                        if (compressionEnabled)
                            dataChannel.Compression = CompressionStrength.Standard;

                        dataChannel.ReceiveBufferSize = ushort.MaxValue;
                        dataChannel.ConnectAsync();
                    }

                    // Assign data channel client reference and attach to needed events
                    this.DataChannel = dataChannel;

                    // Setup subcription packet
                    MemoryStream buffer = new MemoryStream();
                    DataPacketFlags flags = DataPacketFlags.NoFlags;
                    byte[] bytes;

                    if (synchronized)
                        flags |= DataPacketFlags.Synchronized;

                    if (compactFormat)
                        flags |= DataPacketFlags.Compact;

                    // Write data packet flags into buffer
                    buffer.WriteByte((byte)flags);

                    // Get encoded bytes of connection string
                    bytes = Encoding.Unicode.GetBytes(connectionString);

                    // Write encoded connection string length into buffer
                    buffer.Write(EndianOrder.BigEndian.GetBytes(bytes.Length), 0, 4);

                    // Encode connection string into buffer
                    buffer.Write(bytes, 0, bytes.Length);

                    // Cache subscribed synchronization state
                    m_synchronizedSubscription = synchronized;

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
        /// Sends a server command to the publisher connection.
        /// </summary>
        /// <param name="commandCode"><see cref="ServerCommand"/> to send.</param>
        /// <param name="data">Command data to send.</param>
        /// <returns><c>true</c> if <paramref name="commandCode"/> transmission was successful; otherwise <c>false</c>.</returns>
        public virtual bool SendServerCommand(ServerCommand commandCode, byte[] data = null)
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
            m_connectionAttempts = 0;
            m_commandChannel.ConnectAsync();
            m_authenticated = false;
            m_subscribed = false;
            m_keyIVs = null;
            Interlocked.Exchange(ref m_totalBytesReceived, 0L);
            m_lastBytesReceived = 0;
        }

        /// <summary>
        /// Attempts to disconnect from this <see cref="DataSubscriber"/>.
        /// </summary>
        protected override void AttemptDisconnection()
        {
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
            return Encoding.Unicode.GetString(buffer, startIndex, length);
        }

        private void ProcessServerResponse(byte[] buffer, int length)
        {
            if (buffer != null && length > 0)
            {
                try
                {
                    ServerResponse responseCode = (ServerResponse)buffer[0];
                    ServerCommand commandCode = (ServerCommand)buffer[1];
                    int responseLength = EndianOrder.BigEndian.ToInt32(buffer, 2);
                    int responseIndex = 6;
                    bool solicited = false;

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
                                        break;
                                    case ServerCommand.Unsubscribe:
                                        OnStatusMessage("Success code received in response to server command \"{0}\": {1}", commandCode, InterpretResponseMessage(buffer, responseIndex, responseLength));
                                        m_subscribed = false;
                                        break;
                                    case ServerCommand.RotateCipherKeys:
                                        OnStatusMessage("Success code received in response to server command \"{0}\": {1}", commandCode, InterpretResponseMessage(buffer, responseIndex, responseLength));
                                        break;
                                    case ServerCommand.MetaDataRefresh:
                                        OnStatusMessage("Success code received in response to server command \"{0}\": latest meta-data received.", commandCode);
                                        OnMetaDataReceived(Serialization.Deserialize<DataSet>(buffer.BlockCopy(responseIndex, responseLength), TVA.SerializationFormat.Binary));
                                        break;
                                }
                            }
                            else
                                OnProcessException(new InvalidOperationException("Publisher sent a success code for an unsolicited server command: " + commandCode));
                            break;
                        case ServerResponse.Failed:
                            if (solicited)
                                OnStatusMessage("Failure code received in response to server command \"{0}\": {1}", commandCode, InterpretResponseMessage(buffer, responseIndex, responseLength));
                            else
                                OnProcessException(new InvalidOperationException("Publisher sent a failed code for an unsolicited server command: " + commandCode));
                            break;
                        case ServerResponse.DataPacket:
                            // Deserialize data packet
                            List<IMeasurement> measurements = new List<IMeasurement>();
                            DataPacketFlags flags;
                            Ticks timestamp = 0;
                            int count;

                            // Track total data packet bytes received from any channel
                            Interlocked.Add(ref m_totalBytesReceived, m_lastBytesReceived);

                            // Decrypt data packet payload if keys are available
                            if (m_keyIVs != null)
                            {
                                buffer = buffer.BlockCopy(responseIndex, responseLength).Decrypt(m_keyIVs[m_cipherIndex][0], m_keyIVs[m_cipherIndex][1], CipherStrength.Aes256);
                                responseIndex = 0;
                                responseLength = buffer.Length;
                            }

                            // Get data packet flags
                            flags = (DataPacketFlags)buffer[responseIndex];
                            responseIndex++;

                            bool synchronizedMeasurements = ((byte)(flags & DataPacketFlags.Synchronized) > 0);
                            bool compactMeasurementFormat = ((byte)(flags & DataPacketFlags.Compact) > 0);

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
                                if (compactMeasurementFormat)
                                {
                                    // Deserialize compact measurement format
                                    CompactMeasurement measurement = new CompactMeasurement(m_signalIndexCache, m_includeTime, m_baseTimeOffsets);
                                    responseIndex += measurement.ParseBinaryImage(buffer, responseIndex, length - responseIndex);

                                    // Apply timestamp from frame if not included in transmission
                                    if (!measurement.IncludeTime)
                                        measurement.Timestamp = timestamp;

                                    measurements.Add(measurement);
                                }
                                else
                                {
                                    // Deserialize full measurement format
                                    SerializableMeasurement measurement = new SerializableMeasurement();
                                    responseIndex += measurement.ParseBinaryImage(buffer, responseIndex, length - responseIndex);
                                    measurements.Add(measurement);
                                }
                            }

                            // Expose new measurements to consumer
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
                            m_remoteSignalIndexCache = Serialization.Deserialize<SignalIndexCache>(buffer.BlockCopy(responseIndex, responseLength), TVA.SerializationFormat.Binary);
                            m_signalIndexCache = new SignalIndexCache(DataSource, m_remoteSignalIndexCache);
                            break;
                        case ServerResponse.UpdateBaseTimes:
                            // Deserialize new base time offsets
                            m_baseTimeOffsets = Serialization.Deserialize<long[]>(buffer.BlockCopy(responseIndex, responseLength), TVA.SerializationFormat.Binary);
                            break;
                        case ServerResponse.UpdateCipherKeys:
                            // Get active cipher index
                            m_cipherIndex = EndianOrder.BigEndian.ToInt32(buffer, responseIndex);

                            // Extract remaining response
                            byte[] bytes = buffer.BlockCopy(responseIndex + 4, responseLength - 4);

                            // Decrypt response payload if subscription is authenticated
                            if (m_authenticated)
                                bytes = bytes.Decrypt(m_sharedSecret, CipherStrength.Aes256);

                            // Deserialize new cipher keys
                            m_keyIVs = Serialization.Deserialize<byte[][][]>(bytes, TVA.SerializationFormat.Binary);
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
                UnsynchronizedSubscribe(true, false, filterExpression.ToString(), dataChannel);
            }
            else
            {
                OnStatusMessage("WARNING: No measurements are currently defined for subscription.");
            }

            // Initiate meta-data refresh
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
            try
            {
                DataSet metadata = state as DataSet;

                if (metadata != null)
                {
                    AdoDataConnection adoDatabase = new AdoDataConnection("systemSettings");
                    IDbConnection connection = adoDatabase.Connection;
                    int parentID = Convert.ToInt32(connection.ExecuteScalar(string.Format("SELECT SourceID FROM Runtime WHERE ID = {0} AND SourceTable='Device';", ID)));
                    string sourcePrefix = Name + "!";
                    Dictionary<string, int> deviceIDs = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
                    string query;

                    // Initialize active node ID
                    if (m_nodeID == Guid.Empty)
                        m_nodeID = Guid.Parse(connection.ExecuteScalar(string.Format("SELECT NodeID FROM IaonInputAdapter WHERE ID = {0};", ID)).ToString());

                    if (metadata.Tables.Contains("DeviceDetail"))
                    {
                        foreach (DataRow row in metadata.Tables["DeviceDetail"].Rows)
                        {
                            Guid uniqueID = adoDatabase.Guid(row, "UniqueID"); // row.Field<Guid>("UniqueID");
                            // We will synchronize metadata only if the source owns this device. Otherwise skip it.
                            if (row.Field<object>("OriginalSource") == null && !row["IsConcentrator"].ToNonNullString("0").ParseBoolean())
                            {
                                query = adoDatabase.ParameterizedQueryString("SELECT COUNT(*) FROM Device WHERE UniqueID = {0}", "deviceGuid");

                                if (Convert.ToInt32(connection.ExecuteScalar(query, uniqueID)) == 0)
                                {
                                    query = adoDatabase.ParameterizedQueryString("INSERT INTO Device(NodeID, ParentID, UniqueID, Acronym, " +
                                        "Name, IsConcentrator, Enabled, OriginalSource) VALUES ( {0}, {1}, {2}, {3}, {4}, 0, 1, {5})",
                                        "nodeID", "parentID", "uniqueID", "acronym", "name", "originalSource");

                                    connection.ExecuteNonQuery(query, m_nodeID, parentID, uniqueID, sourcePrefix + row.Field<string>("Acronym"), row.Field<string>("Name"),
                                        m_internal == true ? (object)DBNull.Value : string.IsNullOrEmpty(row.Field<string>("ParentAcronym")) ? sourcePrefix + row.Field<string>("Acronym") : sourcePrefix + row.Field<string>("ParentAcronym"));
                                }
                                else
                                {
                                    if (m_internal)
                                    {
                                        query = adoDatabase.ParameterizedQueryString("UPDATE Device SET Acronym = {0}, Name = {1}, OriginalSource = {2} WHERE UniqueID = {3}", "acronym", "name", "originalSource", "uniqueID");
                                        connection.ExecuteNonQuery(query, sourcePrefix + row.Field<string>("Acronym"), row.Field<string>("Name"), (object)DBNull.Value, uniqueID);
                                    }
                                    else
                                    {
                                        query = adoDatabase.ParameterizedQueryString("UPDATE Device SET Acronym = {0}, Name = {1} WHERE UniqueID = {2}", "acronym", "name", "uniqueID");
                                        connection.ExecuteNonQuery(query, sourcePrefix + row.Field<string>("Acronym"), row.Field<string>("Name"), uniqueID);
                                    }
                                }
                            }
                            // Capture new device ID for measurement association
                            query = adoDatabase.ParameterizedQueryString("SELECT ID FROM Device WHERE UniqueID = {0}", "deviceGuid");
                            deviceIDs[row.Field<string>("Acronym")] = Convert.ToInt32(connection.ExecuteScalar(query, uniqueID));
                        }
                    }

                    if (metadata.Tables.Contains("MeasurementDetail"))
                    {
                        // Load signal type ID's from local database associated with their acronym for proper signal type translation
                        Dictionary<string, int> signalTypeIDs = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
                        string signalTypeAcronym, deviceAcronym;

                        foreach (DataRow row in connection.RetrieveData(adoDatabase.AdapterType, "SELECT ID, Acronym FROM SignalType").Rows)
                        {
                            signalTypeAcronym = row.Field<string>("Acronym");

                            if (!string.IsNullOrWhiteSpace(signalTypeAcronym))
                                signalTypeIDs[signalTypeAcronym] = row.ConvertField<int>("ID");
                        }

                        foreach (DataRow row in metadata.Tables["MeasurementDetail"].Rows)
                        {
                            if (row["Internal"].ToNonNullString("1").ParseBoolean())
                            {
                                deviceAcronym = row.Field<string>("DeviceAcronym") ?? string.Empty;
                                signalTypeAcronym = row.Field<string>("SignalAcronym") ?? string.Empty;

                                if (!string.IsNullOrWhiteSpace(deviceAcronym) && deviceIDs.ContainsKey(deviceAcronym) && !string.IsNullOrWhiteSpace(signalTypeAcronym) && signalTypeIDs.ContainsKey(signalTypeAcronym))
                                {
                                    string pointTag = sourcePrefix + row.Field<string>("PointTag") ?? string.Empty;
                                    Guid signalID = adoDatabase.Guid(row, "SignalID");  // row.Field<Guid>("SignalID");

                                    query = adoDatabase.ParameterizedQueryString("SELECT COUNT(*) FROM Measurement WHERE SignalID = {0}", "signalID");

                                    if (Convert.ToInt32(connection.ExecuteScalar(query, signalID)) == 0)
                                    {
                                        string insert = adoDatabase.ParameterizedQueryString("INSERT INTO Measurement (DeviceID, PointTag, SignalTypeID, SignalReference, Description, Internal, Enabled ) VALUES ( {0}, {1}, {2}, {3}, {4}, {5} , 1 )", "deviceID", "pointTag", "signalTypeID", "signalReference", "description", "internal");
                                        string update = adoDatabase.ParameterizedQueryString("UPDATE Measurement SET SignalID = {0} WHERE PointTag = {1}", "signalID", "pointTag");

                                        connection.ExecuteNonQuery(insert, 30, deviceIDs[deviceAcronym], pointTag, signalTypeIDs[signalTypeAcronym], sourcePrefix + row.Field<string>("SignalReference"), row.Field<string>("Description") ?? string.Empty, adoDatabase.Bool(m_internal));
                                        connection.ExecuteNonQuery(update, signalID, pointTag);
                                    }
                                    else
                                    {
                                        query = adoDatabase.ParameterizedQueryString("UPDATE Measurement SET PointTag = {0}, SignalTypeID = {1}, SignalReference = {2}, Description = {3}, Internal = {4}, Subscribed = {5} WHERE SignalID = {6}", "pointTag", "signalTypeID", "signalReference", "description", "internal", "subscribed", "signalID");
                                        connection.ExecuteNonQuery(query, pointTag, signalTypeIDs[signalTypeAcronym], sourcePrefix + row.Field<string>("SignalReference"), row.Field<string>("Description") ?? string.Empty, adoDatabase.Bool(m_internal), adoDatabase.Bool(!m_internal), signalID);
                                    }
                                }
                            }
                        }
                    }

                    // New signals may have been defined, take original remote signal index cache and apply changes
                    if (m_remoteSignalIndexCache != null)
                        m_signalIndexCache = new SignalIndexCache(DataSource, m_remoteSignalIndexCache);

                }
            }
            catch (Exception ex)
            {
                OnProcessException(new InvalidOperationException("Failed to synchronize metadata to local cache: " + ex.Message, ex));
            }
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
            // We handle synchronization on a seperate thread since this process may be lengthy
            if (m_synchronizeMetadata)
                ThreadPool.QueueUserWorkItem(SynchronizeMetadata, e.Argument);
        }

        /// <summary>
        /// Raises the <see cref="ConnectionEstablished"/> event.
        /// </summary>
        protected void OnConnectionEstablished()
        {
            if (ConnectionEstablished != null)
                ConnectionEstablished(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="ConnectionTerminated"/> event.
        /// </summary>
        protected void OnConnectionTerminated()
        {
            if (ConnectionTerminated != null)
                ConnectionTerminated(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="ConnectionAuthenticated"/> event.
        /// </summary>
        protected void OnConnectionAuthenticated()
        {
            if (ConnectionAuthenticated != null)
                ConnectionAuthenticated(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="MetaDataReceived"/> event.
        /// </summary>
        /// <param name="metadata">Meta-data <see cref="DataSet"/> instance to send to client subscription.</param>
        protected void OnMetaDataReceived(DataSet metadata)
        {
            if (MetaDataReceived != null)
                MetaDataReceived(this, new EventArgs<DataSet>(metadata));
        }

        /// <summary>
        /// Raises the <see cref="DataStartTime"/> event.
        /// </summary>
        /// <param name="startTime">Start time, in <see cref="Ticks"/>, of first measurement transmitted.</param>
        protected void OnDataStartTime(Ticks startTime)
        {
            if (DataStartTime != null)
                DataStartTime(this, new EventArgs<Ticks>(startTime));
        }

        /// <summary>
        /// Raises the <see cref="ProcessingComplete"/> event.
        /// </summary>
        /// <param name="source">Type name of adapter that sent the processing completed notification.</param>
        protected void OnProcessingComplete(string source)
        {
            if (ProcessingComplete != null)
                ProcessingComplete(this, new EventArgs<string>(source));

            // Also raise base class event in case this event has been subscribed
            OnProcessingComplete();
        }

        #region [ Command Channel Event Handlers ]

        private void m_commandChannel_ConnectionEstablished(object sender, EventArgs e)
        {
            // Make sure no existing requests are queued for a new publisher connection
            lock (m_requests)
            {
                m_requests.Clear();
            }

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
            OnStatusMessage("Data subscriber connection to publisher was terminated.");
            DataChannel = null;

            if (m_autoConnect && Enabled)
                Start();
        }

        private void m_commandChannel_ConnectionException(object sender, EventArgs<Exception> e)
        {
            Exception ex = e.Argument;
            OnProcessException(new InvalidOperationException("Data subscriber encountered an exception while attempting publisher connection: " + ex.Message, ex));
        }

        private void m_commandChannel_ConnectionAttempt(object sender, EventArgs e)
        {
            // Inject a short delay between multiple connection attempts
            if (m_connectionAttempts > 0)
                Thread.Sleep(2000);

            OnStatusMessage("Data subscriber attempting connection to publisher...");
            m_connectionAttempts++;
        }

        private void m_commandChannel_SendDataException(object sender, EventArgs<Exception> e)
        {
            Exception ex = e.Argument;

            if (!(ex is ObjectDisposedException) && !(ex is System.Net.Sockets.SocketException && ((System.Net.Sockets.SocketException)ex).ErrorCode == 10054))
                OnProcessException(new InvalidOperationException("Data subscriber encountered an exception while sending data to publisher connection: " + ex.Message, ex));
        }

        private void m_commandChannel_ReceiveData(object sender, EventArgs<int> e)
        {
            try
            {
                byte[] buffer;
                int length = e.Argument;
                CipherStrength cryptoLevel = m_commandChannel.Encryption;
                CompressionStrength compressLevel = m_commandChannel.Compression;

                m_lastBytesReceived = length;

                if (cryptoLevel == CipherStrength.None && compressLevel == CompressionStrength.NoCompression)
                {
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
                else
                {
                    buffer = new byte[length];
                    int offset = 0;
                    m_commandChannel.Read(buffer, 0, length);
                    Payload.ProcessReceived(ref buffer, ref offset, ref length, cryptoLevel, m_commandChannel.SharedSecret, compressLevel);
                    ProcessServerResponse(buffer, length);
                }
            }
            catch (Exception ex)
            {
                OnProcessException(ex);
            }
        }

        private void m_commandChannel_ReceiveDataTimeout(object sender, EventArgs e)
        {
            OnProcessException(new InvalidOperationException("Data subscriber timed out while receiving data from publisher connection"));
        }

        private void m_commandChannel_ReceiveDataException(object sender, EventArgs<Exception> e)
        {
            Exception ex = e.Argument;

            if (!(ex is ObjectDisposedException) && !(ex is System.Net.Sockets.SocketException && ((System.Net.Sockets.SocketException)ex).ErrorCode == 10054))
                OnProcessException(new InvalidOperationException("Data subscriber encountered an exception while receiving data from publisher connection: " + ex.Message, ex));
        }

        private void m_commandChannel_HandshakeProcessUnsuccessful(object sender, EventArgs e)
        {
            OnProcessException(new InvalidOperationException("Data subscriber failed to be validated by publisher"));
        }

        private void m_commandChannel_HandshakeProcessTimeout(object sender, EventArgs e)
        {
            OnProcessException(new InvalidOperationException("Data subscriber timed out while waiting for publisher validation"));
        }

        #endregion

        #region [ Data Channel Event Handlers ]

        private void m_dataChannel_ConnectionException(object sender, EventArgs<Exception> e)
        {
            Exception ex = e.Argument;
            OnProcessException(new InvalidOperationException("Data subscriber encountered an exception while attempting to establish UDP data channel connection: " + ex.Message, ex));
        }

        private void m_dataChannel_ReceiveData(object sender, EventArgs<int> e)
        {
            try
            {
                byte[] buffer;
                int length = e.Argument;
                CipherStrength cryptoLevel = m_dataChannel.Encryption;
                CompressionStrength compressLevel = m_dataChannel.Compression;

                m_lastBytesReceived = length;

                if (cryptoLevel == CipherStrength.None && compressLevel == CompressionStrength.NoCompression)
                {
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
                else
                {
                    buffer = new byte[length];
                    int offset = 0;
                    m_dataChannel.Read(buffer, 0, length);
                    Payload.ProcessReceived(ref buffer, ref offset, ref length, cryptoLevel, m_dataChannel.SharedSecret, compressLevel);
                    ProcessServerResponse(buffer, length);
                }
            }
            catch (Exception ex)
            {
                OnProcessException(ex);
            }
        }

        private void m_dataChannel_ReceiveDataTimeout(object sender, EventArgs e)
        {
            OnProcessException(new InvalidOperationException("Data subscriber timed out while receiving UDP data from publisher connection"));
        }

        private void m_dataChannel_ReceiveDataException(object sender, EventArgs<Exception> e)
        {
            Exception ex = e.Argument;

            if (!(ex is ObjectDisposedException) && !(ex is System.Net.Sockets.SocketException && ((System.Net.Sockets.SocketException)ex).ErrorCode == 10054))
                OnProcessException(new InvalidOperationException("Data subscriber encountered an exception while receiving UDP data from publisher connection: " + ex.Message, ex));
        }

        #endregion

        #endregion
    }
}
