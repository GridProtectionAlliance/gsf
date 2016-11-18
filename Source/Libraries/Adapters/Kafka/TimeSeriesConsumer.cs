//******************************************************************************************************
//  Producer.cs - Gbtc
//
//  Copyright © 2015, Grid Protection Alliance.  All Rights Reserved.
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
//  10/20/2015 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using GSF;
using GSF.Collections;
using GSF.IO;
using GSF.Threading;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using Misakai.Kafka;

namespace KafkaAdapters
{
    /// <summary>
    /// Represents a Kafka adapter that will receive time-series data to send into the Time-Series Library.
    /// </summary>
    [Description("Kafka Consumer: Receives time-series data from Kafka")]
    public class TimeSeriesConsumer : InputAdapterBase // Consume from Kafka, Produce to TSL
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Defines the default value for the <see cref="TrackConsumerOffset"/> property.
        /// </summary>
        public const bool DefaultTrackConsumerIndex = true;

        /// <summary>
        /// Defines the default value for the <see cref="ConsumerOffsetCacheInterval"/> property.
        /// </summary>
        public const double DefaultConsumerOffsetCacheInterval = 1.0D;

        /// <summary>
        /// Defines the default value for the <see cref="ReadDelay"/> property.
        /// </summary>
        public const int DefaultReadDelay = 33;

        // Fields
        private Uri[] m_servers;
        private BrokerRouter m_router;
        private Thread[] m_processingThreads;
        private readonly List<WeakReference<Consumer>> m_consumers;
        private readonly LongSynchronizedOperation m_updateMetadata;
        private HashSet<MeasurementKey> m_outputMeasurementKeys;
        private TimeSeriesMetadata m_metadata;
        private volatile byte m_lastMetadataVersion;
        private long m_metadataUpdateCount;
        private LongSynchronizedOperation m_cacheMetadataLocally;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="TimeSeriesConsumer"/> instance.
        /// </summary>
        public TimeSeriesConsumer()
        {
            m_consumers = new List<WeakReference<Consumer>>();
            m_updateMetadata = new LongSynchronizedOperation(UpdateMetadata, OnProcessException) { IsBackground = true };
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets Kafka servers to connect to, comma separated.
        /// </summary>
        [ConnectionStringParameter, Description("Defines comma separated list of Kafka server URIs, e.g.: http://kafka1:9092, http://kafka2:9092")]
        public string Servers
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the Kafka topic name.
        /// </summary>
        [ConnectionStringParameter, Description("Defines the Kafka topic name. Defaults to \"GSF\"."), DefaultValue(TimeSeriesProducer.DefaultTopic)]
        public string Topic
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the Kafka metadata topic name.
        /// </summary>
        public string MetadataTopic => $"{Topic ?? "[undefined]"}-metadata";

        /// <summary>
        /// Gets or sets the total number of partitions used for data distribution.
        /// </summary>
        [ConnectionStringParameter, Description("Defines the total number of partitions defined for distribution of measurement data. One thread will be created per partition to consume data."), DefaultValue(TimeSeriesProducer.DefaultPartitions)]
        public int Partitions
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets flag that determines if consumer offset should be tracked between adapter instantiations.
        /// </summary>
        [ConnectionStringParameter, Description("Defines flag that determines if consumer offset should be tracked between adapter instantiations. If true, will start after last item read during previous run; otherwise, will start read from beginning of partition."), DefaultValue(DefaultTrackConsumerIndex)]
        public bool TrackConsumerOffset
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets file name to use for consumer offset base file name.
        /// </summary>
        [ConnectionStringParameter, Description("Defines value base file name to use for tracking the last consumer offset. Leave blank for cache name to be same as adapter name with an \".offset\" extension. Note that file name actually used will be suffixed with partition index."), DefaultValue("")]
        public string ConsumerOffsetFileName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or set minimum cache interval for consumer offset tracking.
        /// </summary>
        [ConnectionStringParameter, Description("Defines the minimum interval, in floating-point seconds, for caching the tracked consumer offset."), DefaultValue(DefaultConsumerOffsetCacheInterval)]
        public double ConsumerOffsetCacheInterval
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets read delay, in milliseconds, for messages with timestamp changes. Set to -1 for no delay, i.e., read as fast as possible.
        /// </summary>
        [ConnectionStringParameter, Description("Defines the read delay, in milliseconds, i.e., the amount of time to wait between messages with timestamp changes. Set value to -1 to read messages as fast as possible, i.e., no delay."), DefaultValue(DefaultReadDelay)]
        public int ReadDelay
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets flag that determines if metadata should cached locally.
        /// </summary>
        [ConnectionStringParameter, Description("Determines if metadata received from Kafka should be cached locally."), DefaultValue(TimeSeriesProducer.DefaultCacheMetadataLocally)]
        public bool CacheMetadataLocally
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the flag indicating if this adapter supports temporal processing.
        /// </summary>
        public override bool SupportsTemporalProcessing => false;

        /// <summary>
        /// Gets flag that determines if the data output stream connects asynchronously.
        /// </summary>
        /// <remarks>
        /// Derived classes should return true when data output stream connects asynchronously, otherwise return false.
        /// </remarks>
        protected override bool UseAsyncConnect => false;

        /// <summary>
        /// Returns the detailed status of the data input source.  Derived classes should extend status with implementation specific information.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.Append(base.Status);
                status.AppendLine();
                status.AppendFormat("         Kafka server URIs: {0}", m_servers?.ToDelimitedString(", ") ?? "Undefined");
                status.AppendLine();
                status.AppendFormat("       Consumer topic name: {0}", Topic ?? "[undefined]");
                status.AppendLine();
                status.AppendFormat("       Metadata topic name: {0}", MetadataTopic);
                status.AppendLine();
                status.AppendFormat("        Defined partitions: {0:N0}", Partitions);
                status.AppendLine();
                status.AppendFormat("        Message read delay: {0}", ReadDelay > -1 ? ReadDelay.ToString("N0") + "ms per timestamp group" : "As fast as possible");
                status.AppendLine();
                status.AppendFormat("  Caching metadata locally: {0}", CacheMetadataLocally);
                status.AppendLine();
                status.AppendFormat("          Metadata records: {0}", m_metadata?.Count.ToString("N0") ?? "Waiting for metadata...");
                status.AppendLine();
                status.AppendFormat("     Metadata update count: {0:N0}", m_metadataUpdateCount);
                status.AppendLine();
                status.AppendFormat("  Tracking consumer offset: {0}", TrackConsumerOffset);
                status.AppendLine();

                if (TrackConsumerOffset)
                {
                    status.AppendFormat(" Consumer offset file name: {0}", FilePath.TrimFileName(ConsumerOffsetFileName.ToNonNullString("[undefined]"), 51));
                    status.AppendLine();
                    status.AppendFormat("     Offset cache interval: {0:N2} seconds", ConsumerOffsetCacheInterval);
                    status.AppendLine();
                }

                if ((object)m_consumers != null)
                {
                    lock (m_consumers)
                        status.AppendFormat("Active consumer task count: {0:N0}", m_consumers.Sum(cref =>
                        {
                            Consumer consumer;
                            return cref.TryGetTarget(out consumer) ? consumer.ConsumerTaskCount : 0;
                        }));
                    status.AppendLine();
                }

                return status.ToString();
            }
        }

        // Derives meta-data version - byte value reduces context to last 256 changes to minimize message serialization size
        private byte MetadataVersion => (byte)((m_metadata?.Version ?? 0) % byte.MaxValue);

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes <see cref="TimeSeriesProducer"/>.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            Dictionary<string, string> settings = Settings;
            string setting;
            int intValue;
            double doubleValue;

            // Parse required settings
            if (!settings.TryGetValue(nameof(Servers), out setting) || string.IsNullOrWhiteSpace(setting))
                throw new ArgumentException($"Required \"{nameof(Servers)}\" setting is missing.");

            Servers = setting.Trim();
            m_servers = Servers.Split(',').Select(uri => new Uri(uri)).ToArray();

            // Parse optional settings
            if (settings.TryGetValue(nameof(Topic), out setting) && !string.IsNullOrWhiteSpace(setting))
                Topic = setting.Trim();
            else
                Topic = TimeSeriesProducer.DefaultTopic;

            if (settings.TryGetValue(nameof(Partitions), out setting) && int.TryParse(setting, out intValue))
                Partitions = intValue;
            else
                Partitions = TimeSeriesProducer.DefaultPartitions;

            if (settings.TryGetValue(nameof(TrackConsumerOffset), out setting))
                TrackConsumerOffset = setting.ParseBoolean();
            else
                TrackConsumerOffset = DefaultTrackConsumerIndex;

            if (!settings.TryGetValue(nameof(ConsumerOffsetFileName), out setting) || string.IsNullOrWhiteSpace(setting))
                setting = Name + ".offset";

            ConsumerOffsetFileName = FilePath.GetAbsolutePath(setting);

            if (settings.TryGetValue(nameof(ConsumerOffsetCacheInterval), out setting) && double.TryParse(setting, out doubleValue))
                ConsumerOffsetCacheInterval = doubleValue;
            else
                ConsumerOffsetCacheInterval = DefaultConsumerOffsetCacheInterval;

            if (settings.TryGetValue(nameof(ReadDelay), out setting) && int.TryParse(setting, out intValue))
                ReadDelay = intValue;
            else
                ReadDelay = DefaultReadDelay;

            if (settings.TryGetValue(nameof(CacheMetadataLocally), out setting))
                CacheMetadataLocally = setting.ParseBoolean();
            else
                CacheMetadataLocally = TimeSeriesProducer.DefaultCacheMetadataLocally;

            if (CacheMetadataLocally)
                m_cacheMetadataLocally = new LongSynchronizedOperation(() => TimeSeriesMetadata.CacheLocally(m_metadata, MetadataTopic, OnStatusMessage)) { IsBackground = true };

            if ((object)OutputMeasurements != null && OutputMeasurements.Length > 0)
                m_outputMeasurementKeys = new HashSet<MeasurementKey>(OutputMeasurements.Select(m => m.Key));
        }

        /// <summary>
        /// Attempts to connect to data output stream.
        /// </summary>
        /// <remarks>
        /// Derived classes should attempt connection to data output stream here.  Any exceptions thrown
        /// by this implementation will result in restart of the connection cycle.
        /// </remarks>
        protected override void AttemptConnection()
        {
            m_router = new BrokerRouter(new KafkaOptions(m_servers) { Log = new TimeSeriesLogger(OnStatusMessage, OnProcessException) });

            m_processingThreads = new Thread[Partitions];

            for (int partition = 0; partition < m_processingThreads.Length; partition++)
            {
                m_processingThreads[partition] = new Thread(ProcessPartitionMessages) { IsBackground = true };
                m_processingThreads[partition].Start(partition);
            }

            // Kick off process to update metadata
            m_updateMetadata.RunOnceAsync();
        }

        /// <summary>
        /// Attempts to disconnect from data output stream.
        /// </summary>
        /// <remarks>
        /// Derived classes should attempt disconnect from data output stream here.  Any exceptions thrown
        /// by this implementation will be reported to host via <see cref="AdapterBase.ProcessException"/> event.
        /// </remarks>
        protected override void AttemptDisconnection()
        {
            if ((object)m_processingThreads != null)
            {
                foreach (Thread processingThread in m_processingThreads)
                    processingThread.Abort();

                m_processingThreads = null;
            }

            if ((object)m_router != null)
            {
                m_router.Dispose();
                m_router = null;
            }

            lock (m_consumers)
                m_consumers.Clear();
        }

        /// <summary>
        /// Gets a short one-line status of this <see cref="AdapterBase"/>.
        /// </summary>
        /// <param name="maxLength">Maximum number of available characters for display.</param>
        /// <returns>A short one-line summary of the current status of this <see cref="AdapterBase"/>.</returns>
        public override string GetShortStatus(int maxLength)
        {
            return $"Received {ProcessedMeasurements:N0} measurements from Kafka so far...".CenterText(maxLength);
        }

        // Per partition consumer read handler
        private void ProcessPartitionMessages(object state)
        {
            int partition = (int)state;

            try
            {
                Dictionary<uint, MeasurementKey> idTable = new Dictionary<uint, MeasurementKey>();
                ConsumerOptions options = new ConsumerOptions(Topic, m_router);
                LongSynchronizedOperation cacheLastConsumerOffset = null;
                OffsetPosition consumerCursor = new OffsetPosition { PartitionId = partition, Offset = 0 };
                long lastUpdateTime = 0;
                long lastMetadataUpdateCount = 0;
                long lastMeasurementTime = 0;

                options.PartitionWhitelist.Add(partition);
                options.Log = new TimeSeriesLogger((message, parameters) => OnStatusMessage($"P[{partition}]: " + message, parameters), OnProcessException);

                // Handle consumer offset tracking, i.e., adapter will start reading messages where it left off from last run
                if (TrackConsumerOffset)
                {
                    // Parse path/filename.ext into constituent parts
                    string[] fileParts = new string[3];

                    fileParts[0] = FilePath.GetDirectoryName(ConsumerOffsetFileName);               // 0: path/
                    fileParts[1] = FilePath.GetFileNameWithoutExtension(ConsumerOffsetFileName);    // 1: filename
                    fileParts[2] = FilePath.GetExtension(ConsumerOffsetFileName);                   // 2: .ext

                    // Include partition index as part of consumer offset cache file name
                    string fileName = $"{fileParts[0]}{fileParts[1]}-P{partition}{fileParts[2]}";

                    if (File.Exists(fileName))
                    {
                        try
                        {
                            // Read last consumer offset
                            consumerCursor.Offset = long.Parse(File.ReadAllText(fileName));
                        }
                        catch (Exception ex)
                        {
                            OnProcessException(new InvalidOperationException($"Failed to read last consumer offset from \"{fileName}\": {ex.Message}", ex));
                        }
                    }

                    cacheLastConsumerOffset = new LongSynchronizedOperation(() =>
                    {
                        // Do not write file any more often than defined consumer offset cache interval
                        int restTime = (int)(Ticks.FromSeconds(ConsumerOffsetCacheInterval) - (DateTime.UtcNow.Ticks - lastUpdateTime)).ToMilliseconds();

                        if (restTime > 0)
                            Thread.Sleep(restTime);

                        lastUpdateTime = DateTime.UtcNow.Ticks;

                        // Write current consumer offset
                        File.WriteAllText(fileName, consumerCursor.Offset.ToString());
                    }, 
                    ex => OnProcessException(new InvalidOperationException($"Failed to cache current consumer offset to \"{fileName}\": {ex.Message}", ex)))
                    {
                        IsBackground = true
                    };
                }

                using (Consumer consumer = new Consumer(options, new OffsetPosition(partition, consumerCursor.Offset)))
                {
                    lock (m_consumers)
                        m_consumers.Add(new WeakReference<Consumer>(consumer));

                    foreach (Message message in consumer.Consume())
                    {
                        if ((object)m_metadata == null)
                            continue;

                        uint id;
                        byte metadataVersion;
                        IMeasurement measurement = message.KafkaDeserialize(out id, out metadataVersion);

                        // Kick-off a refresh for new metadata if message version numbers change
                        if (m_lastMetadataVersion != metadataVersion)
                        {
                            m_lastMetadataVersion = metadataVersion;
                            m_updateMetadata.RunOnceAsync();
                        }

                        // Clear all undefined items in dictionary when metadata gets updated
                        if (lastMetadataUpdateCount < m_metadataUpdateCount)
                        {
                            lastMetadataUpdateCount = m_metadataUpdateCount;
                            foreach (uint undefinedID in idTable.Where(item => item.Value.SignalID == Guid.Empty).Select(item => item.Key).ToArray())
                                idTable.Remove(undefinedID);
                        }

                        // Get associated measurement key, or look it up in metadata table
                        measurement.CommonMeasurementFields = idTable.GetOrAdd(id, lookupID => MeasurementKey.LookUpBySignalID(m_metadata?.Records?.FirstOrDefault(record => record.ID == lookupID)?.ParseSignalID() ?? Guid.Empty)).CommonMeasurementFields;

                        // Only publish measurements with associated metadata and are assigned to this adapter
                        if (measurement.Key != MeasurementKey.Undefined && ((object)m_outputMeasurementKeys == null || m_outputMeasurementKeys.Contains(measurement.Key)))
                            OnNewMeasurements(new[] { measurement });

                        // Cache last consumer offset
                        consumerCursor.Offset = message.Offset;

                        if ((object)cacheLastConsumerOffset != null)
                            cacheLastConsumerOffset.RunOnceAsync();

                        if (ReadDelay > -1)
                        {
                            // As a group of measurements transition from timestamp to another, inject configured read delay
                            if (lastMeasurementTime != measurement.Timestamp)
                                Thread.Sleep(ReadDelay);

                            lastMeasurementTime = measurement.Timestamp;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                OnProcessException(new InvalidOperationException($"Exception while reading Kafka messages for topic \"{Topic}\" P[{partition}]: {ex.Message}", ex));
            }
        }

        // Update metadata from latest Kafka records
        private void UpdateMetadata()
        {
            // Attempt to retrieve last known metadata record from Kafka
            try
            {
                using (BrokerRouter router = new BrokerRouter(new KafkaOptions(m_servers) { Log = new TimeSeriesLogger(OnStatusMessage, ex => OnProcessException(new InvalidOperationException($"[{MetadataTopic}]: {ex.Message}", ex))) }))
                {
                    Ticks serializationTime;

                    OnStatusMessage("Reading latest time-series metadata records from Kafka...");

                    TimeSeriesMetadata metadata = TimeSeriesMetadata.ReadFromKafka(router, MetadataTopic, OnStatusMessage, out serializationTime);

                    if ((object)metadata != null)
                    {
                        m_metadata = metadata;

                        OnStatusMessage($"Deserialized {m_metadata.Count:N0} Kafka time-series metadata records, version {m_metadata.Version:N0}, from \"{MetadataTopic}\" serialized at {serializationTime.ToString(MetadataRecord.DateTimeFormat)}");

                        if (m_lastMetadataVersion != MetadataVersion)
                        {
                            m_lastMetadataVersion = MetadataVersion;
                            m_metadataUpdateCount++;
                        }

                        // Cache metadata locally, if configured
                        m_cacheMetadataLocally?.RunOnceAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                if (Enabled)
                {
                    // Treat exception as a warning if metadata already exists
                    if ((object)m_metadata == null)
                        throw;

                    OnStatusMessage($"WARNING: Failed to read latest Kafka time-series metadata records from topic \"{MetadataTopic}\": {ex.Message}");
                }
            }
        }

        #endregion
    }
}
