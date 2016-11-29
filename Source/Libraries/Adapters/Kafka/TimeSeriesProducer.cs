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
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using GSF;
using GSF.Collections;
using GSF.Data;
using GSF.Diagnostics;
using GSF.Threading;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using Misakai.Kafka;

namespace KafkaAdapters
{
    /// <summary>
    /// Represents a Kafka adapter that will produce time-series data based on output from the Time-Series Library.
    /// </summary>
    [Description("Kafka Producer: Sends time-series data to Kafka")]
    public class TimeSeriesProducer : OutputAdapterBase // Consume from TSL, Produce to Kafka
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Defines the default value for the <see cref="Topic"/> property.
        /// </summary>
        public const string DefaultTopic = "GSF";

        /// <summary>
        /// Defines the default value for the <see cref="Partitions"/> property.
        /// </summary>
        public const int DefaultPartitions = 1;

        /// <summary>
        /// Defines the default value for the <see cref="Encoding"/> property.
        /// </summary>
        public const Encoding DefaultEncoding = null;

        /// <summary>
        /// Defines the default value for the <see cref="SerializeMetadata"/> property.
        /// </summary>
        public const bool DefaultSerializeMetadata = true;

        /// <summary>
        /// Defines the default value for the <see cref="TimestampFormat"/> property.
        /// </summary>
        public const string DefaultTimestampFormat = "yyyy-MM-dd HH:mm:ss.fffffff";

        /// <summary>
        /// Defines the default value for the <see cref="ValueFormat"/> property.
        /// </summary>
        public const string DefaultValueFormat = "0.######";

        /// <summary>
        /// Defines the default value for the <see cref="CacheMetadataLocally"/> property.
        /// </summary>
        public const bool DefaultCacheMetadataLocally = false;

        // Fields
        private Uri[] m_servers;
        private BrokerRouter m_router;
        private Producer m_producer;
        private TimeSeriesMetadata m_metadata;
        private long m_metadataUpdateCount;
        private LongSynchronizedOperation m_cacheMetadataLocally;
        private Encoding m_encoding;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="TimeSeriesProducer"/> instance.
        /// </summary>
        public TimeSeriesProducer()
        {
            DefaultEventName = "KafkaProducer";
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
        [ConnectionStringParameter, Description("Defines the Kafka topic name. Defaults to \"GSF\"."), DefaultValue(DefaultTopic)]
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
        [ConnectionStringParameter, Description("Defines the total number of partitions defined for distribution of measurement data. The measurement key ID will be used to target a particular partition (ID % Partitions)."), DefaultValue(DefaultPartitions)]
        public int Partitions
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the encoding used to serialize measurements into the Kafka stream. Default value of null defines binary encoding.
        /// </summary>
        [ConnectionStringParameter, Description("Defines the encoding used to serialize measurements."), DefaultValue(DefaultEncoding)]
        public string Encoding
        {
            get
            {
                if ((object)m_encoding != null)
                    return m_encoding.EncodingName;

                return null;
            }
            set
            {
                if ((object)value != null)
                    m_encoding = System.Text.Encoding.GetEncoding(value);
                else
                    m_encoding = null;
            }
        }
        
        /// <summary>
        /// Gets or sets the text format for measurement timestamps.
        /// </summary>
        [ConnectionStringParameter, Description("Defines the text format for measurement timestamps."), DefaultValue(DefaultTimestampFormat)]
        public string TimestampFormat
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the text format for measurement values.
        /// </summary>
        [ConnectionStringParameter, Description("Defines the text format for measurement values."), DefaultValue(DefaultValueFormat)]
        public string ValueFormat
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets flag that determines if metadata should be serialized into Kafka.
        /// </summary>
        [ConnectionStringParameter, Description("Determines if metadata should be serialized into Kafka."), DefaultValue(DefaultSerializeMetadata)]
        public bool SerializeMetadata
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets flag that determines if metadata should cached locally.
        /// </summary>
        [ConnectionStringParameter, Description("Determines if metadata should be cached locally."), DefaultValue(DefaultCacheMetadataLocally)]
        public bool CacheMetadataLocally
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the flag that determines if measurements sent to this <see cref="OutputAdapterBase"/> are destined for archival.
        /// </summary>
        /// <remarks>
        /// This property allows the <see cref="OutputAdapterCollection"/> to calculate statistics on how many measurements have
        /// been archived per minute. Historians would normally set this property to <c>true</c>; other custom exports would set
        /// this property to <c>false</c>.
        /// </remarks>
        public override bool OutputIsForArchive => true;

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
                status.AppendFormat("       Producer topic name: {0}", Topic ?? "[undefined]");
                status.AppendLine();
                status.AppendFormat("        Defined partitions: {0:N0}", Partitions);
                status.AppendLine();
                status.AppendFormat("  Caching metadata locally: {0}", CacheMetadataLocally);
                status.AppendLine();
                status.AppendFormat("    Metadata serialization: {0}", SerializeMetadata ? "Enabled" : "Disabled");
                status.AppendLine();

                if (SerializeMetadata)
                {
                    status.AppendFormat("       Metadata topic name: {0}", MetadataTopic);
                    status.AppendLine();
                    status.AppendFormat("          Metadata records: {0}", m_metadata?.Count.ToString("N0") ?? "Waiting for metadata...");
                    status.AppendLine();
                    status.AppendFormat("     Metadata update count: {0:N0}", m_metadataUpdateCount);
                    status.AppendLine();
                }

                if ((object)m_producer != null)
                {
                    status.AppendFormat("Active producer call count: {0:N0}", m_producer.ActiveCount);
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
            int value;

            // Parse required settings
            if (!settings.TryGetValue(nameof(Servers), out setting) || string.IsNullOrWhiteSpace(setting))
                throw new ArgumentException($"Required \"{nameof(Servers)}\" setting is missing.");

            Servers = setting.Trim();
            m_servers = Servers.Split(',').Select(uri => new Uri(uri)).ToArray();

            // Parse optional settings
            if (settings.TryGetValue(nameof(Topic), out setting) && !string.IsNullOrWhiteSpace(setting))
                Topic = setting.Trim();
            else
                Topic = DefaultTopic;

            if (settings.TryGetValue(nameof(Partitions), out setting) && int.TryParse(setting, out value))
                Partitions = value;
            else
                Partitions = DefaultPartitions;

            if (settings.TryGetValue(nameof(Encoding), out setting))
                Encoding = setting;
            else
                Encoding = null;

            if (settings.TryGetValue(nameof(TimestampFormat), out setting))
                TimestampFormat = setting;
            else
                TimestampFormat = DefaultTimestampFormat;

            if (settings.TryGetValue(nameof(ValueFormat), out setting))
                ValueFormat = setting;
            else
                ValueFormat = DefaultValueFormat;

            if (settings.TryGetValue(nameof(SerializeMetadata), out setting))
                SerializeMetadata = setting.ParseBoolean();
            else
                SerializeMetadata = DefaultSerializeMetadata;

            if (settings.TryGetValue(nameof(CacheMetadataLocally), out setting))
                CacheMetadataLocally = setting.ParseBoolean();
            else
                CacheMetadataLocally = DefaultCacheMetadataLocally;

            if (CacheMetadataLocally)
                m_cacheMetadataLocally = new LongSynchronizedOperation(() => TimeSeriesMetadata.CacheLocally(m_metadata, MetadataTopic, status => OnStatusMessage(MessageLevel.Info, status))) { IsBackground = true };
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
            m_router = new BrokerRouter(new KafkaOptions(m_servers)
            {
                Log = new TimeSeriesLogger
                (
                    (status, args) => OnStatusMessage(MessageLevel.Info, string.Format(status, args)), 
                    ex => OnProcessException(MessageLevel.Warning, ex)
                )
            });
            m_producer = new Producer(m_router);
            MetadataRefreshOperation.RunOnceAsync();
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
            if ((object)m_producer != null)
            {
                m_producer.Dispose();
                m_producer = null;
            }

            if ((object)m_router != null)
            {
                m_router.Dispose();
                m_router = null;
            }
        }

        /// <summary>
        /// Gets a short one-line status of this <see cref="AdapterBase"/>.
        /// </summary>
        /// <param name="maxLength">Maximum number of available characters for display.</param>
        /// <returns>A short one-line summary of the current status of this <see cref="AdapterBase"/>.</returns>
        public override string GetShortStatus(int maxLength)
        {
            return $"Sent {ProcessedMeasurements:N0} measurements to Kafka so far...".CenterText(maxLength);
        }

        /// <summary>
        /// Executes the metadata refresh in a synchronous fashion.
        /// </summary>
        protected override void ExecuteMetadataRefresh()
        {
            if (!Initialized || !Enabled || !SerializeMetadata)
                return;

            try
            {
                using (BrokerRouter router = new BrokerRouter(new KafkaOptions(m_servers)
                {
                    Log = new TimeSeriesLogger
                    (
                        (status, args) => OnStatusMessage(MessageLevel.Info, string.Format(status, args)),
                        ex => OnProcessException(MessageLevel.Warning, new InvalidOperationException($"[{MetadataTopic}]: {ex.Message}", ex))
                    )
                }))
                {
                    // Attempt to retrieve last known metadata record from Kafka
                    if ((object)m_metadata == null)
                    {
                        try
                        {
                            Ticks serializationTime;

                            OnStatusMessage(MessageLevel.Info, "Reading latest time-series metadata records from Kafka...");

                            m_metadata = TimeSeriesMetadata.ReadFromKafka(router, MetadataTopic, status => OnStatusMessage(MessageLevel.Info, status), out serializationTime);

                            OnStatusMessage(MessageLevel.Info, $"Deserialized {m_metadata.Count:N0} Kafka time-series metadata records, version {m_metadata.Version:N0}, from \"{MetadataTopic}\" serialized at {serializationTime.ToString(MetadataRecord.DateTimeFormat)}");
                        }
                        catch (Exception ex)
                        {
                            OnStatusMessage(MessageLevel.Warning, $"WARNING: Failed to read any existing Kafka time-series metadata records from topic \"{MetadataTopic}\": {ex.Message}");
                        }
                    }

                    // Create new meta-data object based on newly loaded configuration
                    TimeSeriesMetadata metadata = new TimeSeriesMetadata();

                    try
                    {
                        foreach (DataRow row in DataSource.Tables["ActiveMeasurements"].AsEnumerable())
                        {
                            MeasurementKey key;

                            if (MeasurementKey.TryParse(row.Field<string>("ID") ?? MeasurementKey.Undefined.ToString(), out key))
                            {
                                metadata.Records.Add(new MetadataRecord
                                {
                                    ID = key.ID,
                                    Source = key.Source,
                                    UniqueID = row.Field<object>("SignalID").ToString(),
                                    PointTag = row.Field<string>("PointTag"),
                                    Device = row.Field<string>("Device"),
                                    Longitude = row.ConvertField("Longitude", 0.0F),
                                    Latitude = row.ConvertField("Latitude", 0.0F),
                                    Protocol = row.Field<string>("Protocol"),
                                    SignalType = row.Field<string>("SignalType"),
                                    EngineeringUnits = row.Field<string>("EngineeringUnits"),
                                    PhasorType = row.Field<string>("PhasorType"),
                                    Phase = row.Field<string>("Phase"),
                                    Description = row.Field<string>("Description"),
                                    LastUpdate = row.Field<DateTime>("UpdatedOn").ToString(MetadataRecord.DateTimeFormat)
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        OnProcessException(MessageLevel.Warning, new InvalidOperationException($"Failed to serialize current time-series metadata records: {ex.Message}", ex));
                    }

                    if (metadata.Count > 0)
                    {
                        // See if metadata has not been created yet or is different from last known Kafka record
                        if ((object)m_metadata == null || m_metadata.CalculateChecksum() != metadata.CalculateChecksum())
                        {
                            // Update local metadata reference
                            m_metadata = metadata;

                            // Send updated metadata to Kafka
                            TimeSeriesMetadata.WriteToKafka(m_metadata, router, MetadataTopic);

                            // Cache metadata locally, if configured
                            m_cacheMetadataLocally?.RunOnceAsync();

                            m_metadataUpdateCount++;

                            OnStatusMessage(MessageLevel.Info, $"Updated \"{MetadataTopic}\" with {m_metadata.Count:N0} Kafka time-series metadata records...");
                        }
                        else
                        {
                            OnStatusMessage(MessageLevel.Info, $"Latest \"{MetadataTopic}\" is up to date with current time-series metadata records...");
                        }
                    }
                    else
                    {
                        OnStatusMessage(MessageLevel.Info, "WARNING: No available local time-series metadata available to serialize...");
                    }
                }
            }
            catch (Exception ex)
            {
                OnProcessException(MessageLevel.Warning, new InvalidOperationException($"Failed to update \"{MetadataTopic}\" with current time-series metadata records: {ex.Message}", ex));
            }
        }

        /// <summary>
        /// Serializes measurements to data output stream.
        /// </summary>
        protected override async void ProcessMeasurements(IMeasurement[] measurements)
        {
            try
            {
                List<Message> messages = new List<Message>(measurements.Length);

                foreach (IMeasurement measurement in measurements)
                {
                    Message message = new Message { PartitionId = (int)(measurement.Key.ID % Partitions) };

                    if ((object)m_encoding != null)
                        KafkaTextEncode(measurement, message, MetadataVersion);
                    else
                        measurement.KakfaSerialize(message, MetadataVersion);

                    messages.Add(message);
                }

                await m_producer.SendMessageAsync(Topic, messages);
            }
            catch (Exception ex)
            {
                OnProcessException(MessageLevel.Warning, new InvalidOperationException($"Exception while sending Kafka messages for topic \"{Topic}\": {ex.Message}", ex));

                try
                {
                    Start();
                }
                catch (Exception ex2)
                {
                    OnStatusMessage(MessageLevel.Warning, $"WARNING: Exception occurred while attempting to restart adapter from the ProcessMeasurements handler: {ex2.Message}");
                }
            }
        }

        private void KafkaTextEncode(IMeasurement measurement, Message message, byte metadataVersion)
        {
            if ((object)measurement == null)
                throw new ArgumentNullException(nameof(measurement));

            if ((object)message == null)
                throw new ArgumentNullException(nameof(message));

            // Copy in "key" for Kafka message, for a time-series entity, the "timestamp" and "ID"
            string timeString = measurement.Timestamp.ToString(TimestampFormat, CultureInfo.InvariantCulture);
            string idString = measurement.Key.ID.ToString();
            string messageKeyString = string.Concat(timeString, ",", idString);

            // Copy in "value" for Kafka message, for a time-series entity, the "version", "quality" and "value"
            string versionString = metadataVersion.ToString();
            string qualityString = ((byte)((int)measurement.StateFlags.DeriveQualityFlags() | (SerializationExtensions.DecodeVersion << 2))).ToString();
            string valueString = measurement.AdjustedValue.ToString(ValueFormat, CultureInfo.InvariantCulture);
            string messageValueString = string.Join(",", versionString, qualityString, valueString);

            // Text-encode the key and value
            message.Key = m_encoding.GetBytes(messageKeyString);
            message.Value = m_encoding.GetBytes(messageValueString);
        }

        #endregion
    }
}
