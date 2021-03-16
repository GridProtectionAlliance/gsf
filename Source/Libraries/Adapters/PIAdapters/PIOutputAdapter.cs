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
//
//******************************************************************************************************

using GSF;
using GSF.Collections;
using GSF.Data;
using GSF.Diagnostics;
using GSF.IO;
using GSF.Threading;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using OSIsoft.AF.Asset;
using OSIsoft.AF.Data;
using OSIsoft.AF.PI;
using OSIsoft.AF.Time;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

// ReSharper disable InconsistentlySynchronizedField
namespace PIAdapters
{
    /// <summary>
    /// Exports measurements to PI if the point tag or alternate tag corresponds to a PI point's tag name.
    /// </summary>
    [Description("OSI-PI: Archives measurements to an OSI-PI server using AF-SDK")]
    public class PIOutputAdapter : OutputAdapterBase
    {
        #region [ Members ]

        // Fields

        // Define cached mapping between GSFSchema measurements and PI points
        private readonly ConcurrentDictionary<MeasurementKey, PIPoint> m_mappedPIPoints;

        private readonly ProcessQueue<AFValue>[] m_archiveQueues;           // Collection of point archival queues
        private readonly ProcessQueue<MeasurementKey> m_mapRequestQueue;    // Requested measurement to PI point mapping queue
        private readonly ShortSynchronizedOperation m_restartConnection;    // Restart connection operation
        private readonly ConcurrentDictionary<Guid, string> m_tagMap;       // Tag name to measurement Guid lookup
        private readonly HashSet<MeasurementKey> m_pendingMappings;         // List of pending measurement mappings
        private Dictionary<Guid, Ticks> m_lastArchiveTimes;                 // Cache of last point archive times
        private PIConnection m_connection;                                  // PI server connection for meta-data synchronization
        private DateTime m_lastMetadataRefresh;                             // Tracks time of last meta-data refresh
        private long m_processedMappings;                                   // Total number of mappings processed so far
        private long m_processedMeasurements;                               // Total number of measurements processed so far
        private long m_totalProcessingTime;                                 // Total point processing time 
        private volatile bool m_refreshingMetadata;                         // Flag that determines if meta-data is currently refreshing
        private double m_metadataRefreshProgress;                           // Current meta-data refresh progress
        private bool m_disposed;                                            // Flag that determines if class is disposed

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
            m_pendingMappings = new HashSet<MeasurementKey>();
            m_lastMetadataRefresh = DateTime.MinValue;
            RunMetadataSync = true;
            AutoCreateTags = true;
            AutoUpdateTags = true;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Returns true to indicate that this <see cref="PIOutputAdapter"/> is sending measurements to a historian, OSIsoft PI.
        /// </summary>
        public override bool OutputIsForArchive => true;

        /// <summary>
        /// Returns false to indicate that this <see cref="PIOutputAdapter"/> will connect synchronously
        /// </summary>
        protected override bool UseAsyncConnect => false;

        /// <summary>
        /// Gets or sets the name of the PI server for the adapter's PI connection.
        /// </summary>
        [ConnectionStringParameter, Description("Defines the name of the PI server for the adapter's PI connection.")]
        public string ServerName { get; set; }

        /// <summary>
        /// Gets or sets the name of the PI user ID for the adapter's PI connection.
        /// </summary>
        [ConnectionStringParameter, Description("Defines the name of the PI user ID for the adapter's PI connection."), DefaultValue("")]
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the password used for the adapter's PI connection.
        /// </summary>
        [ConnectionStringParameter, Description("Defines the password used for the adapter's PI connection."), DefaultValue("")]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the timeout interval (in milliseconds) for the adapter's connection
        /// </summary>
        [ConnectionStringParameter, Description("Defines the timeout interval (in milliseconds) for the adapter's connection"), DefaultValue(PIConnection.DefaultConnectTimeout)]
        public int ConnectTimeout { get; set; }

        /// <summary>
        /// Gets or sets whether or not this adapter should automatically manage metadata for PI points.
        /// </summary>
        [ConnectionStringParameter, Description("Determines if this adapter should automatically manage metadata for PI points (recommended)."), DefaultValue(true)]
        public bool RunMetadataSync { get; set; }

        /// <summary>
        /// Gets or sets whether or not this adapter should automatically manage metadata for PI points.
        /// </summary>
        [ConnectionStringParameter, Description("Determines if this adapter should automatically create new tags when managing metadata for PI points (recommended). Value will only be considered when RunMetadataSync is True."), DefaultValue(true)]
        public bool AutoCreateTags { get; set; }

        /// <summary>
        /// Gets or sets whether or not this adapter should automatically manage metadata for PI points.
        /// </summary>
        [ConnectionStringParameter, Description("Determines if this adapter should automatically update existing tags when managing metadata for PI points (recommended). This will make openPDC the master for maintaining PI tag metadata (like the tag name); otherwise, when False, PI will be the master. Value will only be considered when RunMetadataSync is True."), DefaultValue(true)]
        public bool AutoUpdateTags { get; set; }

        /// <summary>
        /// Gets or sets the number of tag name prefixes, e.g., "SOURCE!", applied by subscriptions to remove from PI tag names.
        /// </summary>
        [ConnectionStringParameter, Description("Defines the number of tag name prefixes applied by subscriptions, e.g., \"SOURCE!\", to remove from PI tag names. Value will only be considered when RunMetadataSync is True."), DefaultValue(0)]
        public int TagNamePrefixRemoveCount { get; set; }

        /// <summary>
        /// Gets or sets the point source string used when automatically creating new PI points during the metadata update
        /// </summary>
        [ConnectionStringParameter, Description("Defines the point source string used when automatically creating new PI points during the metadata update. Value will only be considered when RunMetadataSync is True."), DefaultValue("GSF")]
        public string PIPointSource { get; set; }

        /// <summary>
        /// Gets or sets the point class string used when automatically creating new PI points during the metadata update. On the PI server, this class should inherit from classic.
        /// </summary>
        [ConnectionStringParameter, Description("Defines the point class string used when automatically creating new PI points during the metadata update. On the PI server, this class should inherit from classic. Value will only be considered when RunMetadataSync is True."), DefaultValue("classic")]
        public string PIPointClass { get; set; }

        /// <summary>
        /// Gets or sets flag that determines if defined point compression will be used during archiving.
        /// </summary>
        [ConnectionStringParameter, Description("Defines the flag that determines if defined point compression will be used during archiving."), DefaultValue(true)]
        public bool UseCompression { get; set; }

        /// <summary>
        /// Gets or sets the filename to be used for tag map cache.
        /// </summary>
        [ConnectionStringParameter, Description("Defines the filename to be used for tag map cache file name. Leave blank for cache name to be same as adapter name with a \".cache\" extension."), DefaultValue("")]
        public string TagMapCacheFileName { get; set; }

        /// <summary>
        /// Gets or sets the maximum time resolution, in seconds, for data points being archived, e.g., a value 1.0 would mean that data would be archived no more than once per second. A zero value indicates that all data should be archived.
        /// </summary>
        [ConnectionStringParameter, Description("Defines the maximum time resolution, in seconds, for data points being archived, e.g., a value 1.0 would mean that data would be archived no more than once per second. A zero value indicates that all data should be archived."), DefaultValue(0.0D)]
        public double MaximumPointResolution { get; set; }

        /// <summary>
        /// Returns the detailed status of the data output source.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.Append(base.Status);
                status.AppendLine();
                status.AppendLine($"        OSI-PI server name: {ServerName}");
                status.AppendLine($"       Connected to server: {(m_connection?.Connected ?? false ? "Yes" : "No")}");
                status.AppendLine($"         Using compression: {UseCompression}");
                status.AppendLine($"  Maximum point resolution: {MaximumPointResolution:N3} seconds{(MaximumPointResolution <= 0.0D ? " - all data will be archived" : "")}");
                status.AppendLine($"    Meta-data sync enabled: {RunMetadataSync}");

                if (RunMetadataSync)
                {
                    status.AppendLine($"          Auto-create tags: {AutoCreateTags}");
                    status.AppendLine($"          Auto-update tags: {AutoUpdateTags}");
                    status.AppendLine($"    Tag prefixes to remove: {TagNamePrefixRemoveCount}");
                    status.AppendLine($"       OSI-PI point source: {PIPointSource}");
                    status.AppendLine($"        OSI-PI point class: {PIPointClass}");
                }

                if (!(m_mapRequestQueue is null))
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

                if (!(m_archiveQueues is null) && m_archiveQueues.Length > 0)
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

                if (!(m_archiveQueues is null))
                {
                    foreach (ProcessQueue<AFValue> archiveQueue in m_archiveQueues)
                        archiveQueue.Dispose();
                }

                if (!(m_connection is null))
                {
                    m_connection.Disconnected -= Connection_Disconnected;
                    m_connection.Dispose();
                    m_connection = null;
                }

                m_mappedPIPoints?.Clear();
            }
            finally
            {
                m_disposed = true;       // Prevent duplicate dispose.
                base.Dispose(disposing); // Call base class Dispose().
            }
        }

        /// <summary>
        /// Returns a brief status of this <see cref="PIOutputAdapter"/>
        /// </summary>
        /// <param name="maxLength">Maximum number of characters in the status string</param>
        /// <returns>Status</returns>
        public override string GetShortStatus(int maxLength) => 
            $"Archived {m_processedMeasurements:N0} measurements to PI.".CenterText(maxLength);

        /// <summary>
        /// Initializes this <see cref="PIOutputAdapter"/>.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            Dictionary<string, string> settings = Settings;

            if (!settings.TryGetValue(nameof(ServerName), out string setting))
                throw new InvalidOperationException($"The \"{nameof(ServerName)}\" setting is required for PI connections.");

            ServerName = setting;

            // Track instance in static dictionary
            Instances[ServerName] = this;

            UserName = settings.TryGetValue(nameof(UserName), out setting) ? setting : null;
            Password = settings.TryGetValue(nameof(Password), out setting) ? setting : null;

            if (!settings.TryGetValue(nameof(ConnectTimeout), out setting) || !int.TryParse(setting, out int intVal))
                intVal = PIConnection.DefaultConnectTimeout;

            ConnectTimeout = intVal;
            UseCompression = !settings.TryGetValue(nameof(UseCompression), out setting) || setting.ParseBoolean();

            if (!settings.TryGetValue(nameof(TagNamePrefixRemoveCount), out setting) || !int.TryParse(setting, out intVal))
                intVal = 0;

            TagNamePrefixRemoveCount = intVal;

            if (settings.TryGetValue(nameof(RunMetadataSync), out setting))
                RunMetadataSync = setting.ParseBoolean();

            if (settings.TryGetValue(nameof(AutoCreateTags), out setting))
                AutoCreateTags = setting.ParseBoolean();

            if (settings.TryGetValue(nameof(AutoUpdateTags), out setting))
                AutoUpdateTags = setting.ParseBoolean();

            PIPointSource = settings.TryGetValue(nameof(PIPointSource), out setting) ? setting : "GSF";
            PIPointClass = settings.TryGetValue(nameof(PIPointClass), out setting) ? setting : "classic";

            if (!settings.TryGetValue(nameof(TagMapCacheFileName), out setting) || string.IsNullOrWhiteSpace(setting))
                setting = Name + "_TagMap.cache";

            TagMapCacheFileName = FilePath.GetAbsolutePath(setting);

            if (settings.TryGetValue(nameof(MaximumPointResolution), out setting) && double.TryParse(setting, out double doubleVal) && doubleVal > 0.0D)
                MaximumPointResolution = doubleVal;

            if (MaximumPointResolution > 0.0D)
                m_lastArchiveTimes = new Dictionary<Guid, Ticks>();
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
                ServerName = this.ServerName,
                UserName = this.UserName,
                Password = this.Password,
                ConnectTimeout = this.ConnectTimeout
            };

            m_connection.Disconnected += Connection_Disconnected;
            m_connection.Open();

            m_mappedPIPoints.Clear();

            lock (m_pendingMappings)
            {
                m_pendingMappings.Clear();
            }

            m_mapRequestQueue.Clear();
            m_mapRequestQueue.Start();

            foreach (ProcessQueue<AFValue> archiveQueue in m_archiveQueues)
                archiveQueue.Start();

            // Kick off meta-data refresh
            RefreshMetadata();
        }

        /// <summary>
        /// Closes this <see cref="PIOutputAdapter"/> connections to the PI server.
        /// </summary>
        protected override void AttemptDisconnection()
        {
            foreach (ProcessQueue<AFValue> archiveQueue in m_archiveQueues)
                archiveQueue.Stop();

            m_mapRequestQueue.Stop();
            m_mapRequestQueue.Clear();

            m_mappedPIPoints.Clear();

            lock (m_pendingMappings)
            {
                m_pendingMappings.Clear();
            }

            if (!(m_connection is null))
            {
                m_connection.Disconnected -= Connection_Disconnected;
                m_connection.Dispose();
                m_connection = null;
            }
        }

        /// <summary>
        /// Sorts measurements and sends them to the configured PI server in batches
        /// </summary>
        /// <param name="measurements">Measurements to queue</param>
        protected override void ProcessMeasurements(IMeasurement[] measurements)
        {
            if (measurements is null || measurements.Length <= 0 || m_connection is null)
                return;

            foreach (IMeasurement measurement in measurements)
            {
                // If adapter gets disabled while executing this thread - go ahead and exit
                if (!Enabled)
                    return;

                // Lookup connection point mapping for this measurement, if it wasn't found - go ahead and exit
                if (!m_mappedPIPoints.TryGetValue(measurement.Key, out PIPoint point))
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
                            if (!m_pendingMappings.Contains(measurement.Key))
                            {
                                mappingRequested = true;
                                m_pendingMappings.Add(measurement.Key);
                            }
                        }

                        if (mappingRequested)
                        {
                            lock (m_mapRequestQueue.SyncRoot)
                            {
                                // No mapping is defined for this point, queue up mapping
                                m_mapRequestQueue.Add(measurement.Key);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        OnProcessException(MessageLevel.Info, new InvalidOperationException($"Failed to associate measurement value with connection point for '{measurement.Key}': {ex.Message}", ex));
                    }
                }
                else
                {
                    try
                    {
                        // Verify maximum per point archive resolution
                        if (!(m_lastArchiveTimes is null) && (measurement.Timestamp - m_lastArchiveTimes.GetOrAdd(measurement.Key.SignalID, measurement.Timestamp)).ToSeconds() < MaximumPointResolution)
                            continue;

                        // Queue up insert operations for parallel processing
                        m_archiveQueues[point.ID % m_archiveQueues.Length].Add(
                            new AFValue((float)measurement.AdjustedValue, new AFTime(new DateTime(measurement.Timestamp, DateTimeKind.Utc)))
                            {
                                PIPoint = point
                            });


                        if (!(m_lastArchiveTimes is null))
                            m_lastArchiveTimes[measurement.Key.SignalID] = measurement.Timestamp;
                    }
                    catch (Exception ex)
                    {
                        OnProcessException(MessageLevel.Info, new InvalidOperationException($"Failed to collate measurement value into OSI-PI point value collection for '{measurement.Key}': {ex.Message}", ex));
                    }
                }
            }
        }

        private void ArchiveAFValues(AFValue[] values)
        {
            Ticks startTime = DateTime.UtcNow.Ticks;

            m_connection.Server.UpdateValues(values, UseCompression ? AFUpdateOption.Insert : AFUpdateOption.InsertNoCompression);

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

                            if (!(point is null))
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
                    foundPoint = !(point is null);
                }

                if (!foundPoint)
                {
                    // Lookup meta-data for current measurement
                    DataRow[] rows = DataSource.Tables["ActiveMeasurements"].Select($"SignalID='{signalID}'");

                    if (rows.Length > 0)
                    {
                        DataRow measurementRow = rows[0];
                        string tagName = measurementRow["PointTag"].ToNonNullString().Trim();

                        // Use alternate tag if one is defined
                        if (!string.IsNullOrWhiteSpace(measurementRow["AlternateTag"].ToString()) && !measurementRow["SignalType"].ToString().Equals("DIGI", StringComparison.OrdinalIgnoreCase))
                            tagName = measurementRow["AlternateTag"].ToString().Trim();

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

            MeasurementKey[] inputMeasurements = InputMeasurementKeys;
            int previousMeasurementReportingInterval = MeasurementReportingInterval;
            DateTime latestUpdateTime = DateTime.MinValue;

            m_refreshingMetadata = RunMetadataSync;

            // For first runs, don't report archived points until PI meta-data has been established
            MeasurementReportingInterval = 0;

            // Attempt to load tag-map from existing cache, if any
            LoadCachedTagMap();

            if (m_tagMap.Count > 0)
                MeasurementReportingInterval = previousMeasurementReportingInterval;

            // Establish initial connection point dictionary (much of the meta-data may already exist)
            EstablishPIPointDictionary(inputMeasurements);

            if (!RunMetadataSync)
            {
                MeasurementReportingInterval = previousMeasurementReportingInterval;
                return;
            }

            try
            {
                OnStatusMessage(MessageLevel.Info, "Beginning metadata refresh...");

                if (!(inputMeasurements is null) && inputMeasurements.Length > 0)
                {
                    PIServer server = m_connection.Server;
                    int processed = 0, total = inputMeasurements.Length;
                    AdoDataConnection database = null;
                    DataTable measurements = DataSource.Tables["ActiveMeasurements"];

                    foreach (MeasurementKey key in inputMeasurements)
                    {
                        // If adapter gets disabled while executing this thread - go ahead and exit
                        if (!Enabled)
                            return;

                        Guid signalID = key.SignalID;
                        DataRow[] rows = measurements.Select($"SignalID='{signalID}'");
                        string tagName;
                        bool createdNewTag = false;
                        bool refreshMetadata = false;

                        if (rows.Length <= 0)
                        {
                            m_tagMap.TryRemove(signalID, out tagName);
                            continue;
                        }

                        // Get matching measurement row
                        DataRow measurementRow = rows[0];

                        // Get tag-name as defined in meta-data, adjusting as needed
                        tagName = GetPITagName(measurementRow["PointTag"].ToNonNullString().Trim());

                        // Use alternate tag if one is defined - note that digitals are an exception since they use this field for special labeling
                        if (!string.IsNullOrWhiteSpace(measurementRow["AlternateTag"].ToString()) && !measurementRow["SignalType"].ToString().Equals("DIGI", StringComparison.OrdinalIgnoreCase))
                            tagName = measurementRow["AlternateTag"].ToString().Trim();

                        // If no tag name is defined in measurements there is no need to continue processing
                        if (string.IsNullOrWhiteSpace(tagName))
                        {
                            m_tagMap.TryRemove(signalID, out tagName);
                            continue;
                        }

                        // Lookup PI point trying signal ID and tag name
                        PIPoint point = GetPIPoint(server, signalID, tagName);

                        if (AutoCreateTags && point is null)
                        {
                            try
                            {
                                // Attempt to add point if it doesn't exist
                                Dictionary<string, object> attributes = new Dictionary<string, object>
                                {
                                    [PICommonPointAttributes.PointClassName] = PIPointClass,
                                    [PICommonPointAttributes.PointType] = PIPointType.Float32
                                };

                                point = server.CreatePIPoint(tagName, attributes);

                                refreshMetadata = true;
                                createdNewTag = true;
                            }
                            catch (Exception ex)
                            {
                                OnProcessException(MessageLevel.Warning, new InvalidOperationException($"Failed to add PI tag '{tagName}' for measurement '{key}': {ex.Message}", ex));
                            }
                        }

                        if (!(point is null))
                        {
                            // Update tag-map for faster future loads
                            m_tagMap[signalID] = tagName;

                            try
                            {
                                // Rename tag-name if needed - PI tags are not case sensitive
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
                                // Attempt to lookup last update time for record
                                if (database is null)
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

                        if ((refreshMetadata || updateTime > m_lastMetadataRefresh) && !(point is null))
                        {
                            try
                            {
                                List<string> updatedAttributes = new List<string>();

                                void updateAttribute(string attribute, object newValue)
                                {
                                    string oldValue = point.GetAttribute(attribute).ToString();

                                    if (string.IsNullOrEmpty(oldValue) || string.CompareOrdinal(oldValue, newValue.ToString()) != 0)
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
                                        PICommonPointAttributes.EngineeringUnits);

                                    // Update tag meta-data if it has changed
                                    updateAttribute(PICommonPointAttributes.PointSource, PIPointSource);
                                    updateAttribute(PICommonPointAttributes.Descriptor, measurementRow["Description"].ToString());
                                    updateAttribute(PICommonPointAttributes.ExtendedDescriptor, measurementRow["SignalID"].ToString());
                                    updateAttribute(PICommonPointAttributes.Tag, tagName);
                                    updateAttribute(PICommonPointAttributes.Compressing, UseCompression ? 1 : 0);

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

                        // If mapping for this connection was removed, it may have been because there was no meta-data so
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
                    OnStatusMessage(MessageLevel.Warning, "OSI-PI historian output adapter is not configured to receive any input measurements - metadata refresh canceled.");
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

                OnStatusMessage(MessageLevel.Info, "Completed metadata refresh successfully.");

                // Re-establish connection point dictionary since meta-data and tags may exist in PI server now that didn't before.
                // This will also start showing warning messages in CreateMappedPIPoint function for unmappable points
                // now that meta-data refresh has completed.
                EstablishPIPointDictionary(inputMeasurements);
            }

            // Restore original measurement reporting interval
            MeasurementReportingInterval = previousMeasurementReportingInterval;
        }

        // Resets the PI tag to MeasurementKey mapping for loading data into PI by finding PI points that match either the GSFSchema point tag or alternate tag
        private void EstablishPIPointDictionary(MeasurementKey[] inputMeasurements)
        {
            OnStatusMessage(MessageLevel.Info, "Establishing connection points for mapping...");

            List<MeasurementKey> newTags = new List<MeasurementKey>();

            if (!(inputMeasurements is null) && inputMeasurements.Length > 0)
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
                HashSet<MeasurementKey> tagsToRemove = new HashSet<MeasurementKey>(m_mappedPIPoints.Keys);

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
                using (FileStream tagMapCache = File.Open(TagMapCacheFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    m_tagMap.Merge(true, Serialization.Deserialize<Dictionary<Guid, string>>(tagMapCache, GSF.SerializationFormat.Binary));

                    OnStatusMessage(MessageLevel.Info, $"Loaded {m_tagMap.Count:N0} mappings from tag-map cache.");

                    // Use last write time of file as the last meta-data refresh time - rough value is OK
                    if (m_lastMetadataRefresh == DateTime.MinValue)
                        m_lastMetadataRefresh = File.GetLastWriteTimeUtc(TagMapCacheFileName);
                }
            }
            catch (Exception ex)
            {
                OnProcessException(MessageLevel.Info, new InvalidOperationException($"Failed to load mappings from cached tag-map to file '{TagMapCacheFileName}': {ex.Message}", ex));
            }
        }

        private static PIPoint GetPIPoint(PIServer server, string tagName)
        {
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

                if (!(point is null))
                    cachedTagName = point.Name;
            }

            return point;
        }

        private string GetPITagName(string tagName)
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

        // Since we may get a plethora of these requests, we use a synchronized operation to restart once
        private void Connection_Disconnected(object sender, EventArgs e) => 
            m_restartConnection.RunOnceAsync();

        #endregion

        #region [ Static ]

        /// <summary>
        /// Accesses local output adapter instances (normally only one).
        /// </summary>
        public static readonly ConcurrentDictionary<string, PIOutputAdapter> Instances = new ConcurrentDictionary<string, PIOutputAdapter>(StringComparer.OrdinalIgnoreCase);

        #endregion
    }
}