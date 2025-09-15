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
// ReSharper disable UnusedTupleComponentInReturnValue

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Automatak.DNP3.Adapter;
using Automatak.DNP3.Interface;
using GSF;
using GSF.Data;
using GSF.Diagnostics;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using GSF.TimeSeries.Data;
using ConnectionStringParser = GSF.Configuration.ConnectionStringParser<GSF.TimeSeries.Adapters.ConnectionStringParameterAttribute>;

#nullable enable

namespace DNP3Adapters;

/// <summary>
/// Output adapter that sends measurements to a remote DNP3 endpoint.
/// </summary>
[Description("DNP3: Acts as a DNP3 outstation server which sends measurements to a remote DNP3 endpoint.")]
public class Dnp3OutputAdapter : OutputAdapterBase, IDnp3Adapter
{
    #region [ Members ]

    // Nested Types
    private readonly struct PointKey(PointType type, int index) : IEquatable<PointKey>
    {
        public readonly PointType Type = type;
        public readonly int Index = index;

        public bool Equals(PointKey other) => Type == other.Type && Index == other.Index;

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

        public override bool Equals(object? obj) => obj is PointKey other && Equals(other);
    }

    // Simplified thread-safe cached value for DNP3 point data
    private sealed class CachedValue(double value, DNPTime timestamp, Flags quality)
    {
        private readonly object m_lock = new();
        private double m_value = value;
        private DNPTime m_timestamp = timestamp;
        private Flags m_quality = quality;
        private long m_updateCounter = 1;

        public void Update(double value, DNPTime timestamp, Flags quality)
        {
            lock (m_lock)
            {
                m_value = value;
                m_timestamp = timestamp;
                m_quality = quality;
                m_updateCounter++;
            }
        }

        public (double Value, DNPTime Timestamp, Flags Quality, long UpdateCounter) GetSnapshot()
        {
            lock (m_lock)
                return (m_value, m_timestamp, m_quality, m_updateCounter);
        }
    }

    // Custom command handler that populates database with static cached values
    private sealed class CacheBasedCommandHandler(Dnp3OutputAdapter adapter) : ICommandHandler
    {
        public void Begin()
        {
            adapter.PopulateStaticDatabase();
        }

        public void End() { }

        // All control operations rejected
        public CommandStatus Select(ControlRelayOutputBlock command, ushort index) => CommandStatus.NOT_SUPPORTED;
        public CommandStatus Select(AnalogOutputInt32 command, ushort index) => CommandStatus.NOT_SUPPORTED;
        public CommandStatus Select(AnalogOutputInt16 command, ushort index) => CommandStatus.NOT_SUPPORTED;
        public CommandStatus Select(AnalogOutputFloat32 command, ushort index) => CommandStatus.NOT_SUPPORTED;
        public CommandStatus Select(AnalogOutputDouble64 command, ushort index) => CommandStatus.NOT_SUPPORTED;
        public CommandStatus Operate(ControlRelayOutputBlock command, ushort index, IDatabase database, OperateType opType) => CommandStatus.NOT_SUPPORTED;
        public CommandStatus Operate(AnalogOutputInt32 command, ushort index, IDatabase database, OperateType opType) => CommandStatus.NOT_SUPPORTED;
        public CommandStatus Operate(AnalogOutputInt16 command, ushort index, IDatabase database, OperateType opType) => CommandStatus.NOT_SUPPORTED;
        public CommandStatus Operate(AnalogOutputFloat32 command, ushort index, IDatabase database, OperateType opType) => CommandStatus.NOT_SUPPORTED;
        public CommandStatus Operate(AnalogOutputDouble64 command, ushort index, IDatabase database, OperateType opType) => CommandStatus.NOT_SUPPORTED;
    }

    private enum PointType
    {
        Binary,
        DoubleBitBinary,
        Analog,
        Counter
    }

    private sealed class PointDef
    {
        private readonly object m_lock = new();
        private double m_lastEventValue;
        private bool m_hasLastEventValue;

        public PointType Type;
        public int Index;
        public int Class = 1;
        public double Deadband;

        public double LastEventValue
        {
            get
            {
                lock (m_lock)
                    return m_lastEventValue;
            }
        }

        public bool HasLastEventValue
        {
            get
            {
                lock (m_lock) 
                    return m_hasLastEventValue;
            }
        }

        public void SetLastEventValue(double value)
        {
            lock (m_lock)
            {
                m_lastEventValue = value;
                m_hasLastEventValue = true;
            }
        }

        public void ResetLastEventValue()
        {
            lock (m_lock)
            {
                m_lastEventValue = 0.0D;
                m_hasLastEventValue = false;
            }
        }

        // Resolved variations (defaults set in BuildDatabase)
        public StaticBinaryVariation StaticVariationBinary;
        public EventBinaryVariation EventVariationBinary;
        public StaticDoubleBinaryVariation StaticVariationDoubleBit;
        public EventDoubleBinaryVariation EventVariationDoubleBit;
        public StaticAnalogVariation StaticVariationAnalog;
        public EventAnalogVariation EventVariationAnalog;
        public StaticCounterVariation StaticVariationCounter;
        public EventCounterVariation EventVariationCounter;
    }

    // Constants
    private const ushort DefaultPort = 20000;
    private const string DefaultInterface = "0.0.0.0";
    private const bool DefaultAllowUnsolicited = true;
    private const bool DefaultIsMaster = false;
    private const string DefaultLocalAddress = "1024";
    private const string DefaultRemoteAddress = "1";
    private const int DefaultResponseTimeout = 1000;
    private const int DefaultKeepAliveTimeout = 60000;
    private const string DefaultUnsolicitedClassMask = "false,true,true,true";
    private const bool DefaultMapQualityToStateFlags = true;
    private const int DefaultStaticUpdateBatchSize = 100;

    // Regex for parsing AlternateTag configuration
    // Format: DNP3{Type=Analog;Index=0;Class=1;Deadband=0.001;StaticVar=Group30Var1;EventVar=Group32Var7}
    private static readonly Regex s_alternateTagRegex = new(@"^DNP3\{(?<config>[^}]+)\}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // Fields
    private IChannel? m_channel;
    private IOutstation? m_outstation;

    // Static cache for Class 0 reads
    private readonly ConcurrentDictionary<PointKey, CachedValue> m_staticCache;
    private readonly ConcurrentDictionary<MeasurementKey, PointDef> m_pointDefinitions;
    private DatabaseTemplate? m_databaseTemplate;
    private readonly bool[] m_unsolicitedClassMask;

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
        m_pointDefinitions = new ConcurrentDictionary<MeasurementKey, PointDef>();
        m_staticCache = new ConcurrentDictionary<PointKey, CachedValue>();
        m_unsolicitedClassMask = new bool[4];
    }

    #endregion

    #region [ Properties ]

    /// <summary>
    /// Gets or sets the port the DNP3 adapter will listen for master connections on.
    /// </summary>
    [Description("Defines the port the DNP3 adapter will listen for master connections on")]
    [ConnectionStringParameter]
    [DefaultValue(DefaultPort)]
    public ushort Port { get; set; }

    /// <summary>
    /// Gets or sets the IP address of the interface the DNP3 adapter will bind to for listening for master connections.
    /// </summary>
    [Description("Defines the IP address of the interface the DNP3 adapter will bind to for listening for master connections")]
    [ConnectionStringParameter]
    [DefaultValue(DefaultInterface)]
    public string Interface { get; set; } = string.Empty;

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
    public string UnsolicitedClassMask { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets flag that determines if GSF measurement state flags should be mapped to DNP3 quality flags.
    /// </summary>
    [Description("Define flag that determines if GSF measurement state flags should be mapped to DNP3 quality flags.")]
    [ConnectionStringParameter]
    [DefaultValue(DefaultMapQualityToStateFlags)]
    public bool MapQualityToStateFlags { get; set; }

    /// <summary>
    /// Gets or sets the batch size for static database updates.
    /// </summary>
    [Description("Defines the batch size for static database updates")]
    [ConnectionStringParameter]
    [DefaultValue(DefaultStaticUpdateBatchSize)]
    public int StaticUpdateBatchSize { get; set; }

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

        // Build mapping and database template from OutputMeasurements
        BuildDatabase();

        s_logHandler.RegisterAdapter(this);
    }

    /// <inheritdoc />
    protected override void AttemptConnection()
    {
        // TCP server channel (listener)
        m_channel = s_manager.AddTCPServer(
            $"gsf-dnp3-outstation:{Name}",
            filters: 0,
            ServerAcceptMode.CloseExisting,
            new IPEndpoint(Interface, Port),
            listener: null
        );

        // Outstation config
        OutstationStackConfig outstationStackConfig = new()
        {
            outstation = new OutstationConfig
            {
                config = new OutstationParams
                {
                    allowUnsolicited = AllowUnsolicited,
                    unsolClassMask = new ClassField((byte)(
                        (byte)(m_unsolicitedClassMask[0] ? PointClass.Class0 : 0) |
                        (byte)(m_unsolicitedClassMask[1] ? PointClass.Class1 : 0) |
                        (byte)(m_unsolicitedClassMask[2] ? PointClass.Class2 : 0) |
                        (byte)(m_unsolicitedClassMask[3] ? PointClass.Class3 : 0)
                    ))
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

        // Create outstation with cache-based command handler and default application
        m_outstation = m_channel.AddOutstation(
            $"gsf-dnp3-outstation:{Name}",
            new CacheBasedCommandHandler(this),
            DefaultOutstationApplication.Instance, // Using the default implementation
            outstationStackConfig
        );

        m_outstation.Enable(); // Begin accepting masters
    }

    /// <inheritdoc />
    protected override void AttemptDisconnection()
    {
        Shutdown();
    }

    /// <summary>
    /// Clears the static cache and resets statistics.
    /// </summary>
    [AdapterCommand("Clears the static cache and resets statistics", "Administrator", "Editor")]
    public void ClearStaticCache()
    {
        m_staticCache.Clear();

        // Reset event state for all point definitions
        foreach (PointDef pointDef in m_pointDefinitions.Values)
            pointDef.ResetLastEventValue();

        Interlocked.Exchange(ref m_staticUpdatesCount, 0);
        Interlocked.Exchange(ref m_eventsGeneratedCount, 0);
    }

    /// <inheritdoc />
    public override string GetShortStatus(int maxLength)
    {
        return $"Published {ProcessedMeasurements:N0} measurements, static updates: {m_staticUpdatesCount:N0}, events: {m_eventsGeneratedCount:N0}, cache size: {m_staticCache.Count:N0}".CenterText(maxLength);
    }

    /// <inheritdoc />
    protected override void ProcessMeasurements(IMeasurement[] measurements)
    {
        if (m_outstation is null)
            return;

        ChangeSet? eventChangeSet = null;
        int eventCount = 0;

        // Process measurements for cache updates and event generation
        foreach (IMeasurement measurement in measurements)
        {
            if (!m_pointDefinitions.TryGetValue(measurement.Key, out PointDef? pointDef))
                continue;

            // Update static cache and check for events
            bool eventGenerated = UpdateCacheAndCheckEvent(pointDef, measurement, ref eventChangeSet);

            Interlocked.Increment(ref m_staticUpdatesCount);

            if (!eventGenerated)
                continue;
            
            eventCount++;
            Interlocked.Increment(ref m_eventsGeneratedCount);
        }

        // Publish events if any were generated
        if (eventChangeSet == null || eventChangeSet.IsEmpty())
            return;
        
        try
        {
            m_outstation.Load(eventChangeSet);
            OnStatusMessage(MessageLevel.Debug, $"Published {eventCount:N0} DNP3 events");
        }
        catch (Exception ex)
        {
            OnProcessException(MessageLevel.Warning, new InvalidOperationException($"Failed to publish DNP3 events: {ex.Message}", ex));
        }
    }

    // Updates static cache and generates events if criteria are met
    private bool UpdateCacheAndCheckEvent(PointDef pointDef, IMeasurement measurement, ref ChangeSet? eventChangeSet)
    {
        DNPTime timestamp = new(new DateTime(measurement.Timestamp, DateTimeKind.Utc));
        PointKey key = new(pointDef.Type, pointDef.Index);
        bool eventGenerated = false;

        switch (pointDef.Type)
        {
            case PointType.Analog:
            {
                Flags qualityFlags = MapAnalogFlags(measurement);
                double value = measurement.AdjustedValue;

                // Check for event before updating cache
                bool shouldGenerateEvent = ShouldGenerateAnalogEvent(pointDef, value);

                // Update cache
                m_staticCache.AddOrUpdate(key,
                    new CachedValue(value, timestamp, qualityFlags),
                    (_, existing) =>
                    {
                        existing.Update(value, timestamp, qualityFlags);
                        return existing;
                    }
                );

                if (shouldGenerateEvent)
                {
                    eventChangeSet ??= new ChangeSet();
                    eventChangeSet.Update(new Analog(value, qualityFlags, timestamp), (ushort)pointDef.Index, EventMode.Force);
                    pointDef.SetLastEventValue(value);
                    eventGenerated = true;
                }
                break;
            }

            case PointType.Binary:
            {
                bool boolValue = measurement.AdjustedValue != 0.0D;
                Flags qualityFlags = MapBinaryFlags(measurement, boolValue);
                double numericValue = boolValue ? 1.0D : 0.0D;

                bool shouldGenerateEvent = ShouldGenerateBinaryEvent(pointDef, numericValue);

                m_staticCache.AddOrUpdate(key,
                    new CachedValue(numericValue, timestamp, qualityFlags),
                    (_, existing) =>
                    {
                        existing.Update(numericValue, timestamp, qualityFlags);
                        return existing;
                    }
                );

                if (shouldGenerateEvent)
                {
                    eventChangeSet ??= new ChangeSet();
                    eventChangeSet.Update(new Binary(boolValue, qualityFlags, timestamp), (ushort)pointDef.Index, EventMode.Force);
                    pointDef.SetLastEventValue(numericValue);
                    eventGenerated = true;
                }
                break;
            }

            case PointType.DoubleBitBinary:
            {
                DoubleBit state = (DoubleBit)((int)measurement.AdjustedValue & 0x3);
                Flags qualityFlags = MapDoubleBitBinaryFlags(measurement, state);
                double numericValue = (int)state;

                bool shouldGenerateEvent = ShouldGenerateDoubleBitEvent(pointDef, numericValue);

                m_staticCache.AddOrUpdate(key,
                    new CachedValue(numericValue, timestamp, qualityFlags),
                    (_, existing) =>
                    {
                        existing.Update(numericValue, timestamp, qualityFlags);
                        return existing;
                    }
                );

                if (shouldGenerateEvent)
                {
                    eventChangeSet ??= new ChangeSet();
                    eventChangeSet.Update(new DoubleBitBinary(state, qualityFlags, timestamp), (ushort)pointDef.Index, EventMode.Force);
                    pointDef.SetLastEventValue(numericValue);
                    eventGenerated = true;
                }
                break;
            }

            case PointType.Counter:
            {
                uint count = (uint)Math.Max(0L, (long)measurement.AdjustedValue);
                Flags qualityFlags = MapCounterFlags(measurement);
                double numericValue = count;

                bool shouldGenerateEvent = ShouldGenerateCounterEvent(pointDef, numericValue);

                m_staticCache.AddOrUpdate(key,
                    new CachedValue(numericValue, timestamp, qualityFlags),
                    (_, existing) =>
                    {
                        existing.Update(numericValue, timestamp, qualityFlags);
                        return existing;
                    }
                );

                if (shouldGenerateEvent)
                {
                    eventChangeSet ??= new ChangeSet();
                    eventChangeSet.Update(new Counter(count, qualityFlags, timestamp), (ushort)pointDef.Index, EventMode.Force);
                    pointDef.SetLastEventValue(numericValue);
                    eventGenerated = true;
                }
                break;
            }
            default:
                throw new ArgumentOutOfRangeException($"Unsupported point type: {pointDef.Type}");
        }

        return eventGenerated;
    }

    // Event generation logic simplified and moved to separate methods
    private bool ShouldGenerateAnalogEvent(PointDef pointDef, double newValue)
    {
        if (!pointDef.HasLastEventValue)
            return true; // First measurement always generates event

        double deltaValue = Math.Abs(newValue - pointDef.LastEventValue);
        return deltaValue >= pointDef.Deadband;
    }

    private bool ShouldGenerateBinaryEvent(PointDef pointDef, double newValue)
    {
        if (!pointDef.HasLastEventValue)
            return true;

        // Event on logical state change
        bool currentState = pointDef.LastEventValue != 0.0D;
        bool newState = newValue != 0.0D;
        return currentState != newState;
    }

    private bool ShouldGenerateDoubleBitEvent(PointDef pointDef, double newValue)
    {
        if (!pointDef.HasLastEventValue)
            return true;

        // Event on state change
        return Math.Abs(newValue - pointDef.LastEventValue) > 0.001D; // Small tolerance for double comparison
    }

    private bool ShouldGenerateCounterEvent(PointDef pointDef, double newValue)
    {
        if (!pointDef.HasLastEventValue)
            return true;

        uint currentCount = (uint)Math.Max(0L, (long)pointDef.LastEventValue);
        uint newCount = (uint)Math.Max(0L, (long)newValue);

        // Check for rollover (newValue < currentValue)
        if (newCount < currentCount)
            return true;

        // Check for increment >= deadband
        uint increment = newCount - currentCount;
        uint deadbandThreshold = (uint)Math.Max(0.0D, pointDef.Deadband);
        return increment >= deadbandThreshold;
    }

    /// <summary>
    /// Populates the DNP3 outstation database with current static cached values.
    /// </summary>
    internal void PopulateStaticDatabase()
    {
        if (m_outstation is null)
            return;

        ChangeSet changeSet = new();
        int updateCount = 0;

        // If cache has data, use it; otherwise populate with default values
        if (m_staticCache.Count > 0)
        {
            // Populate from cached values
            foreach (KeyValuePair<PointKey, CachedValue> kvp in m_staticCache)
            {
                PointKey pointKey = kvp.Key;
                CachedValue cachedValue = kvp.Value;
                (double value, DNPTime timestamp, Flags flags, _) = cachedValue.GetSnapshot();

                AddToChangeSet(changeSet, pointKey, value, timestamp, flags);
                updateCount++;

                // Apply in batches to avoid overwhelming the DNP3 stack
                if (updateCount < StaticUpdateBatchSize)
                    continue;
                
                m_outstation.Load(changeSet);
                changeSet.Clear();
                updateCount = 0;
            }
        }
        else
        {
            // Cache is empty - populate with default values and appropriate quality flags
            // This handles the case where PopulateStaticDatabase is called before any measurements arrive
            DNPTime defaultTime = DNPTime.Now;

            foreach (PointDef pointDef in m_pointDefinitions.Values)
            {
                PointKey pointKey = new(pointDef.Type, pointDef.Index);

                // Create flags indicating data is not yet available (RESTART + not ONLINE)
                Flags defaultFlags = new((byte)Bits.Bit01);

                // Use default values appropriate for each type
                double defaultValue = pointDef.Type switch
                {
                    PointType.Binary => 0.0D,
                    PointType.DoubleBitBinary => (double)DoubleBit.INDETERMINATE,
                    PointType.Analog => 0.0D,
                    PointType.Counter => 0.0D,
                    _ => 0.0D
                };

                AddToChangeSet(changeSet, pointKey, defaultValue, defaultTime, defaultFlags);
                updateCount++;

                if (updateCount < StaticUpdateBatchSize)
                    continue;
                
                m_outstation.Load(changeSet);
                changeSet.Clear();
                updateCount = 0;
            }
        }

        // Apply any remaining updates
        if (!changeSet.IsEmpty())
            m_outstation.Load(changeSet);
    }

    /// <summary>
    /// Helper method to add values to ChangeSet based on point type
    /// </summary>
    private static void AddToChangeSet(ChangeSet changeSet, PointKey pointKey, double value, DNPTime timestamp, Flags flags)
    {
        switch (pointKey.Type)
        {
            case PointType.Analog:
                changeSet.Update(new Analog(value, flags, timestamp), (ushort)pointKey.Index);
                break;
            case PointType.Binary:
                changeSet.Update(new Binary(value != 0.0D, flags, timestamp), (ushort)pointKey.Index);
                break;
            case PointType.DoubleBitBinary:
                changeSet.Update(new DoubleBitBinary((DoubleBit)((int)value & 0x3), flags, timestamp), (ushort)pointKey.Index);
                break;
            case PointType.Counter:
                changeSet.Update(new Counter((uint)value, flags, timestamp), (ushort)pointKey.Index);
                break;
            default:
                throw new ArgumentOutOfRangeException($"Unsupported point type: {pointKey.Type}");
        }
    }

    private void BuildDatabase()
    {
        DatabaseTemplate template = new();
        DataSet dataSource = DataSource;

        if (dataSource == null)
            throw new InvalidOperationException("DataSource is not available");

        ActiveMeasurementsTableLookup activeMeasurements = DataSourceLookups.ActiveMeasurements(dataSource);
        IEnumerable<DataRow> deviceMeasurements = activeMeasurements.LookupByDeviceNameNoStat(Name);

        // Track used indices to detect conflicts
        HashSet<(PointType Type, int Index)> usedIndices = [];

        foreach (DataRow row in deviceMeasurements)
        {
            string? alternateTag = row.AsString("AlternateTag");

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

            if (!settings.TryGetValue("Index", out setting) || !int.TryParse(setting, out int index) || index < 0 || index > ushort.MaxValue)
            {
                OnStatusMessage(MessageLevel.Warning, $"Invalid or missing 'Index' in DNP3 configuration: {alternateTag}");
                continue;
            }

            // Check for duplicate indices
            (PointType type, int index) indexKey = (type, index);

            if (!usedIndices.Add(indexKey))
            {
                OnStatusMessage(MessageLevel.Warning, $"Duplicate DNP3 index {index} for type {type} in configuration: {alternateTag}");
                continue;
            }

            // Get measurement key
            Guid signalID = row.AsGuid("SignalID") ?? Guid.Empty;

            if (signalID == Guid.Empty)
                continue;

            MeasurementKey key = MeasurementKey.LookUpBySignalID(signalID);

            if (key == MeasurementKey.Undefined)
                continue;

            // Parse optional parameters with validation
            int classNum = 1;

            if (settings.TryGetValue("Class", out setting))
            {
                if (!int.TryParse(setting, out classNum) || classNum < 0 || classNum > 3)
                {
                    OnStatusMessage(MessageLevel.Warning, $"Invalid Class value {setting} in DNP3 configuration: {alternateTag}, using default 1");
                    classNum = 1;
                }
            }

            double deadband = 0.0D;

            if (settings.TryGetValue("Deadband", out setting))
            {
                if (!double.TryParse(setting, out deadband) || deadband < 0.0D)
                {
                    OnStatusMessage(MessageLevel.Warning, $"Invalid Deadband value {setting} in DNP3 configuration: {alternateTag}, using default 0.0");
                    deadband = 0.0D;
                }
            }

            PointDef definition = new()
            {
                Type = type,
                Index = index,
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

                    template.binary.Add((ushort)index, new BinaryConfig
                    {
                        clazz = GetPointClass(definition.Class),
                        eventVariation = definition.EventVariationBinary,
                        staticVariation = definition.StaticVariationBinary
                    });
                    break;

                case PointType.DoubleBitBinary:
                    definition.EventVariationDoubleBit = ParseEventDoubleBinaryVariation(eventVariation) ?? EventDoubleBinaryVariation.Group4Var2;
                    definition.StaticVariationDoubleBit = ParseStaticDoubleBinaryVariation(staticVariation) ?? StaticDoubleBinaryVariation.Group3Var2;

                    template.doubleBinary.Add((ushort)index, new DoubleBinaryConfig
                    {
                        clazz = GetPointClass(definition.Class),
                        eventVariation = definition.EventVariationDoubleBit,
                        staticVariation = definition.StaticVariationDoubleBit
                    });
                    break;

                case PointType.Analog:
                    definition.EventVariationAnalog = ParseEventAnalogVariation(eventVariation) ?? EventAnalogVariation.Group32Var8;
                    definition.StaticVariationAnalog = ParseStaticAnalogVariation(staticVariation) ?? StaticAnalogVariation.Group30Var1;

                    template.analog.Add((ushort)index, new AnalogConfig
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

                    template.counter.Add((ushort)index, new CounterConfig
                    {
                        clazz = GetPointClass(definition.Class),
                        eventVariation = definition.EventVariationCounter,
                        staticVariation = definition.StaticVariationCounter,
                        deadband = (uint)Math.Max(0, deadband)
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

    // Type-specific flag mapping methods that mirror the input adapter's MeasurementLookup implementation
    private Flags MapBinaryFlags(IMeasurement measurement, bool value)
    {
        if (!MapQualityToStateFlags)
            return new Flags();

        byte qualityFlags = MapCommonStateFlags(measurement.StateFlags);
        MeasurementStateFlags stateFlags = measurement.StateFlags;

        if (stateFlags.HasFlag(MeasurementStateFlags.SuspectData))
            qualityFlags |= (byte)BinaryQuality.CHATTER_FILTER;

        if (stateFlags.HasFlag(MeasurementStateFlags.UserDefinedFlag1))
            qualityFlags |= (byte)BinaryQuality.RESERVED;

        if (stateFlags.HasFlag(MeasurementStateFlags.AlarmHigh))
            qualityFlags |= (byte)BinaryQuality.STATE;

        // If STATE bit is not set, set it based on the actual value
        if ((qualityFlags & (byte)BinaryQuality.STATE) == 0 && value)
            qualityFlags |= (byte)BinaryQuality.STATE;

        return new Flags(qualityFlags);
    }

    private Flags MapDoubleBitBinaryFlags(IMeasurement measurement, DoubleBit value)
    {
        if (!MapQualityToStateFlags)
            return new Flags();

        byte qualityFlags = MapCommonStateFlags(measurement.StateFlags);
        MeasurementStateFlags stateFlags = measurement.StateFlags;

        if (stateFlags.HasFlag(MeasurementStateFlags.SuspectData))
            qualityFlags |= (byte)DoubleBitBinaryQuality.CHATTER_FILTER;

        if (stateFlags.HasFlag(MeasurementStateFlags.AlarmHigh))
            qualityFlags |= (byte)DoubleBitBinaryQuality.STATE1;

        if (stateFlags.HasFlag(MeasurementStateFlags.AlarmLow))
            qualityFlags |= (byte)DoubleBitBinaryQuality.STATE2;

        // Set state bits based on value if not already set by flags
        if ((qualityFlags & ((byte)DoubleBitBinaryQuality.STATE1 | (byte)DoubleBitBinaryQuality.STATE2)) == 0)
        {
            qualityFlags |= (byte)((int)value << 6); // STATE1 and STATE2 are bits 6-7
        }

        return new Flags(qualityFlags);
    }

    private Flags MapAnalogFlags(IMeasurement measurement)
    {
        if (!MapQualityToStateFlags)
            return new Flags();

        byte qualityFlags = MapCommonStateFlags(measurement.StateFlags);
        MeasurementStateFlags stateFlags = measurement.StateFlags;

        if (stateFlags.HasFlag(MeasurementStateFlags.OverRangeError))
            qualityFlags |= (byte)AnalogQuality.OVERRANGE;

        if (stateFlags.HasFlag(MeasurementStateFlags.MeasurementError))
            qualityFlags |= (byte)AnalogQuality.REFERENCE_ERR;

        if (stateFlags.HasFlag(MeasurementStateFlags.UserDefinedFlag1))
            qualityFlags |= (byte)AnalogQuality.RESERVED;

        return new Flags(qualityFlags);
    }

    private Flags MapCounterFlags(IMeasurement measurement)
    {
        if (!MapQualityToStateFlags)
            return new Flags();

        byte qualityFlags = MapCommonStateFlags(measurement.StateFlags);
        MeasurementStateFlags stateFlags = measurement.StateFlags;

        if (stateFlags.HasFlag(MeasurementStateFlags.OverRangeError))
            qualityFlags |= (byte)CounterQuality.ROLLOVER;

        if (stateFlags.HasFlag(MeasurementStateFlags.DiscardedValue))
            qualityFlags |= (byte)CounterQuality.DISCONTINUITY;

        if (stateFlags.HasFlag(MeasurementStateFlags.UserDefinedFlag1))
            qualityFlags |= (byte)CounterQuality.RESERVED;

        return new Flags(qualityFlags);
    }

    // Common flag mapping shared by all types - mirrors MeasurementLookup.MapCommonStateFlags
    private static byte MapCommonStateFlags(MeasurementStateFlags stateFlags)
    {
        const byte ONLINE = (byte)Bits.Bit00;
        const byte RESTART = (byte)Bits.Bit01;
        const byte COMM_LOST = (byte)Bits.Bit02;
        const byte REMOTE_FORCED = (byte)Bits.Bit03;
        const byte LOCAL_FORCED = (byte)Bits.Bit04;

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

    // Static Constructor
    static Dnp3OutputAdapter()
    {
        s_logHandler = new IaonProxyLogHandler<Dnp3OutputAdapter>();
        s_manager = DNP3ManagerFactory.CreateManager(Environment.ProcessorCount, s_logHandler);
    }

    #endregion
}