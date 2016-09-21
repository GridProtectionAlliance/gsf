//******************************************************************************************************
//  GrafanaDataService.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
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
//  09/12/2016 - Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using GSF;
using GSF.Historian;
using GSF.Historian.DataServices;
using GSF.Historian.Files;
using GSF.TimeSeries.Adapters;
using GSF.Web;
using HistorianAdapters;

namespace GrafanaAdapters
{
    /// <summary>
    /// Represents a REST based API for a simple JSON based Grafana data source.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class GrafanaDataService : DataService, IGrafanaDataService
    {
        #region [ Members ]

        // Fields
        private string m_instanceName;
        private volatile bool m_initializedMetadata;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of a <see cref="GrafanaDataService"/>.
        /// </summary>
        public GrafanaDataService()
        {
            Endpoints = "http.rest://localhost:6052/api/grafana/";
            ServiceEnabled = false;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="IArchive"/> used by the web service for its data.
        /// </summary>
        public override IArchive Archive
        {
            get
            {
                return base.Archive;
            }

            set
            {
                if ((object)base.Archive != null)
                    base.Archive.MetadataUpdated -= ArchiveMetadataUpdated;

                base.Archive = value;

                if ((object)base.Archive != null)
                    base.Archive.MetadataUpdated += ArchiveMetadataUpdated;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="GrafanaDataService"/> object and optionally releases the managed resources.
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
                        if ((object)Archive != null)
                            Archive.MetadataUpdated -= ArchiveMetadataUpdated;
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
        /// Initializes the web service.
        /// </summary>
        public override void Initialize()
        {
            // Get historian instance name as assigned to settings category by data services host
            m_instanceName = SettingsCategory.Substring(0, SettingsCategory.IndexOf(this.GetType().Name, StringComparison.OrdinalIgnoreCase));

            base.Initialize();
        }

        /// <summary>
        /// Validates that openHistorian Grafana data source is responding as expected.
        /// </summary>
        public void TestDataSource()
        {
        }

        /// <summary>
        /// Queries openHistorian as a Grafana data source.
        /// </summary>
        /// <param name="request">Query request.</param>
        public Task<List<TimeSeriesValues>> Query(QueryRequest request)
        {
            // Task allows processing of multiple simultaneous queries
            return Task.Factory.StartNew(() =>
            {
                // Abort if services is not enabled.
                if (!Enabled || (object)Archive == null)
                    return null;

                if (!request.format?.Equals("json", StringComparison.OrdinalIgnoreCase) ?? false)
                    throw new InvalidOperationException("Only JSON formatted query requests are currently supported.");

                InitializeMetadata();

                DateTime startTime = request.range.from.ParseJsonTimestamp();
                DateTime stopTime = request.range.to.ParseJsonTimestamp();
                HashSet<string> targets = new HashSet<string>(request.targets.Select(requestTarget => requestTarget.target));

                foreach (string requestTarget in request.targets.Select(requestTarget => requestTarget.target))
                {
                    if (string.IsNullOrWhiteSpace(requestTarget))
                        continue;

                    foreach (string targetItem in requestTarget.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        string target = targetItem.Trim();

                        if (target.StartsWith("FILTER", StringComparison.OrdinalIgnoreCase))
                        {
                            string tableName, expression, sortField;
                            int takeCount;

                            if (AdapterBase.ParseFilterExpression(target, out tableName, out expression, out sortField, out takeCount))
                            {
                                if (takeCount == int.MaxValue)
                                    takeCount = 10;

                                targets.UnionWith(LocalOutputAdapter.Instances[m_instanceName].DataSource.Tables[tableName].Select(expression, sortField).Take(takeCount).Select(row => $"{row["ID"]}"));
                            }
                        }
                        else
                        {
                            targets.Add(target);
                        }
                    }
                }

                Dictionary<int, string> targetMap = targets.Select(target => new KeyValuePair<int, string>(s_metadata.FirstOrDefault(kvp => target.Split(' ')[0].Equals(kvp.Value, StringComparison.OrdinalIgnoreCase)).Key, target)).Where(kvp => kvp.Key > 0).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                List<TimeSeriesValues> queriedTimeSeriesValues = new List<TimeSeriesValues>();
                long baseTicks = UnixTimeTag.BaseTicks.Value;

                if (targetMap.Count > 0)
                {
                    foreach (int measurementID in targetMap.Keys)
                    {
                        TimeSeriesValues series = new TimeSeriesValues { target = targetMap[measurementID], datapoints = new List<double[]>() };
                        IDataPoint[] data = Archive.ReadData(measurementID, startTime, stopTime, false).ToArray();
                        int interval = data.Length / request.maxDataPoints + 1;
                        int pointCount = 0;

                        series.datapoints.AddRange(data.Where(point => pointCount++ % interval == 0).Select(point => new[] { point.Value, (point.Time.ToDateTime().Ticks - baseTicks) / (double)Ticks.PerMillisecond }));

                        queriedTimeSeriesValues.Add(series);
                    }
                }

                return queriedTimeSeriesValues;
            });
        }

        /// <summary>
        /// Search openHistorian for a target.
        /// </summary>
        /// <param name="request">Search target.</param>
        public Task<string[]> Search(Target request)
        {
            return Task.Factory.StartNew(() =>
            {
                DataTable measurements = LocalOutputAdapter.Instances[m_instanceName].DataSource.Tables["ActiveMeasurements"];

                return s_metadata.Take(200)
                        .Select(entry => entry.Value)
                        .Select(id => measurements.Select($"ID = '{id}'").FirstOrDefault())
                        .Where(row => (object)row != null)
                        .Select(row => $"{row["ID"]} [{row["PointTag"]}]")
                        .ToArray();
            });
        }    

        /// <summary>
        /// Queries openHistorian for annotations in a time-range (e.g., Alarms).
        /// </summary>
        /// <param name="request">Annotation request.</param>
        public async Task<List<AnnotationResponse>> Annotations(AnnotationRequest request)
        {
            bool useFilterExpression;
            AnnotationType type = request.ParseQueryType(out useFilterExpression);
            DataSet metadata = LocalOutputAdapter.Instances[m_instanceName].DataSource;
            DataRow[] definitions = request.ParseSourceDefinitions(type, metadata, useFilterExpression);
            List<TimeSeriesValues> annotationData = await Query(request.ExtractQueryRequest(type.GetTargets(definitions), 100));
            List<AnnotationResponse> responses = new List<AnnotationResponse>();

            foreach (TimeSeriesValues values in annotationData)
            {
                responses.Add(new AnnotationResponse
                {
                    annotation = request.annotation,
                    title = "Get Title"
                });
            }

            return responses;
        }

        private void InitializeMetadata()
        {
            // Abort if services is not enabled or Archive is not defined
            if (m_initializedMetadata || !Enabled || (object)Archive == null)
                return;

            m_initializedMetadata = true;

            int id = 0;

            while (true)
            {
                byte[] buffer = Archive.ReadMetaData(++id);

                if ((object)buffer == null)
                    break;

                SerializableMetadataRecord newRecord = new SerializableMetadataRecord(new MetadataRecord(id, MetadataFileLegacyMode.Enabled, buffer, 0, buffer.Length));
                s_metadata[newRecord.HistorianID] = $"{newRecord.PlantCode}:{newRecord.HistorianID}";
            }
        }

        private void ArchiveMetadataUpdated(object sender, EventArgs e)
        {
            m_initializedMetadata = false;
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly ConcurrentDictionary<int, string> s_metadata = new ConcurrentDictionary<int, string>();

        #endregion
    }
}
