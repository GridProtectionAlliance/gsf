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
using System.Text;
using System.Threading;
using System.Timers;
using GSF;
using GSF.Collections;
using GSF.Data;
using GSF.Diagnostics;
using GSF.IO;
using GSF.Threading;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using InStep.eDNA.EzDNAApiNet;
using SerializationFormat = GSF.SerializationFormat;
using Timer = System.Timers.Timer;

namespace eDNAAdapters
{
    /// <summary>
    /// Exports measurements to eDNA if the point tag or alternate tag corresponds to a eDNA point's tag name.
    /// </summary>
    [Description("eDNA: Archives measurements to an eDNA server")]
    public class OutputAdapter : OutputAdapterBase
    {
        #region [ Members ]

        // Nested Types
        [Serializable]
        private class Point
        {
            private static RadixCodec Base36 = RadixCodec.Radix36;

            /// <summary>
            /// Base-36 encoded string of point ID value.
            /// </summary>
            public readonly string ID;

            /// <summary>
            /// Point data type, i.e., analog or digital.
            /// </summary>
            public readonly DataType Type;

            public Point(uint id, DataType type)
            {
                // Encode ID value to base-36 string
                ID = Base36.Encode(id);
                Type = type;
            }

            /// <summary>
            /// Point ID integer value decoded from base-36 string.
            /// </summary>
            public uint IDValue => Base36.Decode<uint>(ID);
        }

        // Constants
        private const string DataCacheSuffix = "_eDNAData.cache";
        private const string MetaCacheSuffix = "_eDNAMeta.cache";

        // Fields        
        private uint m_dataConnection;                                      // eDNA server connection handle for data
        private uint m_metaConnection;                                      // eDNA server connection handle for meta-data
        private readonly Timer m_connectionMonitor;                         // eDNA server connection monitor, optional
        private readonly ConcurrentDictionary<Guid, Point> m_pointMap;      // Persisted measurement signal ID to point map
        private readonly LongSynchronizedOperation m_savePointMapCache;     // Save point map cache operation
        private Dictionary<Guid, Ticks> m_lastArchiveTimes;                 // Cache of last point archive times
        private DateTime m_lastMetadataRefresh;                             // Tracks time of last meta-data refresh
        private long m_processedMeasurements;                               // Total number of measurements processed so far
        private volatile bool m_refreshingMetadata;                         // Flag that determines if meta-data is currently refreshing
        private double m_metadataRefreshProgress;                           // Current meta-data refresh progress
        private Ticks m_lastPointMapFlushTime;                              // Last time of point map file flush
        private bool m_disposed;                                            // Flag that determines if class is disposed

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="OutputAdapter"/>
        /// </summary>
        public OutputAdapter()
        {
            m_dataConnection = uint.MaxValue;
            m_metaConnection = uint.MaxValue;
            m_connectionMonitor = new Timer();
            m_connectionMonitor.Enabled = false;
            m_connectionMonitor.AutoReset = true;
            m_connectionMonitor.Elapsed += m_connectionMonitor_Elapsed;
            m_pointMap = new ConcurrentDictionary<Guid, Point>();
            m_savePointMapCache = new LongSynchronizedOperation(SavePointMapCache, ex => OnProcessException(MessageLevel.Warning, ex));
            m_savePointMapCache.IsBackground = false;
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
        public string PrimaryServer { get; set; }

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
        public string Site { get; set; }

        /// <summary>
        /// Gets or sets the eDNA service for the adapter's connection.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the eDNA service for the adapter's connection.")]
        public string Service { get; set; }

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
        /// Gets or sets the filename to be used for point map cache.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the path and filename to be used for the eDNA client API cache. Leave blank for cache name to target host working directory and be same as adapter name.")]
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
        /// Gets or sets the filename to be used for point map cache.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the path and filename to be used for the point map cache file name. Leave blank for cache name to target host working directory and to be same as adapter name with a \"_PointMap.cache\" extension.")]
        [DefaultValue(Default.PointMapCacheFileName)]
        public string PointMapCacheFileName { get; set; } = Default.PointMapCacheFileName;

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
        /// Gets or sets the timeout, in milliseconds, for writing data to eDNA connection. A value of -1 will wait indefinitely.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the timeout, in milliseconds, for writing data to eDNA connection. A value of -1 will wait indefinitely.")]
        [DefaultValue(Default.WriteTimeout)]
        public int WriteTimeout { get; set; } = Default.WriteTimeout;

        /// <summary>
        /// Gets or sets flag that determines if each bit of digital words are expanded to individual points. Set to False to treat words as analog values.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines flag that determines if each bit of digital words are expanded to individual points. Set to False to treat words as analog values.")]
        [DefaultValue(Default.ExpandDigitalWordBits)]
        public bool ExpandDigitalWordBits { get; set; } = Default.ExpandDigitalWordBits;

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
                status.AppendFormat("       eDNA primary server: {0}:{1}", PrimaryServer ?? "[undefined]", PrimaryPort);
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
                status.AppendFormat("       Connected to server: {0}", m_dataConnection == uint.MaxValue ? "No" : $"Yes, handle = {m_dataConnection}");
                status.AppendLine();
                status.AppendFormat("       Monitoring interval: {0:N0}ms{1}", ConnectionMonitoringInterval, ConnectionMonitoringInterval <= 0 ? " - monitoring disabled" : "");
                status.AppendLine();
                status.AppendFormat("             Write timeout: {0}", WriteTimeout == Timeout.Infinite ? "No timeout defined" : $"{WriteTimeout:N0}ms");
                status.AppendLine();
                status.AppendFormat(" Expanding 16-bit digitals: {0}", ExpandDigitalWordBits);
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
                    status.AppendFormat("          Local cache file: {0}", FilePath.TrimFileName($"{LocalCacheFileName}{DataCacheSuffix}", 51));
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

                status.AppendFormat("      Point map cache file: {0}", FilePath.TrimFileName(PointMapCacheFileName.ToNonNullString("[undefined]"), 51));
                status.AppendLine();
                status.AppendFormat("      Point map cache size: {0:N0} mappings", m_pointMap.Count);
                status.AppendLine();

                if (RunMetadataSync)
                {
                    status.AppendFormat("      Meta-data connection: {0}", m_metaConnection == uint.MaxValue ? "Not connected" : $"Established, handle = {m_metaConnection}");
                    status.AppendLine();
                    status.AppendFormat("    Meta-data sync process: {0}, {1:0.00%} complete", m_refreshingMetadata ? "Active" : "Idle", m_metadataRefreshProgress);
                    status.AppendLine();
                }

                status.AppendFormat("           Points archived: {0:N0} - {1:#,##0}/second", Interlocked.Read(ref m_processedMeasurements), Interlocked.Read(ref m_processedMeasurements) / RunTime);
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
            else
                throw new ArgumentException($"Cannot initialize adapter: required connection string parameter \"{nameof(PrimaryServer)}\" is not defined.");

            if (settings.TryGetValue(nameof(PrimaryPort), out setting) && ushort.TryParse(setting, out ushortVal))
                PrimaryPort = ushortVal;

            if (settings.TryGetValue(nameof(SecondaryServer), out setting) && !string.IsNullOrWhiteSpace(setting))
                SecondaryServer = setting;

            if (settings.TryGetValue(nameof(SecondaryPort), out setting) && ushort.TryParse(setting, out ushortVal))
                SecondaryPort = ushortVal;

            if (settings.TryGetValue(nameof(Site), out setting) && !string.IsNullOrWhiteSpace(setting))
                Site = setting;
            else
                throw new ArgumentException($"Cannot initialize adapter: required connection string parameter \"{nameof(Site)}\" is not defined.");

            if (settings.TryGetValue(nameof(Service), out setting) && !string.IsNullOrWhiteSpace(setting))
                Service = setting;
            else
                throw new ArgumentException($"Cannot initialize adapter: required connection string parameter \"{nameof(Service)}\" is not defined.");

            if (settings.TryGetValue(nameof(AcknowledgeDataPackets), out setting) && !string.IsNullOrWhiteSpace(setting))
                AcknowledgeDataPackets = setting.ParseBoolean();

            if (settings.TryGetValue(nameof(EnableQueuing), out setting) && !string.IsNullOrWhiteSpace(setting))
                EnableQueuing = setting.ParseBoolean();

            if (settings.TryGetValue(nameof(EnableCaching), out setting) && !string.IsNullOrWhiteSpace(setting))
                EnableCaching = setting.ParseBoolean();

            if (!settings.TryGetValue(nameof(LocalCacheFileName), out setting) || string.IsNullOrWhiteSpace(setting))
                setting = Name;

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

            if (!settings.TryGetValue(nameof(PointMapCacheFileName), out setting) || string.IsNullOrWhiteSpace(setting))
                setting = $"{Name}_PointMap.cache";

            PointMapCacheFileName = FilePath.GetAbsolutePath(setting);

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

            if (settings.TryGetValue(nameof(WriteTimeout), out setting) && int.TryParse(setting, out intVal) && intVal >= -1)
                WriteTimeout = intVal;

            InternalProcessQueue.ProcessTimeout = WriteTimeout;

            if (settings.TryGetValue(nameof(ExpandDigitalWordBits), out setting) && !string.IsNullOrWhiteSpace(setting))
                ExpandDigitalWordBits = setting.ParseBoolean();
        }

        private int ConnectToDNAService(out uint connection, string cacheSuffix)
        {
            string fileName = $"{LocalCacheFileName}{cacheSuffix}";
            string cacheFileName = FilePath.GetFileName(fileName);
            string cachePath = FilePath.GetDirectoryName(fileName);

            int result = LinkMX.eDnaMxUniversalInitialize(out connection, AcknowledgeDataPackets, EnableQueuing, EnableCaching, int.MaxValue, cacheFileName, cachePath);

            if (result != 0)
                throw new EzDNAApiNetException($"Failed to initialize connection to eDNA Universal Service: {(LinkMXReturnStatus)result}", result);

            if (EnableCaching && ClearCacheOnStartup)
            {
                result = LinkMX.eDnaMxDeleteCacheFiles(connection);

                if (result != 0)
                    OnProcessException(MessageLevel.Warning, new EzDNAApiNetException($"Failed to delete local cache files: {(LinkMXReturnStatus)result}", result));
            }

            return LinkMX.eDnaMxUniversalDataConnect(connection, PrimaryServer, PrimaryPort, SecondaryServer, SecondaryPort);
        }

        /// <summary>
        /// Connects to the configured eDNA server.
        /// </summary>
        protected override void AttemptConnection()
        {
            m_processedMeasurements = 0;

            // Initialize connection eDNA client API
            int result = ConnectToDNAService(out m_dataConnection, DataCacheSuffix);

            string connectionInfo = $"{PrimaryServer}:{PrimaryPort}{(string.IsNullOrWhiteSpace(SecondaryServer) ? "" : $" / {SecondaryServer}:{SecondaryPort}")}";

            switch (result)
            {
                case 0:
                    OnStatusMessage(MessageLevel.Info, $"Connected to eDNA service on {connectionInfo} - connection handle = {m_dataConnection}");
                    break;
                case 1:
                    OnStatusMessage(MessageLevel.Info, $"Connected to primary eDNA service on {PrimaryServer}:{PrimaryPort} - connection handle = {m_dataConnection}");
                    break;
                case 2:
                    OnStatusMessage(MessageLevel.Info, $"Connected to secondary eDNA service on {SecondaryServer}:{SecondaryPort} - connection handle = {m_dataConnection}");
                    break;
                default:
                    throw new EzDNAApiNetException($"Failed to connect to eDNA service on {connectionInfo}: {(LinkMXReturnStatus)result}", result);
            }

            if (ConnectionMonitoringInterval > 0)
            {
                m_connectionMonitor.Interval = ConnectionMonitoringInterval;
                m_connectionMonitor.Start();
            }
        }

        /// <summary>
        /// Called when data output source connection is established.
        /// </summary>
        protected override void OnConnected()
        {
            base.OnConnected();

            // Refresh meta-data once connected
            RefreshMetadata();
        }

        /// <summary>
        /// Closes this <see cref="OutputAdapter"/> connections to the eDNA server.
        /// </summary>
        protected override void AttemptDisconnection()
        {
            m_connectionMonitor.Stop();

            if (m_dataConnection < uint.MaxValue)
                LinkMX.eDnaMxUniversalCloseSocketSoft(m_dataConnection);

            if (m_metaConnection < uint.MaxValue)
                LinkMX.eDnaMxUniversalCloseSocketSoft(m_metaConnection);

            m_dataConnection = uint.MaxValue;
            m_metaConnection = uint.MaxValue;
        }

        /// <summary>
        /// Sorts measurements and sends them to the configured eDNA server in batches
        /// </summary>
        /// <param name="measurements">Measurements to queue</param>
        protected override void ProcessMeasurements(IMeasurement[] measurements)
        {
            if ((object)measurements == null || measurements.Length <= 0 || m_dataConnection == uint.MaxValue)
                return;

            bool addedRecords = false;
            int result;

            foreach (IMeasurement measurement in measurements)
            {
                Guid signalID = measurement.Key.SignalID;
                Point point;

                // If adapter gets disabled while executing this thread - go ahead and exit
                if (!Enabled)
                    return;

                // Lookup point mapping for this measurement - if no mapping was found, move on to next measurement
                if (!m_pointMap.TryGetValue(signalID, out point) || (object)point == null)
                    continue;

                // Verify maximum per point archive resolution
                if ((object)m_lastArchiveTimes != null && (measurement.Timestamp - m_lastArchiveTimes.GetOrAdd(signalID, measurement.Timestamp)).ToSeconds() < MaximumPointResolution)
                    continue;

                // Separate seconds from milliseconds and convert seconds to have a Unix base value, i.e., midnight 1/1/1970
                Ticks baselinedTicks = measurement.Timestamp.BaselinedTimestamp(BaselineTimeInterval.Second);
                int seconds = (int)new UnixTimeTag(baselinedTicks).Value;
                ushort milliseconds = (ushort)(measurement.Timestamp - baselinedTicks).ToMilliseconds();
                double value = measurement.AdjustedValue;

                // Queue measurement record to eDNA
                if (point.Type == DataType.Digital)
                {
                    ushort word = (ushort)value;

                    for (int i = 0; i < 16; i++)
                    {
                        ushort bit = (ushort)BitExtensions.BitVal(i);
                        bool digitalIsSet = (word & bit) > 0;
                        
                        result = LinkMX.eDnaMxAddRec(m_dataConnection, $"{point.ID}-{(byte)i:x}", seconds, milliseconds, measurement.StateFlags.MapToStatus(DataType.Digital, digitalIsSet), digitalIsSet ? 1.0D : 0.0D);

                        if (result != 0)
                            OnProcessException(MessageLevel.Warning, new EzDNAApiNetException($"Failed to write measurement \"{measurement.Key}\" bit {i} to eDNA point \"{string.Format(Default.PointIDFormat, Site, Service, point.ID)}\": LinkMX.eDnaMxAddRec Exception: {(LinkMXReturnStatus)result}", result));
                    }
                }
                else
                {
                    result = LinkMX.eDnaMxAddRec(m_dataConnection, point.ID, seconds, milliseconds, measurement.StateFlags.MapToStatus(), value);

                    if (result != 0)
                        OnProcessException(MessageLevel.Warning, new EzDNAApiNetException($"Failed to write measurement \"{measurement.Key}\" to eDNA point \"{string.Format(Default.PointIDFormat, Site, Service, point.ID)}\": LinkMX.eDnaMxAddRec Exception: {(LinkMXReturnStatus)result}", result));
                }

                if ((object)m_lastArchiveTimes != null)
                    m_lastArchiveTimes[signalID] = measurement.Timestamp;

                Interlocked.Increment(ref m_processedMeasurements);
                addedRecords = true;
            }

            // Commit new records
            if (addedRecords)
            {
                result = LinkMX.eDnaMxFlushUniversalRecord(m_dataConnection, (int)LinkMXConstants.SET_REC);

                if (result != 0)
                    throw new EzDNAApiNetException($"LinkMX.eDnaMxFlushUniversalRecord Exception: {(LinkMXReturnStatus)result}", result);
            }
        }

        /// <summary>
        /// Sends updated meta-data to eDNA.
        /// </summary>
        protected override void ExecuteMetadataRefresh()
        {
            if (!Initialized)
                return;

            try
            {
                // Lookup the data source table for active measurements
                DataTable measurements = null;

                if (DataSource?.Tables.Contains("ActiveMeasurements") ?? false)
                    measurements = DataSource.Tables["ActiveMeasurements"];

                if ((object)measurements == null)
                {
                    OnStatusMessage(MessageLevel.Warning, "Could not find \"ActiveMeasurements\" data source table - meta-data refresh canceled.");
                    return;
                }

                // Get inputs for this adapter
                MeasurementKey[] inputMeasurements = InputMeasurementKeys;

                if ((object)inputMeasurements == null || inputMeasurements.Length == 0)
                {
                    OnStatusMessage(MessageLevel.Warning, "eDNA historian output adapter is not configured to receive any input measurements - meta-data refresh canceled.");
                    return;
                }

                // This meta-data "load" is performed out-of-band with the meta-data "save" operation that
                // follows to reduce read-write contention with the eDNA database service
                Dictionary<Guid, Metadata> metadataCache = QueryMetaDataForInputs(inputMeasurements, measurements);
                int result;

                // Connect to eDNA separately for meta-data refresh, this way meta-data refresh does not contend
                // with time-series data updates. Have to maintain this connection as long as main connection is
                // active because closing this connection will also close the main connection :p
                if (m_metaConnection == uint.MaxValue)
                {
                    result = ConnectToDNAService(out m_metaConnection, MetaCacheSuffix);

                    if (result < 0 || result > 2)
                    {
                        m_metaConnection = uint.MaxValue;
                        throw new EzDNAApiNetException($"Failed to connect to eDNA service for meta-data refresh: {(LinkMXReturnStatus)result}", result);
                    }
                }

                int previousMeasurementReportingInterval = MeasurementReportingInterval;
                DateTime latestUpdateTime = DateTime.MinValue;

                m_refreshingMetadata = RunMetadataSync;

                // For first runs, don't report archived points until eDNA meta-data has been established
                MeasurementReportingInterval = 0;

                // Attempt to load point map from existing cache, if any
                LoadPointMapCache();

                if (m_pointMap.Count > 0)
                    MeasurementReportingInterval = previousMeasurementReportingInterval;

                // Refresh initial point mappings (much of the meta-data may already exist)
                RefreshPointMap(metadataCache, inputMeasurements);

                // Exit now if meta-data is maintained by eDNA
                if (!RunMetadataSync)
                {
                    MeasurementReportingInterval = previousMeasurementReportingInterval;
                    return;
                }

                try
                {
                    OnStatusMessage(MessageLevel.Info, "Beginning meta-data refresh...");

                    int processed = 0, total = inputMeasurements.Length;
                    AdoDataConnection database = null;

                    foreach (MeasurementKey key in inputMeasurements)
                    {
                        // If adapter gets disabled while executing this thread - go ahead and exit
                        if (!Enabled)
                            return;

                        // Lookup meta-data for currently defined input measurement
                        Guid signalID = key.SignalID;
                        DataRow[] rows = measurements.Select($"SignalID='{signalID}'");
                        DateTime updateTime;
                        Metadata metadata;
                        Point point;

                        // It's unlikely that we would encounter an undefined measurement, but if so move on to the next one
                        if (rows.Length <= 0)
                        {
                            m_pointMap.TryRemove(signalID, out point);
                            continue;
                        }

                        DataRow row = rows[0];

                        // Define data type for field
                        DataType dataType = row["SignalType"].ToString().Equals("DIGI", StringComparison.OrdinalIgnoreCase) ? DataType.Digital : DataType.Analog;

                        // Get point tag-name as defined in meta-data, adjusting as needed per TagNamePrefixRemoveCount
                        string tagName = GetAdjustedTagName(row["PointTag"].ToNonNullString().Trim());

                        // Use alternate tag if one is defined - note that digitals are an exception since they typically use this field for special labeling
                        if (!string.IsNullOrWhiteSpace(row["AlternateTag"].ToString()) && dataType != DataType.Digital)
                            tagName = row["AlternateTag"].ToString().Trim();

                        // It's unlikely that no tag name is defined for measurement, but if so move on to the next one
                        if (string.IsNullOrWhiteSpace(tagName))
                        {
                            m_pointMap.TryRemove(signalID, out point);
                            continue;
                        }

                        // Attempt to lookup eDNA point in point map
                        m_pointMap.TryGetValue(signalID, out point);

                        // Attempt to lookup eDNA metadata in cache
                        metadataCache.TryGetValue(signalID, out metadata);

                        // Determine last update time for this meta-data record
                        try
                        {
                            // See if ActiveMeasurements contains updated on column
                            if (measurements.Columns.Contains("UpdatedOn"))
                            {
                                updateTime = Convert.ToDateTime(row["UpdatedOn"]);
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

                        // If digital words are not being expanded as bits, treat them as analogs
                        if (dataType == DataType.Digital && !ExpandDigitalWordBits)
                            dataType = DataType.Analog;

                        // Attempt to create new eDNA point if it doesn't exist, or update it if measurement meta-data was updated or tag name doesn't match
                        if (AutoCreateTags && (object)point == null || AutoUpdateTags && (object)point != null && (updateTime > m_lastMetadataRefresh || !(metadata?.ReferenceField01.Equals(tagName, StringComparison.InvariantCultureIgnoreCase) ?? false) || point.Type != dataType))
                        {
                            try
                            {
                                // Create new point if needed
                                if ((object)point == null || point.Type != dataType)
                                    point = new Point(key.ID, dataType);

                                string units = "";
                                string pointType = dataType == DataType.Digital ? "DI" : "AI";
                                string digitalSet = dataType == DataType.Digital ? DigitalSetString : "";
                                string digitalCleared = dataType == DataType.Digital ? DigitalClearedString : "";
                                int values = dataType == DataType.Digital ? 16 : 1;

                                if (measurements.Columns.Contains("EngineeringUnits"))
                                    units = row["EngineeringUnits"].ToNonNullString();

                                if (dataType == DataType.Digital)
                                {
                                    if (measurements.Columns.Contains("DigitalSetString"))
                                    {
                                        string value = row["DigitalSetString"].ToString();

                                        if (!string.IsNullOrWhiteSpace(value))
                                            digitalSet = value;
                                    }

                                    if (measurements.Columns.Contains("DigitalClearedString"))
                                    {
                                        string value = row["DigitalClearedString"].ToString();

                                        if (!string.IsNullOrWhiteSpace(value))
                                            digitalCleared = value;
                                    }
                                }

                                // Update point map
                                m_pointMap[signalID] = point;

                                // ReSharper disable once LoopVariableIsNeverChangedInsideLoop
                                for (int i = 0; i < values; i++)
                                {
                                    string pointID = point.ID;                                          // 8 chars   < reliable base-36 encoded point ID
                                    string longID = signalID.ToString();                                // 60 chars  < reliable signal ID (primary Guid)
                                    string description = tagName;                                       // 24 chars  < truncated tag name
                                    string extendedID = $"A{key.ID}";                                   // 128 chars < reliable point ID (not encoded)
                                    string extendedDescription = $"{row["Description"]}";               // 224 chars < truncated description
                                    string referenceField01 = tagName;                                  // 250 chars < reliable tag name
                                    string referenceField02 = $"{row["SignalReference"]}";              // 250 chars < reliable signal reference
                                    string referenceField03 = $"{row["SignalType"]}";                   // 250 chars < reliable signal type acronym
                                    string referenceField04 = $"{row["Device"]}";                       // 250 chars < reliable device acronym
                                    string referenceField05 = $"{key}";                                 // 250 chars < reliable measurement key (Source:ID)
                                    string referenceField06 = $"{row["Latitude"]},{row["Longitude"]}";  // 250 chars < reliable lat,long
                                    string referenceField07 = $"{row["Company"]}";                      // 250 chars < reliable company acronym
                                    string referenceField08 = $"{row["Protocol"]}";                     // 250 chars < reliable protocol acronym
                                    string referenceField09 = metadata?.ReferenceField09 ?? "";         // 250 chars < spare reference 1
                                    string referenceField10 = metadata?.ReferenceField10 ?? "";         // 250 chars < spare reference 2

                                    if (dataType == DataType.Digital)
                                    {
                                        // Digital point ID with max 6 character base-36 encoded ID value, dash and hex bit suffix is no more than 8 bytes:
                                        pointID = $"{pointID}-{(byte)i:x}";                         // Format digital encoded point ID as "<base36ID>-<bitNhex>"
                                        extendedID = $"D{key.ID}-{i}";                              // Format digital non encoded point ID as "D<pointID>-<bitN>"
                                        description = referenceField01 = $"{description}-{i}";      // Suffix digital tag name with "-<bitN>"
                                        extendedDescription = $"Bit {i} of {extendedDescription}";  // Prefix digital description with bit number info
                                    }

                                    // Add new or update meta-data record, critical time-series library (TSL) mappings are as follows:
                                    //   LongID = Measurement.SignalID (used as primary TSL tag identifier)
                                    //   ExtendedID = "A{Measurement.PointID}" or "D{Measurement.PointID}-{bitN}"
                                    //   ReferenceField01 = Measurement.PointTag (or Measurement.AlternateTag if defined)
                                    //   ReferenceField05 = MeasurementKey (Source:ID)
                                    result = LinkMX.eDnaMxAddConfigRec(m_metaConnection, pointID, longID, description, units, pointType,
                                        false, 0, digitalSet, digitalCleared, false, 0.0D, false, 0.0D, false, 0.0D, false, 0.0D, false,
                                        0.0D, false, 0.0D, true, false, 1, 0, int.MaxValue, 0.0D, 0, extendedID, extendedDescription);

                                    if (result != 0)
                                        throw new EzDNAApiNetException($"LinkMX.eDnaMxAddConfigRec Exception: {(LinkMXReturnStatus)result}", result);

                                    result = LinkMX.eDnaMxFlushUniversalRecord(m_metaConnection, (int)LinkMXConstants.SET_CONFIGURATION_REC);

                                    if (result != 0)
                                        throw new EzDNAApiNetException($"LinkMX.eDnaMxFlushUniversalRecord Exception: {(LinkMXReturnStatus)result}", result);

                                    // Update configuration reference fields that hold reliable meta-data mappings
                                    result = LinkMX.eDnaMxAddConfigReferenceFields(m_metaConnection, pointID, longID, extendedID,
                                        referenceField01, referenceField02, referenceField03, referenceField04, referenceField05,
                                        referenceField06, referenceField07, referenceField08, referenceField09, referenceField10);

                                    if (result != 0)
                                        throw new EzDNAApiNetException($"LinkMX.eDnaMxAddConfigReferenceFields Exception: {(LinkMXReturnStatus)result}", result);

                                    result = LinkMX.eDnaMxFlushUniversalRecord(m_metaConnection, (int)LinkMXConstants.SET_CONFIGURATION_REF_FIELDS);

                                    if (result != 0)
                                        throw new EzDNAApiNetException($"LinkMX.eDnaMxFlushUniversalRecord Exception: {(LinkMXReturnStatus)result}", result);
                                }
                            }
                            catch (Exception ex)
                            {
                                OnProcessException(MessageLevel.Warning, new InvalidOperationException($"Failed to add or update eDNA meta-data{(point == null ? "" : $" \"{string.Format(Default.PointIDFormat, Site, Service, point.ID)}\"")} for measurement \"[{key}]: {tagName}\": {ex.Message}", ex));
                            }
                        }

                        processed++;
                        m_metadataRefreshProgress = processed / (double)total;

                        if (processed % 500 == 0)
                            OnStatusMessage(MessageLevel.Info, $"Updated {processed:N0} eDNA tags and associated meta-data, {m_metadataRefreshProgress:0.00%} complete...");

                        // Queue up a save operation for point map cache
                        m_savePointMapCache.RunOnceAsync();
                    }

                    if ((object)database != null)
                        database.Dispose();
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
                }

                // Restore original measurement reporting interval
                MeasurementReportingInterval = previousMeasurementReportingInterval;
            }
            catch (Exception ex)
            {
                OnProcessException(MessageLevel.Warning, new InvalidOperationException($"Failed during meta-data refresh: {ex.Message}"));
            }
        }

        private void LoadPointMapCache()
        {
            if (!File.Exists(PointMapCacheFileName))
                return;

            // Attempt to load point map from existing cache file
            lock (m_savePointMapCache)
            {
                try
                {
                    using (FileStream pointMapCache = File.Open(PointMapCacheFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        // Deserialize last point map for quicker startup
                        Dictionary<Guid, Point> pointMap = Serialization.Deserialize<Dictionary<Guid, Point>>(pointMapCache, SerializationFormat.Binary);

                        // Copy cached set to active point map
                        foreach (KeyValuePair<Guid, Point> kvp in pointMap)
                            m_pointMap[kvp.Key] = kvp.Value;

                        OnStatusMessage(MessageLevel.Info, $"Loaded {pointMap.Count:N0} mappings from point map cache.");

                        // Use last write time of file as the last meta-data refresh time - rough value is OK
                        if (m_lastMetadataRefresh == DateTime.MinValue)
                            m_lastMetadataRefresh = File.GetLastWriteTimeUtc(PointMapCacheFileName);
                    }
                }
                catch (Exception ex)
                {
                    OnProcessException(MessageLevel.Info, new InvalidOperationException($"Failed to load point map cache from file \"{PointMapCacheFileName}\"': {ex.Message}", ex));
                }
            }
        }

        // Do not call method directly, invoke via synchronized operation: m_savePointMapCache.RunOnceAsync();
        private void SavePointMapCache()
        {
            if (m_pointMap.Count <= 0 || !Enabled)
                return;

            int spanSinceLastFlush = (int)(DateTime.UtcNow.Ticks - m_lastPointMapFlushTime).ToMilliseconds();

            if (spanSinceLastFlush < 0)
                spanSinceLastFlush = 0;

            // Don't flush file more than once per 5 seconds
            if (spanSinceLastFlush < 5000)
            {
                int count = 0, total = (5000 - spanSinceLastFlush) / 100;

                while (Enabled && count < total)
                {
                    count++;
                    Thread.Sleep(100);
                }
            }

            // Cache point map for faster future eDNA adapter startup
            lock (m_savePointMapCache)
            {
                try
                {
                    using (FileStream pointMapCache = File.Create(PointMapCacheFileName))
                        Serialization.Serialize(m_pointMap.ToDictionary(kvp => kvp.Key, kvp => kvp.Value), SerializationFormat.Binary, pointMapCache);

                    OnStatusMessage(MessageLevel.Info, $"Saved {m_pointMap.Count:N0} mappings to point map cache.");
                }
                catch (Exception ex)
                {
                    OnProcessException(MessageLevel.Info, new InvalidOperationException($"Failed to save point map cache to file \"{PointMapCacheFileName}\": {ex.Message}", ex));
                }
            }

            m_lastPointMapFlushTime = DateTime.UtcNow.Ticks;
        }

        /// <summary>
        /// Scan eDNA meta-data intersecting defined time-series meta-data filtering for inputs defined for this adapter.
        /// </summary>
        /// <param name="inputMeasurements">Input measurements keys defined for this output adapter.</param>
        /// <param name="measurements">Available time-series meta-data table.</param>
        /// <returns>Matching eDNA meta-data map keyed by time-series signal ID.</returns>
        private Dictionary<Guid, Metadata> QueryMetaDataForInputs(MeasurementKey[] inputMeasurements, DataTable measurements)
        {
            Dictionary<Guid, Metadata> metadataCache = new Dictionary<Guid, Metadata>();

            // Get meta-data count
            int count = 0, total = Metadata.Count(Site, Service).NotZero(inputMeasurements.Length);

            OnStatusMessage(MessageLevel.Info, $"Querying {total:N0} eDNA \"{Site}.{Service}\" meta-data records for matching inputs...");

            Ticks startTime = DateTime.UtcNow.Ticks;

            // Scan all meta-data for records that match adapter inputs. A full scan is needed because some key TSL
            // meta-data mappings are stored in non-key reference fields and API functions perform an O(n) operation
            // for lookups into non-key fields - so one full scan is better than a full scan per adapter input
            foreach (Metadata record in Metadata.Query(new Metadata(/* search all */)))
            {
                Guid signalID;
                bool foundMatch = false;

                // LongID = Measurement.SignalID (used as primary TSL tag identifier)
                if (!string.IsNullOrWhiteSpace(record.LongID) && Guid.TryParse(record.LongID, out signalID))
                {
                    // See if an input exists that matches this meta-data record
                    if (inputMeasurements.Any(key => key.SignalID == signalID))
                    {
                        metadataCache[signalID] = record;
                        foundMatch = true;
                    }
                }

                // ReferenceField01 = Measurement.PointTag (or Measurement.AlternateTag if defined)
                if (!foundMatch && !string.IsNullOrWhiteSpace(record.ReferenceField01))
                {
                    DataRow[] rows;

                    // Lookup tag name in AlternateTag field first, this should take precedence if defined
                    rows = measurements.Select($"AlternateTag='{record.ReferenceField01}'");

                    if (rows.Length == 0)
                    {
                        // Lookup tag name in PointTag field
                        rows = measurements.Select($"PointTag='{record.ReferenceField01}'");

                        if (rows.Length > 0)
                            foundMatch = true;
                    }
                    else
                    {
                        foundMatch = true;
                    }

                    // If a match was found for tag name, attempt to parse its signal ID
                    if (foundMatch && Guid.TryParse(rows[0]["SignalID"].ToString(), out signalID))
                    {
                        // See if an input exists that matches this meta-data record
                        if (inputMeasurements.Any(key => key.SignalID == signalID))
                            metadataCache[signalID] = record;
                    }
                }

                if (count++ % 500 == 0 && count > 1)
                    OnStatusMessage(MessageLevel.Info, $"Queried {count:N0} eDNA \"{Site}.{Service}\" meta-data records, {metadataCache.Count:N0} matches, {count / (double)total:0.00%} complete...");
            }

            if (metadataCache.Count > 0)
                OnStatusMessage(MessageLevel.Info, $"Loaded {metadataCache.Count:N0} matching eDNA \"{Site}.{Service}\" meta-data records in {(DateTime.UtcNow.Ticks - startTime).ToElapsedTimeString(3)}.");
            else
                OnStatusMessage(MessageLevel.Info, $"Found no matching eDNA \"{Site}.{Service}\" meta-data records - scan time: {(DateTime.UtcNow.Ticks - startTime).ToElapsedTimeString(3)}.");

            return metadataCache;
        }

        private void RefreshPointMap(Dictionary<Guid, Metadata> metadataCache, MeasurementKey[] inputMeasurements)
        {
            OnStatusMessage(MessageLevel.Info, "Refreshing point mappings...");

            List<Guid> signalIDs = new List<Guid>();

            foreach (MeasurementKey key in inputMeasurements)
            {
                Guid signalID = key.SignalID;
                Metadata metadata;

                if (!m_pointMap.ContainsKey(signalID) && metadataCache.TryGetValue(signalID, out metadata))
                    m_pointMap[signalID] = new Point(key.ID, metadata.DataType);

                signalIDs.Add(signalID);
            }

            if (signalIDs.Count > 0)
            {
                // Determine which tags no longer exist
                Point removedPoint;
                HashSet<Guid> signalIDsToRemove = new HashSet<Guid>(m_pointMap.Keys);

                // If there are existing tags that are not part of new updates, these need to be removed
                signalIDsToRemove.ExceptWith(signalIDs);

                if (signalIDsToRemove.Count > 0)
                {
                    foreach (Guid signalID in signalIDsToRemove)
                        m_pointMap.TryRemove(signalID, out removedPoint);

                    OnStatusMessage(MessageLevel.Info, $"Detected {signalIDsToRemove.Count:N0} tags that have been removed from eDNA output - primary point map has been updated...");
                }

                if (m_pointMap.Count == 0)
                    OnStatusMessage(MessageLevel.Warning, "No eDNA tags were mapped to measurements - no point map exists so no points will be archived.");
            }
            else
            {
                if (m_pointMap.Count > 0)
                    OnStatusMessage(MessageLevel.Warning, $"No eDNA tags were mapped to measurements - existing point map with {m_pointMap.Count:N0} defined mappings remains in use.");
                else
                    OnStatusMessage(MessageLevel.Warning, "No eDNA tags were mapped to measurements - no point map exists so no points will be archived.");
            }
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

        private void m_connectionMonitor_Elapsed(object sender, ElapsedEventArgs e)
        {
            bool reconnect = true;

            if (LinkMX.ISeDnaMxUniversalConnected(m_dataConnection))
            {
                // Verify connection is up at socket level, API is not always accurate
                Func<string, IPAddress> parseIP = host => string.IsNullOrWhiteSpace(host) ? IPAddress.None : IPAddress.Parse(host);

                IPAddress primaryIP = parseIP(PrimaryServer);
                IPAddress secondaryIP = parseIP(SecondaryServer);

                Func<TcpConnectionInformation, IPAddress, int, bool> isEstablished = (connection, address, port) =>
                    connection.RemoteEndPoint.Address.Equals(address) &&
                    connection.RemoteEndPoint.Port == port &&
                    connection.State == TcpState.Established;

                reconnect = !IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections().Any(connection =>
                    isEstablished(connection, primaryIP, PrimaryPort) || isEstablished(connection, secondaryIP, SecondaryPort));
            }

            if (reconnect)
            {
                OnStatusMessage(MessageLevel.Warning, "Detected no active socket connections to eDNA service, restarting connection cycle...");
                Start();
            }
        }

        #endregion
    }
}