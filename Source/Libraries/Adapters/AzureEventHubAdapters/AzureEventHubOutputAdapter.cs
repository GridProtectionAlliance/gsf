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

#pragma warning disable 4014

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
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
        /// Default value for <see cref="UseParallelPosting"/>.
        /// </summary>
        public const bool DefaultUseParallelPosting = false;

        /// <summary>
        /// Default value for <see cref="ValuesPerPost"/>.
        /// </summary>
        public const int DefaultValuesPerPost = 50;

        /// <summary>
        /// Default value for <see cref="SerializeMetadata"/>.
        /// </summary>
        public const bool DefaultSerializeMetadata = true;

        /// <summary>
        /// Default value for <see cref="DataPartitionKey"/>.
        /// </summary>
        public const string DefaultDataPartitionKey = "data";

        /// <summary>
        /// Default value for <see cref="MetadataPartitionKey"/>.
        /// </summary>
        public const string DefaultMetadataPartitionKey = "metadata";

        private EventHubClient m_eventHubClient;    // Azure Event Hub Client
        private string m_connectionResponse;        // Response from connection attempt
        private long m_totalValues;                 // Total archived values
        private long m_totalPosts;                  // Total post to the Azure Event Hub connection
        private long m_totalParallelGroups;         // Total measurement groups processed in parallel

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the event hub connection string for the Azure event hub connection.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the event hub connection string for the Azure event hub connection.")]
        public string EventHubConnectionString
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the event hub name for the Azure event hub connection.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the event hub name for the Azure event hub connection.")]
        public string EventHubName
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
        /// Gets or sets flag that determines if multiple posts to Azure Event Hub should be made in parallel.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines flag that determines if multiple posts to Azure Event Hub should be made in parallel.")]
        [DefaultValue(DefaultUseParallelPosting)]
        public bool UseParallelPosting
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the maximum values to send per post when <see cref="UseParallelPosting"/> is <c>true</c> for the Azure Event Hub connection.
        /// </summary>
        [ConnectionStringParameter]
        [Description("When parallel posting is enabled, defines the maximum values to send per post for the Azure Event Hub connection.")]
        [DefaultValue(DefaultValuesPerPost)]
        public int ValuesPerPost
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

                status.AppendFormat("      Azure event hub name: {0}", EventHubName);
                status.AppendLine();
                status.AppendFormat("      Use parallel posting: {0}", UseParallelPosting);
                status.AppendLine();
                status.AppendFormat("       Serialize meta-data: {0}", SerializeMetadata);
                status.AppendLine();

                if (UseParallelPosting)
                {
                    status.AppendFormat("   Maximum values per post: {0:N0}", ValuesPerPost);
                    status.AppendLine();
                    status.AppendFormat("  Average parallel threads: {0:0.0}", m_totalParallelGroups / (double)InternalProcessQueue.TotalFunctionCalls);
                    status.AppendLine();
                }

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
            return $"Archived {m_totalValues:N0} measurements via {m_totalPosts:N0} posts to \"{EventHubName}\".".CenterText(maxLength);
        }

        /// <summary>
        /// Initializes <see cref="AzureEventHubOutputAdapter"/>.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            new ConnectionStringParser().ParseConnectionString(ConnectionString, this);
        }

        /// <summary>
        /// Attempts to connect to Azure Event Hub database.
        /// </summary>
        protected override void AttemptConnection()
        {
            try
            {
                // Establish event hub connection
                EventHubsConnectionStringBuilder builder = new EventHubsConnectionStringBuilder(EventHubConnectionString)
                {
                    EntityPath = EventHubName
                };

                m_eventHubClient = EventHubClient.CreateFromConnectionString(builder.ToString());
                
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
            m_eventHubClient.Close();
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
                m_eventHubClient.SendAsync(new EventData(Encoding.UTF8.GetBytes(jsonMetadata.ToString())), MetadataPartitionKey);
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

            if (UseParallelPosting)
            {
                List<IMeasurement[]> measurementGroups = new List<IMeasurement[]>();
                IMeasurement[] measurementGroup = measurements.Take(ValuesPerPost).ToArray();
                int skipCount = measurementGroup.Length;

                while (measurementGroup.Length > 0)
                {
                    measurementGroups.Add(measurementGroup);
                    measurementGroup = measurements.Skip(skipCount).Take(ValuesPerPost).ToArray();
                    skipCount += measurementGroup.Length;
                }

                Parallel.ForEach(measurementGroups, PostMeasurementsToEventHub);
                m_totalParallelGroups += measurementGroups.Count;
            }
            else
            {
                PostMeasurementsToEventHub(measurements);
            }
        }

        private void PostMeasurementsToEventHub(IMeasurement[] measurements)
        {
            const string PostFormat = "{{\"V{0}\":[{1},{2},{3}]}}";

            try
            {
                // Build a JSON post expression with measurement values to use as post data
                List<EventData> samples = new List<EventData>();

                foreach (IMeasurement measurement in measurements)
                {
                    // Encode JSON data as UTF8
                    string jsonData = string.Format(PostFormat, measurement.Key.ID, GetEpochMilliseconds(measurement.Timestamp), measurement.AdjustedValue, (uint)measurement.StateFlags);
                    samples.Add(new EventData(Encoding.UTF8.GetBytes(jsonData)));
                }             

                // Write data to event hub
                m_eventHubClient.SendAsync(samples, DataPartitionKey);

                Interlocked.Add(ref m_totalValues, measurements.Length);
                Interlocked.Increment(ref m_totalPosts);
            }
            catch
            {
                if (RequeueOnException)
                    InternalProcessQueue.InsertRange(0, measurements);

                throw;
            }
        }

        // Produce a web friendly timestamp
        private long GetEpochMilliseconds(Ticks timestamp) => (long)(timestamp - UnixTimeTag.BaseTicks).ToMilliseconds();

        #endregion
    }
}
