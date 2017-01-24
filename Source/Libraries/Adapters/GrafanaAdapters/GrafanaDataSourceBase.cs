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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using GSF.Collections;
using GSF.NumericalAnalysis;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using GSF.Web;

// ReSharper disable PossibleMultipleEnumeration
namespace GrafanaAdapters
{
    #region [ Enumerations ]

    internal enum Aggregate
    {
        Average,
        Minimum,
        Maximum,
        Total,
        Range,
        Count,
        TimeInt,
        Derivative,
        StdDev,
        StDevSamp,
        None
    }

    #endregion

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
                int maxDataPoints = request.maxDataPoints;

                return QueryTimeSeriesValuesFromTargets(request.targets.Select(target => target.target), startTime, stopTime, maxDataPoints, cancellationToken);
            },
            cancellationToken);
        }

        private List<TimeSeriesValues> QueryTimeSeriesValuesFromTargets(IEnumerable<string> targets, DateTime startTime, DateTime stopTime, int maxDataPoints, CancellationToken cancellationToken)
        {
            // A single target might look like the following:
            // PPA:15; STAT:20; SETSUM(COUNT(PPA:8; PPA:9; PPA:10)); FILTER ActiveMeasurements WHERE SignalType = 'VPHA'; RANGE(PPA:99; SUM(FILTER ActiveMeasurements WHERE SignalType = 'FREQ'; STAT:12))

            List<TimeSeriesValues> results = new List<TimeSeriesValues>();
            HashSet<string> targetSet = new HashSet<string>(targets, StringComparer.OrdinalIgnoreCase); // Targets include user provided input, so casing should be ignored
            HashSet<string> reducedTargetSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            List<Match> aggregateExpressions = new List<Match>();

            foreach (string target in targetSet)
            {
                // Find any aggregate expressions in target
                Match[] aggregates = TargetCache<Match[]>.GetOrAdd(target, () =>
                {
                    lock (s_aggregateExpressions)
                        return s_aggregateExpressions.Matches(target).Cast<Match>().ToArray();
                });

                if (aggregates.Length > 0)
                {
                    aggregateExpressions.AddRange(aggregates);

                    // Reduce target to non-aggregate expressions - important so later split on ';' succeeds properly
                    string reducedTarget = target;

                    foreach (string aggregate in aggregates.Select(match => match.Value))
                        reducedTarget = reducedTarget.Replace(aggregate, "");

                    if (!string.IsNullOrWhiteSpace(reducedTarget))
                        reducedTargetSet.Add(reducedTarget);
                }
                else
                {
                    reducedTargetSet.Add(target);
                }
            }

            if (aggregateExpressions.Count > 0)
            {
                // Parse aggregate expressions
                IEnumerable<Tuple<Aggregate, string, bool>> parsedAggregates = aggregateExpressions.Select(ParseAggregate);

                // Execute aggregate expressions
                foreach (Tuple<Aggregate, string, bool> parsedAggregate in parsedAggregates)
                    results.AddRange(ExecuteAggregate(parsedAggregate, startTime, stopTime, maxDataPoints, cancellationToken));

                // Use reduced target set that excludes any aggregate expressions
                targetSet = reducedTargetSet;
            }

            // Query any remaining targets
            if (targetSet.Count > 0)
            {
                // Split remaining targets on semi-colon, this way even multiple filter expressions can be used as inputs to aggregations
                string[] allTargets = targetSet.Select(target => target.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)).SelectMany(currentTargets => currentTargets).ToArray();

                // Expand target set to include point tags for all parsed inputs
                foreach (string target in allTargets)
                    targetSet.UnionWith(TargetCache<string[]>.GetOrAdd(target, () => AdapterBase.ParseInputMeasurementKeys(Metadata, false, target).Select(key => key.TagFromKey(Metadata)).ToArray()));

                Dictionary<ulong, string> targetMap = new Dictionary<ulong, string>();

                // Target set now contains both original expressions and newly parsed individual point tags - to create final point list we
                // are only interested in the point tags, provided either by direct user entry or derived by parsing filter expressions
                foreach (string target in targetSet)
                {
                    // Reduce all targets down to a dictionary of point ID's mapped to point tags
                    MeasurementKey key = TargetCache<MeasurementKey>.GetOrAdd(target, () => target.KeyFromTag(Metadata));

                    if (key != MeasurementKey.Undefined)
                        targetMap[key.ID] = target;
                }

                // Query underlying data source for data
                results.AddRange(QueryTimeSeriesValues(startTime, stopTime, maxDataPoints, targetMap, cancellationToken));
            }

            return results;
        }

        private Tuple<Aggregate, string, bool> ParseAggregate(Match aggregate)
        {
            bool setOperation = aggregate.Groups[1].Success;
            string aggregateExpression = setOperation ? aggregate.Value.Substring(3) : aggregate.Value;
            Tuple<Aggregate, string, bool> result = TargetCache<Tuple<Aggregate, string, bool>>.GetOrAdd(aggregate.Value, () =>
            {
                Match filterMatch;

                // Look for average aggregate
                lock (s_averageExpression)
                    filterMatch = s_averageExpression.Match(aggregateExpression);

                if (filterMatch.Success)
                    return new Tuple<Aggregate, string, bool>(Aggregate.Average, filterMatch.Result("${Expression}").Trim(), setOperation);

                // Look for minimum aggregate
                lock (s_minimumExpression)
                    filterMatch = s_minimumExpression.Match(aggregateExpression);

                if (filterMatch.Success)
                    return new Tuple<Aggregate, string, bool>(Aggregate.Minimum, filterMatch.Result("${Expression}").Trim(), setOperation);

                // Look for maximum aggregate
                lock (s_maximumExpression)
                    filterMatch = s_maximumExpression.Match(aggregateExpression);

                if (filterMatch.Success)
                    return new Tuple<Aggregate, string, bool>(Aggregate.Maximum, filterMatch.Result("${Expression}").Trim(), setOperation);

                // Look for total aggregate
                lock (s_totalExpression)
                    filterMatch = s_totalExpression.Match(aggregateExpression);

                if (filterMatch.Success)
                    return new Tuple<Aggregate, string, bool>(Aggregate.Total, filterMatch.Result("${Expression}").Trim(), setOperation);

                // Look for range aggregate
                lock (s_rangeExpression)
                    filterMatch = s_rangeExpression.Match(aggregateExpression);

                if (filterMatch.Success)
                    return new Tuple<Aggregate, string, bool>(Aggregate.Range, filterMatch.Result("${Expression}").Trim(), setOperation);

                // Look for count aggregate
                lock (s_countExpression)
                    filterMatch = s_countExpression.Match(aggregateExpression);

                if (filterMatch.Success)
                    return new Tuple<Aggregate, string, bool>(Aggregate.Count, filterMatch.Result("${Expression}").Trim(), setOperation);

                // Look for standard deviation aggregate
                lock (s_stdDevExpression)
                    filterMatch = s_stdDevExpression.Match(aggregateExpression);

                if (filterMatch.Success)
                    return new Tuple<Aggregate, string, bool>(Aggregate.StdDev, filterMatch.Result("${Expression}").Trim(), setOperation);

                // Look for sampled-based standard deviation aggregate
                lock (s_stdDevSampExpression)
                    filterMatch = s_stdDevSampExpression.Match(aggregateExpression);

                if (filterMatch.Success)
                    return new Tuple<Aggregate, string, bool>(Aggregate.StDevSamp, filterMatch.Result("${Expression}").Trim(), setOperation);

                if (!setOperation)
                {
                    // Look for time integration aggregate
                    lock (s_timeIntExpression)
                        filterMatch = s_timeIntExpression.Match(aggregateExpression);

                    if (filterMatch.Success)
                        return new Tuple<Aggregate, string, bool>(Aggregate.TimeInt, filterMatch.Result("${Expression}").Trim(), false);

                    // Look for derivative aggregate
                    lock (s_derivativeExpression)
                        filterMatch = s_derivativeExpression.Match(aggregateExpression);

                    if (filterMatch.Success)
                        return new Tuple<Aggregate, string, bool>(Aggregate.Derivative, filterMatch.Result("${Expression}").Trim(), false);
                }

                // Target is not a recognized aggregate
                return new Tuple<Aggregate, string, bool>(Aggregate.None, aggregateExpression, false);
            });

            if (result.Item1 == Aggregate.None)
                throw new InvalidOperationException($"Unrecognized aggregate type \"{aggregateExpression}\"");

            return result;
        }

        private List<TimeSeriesValues> ExecuteAggregate(Tuple<Aggregate, string, bool> parsedAggregate, DateTime startTime, DateTime stopTime, int maxDataPoints, CancellationToken cancellationToken)
        {
            List<TimeSeriesValues> results = new List<TimeSeriesValues>();
            List<TimeSeriesValues> dataset;
            TimeSeriesValues result;

            Aggregate aggregate = parsedAggregate.Item1;
            string expression = parsedAggregate.Item2;
            bool setOperation = parsedAggregate.Item3;

            // Handle min and max set operations as special cases
            if (setOperation && (aggregate == Aggregate.Minimum || aggregate == Aggregate.Maximum))
            {
                // Execute expression as a non-set function to get min/max of each series
                dataset = ExecuteAggregate(new Tuple<Aggregate, string, bool>(aggregate, expression, false), startTime, stopTime, maxDataPoints, cancellationToken);

                if (aggregate == Aggregate.Minimum)
                {
                    result = dataset.MinBy(series => series.datapoints[0][TimeSeriesValues.Value]);
                    result.target = $"SetMinimum = {result.target}";
                }
                else
                {
                    result = dataset.MaxBy(series => series.datapoints[0][TimeSeriesValues.Value]);
                    result.target = $"SetMaximum = {result.target}";
                }

                results.Add(result);
                return results;
            }

            // Query aggregate expression to get series data
            dataset = QueryTimeSeriesValuesFromTargets(new[] { expression }, startTime, stopTime, maxDataPoints, cancellationToken);

            if (dataset.Count == 0 || cancellationToken.IsCancellationRequested)
                return results;

            if (setOperation)
            {
                result = new TimeSeriesValues
                {
                    target = $"Set{aggregate}({expression})",
                    datapoints = new List<double[]>()
                };

                IEnumerable<double> values = dataset.Select(series => series.datapoints).SelectMany(points => points[TimeSeriesValues.Value]);
                double lastTime = dataset.Select(series => series.datapoints).SelectMany(points => points[TimeSeriesValues.Time]).Max();

                switch (aggregate)
                {
                    case Aggregate.Average:
                        result.datapoints.Add(new[] { values.Average(), lastTime });
                        break;
                    case Aggregate.Total:
                        result.datapoints.Add(new[] { values.Sum(), lastTime });
                        break;
                    case Aggregate.Range:
                        result.datapoints.Add(new[] { values.Max() - values.Min(), lastTime });
                        break;
                    case Aggregate.Count:
                        result.datapoints.Add(new[] { values.Count(), lastTime });
                        break;
                    case Aggregate.StdDev:
                        result.datapoints.Add(new[] { values.StandardDeviation(), lastTime });
                        break;
                    case Aggregate.StDevSamp:
                        result.datapoints.Add(new[] { values.StandardDeviation(true), lastTime });
                        break;
                    //case Aggregate.TimeInt:
                    //    double integratedValue = 0.0D;
                    //    currentSeries = values.ToArray();
                    //    currentTimes = times.ToArray();

                    //    for (int i = 1; i < currentSeries.Length; i++)
                    //        integratedValue += currentSeries[i] * (currentTimes[i] - currentTimes[i - 1]);

                    //    result.datapoints.Add(new [] { integratedValue, lastTime });
                    //    break;
                    //case Aggregate.Derivative:
                    //    currentSeries = values.ToArray();
                    //    currentTimes = times.ToArray();

                    //    for (int i = 1; i < currentSeries.Length; i++)
                    //        result.datapoints.Add(new[] { currentSeries[i] - currentSeries[i - 1], currentTimes[i] });

                    //    break;
                }

                results.Add(result);
            }
            else
            {
                foreach (TimeSeriesValues source in dataset)
                {
                    result = new TimeSeriesValues
                    {
                        target = $"{aggregate}({source.target})",
                        datapoints = new List<double[]>()
                    };

                    IEnumerable<double> values = source.datapoints.Select(points => points[TimeSeriesValues.Value]);
                    double lastTime = source.datapoints[source.datapoints.Count - 1][TimeSeriesValues.Time];
                    double value;

                    switch (aggregate)
                    {
                        case Aggregate.Minimum:
                            double minValue = double.MaxValue;
                            int minIndex = 0;

                            for (int i = 0; i < source.datapoints.Count; i++)
                            {
                                value = source.datapoints[i][TimeSeriesValues.Value];

                                if (value < minValue)
                                {
                                    minValue = value;
                                    minIndex = i;
                                }
                            }

                            result.datapoints.Add(new[] { minValue, source.datapoints[minIndex][TimeSeriesValues.Time] });
                            break;
                        case Aggregate.Maximum:
                            double maxValue = double.MinValue;
                            int maxIndex = 0;

                            for (int i = 0; i < source.datapoints.Count; i++)
                            {
                                value = source.datapoints[i][TimeSeriesValues.Value];

                                if (value > maxValue)
                                {
                                    maxValue = value;
                                    maxIndex = i;
                                }
                            }

                            result.datapoints.Add(new[] { maxValue, source.datapoints[maxIndex][TimeSeriesValues.Time] });
                            break;
                        case Aggregate.Average:
                            result.datapoints.Add(new[] { values.Average(), lastTime });
                            break;
                        case Aggregate.Total:
                            result.datapoints.Add(new[] { values.Sum(), lastTime });
                            break;
                        case Aggregate.Range:
                            result.datapoints.Add(new[] { values.Max() - values.Min(), lastTime });
                            break;
                        case Aggregate.Count:
                            result.datapoints.Add(new[] { source.datapoints.Count, lastTime });
                            break;
                        case Aggregate.StdDev:
                            result.datapoints.Add(new[] { values.StandardDeviation(), lastTime });
                            break;
                        case Aggregate.StDevSamp:
                            result.datapoints.Add(new[] { values.StandardDeviation(true), lastTime });
                            break;
                        case Aggregate.TimeInt:
                            double integratedValue = 0.0D;

                            for (int i = 1; i < source.datapoints.Count; i++)
                                integratedValue += source.datapoints[i][TimeSeriesValues.Value] * (source.datapoints[i][TimeSeriesValues.Time] - source.datapoints[i - 1][TimeSeriesValues.Time]);

                            result.datapoints.Add(new[] { integratedValue, lastTime });
                            break;
                        case Aggregate.Derivative:
                            for (int i = 1; i < source.datapoints.Count; i++)
                                result.datapoints.Add(new[] { source.datapoints[i][TimeSeriesValues.Value] - source.datapoints[i -1][TimeSeriesValues.Value], source.datapoints[i][TimeSeriesValues.Time] });

                            break;
                    }

                    results.Add(result);
                }
            }

            return results;
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
            // TODO: Make openHistorian Grafana data source metric query more interactive, adding drop-downs and/or query builders

            // For now, just return a truncated list of tag names
            return Task.Factory.StartNew(() => { return Metadata.Tables["ActiveMeasurements"].Select($"ID LIKE '{InstanceName}:%'").Take(MaximumSearchTargetsPerRequest).Select(row => $"{row["PointTag"]}").ToArray(); });
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

                        type.PopulateResponse(response, target, definition, datapoint, Metadata);

                        responses.Add(response);
                    }
                }
            }

            return responses;
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly Regex s_aggregateExpressions;
        private static readonly Regex s_averageExpression;
        private static readonly Regex s_minimumExpression;
        private static readonly Regex s_maximumExpression;
        private static readonly Regex s_totalExpression;
        private static readonly Regex s_rangeExpression;
        private static readonly Regex s_countExpression;
        private static readonly Regex s_timeIntExpression;
        private static readonly Regex s_derivativeExpression;
        private static readonly Regex s_stdDevExpression;
        private static readonly Regex s_stdDevSampExpression;

        // Static Constructor
        static GrafanaDataSourceBase()
        {
            const string ExtractAggregateExpression = @"{0}\s*\(\s*(?<Expression>.+)\s*\)";

            // RegEx instance to find all aggregates
            s_aggregateExpressions = new Regex(@"(SET)?\w+\s*\(([^)]+[\)\s]*)\)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            // RegEx instances to identify specific aggregates and extract internal expressions
            s_averageExpression = new Regex(string.Format(ExtractAggregateExpression, "(Average|Avg)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_minimumExpression = new Regex(string.Format(ExtractAggregateExpression, "(Minimum|Min)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_maximumExpression = new Regex(string.Format(ExtractAggregateExpression, "(Maximum|Max)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_totalExpression = new Regex(string.Format(ExtractAggregateExpression, "(Total|Sum)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_rangeExpression = new Regex(string.Format(ExtractAggregateExpression, "Range"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_countExpression = new Regex(string.Format(ExtractAggregateExpression, "Count"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_timeIntExpression = new Regex(string.Format(ExtractAggregateExpression, "(TimeIntegration|TimeInt)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_derivativeExpression = new Regex(string.Format(ExtractAggregateExpression, "(Derivative|Der)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_stdDevExpression = new Regex(string.Format(ExtractAggregateExpression, "(StandardDeviation|StdDev)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_stdDevSampExpression = new Regex(string.Format(ExtractAggregateExpression, "(StandardDeviationSample|StdDevSamp)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        #endregion
    }
}
