//******************************************************************************************************
//  PIOutputAdapter.cs - Gbtc
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
//       Optimized PI-SDK code performance with connection pooling
//  12/17/2014 - J. Ritchie Carroll
//       Updated code to use AF-SDK - using single PIConnection for now
//  03/16/2021 - J. Ritchie Carroll
//       Updated to add automated tag-removal operations during metadata synchronization.
//  05/31/2024 - J. Ritchie Carroll
//       Updated to add digital state set support and improved configuration options.
//
//******************************************************************************************************
// ReSharper disable InconsistentlySynchronizedField

using GSF;
using GSF.Collections;
using GSF.Configuration;
using GSF.Data;
using GSF.Diagnostics;
using GSF.IO;
using GSF.PhasorProtocols;
using GSF.PhasorProtocols.Anonymous;
using GSF.PhasorProtocols.IEEEC37_118;
using GSF.Threading;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using GSF.TimeSeries.Data;
using GSF.TimeSeries.Statistics;
using GSF.Units.EE;
using OSIsoft.AF;
using OSIsoft.AF.Asset;
using OSIsoft.AF.Data;
using OSIsoft.AF.PI;
using OSIsoft.AF.Time;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using MeasurementRecord = GSF.TimeSeries.Model.Measurement;
using TagGenerator = GSF.Parsing.TemplatedExpressionParser;

namespace PIAdapters;

#region [ Enumerations ]

/// <summary>
/// Enumeration that defines the available operations for PI tag removal during metadata synchronization.
/// </summary>
public enum TagRemovalOperation
{
    /// <summary>
    /// Do not remove any existing PI tags. This is the default operation.
    /// </summary>
    [Description("Do not remove any existing PI tags. This is the default operation.")]
    DoNotRemove,
    /// <summary>
    /// Remove any PI tags where the PI point source matches the setting defined in the PI output adapter and
    /// the PI tag no longer exists in the local metadata. This is the recommended option when it is desirable
    /// that any locally defined tags that get removed also get removed from PI.
    /// </summary>
    /// <remarks>
    /// This is the least aggressive tag removal synchronization operation. Only PI tags that have a matching
    /// point source name will be considered for removal. After point source match, then any PI tags where
    /// existing measurement signal ID is not found in any extended descriptors will be removed. Note that
    /// removal of any point tags in PI metadata will cause history for removed tags to be inaccessible.
    /// </remarks>
    [Description(
        "Remove any PI tags where the PI point source matches the setting defined in the PI output adapter and the PI tag no longer exists in the local metadata. " +
        "This is the recommended option when it is desirable that any locally defined tags that get removed also get removed from PI.\r\n" +
        "This is the least aggressive tag removal synchronization operation. Only PI tags that have a matching point source name will be considered for removal. " +
        "After point source match, then any PI tags where existing measurement signal ID is not found in any extended descriptors will be removed. Note that removal " +
        "of any point tags in PI metadata will cause history for removed tags to be inaccessible.")]
    LocalOnly,
    /// <summary>
    /// Remove any PI tags where existing measurement signal ID is not found in any extended descriptors.
    /// Use with caution as metadata in PI will be updated to exactly match local metadata.
    /// </summary>
    /// <remarks>
    /// WARNING: Do not use this option if target PI instance is used for storage of any data other than this
    /// local PI adapter. This is the most aggressive tag removal synchronization operation. Any PI tags that
    /// do not exist in local metadata will be removed from PI. This option ensures that PI metadata will
    /// exactly match local metadata. Note that removal of any point tags in PI metadata will cause history
    /// for removed tags to be inaccessible.
    /// </remarks>
    [Description(
        "Remove any PI tags where existing measurement signal ID is not found in any extended descriptors. Use with caution as metadata in PI will be updated to exactly " +
        "match local metadata.\r\nWARNING: Do not use this option if target PI instance is used for storage of any data other than this local PI adapter. This is " +
        "the most aggressive tag removal synchronization operation. Any PI tags that do not exist in local metadata will be removed from PI. This option ensures that " +
        "PI metadata will exactly match local metadata. Note that removal of any point tags in PI metadata will cause history for removed tags to be inaccessible.")]
    FullClone
}

/// <summary>
/// Defines the available digital state sets for IEEE C37.118 connection states, quality and status word bits.
/// </summary>
/// <remarks>
/// These digital state sets are used to expand the status word bits into individual digital tags as well
/// as other pertinent information related to an IEEE C37.118 connection. These digital state sets are
/// based on, and are compatible with, the digital state sets defined for the IEEE C37.118 Interface to
/// the PI System.
/// </remarks>
public readonly struct C37118DigitalStateSets
{
    /// <summary>
    /// Represents the composite quality status digital state.
    /// </summary>
    /// <remarks>
    /// Digital tag receives a composite status from the Data Valid, PMU Error, Sync Error and Data Sorting flags.
    /// If any of these flags are set, the value of this tag will be set to 1. Otherwise, it will be zero.
    /// </remarks>
    public const int CompositeQual = 0;

    /// <summary>
    /// Represents the configuration change status bit digital state.
    /// </summary>
    /// <remarks>
    /// Digital tag that is set to 1 when the configuration change flag (bit 10) in the STAT word is true.
    /// </remarks>
    public const int ConfigChange = 1;

    /// <summary>
    /// Represents the connect state status bit digital state.
    /// </summary>
    /// <remarks>
    /// Digital tag receives the current connection state.
    /// </remarks>
    public const int ConnectState = 2;

    /// <summary>
    /// Represents the data sorting status bit digital state.
    /// </summary>
    /// <remarks>
    /// Digital tag receives the Data Sorting bit (12) from the STAT word.
    /// </remarks>
    public const int DataSorting = 3;

    /// <summary>
    /// Represents the data valid status bit digital state.
    /// </summary>
    /// <remarks>
    /// Digital tag receives the Data Validity bit (15) from the STAT word. 
    /// </remarks>
    public const int DataValid = 4;

    /// <summary>
    /// Represents the leap second status bits digital state.
    /// </summary>
    /// <remarks>
    /// Digital tag receives the Leap Second quality bits (6-4) from the time quality flags. 
    /// </remarks>
    public const int LeapSecond = 5;

    /// <summary>
    /// Represents the nominal frequency status bit digital state.
    /// </summary>
    /// <remarks>
    /// Nominal line frequency (FNOM) from last CONFIG block.
    /// </remarks>
    public const int NominalFreq = 6;

    /// <summary>
    /// Represents the PMU error status bit digital state.
    /// </summary>
    /// <remarks>
    /// Digital tag receives the PMU Error bit (14) from the STAT word.
    /// </remarks>
    public const int PMUError = 7;

    /// <summary>
    /// Represents the time sync error status bit digital state.
    /// </summary>
    /// <remarks>
    /// Digital tag receives the PMU Sync Error bit (13) from the STAT word. 
    /// </remarks>
    public const int SyncError = 8;

    /// <summary>
    /// Represents the timelock status bits digital state.
    /// </summary>
    /// <remarks>
    /// Digital tag receives the Unlocked Time quality bits (5-4) from the STAT word.
    /// </remarks>
    public const int Timelock = 9;

    /// <summary>
    /// Represents the time quality status bits digital state.
    /// </summary>
    /// <remarks>
    /// Digital tag receives the Time Quality bits (3-0) from the time quality flags.
    /// </remarks>
    public const int TimeQuality = 10;

    /// <summary>
    /// Represents the trigger status bits digital state.
    /// </summary>
    /// <remarks>
    /// Digital tag receives the Trigger Reason bits (3-0) from the STAT word
    /// </remarks>
    public const int Trigger = 11;

    internal const int Count = Trigger + 1;

    internal static HashSet<int> StatusBitStates => [CompositeQual, ConfigChange, DataSorting, DataValid, NominalFreq, PMUError, SyncError, Timelock, Trigger];
    
    internal static HashSet<int> QualityBitStates => [ConnectState, LeapSecond, TimeQuality];
}

#endregion

/// <summary>
/// Exports measurements to PI if the point tag or alternate tag corresponds to a PI point's tag name.
/// </summary>
[Description("OSI-PI: Archives measurements to an OSI-PI server using AF-SDK")]
public class PIOutputAdapter : OutputAdapterBase
{
    #region [ Members ]

    // Constants
    private const string DefaultUserName = "";
    private const string DefaultPassword = "";
    private const bool DefaultRunMetadataSync = true;
    private const bool DefaultAutoCreateTags = true;
    private const bool DefaultAutoUpdateTags = true;
    private const string DefaultAutoRemoveTags = nameof(TagRemovalOperation.DoNotRemove);
    private const bool DefaultSyncAlternateTagOnly = false;
    private const bool DefaultSkipDigitalAlternateTagSync = true;
    private const bool DefaultSkipAnalogAlternateTagSync = true;
    private const int DefaultTagNamePrefixRemoveCount = 0;
    private const string DefaultPIPointSource = nameof(GSF);
    private const string DefaultPIPointClass = "classic";
    private const bool DefaultUseCompression = true;
    private const bool DefaultReplaceValues = false;
    private const string DefaultBadDataValueStatus = nameof(AFValueStatus.Good);
    private const string DefaultBadTimeValueStatus = nameof(AFValueStatus.Good);
    private const bool DefaultUpdateExistingDescriptorState = true;
    private const bool DefaultAddTagCompressionState = false;
    private const bool DefaultUpdateExistingTagCompressionState = false;
    private const string DefaultCompDev = "0.001";
    private const string DefaultCompDevDataTypeMap = 
        "IPHM=0.1," +
        "IPHA=0.001," +
        "VPHM=10," +
        "VPHA=0.001," +
        "FREQ=0.0001," +
        "DFDT=0.0001," +
        "ALOG=0.001," +
        "DIGI=0,"+
        "FLAG=0," +
        "ALRM=0," +
        "QUAL=0," +
        $"*={DefaultCompDev}";
    private const string DefaultArchiveFilterDataTypes = "*";
    private const string DefaultArchiveOnChangeDataTypes = "";
    private const string DefaultTagMapCacheFileName = "";
    private const double DefaultMaximumPointResolution = 0.0D;
    private const bool DefaultEnableTimeReasonabilityCheck = false;
    private const double DefaultPastTimeReasonabilityLimit = 43200.0D;
    private const double DefaultFutureTimeReasonabilityLimit = 43200.0D;
    private const bool DefaultExpandStatusBitsToTags = false;
    private const bool DefaultWriteStatusWord = true;
    private const bool DefaultExpandQualityBitsToTags = false;
    private const bool DefaultWriteQualityWord = true;
    private const string DefaultIEEEC37118DigitalStates = 
        $"C37118_{nameof(C37118DigitalStateSets.CompositeQual)}," + 
        $"C37118_{nameof(C37118DigitalStateSets.ConfigChange)}," +
        $"C37118_{nameof(C37118DigitalStateSets.ConnectState)}," +
        $"C37118_{nameof(C37118DigitalStateSets.DataSorting)}," + 
        $"C37118_{nameof(C37118DigitalStateSets.DataValid)}," +
        $"C37118_{nameof(C37118DigitalStateSets.LeapSecond)}," +
        $"C37118_{nameof(C37118DigitalStateSets.NominalFreq)}," +
        $"C37118_{nameof(C37118DigitalStateSets.PMUError)}," + 
        $"C37118_{nameof(C37118DigitalStateSets.SyncError)}," + 
        $"C37118_{nameof(C37118DigitalStateSets.Timelock)}," + 
        $"C37118_{nameof(C37118DigitalStateSets.TimeQuality)}" +
        $"C37118_{nameof(C37118DigitalStateSets.Trigger)}";
    private const string DefaultIEEEC371118TagNameExpressions = 
        "{CompanyAcronym}_{DeviceAcronym}.COMPOSITE_QUAL;" +
        "{CompanyAcronym}_{DeviceAcronym}.CONFIG_CHANGE;" +
        "{CompanyAcronym}_{DeviceAcronym}.CONNECT_STATE;" +
        "{CompanyAcronym}_{DeviceAcronym}.DATA_SORTING;" +
        "{CompanyAcronym}_{DeviceAcronym}.DATA_VALID;" +
        "{CompanyAcronym}_{DeviceAcronym}.LEAP_SECOND;" +
        "{CompanyAcronym}_{DeviceAcronym}.NOMINAL_FREQ;" +
        "{CompanyAcronym}_{DeviceAcronym}.PMU_ERROR;" +
        "{CompanyAcronym}_{DeviceAcronym}.SYNC_ERROR;" +
        "{CompanyAcronym}_{DeviceAcronym}.TIME_LOCK;" +
        "{CompanyAcronym}_{DeviceAcronym}.TIME_QUALITY" +
        "{CompanyAcronym}_{DeviceAcronym}.TRIGGER";
    private const bool DefaultExpandDigitalBitsToTags = false;
    private const bool DefaultWriteDigitalWord = true;
    private const string DefaultDigitalBitStateExpressionMap = "";
    private const string DefaultDigitalBitTagNameExpressionMap = "";
    private const string DefaultDigitalBitExcludedExpressions = 
        "SPARE_BIT;" +
        "RESERVED_BIT";

    // Fields
    private readonly ConcurrentDictionary<MeasurementKey, PIPoint> m_mappedPIPoints;    // Cached mapping between GSFSchema measurements and PI points
    private readonly ProcessQueue<AFValue>[] m_archiveQueues;                           // Collection of point archival queues
    private readonly ProcessQueue<MeasurementKey> m_mapRequestQueue;                    // Requested measurement to PI point mapping queue
    private readonly ShortSynchronizedOperation m_restartConnection;                    // Restart connection operation
    private readonly ConcurrentDictionary<Guid, string> m_tagMap;                       // Tag name to measurement Guid lookup
    private readonly HashSet<MeasurementKey> m_pendingMappings;                         // List of pending measurement mappings
    private readonly LongSynchronizedOperation m_handleTagRemoval;                      // Tag removal operation, if any
    private Dictionary<Guid, Ticks> m_lastArchiveTimes;                                 // Cache of last point archive times
    private HashSet<SignalType> m_archiveFilterDataTypes;                               // Data types to archive
    private HashSet<SignalType> m_archiveOnChangeDataTypes;                             // Data types to archive on change
    private Dictionary<string, double> m_compDevDataTypeMap;                            // Defined compression deviations for data types
    private double m_defaultCompDev;                                                    // Default compression deviation
    private string[] m_c37118DigitalStates;                                             // IEEE C37.118 digital state set names
    private readonly bool[] m_c37118MappedDigitalStates;                                // IEEE C37.118 mapped digital state sets
    private string[] m_c37118TagNameExpressions;                                        // IEEE C37.118 tag name expressions
    private readonly TagGenerator[] m_c37118TagNameGenerators;                          // IEEE C37.118 tag name generators
    private Dictionary<MeasurementKey, MeasurementKey[]> m_statusWordMeasurements;      // Status word measurement to status bit measurements map
    private Dictionary<MeasurementKey, int> m_statusBitMeasurements;                    // Status bit measurement to digital state index map
    private Dictionary<MeasurementKey, LineFrequency> m_nominalFrequencies;             // Status word measurement to nominal frequency map
    private Dictionary<MeasurementKey, MeasurementKey[]> m_qualityWordMeasurements;     // Quality word measurement to quality bit measurements map
    private Dictionary<MeasurementKey, int> m_qualityBitMeasurements;                   // Quality bit measurement to digital state index map
    private Dictionary<MeasurementKey, MeasurementKey> m_connectionStatistics;          // Quality word measurement to connection statistic map
    private ReadOnlyDictionary<MeasurementKey, Statistic> m_activeStatistics;           // Registered statistics for the active statistics engine
    private Dictionary<string, Regex> m_digitalBitStateExpressionMap;                   // Digital state set name to digital label expression map
    private Dictionary<string, string> m_digitalBitTagNameExpressionMap;                // Digital state set name to digital bit tag name expression map
    private Dictionary<string, TagGenerator> m_digitalBitTagNameGeneratorMap;           // Digital state set name to digital bit tag name generator map
    private Regex[] m_digitalBitExcludedExpressions;                                    // Excluded digital label expressions
    private Dictionary<MeasurementKey, MeasurementKey[]> m_digitalWordMeasurements;     // Digital word measurement to digital bit measurements map
    private Dictionary<MeasurementKey, (string, int)> m_digitalBitMeasurements;         // Digital bit measurement to digital state / bit map
    private HashSet<MeasurementKey> m_integerValueTypes;                                // Set of measurements that should be treated as integer data types
    private HashSet<MeasurementKey> m_excludedWords;                                    // Set of word-type measurements that should be excluded from archive
    private Dictionary<string, string> m_deviceFirstPhasorLabelCache;                   // Cache of first phasor label for each device lookups
    private readonly Dictionary<Guid, double> m_lastArchiveValues;                      // Cache of last point archive values
    private readonly Dictionary<Guid, SignalType> m_signalTypeMap;                      // Map of signal types for encountered measurements
    private PIConnection m_connection;                                                  // PI server connection for meta-data synchronization
    private long m_pastTimeReasonabilityLimit;                                          // Past-timestamp reasonability limit
    private long m_futureTimeReasonabilityLimit;                                        // Future-timestamp reasonability limit
    private DateTime m_lastMetadataRefresh;                                             // Tracks time of last meta-data refresh
    private long m_processedMappings;                                                   // Total number of mappings processed so far
    private long m_processedMeasurements;                                               // Total number of measurements processed so far
    private long m_totalProcessingTime;                                                 // Total point processing time 
    private volatile bool m_refreshingMetadata;                                         // Flag that determines if meta-data is currently refreshing
    private double m_metadataRefreshProgress;                                           // Current meta-data refresh progress
    private AFUpdateOption m_updateOption;                                              // Active update option for PI point updates
    private ManualResetEventSlim m_configurationReloaded;                               // Wait handle for configuration reload
    private bool m_disposed;                                                            // Flag that determines if class is disposed

    #endregion

    #region [ Constructors ]

    /// <summary>
    /// Creates a new <see cref="PIOutputAdapter"/>
    /// </summary>
    public PIOutputAdapter()
    {
        m_mappedPIPoints = new ConcurrentDictionary<MeasurementKey, PIPoint>();
        m_archiveQueues = new ProcessQueue<AFValue>[Environment.ProcessorCount];

        for (int i = 0; i < m_archiveQueues.Length; i++)
            m_archiveQueues[i] = ProcessQueue<AFValue>.CreateRealTimeQueue(ArchiveAFValues, Timeout.Infinite, false, false);

        m_mapRequestQueue = ProcessQueue<MeasurementKey>.CreateAsynchronousQueue(EstablishPIPointMappings, Environment.ProcessorCount);
        m_restartConnection = new ShortSynchronizedOperation(Start);
        m_tagMap = new ConcurrentDictionary<Guid, string>();
        m_pendingMappings = [];
        m_handleTagRemoval = new LongSynchronizedOperation(HandleTagRemoval, ex => OnProcessException(MessageLevel.Error, ex)) { IsBackground = true };
        m_lastArchiveValues = new Dictionary<Guid, double>();
        m_signalTypeMap = new Dictionary<Guid, SignalType>();
        m_lastMetadataRefresh = DateTime.MinValue;
        m_c37118DigitalStates = DefaultIEEEC37118DigitalStates.Split(',');
        m_c37118MappedDigitalStates = new bool[C37118DigitalStateSets.Count];
        m_c37118TagNameExpressions = DefaultIEEEC371118TagNameExpressions.Split(';');
        m_c37118TagNameGenerators = new TagGenerator[C37118DigitalStateSets.Count];
        m_statusWordMeasurements = [];
        m_statusBitMeasurements = [];
        m_nominalFrequencies = [];
        m_qualityWordMeasurements = [];
        m_qualityBitMeasurements = [];
        m_connectionStatistics = [];
        m_digitalWordMeasurements = [];
        m_digitalBitMeasurements = [];
        m_integerValueTypes = [];
        m_excludedWords = [];
        DigitalBitExcludedExpressions = DefaultDigitalBitExcludedExpressions;
        m_deviceFirstPhasorLabelCache = [];
    }

    #endregion

    #region [ Properties ]
    
    /// <summary>
    /// Gets or sets <see cref="DataSet"/> based data source available to this <see cref="PIOutputAdapter"/>.
    /// </summary>
    public override DataSet DataSource
    {
        get => base.DataSource;
        set
        {
            base.DataSource = value;

            // Notify adapter that configuration has been reloaded
            m_configurationReloaded?.Set();
        }
    }

    /// <summary>
    /// Gets or sets primary keys of input measurements the <see cref="PIOutputAdapter"/> expects.
    /// </summary>
    public override MeasurementKey[] InputMeasurementKeys
    {
        get => base.InputMeasurementKeys;
        set
        {
            base.InputMeasurementKeys = value;
            InputMeasurementKeyTypes = DataSource.GetSignalTypes(value);
        }
    }

    /// <summary>
    /// Gets or sets input measurement <see cref="SignalType"/>'s for each of the <see cref="ActionAdapterBase.InputMeasurementKeys"/>, if any.
    /// </summary>
    public virtual SignalType[] InputMeasurementKeyTypes { get; private set; }

    /// <summary>
    /// Returns true to indicate that this <see cref="PIOutputAdapter"/> is sending measurements to a historian, OSIsoft PI.
    /// </summary>
    public override bool OutputIsForArchive => true;

    /// <summary>
    /// Returns false to indicate that this <see cref="PIOutputAdapter"/> will connect synchronously.
    /// </summary>
    protected override bool UseAsyncConnect => false;

    /// <summary>
    /// Gets or sets the name of the PI server for the adapter's PI connection.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Defines the name of the PI server for the adapter's PI connection.")]
    public string ServerName { get; set; }

    /// <summary>
    /// Gets or sets the name of the PI user ID for the adapter's PI connection.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Defines the name of the PI user ID for the adapter's PI connection.")]
    [DefaultValue(DefaultUserName)]
    public string UserName { get; set; } = DefaultUserName;

    /// <summary>
    /// Gets or sets the password used for the adapter's PI connection.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Defines the password used for the adapter's PI connection.")]
    [DefaultValue(DefaultPassword)]
    public string Password { get; set; } = DefaultPassword;

    /// <summary>
    /// Gets or sets the timeout interval (in milliseconds) for the adapter's connection.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Defines the timeout interval (in milliseconds) for the adapter's connection")]
    [DefaultValue(PIConnection.DefaultConnectTimeout)]
    public int ConnectTimeout { get; set; } = PIConnection.DefaultConnectTimeout;

    /// <summary>
    /// Gets or sets whether this adapter should automatically manage metadata for PI points.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Determines if this adapter should automatically manage metadata for PI points (recommended).")]
    [DefaultValue(DefaultRunMetadataSync)]
    public bool RunMetadataSync { get; set; } = DefaultRunMetadataSync;

    /// <summary>
    /// Gets or sets whether this adapter should automatically create new tags when managing metadata for PI points.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Determines if this adapter should automatically create new tags when managing metadata for PI points (recommended). Value will only be considered when RunMetadataSync is True.")]
    [DefaultValue(DefaultAutoCreateTags)]
    public bool AutoCreateTags { get; set; } = DefaultAutoCreateTags;

    /// <summary>
    /// Gets or sets whether this adapter should automatically update existing tags when managing metadata for PI points.
    /// </summary>
    [ConnectionStringParameter]
    [Description(
        "Determines if this adapter should automatically update existing tags when managing metadata for PI points (recommended). This will make openPDC the controller for maintaining PI tag "+
        "metadata (like the tag name) for local metadata; otherwise, when False, PI tools will be required to maintain points, including local tag name updates. Value will only be considered "+
        "when RunMetadataSync is True.")]
    [DefaultValue(DefaultAutoUpdateTags)]
    public bool AutoUpdateTags { get; set; } = DefaultAutoUpdateTags;

    /// <summary>
    /// Gets or sets whether this adapter should automatically remove PI tags that no longer exist locally in metadata.
    /// </summary>
    [ConnectionStringParameter]
    [Description(
        "Determines if this adapter should automatically remove PI tags that no longer exist locally in metadata (use with caution). This will make openPDC the controller - even for deletes - "+
        "when maintaining PI tag metadata; otherwise, when value is set to DoNotRemove, PI tags will persist even when they no longer exist in local metadata. Value will only be considered "+
        "when RunMetadataSync is True.")]
    [DefaultValue(typeof(TagRemovalOperation), DefaultAutoRemoveTags)]
    public TagRemovalOperation AutoRemoveTags { get; set; } = (TagRemovalOperation)Enum.Parse(typeof(TagRemovalOperation), DefaultAutoRemoveTags);

    /// <summary>
    /// Gets or sets flag that determines if tag synchronization should only use alternate tag fields.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Determines if tag synchronization should only use alternate tag fields. Only relevant when RunMetadataSync is True.")]
    [DefaultValue(DefaultSyncAlternateTagOnly)]
    public bool SyncAlternateTagOnly { get; set; } = DefaultSyncAlternateTagOnly;

    /// <summary>
    /// Gets or sets flag that determines if tag synchronization should skip digitals when alternate tag field is being used.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Determines if tag synchronization should skip digitals when alternate tag field is being used. AlternateTag field is commonly used to hold IEEE C37.118 16-bit digital labels.")]
    [DefaultValue(DefaultSkipDigitalAlternateTagSync)]
    public bool SkipDigitalAlternateTagSync { get; set; } = DefaultSkipDigitalAlternateTagSync;

    /// <summary>
    /// Gets or sets flag that determines if tag synchronization should skip analogs when alternate tag field is being used.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Determines if tag synchronization should skip analogs when alternate tag field is being used. AlternateTag field is commonly used to hold IEEE C37.118 analog labels.")]
    [DefaultValue(DefaultSkipAnalogAlternateTagSync)]
    public bool SkipAnalogAlternateTagSync { get; set; } = DefaultSkipAnalogAlternateTagSync;

    /// <summary>
    /// Gets or sets the number of tag name prefixes, e.g., "SOURCE!", applied by subscriptions to remove from PI tag names.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Defines the number of tag name prefixes applied by subscriptions, e.g., \"SOURCE!\", to remove from PI tag names. Value will only be considered when RunMetadataSync is True.")]
    [DefaultValue(DefaultTagNamePrefixRemoveCount)]
    public int TagNamePrefixRemoveCount { get; set; } = DefaultTagNamePrefixRemoveCount;

    /// <summary>
    /// Gets or sets the point source string used when automatically creating new PI points during the metadata update
    /// </summary>
    [ConnectionStringParameter]
    [Description("Defines the point source string used when automatically creating new PI points during the metadata update. Value will only be considered when RunMetadataSync is True.")]
    [DefaultValue(DefaultPIPointSource)]
    public string PIPointSource { get; set; } = DefaultPIPointSource;

    /// <summary>
    /// Gets or sets the point class string used when automatically creating new PI points during the metadata update. On the PI server, this class should inherit from classic.
    /// </summary>
    [ConnectionStringParameter]
    [Description(
        "Defines the point class string used when automatically creating new PI points during the metadata update. On the PI server, this class should inherit from classic. "+
        "Value will only be considered when RunMetadataSync is True.")]
    [DefaultValue(DefaultPIPointClass)]
    public string PIPointClass { get; set; } = DefaultPIPointClass;

    /// <summary>
    /// Gets or sets the flag that determines if compression will be used during archiving when compression is configured for tag. If disabled, configured tag compression and ReplaceValues setting are ignored.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Defines the flag that determines if compression will be used during archiving when compression is configured for tag. If disabled, configured tag compression and ReplaceValues setting are ignored.")]
    [DefaultValue(DefaultUseCompression)]
    public bool UseCompression { get; set; } = DefaultUseCompression;

    /// <summary>
    /// Gets or sets flag that determines if existing PI values should be replaced when UseCompression is enabled.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Defines the flag that determines if existing PI values should be replaced when UseCompression is enabled.")]
    [DefaultValue(DefaultReplaceValues)]
    public bool ReplaceValues { get; set; } = DefaultReplaceValues;

    /// <summary>
    /// Gets or sets the PI AF value status to use when storing measurements that have been marked as having bad data.
    /// </summary>
    /// <remarks>
    /// This defaults to <see cref="AFValueStatus.Good"/> meaning values are stored with normal status quality in PI.
    /// </remarks>
    [ConnectionStringParameter]
    [Description("Defines the PI AF value status to use when storing measurements that have been marked as having bad data.")]
    [DefaultValue(typeof(AFValueStatus), DefaultBadDataValueStatus)]
    public AFValueStatus BadDataValueStatus { get; set; } = (AFValueStatus)Enum.Parse(typeof(AFValueStatus), DefaultBadDataValueStatus);

    /// <summary>
    /// Gets or sets the PI AF value status to use when storing measurements that have been marked as having bad time.
    /// </summary>
    /// <remarks>
    /// This defaults to <see cref="AFValueStatus.Good"/> meaning values are stored with normal status quality in PI.
    /// </remarks>
    [ConnectionStringParameter]
    [Description("Defines the PI AF value status to use when storing measurements that have been marked as having bad time.")]
    [DefaultValue(typeof(AFValueStatus), DefaultBadTimeValueStatus)]
    public AFValueStatus BadTimeValueStatus { get; set; } = (AFValueStatus)Enum.Parse(typeof(AFValueStatus), DefaultBadTimeValueStatus);

    /// <summary>
    /// Gets or sets the flag that determines if the descriptor state should be updated for existing tags.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Defines the flag that determines if the descriptor state should be updated for existing tags.")]
    [DefaultValue(DefaultUpdateExistingDescriptorState)]
    public bool UpdateExistingDescriptorState { get; set; } = DefaultUpdateExistingDescriptorState;

    /// <summary>
    /// Gets or sets the flag that determines if the compression enabled state should be added to tags if none already exists.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Defines the flag that determines if the compression enabled state should be added to tags if none already exists.")]
    [DefaultValue(DefaultAddTagCompressionState)]
    public bool AddTagCompressionState { get; set; } = DefaultAddTagCompressionState;

    /// <summary>
    /// Gets or sets the flag that determines if the compression enabled state, per UseCompression flag, should be adjusted for existing tags, overriding existing configuration.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Defines the flag that determines if the compression enabled state, per UseCompression flag, should be adjusted for existing tags, overriding existing configuration.")]
    [DefaultValue(DefaultUpdateExistingTagCompressionState)]
    public bool UpdateExistingTagCompressionState { get; set; } = DefaultUpdateExistingTagCompressionState;

    /// <summary>
    /// Gets or sets the defined compression deviations for data types used when AddTagCompressionState or UpdateExistingTagCompressionState is enabled.
    /// </summary>
    [ConnectionStringParameter]
    [Description(
        "Defines the defined compression deviations for data types used when AddTagCompressionState or UpdateExistingTagCompressionState is enabled. "+
        $"Use format \"DataType=Deviation,DataType=Deviation,...\". Example: \"{DefaultCompDevDataTypeMap}\".")]
    [DefaultValue(DefaultCompDevDataTypeMap)]
    public string CompDevDataTypeMap
    {
        get => m_compDevDataTypeMap is null ? DefaultCompDevDataTypeMap : string.Join(",", m_compDevDataTypeMap.Select(kvp => $"{kvp.Key}={kvp.Value}"));
        set
        {
            m_compDevDataTypeMap = new Dictionary<string, double>();

            foreach (string[] parts in value.Split([','], StringSplitOptions.RemoveEmptyEntries).Select(part => part.Split(['='], StringSplitOptions.RemoveEmptyEntries)))
            {
                if (parts.Length == 2 && double.TryParse(parts[1].Trim(), out double compDev))
                    m_compDevDataTypeMap[parts[0].Trim()] = compDev;
            }

            if (m_compDevDataTypeMap.TryGetValue("*", out m_defaultCompDev))
                return;

            m_defaultCompDev = double.Parse(DefaultCompDev);
            m_compDevDataTypeMap["*"] = m_defaultCompDev;
        }
    }

    /// <summary>
    /// Gets or sets the data types to archive. Value of <c>*</c> (or empty string) means all values archived, <c>DIGI</c> means only archive digital values.
    /// Separate multiple values with a comma, for example: <c>DIGI,VPHM,FREQ</c>.
    /// </summary>
    [ConnectionStringParameter]
    [Description(
        "Defines the data types to archive. Value of <c>*</c> (or empty string) means all values archived, 'DIGI' means only archive digital values. "+
        "Separate multiple values with a comma, for example: DIGI,VPHM,FREQ")]
    [DefaultValue(DefaultArchiveFilterDataTypes)]
    public string ArchiveFilterDataTypes
    {
        get => m_archiveFilterDataTypes is null ? DefaultArchiveFilterDataTypes : string.Join(",", m_archiveFilterDataTypes.Select(signalType => signalType.ToString()));
        set
        {
            if (string.IsNullOrWhiteSpace(value) || value.Trim().Equals(DefaultArchiveFilterDataTypes))
            {
                m_archiveFilterDataTypes = null;
            }
            else
            {
                HashSet<SignalType> archiveFilterDataTypes = [];

                bool hasAsterisk = false;

                foreach (string dataTypeEntry in value.Split([','], StringSplitOptions.RemoveEmptyEntries))
                {
                    string dataType = dataTypeEntry.Trim();

                    if (dataType.Equals(DefaultArchiveFilterDataTypes))
                    {
                        hasAsterisk = true;
                        break;
                    }

                    if (Enum.TryParse(dataType, true, out SignalType signalType))
                        archiveFilterDataTypes.Add(signalType);
                }

                if (hasAsterisk)
                    m_archiveFilterDataTypes = null;
                else
                    m_archiveFilterDataTypes = archiveFilterDataTypes.Count > 0 ? archiveFilterDataTypes : null;
            }
        }
    }

    /// <summary>
    /// Gets or sets the data types to only archive on change. Empty string value means all values archived, <c>*</c> means archive all values on change,
    /// <c>DIGI</c> means only archive digital values on change. Separate multiple values with a comma, for example: <c>DIGI,VPHM,FREQ</c>.
    /// </summary>
    [ConnectionStringParameter]
    [Description(
        "Defines the data types to only archive on change. Empty string value means all values archived, '*' means archive all values on change, "+
        "'DIGI' means only archive digital values on change. Separate multiple values with a comma, for example: DIGI,VPHM,FREQ")]
    [DefaultValue(DefaultArchiveOnChangeDataTypes)]
    public string ArchiveOnChangeDataTypes
    {
        get => m_archiveOnChangeDataTypes is null ? string.Empty : string.Join(",", m_archiveOnChangeDataTypes.Select(signalType => signalType.ToString()));
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                m_archiveOnChangeDataTypes = null;
            }
            else
            {
                HashSet<SignalType> archiveOnChangeDataTypes = [];

                bool hasAsterisk = false;

                foreach (string dataTypeEntry in value.Split([','], StringSplitOptions.RemoveEmptyEntries))
                {
                    string dataType = dataTypeEntry.Trim();

                    if (dataType.Equals("*"))
                    {
                        hasAsterisk = true;
                        break;
                    }

                    if (Enum.TryParse(dataType, true, out SignalType signalType))
                        archiveOnChangeDataTypes.Add(signalType);
                }

                if (hasAsterisk)
                    archiveOnChangeDataTypes = [.. Enum.GetValues(typeof(SignalType)).Cast<SignalType>()];

                m_archiveOnChangeDataTypes = archiveOnChangeDataTypes.Count > 0 ? archiveOnChangeDataTypes : null;
            }
        }
    }

    /// <summary>
    /// Gets or sets the filename to be used for tag map cache.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Defines the filename to be used for tag map cache file name. Leave blank for cache name to be same as adapter name with a \".cache\" extension.")]
    [DefaultValue(DefaultTagMapCacheFileName)]
    public string TagMapCacheFileName { get; set; } = DefaultTagMapCacheFileName;

    /// <summary>
    /// Gets or sets the maximum time resolution, in seconds, for data points being archived, e.g., a value 1.0 would mean that data would be archived no
    /// more than once per second. A zero value indicates that all data should be archived.
    /// </summary>
    [ConnectionStringParameter]
    [Description(
        "Defines the maximum time resolution, in seconds, for data points being archived, e.g., a value 1.0 would mean that data would be archived no "+
        "more than once per second. A zero value indicates that all data should be archived.")]
    [DefaultValue(DefaultMaximumPointResolution)]
    public double MaximumPointResolution { get; set; } = DefaultMaximumPointResolution;

    /// <summary>
    /// Gets or sets flag that indicates if incoming timestamps to the historian should be validated for reasonability.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Define the flag that indicates if incoming timestamps to the historian should be validated for reasonability.")]
    [DefaultValue(DefaultEnableTimeReasonabilityCheck)]
    public bool EnableTimeReasonabilityCheck { get; set; } = DefaultEnableTimeReasonabilityCheck;

    /// <summary>
    /// Gets or sets the maximum number of seconds that a past timestamp, as compared to local clock, will be considered valid.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Define the maximum number of seconds that a past timestamp, as compared to local clock, will be considered valid.")]
    [DefaultValue(DefaultPastTimeReasonabilityLimit)]
    public double PastTimeReasonabilityLimit
    {
        get => new Ticks(m_pastTimeReasonabilityLimit).ToSeconds();
        set => m_pastTimeReasonabilityLimit = Ticks.FromSeconds(Math.Abs(value));
    }

    /// <summary>
    /// Gets or sets the maximum number of seconds that a future timestamp, as compared to local clock, will be considered valid.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Define the maximum number of seconds that a future timestamp, as compared to local clock, will be considered valid.")]
    [DefaultValue(DefaultFutureTimeReasonabilityLimit)]
    public double FutureTimeReasonabilityLimit
    {
        get => new Ticks(m_futureTimeReasonabilityLimit).ToSeconds();
        set => m_futureTimeReasonabilityLimit = Ticks.FromSeconds(Math.Abs(value));
    }

    /// <summary>
    /// Gets or sets flag that determines if IEEE C37.118 status bits should be expanded to tags.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Determines if IEEE C37.118 status bits should be expanded to tags.")]
    [DefaultValue(DefaultExpandStatusBitsToTags)]
    public bool ExpandStatusBitsToTags { get; set; } = DefaultExpandStatusBitsToTags;

    /// <summary>
    /// Gets or sets flag that determines if status word should be written to PI as a separate tag. Commonly disabled if IEEE C37.118 status bits are expanded to tags, see <see cref="ExpandStatusBitsToTags"/>.
    /// </summary>
    [ConnectionStringParameter]
    [Description($"Determines if status word should be written to PI as a separate tag. Commonly disabled if IEEE C37.118 status bits are expanded to tags, see \"{nameof(ExpandStatusBitsToTags)}\".")]
    [DefaultValue(DefaultWriteStatusWord)]
    public bool WriteStatusWord { get; set; } = DefaultWriteStatusWord;

    /// <summary>
    /// Gets or sets flag that determines if IEEE C37.118 quality bits should be expanded to tags.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Determines if IEEE C37.118 quality bits should be expanded to tags.")]
    [DefaultValue(DefaultExpandQualityBitsToTags)]
    public bool ExpandQualityBitsToTags { get; set; } = DefaultExpandQualityBitsToTags;

    /// <summary>
    /// Gets or sets flag that determines if quality word should be written to PI as a separate tag. Commonly disabled if IEEE C37.118 quality bits are expanded to tags, see <see cref="ExpandQualityBitsToTags"/>.
    /// </summary>
    [ConnectionStringParameter]
    [Description($"Determines if quality word should be written to PI as a separate tag. Commonly disabled if IEEE C37.118 quality bits are expanded to tags, see \"{nameof(ExpandQualityBitsToTags)}\".")]
    [DefaultValue(DefaultWriteQualityWord)]
    public bool WriteQualityWord { get; set; } = DefaultWriteQualityWord;

    /// <summary>
    /// Gets or sets the comma separated digital state set names for IEEE C37.118 status states. Specify digital state set name for each of the following digital states using value of 'X' (without quotes)
    /// as the name to indicate state is not mapped: CompositeQual, ConfigChange, ConnectState, DataSorting, DataValid, LeapSecond, NominalFreq, PMUError, SyncError, Timelock, TimeQuality, and Trigger.
    /// If digital state sets are predefined, state values are expected to be zero based and incremented by one for each value. If specified digital set name does not exist, it will be created.
    /// </summary>
    [ConnectionStringParameter]
    [Description(
        "Defines the comma separated digital state set names for IEEE C37.118 status states. Specify digital state set name for each of the following digital states using value of 'X' (without quotes) " + 
        "as the name to indicate state is not mapped:\r\n CompositeQual, ConfigChange, ConnectState, DataSorting, DataValid, LeapSecond, NominalFreq, PMUError, SyncError, Timelock, TimeQuality, and Trigger.\r\n" + 
        "If digital sets are predefined, state values are expected to be zero based and incremented by one for each value. If specified digital set name does not exist, it will be created.")
    ]
    [DefaultValue(DefaultIEEEC37118DigitalStates)]
    public string IEEEC37118DigitalStates
    {
        get => string.Join(", ", m_c37118DigitalStates);
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                value = DefaultIEEEC37118DigitalStates;

            string[] states = value.Split([','], StringSplitOptions.RemoveEmptyEntries);

            if (states.Length != C37118DigitalStateSets.Count)
                throw new InvalidOperationException($"Expecting one digital state set name for each of \"{DefaultIEEEC37118DigitalStates}\"");

            m_c37118DigitalStates = states;
        }
    }

    /// <summary>
    /// Gets or sets the semicolon separated tag naming expressions for IEEE C37.118 status states. Specify tag naming expression for each of the following digital states:
    /// CompositeQual, ConfigChange, ConnectState, DataSorting, DataValid, LeapSecond, NominalFreq, PMUError, SyncError, Timelock, TimeQuality, and Trigger.
    /// Expression can be left blank if no digital state name is mapped for the corresponding state in <see cref="IEEEC37118DigitalStates"/>.
    /// </summary>
    [ConnectionStringParameter]
    [Description(
        "Defines the semicolon separated tag naming expressions for IEEE C37.118 status states. Specify tag naming expression for each of the following digital states:\r\n" + 
        "CompositeQual, ConfigChange, ConnectState, DataSorting, DataValid, LeapSecond, NominalFreq, PMUError, SyncError, Timelock, TimeQuality, and Trigger.\r\n" +
        $"Expression can be left blank if no digital state name is mapped for the corresponding state in '{nameof(IEEEC37118DigitalStates)}'.")
    ]
    [DefaultValue(DefaultIEEEC371118TagNameExpressions)]
    public string IEEEC37118TagNameExpressions
    {
        get => string.Join(", ", m_c37118TagNameExpressions);
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                value = DefaultIEEEC371118TagNameExpressions;

            string[] expressions = value.Split(';');

            if (expressions.Length != C37118DigitalStateSets.Count)
                throw new InvalidOperationException($"Expecting one tag naming expression for each digital state set name of \"{DefaultIEEEC37118DigitalStates}\"");

            // We cache original strings since templated expression parser encodes special characters
            m_c37118TagNameExpressions = expressions;

            // Establish tag name generators for status bits
            for (int i = 0; i < C37118DigitalStateSets.Count; i++)
            {
                m_c37118TagNameGenerators[i] = new TagGenerator
                {
                    TemplatedExpression = m_c37118TagNameExpressions[i]
                };
            }
        }
    }

    /// <summary>
    /// Gets or sets flag that determines if digital bits should be expanded to tags.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Determines if digital bits should be expanded to tags.")]
    [DefaultValue(DefaultExpandDigitalBitsToTags)]
    public bool ExpandDigitalBitsToTags { get; set; } = DefaultExpandDigitalBitsToTags;

    /// <summary>
    /// Gets or sets flag that determines if digital word should be written to PI as a separate tag. Commonly disabled if digital bits are expanded to tags, see <see cref="ExpandDigitalBitsToTags"/>.
    /// </summary>
    [ConnectionStringParameter]
    [Description($"Determines if digital word should be written to PI as a separate tag. Commonly disabled if digital bits are expanded to tags, see \"{nameof(ExpandDigitalBitsToTags)}\".")]
    [DefaultValue(DefaultWriteDigitalWord)]
    public bool WriteDigitalWord { get; set; } = DefaultWriteDigitalWord;

    /// <summary>
    /// Gets or sets the semicolon separated pre-existing digital bit state set names mapped to a regular expression for matching digital labels.
    /// Use format "DigitalStateName=Expression;DigitalStateName=Expression;...". Use "*" for default expression.
    /// </summary>
    [ConnectionStringParameter]
    [Description(
        "Defines the semicolon separated pre-existing digital bit state set names mapped to a regular expression for matching digital labels. " +
        "Use format \"DigitalStateName=Expression;DigitalStateName=Expression;...\". Use \"*\" for default expression.")
    ]
    [DefaultValue(DefaultDigitalBitStateExpressionMap)]
    public string DigitalBitStateExpressionMap
    {
        get => m_digitalBitStateExpressionMap is null ? "" : string.Join("; ", m_digitalBitStateExpressionMap.Select(kvp => $"{kvp.Key}={kvp.Value}"));
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                m_digitalBitStateExpressionMap = null;
                return;
            }

            Dictionary<string, Regex> digitalStateSetExpressionMap = new(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, string> settings = value.ParseKeyValuePairs();

            foreach (KeyValuePair<string, string> setting in settings)
            {
                string key = setting.Key.Trim();
                string expression = setting.Value.Trim();

                digitalStateSetExpressionMap[key] = new Regex(expression, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }

            m_digitalBitStateExpressionMap = digitalStateSetExpressionMap;
        }
    }

    /// <summary>
    /// Gets or sets the semicolon separated digital state set name to tag name expression map. One expression should
    /// exist for each state defined in <see cref="DigitalBitStateExpressionMap"/>.
    /// Use format "DigitalStateName=Expression;DigitalStateName=Expression;...". Use "*" for default expression.
    /// </summary>
    [ConnectionStringParameter]
    [Description(
        $"Defines the semicolon separated digital state set name to tag name expression map. One expression should exist for each state defined in '{nameof(DigitalBitStateExpressionMap)}'. "+
        "Use format \"DigitalStateName=Expression;DigitalStateName=Expression;...\". Use \"*\" for default expression.")
    ]
    [DefaultValue(DefaultDigitalBitTagNameExpressionMap)]
    public string DigitalBitTagNameExpressionMap
    {
        get => m_digitalBitTagNameExpressionMap is null ? "" : m_digitalBitTagNameExpressionMap.JoinKeyValuePairs();
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                m_digitalBitTagNameExpressionMap = null;
                m_digitalBitTagNameGeneratorMap = null;
                return;
            }

            // We cache original strings since templated expression parser encodes special characters
            m_digitalBitTagNameExpressionMap = value.ParseKeyValuePairs();

            // Establish tag name generators for digital bits
            m_digitalBitTagNameGeneratorMap = new Dictionary<string, TagGenerator>(StringComparer.OrdinalIgnoreCase);

            foreach (KeyValuePair<string, string> kvp in m_digitalBitTagNameExpressionMap)
            {
                m_digitalBitTagNameGeneratorMap[kvp.Key] = new TagGenerator
                {
                    TemplatedExpression = kvp.Value
                };
            }
        }
    }

    /// <summary>
    /// Gets or sets the semicolon separated regular expressions for excluding digital labels from being expanded to tags.
    /// </summary>
    [ConnectionStringParameter]
    [Description("Defines the semicolon separated regular expressions for excluding digital labels from being expanded to tags.")]
    [DefaultValue(DefaultDigitalBitExcludedExpressions)]
    public string DigitalBitExcludedExpressions
    {
        get => string.Join("; ", m_digitalBitExcludedExpressions.Select(expression => expression.ToString()));
        set => m_digitalBitExcludedExpressions = string.IsNullOrWhiteSpace(value) ? [] : value.Split([';'], StringSplitOptions.RemoveEmptyEntries)
            .Select(expression => new Regex(expression, RegexOptions.IgnoreCase | RegexOptions.Compiled)).ToArray();
    }

    /// <summary>
    /// Returns the detailed status of the data output source.
    /// </summary>
    public override string Status
    {
        get
        {
            StringBuilder status = new();

            status.Append(base.Status);
            status.AppendLine();
            status.AppendLine($"        OSI-PI server name: {ServerName}");
            status.AppendLine($"       Connected to server: {(m_connection?.Connected ?? false ? "Yes" : "No")}");
            status.AppendLine($"         Using compression: {UseCompression}");
            status.AppendLine($"   Replace existing values: {(UseCompression ? ReplaceValues : "N/A when not using compression")}");
            status.AppendLine($"     Bad data value status: {BadDataValueStatus}");
            status.AppendLine($"     Bad time value status: {BadTimeValueStatus}");
            status.AppendLine($"   Update descriptor state: {UpdateExistingDescriptorState}");
            status.AppendLine($" Add tag compression state: {AddTagCompressionState}");
            status.AppendLine($"  Update compression state: {UpdateExistingTagCompressionState}");
            status.AppendLine($" Compression deviation map: {CompDevDataTypeMap}");
            status.AppendLine($" Archive filter data types: {ArchiveFilterDataTypes}");
            status.AppendLine($"   Archive on change types: {ArchiveOnChangeDataTypes}");
            status.AppendLine($"  Maximum point resolution: {MaximumPointResolution:N3} seconds{(MaximumPointResolution <= 0.0D ? " - all data will be archived" : "")}");
            status.AppendLine($"  Time reasonability check: {(EnableTimeReasonabilityCheck ? "Enabled" : "Not Enabled")}");
            status.AppendLine($" Write Status Bits to Tags: {ExpandStatusBitsToTags}");
            status.AppendLine($"         Write Status Word: {WriteStatusWord}");
            status.AppendLine($"Write Quality Bits to Tags: {ExpandQualityBitsToTags}");
            status.AppendLine($"        Write Quality Word: {WriteQualityWord}");
            status.AppendLine($"IEEEC37.118 Digital States: {IEEEC37118DigitalStates}");
            status.AppendLine($"IEEEC37.118 Tag Name Exprs: {IEEEC37118TagNameExpressions}");
            status.AppendLine($"Write Digital Bits to Tags: {ExpandDigitalBitsToTags}");
            status.AppendLine($"        Write Digital Word: {WriteDigitalWord}");
            status.AppendLine($"Digital Bit State Expr Map: {DigitalBitStateExpressionMap}");
            status.AppendLine($" Digital Tag Name Expr Map: {DigitalBitTagNameExpressionMap}");
            status.AppendLine($"Digital Bit Excluded Exprs: {DigitalBitExcludedExpressions}");

            if (EnableTimeReasonabilityCheck)
            {
                status.AppendLine($"   Maximum past time limit: {PastTimeReasonabilityLimit:N4}s, i.e., {new Ticks(m_pastTimeReasonabilityLimit).ToElapsedTimeString(4)}");
                status.AppendLine($" Maximum future time limit: {FutureTimeReasonabilityLimit:N4}s, i.e., {new Ticks(m_futureTimeReasonabilityLimit).ToElapsedTimeString(4)}");
            }

            status.AppendLine($"    Meta-data sync enabled: {RunMetadataSync}");

            if (RunMetadataSync)
            {
                status.AppendLine($"          Auto-create tags: {AutoCreateTags}");
                status.AppendLine($"          Auto-update tags: {AutoUpdateTags}");
                status.AppendLine($"          Auto-remove tags: {AutoRemoveTags}");
                status.AppendLine($"   Sync alternate tag only: {SyncAlternateTagOnly}");
                status.AppendLine($" Skip digital alt-tag sync: {SkipDigitalAlternateTagSync}");
                status.AppendLine($"  Skip analog alt-tag sync: {SkipAnalogAlternateTagSync}");
                status.AppendLine($"    Tag prefixes to remove: {TagNamePrefixRemoveCount}");
                status.AppendLine($"       OSI-PI point source: {PIPointSource}");
                status.AppendLine($"        OSI-PI point class: {PIPointClass}");
            }

            if (m_mapRequestQueue is not null)
            {
                status.AppendLine();
                status.AppendLine(">> Mapping Request Queue Status");
                status.AppendLine();
                status.Append(m_mapRequestQueue.Status);
                status.AppendLine();
            }

            status.AppendLine($"   Tag-map cache file name: {FilePath.TrimFileName(TagMapCacheFileName.ToNonNullString("[undefined]"), 51)}");
            status.AppendLine($" Active tag-map cache size: {m_mappedPIPoints.Count:N0} mappings");
            status.AppendLine($"      Pending tag-mappings: {m_pendingMappings.Count:N0} mappings, {1.0D - m_pendingMappings.Count / (double)m_mappedPIPoints.Count:0.00%} complete");

            if (RunMetadataSync)
                status.AppendLine($"    Meta-data sync process: {(m_refreshingMetadata ? "Active" : "Idle")}, {m_metadataRefreshProgress:0.00%} complete");

            if (m_archiveQueues is not null && m_archiveQueues.Length > 0)
            {
                status.AppendLine();
                status.AppendLine($">> Archive Queue Status (1 of {m_archiveQueues.Length:N0})");
                status.AppendLine();
                status.Append(m_archiveQueues[0].Status);
                status.AppendLine();
            }

            status.AppendLine($"    Points archived/second: {Interlocked.Read(ref m_processedMeasurements) / (Interlocked.Read(ref m_totalProcessingTime) / (double)Ticks.PerSecond):#,##0.00}");

            return status.ToString();
        }
    }

    #endregion

    #region [ Methods ]

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="PIOutputAdapter"/> object and optionally releases the managed resources.
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

            m_mapRequestQueue?.Dispose();

            if (m_archiveQueues is not null)
            {
                foreach (ProcessQueue<AFValue> archiveQueue in m_archiveQueues)
                    archiveQueue.Dispose();
            }

            if (m_connection is not null)
            {
                m_connection.Disconnected -= Connection_Disconnected;
                m_connection.Dispose();
                m_connection = null;
            }

            m_mappedPIPoints?.Clear();

            if (m_configurationReloaded is null)
                return;

            m_configurationReloaded.Set();
            m_configurationReloaded.Dispose();
        }
        finally
        {
            m_disposed = true;       // Prevent duplicate dispose.
            base.Dispose(disposing); // Call base class Dispose().
        }
    }

    /// <summary>
    /// Gets full archive queue status.
    /// </summary>
    [AdapterCommand("Gets full archive queue status.", "Administrator", "Editor")]
    public void GetArchiveQueueStatus()
    {
        if (m_archiveQueues is null)
            return;

        StringBuilder status = new();

        for (int i = 0; i < m_archiveQueues.Length; i++)
        {
            status.AppendLine();
            status.AppendLine($">> Archive Queue Status (1 of {i:N0})");
            status.AppendLine();
            status.Append(m_archiveQueues[i].Status);
            status.AppendLine();
        }

        OnStatusMessage(MessageLevel.Info, status.ToString());
    }

    /// <summary>
    /// Returns a brief status of this <see cref="PIOutputAdapter"/>
    /// </summary>
    /// <param name="maxLength">Maximum number of characters in the status string</param>
    /// <returns>Short status.</returns>
    public override string GetShortStatus(int maxLength)
    {
        return $"Archived {m_processedMeasurements:N0} measurements to PI.".CenterText(maxLength);
    }

    /// <summary>
    /// Initializes this <see cref="PIOutputAdapter"/>.
    /// </summary>
    public override void Initialize()
    {
        m_configurationReloaded = new ManualResetEventSlim();

        base.Initialize();

        Dictionary<string, string> settings = Settings;

        if (!settings.TryGetValue(nameof(ServerName), out string setting))
            throw new InvalidOperationException($"The \"{nameof(ServerName)}\" setting is required for PI connections.");

        ServerName = setting;

        // Track instance in static dictionary
        Instances[ServerName] = this;

        if (settings.TryGetValue(nameof(UserName), out setting))
            UserName = setting;

        if (settings.TryGetValue(nameof(Password), out setting))
            Password = setting;

        if (settings.TryGetValue(nameof(ConnectTimeout), out setting) && int.TryParse(setting, out int intVal))
            ConnectTimeout = intVal;

        if (settings.TryGetValue(nameof(UseCompression), out setting))
            UseCompression = setting.ParseBoolean();

        if (settings.TryGetValue(nameof(ReplaceValues), out setting))
            ReplaceValues = setting.ParseBoolean();

        if (settings.TryGetValue(nameof(BadDataValueStatus), out setting) && Enum.TryParse(setting, out AFValueStatus badValueStatus))
            BadDataValueStatus = badValueStatus;

        if (settings.TryGetValue(nameof(BadTimeValueStatus), out setting) && Enum.TryParse(setting, out AFValueStatus badTimeStatus))
            BadTimeValueStatus = badTimeStatus;

        if (settings.TryGetValue(nameof(UpdateExistingDescriptorState), out setting))
            UpdateExistingDescriptorState = setting.ParseBoolean();

        // Derive update option based on UseCompression / ReplaceValues settings
        m_updateOption = UseCompression ? ReplaceValues ? AFUpdateOption.Replace : AFUpdateOption.Insert : AFUpdateOption.InsertNoCompression;

        if (settings.TryGetValue(nameof(AddTagCompressionState), out setting))
            AddTagCompressionState = setting.ParseBoolean();

        if (settings.TryGetValue(nameof(UpdateExistingTagCompressionState), out setting))
            UpdateExistingTagCompressionState = setting.ParseBoolean();

        CompDevDataTypeMap = settings.TryGetValue(nameof(CompDevDataTypeMap), out setting) ? setting : DefaultCompDevDataTypeMap;

        if (settings.TryGetValue(nameof(ArchiveFilterDataTypes), out setting))
            ArchiveFilterDataTypes = setting;

        if (settings.TryGetValue(nameof(ArchiveOnChangeDataTypes), out setting))
            ArchiveOnChangeDataTypes = setting;

        if (settings.TryGetValue(nameof(TagNamePrefixRemoveCount), out setting) && int.TryParse(setting, out intVal))
            TagNamePrefixRemoveCount = intVal;

        if (settings.TryGetValue(nameof(RunMetadataSync), out setting))
            RunMetadataSync = setting.ParseBoolean();

        if (settings.TryGetValue(nameof(AutoCreateTags), out setting))
            AutoCreateTags = setting.ParseBoolean();

        if (settings.TryGetValue(nameof(AutoUpdateTags), out setting))
            AutoUpdateTags = setting.ParseBoolean();

        if (settings.TryGetValue(nameof(AutoRemoveTags), out setting) && Enum.TryParse(setting, out TagRemovalOperation removalOperation))
            AutoRemoveTags = removalOperation;

        if (settings.TryGetValue(nameof(SyncAlternateTagOnly), out setting))
            SyncAlternateTagOnly = setting.ParseBoolean();

        if (settings.TryGetValue(nameof(SkipDigitalAlternateTagSync), out setting))
            SkipDigitalAlternateTagSync = setting.ParseBoolean();

        if (settings.TryGetValue(nameof(SkipAnalogAlternateTagSync), out setting))
            SkipAnalogAlternateTagSync = setting.ParseBoolean();

        if (settings.TryGetValue(nameof(PIPointSource), out setting))
            PIPointSource = setting;

        if (settings.TryGetValue(nameof(PIPointClass), out setting))
            PIPointClass = setting;

        if (!settings.TryGetValue(nameof(TagMapCacheFileName), out setting) || string.IsNullOrWhiteSpace(setting))
            setting = Name + "_TagMap.cache";

        TagMapCacheFileName = FilePath.GetAbsolutePath(setting);

        if (settings.TryGetValue(nameof(MaximumPointResolution), out setting) && double.TryParse(setting, out double doubleVal) && doubleVal > 0.0D)
            MaximumPointResolution = doubleVal;

        if (MaximumPointResolution > 0.0D)
            m_lastArchiveTimes = new Dictionary<Guid, Ticks>();

        if (settings.TryGetValue(nameof(EnableTimeReasonabilityCheck), out setting))
            EnableTimeReasonabilityCheck = setting.ParseBoolean();

        if (settings.TryGetValue(nameof(PastTimeReasonabilityLimit), out setting) && double.TryParse(setting, out double value))
            PastTimeReasonabilityLimit = value;
        else
            PastTimeReasonabilityLimit = DefaultPastTimeReasonabilityLimit;

        if (settings.TryGetValue(nameof(FutureTimeReasonabilityLimit), out setting) && double.TryParse(setting, out value))
            FutureTimeReasonabilityLimit = value;
        else
            FutureTimeReasonabilityLimit = DefaultFutureTimeReasonabilityLimit;

        if (settings.TryGetValue(nameof(ExpandStatusBitsToTags), out setting))
            ExpandStatusBitsToTags = setting.ParseBoolean();

        if (settings.TryGetValue(nameof(WriteStatusWord), out setting))
            WriteStatusWord = setting.ParseBoolean();

        if (settings.TryGetValue(nameof(ExpandQualityBitsToTags), out setting))
            ExpandQualityBitsToTags = setting.ParseBoolean();

        if (settings.TryGetValue(nameof(WriteQualityWord), out setting))
            WriteQualityWord = setting.ParseBoolean();

        if (settings.TryGetValue(nameof(IEEEC37118DigitalStates), out setting))
            IEEEC37118DigitalStates = setting;

        if (settings.TryGetValue(nameof(IEEEC37118TagNameExpressions), out setting))
            IEEEC37118TagNameExpressions = setting;

        if (settings.TryGetValue(nameof(ExpandDigitalBitsToTags), out setting))
            ExpandDigitalBitsToTags = setting.ParseBoolean();

        if (settings.TryGetValue(nameof(WriteDigitalWord), out setting))
            WriteDigitalWord = setting.ParseBoolean();

        if (settings.TryGetValue(nameof(DigitalBitStateExpressionMap), out setting))
            DigitalBitStateExpressionMap = setting;

        if (settings.TryGetValue(nameof(DigitalBitTagNameExpressionMap), out setting))
            DigitalBitTagNameExpressionMap = setting;

        if (settings.TryGetValue(nameof(DigitalBitExcludedExpressions), out setting))
            DigitalBitExcludedExpressions = setting;

        if (ExpandDigitalBitsToTags && string.IsNullOrWhiteSpace(DigitalBitStateExpressionMap))
            OnStatusMessage(MessageLevel.Warning, $"No digital state name expressions are defined in '{nameof(DigitalBitStateExpressionMap)}', digital bits will not be expanded to tags.");

        string[] digitalStatesForLabelExpressions = m_digitalBitStateExpressionMap is null ? [] : m_digitalBitStateExpressionMap.Keys.Select(key => key.ToLowerInvariant()).ToArray();
        string[] digitalStatesForTagNameExpressions = m_digitalBitTagNameExpressionMap is null ? [] : m_digitalBitTagNameExpressionMap.Keys.Select(key => key.ToLowerInvariant()).ToArray();

        if (digitalStatesForLabelExpressions.CompareTo(digitalStatesForTagNameExpressions, false) != 0)
            throw new InvalidOperationException($"Digital bit expression configuration mismatch: expecting one digital state set name in '{nameof(DigitalBitTagNameExpressionMap)}' for each defined in '{nameof(DigitalBitStateExpressionMap)}'.");

        if (RunMetadataSync && !SkipDigitalAlternateTagSync && ExpandDigitalBitsToTags && m_digitalBitTagNameExpressionMap?.Count > 0)
            OnStatusMessage(MessageLevel.Warning, "Digital alternate tag sync is enabled, but digital bits are also being requested to be expanded to tags - digital tag expansion may not operate as expected for alternate tags with non-standard digital label formats.");

        // Initialize CALC signal type field substitutions (one time per system run is sufficient)
        if (s_calcSignalTypeFields is not null)
            return;
        
        // Postponed initialization of CALC signal type field substitutions until here, instead of static constructor, so we could be sure database connection is available
        using AdoDataConnection database = new("systemSettings");
        DataRow calcSignalType = database.Connection.RetrieveData(database.AdapterType, "SELECT * FROM SignalType WHERE Acronym='CALC'").AsEnumerable().FirstOrDefault()
                                 ?? throw new InvalidOperationException("No database definition was found for signal type 'CALC'");

        DataColumnCollection columns = calcSignalType.Table.Columns;
        s_calcSignalTypeFields = [];

        for (int i = 0; i < columns.Count; i++)
            s_calcSignalTypeFields.Add($"{{SignalType.{columns[i].ColumnName}}}", calcSignalType[i].ToNonNullString());
    }

    /// <summary>
    /// Connects to the configured PI server.
    /// </summary>
    protected override void AttemptConnection()
    {
        m_processedMappings = 0;
        m_processedMeasurements = 0;
        m_totalProcessingTime = 0;

        m_connection = new PIConnection
        {
            ServerName = ServerName,
            UserName = UserName,
            Password = Password,
            ConnectTimeout = ConnectTimeout
        };

        m_connection.Disconnected += Connection_Disconnected;
        m_connection.Open();

        m_mappedPIPoints.Clear();

        lock (m_pendingMappings)
            m_pendingMappings.Clear();

        m_mapRequestQueue.Clear();
        m_mapRequestQueue.Start();

        foreach (ProcessQueue<AFValue> archiveQueue in m_archiveQueues)
            archiveQueue.Start();

        // Kick off meta-data refresh
        RefreshMetadata();

        if (ExpandQualityBitsToTags)
            StatisticsEngine.Calculated += StatisticsEngine_Calculated;
    }

    /// <summary>
    /// Closes this <see cref="PIOutputAdapter"/> connections to the PI server.
    /// </summary>
    protected override void AttemptDisconnection()
    {
        if (ExpandQualityBitsToTags)
            StatisticsEngine.Calculated -= StatisticsEngine_Calculated;

        foreach (ProcessQueue<AFValue> archiveQueue in m_archiveQueues)
            archiveQueue.Stop();

        m_mapRequestQueue.Stop();
        m_mapRequestQueue.Clear();

        m_mappedPIPoints.Clear();

        lock (m_pendingMappings)
            m_pendingMappings.Clear();

        if (m_connection is not null)
        {
            m_connection.Disconnected -= Connection_Disconnected;
            m_connection.Dispose();
            m_connection = null;
        }

        m_configurationReloaded?.Set();
    }

    /// <summary>
    /// Queues a collection of measurements for processing. Measurements are automatically filtered to the defined <see cref="IAdapter.InputMeasurementKeys"/>.
    /// </summary>
    /// <param name="measurements">Measurements to queue for processing.</param>
    public override void QueueMeasurementsForProcessing(IEnumerable<IMeasurement> measurements)
    {
        if (m_archiveFilterDataTypes is null && m_archiveOnChangeDataTypes is null)
        {
            base.QueueMeasurementsForProcessing(measurements);
        }
        else
        {
            List<IMeasurement> measurementsToProcess = [];

            foreach (IMeasurement measurement in measurements)
            {
                // Lookup measurement's signal type (data source lookup hit taken once per encountered measurement)
                SignalType signalType = m_signalTypeMap.GetOrAdd(measurement.ID, _ => DataSource.GetSignalType(measurement.Key));

                // Handle measurement filtering based on signal type
                if (m_archiveFilterDataTypes is not null && !m_archiveFilterDataTypes.Contains(signalType))
                    continue;

                // If measurement signal type is not an archive on change data type target, process measurement as normal
                if (m_archiveOnChangeDataTypes is null || !m_archiveOnChangeDataTypes.Contains(signalType))
                {
                    measurementsToProcess.Add(measurement);
                    continue;
                }

                // Get measurement's last archive value (NaN for first encounter)
                double value = m_lastArchiveValues.GetOrAdd(measurement.ID, _ => double.NaN);

                // Skip measurement processing if value has not changed
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (value == measurement.Value)
                    continue;

                // Update last archive value and process measurement
                m_lastArchiveValues[measurement.ID] = measurement.Value;
                measurementsToProcess.Add(measurement);
            }

            if (measurementsToProcess.Count > 0)
                base.QueueMeasurementsForProcessing(measurementsToProcess);
        }
    }

    /// <summary>
    /// Sorts measurements and sends them to the configured PI server in batches
    /// </summary>
    /// <param name="measurements">Measurements to queue</param>
    protected override void ProcessMeasurements(IMeasurement[] measurements)
    {
        if (measurements is null || measurements.Length == 0 || m_connection is null)
            return;

        foreach (IMeasurement measurement in measurements)
        {
            // If adapter gets disabled while executing this thread - go ahead and exit
            if (!Enabled)
                return;

            MeasurementKey key = measurement.Key;
            Ticks timestamp = measurement.Timestamp;

            // Validate timestamp reasonability as compared to local clock, when enabled
            if (EnableTimeReasonabilityCheck)
            {
                long deviation = DateTime.UtcNow.Ticks - timestamp.Value;

                if (deviation < -m_futureTimeReasonabilityLimit || deviation > m_pastTimeReasonabilityLimit)
                    continue;
            }

            MeasurementKey[] bitKeys = null;
            bool isExpandedStatusWord = ExpandStatusBitsToTags && m_statusWordMeasurements.TryGetValue(key, out bitKeys);
            bool isExpandedQualityWord = ExpandQualityBitsToTags && !isExpandedStatusWord && m_qualityWordMeasurements.TryGetValue(key, out bitKeys);
            bool isExpandedDigitalWord = ExpandDigitalBitsToTags && !isExpandedStatusWord && !isExpandedQualityWord && m_digitalWordMeasurements.TryGetValue(key, out bitKeys);
            bool isExpandedWord = isExpandedStatusWord || isExpandedQualityWord || isExpandedDigitalWord;

            // Lookup connection point mapping for this measurement, if it wasn't found - go ahead and exit
            if (!m_mappedPIPoints.TryGetValue(key, out PIPoint point) && !isExpandedWord)
                continue;

            if (isExpandedStatusWord)
                ProcessStatusWordBitStates(measurement, bitKeys);
            else if (isExpandedQualityWord)
                ProcessQualityWordBitStates(measurement, bitKeys);
            else if (isExpandedDigitalWord)
                ProcessDigitalWordBitStates(measurement, bitKeys);

            // If status, quality or digital word is not to be written, skip processing
            if (m_excludedWords.Contains(key))
                continue;

            if (point is null)
            {
                // If connection point is not defined, kick off process to create a new mapping
                try
                {
                    bool mappingRequested = false;

                    lock (m_pendingMappings)
                    {
                        // Only start mapping process if one is not already pending
                        if (!m_pendingMappings.Contains(key))
                        {
                            mappingRequested = true;
                            m_pendingMappings.Add(key);
                        }
                    }

                    if (mappingRequested)
                    {
                        // No mapping is defined for this point, queue up mapping
                        lock (m_mapRequestQueue.SyncRoot)
                            m_mapRequestQueue.Add(key);
                    }
                }
                catch (Exception ex)
                {
                    OnProcessException(MessageLevel.Info, new InvalidOperationException($"Failed to associate measurement value with connection point for '{key}': {ex.Message}", ex));
                }
            }
            else
            {
                try
                {
                    // Verify maximum per point archive resolution
                    if (m_lastArchiveTimes is not null && (timestamp - m_lastArchiveTimes.GetOrAdd(key.SignalID, timestamp)).ToSeconds() < MaximumPointResolution)
                        continue;

                    // Apply bad data and bad time flags to status (if defined)
                    // Note that PI AFValueStatus defines Bad as 0 and Good as 192, hence the following logic:
                    bool badData = BadDataValueStatus != AFValueStatus.Good && (measurement.StateFlags & MeasurementStateFlags.BadData) > 0;
                    bool badTime = BadTimeValueStatus != AFValueStatus.Good && (measurement.StateFlags & MeasurementStateFlags.BadTime) > 0;

                    AFValueStatus status = (badData, badTime) switch
                    {
                        (true, true) => BadDataValueStatus | BadTimeValueStatus,
                        (true, false) => BadDataValueStatus,
                        (false, true) => BadTimeValueStatus,
                        _ => AFValueStatus.Good
                    };

                    object value = m_integerValueTypes.Contains(key) ? (int)measurement.AdjustedValue : (float)measurement.AdjustedValue;

                    // Queue up insert operations for parallel processing
                    m_archiveQueues[point.ID % m_archiveQueues.Length].Add
                    (
                        new AFValue(value, new AFTime(new DateTime(timestamp, DateTimeKind.Utc)), null, status)
                        {
                            PIPoint = point
                        }
                    );

                    // Track last point archive time, for downsampling when enabled
                    if (m_lastArchiveTimes is not null)
                        m_lastArchiveTimes[key.SignalID] = timestamp;
                }
                catch (Exception ex)
                {
                    OnProcessException(MessageLevel.Info, new InvalidOperationException($"Failed to collate measurement value into OSI-PI point value collection for '{key}': {ex.Message}", ex));
                }
            }
        }
    }

    private void ProcessStatusWordBitStates(IMeasurement measurement, MeasurementKey[] statusBitKeys)
    {
        if (statusBitKeys is null || statusBitKeys.Length == 0)
            return;

        ushort statusWord = (ushort)measurement.AdjustedValue;
        bool dataIsValid = (statusWord & (ushort)StatusFlags.DataIsValid) == 0;
        bool deviceError = (statusWord & (ushort)StatusFlags.DeviceError) > 0;
        bool syncError = (statusWord & (ushort)StatusFlags.DeviceSynchronizationError) > 0;
        bool dataSorting = (statusWord & (ushort)StatusFlags.DataSortingType) == 0;
        bool compositeQuality = dataIsValid && !deviceError && !syncError && dataSorting;
        bool configChanged = (statusWord & (ushort)StatusFlags.ConfigurationChanged) > 0;
        IMeasurement[] statusBitMeasurements = new IMeasurement[statusBitKeys.Length];

        // Get nominal frequency from last associated configuration frame
        m_nominalFrequencies.TryGetValue(measurement.Key, out LineFrequency nominalFrequency);

        for (int i = 0; i < statusBitKeys.Length; i++)
        {
            MeasurementKey key = statusBitKeys[i];

            m_statusBitMeasurements.TryGetValue(key, out int stateIndex);

            int value = stateIndex switch
            {
                C37118DigitalStateSets.CompositeQual => compositeQuality ? 0 : 1,
                C37118DigitalStateSets.ConfigChange => configChanged ? 1 : 0,
                C37118DigitalStateSets.DataSorting => dataSorting ? 0 : 1,
                C37118DigitalStateSets.DataValid => dataIsValid ? 0 : 1,
                C37118DigitalStateSets.NominalFreq => nominalFrequency == LineFrequency.Hz60 ? 0 : 1,
                C37118DigitalStateSets.PMUError => deviceError ? 1 : 0,
                C37118DigitalStateSets.SyncError => syncError ? 1 : 0,
                C37118DigitalStateSets.Timelock => statusWord & (ushort)StatusFlags.UnlockedTimeMask,
                C37118DigitalStateSets.Trigger => statusWord & (ushort)StatusFlags.TriggerReasonMask,
                _ => 0
            };

            statusBitMeasurements[i] = new Measurement
            {
                Metadata = key.Metadata,
                Timestamp = measurement.Timestamp,
                Value = value
            };
        }

        ProcessMeasurements(statusBitMeasurements);
    }

    private void ProcessQualityWordBitStates(IMeasurement measurement, MeasurementKey[] qualityBitKeys)
    {
        if (qualityBitKeys is null || qualityBitKeys.Length < 2)
            return;

        const uint QualityMask = (uint)TimeQualityFlags.TimeQualityIndicatorCodeMask;
        uint qualityWord = (uint)measurement.AdjustedValue; // Quality bits are in hi-word
        IMeasurement[] qualityBitMeasurements = new IMeasurement[qualityBitKeys.Length - 1];
        int index = 0;

        foreach (MeasurementKey key in qualityBitKeys)
        {
            m_qualityBitMeasurements.TryGetValue(key, out int stateIndex);

            // Connect state digital state is handled by statistics engine calculation event
            // since if connection is lost, we won't receive a quality measurement
            if (stateIndex == C37118DigitalStateSets.ConnectState)
                continue;

            int value = stateIndex switch
            {
                C37118DigitalStateSets.LeapSecond => (int)((qualityWord & ~QualityMask) >> 28),
                C37118DigitalStateSets.TimeQuality => (int)((qualityWord & QualityMask) >> 24),
                _ => 0
            };

            qualityBitMeasurements[index++] = new Measurement
            {
                Metadata = key.Metadata,
                Timestamp = measurement.Timestamp,
                Value = value
            };
        }

        ProcessMeasurements(qualityBitMeasurements);
    }

    private void ProcessDigitalWordBitStates(IMeasurement measurement, MeasurementKey[] digitalBitKeys)
    {
        if (digitalBitKeys is null || digitalBitKeys.Length == 0)
            return;

        ushort digitalWord = (ushort)measurement.AdjustedValue;
        IMeasurement[] digitalBitMeasurements = new IMeasurement[digitalBitKeys.Length];
        int index = 0;

        foreach (MeasurementKey key in digitalBitKeys)
        {
            m_digitalBitMeasurements.TryGetValue(key, out (string, int bit) digital);

            digitalBitMeasurements[index++] = new Measurement
            {
                Metadata = key.Metadata,
                Timestamp = measurement.Timestamp,
                Value = (ushort)(digitalWord & (ushort)(1 << digital.bit)) == 0 ? 0 : 1
            };
        }

        ProcessMeasurements(digitalBitMeasurements);
    }

    private void StatisticsEngine_Calculated(object sender, EventArgs e)
    {
        KeyValuePair<MeasurementKey, MeasurementKey>[] connectionStatistics = m_connectionStatistics.ToArray();
        List<IMeasurement> connectStateMeasurements = [];

        // Make sure we have a local reference to active statistics, refreshing statistics cache when member value is null.
        // Metadata refresh sets this member to null to force a refresh on next statistics calculation cycle in case new
        // statistics have been added to the engine. Statistics only appear in the cache once they have been calculated.
        ReadOnlyDictionary<MeasurementKey, Statistic> activeStatistics = m_activeStatistics ??= StatisticsEngine.Statistics;

        if (activeStatistics is null)
            return;

        // Update connect states when statistics are calculated, typically every ten seconds
        foreach (KeyValuePair<MeasurementKey, MeasurementKey> kvp in connectionStatistics)
        {
            MeasurementKey sourceKey = kvp.Key;
            MeasurementKey statisticKey = kvp.Value;

            if (!m_qualityWordMeasurements.TryGetValue(sourceKey, out MeasurementKey[] bitKeys))
                continue;

            MeasurementKey connectStateKey = bitKeys.FirstOrDefault(bitKey => 
                m_qualityBitMeasurements.TryGetValue(bitKey, out int stateIndex) && stateIndex == C37118DigitalStateSets.ConnectState);

            if (connectStateKey is null)
                continue;

            bool connected = false;

            // Query last connected state from active statistics
            if (activeStatistics.TryGetValue(statisticKey, out Statistic statistic))
                connected = statistic.Value > 0.0D;

            connectStateMeasurements.Add(new Measurement
            {
                Metadata = connectStateKey.Metadata,
                Timestamp = DateTime.UtcNow,
                Value = connected ? 0 : 1
            });
        }

        ProcessMeasurements(connectStateMeasurements.ToArray());
    }

    private void ArchiveAFValues(AFValue[] values)
    {
        Ticks startTime = DateTime.UtcNow.Ticks;

        m_connection.Server.UpdateValues(values, m_updateOption);

        Interlocked.Add(ref m_totalProcessingTime, DateTime.UtcNow.Ticks - startTime);
        Interlocked.Add(ref m_processedMeasurements, values.Length);
    }

    private void EstablishPIPointMappings(MeasurementKey[] keys)
    {
        bool mappingEstablished = false;

        foreach (MeasurementKey key in keys)
        {
            // If adapter gets disabled while executing this thread - go ahead and exit
            if (!Enabled)
                return;

            PIPoint point;

            try
            {
                mappingEstablished = false;

                if (m_mappedPIPoints.TryGetValue(key, out point))
                {
                    if (point is null)
                    {
                        // Create new connection point
                        point = CreateMappedPIPoint(key);

                        if (point is not null)
                        {
                            m_mappedPIPoints[key] = point;
                            mappingEstablished = true;
                        }
                    }
                    else
                    {
                        // Connection point is already established
                        mappingEstablished = true;
                    }
                }
            }
            catch (Exception ex)
            {
                OnProcessException(MessageLevel.Info, new InvalidOperationException($"Failed to create connection point mapping for '{key}': {ex.Message}", ex));
            }
            finally
            {
                lock (m_pendingMappings)
                    m_pendingMappings.Remove(key);

                if (!mappingEstablished)
                    m_mappedPIPoints.TryRemove(key, out point);

                // Provide some level of feed back on progress of mapping process
                Interlocked.Increment(ref m_processedMappings);

                if (Interlocked.Read(ref m_processedMappings) % 100 == 0)
                    OnStatusMessage(MessageLevel.Info, $"Mapped {m_mappedPIPoints.Count - m_pendingMappings.Count:N0} PI tags to measurements, {1.0D - m_pendingMappings.Count / (double)m_mappedPIPoints.Count:0.00%} complete...");
            }
        }
    }

    private PIPoint CreateMappedPIPoint(MeasurementKey key)
    {
        PIPoint point = null;

        // Map measurement to PI point
        try
        {
            // Two ways to find points here
            // 1. if we are running metadata sync from the adapter, look for the signal ID in the EXDESC field
            // 2. if the pi points are being manually maintained, look for either the point tag or alternate tag in the actual pi point tag
            Guid signalID = key.SignalID;
            bool foundPoint = false;

            if (RunMetadataSync)
            {
                // Attempt lookup by EXDESC signal ID                           
                point = GetPIPointBySignalID(m_connection.Server, signalID);
                foundPoint = point is not null;
            }

            if (!foundPoint)
            {
                // Lookup meta-data for current measurement
                DataRow[] rows = DataSource.Tables["ActiveMeasurements"].Select($"SignalID='{signalID}'");

                if (rows.Length > 0)
                {
                    // Get tag-name as defined in meta-data, adjusting as needed
                    string tagName = GetPITagName(rows[0]);

                    if (!string.IsNullOrWhiteSpace(tagName))
                    {
                        // Attempt lookup by tag name
                        point = GetPIPoint(m_connection.Server, tagName);

                        if (point is null)
                        {
                            if (!m_refreshingMetadata)
                                OnStatusMessage(MessageLevel.Warning, $"No PI points found for tag '{tagName}'. Data will not be archived for '{key}'.");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            OnProcessException(MessageLevel.Warning, new InvalidOperationException($"Failed to map '{key}' to a PI tag: {ex.Message}", ex));
        }

        // If no point could be mapped, return null connection mapping so key can be removed from tag-map
        return point;
    }

    /// <summary>
    /// Sends updated metadata to PI.
    /// </summary>
    protected override void ExecuteMetadataRefresh()
    {
        if (!Initialized || m_connection is null)
            return;

        if (!m_connection.Connected)
            return;

        int previousMeasurementReportingInterval = MeasurementReportingInterval;
        DateTime latestUpdateTime = DateTime.MinValue;
        m_deviceFirstPhasorLabelCache = [];
        List<Guid> newRecords = [];

        if (ExpandStatusBitsToTags)
            MapStatusBitStateSetsToTags(newRecords);

        if (ExpandQualityBitsToTags)
            MapQualityBitStateSetsToTags(newRecords);

        if (ExpandDigitalBitsToTags && m_digitalBitStateExpressionMap?.Count > 0)
            MapDigitalBitStateSetsToTags(newRecords);

        if (newRecords.Count > 0)
        {
            // Notify host system that configuration has changed
            OnConfigurationChanged();

            OnStatusMessage(MessageLevel.Info, "Waiting for the newly created status and digital bit input measurements to be loaded into active configuration...");

            // Wait for new measurements to be loaded into active measurement cache
            if (!this.WaitForSignalsToLoad(m_configurationReloaded, newRecords))
                OnStatusMessage(MessageLevel.Warning, $"Input measurements not found in active configuration after waiting {MetadataHelpers.ElapsedWaitTimeString()} - new inputs may not be available.");
        }

        MeasurementKey[] inputMeasurementKeys = InputMeasurementKeys;
        SignalType[] inputMeasurementTypes = InputMeasurementKeyTypes;

        // A possible race condition exists in accessing input measurement keys and types where the arrays
        // are not yet synchronized, in this case a metadata refresh will be pending, so we'll just return
        if (inputMeasurementKeys is null || inputMeasurementTypes is null || inputMeasurementKeys.Length != inputMeasurementTypes.Length)
            return;

        // Create a set of all input measurements that are of integer type or are word types that should be excluded
        HashSet<MeasurementKey> integerValueTypes = [];
        HashSet<MeasurementKey> excludedWords = [];

        for (int i = 0; i < inputMeasurementKeys.Length; i++)
        {
            MeasurementKey key = inputMeasurementKeys[i];
            SignalType signalType = inputMeasurementTypes[i];

            if (signalType is SignalType.FLAG or SignalType.QUAL or SignalType.DIGI)
                integerValueTypes.Add(key);
            else if (m_statusBitMeasurements.ContainsKey(key))
                integerValueTypes.Add(key);
            else if (m_qualityBitMeasurements.ContainsKey(key))
                integerValueTypes.Add(key);
            else if (m_digitalBitMeasurements.ContainsKey(key))
                integerValueTypes.Add(key);

            if (signalType == SignalType.FLAG && !WriteStatusWord || 
                signalType == SignalType.QUAL && !WriteQualityWord || 
                signalType == SignalType.DIGI && !WriteDigitalWord)
                excludedWords.Add(key);
        }

        Interlocked.Exchange(ref m_integerValueTypes, integerValueTypes);
        Interlocked.Exchange(ref m_excludedWords, excludedWords);

        m_refreshingMetadata = RunMetadataSync;

        // For first runs, don't report archived points until PI meta-data has been established
        MeasurementReportingInterval = 0;

        // Attempt to load tag-map from existing cache, if any
        LoadCachedTagMap();

        if (m_tagMap.Count > 0)
            MeasurementReportingInterval = previousMeasurementReportingInterval;

        // Establish initial connection point dictionary (much of the meta-data may already exist)
        EstablishPIPointDictionary(inputMeasurementKeys);

        if (!RunMetadataSync)
        {
            MeasurementReportingInterval = previousMeasurementReportingInterval;
            return;
        }

        try
        {
            OnStatusMessage(MessageLevel.Info, "Beginning metadata refresh...");

            if (inputMeasurementKeys.Length > 0)
            {
                PIServer server = m_connection.Server;
                int processed = 0, total = inputMeasurementKeys.Length;
                AdoDataConnection database = null;
                DataTable measurements = DataSource.Tables["ActiveMeasurements"];

                for (int i = 0; i < inputMeasurementKeys.Length; i++)
                {
                    MeasurementKey key = inputMeasurementKeys[i];
                    SignalType signalType = inputMeasurementTypes[i];
                    Guid signalID = key.SignalID;

                    // If adapter gets disabled while executing this thread - go ahead and exit
                    if (!Enabled)
                        return;

                    // If status, quality or digital word is not to be written, skip tag mapping
                    if (m_excludedWords.Contains(key))
                    {
                        m_tagMap.TryRemove(signalID, out _);
                        continue;
                    }

                    // If filtering for specific signal types is enabled, skip tag mapping when signal type is not included
                    if (m_archiveFilterDataTypes is not null && !m_archiveFilterDataTypes.Contains(signalType))
                    {
                        m_tagMap.TryRemove(signalID, out _);
                        continue;
                    }

                    DataRow[] rows = measurements.Select($"SignalID='{signalID}'");

                    string tagName;
                    bool createdNewTag = false;
                    bool refreshMetadata = false;

                    if (rows.Length == 0)
                    {
                        m_tagMap.TryRemove(signalID, out tagName);
                        continue;
                    }

                    // Get matching measurement row
                    DataRow measurementRow = rows[0];

                    // Get tag-name as defined in meta-data, adjusting as needed
                    tagName = GetPITagName(measurementRow);

                    // If no tag name is defined in measurements there is no need to continue processing
                    if (string.IsNullOrWhiteSpace(tagName))
                    {
                        m_tagMap.TryRemove(signalID, out _);
                        continue;
                    }

                    // Lookup PI point trying signal ID and tag name
                    PIPoint point = GetPIPoint(server, signalID, tagName);

                    if (AutoCreateTags && point is null)
                    {
                        try
                        {
                            bool isStatusBitTag = m_statusBitMeasurements.TryGetValue(key, out int stateIndex);
                            bool isQualityBitTag = !isStatusBitTag && m_qualityBitMeasurements.TryGetValue(key, out stateIndex);
                            bool isDigitalBitTag = m_digitalBitMeasurements.TryGetValue(key, out (string state, int) digital);
                            bool isDigitalType = isStatusBitTag || isQualityBitTag || isDigitalBitTag;
                            bool isWordType = signalType is SignalType.FLAG or SignalType.QUAL or SignalType.DIGI;
                            string digitalSetName = null;

                            Debug.Assert(isWordType != isDigitalType, "Word type and digital type should not be the same");

                            if (isDigitalType)
                            {
                                if (isStatusBitTag || isQualityBitTag)
                                {
                                    if (stateIndex is < 0 or >= C37118DigitalStateSets.Count || !m_c37118MappedDigitalStates[stateIndex])
                                    {
                                        m_tagMap.TryRemove(signalID, out _);
                                        continue;
                                    }

                                    digitalSetName = m_c37118DigitalStates[stateIndex];
                                }
                                else if (isDigitalBitTag)
                                {
                                    digitalSetName = digital.state;
                                }
                            }

                            // Attempt to add point if it doesn't exist
                            Dictionary<string, object> attributes = new()
                            {
                                [PICommonPointAttributes.PointClassName] = PIPointClass,
                                [PICommonPointAttributes.PointType] = isWordType ? PIPointType.Int32 : isDigitalType ? PIPointType.Digital : PIPointType.Float32
                            };

                            // Assign digital set name for digital types
                            if (isDigitalType)
                                attributes[PICommonPointAttributes.DigitalSetName] = digitalSetName;

                            point = server.CreatePIPoint(tagName, attributes);

                            refreshMetadata = true;
                            createdNewTag = true;
                        }
                        catch (Exception ex)
                        {
                            OnProcessException(MessageLevel.Warning, new InvalidOperationException($"Failed to add PI tag '{tagName}' for measurement '{key}': {ex.Message}", ex));
                        }
                    }

                    if (point is not null)
                    {
                        // Update tag-map for faster future loads
                        m_tagMap[signalID] = tagName;

                        try
                        {
                            // Rename tag-name if needed - PI tags are not case-sensitive
                            if (AutoUpdateTags && string.Compare(point.Name, tagName, StringComparison.InvariantCultureIgnoreCase) != 0)
                                point.Name = tagName;

                            // Make sure renamed point gets fully remapped
                            m_mappedPIPoints.TryRemove(key, out PIPoint _);

                            lock (m_mapRequestQueue.SyncRoot)
                                m_mapRequestQueue.Remove(key);

                            lock (m_pendingMappings)
                                m_pendingMappings.Remove(key);
                        }
                        catch (Exception ex)
                        {
                            OnProcessException(MessageLevel.Warning, new InvalidOperationException($"Failed to rename PI tag '{point.Name}' to '{tagName}' for measurement '{key}': {ex.Message}", ex));
                        }

                        if (!refreshMetadata)
                        {
                            // Validate that PI point has a properly defined Guid in the extended description
                            point.LoadAttributes(PICommonPointAttributes.ExtendedDescriptor);
                            string exdesc = point.GetAttribute(PICommonPointAttributes.ExtendedDescriptor) as string;

                            if (string.IsNullOrWhiteSpace(exdesc))
                                refreshMetadata = true;
                            else
                                refreshMetadata = string.Compare(exdesc.Trim(), signalID.ToString(), StringComparison.OrdinalIgnoreCase) != 0;
                        }
                    }

                    // Determine last update time for this meta-data record
                    DateTime updateTime;

                    try
                    {
                        // See if ActiveMeasurements contains updated on column
                        if (measurements.Columns.Contains("UpdatedOn"))
                        {
                            updateTime = Convert.ToDateTime(measurementRow["UpdatedOn"]);
                        }
                        else
                        {
                            // Attempt to look up last update time for record
                            database ??= new AdoDataConnection("systemSettings");

                            updateTime = Convert.ToDateTime(database.Connection.ExecuteScalar($"SELECT UpdatedOn FROM Measurement WHERE SignalID = '{signalID}'"));
                        }
                    }
                    catch
                    {
                        updateTime = DateTime.UtcNow;
                    }

                    // Tracked latest update time in meta-data, this will become last meta-data refresh time
                    if (updateTime > latestUpdateTime)
                        latestUpdateTime = updateTime;

                    if ((refreshMetadata || updateTime > m_lastMetadataRefresh) && point is not null)
                    {
                        try
                        {
                            List<string> updatedAttributes = [];

                            void updateAttribute(string attribute, object newValue, bool onlyUpdateIfEmpty = false)
                            {
                                string oldValue = point.GetAttribute(attribute).ToString();

                                if (string.IsNullOrEmpty(oldValue) || (!onlyUpdateIfEmpty && string.CompareOrdinal(oldValue, newValue.ToString()) != 0))
                                {
                                    point.SetAttribute(attribute, newValue);
                                    updatedAttributes.Add(attribute);
                                }
                            }

                            if (AutoUpdateTags || createdNewTag)
                            {
                                // Load current attributes
                                point.LoadAttributes(
                                    PICommonPointAttributes.PointSource,
                                    PICommonPointAttributes.Descriptor,
                                    PICommonPointAttributes.ExtendedDescriptor,
                                    PICommonPointAttributes.Tag,
                                    PICommonPointAttributes.Compressing,
                                    PICommonPointAttributes.CompressionDeviation,
                                    PICommonPointAttributes.EngineeringUnits);

                                // Update tag meta-data if it has changed
                                updateAttribute(PICommonPointAttributes.PointSource, PIPointSource);

                                if (createdNewTag || UpdateExistingDescriptorState)
                                    updateAttribute(PICommonPointAttributes.Descriptor, measurementRow["Description"].ToString());

                                updateAttribute(PICommonPointAttributes.ExtendedDescriptor, measurementRow["SignalID"].ToString());
                                updateAttribute(PICommonPointAttributes.Tag, tagName);

                                if (createdNewTag || AddTagCompressionState || UpdateExistingTagCompressionState)
                                {
                                    updateAttribute(PICommonPointAttributes.Compressing, UseCompression ? 1 : 0);

                                    if (UseCompression && (AddTagCompressionState || UpdateExistingTagCompressionState))
                                    {
                                        Debug.Assert(signalType.ToString().Equals(measurementRow["SignalType"].ToString(), StringComparison.OrdinalIgnoreCase), "Signal type mismatch between measurement and meta-data");

                                        // Add or update compression deviation if needed
                                        if (!m_compDevDataTypeMap.TryGetValue(measurementRow["SignalType"].ToString(), out double compDev))
                                            compDev = m_defaultCompDev;

                                        updateAttribute(PICommonPointAttributes.CompressionDeviation, compDev, AddTagCompressionState && !UpdateExistingTagCompressionState);
                                    }
                                }

                                // Engineering units is a new field for this view -- handle the case that it's not there
                                if (measurements.Columns.Contains("EngineeringUnits"))
                                    updateAttribute(PICommonPointAttributes.EngineeringUnits, measurementRow["EngineeringUnits"].ToString());
                            }
                            else
                            {
                                // Have to maintain Guid link at a minimum
                                point.LoadAttributes(PICommonPointAttributes.ExtendedDescriptor);
                                updateAttribute(PICommonPointAttributes.ExtendedDescriptor, measurementRow["SignalID"].ToString());
                            }

                            // Save any changes
                            if (updatedAttributes.Count > 0)
                                point.SaveAttributes(updatedAttributes.ToArray());
                        }
                        catch (Exception ex)
                        {
                            OnProcessException(MessageLevel.Warning, new InvalidOperationException($"Failed to update PI tag '{tagName}' metadata from measurement '{key}': {ex.Message}", ex));
                        }
                    }

                    processed++;
                    m_metadataRefreshProgress = processed / (double)total;

                    if (processed % 100 == 0)
                        OnStatusMessage(MessageLevel.Info, $"Updated {processed:N0} PI tags and associated metadata, {m_metadataRefreshProgress:0.00%} complete...");

                    // If mapping for this connection was removed, it may have been because there was no meta-data, so
                    // we re-add key to dictionary with null value, actual mapping will happen dynamically as needed
                    if (!m_mappedPIPoints.ContainsKey(key))
                        m_mappedPIPoints.TryAdd(key, null);
                }

                database?.Dispose();

                if (m_tagMap.Count > 0 && Enabled)
                {
                    // Cache tag-map for faster future PI adapter startup
                    try
                    {
                        OnStatusMessage(MessageLevel.Info, "Caching tag-map for faster future loads...");

                        using (FileStream tagMapCache = File.Create(TagMapCacheFileName))
                            Serialization.Serialize(m_tagMap.ToDictionary(kvp => kvp.Key, kvp => kvp.Value), GSF.SerializationFormat.Binary, tagMapCache);

                        OnStatusMessage(MessageLevel.Info, "Tag-map cached for faster future loads.");
                    }
                    catch (Exception ex)
                    {
                        OnProcessException(MessageLevel.Info, new InvalidOperationException($"Failed to cache tag-map to file '{TagMapCacheFileName}': {ex.Message}", ex));
                    }
                }
            }
            else
            {
                OnStatusMessage(MessageLevel.Warning, "PI historian output adapter is not configured to receive any input measurements - metadata refresh canceled.");
            }
        }
        catch (Exception ex)
        {
            OnProcessException(MessageLevel.Warning, ex);
        }
        finally
        {
            m_refreshingMetadata = false;
        }

        bool handlingTagRemoval = AutoRemoveTags != TagRemovalOperation.DoNotRemove;

        if (Enabled)
        {
            m_lastMetadataRefresh = latestUpdateTime > DateTime.MinValue ? latestUpdateTime : DateTime.UtcNow;

            OnStatusMessage(MessageLevel.Info, handlingTagRemoval ?
                "Completed metadata add/update sync operations successfully, starting delete sync operations..." :
                "Completed metadata sync operations successfully.");

            // Re-establish connection point dictionary since meta-data and tags may exist in PI server now that didn't before.
            // This will also start showing warning messages in CreateMappedPIPoint function for unmappable points
            // now that meta-data refresh has completed.
            EstablishPIPointDictionary(inputMeasurementKeys);
        }

        // Restore original measurement reporting interval
        MeasurementReportingInterval = previousMeasurementReportingInterval;

        if (handlingTagRemoval)
            m_handleTagRemoval.RunOnceAsync();
    }

    private void ValidateC37118DigitalStates()
    {
        PIServer server = m_connection.Server;
        PIStateSets stateSets = server.StateSets;

        for (int i = 0; i < C37118DigitalStateSets.Count; i++)
        {
            string state = m_c37118DigitalStates[i].Trim();

            // States marked with 'X' are not mapped
            if (state.Equals("X", StringComparison.OrdinalIgnoreCase))
            {
                m_c37118MappedDigitalStates[i] = false;
                continue;
            }

            m_c37118MappedDigitalStates[i] = true;

            if (stateSets.Contains(state))
                continue;

            // Create digital state set if it doesn't exist
            AFEnumerationSet newSet = new(state);
            string[] elements = C37118DigitalStateSetValues[i];

            for (int j = 0; j < elements.Length; j++)
            {
                string element = elements[j].Trim();
                newSet.Add(new AFEnumerationValue(element, j));
            }

            stateSets.Add(newSet);
        }
    }

    private (Dictionary<MeasurementKey, (string, MeasurementKey[])>, Dictionary<MeasurementKey, int>) MapDigitalStateSetsToTags(List<Guid> newRecords, SignalType targetSignalType, HashSet<int> targetBitStates)
    {
        MeasurementKey[] inputMeasurementKeys = InputMeasurementKeys;
        SignalType[] inputMeasurementTypes = InputMeasurementKeyTypes;

        // A possible race condition exists in accessing input measurement keys and types where the arrays are not
        // yet synchronized, in this case a metadata refresh will be pending, so we'll just return empty mappings
        if (inputMeasurementKeys is null || inputMeasurementTypes is null || inputMeasurementKeys.Length != inputMeasurementTypes.Length)
            return ([], []);

        // When expanding status or quality bits to tags, validate digital state sets exist, creating them if needed
        ValidateC37118DigitalStates();

        // Create status or quality bit measurements for each mapped status digital state set
        Dictionary<Guid, (MeasurementKey sourceKey, string sourceSignalReference, int statusIndex, ulong pointID)> signalIDBitStateMap = [];
        bool recordsAdded = false;

        for (int i = 0; i < inputMeasurementKeys.Length; i++)
        {
            SignalType signalType = inputMeasurementTypes[i];

            if (signalType != targetSignalType)
                continue;

            MeasurementKey key = inputMeasurementKeys[i];
            (string deviceAcronym, int deviceID) = this.LookupDevice(key.SignalID);
            string sourceSignalReference = this.LookupSignalReference(key.SignalID);

            for (int j = 0; j < C37118DigitalStateSets.Count; j++)
            {
                if (!m_c37118MappedDigitalStates[j] || !targetBitStates.Contains(j))
                    continue;

                string state = m_c37118DigitalStates[j].Trim();
                string signalReference = SignalReference.ToString(sourceSignalReference, SignalKind.Calculation, j + 1);
                ulong pointID = ulong.MaxValue;

                // Check if measurement already exists in active configuration, create it if it does not
                if (!this.SignalReferenceExists(signalReference, out Guid signalID))
                {
                    OnStatusMessage(MessageLevel.Info, $"Creating {(targetSignalType == SignalType.FLAG ? "status" : "quality")} bit input measurement \"{signalReference}\" for digital state '{state}'...");

                    TagGenerator tagGenerator = m_c37118TagNameGenerators[j];

                    // Create point tag for digital state based on configured tag name generator
                    string pointTag = CreatePointTag(tagGenerator, deviceAcronym, state, j + 1);

                    // Create new measurement record for digital bit
                    MeasurementRecord record = this.GetMeasurementRecord(deviceID, pointTag, null, signalReference, $"{deviceAcronym}: {state}");
                    signalID = record.SignalID;
                    pointID = (ulong)record.PointID;
                    recordsAdded = true;
                }

                signalIDBitStateMap[signalID] = (key, sourceSignalReference, j, pointID);
            }
        }

        if (recordsAdded)
            newRecords.AddRange(signalIDBitStateMap.Keys);

        Dictionary<MeasurementKey, (string, MeasurementKey[])> wordMeasurementSet = [];
        Dictionary<MeasurementKey, int> bitMeasurements = [];
        int index = 0;

        foreach (KeyValuePair<Guid, (MeasurementKey, string, int, ulong)> kvp in signalIDBitStateMap)
        {
            Guid signalID = kvp.Key;
            (MeasurementKey sourceKey, string sourceSignalReference, int stateIndex, ulong pointID) = kvp.Value;
            MeasurementKey statusBitKey = this.LookupMeasurementKey(signalID, pointID);

            if (statusBitKey != MeasurementKey.Undefined)
                bitMeasurements[statusBitKey] = stateIndex;

            (_, MeasurementKey[] bitKeys) = wordMeasurementSet.GetOrAdd(sourceKey, _ => (sourceSignalReference, new MeasurementKey[targetBitStates.Count]));
            bitKeys[index++] = statusBitKey;
        }

        return (wordMeasurementSet, bitMeasurements);
    }

    private void MapStatusBitStateSetsToTags(List<Guid> newRecords)
    {
        (Dictionary<MeasurementKey, (string, MeasurementKey[])> statusWordMeasurementSet, Dictionary<MeasurementKey, int> statusBitMeasurements) = 
            MapDigitalStateSetsToTags(newRecords, SignalType.FLAG, C37118DigitalStateSets.StatusBitStates);

        if (statusWordMeasurementSet.Count == 0 && statusBitMeasurements.Count == 0)
            return;

        Dictionary<MeasurementKey, MeasurementKey[]> statusWordMeasurements = [];
        Dictionary<MeasurementKey, LineFrequency> nominalFrequencies = [];

        foreach (KeyValuePair<MeasurementKey, (string, MeasurementKey[])> kvp in statusWordMeasurementSet)
        {
            MeasurementKey key = kvp.Key;
            (string sourceSignalReference, MeasurementKey[] bitKeys) = kvp.Value;

            // Map status word measurement to associated status bit measurements
            statusWordMeasurements[key] = bitKeys;

            // Load nominal frequency from last associated configuration frame
            nominalFrequencies[key] = LoadNominalFrequency(sourceSignalReference);
        }

        Interlocked.Exchange(ref m_statusWordMeasurements, statusWordMeasurements);
        Interlocked.Exchange(ref m_statusBitMeasurements, statusBitMeasurements);
        Interlocked.Exchange(ref m_nominalFrequencies, nominalFrequencies);
    }

    private void MapQualityBitStateSetsToTags(List<Guid> newRecords)
    {
        (Dictionary<MeasurementKey, (string, MeasurementKey[])> qualityWordMeasurementSet, Dictionary<MeasurementKey, int> qualityBitMeasurements) = 
            MapDigitalStateSetsToTags(newRecords, SignalType.QUAL, C37118DigitalStateSets.QualityBitStates);

        if (qualityWordMeasurementSet.Count == 0 && qualityBitMeasurements.Count == 0)
            return;

        // Get the input stream connected state statistic index
        DataRow[] rows = DataSource.Tables["Statistics"]?.Select("Source='InputStream' AND IsConnectedState > 0") ?? [];
        int connectedStatisticIndex = rows.Length > 0 ? rows[0].Field<int>("SignalIndex") : 8;
        DataTable measurements = DataSource.Tables["ActiveMeasurements"];
        
        Dictionary<MeasurementKey, MeasurementKey[]> qualityWordMeasurements = [];
        Dictionary<MeasurementKey, MeasurementKey> connectionStatistics = [];

        foreach (KeyValuePair<MeasurementKey, (string, MeasurementKey[])> kvp in qualityWordMeasurementSet)
        {
            MeasurementKey key = kvp.Key;
            (string sourceSignalReference, MeasurementKey[] bitKeys) = kvp.Value;

            // Map quality word measurement to associated quality bit measurements
            qualityWordMeasurements[key] = bitKeys;

            // Quality flags are always associated with parent connection, so acronym is expected to match input stream statistic source
            string deviceAcronym = new SignalReference(sourceSignalReference).Acronym;

            // Lookup associated connection status statistic - this may only exist for phasor protocol connections
            string connectedStatistic = SignalReference.ToString($"{deviceAcronym}!IS", SignalKind.Statistic, connectedStatisticIndex);
            rows = measurements.Select($"SignalReference='{connectedStatistic}'");

            if (rows.Length == 0)
                continue;

            connectionStatistics[key] = MeasurementKey.LookUpOrCreate(rows[0].Field<string>("ID"));
        }

        Interlocked.Exchange(ref m_qualityWordMeasurements, qualityWordMeasurements);
        Interlocked.Exchange(ref m_qualityBitMeasurements, qualityBitMeasurements);
        Interlocked.Exchange(ref m_connectionStatistics, connectionStatistics);

        // Reset active statistics cache
        Interlocked.Exchange(ref m_activeStatistics, null);
    }

    private void MapDigitalBitStateSetsToTags(List<Guid> newRecords)
    {
        MeasurementKey[] inputMeasurementKeys = InputMeasurementKeys;
        SignalType[] inputMeasurementTypes = InputMeasurementKeyTypes;

        // A possible race condition exists in accessing input measurement keys and types where the arrays
        // are not yet synchronized, in this case a metadata refresh will be pending, so we'll just return
        if (inputMeasurementKeys is null || inputMeasurementTypes is null || inputMeasurementKeys.Length != inputMeasurementTypes.Length)
            return;

        HashSet<string> validDigitalStateNames = new(StringComparer.OrdinalIgnoreCase);
        PIServer server = m_connection.Server;
        PIStateSets stateSets = server.StateSets;
        DataTable measurements = DataSource.Tables["ActiveMeasurements"];

        foreach (string state in m_digitalBitStateExpressionMap.Keys)
        {
            // Validate digital state sets exists, has two elements, and first element is zero and second element is one
            if (stateSets.Contains(state))
            {
                AFEnumerationSet stateSet = stateSets[state];

                if (stateSet.Count != 2)
                {
                    OnStatusMessage(MessageLevel.Warning, $"Digital state set '{state}' does not define exactly two elements, digital bit expansion will be skipped for this state.");
                    continue;
                }

                if (stateSet[0].Value != 0)
                {
                    OnStatusMessage(MessageLevel.Warning, $"Digital state set '{state}' does not define first element value as zero, digital bit expansion will be skipped for this state.");
                    continue;
                }

                if (stateSet[1].Value != 1)
                {
                    OnStatusMessage(MessageLevel.Warning, $"Digital state set '{state}' does not define second element value as one, digital bit expansion will be skipped for this state.");
                    continue;
                }

                validDigitalStateNames.Add(state);
            }
            else
            {
                OnStatusMessage(MessageLevel.Warning, $"Digital state set '{state}' not found in PI server, digital bit expansion will be skipped for this state.");
            }
        }

        if (validDigitalStateNames.Count == 0)
        {
            OnStatusMessage(MessageLevel.Error, "None of the configured digital state sets were valid and/or found in PI server, no digital bit expansions will occur.");
        }
        else
        {
            // The goal here is to create a map of digital measurements to digital states. In order to determine which digital states are
            // associated with each digital bit, the user defines a regular expression for each digital state that is used to match the
            // digital label defined in the IEEE C37.118 configuration. Certain naming conventions are expected to have a pattern that can
            // be used to determine which digital state is associated with each bit. For example, a digital label of "BREAKER_101" might
            // have a PI digital state of "Breaker" with elements "Open" and "Closed" and be at bit 3 (of 0-15) in the 16-bit digital word.
            (string state, Regex expression)[] stateExpressions = m_digitalBitStateExpressionMap.Where(item => !item.Value.ToString().Equals("*") && validDigitalStateNames.Contains(item.Key)).Select(item => (item.Key, item.Value)).ToArray();
            string defaultState = m_digitalBitStateExpressionMap.Where(item => item.Value.ToString().Equals("*") && validDigitalStateNames.Contains(item.Key)).Select(item => item.Key).FirstOrDefault();
            Dictionary<MeasurementKey, (string[] digitalLabels, (string state, int bit)[])> digitalWordLabelBitStateMap = []; 

            // Create map of digital states to be used for tag expansion
            for (int i = 0; i < inputMeasurementKeys.Length; i++)
            {
                SignalType signalType = inputMeasurementTypes[i];

                if (signalType != SignalType.DIGI)
                    continue;

                MeasurementKey key = inputMeasurementKeys[i];
                Guid signalID = key.SignalID;
                DataRow[] rows = measurements.Select($"SignalID='{signalID}'");

                if (rows.Length == 0)
                    continue;

                // Get matching measurement row
                DataRow measurementRow = rows[0];

                // Access digital label from AlternateTag field in meta-data
                string digitalLabel = measurementRow["AlternateTag"].ToNonNullString().Trim();

                if (string.IsNullOrWhiteSpace(digitalLabel))
                    continue;

                string[] digitalLabels = new string[16];

                // Split digital label into 16 separate labels, one for each bit
                // For IEEE C37.118.2-2011 (or earlier) Config Frame 2, each 16 characters represent each label
                // For IEEE C37.118.2-2011 (or later) Config Frame 3, labels are variable length and separated with a pipe-symbol
                // If pipe symbol exists, assume Config Frame 3
                string[] parts = digitalLabel.Split('|');

                if (parts.Length == 1)
                {
                    // Fixed length digital labels for config frame 2
                    for (int j = 0; j < digitalLabel.Length && j < 16*16; j += 16)
                        digitalLabels[j] = digitalLabel.Substring(j, 16).PadRight(16);
                }
                else
                {
                    // Variable length digital labels for config frame 3
                    for (int j = 0; j < Math.Min(parts.Length, 16); j++)
                        digitalLabels[j] = parts[j].Trim();
                }

                // Attempt to match digital labels to a digital state
                List<(string state, int bit)> mappedDigitalStates = [];

                for (int j = 0; j < 16; j++)
                {
                    if (string.IsNullOrWhiteSpace(digitalLabels[j]))
                        continue;

                    // Skip digital labels that match excluded expressions, e.g., SPARE or RESERVED
                    if (m_digitalBitExcludedExpressions.Any(expression => expression.IsMatch(digitalLabels[j])))
                        continue;

                    bool matched = false;

                    foreach ((string state, Regex expression) in stateExpressions)
                    {
                        if (expression.IsMatch(digitalLabels[j]))
                        {
                            mappedDigitalStates.Add((state, j));
                            matched = true;
                            break;
                        }
                    }

                    if (!matched && !string.IsNullOrWhiteSpace(defaultState))
                        mappedDigitalStates.Add((defaultState, j));
                }

                if (mappedDigitalStates.Count == 0)
                    continue;
                    
                digitalWordLabelBitStateMap[key] = (digitalLabels, mappedDigitalStates.ToArray());
            }

            // Create digital bit measurements for each digital state / bit / point ID
            Dictionary<Guid, (MeasurementKey sourceKey, string state, int bit, ulong pointID)> signalIDBitStateMap = [];
            bool recordsAdded = false;

            foreach (KeyValuePair<MeasurementKey, (string[] digitalLabels, (string state, int bit)[])> kvp in digitalWordLabelBitStateMap)
            {
                MeasurementKey key = kvp.Key;
                (string[] digitalLabels, (string state, int bit)[] sets) = kvp.Value;
                (string deviceAcronym, int deviceID) = this.LookupDevice(key.SignalID);
                string sourceSignalReference = this.LookupSignalReference(key.SignalID);

                foreach ((string state, int bit) in sets)
                {
                    if (!m_digitalBitTagNameGeneratorMap.TryGetValue(state, out TagGenerator tagGenerator))
                    {
                        OnStatusMessage(MessageLevel.Warning, $"No tag name generator defined for digital state '{state}', skipping creation for '{key}' digital bit {bit}.");
                        continue;
                    }

                    if (bit is < 0 or > 15)
                    {
                        OnStatusMessage(MessageLevel.Warning, $"Digital bit index {bit} is out of range for digital state '{state}', skipping creation for '{key}'.");
                        continue;
                    }

                    string signalReference = SignalReference.ToString(sourceSignalReference, SignalKind.Calculation, bit + 1);
                    ulong pointID = ulong.MaxValue;

                    // Check if measurement already exists in active configuration, create it if it does not
                    if (!this.SignalReferenceExists(signalReference, out Guid signalID))
                    {
                        OnStatusMessage(MessageLevel.Info, $"Creating digital bit {bit} input measurement \"{signalReference}\" for digital state '{state}'...");

                        string label = digitalLabels[bit].ToUpperInvariant().Trim();

                        // Create point tag for digital bit based on configured tag name generator
                        string pointTag = CreatePointTag(tagGenerator, deviceAcronym, label, bit + 1);

                        // Create new measurement record for digital bit
                        MeasurementRecord record = this.GetMeasurementRecord(deviceID, pointTag, null, signalReference, $"{deviceAcronym}: {label} {state}");
                        signalID = record.SignalID;
                        pointID = (ulong)record.PointID;
                        recordsAdded = true;
                    }

                    signalIDBitStateMap[signalID] = (key, state, bit, pointID);
                }
            }

            if (recordsAdded)
                newRecords.AddRange(signalIDBitStateMap.Keys);

            Dictionary<MeasurementKey, MeasurementKey[]> digitalWordMeasurements = [];
            Dictionary<MeasurementKey, (string state, int bit)> digitalBitMeasurements = [];

            foreach (KeyValuePair<Guid, (MeasurementKey, string, int, ulong)> kvp in signalIDBitStateMap)
            {
                Guid signalID = kvp.Key;
                (MeasurementKey sourceKey, string state, int bit, ulong pointID) = kvp.Value;
                MeasurementKey digitalBitKey = this.LookupMeasurementKey(signalID, pointID);

                // Map digital bit measurements to their associated digital states / bits
                if (digitalBitKey != MeasurementKey.Undefined)
                    digitalBitMeasurements[digitalBitKey] = (state, bit);

                // Map digital word measurement to associated digital bit measurements
                digitalWordMeasurements.GetOrAdd(sourceKey, _ => new MeasurementKey[16])[bit] = digitalBitKey;
            }

            Interlocked.Exchange(ref m_digitalWordMeasurements, digitalWordMeasurements);
            Interlocked.Exchange(ref m_digitalBitMeasurements, digitalBitMeasurements);
        }
    }

    private string CreatePointTag(TagGenerator tagNameGenerator, string deviceAcronym, string label, int signalIndex)
    {
        // Validate key acronyms
        deviceAcronym ??= "";
        label ??= "";

        deviceAcronym = deviceAcronym.ToUpperInvariant().Trim();

        // Attempt to lookup first phasor label associated with this device for better base KV guess
        string firstPhasorLabel = m_deviceFirstPhasorLabelCache.GetOrAdd(deviceAcronym, _ =>
        {
            string firstPhasorSignalReference = SignalReference.ToString(deviceAcronym, SignalKind.Magnitude, 1);
            return this.SignalReferenceExists(firstPhasorSignalReference, out Guid signalID) ? this.LookupPointTag(signalID) : "";
        });

        // Define fixed parameter replacements
        Dictionary<string, string> substitutions = new(StringComparer.OrdinalIgnoreCase)
        {
            { "{CompanyAcronym}", s_companyAcronym },
            { "{DeviceAcronym}", deviceAcronym },
            { "{Label}", label },
            { "{SignalIndex}", signalIndex.ToString() },
            { "{BaseKV}", GuessBaseKV(firstPhasorLabel, deviceAcronym) },
            { "{VendorAcronym}", "" },
            { "{PhasorLabel}", "" },
            { "{Phase}", "_" }
        };

        // Define CALC signal type field value replacements
        foreach (KeyValuePair<string, string> field in s_calcSignalTypeFields)
            substitutions.Add(field.Key, field.Value);

        return tagNameGenerator.Execute(substitutions);
    }

    private LineFrequency LoadNominalFrequency(string signalReference)
    {
        // Fall back on default configured nominal frequency if lookup fails
        LineFrequency frequency = s_nominalFrequency;

        string parentAcronym = GetParentDeviceAcronym(signalReference);
        string deviceAcronym = new SignalReference(signalReference).Acronym;

        try
        {
            // Attempt to load last nominal frequency from cached configuration frame
            IConfigurationFrame configFrame = ConfigurationFrame.GetCachedConfiguration(parentAcronym, true);
            IConfigurationCell configCell = configFrame?.Cells.FirstOrDefault(cell => cell.StationName.Equals(deviceAcronym, StringComparison.OrdinalIgnoreCase));

            if (configCell is null)
            {
                // Commonly, all devices will have the same nominal frequency, so fall back
                // on first cell if specific device name is not found in configuration
                if (configFrame?.Cells.Count > 0)
                    frequency = configFrame.Cells[0].NominalFrequency;
            }
            else
            {
                frequency = configCell.NominalFrequency;
            }
        }
        catch (Exception ex)
        {
            Logger.SwallowException(ex, $"{nameof(PIOutputAdapter)} failed to load cached configuration for '{parentAcronym}'.");
        }

        return frequency;
    }

    private string GetParentDeviceAcronym(string signalReference)
    {
        DataTable measurements = DataSource.Tables["ActiveMeasurements"];
        DataRow[] rows = measurements.Select($"SignalReference='{signalReference}'");

        if (rows.Length > 0)
        {
            // DeviceID in ActiveMeasurements is a runtime parent ID for PDC devices
            int? deviceID = rows[0].ConvertField<int?>("DeviceID");

            if (deviceID.HasValue)
            {
                // Lookup device acronym from input adapters with matching runtime ID
                rows = DataSource.Tables["InputAdapters"]?.Select($"ID={deviceID}") ?? [];

                // Get adapter name, i.e., device acronym, if this is a phasor protocol adapter
                if (rows.Length > 0 && rows[0].ConvertField<string>("TypeName").Equals("PhasorProtocolAdapters.PhasorMeasurementMapper"))
                    return rows[0].ConvertField<string>("AdapterName");
            }
        }

        // Fall back on assumption that this is a direct PMU connection
        return new SignalReference(signalReference).Acronym;
    }

    private void HandleTagRemoval()
    {
        if (!Enabled)
            return;

        // Create hash set of all local measurement Guids for quick lookup
        HashSet<Guid> activeMeasurements =
        [
            ..DataSource.Tables["ActiveMeasurements"].AsEnumerable()
                .Select(row => Guid.TryParse(row["SignalID"].ToString(), out Guid signalID) ? signalID : Guid.Empty)
                .Where(guid => guid != Guid.Empty)
        ];

        PIServer server = m_connection.Server;
        string sourceFilter = AutoRemoveTags == TagRemovalOperation.LocalOnly ? PIPointSource : null;
        IEnumerable<PIPoint> points = PIPoint.FindPIPoints(server, "*", sourceFilter, [PICommonPointAttributes.Tag, PICommonPointAttributes.ExtendedDescriptor]);
        List<string> pointsToDelete = [];
        double total = server.GetPointCount();
        long processed = 0L;
        long descriptorIssues = 0L;
        Ticks startTime = DateTime.UtcNow.Ticks;

        foreach (PIPoint point in points)
        {
            // If adapter gets disabled while executing this thread - go ahead and exit
            if (!Enabled)
                return;

            // Get point's Guid-based signal ID from extended descriptor
            string exdesc = point.GetAttribute(PICommonPointAttributes.ExtendedDescriptor) as string;
            bool deletePointID;

            if (string.IsNullOrWhiteSpace(exdesc))
            {
                // Tags without extended descriptor considered extraneous during full clone
                deletePointID = AutoRemoveTags == TagRemovalOperation.FullClone;
                descriptorIssues++;
            }
            else
            {
                if (Guid.TryParse(exdesc.Trim(), out Guid signalID))
                {
                    // Point ID to be deleted if it's not in local active measurement set
                    deletePointID = !activeMeasurements.Contains(signalID);
                }
                else
                {
                    // Tags with non-Guid extended descriptor considered extraneous during full clone
                    deletePointID = AutoRemoveTags == TagRemovalOperation.FullClone;
                    descriptorIssues++;
                }
            }

            if (deletePointID)
                pointsToDelete.Add(point.Name);

            if (++processed % 200 == 0)
                OnStatusMessage(MessageLevel.Info, $"Delete sync operation has scanned {processed:N0} PI tags, {processed / total:0.00%} complete...");
        }

        OnStatusMessage(MessageLevel.Info, $"Delete sync operation scan completed in {(DateTime.UtcNow.Ticks - startTime).ToElapsedTimeString(2)}. There were {descriptorIssues:N0} tags with signal ID extended descriptor issues.");

        if (pointsToDelete.Count > 0)
        {
            OnStatusMessage(MessageLevel.Info, $"Delete sync operation found {pointsToDelete.Count:N0} PI tags to be removed, attempting deletion...");

            AFErrors<string> result = server.DeletePIPoints(pointsToDelete);

            if (result?.HasErrors ?? false)
            {
                int errors = result.Errors.Count;
                OnStatusMessage(MessageLevel.Info, $"Delete sync operations completed, {pointsToDelete.Count:N0} PI tag deletions attempted with {errors:N0} errors.");
                OnProcessException(MessageLevel.Warning, new AggregateException(result.Errors.Values), $"{nameof(PIOutputAdapter)} delete sync operation errors");
            }
            else
            {
                OnStatusMessage(MessageLevel.Info, $"Delete sync operations completed, {pointsToDelete.Count:N0} PI tags were deleted.");
            }
        }
        else
        {
            OnStatusMessage(MessageLevel.Info, "Delete sync operations completed, no PI tags were deleted.");
        }
    }

    // Resets the PI tag to MeasurementKey mapping for loading data into PI by finding PI points that match either the GSFSchema point tag or alternate tag
    private void EstablishPIPointDictionary(MeasurementKey[] inputMeasurements)
    {
        OnStatusMessage(MessageLevel.Info, "Establishing connection points for mapping...");

        List<MeasurementKey> newTags = [];

        if (inputMeasurements?.Length > 0)
        {
            foreach (MeasurementKey key in inputMeasurements)
            {
                // Add key to dictionary with null value if not defined, actual mapping will happen dynamically as needed
                if (!m_mappedPIPoints.ContainsKey(key))
                    m_mappedPIPoints.TryAdd(key, null);

                newTags.Add(key);
            }
        }

        if (newTags.Count > 0)
        {
            // Determine which tags no longer exist
            HashSet<MeasurementKey> tagsToRemove = [.. m_mappedPIPoints.Keys];

            // If there are existing tags that are not part of new updates, these need to be removed
            tagsToRemove.ExceptWith(newTags);

            if (tagsToRemove.Count > 0)
            {
                foreach (MeasurementKey key in tagsToRemove)
                    m_mappedPIPoints.TryRemove(key, out PIPoint _);

                OnStatusMessage(MessageLevel.Info, $"Detected {tagsToRemove.Count:N0} tags that have been removed from OSI-PI output - primary tag-map has been updated...");
            }

            if (m_mappedPIPoints.Count == 0)
                OnStatusMessage(MessageLevel.Warning, "No PI tags were mapped to measurements - no tag-map exists so no points will be archived.");
        }
        else
        {
            OnStatusMessage(MessageLevel.Warning, "No PI tags were mapped to measurements - " +
            (
              m_mappedPIPoints.Count > 0 ?
                  $"existing tag-map with {m_mappedPIPoints.Count:N0} tags remains in use." :
                  "no tag-map exists so no points will be archived.")
            );
        }
    }

    // Map measurement Guids to any existing point tags for faster PI point name resolution
    private void LoadCachedTagMap()
    {
        if (!File.Exists(TagMapCacheFileName))
            return;

        try
        {
            // Attempt to load tag-map from existing cache file
            using FileStream tagMapCache = File.Open(TagMapCacheFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            m_tagMap.Merge(true, Serialization.Deserialize<Dictionary<Guid, string>>(tagMapCache, GSF.SerializationFormat.Binary));

            OnStatusMessage(MessageLevel.Info, $"Loaded {m_tagMap.Count:N0} mappings from tag-map cache.");

            // Use last write time of file as the last meta-data refresh time - rough value is OK
            if (m_lastMetadataRefresh == DateTime.MinValue)
                m_lastMetadataRefresh = File.GetLastWriteTimeUtc(TagMapCacheFileName);
        }
        catch (Exception ex)
        {
            OnProcessException(MessageLevel.Info, new InvalidOperationException($"Failed to load mappings from cached tag-map to file '{TagMapCacheFileName}': {ex.Message}", ex));
        }
    }

    private static PIPoint GetPIPoint(PIServer server, string tagName)
    {
        if (server is null || string.IsNullOrWhiteSpace(tagName))
            return null;

        PIPoint.TryFindPIPoint(server, tagName, out PIPoint point);
        return point;
    }

    private PIPoint GetPIPoint(PIServer server, Guid signalID, string tagName)
    {
        PIPoint point = GetPIPointBySignalID(server, signalID, out string cachedTagName);

        // If point was not found in cache and cached tag name does not match current tag name, attempt to lookup using current tag name
        if (point is null && string.Compare(cachedTagName, tagName, StringComparison.OrdinalIgnoreCase) != 0)
            point = GetPIPoint(server, tagName);

        return point;
    }

    private PIPoint GetPIPointBySignalID(PIServer server, Guid signalID) =>
        GetPIPointBySignalID(server, signalID, out string _);

    private PIPoint GetPIPointBySignalID(PIServer server, Guid signalID, out string cachedTagName)
    {
        PIPoint point = null;

        // See if cached mapping exists between guid and tag name - tag name lookups are faster than GetPoints query
        if (m_tagMap.TryGetValue(signalID, out cachedTagName))
            point = GetPIPoint(server, cachedTagName);

        if (point is null)
        {
            // Point was not previously cached, lookup tag using signal ID stored in extended description field
            IEnumerable<PIPoint> points = PIPoint.FindPIPoints(server, $"EXDESC:='{signalID}'", false);

            point = points.FirstOrDefault();

            if (point is not null)
                cachedTagName = point.Name;
        }

        return point;
    }

    private string RemoveTagNamePrefixes(string tagName)
    {
        if (TagNamePrefixRemoveCount < 1)
            return tagName;

        for (int i = 0; i < TagNamePrefixRemoveCount; i++)
        {
            int prefixIndex = tagName.IndexOf('!');

            if (prefixIndex > -1 && prefixIndex + 1 < tagName.Length)
                tagName = tagName.Substring(prefixIndex + 1);
            else
                break;
        }

        return tagName;
    }

    private string GetPITagName(DataRow measurementRow)
    {
        string tagName = null;

        if (!SyncAlternateTagOnly)
            tagName = RemoveTagNamePrefixes(measurementRow["PointTag"].ToNonNullString().Trim());

        string signalType = measurementRow["SignalType"].ToString();
        bool skipDigital = signalType.Equals("DIGI", StringComparison.OrdinalIgnoreCase) && SkipDigitalAlternateTagSync;
        bool skipAnalog = signalType.Equals("ALOG", StringComparison.OrdinalIgnoreCase) && SkipAnalogAlternateTagSync;

        // Use alternate tag if one is defined - note that digitals and analogs are an exception since they use this field for special labeling
        if (!string.IsNullOrWhiteSpace(measurementRow["AlternateTag"].ToString()) && !(skipDigital || skipAnalog))
            tagName = measurementRow["AlternateTag"].ToString().Trim();

        return tagName;
    }

    // Since we may get a plethora of these requests, we use a synchronized operation to restart once
    private void Connection_Disconnected(object sender, EventArgs e) =>
        m_restartConnection.RunOnceAsync();

    #endregion

    #region [ Static ]

    // Static Fields
    private static Dictionary<string, string> s_calcSignalTypeFields;
    private static readonly string s_companyAcronym;
    private static readonly LineFrequency s_nominalFrequency;
    private static readonly string[] s_commonVoltageLevels = ["44", "69", "115", "138", "161", "169", "230", "345", "500", "765", "1100"];

    /// <summary>
    /// Accesses local output adapter instances (normally only one).
    /// </summary>
    public static readonly ConcurrentDictionary<string, PIOutputAdapter> Instances = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Predefined digital state set AF enumeration values for IEEE C37.118 connection states, quality and status word bits,
    /// see <see cref="C37118DigitalStateSets"/>.
    /// </summary>
    public static readonly ReadOnlyCollection<string[]> C37118DigitalStateSetValues = new(
    [
        /* 00: C37118_CompositeQual */ ["Good", "Bad Quality"],
        /* 01: C37118_ConfigChange */  ["Normal", "Change Pending"],
        /* 02: C37118_ConnectState */  ["Connected", "Disconnected"],
        /* 03: C37118_DataSorting */   ["By Time", "By Arrival"],
        /* 04: C37118_DataValid */     ["Valid", "Invalid"],
        /* 05: C37118_LeapSecond */    ["Normal", "LeapAddPending", "LeapAddOccurred", "Transient", "Normal", "LeapDeletePending", "LeapDeleteOccurred", "Transient"],
        /* 06: C37118_NominalFreq */   ["60Hz", "50Hz"],
        /* 07: C37118_PMUError */      ["Normal", "PMU Error"],
        /* 08: C37118_SyncError */     ["Normal", "Sync Error"],
        /* 09: C37118_TimeLock */      ["Locked", "Unlocked_10s", "Unlocked_100s", "Unlocked_1000s"],
        /* 10: C37118_TimeQuality */   ["Locked", "MaxError_10E-9s", "MaxError_10E-8s", "MaxError_10E-7s",
                                        "MaxError_10E-6s", "MaxError_10E-5s", "MaxError_10E-4s", "MaxError_10E-3s",
                                        "MaxError_10E-2s", "MaxError_10E-1s", "MaxError_1s", "MaxError_10s", "Clock_Failure"],
        /* 11: C37118_Trigger */       ["Manual", "Magnitude Low", "Magnitude_High", "PhaseAngle_Diff", "Frequency_High_Low", "df/dt_High",
                                        "Reserved", "Digital", "UserDefined_1", "UserDefined_2", "UserDefined_3",
                                        "UserDefined_4", "UserDefined_5", "UserDefined_6", "UserDefined_7", "Normal"]
    ]);

    // Static Constructor
    static PIOutputAdapter()
    {
        try
        {
            CategorizedSettingsElementCollection systemSettings = ConfigurationFile.Current.Settings["systemSettings"];
            s_companyAcronym = systemSettings["CompanyAcronym"]?.Value;

            if (string.IsNullOrWhiteSpace(s_companyAcronym))
                s_companyAcronym = "GPA";

            s_companyAcronym = s_companyAcronym.ToUpperInvariant().Trim();

            if (!Enum.TryParse(systemSettings["NominalFrequency"]?.Value, true, out s_nominalFrequency))
                s_nominalFrequency = LineFrequency.Hz60;
        }
        catch (Exception ex)
        {
            Logger.SwallowException(ex, "Failed to initialize default company acronym");
        }
    }

    // Static Methods
    private static string GuessBaseKV(string phasorLabel, string deviceAcronym)
    {
        // Check phasor label for voltage level as a priority over device acronym for better base KV guess
        foreach (string voltageLevel in s_commonVoltageLevels)
        {
            if (phasorLabel.IndexOf(voltageLevel, StringComparison.Ordinal) > -1)
                return voltageLevel;
        }

        foreach (string voltageLevel in s_commonVoltageLevels)
        {
            if (deviceAcronym.IndexOf(voltageLevel, StringComparison.Ordinal) > -1)
                return voltageLevel;
        }

        return "0";
    }

    #endregion
}