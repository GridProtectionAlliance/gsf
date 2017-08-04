//******************************************************************************************************
//  OutputAdapter.cs - Gbtc
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
//  03/14/2017 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using GSF;
using GSF.Collections;
using GSF.Data;
using GSF.Diagnostics;
using GSF.IO;
using GSF.Threading;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using InStep.eDNA.EzDNAApiNet;
using Timer = System.Timers.Timer;

namespace eDNAAdapters
{
    #region [ Enumerations ]

    /// <summary>
    /// Defines eDNA data types.
    /// </summary>
    public enum DataType
    {
        /// <summary>
        /// Analog data type.
        /// </summary>
        Analog,
        /// <summary>
        /// Digital data type.
        /// </summary>
        Digital
    }

    #endregion

    /// <summary>
    /// Exports measurements to eDNA if the point tag or alternate tag corresponds to a eDNA point's tag name.
    /// </summary>
    [Description("eDNA: Archives measurements to an eDNA server")]
    public class OutputAdapter : OutputAdapterBase
    {
        #region [ Members ]

        // Fields        
        private uint m_connection;                                          // eDNA server connection handle
        private readonly Timer m_connectionMonitor;                         // eDNA server connection monitor
        private readonly object m_connectionOperationLock;                  // Connection operation lock object
        private readonly ConcurrentDictionary<Guid, string> m_pointIDMap;   // Measurement signal ID to point ID cache map
        private readonly ProcessQueue<Guid> m_mapRequestQueue;              // Requested measurement to eDNA point mapping queue
        private readonly LongSynchronizedOperation m_savePointIDMapCache;   // Save point ID map cache operation
        private readonly HashSet<Guid> m_pendingMappings;                   // List of pending measurement mappings
        private Dictionary<Guid, Ticks> m_lastArchiveTimes;                 // Cache of last point archive times
        private DateTime m_lastMetadataRefresh;                             // Tracks time of last meta-data refresh
        private long m_processedMappings;                                   // Total number of mappings processed so far
        private long m_processedMeasurements;                               // Total number of measurements processed so far
        private long m_totalProcessingTime;                                 // Total point processing time 
        private volatile bool m_refreshingMetadata;                         // Flag that determines if meta-data is currently refreshing
        private double m_metadataRefreshProgress;                           // Current meta-data refresh progress
        private Ticks m_lastPointIDMapFlushTime;                            // Last time of point ID map file flush
        private bool m_disposed;                                            // Flag that determines if class is disposed

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="OutputAdapter"/>
        /// </summary>
        public OutputAdapter()
        {
            m_connectionMonitor = new Timer();
            m_connectionMonitor.Enabled = false;
            m_connectionMonitor.AutoReset = true;
            m_connectionMonitor.Elapsed += m_connectionMonitor_Elapsed;
            m_connectionOperationLock = new object();
            m_pointIDMap = new ConcurrentDictionary<Guid, string>();
            m_mapRequestQueue = ProcessQueue<Guid>.CreateRealTimeQueue(EstablishPointIDMappings);
            m_mapRequestQueue.ProcessException += m_mapRequestQueue_ProcessException;
            m_savePointIDMapCache = new LongSynchronizedOperation(SavePointIDMapCache, ex => OnProcessException(MessageLevel.Warning, ex));
            m_savePointIDMapCache.IsBackground = false;
            m_pendingMappings = new HashSet<Guid>();
            m_lastMetadataRefresh = DateTime.MinValue;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Returns true to indicate that this <see cref="OutputAdapter"/> is sending measurements to a historian, i.e., eDNA.
        /// </summary>
        public override bool OutputIsForArchive => true;

        /// <summary>
        /// Returns false to indicate that this <see cref="OutputAdapter"/> will connect synchronously based on eDNA API operation.
        /// </summary>
        protected override bool UseAsyncConnect => false;

        /// <summary>
        /// Gets or sets the name of the eDNA primary server for the adapter's connection.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the name of the eDNA primary server for the adapter's connection.")]
        [DefaultValue(Default.PrimaryServer)]
        public string PrimaryServer { get; set; } = Default.PrimaryServer;

        /// <summary>
        /// Gets or sets the eDNA primary port for the adapter's connection.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the eDNA primary port for the adapter's connection.")]
        [DefaultValue(Default.PrimaryPort)]
        public ushort PrimaryPort { get; set; } = Default.PrimaryPort;

        /// <summary>
        /// Gets or sets the name of the eDNA secondary server for the adapter's connection.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the name of the eDNA secondary server for the adapter's connection.")]
        [DefaultValue(Default.SecondaryServer)]
        public string SecondaryServer { get; set; } = Default.SecondaryServer;

        /// <summary>
        /// Gets or sets the eDNA secondary port for the adapter's connection.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the eDNA secondary port for the adapter's connection.")]
        [DefaultValue(Default.SecondaryPort)]
        public ushort SecondaryPort { get; set; } = Default.SecondaryPort;

        /// <summary>
        /// Gets or sets the eDNA site for the adapter's connection.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the eDNA site for the adapter's connection.")]
        [DefaultValue(Default.Site)]
        public string Site { get; set; } = Default.Site;

        /// <summary>
        /// Gets or sets the eDNA service for the adapter's connection.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the eDNA service for the adapter's connection.")]
        [DefaultValue(Default.Service)]
        public string Service { get; set; } = Default.Service;

        /// <summary>
        /// Gets or sets flag that determines if eDNA client API should acknowledge all process errors or protocol errors.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines flag that determines if eDNA client API should acknowledge all process errors or protocol errors.")]
        [DefaultValue(Default.AcknowledgeDataPackets)]
        public bool AcknowledgeDataPackets { get; set; } = Default.AcknowledgeDataPackets;

        /// <summary>
        /// Gets or sets flag that determines if eDNA client API should enable its memory queue.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines flag that determines if eDNA client API should enable its memory queue.")]
        [DefaultValue(Default.EnableQueuing)]
        public bool EnableQueuing { get; set; } = Default.EnableQueuing;

        /// <summary>
        /// Gets or sets flag that determines if eDNA client API should enable use of a local file system cache.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines flag that determines if eDNA client API should enable use of a local file system cache.")]
        [DefaultValue(Default.EnableCaching)]
        public bool EnableCaching { get; set; } = Default.EnableCaching;

        /// <summary>
        /// Gets or sets the filename to be used for point ID map cache.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the path and filename to be used for the eDNA client API cache. Leave blank for cache name to target host working directory and be same as adapter name with a \"_eDNA.cache\" extension.")]
        [DefaultValue(Default.LocalCacheFileName)]
        public string LocalCacheFileName { get; set; } = Default.LocalCacheFileName;

        /// <summary>
        /// Gets or sets flag that determines if eDNA client API cache should be cleared on startup.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines flag that determines if eDNA client API cache should be cleared on startup.")]
        [DefaultValue(Default.ClearCacheOnStartup)]
        public bool ClearCacheOnStartup { get; set; } = Default.ClearCacheOnStartup;

        /// <summary>
        /// Gets or sets whether or not this adapter should automatically manage meta-data for eDNA points.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Determines if this adapter should automatically manage meta-data for eDNA points (recommended).")]
        [DefaultValue(Default.RunMetadataSync)]
        public bool RunMetadataSync { get; set; } = Default.RunMetadataSync;

        /// <summary>
        /// Gets or sets whether or not this adapter should automatically manage meta-data for eDNA points.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Determines if this adapter should automatically create new tags when managing meta-data for eDNA points (recommended). Value will only be considered when RunMetadataSync is True.")]
        [DefaultValue(Default.AutoCreateTags)]
        public bool AutoCreateTags { get; set; } = Default.AutoCreateTags;

        /// <summary>
        /// Gets or sets whether or not this adapter should automatically manage meta-data for eDNA points.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Determines if this adapter should automatically update existing tags when managing meta-data for eDNA points (recommended). This will make the time-series library host application the master for maintaining eDNA tag meta-data (like the tag name); otherwise, when False, eDNA will be the master. Value will only be considered when RunMetadataSync is True.")]
        [DefaultValue(Default.AutoUpdateTags)]
        public bool AutoUpdateTags { get; set; } = Default.AutoUpdateTags;

        /// <summary>
        /// Gets or sets the number of tag name prefixes, e.g., "SOURCE!", applied by subscriptions to remove from eDNA tag names.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the number of tag name prefixes applied by subscriptions, e.g., \"SOURCE!\", to remove from eDNA tag names. Value will only be considered when RunMetadataSync is True.")]
        [DefaultValue(Default.TagNamePrefixRemoveCount)]
        public int TagNamePrefixRemoveCount { get; set; } = Default.TagNamePrefixRemoveCount;

        /// <summary>
        /// Gets or sets the filename to be used for point ID map cache.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the path and filename to be used for the point ID map cache file name. Leave blank for cache name to target host working directory and to be same as adapter name with a \"_PointIDMap.cache\" extension.")]
        [DefaultValue(Default.PointIDMapCacheFileName)]
        public string PointIDMapCacheFileName { get; set; } = Default.PointIDMapCacheFileName;

        /// <summary>
        /// Gets or sets the default string to represent the digital set state.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the default string to represent the digital set state.")]
        [DefaultValue(Default.DigitalSetString)]
        public string DigitalSetString { get; set; } = Default.DigitalSetString;

        /// <summary>
        /// Gets or sets the default string to represent the digital cleared state.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the default string to represent the digital cleared state.")]
        [DefaultValue(Default.DigitalClearedString)]
        public string DigitalClearedString { get; set; } = Default.DigitalClearedString;

        /// <summary>
        /// Gets or sets flag that determines if eDNA client API INI configuration file should be validated to exist before starting adapter. When set to True, adapter will fail to start when INI file does not exist.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines flag that determines if eDNA client API INI configuration file should be validated to exist before starting adapter. When set to True, adapter will fail to start when INI file does not exist.")]
        [DefaultValue(Default.ValidateINIFileExists)]
        public bool ValidateINIFileExists { get; set; } = Default.ValidateINIFileExists;

        /// <summary>
        /// Gets or sets the maximum time resolution, in seconds, for data points being archived, e.g., a value 1.0 would mean that data would be archived no more than once per second. A zero value indicates that all data should be archived.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the maximum time resolution, in seconds, for data points being archived, e.g., a value 1.0 would mean that data would be archived no more than once per second. A zero value indicates that all data should be archived.")]
        [DefaultValue(Default.MaximumPointResolution)]
        public double MaximumPointResolution { get; set; } = Default.MaximumPointResolution;

        /// <summary>
        /// Gets or sets the interval, in milliseconds, to monitor eDNA connection. A zero value will disable the monitor.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the interval, in milliseconds, to monitor an eDNA connection. A zero value will disable the monitor.")]
        [DefaultValue(Default.ConnectionMonitoringInterval)]
        public int ConnectionMonitoringInterval { get; set; } = Default.ConnectionMonitoringInterval;

        /// <summary>
        /// Gets the detailed status of the eDNA output adapter.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.Append(base.Status);
                status.AppendLine();
                status.AppendFormat("       eDNA primary server: {0}:{1}", PrimaryServer, PrimaryPort);
                status.AppendLine();
                if (!string.IsNullOrWhiteSpace(SecondaryServer))
                {
                    status.AppendFormat("     eDNA secondary server: {0}:{1}", SecondaryServer, SecondaryPort);
                    status.AppendLine();
                }
                status.AppendFormat("                 eDNA site: {0}", Site);
                status.AppendLine();
                status.AppendFormat("              eDNA service: {0}", Service);
                status.AppendLine();
                status.AppendFormat("       Connected to server: {0}", m_connection > 0U ? "No" : $"Yes, handle = {m_connection}");
                status.AppendLine();
                status.AppendFormat("       Monitoring interval: {0:N0}ms", ConnectionMonitoringInterval);
                status.AppendLine();
                status.AppendFormat("  Maximum point resolution: {0:N3} seconds{1}", MaximumPointResolution, MaximumPointResolution <= 0.0D ? " - all data will be archived" : "");
                status.AppendLine();
                status.AppendFormat("Validating INI file exists: {0}", ValidateINIFileExists);
                status.AppendLine();
                status.AppendFormat("Acknowledging data packets: {0}", AcknowledgeDataPackets);
                status.AppendLine();
                status.AppendFormat("    Memory queuing enabled: {0}", EnableQueuing);
                status.AppendLine();
                status.AppendFormat("       Local cache enabled: {0}", EnableCaching);
                status.AppendLine();

                if (EnableCaching)
                {
                    status.AppendFormat(" Clearing cache on startup: {0}", ClearCacheOnStartup);
                    status.AppendLine();
                    status.AppendFormat("          Local cache file: {0}", FilePath.TrimFileName(LocalCacheFileName.ToNonNullString("[undefined]"), 51));
                    status.AppendLine();
                }

                status.AppendFormat("    Meta-data sync enabled: {0}", RunMetadataSync);
                status.AppendLine();

                if (RunMetadataSync)
                {
                    status.AppendFormat("          Auto-create tags: {0}", AutoCreateTags);
                    status.AppendLine();
                    status.AppendFormat("          Auto-update tags: {0}", AutoUpdateTags);
                    status.AppendLine();
                    status.AppendFormat("    Tag prefixes to remove: {0:N0}", TagNamePrefixRemoveCount);
                    status.AppendLine();
                }

                if ((object)m_mapRequestQueue != null)
                {
                    status.AppendLine();
                    status.AppendLine(">> Mapping Request Queue Status");
                    status.AppendLine();
                    status.Append(m_mapRequestQueue.Status);
                    status.AppendLine();
                }

                status.AppendFormat("   Point ID map cache file: {0}", FilePath.TrimFileName(PointIDMapCacheFileName.ToNonNullString("[undefined]"), 51));
                status.AppendLine();
                status.AppendFormat("   Point ID map cache size: {0:N0} mappings", m_pointIDMap.Count);
                status.AppendLine();
                status.AppendFormat(" Pending point ID mappings: {0:N0} mappings, {1:0.00%} complete", m_pendingMappings.Count, 1.0D - m_pendingMappings.Count / (double)m_pointIDMap.Count);
                status.AppendLine();

                if (RunMetadataSync)
                {
                    status.AppendFormat("    Meta-data sync process: {0}, {1:0.00%} complete", m_refreshingMetadata ? "Active" : "Idle", m_metadataRefreshProgress);
                    status.AppendLine();
                }

                status.AppendFormat("    Points archived/second: {0:#,##0.00}", Interlocked.Read(ref m_processedMeasurements) / (Interlocked.Read(ref m_totalProcessingTime) / (double)Ticks.PerSecond));
                status.AppendLine();

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="OutputAdapter"/> object and optionally releases the managed resources.
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
                        m_connectionMonitor.Elapsed -= m_connectionMonitor_Elapsed;
                        m_connectionMonitor.Dispose();

                        if ((object)m_mapRequestQueue != null)
                        {
                            m_mapRequestQueue.ProcessException -= m_mapRequestQueue_ProcessException;
                            m_mapRequestQueue.Dispose();
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
        /// Returns a brief status of this <see cref="OutputAdapter"/>
        /// </summary>
        /// <param name="maxLength">Maximum number of characters in the status string</param>
        /// <returns>Status</returns>
        public override string GetShortStatus(int maxLength)
        {
            return $"Archived {m_processedMeasurements:N0} measurements to eDNA.".CenterText(maxLength);
        }

        /// <summary>
        /// Initializes this <see cref="OutputAdapter"/>.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            Dictionary<string, string> settings = Settings;
            string setting;
            ushort ushortVal;
            int intVal;
            double doubleVal;

            if (settings.TryGetValue(nameof(PrimaryServer), out setting) && !string.IsNullOrWhiteSpace(setting))
                PrimaryServer = setting;

            if (settings.TryGetValue(nameof(PrimaryPort), out setting) && ushort.TryParse(setting, out ushortVal))
                PrimaryPort = ushortVal;

            if (settings.TryGetValue(nameof(SecondaryServer), out setting) && !string.IsNullOrWhiteSpace(setting))
                SecondaryServer = setting;

            if (settings.TryGetValue(nameof(SecondaryPort), out setting) && ushort.TryParse(setting, out ushortVal))
                SecondaryPort = ushortVal;

            if (settings.TryGetValue(nameof(Site), out setting) && !string.IsNullOrWhiteSpace(setting))
                Site = setting;

            if (settings.TryGetValue(nameof(Service), out setting) && !string.IsNullOrWhiteSpace(setting))
                Service = setting;

            if (settings.TryGetValue(nameof(AcknowledgeDataPackets), out setting) && !string.IsNullOrWhiteSpace(setting))
                AcknowledgeDataPackets = setting.ParseBoolean();

            if (settings.TryGetValue(nameof(EnableQueuing), out setting) && !string.IsNullOrWhiteSpace(setting))
                EnableQueuing = setting.ParseBoolean();

            if (settings.TryGetValue(nameof(EnableCaching), out setting) && !string.IsNullOrWhiteSpace(setting))
                EnableCaching = setting.ParseBoolean();

            if (!settings.TryGetValue(nameof(LocalCacheFileName), out setting) || string.IsNullOrWhiteSpace(setting))
                setting = $"{Name}_eDNA.cache";

            LocalCacheFileName = FilePath.GetAbsolutePath(setting);

            if (settings.TryGetValue(nameof(ClearCacheOnStartup), out setting) && !string.IsNullOrWhiteSpace(setting))
                ClearCacheOnStartup = setting.ParseBoolean();

            if (settings.TryGetValue(nameof(RunMetadataSync), out setting) && !string.IsNullOrWhiteSpace(setting))
                RunMetadataSync = setting.ParseBoolean();

            if (settings.TryGetValue(nameof(AutoCreateTags), out setting) && !string.IsNullOrWhiteSpace(setting))
                AutoCreateTags = setting.ParseBoolean();

            if (settings.TryGetValue(nameof(AutoUpdateTags), out setting) && !string.IsNullOrWhiteSpace(setting))
                AutoUpdateTags = setting.ParseBoolean();

            if (settings.TryGetValue(nameof(TagNamePrefixRemoveCount), out setting) && int.TryParse(setting, out intVal) && intVal >= 0)
                TagNamePrefixRemoveCount = intVal;

            if (!settings.TryGetValue(nameof(PointIDMapCacheFileName), out setting) || string.IsNullOrWhiteSpace(setting))
                setting = $"{Name}_PointIDMap.cache";

            PointIDMapCacheFileName = FilePath.GetAbsolutePath(setting);

            if (settings.TryGetValue(nameof(DigitalSetString), out setting) && !string.IsNullOrWhiteSpace(setting))
                DigitalSetString = setting;

            if (settings.TryGetValue(nameof(DigitalClearedString), out setting) && !string.IsNullOrWhiteSpace(setting))
                DigitalClearedString = setting;

            if (settings.TryGetValue(nameof(ValidateINIFileExists), out setting) && !string.IsNullOrWhiteSpace(setting))
                ValidateINIFileExists = setting.ParseBoolean();

            if (ValidateINIFileExists)
            {
                string iniFile = string.Format(Default.IniFilePathFormat, Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));

                if (!File.Exists(iniFile))
                    throw new FileNotFoundException($"Cannot initialize adapter: eDNA client API INI configuration file \"{iniFile}\" does not exist.");
            }

            if (settings.TryGetValue(nameof(MaximumPointResolution), out setting) && double.TryParse(setting, out doubleVal) && doubleVal >= 0.0D)
                MaximumPointResolution = doubleVal;

            if (MaximumPointResolution > 0.0D)
                m_lastArchiveTimes = new Dictionary<Guid, Ticks>();

            if (settings.TryGetValue(nameof(ConnectionMonitoringInterval), out setting) && int.TryParse(setting, out intVal) && intVal >= 0)
                ConnectionMonitoringInterval = intVal;
        }

        /// <summary>
        /// Connects to the configured eDNA server.
        /// </summary>
        protected override void AttemptConnection()
        {
            int result;

            m_processedMappings = 0;
            m_processedMeasurements = 0;
            m_totalProcessingTime = 0;

            lock (m_pendingMappings)
                m_pendingMappings.Clear();

            m_mapRequestQueue.Clear();
            m_mapRequestQueue.Start();

            // Initialize connection eDNA client API
            string cacheFileName = FilePath.GetFileName(LocalCacheFileName);
            string cachePath = FilePath.GetDirectoryName(LocalCacheFileName);

            result = LinkMX.eDnaMxUniversalInitialize(out m_connection, AcknowledgeDataPackets, EnableQueuing, EnableCaching, int.MaxValue, cacheFileName, cachePath);

            if (result != 0)
                throw new EzDNAApiNetException($"Failed to initialize connection to eDNA Universal Service: {(LinkMXReturnStatus)result}", result);

            if (ClearCacheOnStartup)
            {
                result = LinkMX.eDnaMxDeleteCacheFiles(m_connection);

                if (result != 0)
                    OnProcessException(MessageLevel.Warning, new EzDNAApiNetException($"Failed to delete local cache files: {(LinkMXReturnStatus)result}", result));
            }

            result = LinkMX.eDnaMxUniversalDataConnect(m_connection, PrimaryServer, PrimaryPort, SecondaryServer, SecondaryPort);

            string connectionInfo = $"{PrimaryServer}:{PrimaryPort}{(string.IsNullOrWhiteSpace(SecondaryServer) ? "" : $" / {SecondaryServer}:{SecondaryPort}")}";

            switch (result)
            {
                case 0:
                    OnStatusMessage(MessageLevel.Info, $"Connected to eDNA service on {connectionInfo} - connection handle = {m_connection}");
                    break;
                case 1:
                    OnStatusMessage(MessageLevel.Info, $"Connected to primary eDNA service on {PrimaryServer}:{PrimaryPort} - connection handle = {m_connection}");
                    break;
                case 2:
                    OnStatusMessage(MessageLevel.Info, $"Connected to secondary eDNA service on {SecondaryServer}:{SecondaryPort} - connection handle = {m_connection}");
                    break;
                default:
                    throw new EzDNAApiNetException($"Failed to connect to eDNA service on {connectionInfo}: {(LinkMXReturnStatus)result}", result);
            }

            if (ConnectionMonitoringInterval > 0)
            {
                m_connectionMonitor.Interval = ConnectionMonitoringInterval;
                m_connectionMonitor.Start();
            }

            // Kick off meta-data refresh
            RefreshMetadata();
        }

        /// <summary>
        /// Closes this <see cref="OutputAdapter"/> connections to the eDNA server.
        /// </summary>
        protected override void AttemptDisconnection()
        {
            m_mapRequestQueue.Stop();
            m_mapRequestQueue.Clear();

            lock (m_pendingMappings)
                m_pendingMappings.Clear();

            m_connectionMonitor.Stop();

            if (m_connection > 0U)
                LinkMX.eDnaMxUniversalCloseSocketSoft(m_connection);

            m_connection = 0U;
        }

        // Make sure API operations against connection handle are handled synchronously
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T ExecuteConnectionOperation<T>(Func<T> operation)
        {
            lock (m_connectionOperationLock)
                return operation();
        }

        /// <summary>
        /// Sorts measurements and sends them to the configured eDNA server in batches
        /// </summary>
        /// <param name="measurements">Measurements to queue</param>
        protected override void ProcessMeasurements(IMeasurement[] measurements)
        {
            if ((object)measurements == null || measurements.Length <= 0 || m_connection == 0U)
                return;

            bool addedRecords = false;

            // Strategy here is to start dynamically mapping measurements to eDNA point IDs as they
            // are encountered - this is important since we may be simultaneously synchronizing
            // meta-data, need to define the point in eDNA before we can successfully archive it.
            foreach (IMeasurement measurement in measurements)
            {
                Guid signalID = measurement.Key.SignalID;
                string pointID;

                // If adapter gets disabled while executing this thread - go ahead and exit
                if (!Enabled)
                    return;

                // Lookup point ID mapping for this measurement - if no mapping was found, move on to next measurement
                if (!m_pointIDMap.TryGetValue(signalID, out pointID))
                    continue;

                if ((object)pointID == null)
                {
                    // If point ID mapping is null, kick off process to look it up in eDNA...
                    try
                    {
                        bool mappingRequested = false;

                        lock (m_pendingMappings)
                        {
                            // Only start mapping process if one is not already pending
                            if (!m_pendingMappings.Contains(signalID))
                            {
                                mappingRequested = true;
                                m_pendingMappings.Add(signalID);
                            }
                        }

                        if (mappingRequested)
                        {
                            // No mapping is defined for this point, queue up mapping
                            lock (m_mapRequestQueue.SyncRoot)
                                m_mapRequestQueue.Add(signalID);
                        }
                    }
                    catch (Exception ex)
                    {
                        OnProcessException(MessageLevel.Info, new InvalidOperationException($"Failed to associate measurement value with connection point for '{measurement.Key}': {ex.Message}", ex));
                    }
                }
                else
                {
                    // Have a valid mapping, archive the data to eDNA service
                    Ticks startTime = DateTime.UtcNow.Ticks;

                    try
                    {
                        // Verify maximum per point archive resolution
                        if ((object)m_lastArchiveTimes != null && (measurement.Timestamp - m_lastArchiveTimes.GetOrAdd(signalID, measurement.Timestamp)).ToSeconds() < MaximumPointResolution)
                            continue;

                        // Separate seconds from milliseconds and convert seconds to have a Unix relative base, i.e., midnight 1/1/1970
                        Ticks baselinedTicks = measurement.Timestamp.BaselinedTimestamp(BaselineTimeInterval.Second);
                        int seconds = (int)new UnixTimeTag(baselinedTicks).Value;
                        ushort milliseconds = (ushort)(measurement.Timestamp - baselinedTicks).ToMilliseconds();

                        // Queue measurement record to eDNA
                        int result = ExecuteConnectionOperation(() => 
                            LinkMX.eDnaMxAddRec(m_connection, pointID, seconds, milliseconds, measurement.StateFlags.MapToStatus(), measurement.AdjustedValue));

                        if (result != 0)
                            OnProcessException(MessageLevel.Warning, new EzDNAApiNetException($"Failed to write measurement \"{measurement.Key}\" to eDNA point \"{pointID}\": {(LinkMXReturnStatus)result}", result));

                        if ((object)m_lastArchiveTimes != null)
                            m_lastArchiveTimes[signalID] = measurement.Timestamp;

                        Interlocked.Increment(ref m_processedMeasurements);
                        addedRecords = true;
                    }
                    finally
                    {
                        Interlocked.Add(ref m_totalProcessingTime, DateTime.UtcNow.Ticks - startTime);
                    }
                }
            }

            // Commit new records
            if (addedRecords)
            {
                int result = ExecuteConnectionOperation(() => 
                    LinkMX.eDnaMxFlushUniversalRecord(m_connection, (int)LinkMXConstants.SET_REC));

                if (result != 0)
                    throw new EzDNAApiNetException($"{(LinkMXReturnStatus)result}", result);
            }
        }

        private void EstablishPointIDMappings(Guid[] signalIDs)
        {
            try
            {
                string pointID;
                bool mappingEstablished = false;

                foreach (Guid signalID in signalIDs)
                {
                    // If adapter gets disabled while executing this thread - go ahead and exit
                    if (!Enabled)
                        return;

                    try
                    {
                        mappingEstablished = false;

                        if (m_pointIDMap.TryGetValue(signalID, out pointID))
                        {
                            if ((object)pointID == null)
                            {
                                pointID = QueryPointID(signalID);

                                if ((object)pointID != null)
                                {
                                    m_pointIDMap[signalID] = pointID;
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
                        OnProcessException(MessageLevel.Info, new InvalidOperationException($"Failed to establish point ID mapping for \"{signalID}\": {ex.Message}", ex));
                    }
                    finally
                    {
                        lock (m_pendingMappings)
                            m_pendingMappings.Remove(signalID);

                        if (!mappingEstablished)
                            m_pointIDMap.TryRemove(signalID, out pointID);

                        // Provide some level of feed back on progress of mapping process
                        Interlocked.Increment(ref m_processedMappings);

                        if (Interlocked.Read(ref m_processedMappings) % 100 == 0)
                            OnStatusMessage(MessageLevel.Info, $"Mapped {m_pointIDMap.Count - m_pendingMappings.Count:N0} point IDs to measurements, {1.0D - m_pendingMappings.Count / (double)m_pointIDMap.Count:0.00%} complete...");
                    }
                }
            }
            finally
            {
                // Make sure point ID map cache gets updated
                m_savePointIDMapCache.RunOnceAsync();
            }
        }

        private string QueryPointID(Guid signalID)
        {
            string pointID = null;
            string tagName = null;

            // Map measurement to eDNA point
            try
            {
                // Two ways to find points here:
                //   1. if running meta-data sync for the adapter, look for the signal ID in the long ID field
                //   2. if points are being manually maintained, look for either the point tag or alternate tag in the short ID field
                bool foundPoint = false;

                if (RunMetadataSync)
                {
                    // Attempt lookup by Guid based signal ID
                    pointID = QueryPointIDForSignalID(signalID); // LongID search
                    foundPoint = (object)pointID != null;
                }

                if (!foundPoint)
                {
                    // Lookup meta-data for current measurement
                    DataRow[] rows = DataSource.Tables["ActiveMeasurements"].Select($"SignalID='{signalID}'");

                    if (rows.Length > 0)
                    {
                        DataRow measurementRow = rows[0];
                        tagName = measurementRow["PointTag"].ToNonNullString().Trim();

                        // Use alternate tag if one is defined
                        if (!string.IsNullOrWhiteSpace(measurementRow["AlternateTag"].ToString()) && !measurementRow["SignalType"].ToString().Equals("DIGI", StringComparison.OrdinalIgnoreCase))
                            tagName = measurementRow["AlternateTag"].ToString().Trim();

                        // Attempt lookup by tag name
                        pointID = QueryPointIDForTagName(tagName); // ShortID search

                        if ((object)pointID == null)
                        {
                            if (!m_refreshingMetadata)
                                OnStatusMessage(MessageLevel.Warning, $"No eDNA points found for tag \"{tagName}\". Data will not be archived for measurement.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                OnProcessException(MessageLevel.Warning, new InvalidOperationException($"Failed to map measurement \"{tagName ?? signalID.ToString()}\" to eDNA point ID: {ex.Message}", ex));
            }

            return pointID;
        }

        /// <summary>
        /// Sends updated meta-data to eDNA.
        /// </summary>
        protected override void ExecuteMetadataRefresh()
        {
            if (!Initialized || m_connection == 0U)
                return;

            MeasurementKey[] inputMeasurements = InputMeasurementKeys;
            int previousMeasurementReportingInterval = MeasurementReportingInterval;
            DateTime latestUpdateTime = DateTime.MinValue;

            m_refreshingMetadata = RunMetadataSync;

            // For first runs, don't report archived points until eDNA meta-data has been established
            MeasurementReportingInterval = 0;

            // Attempt to load point ID map from existing cache, if any
            LoadPointIDMapCache();

            if (m_pointIDMap.Count > 0)
                MeasurementReportingInterval = previousMeasurementReportingInterval;

            // Refresh initial point ID mappings (much of the meta-data may already exist)
            RefreshPointIDMap(inputMeasurements);

            if (!RunMetadataSync)
            {
                MeasurementReportingInterval = previousMeasurementReportingInterval;
                return;
            }

            try
            {
                OnStatusMessage(MessageLevel.Info, "Beginning meta-data refresh...");

                if ((object)inputMeasurements != null && inputMeasurements.Length > 0)
                {
                    int processed = 0, total = inputMeasurements.Length;
                    AdoDataConnection database = null;
                    DataTable measurements = DataSource.Tables["ActiveMeasurements"];

                    foreach (MeasurementKey key in inputMeasurements)
                    {
                        // If adapter gets disabled while executing this thread - go ahead and exit
                        if (!Enabled)
                            return;

                        // Lookup meta-data for currently defined input measurement
                        Guid signalID = key.SignalID;
                        DataRow[] rows = measurements.Select($"SignalID='{signalID}'");
                        DateTime updateTime;
                        string pointID;

                        // It's unlikely that we would encounter an undefined measurement, but if so move on to the next one
                        if (rows.Length <= 0)
                        {
                            m_pointIDMap.TryRemove(signalID, out pointID);
                            continue;
                        }

                        // Get matching measurement row
                        DataRow measurementRow = rows[0];

                        // Define data type for field
                        DataType dataType = measurementRow["SignalType"].ToString().Equals("DIGI", StringComparison.OrdinalIgnoreCase) ? DataType.Digital : DataType.Analog;

                        // Get point tag-name as defined in meta-data, adjusting as needed per TagNamePrefixRemoveCount
                        string tagName = GetAdjustedTagName(measurementRow["PointTag"].ToNonNullString().Trim());

                        // Use alternate tag if one is defined - note that digitals are an exception since they typically use this field for special labeling
                        if (!string.IsNullOrWhiteSpace(measurementRow["AlternateTag"].ToString()) && dataType != DataType.Digital)
                            tagName = measurementRow["AlternateTag"].ToString().Trim();

                        // It's unlikley that no tag name is defined for measurement, but if so move on to the next one
                        if (string.IsNullOrWhiteSpace(tagName))
                        {
                            m_pointIDMap.TryRemove(signalID, out pointID);
                            continue;
                        }

                        // Attempt to lookup eDNA point ID trying both signal ID and tag name
                        pointID = QueryPointIDForSignalID(signalID) ?? QueryPointIDForTagName(tagName);

                        // Determine last update time for this meta-data record
                        try
                        {
                            // See if ActiveMeasurements contains updated on column
                            if (measurements.Columns.Contains("UpdatedOn"))
                            {
                                updateTime = Convert.ToDateTime(measurementRow["UpdatedOn"]);
                            }
                            else
                            {
                                // Attempt to lookup last update time for record
                                if ((object)database == null)
                                    database = new AdoDataConnection("systemSettings");

                                updateTime = Convert.ToDateTime(database.Connection.ExecuteScalar($"SELECT UpdatedOn FROM Measurement WHERE SignalID = '{signalID}'"));
                            }
                        }
                        catch (Exception)
                        {
                            updateTime = DateTime.UtcNow;
                        }

                        // Tracked latest update time in meta-data, this will become last meta-data refresh time
                        if (updateTime > latestUpdateTime)
                            latestUpdateTime = updateTime;

                        // Attempt to create new eDNA point if it doesn't exist, or update it if measurement meta-data was updated or tag name doesn't match
                        if (AutoCreateTags && (object)pointID == null || AutoUpdateTags && (updateTime > m_lastMetadataRefresh || !(QueryTagNameForSignalID(signalID)?.Equals(tagName, StringComparison.InvariantCultureIgnoreCase) ?? false)))
                        {
                            try
                            {
                                string units = "";
                                string pointType = dataType == DataType.Digital ? "DI" : "AI";
                                string digitalSet = dataType == DataType.Digital ? DigitalSetString : "";
                                string digitalCleared = dataType == DataType.Digital ? DigitalClearedString : "";

                                if (measurements.Columns.Contains("EngineeringUnits"))
                                    units = measurementRow["EngineeringUnits"].ToNonNullString();

                                if (dataType == DataType.Digital)
                                {
                                    if (measurements.Columns.Contains("DigitalSetString"))
                                    {
                                        string value = measurementRow["DigitalSetString"].ToString();

                                        if (!string.IsNullOrWhiteSpace(value))
                                            digitalSet = value;
                                    }

                                    if (measurements.Columns.Contains("DigitalClearedString"))
                                    {
                                        string value = measurementRow["DigitalClearedString"].ToString();

                                        if (!string.IsNullOrWhiteSpace(value))
                                            digitalCleared = value;
                                    }
                                }

                                int result;

                                if ((object)pointID == null)
                                {
                                    // Add new meta-data record
                                    result = ExecuteConnectionOperation(() => 
                                        LinkMX.eDnaMxAddConfigRec(m_connection, tagName, key.SignalID.ToString(), measurementRow["Description"].ToString(),
                                        units, pointType, false, 0, digitalSet, digitalCleared, false, 0.0D, false, 0.0D, false, 0.0D, false, 0.0D,
                                        false, 0.0D, false, 0.0D, true, false, 1, 0, int.MaxValue, 0.0D, 0, pointID, null));
                                }
                                else
                                {
                                    int channelNumber = int.Parse(pointID.Substring(pointID.LastIndexOf('.') + 1));

                                    // Edit existing meta-data record
                                    result = ExecuteConnectionOperation(() => 
                                        LinkMX.eDnaMxAddConfigRecChannelNum(m_connection, channelNumber, tagName, key.SignalID.ToString(), measurementRow["Description"].ToString(),
                                        units, pointType, false, 0, digitalSet, digitalCleared, false, 0.0D, false, 0.0D, false, 0.0D, false, 0.0D,
                                        false, 0.0D, false, 0.0D, true, false, 1, 0, int.MaxValue, 0.0D, 0, pointID, null));
                                }

                                if (result != 0)
                                    throw new EzDNAApiNetException($"{(LinkMXReturnStatus)result}", result);

                                result = ExecuteConnectionOperation(() => 
                                    LinkMX.eDnaMxFlushUniversalRecord(m_connection, (int)LinkMXConstants.SET_CONFIGURATION_REC));

                                if (result != 0)
                                    throw new EzDNAApiNetException($"{(LinkMXReturnStatus)result}", result);
                            }
                            catch (Exception ex)
                            {
                                OnProcessException(MessageLevel.Warning, new InvalidOperationException($"Failed to add or update eDNA meta-data \"{tagName}\" for measurement \"{key}\": {ex.Message}", ex));
                            }
                        }

                        processed++;
                        m_metadataRefreshProgress = processed / (double)total;

                        if (processed % 100 == 0)
                            OnStatusMessage(MessageLevel.Info, $"Updated {processed:N0} eDNA tags and associated meta-data, {m_metadataRefreshProgress:0.00%} complete...");

                        // If mapping for this point ID does not exist, it may have been because there was no meta-data so
                        // we re-add to dictionary with null value, then actual mapping will happen dynamically as needed
                        if (!m_pointIDMap.ContainsKey(signalID))
                            m_pointIDMap.TryAdd(signalID, null);
                    }

                    if ((object)database != null)
                        database.Dispose();
                }
                else
                {
                    OnStatusMessage(MessageLevel.Warning, "eDNA historian output adapter is not configured to receive any input measurements - meta-data refresh canceled.");
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

            if (Enabled)
            {
                m_lastMetadataRefresh = latestUpdateTime > DateTime.MinValue ? latestUpdateTime : DateTime.UtcNow;

                OnStatusMessage(MessageLevel.Info, "Completed meta-data refresh successfully.");

                // Re-establish connection point dictionary since meta-data and tags may exist in eDNA server now that didn't before.
                // This will also start showing warning messages in CreateMappedPoint function for unmappable points now that
                // meta-data refresh has completed.
                RefreshPointIDMap(inputMeasurements);
            }

            // Restore original measurement reporting interval
            MeasurementReportingInterval = previousMeasurementReportingInterval;
        }

        private void LoadPointIDMapCache()
        {
            if (!File.Exists(PointIDMapCacheFileName))
                return;

            // Attempt to load point ID map from existing cache file
            lock (m_savePointIDMapCache)
            {
                try
                {
                    using (FileStream pointIDMapCache = File.Open(PointIDMapCacheFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        m_pointIDMap.Merge(true, Serialization.Deserialize<Dictionary<Guid, string>>(pointIDMapCache, GSF.SerializationFormat.Binary));

                        OnStatusMessage(MessageLevel.Info, $"Loaded {m_pointIDMap.Count:N0} mappings from point ID map cache.");

                        // Use last write time of file as the last meta-data refresh time - rough value is OK
                        if (m_lastMetadataRefresh == DateTime.MinValue)
                            m_lastMetadataRefresh = File.GetLastWriteTimeUtc(PointIDMapCacheFileName);
                    }
                }
                catch (Exception ex)
                {
                    OnProcessException(MessageLevel.Info, new InvalidOperationException($"Failed to load point ID map cache from file \"{PointIDMapCacheFileName}\"': {ex.Message}", ex));
                }
            }
        }

        // Do not call method directly, invoke via synchronized operation: m_savePointIDMapCache.RunOnceAsync();
        private void SavePointIDMapCache()
        {
            if (m_pointIDMap.Count <= 0 || !Enabled)
                return;

            int spanSinceLastFlush = (int)(DateTime.UtcNow.Ticks - m_lastPointIDMapFlushTime).ToMilliseconds();

            // Don't flush file more than once per second
            if (spanSinceLastFlush < 1000)
                Thread.Sleep(1000 - spanSinceLastFlush);

            // Cache point ID map for faster future eDNA adapter startup
            lock (m_savePointIDMapCache)
            {
                try
                {
                    using (FileStream pointIDMapCache = File.Create(PointIDMapCacheFileName))
                        Serialization.Serialize(m_pointIDMap.ToDictionary(kvp => kvp.Key, kvp => kvp.Value), GSF.SerializationFormat.Binary, pointIDMapCache);
                }
                catch (Exception ex)
                {
                    OnProcessException(MessageLevel.Info, new InvalidOperationException($"Failed to save point ID map cache to file \"{PointIDMapCacheFileName}\": {ex.Message}", ex));
                }
            }

            m_lastPointIDMapFlushTime = DateTime.UtcNow.Ticks;
        }

        private void RefreshPointIDMap(MeasurementKey[] inputMeasurements)
        {
            OnStatusMessage(MessageLevel.Info, "Refreshing point ID mappings...");

            List<Guid> newSignalIDs = new List<Guid>();

            if ((object)inputMeasurements != null && inputMeasurements.Length > 0)
            {
                foreach (MeasurementKey key in inputMeasurements)
                {
                    Guid signalID = key.SignalID;

                    // Add key to dictionary with null value if not defined, actual mapping will be established dynamically as needed
                    if (!m_pointIDMap.ContainsKey(signalID))
                        m_pointIDMap.TryAdd(signalID, null);

                    newSignalIDs.Add(signalID);
                }
            }

            if (newSignalIDs.Count > 0)
            {
                // Determine which tags no longer exist
                string removedPointID;
                HashSet<Guid> signalIDsToRemove = new HashSet<Guid>(m_pointIDMap.Keys);

                // If there are existing tags that are not part of new updates, these need to be removed
                signalIDsToRemove.ExceptWith(newSignalIDs);

                if (signalIDsToRemove.Count > 0)
                {
                    foreach (Guid signalID in signalIDsToRemove)
                        m_pointIDMap.TryRemove(signalID, out removedPointID);

                    OnStatusMessage(MessageLevel.Info, $"Detected {signalIDsToRemove.Count:N0} tags that have been removed from eDNA output - primary point ID map has been updated...");
                }

                if (m_pointIDMap.Count == 0)
                    OnStatusMessage(MessageLevel.Warning, "No eDNA tags were mapped to measurements - no point ID map exists so no points will be archived.");
            }
            else
            {
                if (m_pointIDMap.Count > 0)
                    OnStatusMessage(MessageLevel.Warning, $"No eDNA tags were mapped to measurements - existing point ID map with {m_pointIDMap.Count:N0} defined mappings remains in use.");
                else
                    OnStatusMessage(MessageLevel.Warning, "No eDNA tags were mapped to measurements - no point ID map exists so no points will be archived.");
            }
        }

        // Lookup eDNA point ID using point tag name (i.e., short ID)
        private string QueryPointIDForTagName(string tagName)
        {
            string shortID;
            return QueryPointID("shortID", tagName, out shortID);
        }

        // Lookup eDNA point ID using signal ID (i.e., long ID)
        private string QueryPointIDForSignalID(Guid signalID)
        {
            string pointID, shortID;
            m_pointIDMap.TryGetValue(signalID, out pointID);
            return pointID ?? QueryPointID("longID", signalID.ToString(), out shortID);
        }

        // Lookup tag name in eDNA meta-data using signal ID (i.e., long ID)
        private string QueryTagNameForSignalID(Guid signalID)
        {
            string shortID, pointID = QueryPointID("longID", signalID.ToString(), out shortID);
            return (object)pointID == null || pointID == "*" ? null : shortID;
        }

        private string QueryPointID(string searchKey, string value, out string shortID)
        {
            string pointID = null;
            string error;
            int key;

            string longID = searchKey.Equals("longID", StringComparison.OrdinalIgnoreCase) ? value : "*";   // Used to store Guid based SignalID
            shortID = searchKey.Equals("shortID", StringComparison.OrdinalIgnoreCase) ? value : "*";        // Used to store PointTag (or AlternateTag)

            int result = Configuration.EzSimpleFindPoints(Site, Service, shortID, longID, 
                "*", "*", "*", "*", "*", "*", "*", "*", "*", "*", "*", "*", "*", "*", "*",
                -1, out key);

            if (result == 0)
            {
                if (Configuration.EzSimpleFindPointsSize(key) > 0)
                {
                    int channelNumber;
                    string site, service, extendedID, description, extendedDescription, pointType, units,
                           referenceField01, referenceField02, referenceField03, referenceField04, referenceField05,
                           referenceField06, referenceField07, referenceField08, referenceField09, referenceField10;

                    result = Configuration.EzSimpleFindPointsRec(key, 0,
                        out site, out service, out shortID, out longID, out extendedID,out description, out extendedDescription, out pointType, out units,
                        out referenceField01, out referenceField02, out referenceField03, out referenceField04, out referenceField05, out referenceField06,
                        out referenceField07, out referenceField08, out referenceField09, out referenceField10, out channelNumber);

                    if (result == 0)
                    {
                        pointID = string.Format(Default.PointIDFormat, site, service, channelNumber);
                    }
                    else
                    {
                        Configuration.EzSimpleFindPointsGetLastError(out error);
                        OnProcessException(MessageLevel.Warning, new EzDNAApiNetException($"Failed to read pointID meta-data for \"{searchKey}\"=\"{value}\": {error}", result));
                    }
                }

                Configuration.EzFindPointsRemoveKey(key);
            }
            else
            {
                Configuration.EzSimpleFindPointsGetLastError(out error);
                OnProcessException(MessageLevel.Warning, new EzDNAApiNetException($"Failed to lookup pointID meta-data searching for \"{searchKey}\"=\"{value}\": {error}", result));
            }

            return pointID;
        }

        private string GetAdjustedTagName(string tagName)
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

        private void m_connectionMonitor_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            bool reconnect = true;

            if (ExecuteConnectionOperation(() => LinkMX.ISeDnaMxUniversalConnected(m_connection)))
            {
                // Verify connection is up at socket level, API is not always accurate
                Func<string, IPAddress> parseIP = host => string.IsNullOrWhiteSpace(host) ? IPAddress.None : IPAddress.Parse(host);

                IPAddress primary = parseIP(PrimaryServer);
                IPAddress secondary = parseIP(SecondaryServer);

                Func<TcpConnectionInformation, IPAddress, int, bool> isEstablished = (connection, address, port) =>
                    connection.RemoteEndPoint.Address.Equals(address) &&
                    connection.RemoteEndPoint.Port == (int)port &&
                    connection.State == TcpState.Established;

                reconnect = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections().Any(connection => 
                    isEstablished(connection, primary, PrimaryPort) || isEstablished(connection, secondary, SecondaryPort));
            }

            if (reconnect)
            {
                OnStatusMessage(MessageLevel.Warning, "Detected no active connections to eDNA service, restarting connection cycle...");
                Start();
            }
        }

        private void m_mapRequestQueue_ProcessException(object sender, EventArgs<Exception> e)
        {
            OnProcessException(MessageLevel.Warning, e.Argument);
        }

        #endregion
    }
}