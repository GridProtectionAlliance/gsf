//******************************************************************************************************
//  GrafanaDataSourceBase.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  09/22/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using GSF.Web;

namespace GrafanaAdapters
{
    /// <summary>
    /// Represents a base implementation for Grafana data sources.
    /// </summary>
    public abstract class GrafanaDataSourceBase
    {
        #region [ Properties ]

        /// <summary>
        /// Gets or sets instance name for this <see cref="GrafanaDataSourceBase"/> implementation.
        /// </summary>
        public virtual string InstanceName { get; set; }

        /// <summary>
        /// Gets or sets <see cref="DataSet"/> based meta-data source available to this <see cref="GrafanaDataSourceBase"/> implementation.
        /// </summary>
        public virtual DataSet Metadata { get; set; }

        /// <summary>
        /// Gets or sets maximum number of search targets to return during a search query.
        /// </summary>
        public int MaximumSearchTargetsPerRequest { get; set; } = 200;

        /// <summary>
        /// Gets or sets maximum number of annotations to return during an annotations query.
        /// </summary>
        public int MaximumAnnotationsPerRequest { get; set; } = 100;

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Queries openHistorian as a Grafana data source.
        /// </summary>
        /// <param name="request">Query request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public Task<List<TimeSeriesValues>> Query(QueryRequest request, CancellationToken cancellationToken)
        {
            // Task allows processing of multiple simultaneous queries
            return Task.Factory.StartNew(() =>
            {
                if (!request.format?.Equals("json", StringComparison.OrdinalIgnoreCase) ?? false)
                    throw new InvalidOperationException("Only JSON formatted query requests are currently supported.");

                DateTime startTime = request.range.from.ParseJsonTimestamp();
                DateTime stopTime = request.range.to.ParseJsonTimestamp();
                HashSet<string> targets = new HashSet<string>(request.targets.Select(target => target.target));

                foreach (string target in request.targets.Select(target => target.target))
                    targets.UnionWith(AdapterBase.ParseInputMeasurementKeys(Metadata, false, target).Select(key => key.ToString()));

                Dictionary<ulong, string> targetMap = new Dictionary<ulong, string>();

                foreach (string target in targets)
                {
                    MeasurementKey key = MeasurementKey.LookUpOrCreate(target);

                    if (key != MeasurementKey.Undefined)
                        targetMap[key.ID] = target;
                }

                return QueryTimeSeriesValues(startTime, stopTime, request.maxDataPoints, targetMap, cancellationToken);
            },
            cancellationToken);
        }

        /// <summary>
        /// Queries data source for time-series values given a target map.
        /// </summary>
        /// <param name="startTime">Start-time for query.</param>
        /// <param name="stopTime">Stop-time for query.</param>
        /// <param name="maxDataPoints">Maximum data points to return.</param>
        /// <param name="targetMap">Point ID's</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Queried data source data in time-series values format.</returns>
        protected abstract List<TimeSeriesValues> QueryTimeSeriesValues(DateTime startTime, DateTime stopTime, int maxDataPoints, Dictionary<ulong, string> targetMap, CancellationToken cancellationToken);

        /// <summary>
        /// Search openHistorian for a target.
        /// </summary>
        /// <param name="request">Search target.</param>
        public Task<string[]> Search(Target request)
        {
            return Task.Factory.StartNew(() =>
            {
                return Metadata.Tables["ActiveMeasurements"]
                    .Select($"ID LIKE '{InstanceName}:%'")
                    .Take(MaximumSearchTargetsPerRequest)
                    .Select(row => $"{row["ID"]}")
                    .ToArray();
            });
        }

        /// <summary>
        /// Queries openHistorian for annotations in a time-range (e.g., Alarms).
        /// </summary>
        /// <param name="request">Annotation request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Queried annotations from data source.</returns>
        public async Task<List<AnnotationResponse>> Annotations(AnnotationRequest request, CancellationToken cancellationToken)
        {
            bool useFilterExpression;
            AnnotationType type = request.ParseQueryType(out useFilterExpression);
            Dictionary<string, DataRow> definitions = request.ParseSourceDefinitions(type, Metadata, useFilterExpression);
            List<TimeSeriesValues> annotationData = await Query(request.ExtractQueryRequest(definitions.Keys, MaximumAnnotationsPerRequest), cancellationToken);
            List<AnnotationResponse> responses = new List<AnnotationResponse>();

            foreach (TimeSeriesValues values in annotationData)
            {
                string target = values.target;
                DataRow definition = definitions[target];

                foreach (double[] datapoint in values.datapoints)
                {
                    if (type.IsApplicable(datapoint))
                    {
                        AnnotationResponse response = new AnnotationResponse
                        {
                            annotation = request.annotation,
                            time = datapoint[TimeSeriesValues.Time]
                        };

                        type.PopulateResponse(response, target, definition, datapoint);

                        responses.Add(response);
                    }
                }
            }

            return responses;
        }

        #endregion
    }
}
