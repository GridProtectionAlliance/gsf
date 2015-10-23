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
using System.Linq;
using System.Text;
using GSF;
using GSF.Collections;
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

        // Fields
        private Uri[] m_servers;
        private BrokerRouter m_router;
        private Producer m_producer;

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
        [ConnectionStringParameter, Description("Defines the Kafka topic name. Defaults to \"GSF\"."), DefaultValue(DefaultTopic)]
        public string Topic
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
                status.AppendFormat("         Kafka server URIs: {0}", (object)m_servers == null ? "Undefined" : m_servers.ToDelimitedString(", "));
                status.AppendLine();
                status.AppendFormat("       Producer topic name: {0}", Topic);
                status.AppendLine();

                if ((object)m_producer != null)
                {
                    status.AppendFormat("Active producer call count: {0:N0}", m_producer.ActiveCount);
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
                Topic = DefaultTopic;
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
            m_producer = new Producer(m_router);
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
        /// Serializes measurements to data output stream.
        /// </summary>
        protected override async void ProcessMeasurements(IMeasurement[] measurements)
        {
            List<Message> messages = new List<Message>(measurements.Length);

            foreach (IMeasurement measurement in measurements)
            {
                Message message = new Message { PartitionId = (int)measurement.Key.ID };
                measurement.KakfaSerialize(message);
                messages.Add(message);
            }

            await m_producer.SendMessageAsync(Topic, messages);
        }

        #endregion
    }
}
