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
using System.Linq;
using System.Text;
using System.Threading;
using GSF;
using GSF.Collections;
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

        // Fields
        private Uri[] m_servers;
        private BrokerRouter m_router;
        private Consumer m_consumer;
        private Thread m_processMessages;
        private readonly Dictionary<int, MeasurementKey> m_idTable;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="TimeSeriesConsumer"/>.
        /// </summary>
        public TimeSeriesConsumer()
        {
            m_idTable = new Dictionary<int, MeasurementKey>();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets Kafka servers to connect to, comma separated.
        /// </summary>
        [ConnectionStringParameter, Description("Defines comma separated list of Kakfa server URIs, e.g.: http://kafka1:9092, http://kafka2:9092")]
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
                status.AppendFormat("         Kafka server URIs: {0}", (object)m_servers == null ? "Undefined" : m_servers.ToDelimitedString(", "));
                status.AppendLine();
                status.AppendFormat("       Consumer topic name: {0}", Topic);
                status.AppendLine();
                status.AppendFormat("   Partition ID table size: {0:N0}", m_idTable.Count);
                status.AppendLine();

                if ((object)m_consumer != null)
                {
                    status.AppendFormat("Active consumer task count: {0:N0}", m_consumer.ConsumerTaskCount);
                    status.AppendLine();
                }

                return status.ToString();
            }
        }

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

            // Parse required settings
            if (!settings.TryGetValue("Servers", out setting) || string.IsNullOrWhiteSpace(setting))
                throw new ArgumentException("Required \"servers\" setting is missing.");

            Servers = setting.Trim();
            m_servers = Servers.Split(',').Select(uri => new Uri(uri)).ToArray();

            // Parse optional settings
            if (settings.TryGetValue("Topic", out setting) && !string.IsNullOrWhiteSpace(setting))
                Topic = setting.Trim();
            else
                Topic = TimeSeriesProducer.DefaultTopic;
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

            ConsumerOptions options = new ConsumerOptions(Topic, m_router);

            // Reduce partitions to consume based on defined output measurements; otherwise will receive all
            if (OutputMeasurements.Length > 0)
                options.PartitionWhitelist = OutputMeasurements.Select(m => (int)m.Key.ID).ToList();

            m_consumer = new Consumer(options);

            m_processMessages = new Thread(ProcessMessages) { IsBackground = true };
            m_processMessages.Start();
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
            if ((object)m_processMessages != null)
            {
                m_processMessages.Abort();
                m_processMessages = null;
            }

            if ((object)m_consumer != null)
            {
                m_consumer.Dispose();
                m_consumer = null;
            }

            if ((object)m_router != null)
            {
                m_router.Dispose();
                m_router = null;
            }

            m_idTable.Clear();
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

        private void ProcessMessages(object state)
        {
            foreach (Message message in m_consumer.Consume())
            {
                IMeasurement measurement = message.KafkaDeserialize();

                measurement.Key = m_idTable.GetOrAdd(message.PartitionId, id =>
                {
                    MeasurementKey key = MeasurementKey.Undefined;
                    DataRow[] rows = DataSource.Tables["ActiveMeasurements"].Select($"ID LIKE '*:{id}'");

                    if (rows.Length > 0)
                        MeasurementKey.TryParse(rows[0]["ID"].ToNonNullString(), out key);

                    return key;
                });

                OnNewMeasurements(new[] { measurement });
            }
        }

        #endregion
    }
}
