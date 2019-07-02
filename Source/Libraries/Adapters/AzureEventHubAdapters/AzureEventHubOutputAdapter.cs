//******************************************************************************************************
//  AzureEventHubOutputAdapter.cs - Gbtc
//
//  Copyright © 2019, Grid Protection Alliance.  All Rights Reserved.
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
//  06/10/2019 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GSF;
using GSF.Data;
using GSF.Diagnostics;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using Microsoft.Azure.EventHubs;
using ConnectionStringParser = GSF.Configuration.ConnectionStringParser<GSF.TimeSeries.Adapters.ConnectionStringParameterAttribute>;

namespace AzureEventHubAdapters
{
    /// <summary>
    /// Represents an output adapter that sends measurements to an Azure Event Hub
    /// </summary>
    [Description("AzureEventHub: Sends measurements to an Azure Event Hub")]
    public class AzureEventHubOutputAdapter : OutputAdapterBase
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Default value for <see cref="DataPartitionKey"/>.
        /// </summary>
        public const string DefaultDataPartitionKey = "data";

        /// <summary>
        /// Default value for <see cref="MetadataPartitionKey"/>.
        /// </summary>
        public const string DefaultMetadataPartitionKey = "metadata";

        /// <summary>
        /// Default value for <see cref="DataPostFormat"/>.
        /// </summary>
        public const string DefaultDataPostFormat = "{{V{0}:[{1},{2},{3}]}}";

        /// <summary>
        /// Default value for <see cref="TimestampFormat"/>.
        /// </summary>
        public const string DefaultTimestampFormat = "EpochMilliseconds";

        /// <summary>
        /// Default value for <see cref="SerializeMetadata"/>.
        /// </summary>
        public const bool DefaultSerializeMetadata = true;

        private EventHubClient m_eventHubDataClient;        // Azure Event Hub Data Client
        private EventHubClient m_eventHubMetadataClient;    // Azure Event Hub Metadata Client
        private string m_connectionResponse;                // Response from connection attempt
        private long m_totalValues;                         // Total archived values
        private long m_totalPosts;                          // Total post to the Azure Event Hub connection
        private bool m_useEpochMilliseconds;                // Flag that determines if timestamp should be Unix epoch milliseconds

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the event hub time-series data client connection string for the Azure event hub connection.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the event hub time-series data client connection string for the Azure event hub connection.")]
        public string EventHubDataClientConnectionString
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the event hub time-series data client name for the Azure event hub connection.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the event hub time-series data client name for the Azure event hub connection.")]
        public string EventHubDataClientName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the event hub meta-data client connection string for the Azure event hub connection.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the event hub meta-data client connection string for the Azure event hub connection. Leave empty to post to time-series data client event hub.")]
        [DefaultValue("")]
        public string EventHubMetadataClientConnectionString
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the event hub meta-data client name for the Azure event hub connection.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the event hub meta-data client name for the Azure event hub connection. Empty value will default to time-series data client name.")]
        [DefaultValue("")]
        public string EventHubMetadataClientName
        {
            get;
            set;
        }
        
        /// <summary>
        /// Gets or sets the Azure event hub partition key for the time-series data.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the Azure event hub partition key for the time-series data.")]
        [DefaultValue(DefaultDataPartitionKey)]
        public string DataPartitionKey
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the Azure event hub partition key for the time-series meta-data.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the Azure event hub partition key for the time-series meta-data.")]
        [DefaultValue(DefaultMetadataPartitionKey)]
        public string MetadataPartitionKey
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the Azure event hub JSON data posting format for the time-series meta-data.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the Azure event hub JSON data posting format for the time-series meta-data.")]
        [DefaultValue(DefaultDataPostFormat)]
        public string DataPostFormat
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the default timestamp format for the time-series data.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the default timestamp format for the time-series data, e.g.: \"yyyy-MM-dd HH:mm:ss.fff\", without quotes. Set to literal \"EpochMilliseconds\", without quotes, to use Unix epoch milliseconds timestamp.")]
        [DefaultValue(DefaultTimestampFormat)]
        public string TimestampFormat
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets flag that determines if metadata should be serialized into Azure event hub.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Determines if metadata should be serialized into Azure event hub.")]
        [DefaultValue(DefaultSerializeMetadata)]
        public bool SerializeMetadata
        {
            get;
            set;
        }

        /// <summary>
        /// Returns a flag that determines if measurements sent to this <see cref="AzureEventHubOutputAdapter"/> are destined for archival.
        /// </summary>
        public override bool OutputIsForArchive => true;

        /// <summary>
        /// Gets flag that determines if this <see cref="AzureEventHubOutputAdapter"/> uses an asynchronous connection.
        /// </summary>
        protected override bool UseAsyncConnect => false;

        /// <summary>
        /// Gets a detailed status for this <see cref="AzureEventHubOutputAdapter"/>.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.Append(base.Status);

                status.AppendFormat("       Data event hub name: {0}", EventHubDataClientName);
                status.AppendLine();
                status.AppendFormat("  Meta-data event hub name: {0}", string.IsNullOrWhiteSpace(EventHubMetadataClientName) ? EventHubDataClientName : EventHubMetadataClientName);
                status.AppendLine();
                status.AppendFormat("          Data post format: {0}", DataPostFormat);
                status.AppendLine();
                status.AppendFormat("          Timestamp format: {0}", TimestampFormat);
                status.AppendLine();
                status.AppendFormat("       Serialize meta-data: {0}", SerializeMetadata);
                status.AppendLine();
                status.AppendFormat("     Total archived values: {0:N0}", m_totalValues);
                status.AppendLine();
                status.AppendFormat("               Total posts: {0:N0}", m_totalPosts);
                status.AppendLine();
                status.AppendFormat("   Average values per post: {0:R}", Math.Round(m_totalValues / (double)m_totalPosts, 2));
                status.AppendLine();
                status.AppendFormat("       Connection response: {0}", m_connectionResponse);
                status.AppendLine();

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Returns a brief status of this <see cref="AzureEventHubOutputAdapter"/>
        /// </summary>
        /// <param name="maxLength">Maximum number of characters in the status string</param>
        /// <returns>Status</returns>
        public override string GetShortStatus(int maxLength)
        {
            return $"Archived {m_totalValues:N0} measurements via {m_totalPosts:N0} posts to \"{EventHubDataClientName}\".".CenterText(maxLength);
        }

        /// <summary>
        /// Initializes <see cref="AzureEventHubOutputAdapter"/>.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            new ConnectionStringParser().ParseConnectionString(ConnectionString, this);

            if (string.IsNullOrWhiteSpace(TimestampFormat))
                TimestampFormat = DefaultTimestampFormat;

            m_useEpochMilliseconds = TimestampFormat.Trim().Equals(DefaultTimestampFormat, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Attempts to connect to Azure Event Hub database.
        /// </summary>
        protected override void AttemptConnection()
        {
            try
            {
                // Establish data event hub connection
                EventHubsConnectionStringBuilder builder = new EventHubsConnectionStringBuilder(EventHubDataClientConnectionString)
                {
                    EntityPath = EventHubDataClientName
                };

                m_eventHubDataClient = EventHubClient.CreateFromConnectionString(builder.ToString());

                if (string.IsNullOrWhiteSpace(EventHubMetadataClientConnectionString))
                {
                    m_eventHubMetadataClient = m_eventHubDataClient;
                }
                else
                {
                    // Establish meta-data event hub connection
                    builder = new EventHubsConnectionStringBuilder(EventHubMetadataClientConnectionString)
                    {
                        EntityPath = string.IsNullOrWhiteSpace(EventHubMetadataClientName) ? EventHubDataClientName : EventHubMetadataClientName
                    };

                    m_eventHubMetadataClient = EventHubClient.CreateFromConnectionString(builder.ToString());
                }
                
                m_connectionResponse = "Connected";
            }
            catch (Exception ex)
            {
                // Hang onto response for status logging
                m_connectionResponse = ex.Message;

                // Re-throw any captured exceptions, this will restart connection cycle as needed
                throw;
            }
        }

        /// <summary>
        /// Attempts to disconnect from Azure Event Hub.
        /// </summary>
        protected override void AttemptDisconnection()
        {
            m_eventHubDataClient.Close();
            
            if (m_eventHubDataClient != m_eventHubMetadataClient)
                m_eventHubMetadataClient.Close();
        }

        /// <summary>
        /// Executes the metadata refresh in a synchronous fashion.
        /// </summary>
        protected override void ExecuteMetadataRefresh()
        {
            if (!Initialized || !Enabled || !SerializeMetadata)
                return;

            const string PostFormat = "{{ID:{0},Source:\"{1}\",SignalID:\"{2}\",PointTag:\"{3}\",Device:\"{4}\",SignalType:\"{5}\",Longitude:{6},Latitude:{7},Description:\"{8}\",LastUpdate:{9}}}";

            try
            {
                StringBuilder jsonMetadata = new StringBuilder("{Metadata:[");
                bool injectComma = false;

                foreach (DataRow row in DataSource.Tables["ActiveMeasurements"].AsEnumerable())
                {
                    if (MeasurementKey.TryParse(row.Field<string>("ID") ?? MeasurementKey.Undefined.ToString(), out MeasurementKey key))
                    {
                        if (injectComma)
                            jsonMetadata.Append(',');
                        else
                            injectComma = true;

                        jsonMetadata.AppendFormat(PostFormat,
                            /* {0} */ (uint)key.ID,
                            /* {1} */ key.Source,
                            /* {2} */ row.Field<object>("SignalID"),
                            /* {3} */ row.Field<string>("PointTag"),
                            /* {4} */ row.Field<string>("Device"),
                            /* {5} */ row.Field<string>("SignalType"),
                            /* {6} */ row.ConvertField("Longitude", 0.0F),
                            /* {7} */ row.ConvertField("Latitude", 0.0F),
                            /* {8} */ row.Field<string>("Description"),
                            /* {9} */ GetEpochMilliseconds(row.Field<DateTime>("UpdatedOn").Ticks)
                        );
                    }
                }

                jsonMetadata.Append("]}");

                // Write metadata to event hub:
                new Task(async() => await m_eventHubMetadataClient.SendAsync(new EventData(Encoding.UTF8.GetBytes(jsonMetadata.ToString())), MetadataPartitionKey)).Wait();
            }
            catch (Exception ex)
            {
                OnProcessException(MessageLevel.Warning, new InvalidOperationException($"Failed to serialize current time-series metadata records: {ex.Message}", ex));
            }
        }

        /// <summary>
        /// Serializes measurements to Azure Event Hub.
        /// </summary>
        protected override void ProcessMeasurements(IMeasurement[] measurements)
        {
            if (measurements.Length == 0)
                return;

            try
            {
                // Build a JSON post expression with measurement values to use as post data
                List<EventData> samples = new List<EventData>();

                foreach (IMeasurement measurement in measurements)
                {
                    // Encode JSON data as UTF8
                    string jsonData = string.Format(DataPostFormat, 
                        measurement.Key.ID,
                        m_useEpochMilliseconds ? GetEpochMilliseconds(measurement.Timestamp).ToString() : measurement.Timestamp.ToString(TimestampFormat),
                        measurement.AdjustedValue,
                        (uint)measurement.StateFlags);

                    samples.Add(new EventData(Encoding.UTF8.GetBytes(jsonData)));
                }             

                // Write data to event hub
                m_eventHubDataClient.SendAsync(samples, DataPartitionKey).Wait();

                Interlocked.Add(ref m_totalValues, measurements.Length);
                Interlocked.Increment(ref m_totalPosts);
            }
            catch (Exception ex)
            {
                OnProcessException(MessageLevel.Warning, new InvalidOperationException($"Failed to serialize current time-series data records: {ex.Message}", ex));
            }
        }

        // Produce a web friendly timestamp
        private long GetEpochMilliseconds(Ticks timestamp) => (long)(timestamp - UnixTimeTag.BaseTicks).ToMilliseconds();

        #endregion
    }
}
