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
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using GSF.Collections;
using GSF.Historian.Files;
using GSF.Web;

namespace GSF.Historian.DataServices.Grafana
{
    /// <summary>
    /// Represents a REST based API for a simple JSON based Grafana data source.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class GrafanaDataService : DataService, IGrafanaDataService
    {
        #region [ Members ]

        // Fields
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
            // Abort if services is not enabled.
            if (!Enabled || (object)Archive == null)
                return null;

            // Task allows processing of multiple simultaneous queries
            return Task.Factory.StartNew(() =>
            {
                if (!request.format?.Equals("json", StringComparison.OrdinalIgnoreCase) ?? false)
                    throw new InvalidOperationException("Only JSON formatted query requests are currently supported.");

                InitializeMetadata();

                DateTime startTime = request.range.from.ParseJsonTimestamp();
                DateTime stopTime = request.range.to.ParseJsonTimestamp();
                HashSet<string> targets = new HashSet<string>(request.targets.Select(requestTarget => requestTarget.target));
                Dictionary<int, string> targetMap = targets.Select(target => new KeyValuePair<int, string>(s_metadata.FirstOrDefault(kvp => target.Split(' ')[0].Equals(kvp.Value, StringComparison.OrdinalIgnoreCase)).Key, target)).Where(kvp => kvp.Key > 0).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                Dictionary<int, TimeSeriesValues> queriedTimeSeriesValues = new Dictionary<int, TimeSeriesValues>();
                long baseTicks = UnixTimeTag.BaseTicks.Value;

                if (targetMap.Count > 0)
                {
                    int[] measurementIDs = targetMap.Keys.ToArray();

                    // TODO: Change to decimated data read
                    foreach (IDataPoint point in Archive.ReadData(measurementIDs, startTime, stopTime, false))
                    {
                        queriedTimeSeriesValues.GetOrAdd(point.HistorianID, id => new TimeSeriesValues { target = targetMap[id], datapoints = new List<double[]>() })
                            .datapoints.Add(new[] { point.Value, (point.Time.ToDateTime().Ticks - baseTicks) / (double)Ticks.PerMillisecond });
                    }
                }

                return new List<TimeSeriesValues>(queriedTimeSeriesValues.Values);
            });
        }

        /// <summary>
        /// Search openHistorian for a target.
        /// </summary>
        /// <param name="request">Search target.</param>
        public string[] Search(Target request)
        {
            return s_metadata.Take(500).Select(entry => entry.Value).ToArray();
        }

        /// <summary>
        /// Queries openHistorian for annotations in a time-range (e.g., Alarms).
        /// </summary>
        /// <param name="request">Annotation request.</param>
        public List<AnnotationResponse> Annotations(AnnotationRequest request)
        {
            return new List<AnnotationResponse>();
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
