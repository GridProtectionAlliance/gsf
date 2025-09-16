//******************************************************************************************************
//  Dnp3OutputAdapter.cs - Gbtc
//
//  Copyright © 2025, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  08/05/2025 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************
// ReSharper disable InconsistentNaming
// ReSharper disable RedundantSwitchExpressionArms
// ReSharper disable PreferConcreteValueOverDefault

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Automatak.DNP3.Adapter;
using Automatak.DNP3.Interface;
using GSF;
using GSF.Diagnostics;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using GSF.TimeSeries.Data;
using ConnectionStringParser = GSF.Configuration.ConnectionStringParser<GSF.TimeSeries.Adapters.ConnectionStringParameterAttribute>;

#nullable enable

namespace DNP3Adapters;

/// <summary>
/// DNP3 log levels.
/// </summary>
public enum LogLevel
{
    /// <summary>
    /// No logging.
    /// </summary>
    None,
    /// <summary>
    /// All log messages.
    /// </summary>
    All,
    /// <summary>
    /// Normal log messages.
    /// </summary>
    Normal,
    /// <summary>
    /// Application communications log messages.
    /// </summary>
    AppComms,
    /// <summary>
    /// All log messages including debug.
    /// </summary>
    Debug
}

/// <summary>
/// Output adapter that sends measurements to a remote DNP3 endpoint.
/// </summary>
[Description("DNP3: Acts as a DNP3 outstation server which sends measurements to a remote DNP3 endpoint.")]
public class Dnp3OutputAdapter : OutputAdapterBase, IDnp3Adapter
{
    #region [ Members ]

    // Nested Types
    private enum PointType
    {
        Binary,
        DoubleBitBinary,
        Analog,
        Counter
    }

    private readonly struct PointKey(PointType type, ushort index) : IEquatable<PointKey>
    {
        public readonly PointType Type = type;
        public readonly ushort Index = index;

        public override bool Equals(object? obj)
        {
            return obj is PointKey other && Equals(other);
        }

        public bool Equals(PointKey other)
        {
            return Type == other.Type && Index == other.Index;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + Type.GetHashCode();
                hash = hash * 31 + Index.GetHashCode();
                return hash;
            }
        }
    }

    private sealed class PointDefinition
    {
        public PointKey Key;
        public int Class = 1;
        public double Deadband;

        // Resolved variations (defaults set in BuildDatabase)
        // These define encodings for static and event data
        public StaticBinaryVariation StaticVariationBinary;
        public EventBinaryVariation EventVariationBinary;
        public StaticDoubleBinaryVariation StaticVariationDoubleBit;
        public EventDoubleBinaryVariation EventVariationDoubleBit;
        public StaticAnalogVariation StaticVariationAnalog;
        public EventAnalogVariation EventVariationAnalog;
        public StaticCounterVariation StaticVariationCounter;
        public EventCounterVariation EventVariationCounter;
    }

    // Thread-safe cached value for DNP3 point data
    private sealed class CachedValue(double value, DNPTime timestamp, Flags quality)
    {
        private readonly object m_lock = new();
        private double m_value = value;
        private DNPTime m_timestamp = timestamp;
        private Flags m_quality = quality;
        private double m_lastEventValue = value;
        private bool m_hasLastEventValue; // First update will generate event
        private long m_lastPublishTimestamp;

        public Ticks Timestamp
        {
            get
            {
                lock (m_lock)
                    return m_timestamp.Value.Ticks;
            }
        }

        public Ticks LastPublishTimestamp
        {
            get
            {
                lock (m_lock)
                    return m_lastPublishTimestamp;
            }
        }

        public void Update(double value, DNPTime timestamp, Flags quality)
        {
            lock (m_lock)
            {
                m_value = value;
                m_timestamp = timestamp;
                m_quality = quality;
            }
        }

        public (double Value, DNPTime Timestamp, Flags Quality) GetCurrentValue()
        {
            lock (m_lock)
                return (m_value, m_timestamp, m_quality);
        }

        public (double LastEventValue, bool HasLastEventValue) GetEventState()
        {
            lock (m_lock)
                return (m_lastEventValue, m_hasLastEventValue);
        }

        public void SetEvent(double eventValue, long timestamp)
        {
            lock (m_lock)
            {
                m_lastEventValue = eventValue;
                m_hasLastEventValue = true;
                m_lastPublishTimestamp = timestamp;
            }
        }
    }

    // Implementation currently rejects control ops
    private sealed class ReadOnlyCommandHandler : ICommandHandler
    {
        public void Begin() { }
        public void End() { }
        public CommandStatus Select(ControlRelayOutputBlock command, ushort index) => Reject;
        public CommandStatus Select(AnalogOutputInt32 command, ushort index) => Reject;
        public CommandStatus Select(AnalogOutputInt16 command, ushort index) => Reject;
        public CommandStatus Select(AnalogOutputFloat32 command, ushort index) => Reject;
        public CommandStatus Select(AnalogOutputDouble64 command, ushort index) => Reject;
        public CommandStatus Operate(ControlRelayOutputBlock command, ushort index, IDatabase database, OperateType opType) => Reject;
        public CommandStatus Operate(AnalogOutputInt32 command, ushort index, IDatabase database, OperateType opType) => Reject;
        public CommandStatus Operate(AnalogOutputInt16 command, ushort index, IDatabase database, OperateType opType) => Reject;
        public CommandStatus Operate(AnalogOutputFloat32 command, ushort index, IDatabase database, OperateType opType) => Reject;
        public CommandStatus Operate(AnalogOutputDouble64 command, ushort index, IDatabase database, OperateType opType) => Reject;
        private static CommandStatus Reject => CommandStatus.NOT_SUPPORTED;
    }

    private sealed class GSFOutstationApplication(Dnp3OutputAdapter adapter) : IOutstationApplication
    {
        void ILinkStatusListener.OnStateChange(LinkStatus value) => adapter.OnStatusMessage(MessageLevel.Info, $"Link state changed to: {value}");
        void ILinkStatusListener.OnUnknownDestinationAddress(ushort destination) => adapter.OnStatusMessage(MessageLevel.Warning, $"Unknown destination address: {destination}");
        void ILinkStatusListener.OnUnknownSourceAddress(ushort source) => adapter.OnStatusMessage(MessageLevel.Warning, $"Unknown source address: {source}");
        void ILinkStatusListener.OnKeepAliveInitiated() => adapter.OnStatusMessage(MessageLevel.Info, "Keep alive initiated");
        void ILinkStatusListener.OnKeepAliveFailure() => adapter.OnStatusMessage(MessageLevel.Warning, "Keep alive failure");
        void ILinkStatusListener.OnKeepAliveSuccess() => adapter.OnStatusMessage(MessageLevel.Info, "Keep alive success");

        bool IOutstationApplication.SupportsWriteAbsoluteTime => false;
        bool IOutstationApplication.WriteAbsoluteTime(ulong millisecSinceEpoch) => false;
        bool IOutstationApplication.SupportsWriteTimeAndInterval() => false;
        bool IOutstationApplication.WriteTimeAndInterval(IEnumerable<IndexedValue<TimeAndInterval>> values) => false;
        bool IOutstationApplication.SupportsAssignClass() => false;
        void IOutstationApplication.RecordClassAssignment(AssignClassType type, PointClass clazz, ushort start, ushort stop) { }
        ApplicationIIN IOutstationApplication.ApplicationIndications => new();
        RestartMode IOutstationApplication.ColdRestartSupport => RestartMode.UNSUPPORTED;
        RestartMode IOutstationApplication.WarmRestartSupport => RestartMode.UNSUPPORTED;
        ushort IOutstationApplication.ColdRestart() => ushort.MaxValue;
        ushort IOutstationApplication.WarmRestart() => ushort.MaxValue;
        void IOutstationApplication.OnConfirmProcessed(bool is_unsolicited, uint num_class1, uint num_class2, uint num_class3)
        {
            if (adapter.LogLevel < LogLevel.Debug)
                return;

            adapter.OnStatusMessage(MessageLevel.Debug, $"Confirm processed - Unsolicited: {is_unsolicited}, Class1: {num_class1}, Class2: {num_class2}, Class3: {num_class3}");
        }

        DNPTime IDnpTimeSource.Now() => DNPTime.Now;
    }

    // Constants
    private const string DefaultPort = "20000";
    private const string DefaultInterface = "0.0.0.0";
    private const bool DefaultAllowUnsolicited = true;
    private const bool DefaultIsMaster = false;
    private const string DefaultLocalAddress = "1024";
    private const string DefaultRemoteAddress = "1";
    private const int DefaultResponseTimeout = 1000;
    private const int DefaultKeepAliveTimeout = 60000;
    private const string DefaultUnsolicitedClassMask = "false,true,true,true";
    private const bool DefaultMapStateFlagsToQuality = true;
    private const int DefaultStaticUpdateBatchSize = 100;
    private const string DefaultLogLevel = "Normal";
    private const int DefaultMaxPublishRate = 2000;
    private const int DefaultClass = 1;
    private const double DefaultDeadband = 0.0D;
    private const int DoubleBitMask = 0x3;

    // Common DNP3 quality flags
    private const byte ONLINE = (byte)Bits.Bit00;
    private const byte RESTART = (byte)Bits.Bit01;
    private const byte COMM_LOST = (byte)Bits.Bit02;
    private const byte REMOTE_FORCED = (byte)Bits.Bit03;
    private const byte LOCAL_FORCED = (byte)Bits.Bit04;

    // Fields
    private IChannel? m_channel;
    private IOutstation? m_outstation;

    // Static cache for Class 0 reads
    private readonly ConcurrentDictionary<PointKey, CachedValue> m_staticCache;
    private readonly ConcurrentDictionary<MeasurementKey, PointDefinition> m_pointDefinitions;
    private DatabaseTemplate? m_databaseTemplate;
    private readonly bool[] m_unsolicitedClassMask;
    private volatile bool m_databaseInitialized;
    private readonly object m_outstationLoadLock;

    // Statistics tracking
    private long m_staticUpdatesCount;
    private long m_eventsGeneratedCount;
    private bool m_disposed;

    #endregion

    #region [ Constructors ]

    /// <summary>
    /// Creates a new <see cref="Dnp3OutputAdapter"/>.
    /// </summary>
    public Dnp3OutputAdapter()
    {
        m_pointDefinitions = new ConcurrentDictionary<MeasurementKey, PointDefinition>();
        m_staticCache = new ConcurrentDictionary<PointKey, CachedValue>();
        m_unsolicitedClassMask = new bool[4];
        m_outstationLoadLock = new object();
    }

    #endregion

    #region [ Properties ]

    /// <summary>
    /// Gets or sets the port the DNP3 adapter will listen for master connections on.
    /// </summary>
    [Description("Defines the port the DNP3 adapter will listen for master connections on")]
    [ConnectionStringParameter]
    [DefaultValue(typeof(ushort), DefaultPort)]
    public ushort Port { get; set; }

    /// <summary>
    /// Gets or sets the IP address of the interface the DNP3 adapter will bind to for listening for master connections.
    /// </summary>
    [Description("Defines the IP address of the interface the DNP3 adapter will bind to for listening for master connections")]
    [ConnectionStringParameter]
    [DefaultValue(DefaultInterface)]
    public string Interface { get; set; } = default!;

    /// <summary>
    /// Gets or sets a flag that determines if unsolicited responses are allowed.
    /// </summary>
    [Description("Defines whether unsolicited responses are allowed")]
    [ConnectionStringParameter]
    [DefaultValue(DefaultAllowUnsolicited)]
    public bool AllowUnsolicited { get; set; }

    /// <summary>
    /// Gets or sets flag that determines if the DNP3 output adapter is configured as 'master' in the link configuration.
    /// </summary>
    [Description("Determines if the adapter is configured as 'master' in the link configuration")]
    [ConnectionStringParameter]
    [DefaultValue(DefaultIsMaster)]
    public bool IsMaster { get; set; }

    /// <summary>
    /// Gets or sets the local address used for the link configuration.
    /// </summary>
    [Description("Defines the local address used for the link configuration")]
    [ConnectionStringParameter]
    [DefaultValue(typeof(ushort), DefaultLocalAddress)]
    public ushort LocalAddress { get; set; }

    /// <summary>
    /// Gets or sets the remote address used for the link configuration.
    /// </summary>
    [Description("Defines the remote address used for the link configuration")]
    [ConnectionStringParameter]
    [DefaultValue(typeof(ushort), DefaultRemoteAddress)]
    public ushort RemoteAddress { get; set; }

    /// <summary>
    /// Gets or sets the response timeout, in milliseconds, used for the link configuration.
    /// </summary>
    [Description("Defines the response timeout, in milliseconds, used for the link configuration")]
    [ConnectionStringParameter]
    [DefaultValue(DefaultResponseTimeout)]
    public int ResponseTimeout { get; set; }

    /// <summary>
    /// Gets or sets the keep-alive timeout, in milliseconds, used for the link configuration.
    /// </summary>
    [Description("Defines the keep-alive timeout, in milliseconds, used for the link configuration")]
    [ConnectionStringParameter]
    [DefaultValue(DefaultKeepAliveTimeout)]
    public int KeepAliveTimeout { get; set; }

    /// <summary>
    /// Gets or sets the unsolicited class mask. Format is four, comma separated booleans, e.g.: false, true, true, true.
    /// </summary>
    [Description("Defines the unsolicited class mask. Format is four comma separated booleans, e.g.: false, true, true, true")]
    [ConnectionStringParameter]
    [DefaultValue(DefaultUnsolicitedClassMask)]
    public string UnsolicitedClassMask { get; set; } = default!;

    /// <summary>
    /// Gets or sets flag that determines if GSF measurement state flags should be mapped to DNP3 quality flags.
    /// </summary>
    [Description("Define flag that determines if GSF measurement state flags should be mapped to DNP3 quality flags.")]
    [ConnectionStringParameter]
    [DefaultValue(DefaultMapStateFlagsToQuality)]
    public bool MapStateFlagsToQuality { get; set; }

    /// <summary>
    /// Gets or sets the batch size for static database updates.
    /// </summary>
    [Description("Defines the batch size for static database updates")]
    [ConnectionStringParameter]
    [DefaultValue(DefaultStaticUpdateBatchSize)]
    public int StaticUpdateBatchSize { get; set; }

    /// <summary>
    /// Gets or sets the maximum publish rate in milliseconds. This is used to throttle
    /// event publishing to the master stations when many events are being generated.
    /// Set to zero to publish events as fast as possible.
    /// </summary>
    [Description("Defines the maximum publish frequency in milliseconds (0 for no limit)")]
    [ConnectionStringParameter]
    [DefaultValue(DefaultMaxPublishRate)]
    public int MaxPublishRate { get; set; }

    /// <summary>
    /// Gets or sets the DNP3 log level for the server connection.
    /// </summary>
    [Description("Defines the DNP3 log level for the server connection")]
    [ConnectionStringParameter]
    [DefaultValue(typeof(LogLevel), DefaultLogLevel)]
    public LogLevel LogLevel { get; set; }

    /// <summary>
    /// Gets the count of static cache updates performed.
    /// </summary>
    public long StaticUpdatesCount => m_staticUpdatesCount;

    /// <summary>
    /// Gets the count of events generated.
    /// </summary>
    public long EventsGeneratedCount => m_eventsGeneratedCount;

    /// <summary>
    /// Gets the current size of the static cache.
    /// </summary>
    public int StaticCacheSize => m_staticCache.Count;

    /// <inheritdoc />
    public override bool OutputIsForArchive => false;

    /// <inheritdoc />
    protected override bool UseAsyncConnect => false;

    /// <inheritdoc />
    public override string Status
    {
        get
        {
            StringBuilder status = new();

            status.AppendLine("---- Configuration ----");
            status.AppendLine();

            status.AppendLine($"         TCP Server Config: {Port}/{Interface}");
            status.AppendLine($"                 Is Master: {IsMaster}");
            status.AppendLine($"             Local Address: {LocalAddress}");
            status.AppendLine($"            Remote Address: {RemoteAddress}");
            status.AppendLine($"          Response Timeout: {ResponseTimeout:N0}ms");
            status.AppendLine($"        Keep Alive Timeout: {KeepAliveTimeout:N0}ms");
            status.AppendLine($"         Allow Unsolicited: {AllowUnsolicited}");
            status.AppendLine($"    Unsolicited Class Mask: {UnsolicitedClassMask}");
            status.AppendLine($"Map State Flags to Quality: {MapStateFlagsToQuality}");
            status.AppendLine($"  Static Update Batch Size: {StaticUpdateBatchSize:N0}");
            status.AppendLine($"          Max Publish Rate: {(MaxPublishRate == 0 ? "No Limit" : $"{MaxPublishRate:N0}ms")}");
            status.AppendLine($"                 Log Level: {LogLevel}");

            status.AppendLine();
            status.AppendLine("---- Current State ----");
            status.AppendLine();

            IChannelStatistics? channelStats = m_channel?.GetChannelStatistics();

            if (channelStats is not null)
            {
                status.AppendLine($"           Channels Opened: {channelStats.NumOpen:N0}");
                status.AppendLine($"     Channel Open Failures: {channelStats.NumOpenFail:N0}");
                status.AppendLine($"           Channels Closed: {channelStats.NumClose:N0}");
                status.AppendLine($"            Bytes Received: {channelStats.NumBytesRx:N0}");
                status.AppendLine($"         Bytes Transmitted: {channelStats.NumBytesTx:N0}");
                status.AppendLine($"                CRC Errors: {channelStats.NumCrcError:N0}");
                status.AppendLine($"      Link Frames Received: {channelStats.NumLinkFrameRx:N0}");
                status.AppendLine($"   Link Frames Transmitted: {channelStats.NumLinkFrameTx:N0}");
                status.AppendLine($"  Bad Link Frames Received: {channelStats.NumBadLinkFrameRx:N0}");
            }

            IStackStatistics? stackStats = m_outstation?.GetStackStatistics();

            if (stackStats is not null)
            {
                status.AppendLine($"            Stack Received: {stackStats.NumTransportRx:N0}");
                status.AppendLine($"         Stack Transmitted: {stackStats.NumTransportTx:N0}");
                status.AppendLine($"      Stack Receive Errors: {stackStats.NumTransportErrorRx:N0}");
            }

            status.AppendLine($"            Static Updates: {StaticUpdatesCount:N0}");
            status.AppendLine($"          Events Generated: {EventsGeneratedCount:N0}");
            status.AppendLine($"         Static Cache Size: {StaticCacheSize:N0}");

            status.AppendLine();
            status.AppendLine("---- Adapter Status ----");
            status.AppendLine();

            status.Append(base.Status);

            return status.ToString();
        }
    }

    #endregion

    #region [ Methods ]

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="Dnp3OutputAdapter"/> object and optionally releases the managed resources.
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

            Shutdown();
            s_logHandler.UnregisterAdapter(this);
        }
        finally
        {
            m_disposed = true;  // Prevent duplicate dispose.
            base.Dispose(disposing);
        }
    }

    private void Shutdown()
    {
        m_outstation?.Shutdown();
        m_outstation = null;
        m_channel?.Shutdown();
        m_channel = null;

        m_staticCache.Clear();
        m_pointDefinitions.Clear();
    }

    /// <inheritdoc />
    public override void Initialize()
    {
        new ConnectionStringParser().ParseConnectionString(ConnectionString, this);

        base.Initialize();

        if (string.IsNullOrWhiteSpace(UnsolicitedClassMask))
            UnsolicitedClassMask = DefaultUnsolicitedClassMask;

        bool[] defaultMasks = DefaultUnsolicitedClassMask.Split(',').Select(bool.Parse).ToArray();
        string[] configuredMasks = UnsolicitedClassMask.Split(',');

        for (int i = 0; i < 4; i++)
        {
            if (i <= configuredMasks.Length - 1)
                m_unsolicitedClassMask[i] = configuredMasks[i].ParseBoolean();
            else
                m_unsolicitedClassMask[i] = defaultMasks[i];
        }

        if (m_unsolicitedClassMask[0])
            OnStatusMessage(MessageLevel.Warning, "Class 0 unsolicited responses enabled - this is not common and could cause network flooding");

        // Attach to all defined DNP3 measurements if none were explicitly defined
        if (InputMeasurementKeys is null || InputMeasurementKeys.Length == 0)
        {
            const string DNP3FilterExpression = "FILTER ActiveMeasurements WHERE AlternateTag LIKE '%DNP3{%'";
            InputMeasurementKeys = ParseInputMeasurementKeys(DataSource, false, DNP3FilterExpression);

            // Metadata refresh on output adapters forces a recalculation of input measurement keys using
            // InputMeasurementKeys settings key so that system can automatically update routing tables,
            // so assign InputMeasurementKeys settings value since it was not explicitly defined:
            Settings[nameof(InputMeasurementKeys)] = DNP3FilterExpression;
        }

        s_logHandler.RegisterAdapter(this);
    }

    /// <inheritdoc />
    protected override void AttemptConnection()
    {
        // TCP server channel (listener)
        m_channel = s_manager.AddTCPServer(
            $"gsf-dnp3-outstation:{Name}",
            getLogLevel(),
            ServerAcceptMode.CloseExisting,
            new IPEndpoint(Interface, Port),
            listener: null
        );

        // Build mapping and database template from InputMeasurementKeys, an adapter reconnect
        // operation will rebuild the database template in case point definitions have changed
        BuildDatabase();

        // Outstation config
        OutstationStackConfig outstationStackConfig = new()
        {
            outstation = new OutstationConfig
            {
                config = new OutstationParams
                {
                    allowUnsolicited = AllowUnsolicited,
                    unsolClassMask = new ClassField
                    {
                        Class0 = m_unsolicitedClassMask[0],
                        Class1 = m_unsolicitedClassMask[1],
                        Class2 = m_unsolicitedClassMask[2],
                        Class3 = m_unsolicitedClassMask[3]
                    },
                }
            },
            link = new LinkConfig(
                IsMaster, 
                LocalAddress, 
                RemoteAddress, 
                TimeSpan.FromMilliseconds(ResponseTimeout), 
                TimeSpan.FromMilliseconds(KeepAliveTimeout)
            ),
            databaseTemplate = m_databaseTemplate ?? new DatabaseTemplate()
        };

        // Create a GSF outstation application that includes adapter logging
        m_outstation = m_channel.AddOutstation(
            $"gsf-dnp3-outstation:{Name}",
            new ReadOnlyCommandHandler(),
            new GSFOutstationApplication(this),
            outstationStackConfig
        );

        // Begin accepting masters
        m_outstation.Enable();

        // Initialize static database with current or initial cached values
        ReinitializeDatabase();

        return;

        uint getLogLevel()
        {
            return LogLevel switch 
            {
                LogLevel.None => LogLevels.NONE,
                LogLevel.All => LogLevels.ALL,
                LogLevel.Normal => LogLevels.NORMAL,
                LogLevel.AppComms => LogLevels.APP_COMMS,
                LogLevel.Debug => LogLevels.ALL,
                _ => LogLevels.NORMAL
            };
        }
    }

    /// <inheritdoc />
    protected override void AttemptDisconnection()
    {
        Shutdown();
    }

    /// <summary>
    /// Forces re-initialization of the DNP3 database with current cached values.
    /// </summary>
    [AdapterCommand("Forces re-initialization of the DNP3 database with current cached values", "Administrator")]
    public void ReinitializeDatabase()
    {
        lock (m_outstationLoadLock)
        {
            m_databaseInitialized = false;
            InitializeStaticDatabase();
        }
    }

    /// <summary>
    /// Clears the static cache and resets statistics.
    /// </summary>
    [AdapterCommand("Clears the static cache and resets statistics", "Administrator", "Editor")]
    public void ClearStaticCache()
    {
        m_staticCache.Clear();

        Interlocked.Exchange(ref m_staticUpdatesCount, 0);
        Interlocked.Exchange(ref m_eventsGeneratedCount, 0);
    }

    /// <inheritdoc />
    public override string GetShortStatus(int maxLength)
    {
        return $"Published {ProcessedMeasurements:N0} measurements, events: {m_eventsGeneratedCount:N0}, cache size: {m_staticCache.Count:N0}".CenterText(maxLength);
    }

    /// <inheritdoc />
    protected override void ProcessMeasurements(IMeasurement[] measurements)
    {
        if (m_outstation is null)
            return;

        ChangeSet changeSet = new();
        int updateCount = 0;

        // Process measurements for cache updates and event generation
        foreach (IMeasurement measurement in measurements)
        {
            if (!m_pointDefinitions.TryGetValue(measurement.Key, out PointDefinition? definition))
                continue;

            // Update static cache and check for events
            bool eventGenerated = UpdateCacheAndCheckEvent(definition, measurement, ref changeSet);

            Interlocked.Increment(ref m_staticUpdatesCount);

            if (!eventGenerated)
                continue;
            
            Interlocked.Increment(ref m_eventsGeneratedCount);

            updateCount++;

            if (updateCount < StaticUpdateBatchSize || changeSet.IsEmpty())
                continue;

            // Publish next batch of events
            lock (m_outstationLoadLock)
                m_outstation.Load(changeSet);

            changeSet = new ChangeSet();
            updateCount = 0;
        }

        if (updateCount == 0 || changeSet.IsEmpty())
            return;
        
        // Publish remaining events
        lock (m_outstationLoadLock)
            m_outstation.Load(changeSet);
    }

    // Updates static cache and generates events if criteria are met
    private bool UpdateCacheAndCheckEvent(PointDefinition definition, IMeasurement measurement, ref ChangeSet changeSet)
    {
        PointKey key = definition.Key;
        DNPTime timestamp = new(new DateTime(measurement.Timestamp, DateTimeKind.Utc));

        return key.Type switch
        {
            PointType.Analog => ProcessAnalogMeasurement(measurement, key, timestamp, definition.Deadband, ref changeSet),
            PointType.Binary => ProcessBinaryMeasurement(measurement, key, timestamp, ref changeSet),
            PointType.DoubleBitBinary => ProcessDoubleBitMeasurement(measurement, key, timestamp, ref changeSet),
            PointType.Counter => ProcessCounterMeasurement(measurement, key, timestamp, (uint)definition.Deadband, ref changeSet),
            _ => throw new ArgumentOutOfRangeException($"Unsupported point type: {key.Type}")
        };
    }

    private bool ProcessAnalogMeasurement(IMeasurement measurement, PointKey key, DNPTime timestamp, double deadband, ref ChangeSet changeSet)
    {
        Flags qualityFlags = mapQualityFlags();
        double value = measurement.AdjustedValue;
        long previousTimestamp = 0L;

        CachedValue cachedValue = m_staticCache.AddOrUpdate(key,
            _ => new CachedValue(value, timestamp, qualityFlags),
            (_, existing) =>
            {
                previousTimestamp = existing.Timestamp.Value;
                existing.Update(value, timestamp, qualityFlags);
                return existing;
            }
        );

        if (!IsValidTimestamp(measurement.Timestamp.Value, previousTimestamp, cachedValue) || !shouldGenerateEvent(deadband, value, cachedValue))
            return false;
        
        changeSet.Update(new Analog(value, qualityFlags, timestamp), key.Index, EventMode.Force);
        cachedValue.SetEvent(value, measurement.Timestamp.Value);
        return true;

        Flags mapQualityFlags()
        {
            if (!MapStateFlagsToQuality)
                return new Flags((byte)AnalogQuality.ONLINE);

            byte flags = MapCommonStateFlags(measurement.StateFlags);
            MeasurementStateFlags stateFlags = measurement.StateFlags;

            if (stateFlags.HasFlag(MeasurementStateFlags.OverRangeError))
                flags |= (byte)AnalogQuality.OVERRANGE;

            if (stateFlags.HasFlag(MeasurementStateFlags.MeasurementError))
                flags |= (byte)AnalogQuality.REFERENCE_ERR;

            if (stateFlags.HasFlag(MeasurementStateFlags.UserDefinedFlag1))
                flags |= (byte)AnalogQuality.RESERVED;

            return new Flags(flags);
        }

        static bool shouldGenerateEvent(double deadband, double newValue, CachedValue cachedValue)
        {
            (double lastEventValue, bool hasLastEventValue) = cachedValue.GetEventState();

            if (!hasLastEventValue)
                return true; // First measurement always generates event

            double deltaValue = Math.Abs(newValue - lastEventValue);
            return deltaValue >= deadband;
        }
    }
    
    private bool ProcessBinaryMeasurement(IMeasurement measurement, PointKey key, DNPTime timestamp, ref ChangeSet changeSet)
    {
        bool boolValue = measurement.AdjustedValue != 0.0D;
        Flags qualityFlags = mapQualityFlags();
        double numericValue = measurement.AdjustedValue != 0.0D ? 1.0D : 0.0D;
        long previousTimestamp = 0L;

        CachedValue cachedValue = m_staticCache.AddOrUpdate(key,
            _ => new CachedValue(numericValue, timestamp, qualityFlags),
            (_, existing) =>
            {
                previousTimestamp = existing.Timestamp.Value;
                existing.Update(numericValue, timestamp, qualityFlags);
                return existing;
            }
        );

        if (!IsValidTimestamp(measurement.Timestamp.Value, previousTimestamp, cachedValue) || !shouldGenerateEvent(numericValue, cachedValue))
            return false;

        changeSet.Update(new Binary(boolValue, qualityFlags, timestamp), key.Index, EventMode.Force);
        cachedValue.SetEvent(numericValue, measurement.Timestamp.Value);
        return true;
        
        Flags mapQualityFlags()
        {
            if (!MapStateFlagsToQuality)
                return new Flags((byte)BinaryQuality.ONLINE);

            byte flags = MapCommonStateFlags(measurement.StateFlags);
            MeasurementStateFlags stateFlags = measurement.StateFlags;

            if (stateFlags.HasFlag(MeasurementStateFlags.SuspectData))
                flags |= (byte)BinaryQuality.CHATTER_FILTER;

            if (stateFlags.HasFlag(MeasurementStateFlags.UserDefinedFlag1))
                flags |= (byte)BinaryQuality.RESERVED;

            if (stateFlags.HasFlag(MeasurementStateFlags.AlarmHigh))
                flags |= (byte)BinaryQuality.STATE;

            return new Flags(flags);
        }
        
        static bool shouldGenerateEvent(double newValue, CachedValue cachedValue)
        {
            (double lastEventValue, bool hasLastEventValue) = cachedValue.GetEventState();

            if (!hasLastEventValue)
                return true;

            // Event on logical state change
            bool currentState = lastEventValue != 0.0D;
            bool newState = newValue != 0.0D;
            return currentState != newState;
        }
    }

    private bool ProcessDoubleBitMeasurement(IMeasurement measurement, PointKey key, DNPTime timestamp, ref ChangeSet changeSet)
    {
        DoubleBit state = (DoubleBit)((int)measurement.AdjustedValue & DoubleBitMask);
        Flags qualityFlags = mapQualityFlags();
        double numericValue = (int)state;
        long previousTimestamp = 0L;

        CachedValue cachedValue = m_staticCache.AddOrUpdate(key,
            _ => new CachedValue(numericValue, timestamp, qualityFlags),
            (_, existing) =>
            {
                previousTimestamp = existing.Timestamp.Value;
                existing.Update(numericValue, timestamp, qualityFlags);
                return existing;
            }
        );

        if (!IsValidTimestamp(measurement.Timestamp.Value, previousTimestamp, cachedValue) || !shouldGenerateEvent(numericValue, cachedValue))
            return false;
        
        changeSet.Update(new DoubleBitBinary(state, qualityFlags, timestamp), key.Index, EventMode.Force);
        cachedValue.SetEvent(numericValue, measurement.Timestamp.Value);
        return true;

        Flags mapQualityFlags()
        {
            if (!MapStateFlagsToQuality)
                return new Flags((byte)DoubleBitBinaryQuality.ONLINE);

            byte flags = MapCommonStateFlags(measurement.StateFlags);
            MeasurementStateFlags stateFlags = measurement.StateFlags;

            if (stateFlags.HasFlag(MeasurementStateFlags.SuspectData))
                flags |= (byte)DoubleBitBinaryQuality.CHATTER_FILTER;

            if (stateFlags.HasFlag(MeasurementStateFlags.AlarmHigh))
                flags |= (byte)DoubleBitBinaryQuality.STATE1;

            if (stateFlags.HasFlag(MeasurementStateFlags.AlarmLow))
                flags |= (byte)DoubleBitBinaryQuality.STATE2;

            return new Flags(flags);
        }

        static bool shouldGenerateEvent(double newValue, CachedValue cachedValue)
        {
            (double lastEventValue, bool hasLastEventValue) = cachedValue.GetEventState();

            if (!hasLastEventValue)
                return true;

            // Event on state change
            return (int)newValue != (int)lastEventValue;
        }
    }

    private bool ProcessCounterMeasurement(IMeasurement measurement, PointKey key, DNPTime timestamp, uint deadband, ref ChangeSet changeSet)
    {
        uint count = (uint)Math.Max(0L, (long)measurement.AdjustedValue);
        Flags qualityFlags = mapQualityFlags();
        double numericValue = count;
        long previousTimestamp = 0L;

        CachedValue cachedValue = m_staticCache.AddOrUpdate(key,
            _ => new CachedValue(numericValue, timestamp, qualityFlags),
            (_, existing) =>
            {
                previousTimestamp = existing.Timestamp.Value;
                existing.Update(numericValue, timestamp, qualityFlags);
                return existing;
            }
        );

        if (!IsValidTimestamp(measurement.Timestamp.Value, previousTimestamp, cachedValue) || !shouldGenerateEvent(deadband, numericValue, cachedValue))
            return false;
        
        changeSet.Update(new Counter(count, qualityFlags, timestamp), key.Index, EventMode.Force);
        cachedValue.SetEvent(numericValue, measurement.Timestamp.Value);
        return true;

        Flags mapQualityFlags()
        {
            if (!MapStateFlagsToQuality)
                return new Flags((byte)CounterQuality.ONLINE);

            byte flags = MapCommonStateFlags(measurement.StateFlags);
            MeasurementStateFlags stateFlags = measurement.StateFlags;

            if (stateFlags.HasFlag(MeasurementStateFlags.OverRangeError))
                flags |= (byte)CounterQuality.ROLLOVER;

            if (stateFlags.HasFlag(MeasurementStateFlags.DiscardedValue))
                flags |= (byte)CounterQuality.DISCONTINUITY;

            if (stateFlags.HasFlag(MeasurementStateFlags.UserDefinedFlag1))
                flags |= (byte)CounterQuality.RESERVED;

            return new Flags(flags);
        }

        static bool shouldGenerateEvent(uint deadband, double newValue, CachedValue cachedValue)
        {
            (double lastEventValue, bool hasLastEventValue) = cachedValue.GetEventState();

            if (!hasLastEventValue)
                return true;

            uint currentCount = (uint)Math.Max(0L, (long)lastEventValue);
            uint newCount = (uint)Math.Max(0L, (long)newValue);

            // Check for rollover (newValue < currentValue)
            if (newCount < currentCount)
                return true;

            // Check for increment >= deadband
            uint increment = newCount - currentCount;
            return increment >= deadband;
        }
    }

    private bool IsValidTimestamp(long currentTimestamp, long previousTimestamp, CachedValue cachedValue)
    {
        if (previousTimestamp == 0L)
            return true; // First measurement always valid

        return currentTimestamp > previousTimestamp && (currentTimestamp - cachedValue.LastPublishTimestamp).ToMilliseconds() >= MaxPublishRate;
    }

    private void InitializeStaticDatabase()
    {
        // Only populate with defaults on first initialization
        if (m_databaseInitialized)
            return;

        lock (m_outstationLoadLock)
        {
            // See if another thread already initialized the database
            if (m_databaseInitialized)
                return;

            if (m_outstation is null)
                return;

            ChangeSet changeSet = new();
            int updateCount = 0;

            DNPTime defaultTime = DNPTime.Now;

            foreach (PointDefinition definition in m_pointDefinitions.Values)
            {
                PointKey key = definition.Key;

                // Try to get cached value first, otherwise use defaults
                if (m_staticCache.TryGetValue(key, out CachedValue? cachedValue))
                {
                    // Use actual cached values
                    (double value, DNPTime timestamp, Flags quality) = cachedValue.GetCurrentValue();
                    addToChangeSet(changeSet, key, value, timestamp, quality);
                }
                else
                {
                    // Use default values for points that haven't received measurements yet
                    Flags defaultFlags = new(RESTART); // RESTART + not ONLINE

                    double defaultValue = key.Type switch
                    {
                        PointType.Binary => 0.0D,
                        PointType.DoubleBitBinary => (double)DoubleBit.INDETERMINATE,
                        PointType.Analog => 0.0D,
                        PointType.Counter => 0.0D,
                        _ => 0.0D
                    };

                    addToChangeSet(changeSet, key, defaultValue, defaultTime, defaultFlags);
                }

                updateCount++;

                if (updateCount < StaticUpdateBatchSize)
                    continue;

                m_outstation.Load(changeSet);
                changeSet = new ChangeSet();
                updateCount = 0;
            }

            // Apply any remaining updates
            if (!changeSet.IsEmpty())
                m_outstation.Load(changeSet);

            m_databaseInitialized = true;

            int cachedCount = m_staticCache.Count;
            int totalCount = m_pointDefinitions.Count;
            OnStatusMessage(MessageLevel.Info, $"Initialized DNP3 database with {cachedCount:N0} cached values and {totalCount - cachedCount:N0} default values for {totalCount:N0} total points");
        }

        return;

        static void addToChangeSet(ChangeSet changeSet, PointKey key, double value, DNPTime timestamp, Flags flags)
        {
            switch (key.Type)
            {
                case PointType.Analog:
                    changeSet.Update(new Analog(value, flags, timestamp), key.Index);
                    break;
                case PointType.Binary:
                    changeSet.Update(new Binary(value != 0.0D, flags, timestamp), key.Index);
                    break;
                case PointType.DoubleBitBinary:
                    changeSet.Update(new DoubleBitBinary((DoubleBit)((int)value & DoubleBitMask), flags, timestamp), key.Index);
                    break;
                case PointType.Counter:
                    changeSet.Update(new Counter((uint)value, flags, timestamp), key.Index);
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Unsupported point type: {key.Type}");
            }
        }
    }

    // Builds database when adapter connects or reconnects
    private void BuildDatabase()
    {
        DatabaseTemplate template = new();
        DataSet dataSource = DataSource;

        if (dataSource is null)
            throw new InvalidOperationException("DataSource is not available");

        // Track used indices to detect conflicts
        HashSet<PointKey> usedIndices = [];

        // Reset point definitions
        m_pointDefinitions.Clear();

        foreach (MeasurementKey key in InputMeasurementKeys)
        {
            DataRow record = DataSource.LookupMetadata(key.SignalID);
            string? alternateTag = null;

            if (record is not null)
                alternateTag = record["AlternateTag"].ToString();

            if (string.IsNullOrWhiteSpace(alternateTag))
                continue;

            // Parse DNP3 configuration from AlternateTag
            Match match = s_alternateTagRegex.Match(alternateTag);

            if (!match.Success)
                continue;

            Dictionary<string, string> settings = match.Groups["config"].Value.ParseKeyValuePairs();

            if (!settings.TryGetValue("Type", out string? setting) || !Enum.TryParse(setting, true, out PointType type))
            {
                OnStatusMessage(MessageLevel.Warning, $"Invalid or missing 'Type' in DNP3 configuration: {alternateTag}");
                continue;
            }

            if (!settings.TryGetValue("Index", out setting) || !ushort.TryParse(setting, out ushort index))
            {
                OnStatusMessage(MessageLevel.Warning, $"Invalid or missing 'Index' in DNP3 configuration: {alternateTag}");
                continue;
            }

            // Check for duplicate indices
            if (!usedIndices.Add(new PointKey(type, index)))
            {
                OnStatusMessage(MessageLevel.Warning, $"Duplicate DNP3 index {index} for {type} type in configuration: {alternateTag}");
                continue;
            }

            // Parse optional parameters with validation
            int classNum = DefaultClass;

            if (settings.TryGetValue("Class", out setting))
            {
                if (!int.TryParse(setting, out classNum) || classNum < 0 || classNum > 3)
                {
                    OnStatusMessage(MessageLevel.Warning, $"Invalid Class value {setting} in DNP3 configuration: {alternateTag}, using default {DefaultClass}");
                    classNum = DefaultClass;
                }
            }

            double deadband = DefaultDeadband;

            if (settings.TryGetValue("Deadband", out setting))
            {
                if (!double.TryParse(setting, out deadband) || deadband < 0.0D)
                {
                    OnStatusMessage(MessageLevel.Warning, $"Invalid Deadband value {setting} in DNP3 configuration: {alternateTag}, using default {DefaultDeadband:N2}");
                    deadband = DefaultDeadband;
                }
            }

            PointDefinition definition = new()
            {
                Key = new PointKey(type, index),
                Class = classNum,
                Deadband = deadband
            };

            // Parse variation overrides if provided
            string? eventVariation = settings.TryGetValue("EventVar", out string? variation) ? variation : null;
            string? staticVariation = settings.TryGetValue("StaticVar", out variation) ? variation : null;

            switch (type)
            {
                case PointType.Binary:
                    definition.EventVariationBinary = ParseEventBinaryVariation(eventVariation) ?? EventBinaryVariation.Group2Var2;
                    definition.StaticVariationBinary = ParseStaticBinaryVariation(staticVariation) ?? StaticBinaryVariation.Group1Var2;

                    template.binary.Add(index, new BinaryConfig
                    {
                        clazz = GetPointClass(definition.Class),
                        eventVariation = definition.EventVariationBinary,
                        staticVariation = definition.StaticVariationBinary
                    });
                    break;

                case PointType.DoubleBitBinary:
                    definition.EventVariationDoubleBit = ParseEventDoubleBinaryVariation(eventVariation) ?? EventDoubleBinaryVariation.Group4Var2;
                    definition.StaticVariationDoubleBit = ParseStaticDoubleBinaryVariation(staticVariation) ?? StaticDoubleBinaryVariation.Group3Var2;

                    template.doubleBinary.Add(index, new DoubleBinaryConfig
                    {
                        clazz = GetPointClass(definition.Class),
                        eventVariation = definition.EventVariationDoubleBit,
                        staticVariation = definition.StaticVariationDoubleBit
                    });
                    break;

                case PointType.Analog:
                    definition.EventVariationAnalog = ParseEventAnalogVariation(eventVariation) ?? EventAnalogVariation.Group32Var8;
                    definition.StaticVariationAnalog = ParseStaticAnalogVariation(staticVariation) ?? StaticAnalogVariation.Group30Var1;

                    template.analog.Add(index, new AnalogConfig
                    {
                        clazz = GetPointClass(definition.Class),
                        eventVariation = definition.EventVariationAnalog,
                        staticVariation = definition.StaticVariationAnalog,
                        deadband = deadband
                    });
                    break;

                case PointType.Counter:
                    definition.EventVariationCounter = ParseEventCounterVariation(eventVariation) ?? EventCounterVariation.Group22Var6;
                    definition.StaticVariationCounter = ParseStaticCounterVariation(staticVariation) ?? StaticCounterVariation.Group20Var1;

                    template.counter.Add(index, new CounterConfig
                    {
                        clazz = GetPointClass(definition.Class),
                        eventVariation = definition.EventVariationCounter,
                        staticVariation = definition.StaticVariationCounter,
                        deadband = (uint)deadband
                    });
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Unsupported PointType {type} in DNP3 configuration: {alternateTag}");
            }

            m_pointDefinitions[key] = definition;
        }

        m_databaseTemplate = template;
        OnStatusMessage(MessageLevel.Info, $"Built DNP3 database with {m_pointDefinitions.Count:N0} points");
    }

    void IDnp3Adapter.OnProcessException(MessageLevel level, Exception exception)
    {
        OnProcessException(level, exception);
    }

    void IDnp3Adapter.OnStatusMessage(MessageLevel level, string status)
    {
        OnStatusMessage(level, status);
    }

    #endregion

    #region [ Static ]

    // Static Fields

    // Proxy log handler with registered DNP3 output adapters instances
    private static readonly IaonProxyLogHandler<Dnp3OutputAdapter> s_logHandler;

    // DNP3 manager shared across all the DNP3 output adapters, concurrency level defaults to number of processors
    private static readonly IDNP3Manager s_manager;

    // Regex for parsing AlternateTag configuration
    // Format: DNP3{Type=Analog;Index=0;Class=1;Deadband=0.001;StaticVar=Group30Var1;EventVar=Group32Var7}
    private static readonly Regex s_alternateTagRegex = new(@"^DNP3\{(?<config>[^}]+)\}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // Static Constructor
    static Dnp3OutputAdapter()
    {
        s_logHandler = new IaonProxyLogHandler<Dnp3OutputAdapter>();
        s_manager = DNP3ManagerFactory.CreateManager(Environment.ProcessorCount, s_logHandler);
    }

    // Static Methods

    private static PointClass GetPointClass(int classNum)
    {
        return classNum switch
        {
            0 => PointClass.Class0,
            1 => PointClass.Class1,
            2 => PointClass.Class2,
            3 => PointClass.Class3,
            _ => PointClass.Class1
        };
    }

    // Common flag mapping shared by all types - mirrors MeasurementLookup.MapCommonStateFlags
    private static byte MapCommonStateFlags(MeasurementStateFlags stateFlags)
    {
        byte qualityFlags = 0;

        // ONLINE bit: 1 = ONLINE, 0 = NOT ONLINE
        if (!stateFlags.HasFlag(MeasurementStateFlags.BadData))
            qualityFlags |= ONLINE;

        if (stateFlags.HasFlag(MeasurementStateFlags.FlatlineAlarm))
            qualityFlags |= RESTART;

        if (stateFlags.HasFlag(MeasurementStateFlags.ReceivedAsBad))
            qualityFlags |= COMM_LOST;

        if (stateFlags.HasFlag(MeasurementStateFlags.WarningHigh))
            qualityFlags |= REMOTE_FORCED;

        if (stateFlags.HasFlag(MeasurementStateFlags.WarningLow))
            qualityFlags |= LOCAL_FORCED;

        return qualityFlags;
    }

    // Variation parsing helpers
    private static EventBinaryVariation? ParseEventBinaryVariation(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : Enum.TryParse(value, true, out EventBinaryVariation result) ? result : null;
    }

    private static StaticBinaryVariation? ParseStaticBinaryVariation(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : Enum.TryParse(value, true, out StaticBinaryVariation result) ? result : null;
    }

    private static EventDoubleBinaryVariation? ParseEventDoubleBinaryVariation(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : Enum.TryParse(value, true, out EventDoubleBinaryVariation result) ? result : null;
    }

    private static StaticDoubleBinaryVariation? ParseStaticDoubleBinaryVariation(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : Enum.TryParse(value, true, out StaticDoubleBinaryVariation result) ? result : null;
    }

    private static EventAnalogVariation? ParseEventAnalogVariation(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : Enum.TryParse(value, true, out EventAnalogVariation result) ? result : null;
    }

    private static StaticAnalogVariation? ParseStaticAnalogVariation(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : Enum.TryParse(value, true, out StaticAnalogVariation result) ? result : null;
    }

    private static EventCounterVariation? ParseEventCounterVariation(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : Enum.TryParse(value, true, out EventCounterVariation result) ? result : null;
    }

    private static StaticCounterVariation? ParseStaticCounterVariation(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : Enum.TryParse(value, true, out StaticCounterVariation result) ? result : null;
    }

    #endregion
}