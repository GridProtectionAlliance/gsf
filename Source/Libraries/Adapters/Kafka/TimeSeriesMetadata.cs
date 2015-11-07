//******************************************************************************************************
//  TimeSeriesMetadata.cs - Gbtc
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
//  10/28/2015 - Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using GSF;
using GSF.Configuration;
using GSF.IO;
using Misakai.Kafka;

namespace KafkaAdapters
{
    /// <summary>
    /// Represents a set of Kafka time-series metadata.
    /// </summary>
    [XmlRoot("TimeSeriesMetadata", Namespace="http://www.gridprotectionalliance.org/gsf/KafkaAdapters", IsNullable=false)]
    public class TimeSeriesMetadata
    {
        #region [ Members ]

        // Constants
        private const int SizeMessage = 0;
        private const int ValueMessage = 1;

        // Fields

        /// <summary>
        /// Defines the version of this time-series metadata instance.
        /// </summary>       
        public long Version;

        /// <summary>
        /// Defines the records of this time-series metadata instance.
        /// </summary>
        [XmlArrayItem("Record")]
        public List<MetadataRecord> Records = new List<MetadataRecord>();

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the count of records in the time-series metadata.
        /// </summary>
        public int Count => Records?.Count ?? 0;

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Calculates a CRC-32 based check-sum on this <see cref="TimeSeriesMetadata"/> instance.
        /// </summary>
        /// <returns>CRC-32 based check-sum on this <see cref="TimeSeriesMetadata"/> instance.</returns>
        public long CalculateChecksum()
        {
            return Records.Sum(record => record.CalculateChecksum());
        }

        #endregion

        #region [ Static ]

        // Static Methods

        /// <summary>
        /// Serializes <see cref="TimeSeriesMetadata"/> instance to a stream of XML.
        /// </summary>
        /// <param name="metadata">Source time-series metadata object to serialize.</param>
        /// <param name="serializationStream">Destination stream to hold serialized data.</param>
        /// <param name="incrementVersion">Determines of metadata version should be incremented.</param>
        public static void Serialize(TimeSeriesMetadata metadata, Stream serializationStream, bool incrementVersion)
        {
            if ((object)metadata == null)
                throw new ArgumentNullException(nameof(metadata));

            if ((object)serializationStream == null)
                throw new ArgumentNullException(nameof(serializationStream));

            // Increment serialization version
            if (incrementVersion)
                metadata.Version++;

            XmlSerializer serializer = new XmlSerializer(typeof(TimeSeriesMetadata));
            using (TextWriter writer = new StreamWriter(serializationStream, Encoding.UTF8, 8192, true))
                serializer.Serialize(writer, metadata);
        }

        /// <summary>
        /// Deserializes a stream of XML metadata as a <see cref="TimeSeriesMetadata"/> instance.
        /// </summary>
        /// <param name="serializationStream">Source stream of serialized data.</param>
        /// <returns>Deserialized <see cref="TimeSeriesMetadata"/> instance.</returns>
        public static TimeSeriesMetadata Deserialize(Stream serializationStream)
        {
            if ((object)serializationStream == null)
                throw new ArgumentNullException(nameof(serializationStream));

            XmlSerializer serializer = new XmlSerializer(typeof(TimeSeriesMetadata));
            return serializer.Deserialize(serializationStream) as TimeSeriesMetadata;
        }

        /// <summary>
        /// Writes time-series metadata to specified Kafka <paramref name="topic"/>.
        /// </summary>
        /// <param name="metadata">Source time-series metadata object to write to Kafka.</param>
        /// <param name="router">Kafka router connection.</param>
        /// <param name="topic">Kafka topic.</param>
        public static void WriteToKafka(TimeSeriesMetadata metadata, BrokerRouter router, string topic)
        {
            if ((object)metadata == null)
                throw new ArgumentNullException(nameof(metadata));

            if ((object)router == null)
                throw new ArgumentNullException(nameof(router));

            if (string.IsNullOrWhiteSpace(topic))
                throw new ArgumentNullException(nameof(topic));

            using (MemoryStream stream = new MemoryStream())
            {
                Serialize(metadata, stream, true);

                using (Producer producer = new Producer(router))
                {
                    Message[] messages = new Message[2];
                    byte[] timeKey = BitConverter.GetBytes(DateTime.UtcNow.Ticks);

                    // First message used to serialize metadata size (since metadata messages can be large)
                    messages[SizeMessage] = new Message
                    {
                        Key = timeKey,
                        Value = BitConverter.GetBytes(stream.Length)
                    };

                    // Second message used to serialize metadata value
                    messages[ValueMessage] = new Message
                    {
                        Key = timeKey,
                        Value = stream.ToArray()
                    };

                    // Send meta-data to Kafka
                    producer.SendMessageAsync(topic, messages).Wait();
                }
            }
        }

        /// <summary>
        /// Reads latest time-series metadata from specified Kafka <paramref name="topic"/>.
        /// </summary>
        /// <param name="router">Kafka router connection.</param>
        /// <param name="topic">Kafka topic.</param>
        /// <param name="serializationTime">Serialization time.</param>
        /// <param name="statusMessage">Status message function.</param>
        /// <returns>Latest <see cref="TimeSeriesMetadata"/> instance read from Kafka.</returns>
        public static TimeSeriesMetadata ReadFromKafka(BrokerRouter router, string topic, Action<string> statusMessage, out Ticks serializationTime)
        {
            if ((object)router == null)
                throw new ArgumentNullException(nameof(router));

            if (string.IsNullOrWhiteSpace(topic))
                throw new ArgumentNullException(nameof(topic));

            Message sizeMessage = ReadMessage(router, topic, SizeMessage);

            serializationTime = BitConverter.ToInt64(sizeMessage.Key, 0);

            Message valueMessage = ReadMessage(router, topic, ValueMessage, (int)Math.Ceiling(BitConverter.ToInt64(sizeMessage.Value, 0) / 4096.0D) * 4096);

            if (serializationTime != BitConverter.ToInt64(valueMessage.Key, 0))
                statusMessage?.Invoke("WARNING: Timestamp keys for metadata size and value records are mismatched...");

            using (MemoryStream stream = new MemoryStream(valueMessage.Value))
                return Deserialize(stream);
        }

        private static Message ReadMessage(BrokerRouter router, string topic, int messageIndex, int maxBytes = 32768)
        {
            Message message = null;

            long offset = router.LatestOffset(topic) - (2 - messageIndex);

            List<Message> messages = router.Fetch(topic, 0, offset, maxBytes)?.Messages;

            if ((messages?.Count ?? 0) > 0)
                message = messages?[0];

            if ((object)message == null)
                throw new InvalidOperationException("No Kafka record to consume");

            return message;
        }

        /// <summary>
        /// Caches meta-data locally.
        /// </summary>
        /// <param name="metadata">Source time-series metadata object to cache.</param>
        /// <param name="topic">Kafka topic.</param>
        /// <param name="statusMessage">Status message function.</param>
        public static void CacheLocally(TimeSeriesMetadata metadata, string topic, Action<string> statusMessage)
        {
            // Cache meta-data locally so it can be reviewed
            string cacheFileName = "undefined";

            try
            {
                // Define default cache path
                string cachePath = null;

                try
                {
                    // Attempt to retrieve configuration cache path as defined in the config file
                    ConfigurationFile configFile = ConfigurationFile.Current;
                    CategorizedSettingsElementCollection systemSettings = configFile.Settings["systemSettings"];
                    CategorizedSettingsElement configurationCachePathSetting = systemSettings["ConfigurationCachePath"];

                    if ((object)configurationCachePathSetting != null)
                        cachePath = FilePath.GetAbsolutePath(systemSettings["ConfigurationCachePath"].Value);

                    if (string.IsNullOrEmpty(cachePath))
                        cachePath = $"{FilePath.GetAbsolutePath("")}{Path.DirectorySeparatorChar}ConfigurationCache{Path.DirectorySeparatorChar}";
                }
                catch (ConfigurationErrorsException)
                {
                    cachePath = $"{FilePath.GetAbsolutePath("")}{Path.DirectorySeparatorChar}ConfigurationCache{Path.DirectorySeparatorChar}";
                }

                cacheFileName = Path.Combine(cachePath, $"{topic}.xml");

                using (FileStream stream = File.Create(cacheFileName))
                    Serialize(metadata, stream, false);
            }
            catch (Exception ex)
            {
                statusMessage?.Invoke($"WARNING: Failed to locally cache current metadata to \"{cacheFileName}\": {ex.Message}");
            }
        }

        #endregion
    }
}
