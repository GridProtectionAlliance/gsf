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
using GSF;
using GSF.Collections;
using GSF.NumericalAnalysis;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using GSF.Web;

// ReSharper disable PossibleMultipleEnumeration
namespace GrafanaAdapters
{
    #region [ Enumerations ]

    /// <summary>
    /// Defines available functions that can operate on <see cref="TimeSeriesValues"/>.
    /// </summary>
    public enum SeriesFunction
    {
        /// <summary>
        /// Returns a single value that represents the mean of the values in the source series.
        /// </summary>
        /// <remarks>
        /// Example: <c>Average(FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
        /// Variants: Average, Avg, Mean
        /// </remarks>
        Average,
        /// <summary>
        /// Returns a single value that is the minimum of the values in the source series.
        /// </summary>
        /// <remarks>
        /// Example: <c>Minimum(FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
        /// Variants: Minimum, Min
        /// </remarks>
        Minimum,
        /// <summary>
        /// Returns a single value that is the maximum of the values in the source series.
        /// </summary>
        /// <remarks>
        /// Example: <c>Maximum(FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
        /// Variants: Maximum, Max
        /// </remarks>
        Maximum,
        /// <summary>
        /// Returns a single value that represents the sum of the values in the source series.
        /// </summary>
        /// <remarks>
        /// Example: <c>Total(FILTER ActiveMeasurements WHERE SignalType='IPHM')</c><br/>
        /// Variants: Total, Sum
        /// </remarks>
        Total,
        /// <summary>
        /// Returns a single value that represents the range, i.e., maximum - minimum, of the values in the source series.
        /// </summary>
        /// <remarks>
        /// Example: <c>Range(FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
        /// Variants: Range
        /// </remarks>
        Range,
        /// <summary>
        /// Returns a single value that is the count of the values in the source series.
        /// </summary>
        /// <remarks>
        /// Example: <c>Count(PPA:1; PPA:2; PPA:3)</c><br/>
        /// Variants: Count
        /// </remarks>
        Count,
        /// <summary>
        /// Returns a series of values that represent the unique set of values in the source series.
        /// </summary>
        /// <remarks>
        /// Example: <c>Distinct(FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
        /// Variants: Distinct, Unique
        /// </remarks>
        Distinct,
        /// <summary>
        /// Returns a series of values that represent the absolute value each of the values in the source series.
        /// </summary>
        /// <remarks>
        /// Example: <c>AbsoluteValue(FILTER ActiveMeasurements WHERE SignalType='CALC')</c><br/>
        /// Variants: AbsoluteValue, Abs
        /// </remarks>
        AbsoluteValue,
        /// <summary>
        /// Returns a single value that represents the standard deviation of the values in the source series.
        /// </summary>
        /// <remarks>
        /// Example: <c>StandardDeviation(FILTER ActiveMeasurements WHERE SignalType='VPHM')</c><br/>
        /// Variants: StandardDeviation, StdDev
        /// </remarks>
        StandardDeviation,
        /// <summary>
        /// Returns a single value that represents the standard deviation, using sample calculation, of the values in the source series.
        /// </summary>
        /// <remarks>
        /// Example: <c>StandardDeviationSample(FILTER ActiveMeasurements WHERE SignalType='VPHM')</c><br/>
        /// Variants: StandardDeviationSample, StdDevSamp
        /// </remarks>
        StandardDeviationSample,
        /// <summary>
        /// Returns a single value that represents the median of the values in the source series.
        /// </summary>
        /// <remarks>
        /// Example: <c>Median(FILTER ActiveMeasurements WHERE SignalType='ALOG')</c><br/>
        /// Variants: Median, Med, Mid
        /// </remarks>
        Median,
        /// <summary>
        /// Returns a single value that represents the mode of the values in the source series.
        /// </summary>
        /// <remarks>
        /// Example: <c>Mode(FILTER TOP 5 ActiveMeasurements WHERE SignalType='DIGI')</c><br/>
        /// Variants: Mode
        /// </remarks>
        Mode,
        /// <summary>
        /// Returns a series of N, or N% of total, values that are the largest in the source series.
        /// N is either a positive integer value, representing a total, that is greater than zero - or - a floating point value, representing a percentage, that must range from greater than 0 to less than or equal to 100.
        /// </summary>
        /// <remarks>
        /// Example: <c>Top(50%, FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
        /// Variants: Top
        /// </remarks>
        Top,
        /// <summary>
        /// Returns a series of N, or N% of total, values that are the smallest in the source series.
        /// N is either a positive integer value, representing a total, that is greater than zero - or - a floating point value, representing a percentage, that must range from greater than 0 to less than or equal to 100.
        /// </summary>
        /// <remarks>
        /// Example: <c>Bottom(100, FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
        /// Variants: Bottom, Bot
        /// </remarks>
        Bottom,
        /// <summary>
        /// Returns a series of N, or N% of total, values that are a random sample of the values in the source series.
        /// N is either a positive integer value, representing a total, that is greater than zero - or - a floating point value, representing a percentage, that must range from greater than 0 to less than or equal to 100.
        /// Second parameter is a boolean flag representing if time in dataset should be normalized.
        /// </summary>
        /// <remarks>
        /// Example: <c>Random(25%, true, FILTER ActiveMeasurements WHERE SignalType='VPHM')</c><br/>
        /// Variants: Random, Rnd, Sample
        /// </remarks>
        Random,
        /// <summary>
        /// Returns a single value that is the first value, as sorted by time, in the source series.
        /// </summary>
        /// <remarks>
        /// Example: <c>First(FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
        /// Variants: First
        /// </remarks>
        First,
        /// <summary>
        /// Returns a single value that is the last value, as sorted by time, in the source series.
        /// </summary>
        /// <remarks>
        /// Example: <c>Last(FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
        /// Variants: Last
        /// </remarks>
        Last,
        /// <summary>
        /// Returns a single value that represents the Nth order percentile for the sorted values in the source series.
        /// N is a floating point value, representing a percentage, that must range from 0 to 100.
        /// </summary>
        /// <remarks>
        /// Example: <c>Percentile(10%, FILTER ActiveMeasurements WHERE SignalType='VPHM')</c><br/>
        /// Variants: Percentile, Pctl
        /// </remarks>
        Percentile,
        /// <summary>
        /// Returns a series of values that represent the difference between consecutive values in the source series.
        /// </summary>
        /// <remarks>
        /// Example: <c>Difference(FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
        /// Variants: Difference, Diff
        /// </remarks>
        Difference,
        /// <summary>
        /// Returns a series of values that represent the time difference, in seconds, between consecutive values in the source series.
        /// </summary>
        /// <remarks>
        /// Example: <c>TimeDifference(FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
        /// Variants: TimeDifference, TimeDiff
        /// </remarks>
        TimeDifference,
        /// <summary>
        /// Returns a series of values that represent the rate of change, per second, for the difference between consecutive values in the source series.
        /// </summary>
        /// <remarks>
        /// Example: <c>Derivative(FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
        /// Variants: Derivative, Der
        /// </remarks>
        Derivative,
        /// <summary>
        /// Returns a single value that represents the time-based integration, i.e., the sum of V(n) * (T(n) - T(n-1)), of the values in the source series.
        /// </summary>
        /// <remarks>
        /// Example: <c>TimeIntegration(FILTER ActiveMeasurements WHERE SignalType='CALC' AND PointTag LIKE '%-MW:%')</c><br/>
        /// Variants: TimeIntegration, TimeInt
        /// </remarks>
        TimeIntegration,
        /// <summary>
        /// Not a recognized function.
        /// </summary>
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
        /// Queries data source returning data as Grafana time-series data set.
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

        /// <summary>
        /// Search data source for a target.
        /// </summary>
        /// <param name="request">Search target.</param>
        public virtual Task<string[]> Search(Target request)
        {
            // TODO: Make Grafana data source metric query more interactive, adding drop-downs and/or query builders
            // For now, just return a truncated list of tag names
            return Task.Factory.StartNew(() => { return Metadata.Tables["ActiveMeasurements"].Select($"ID LIKE '{InstanceName}:%'").Take(MaximumSearchTargetsPerRequest).Select(row => $"{row["PointTag"]}").ToArray(); });
        }

        /// <summary>
        /// Queries data source for annotations in a time-range (e.g., Alarms).
        /// </summary>
        /// <param name="request">Annotation request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Queried annotations from data source.</returns>
        public virtual async Task<List<AnnotationResponse>> Annotations(AnnotationRequest request, CancellationToken cancellationToken)
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

        private List<TimeSeriesValues> QueryTimeSeriesValuesFromTargets(IEnumerable<string> targets, DateTime startTime, DateTime stopTime, int maxDataPoints, CancellationToken cancellationToken)
        {
            // A single target might look like the following:
            // PPA:15; STAT:20; SETSUM(COUNT(PPA:8; PPA:9; PPA:10)); FILTER ActiveMeasurements WHERE SignalType = 'VPHA'; RANGE(PPA:99; SUM(FILTER ActiveMeasurements WHERE SignalType = 'FREQ'; STAT:12))

            List<TimeSeriesValues> results = new List<TimeSeriesValues>();
            HashSet<string> targetSet = new HashSet<string>(targets, StringComparer.OrdinalIgnoreCase); // Targets include user provided input, so casing should be ignored
            HashSet<string> reducedTargetSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            List<Match> seriesFunctions = new List<Match>();

            foreach (string target in targetSet)
            {
                // Find any series functions in target
                Match[] matchedFunctions = TargetCache<Match[]>.GetOrAdd(target, () =>
                {
                    lock (s_seriesFunctions)
                        return s_seriesFunctions.Matches(target).Cast<Match>().ToArray();
                });

                if (matchedFunctions.Length > 0)
                {
                    seriesFunctions.AddRange(matchedFunctions);

                    // Reduce target to non-function expressions - important so later split on ';' succeeds properly
                    string reducedTarget = target;

                    foreach (string expression in matchedFunctions.Select(match => match.Value))
                        reducedTarget = reducedTarget.Replace(expression, "");

                    if (!string.IsNullOrWhiteSpace(reducedTarget))
                        reducedTargetSet.Add(reducedTarget);
                }
                else
                {
                    reducedTargetSet.Add(target);
                }
            }

            if (seriesFunctions.Count > 0)
            {
                // Parse series functions
                IEnumerable<Tuple<SeriesFunction, string, bool>> parsedFunctions = seriesFunctions.Select(ParseSeriesFunction);

                // Execute series functions
                foreach (Tuple<SeriesFunction, string, bool> parsedFunction in parsedFunctions)
                    results.AddRange(ExecuteSeriesFunction(parsedFunction, startTime, stopTime, maxDataPoints, cancellationToken));

                // Use reduced target set that excludes any series functions
                targetSet = reducedTargetSet;
            }

            // Query any remaining targets
            if (targetSet.Count > 0)
            {
                // Split remaining targets on semi-colon, this way even multiple filter expressions can be used as inputs to functions
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

        private Tuple<SeriesFunction, string, bool> ParseSeriesFunction(Match matchedFunction)
        {
            bool setOperation = matchedFunction.Groups[1].Success;
            string expression = setOperation ? matchedFunction.Value.Substring(3) : matchedFunction.Value;
            Tuple<SeriesFunction, string, bool> result = TargetCache<Tuple<SeriesFunction, string, bool>>.GetOrAdd(matchedFunction.Value, () =>
            {
                Match filterMatch;

                // Look for average function
                lock (s_averageExpression)
                    filterMatch = s_averageExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, bool>(SeriesFunction.Average, filterMatch.Result("${Expression}").Trim(), setOperation);

                // Look for minimum function
                lock (s_minimumExpression)
                    filterMatch = s_minimumExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, bool>(SeriesFunction.Minimum, filterMatch.Result("${Expression}").Trim(), setOperation);

                // Look for maximum function
                lock (s_maximumExpression)
                    filterMatch = s_maximumExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, bool>(SeriesFunction.Maximum, filterMatch.Result("${Expression}").Trim(), setOperation);

                // Look for total function
                lock (s_totalExpression)
                    filterMatch = s_totalExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, bool>(SeriesFunction.Total, filterMatch.Result("${Expression}").Trim(), setOperation);

                // Look for range function
                lock (s_rangeExpression)
                    filterMatch = s_rangeExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, bool>(SeriesFunction.Range, filterMatch.Result("${Expression}").Trim(), setOperation);

                // Look for count function
                lock (s_countExpression)
                    filterMatch = s_countExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, bool>(SeriesFunction.Count, filterMatch.Result("${Expression}").Trim(), setOperation);

                // Look for distinct function
                lock (s_distinctExpression)
                    filterMatch = s_distinctExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, bool>(SeriesFunction.Distinct, filterMatch.Result("${Expression}").Trim(), setOperation);

                // Look for absolute value function
                lock (s_absoluteValueExpression)
                    filterMatch = s_absoluteValueExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, bool>(SeriesFunction.AbsoluteValue, filterMatch.Result("${Expression}").Trim(), setOperation);

                // Look for standard deviation function
                lock (s_standardDeviationExpression)
                    filterMatch = s_standardDeviationExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, bool>(SeriesFunction.StandardDeviation, filterMatch.Result("${Expression}").Trim(), setOperation);

                // Look for sampled-based standard deviation function
                lock (s_standardDeviationSampleExpression)
                    filterMatch = s_standardDeviationSampleExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, bool>(SeriesFunction.StandardDeviationSample, filterMatch.Result("${Expression}").Trim(), setOperation);

                // Look for median function
                lock (s_medianExpression)
                    filterMatch = s_medianExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, bool>(SeriesFunction.Median, filterMatch.Result("${Expression}").Trim(), setOperation);

                // Look for mode function
                lock (s_modeExpression)
                    filterMatch = s_modeExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, bool>(SeriesFunction.Mode, filterMatch.Result("${Expression}").Trim(), setOperation);

                // Look for top function
                lock (s_topExpression)
                    filterMatch = s_topExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, bool>(SeriesFunction.Top, filterMatch.Result("${Expression}").Trim(), setOperation);

                // Look for bottom function
                lock (s_bottomExpression)
                    filterMatch = s_bottomExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, bool>(SeriesFunction.Bottom, filterMatch.Result("${Expression}").Trim(), setOperation);

                // Look for random function
                lock (s_randomExpression)
                    filterMatch = s_randomExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, bool>(SeriesFunction.Random, filterMatch.Result("${Expression}").Trim(), setOperation);

                // Look for first function
                lock (s_firstExpression)
                    filterMatch = s_firstExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, bool>(SeriesFunction.First, filterMatch.Result("${Expression}").Trim(), setOperation);

                // Look for last function
                lock (s_lastExpression)
                    filterMatch = s_lastExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, bool>(SeriesFunction.Last, filterMatch.Result("${Expression}").Trim(), setOperation);

                // Look for percentile function
                lock (s_percentileExpression)
                    filterMatch = s_percentileExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, bool>(SeriesFunction.Percentile, filterMatch.Result("${Expression}").Trim(), setOperation);

                // Look for difference function
                lock (s_differenceExpression)
                    filterMatch = s_differenceExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, bool>(SeriesFunction.Difference, filterMatch.Result("${Expression}").Trim(), setOperation);

                // Look for time difference function
                lock (s_timeDifferenceExpression)
                    filterMatch = s_timeDifferenceExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, bool>(SeriesFunction.TimeDifference, filterMatch.Result("${Expression}").Trim(), setOperation);

                // Look for derivative function
                lock (s_derivativeExpression)
                    filterMatch = s_derivativeExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, bool>(SeriesFunction.Derivative, filterMatch.Result("${Expression}").Trim(), setOperation);

                // Look for time integration function
                lock (s_timeIntegrationExpression)
                    filterMatch = s_timeIntegrationExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, bool>(SeriesFunction.TimeIntegration, filterMatch.Result("${Expression}").Trim(), setOperation);

                // Target is not a recognized function
                return new Tuple<SeriesFunction, string, bool>(SeriesFunction.None, expression, false);
            });

            if (result.Item1 == SeriesFunction.None)
                throw new InvalidOperationException($"Unrecognized series function '{expression}'");

            return result;
        }

        private List<TimeSeriesValues> ExecuteSeriesFunction(Tuple<SeriesFunction, string, bool> parsedFunction, DateTime startTime, DateTime stopTime, int maxDataPoints, CancellationToken cancellationToken)
        {
            List<TimeSeriesValues> results = new List<TimeSeriesValues>();
            List<TimeSeriesValues> dataset;
            TimeSeriesValues result;

            SeriesFunction seriesFunction = parsedFunction.Item1;
            string expression = parsedFunction.Item2;
            bool setOperation = parsedFunction.Item3;

            // Handle edge-case set operations
            if (setOperation && (seriesFunction == SeriesFunction.Minimum || seriesFunction == SeriesFunction.Maximum))
            {
                // Execute expression as a non-set function to get result of each series
                dataset = ExecuteSeriesFunction(new Tuple<SeriesFunction, string, bool>(seriesFunction, expression, false), startTime, stopTime, maxDataPoints, cancellationToken);

                if (seriesFunction == SeriesFunction.Minimum)
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

            // Extract any needed function parameters
            int parameterCount = s_parameterCounts[seriesFunction]; // Safe: no lock needed since content doesn't change
            string[] parameters = new string[0];

            if (parameterCount > 0)
            {
                int index = 0;

                for (int i = 0; i < parameterCount && index > -1; i++)
                    index = expression.IndexOf(',', index + 1);

                if (index > -1)
                    parameters = expression.Substring(0, index).Split(',');

                if (parameters.Length == parameterCount)
                    expression = expression.Substring(index + 1).Trim();
                else
                    throw new FormatException($"Expected {parameterCount + 1} parameters, received {parameters.Length + 1} in: {seriesFunction}({expression})");
            }

            // Query function expression to get series data
            dataset = QueryTimeSeriesValuesFromTargets(new[] { expression }, startTime, stopTime, maxDataPoints, cancellationToken);

            if (dataset.Count == 0 || cancellationToken.IsCancellationRequested)
                return results;

            double percent;
            int count;

            if (setOperation)
            {
                result = new TimeSeriesValues
                {
                    target = $"Set{seriesFunction}({string.Join(", ", parameters)}{(parameters.Length > 0 ? ", " : "")}{expression})",
                    datapoints = new List<double[]>()
                };

                double[] currentSeries, currentTimes;
                IEnumerable<double> values = dataset.SelectMany(series => series.datapoints.Select(points => points[TimeSeriesValues.Value]));
                IEnumerable<double> times = dataset.SelectMany(series => series.datapoints.Select(points => points[TimeSeriesValues.Time]));
                IEnumerable<Tuple<TimeSeriesValues, double>> valuesWithSource = dataset.SelectMany(series => series.datapoints.Select(points => new Tuple<TimeSeriesValues, double>(series, points[TimeSeriesValues.Value])));

                switch (seriesFunction)
                {
                    case SeriesFunction.Average:
                        result.datapoints.Add(new[] { values.Average(), times.Max() });
                        break;
                    case SeriesFunction.Total:
                        result.datapoints.Add(new[] { values.Sum(), times.Max() });
                        break;
                    case SeriesFunction.Range:
                        result.datapoints.Add(new[] { values.Max() - values.Min(), times.Max() });
                        break;
                    case SeriesFunction.Count:
                        result.datapoints.Add(new[] { values.Count(), times.Max() });
                        break;
                    case SeriesFunction.Distinct:
                        result.datapoints.AddRange(dataset.SelectMany(series => series.datapoints).DistinctBy(point => point[TimeSeriesValues.Value], false));
                        break;
                    case SeriesFunction.AbsoluteValue:
                        currentSeries = values.ToArray();
                        currentTimes = times.ToArray();

                        for (int i = 0; i < currentSeries.Length; i++)
                            result.datapoints.Add(new[] { Math.Abs(currentSeries[i]), currentTimes[i] });

                        break;
                    case SeriesFunction.StandardDeviation:
                        result.datapoints.Add(new[] { values.StandardDeviation(), times.Max() });
                        break;
                    case SeriesFunction.StandardDeviationSample:
                        result.datapoints.Add(new[] { values.StandardDeviation(true), times.Max() });
                        break;
                    case SeriesFunction.Median:
                        Tuple<TimeSeriesValues, double> median = valuesWithSource.Median().Last();
                        result.datapoints.Add(new[] { median.Item2, median.Item1.datapoints.Reverse<double[]>().First(points => points[TimeSeriesValues.Value] == median.Item2)[TimeSeriesValues.Time] });
                        result.target = $"SetMedian = {median.Item1.target}";
                        break;
                    case SeriesFunction.Mode:
                        Tuple<TimeSeriesValues, double> mode = valuesWithSource.MajorityBy(valuesWithSource.Last(), key => key.Item2, false);
                        result.datapoints.Add(new[] { mode.Item2, mode.Item1.datapoints.Reverse<double[]>().First(points => points[TimeSeriesValues.Value] == mode.Item2)[TimeSeriesValues.Time] });
                        result.target = $"SetMode = {mode.Item1.target}";
                        break;
                    case SeriesFunction.Top:
                        // Is there a use case to want top items from dataset? e.g.:
                        //results.AddRange(dataset.Take(count));
                        count = ParseCount(parameters[0], dataset.Sum(series => series.datapoints.Count));
                        result.datapoints.AddRange(dataset.SelectMany(series => series.datapoints).Take(count));
                        break;
                    case SeriesFunction.Bottom:
                        // Is there a use case to want bottom items from dataset? e.g.:
                        //results.AddRange(dataset.Reverse<TimeSeriesValues>().Take(count));
                        count = ParseCount(parameters[0], dataset.Sum(series => series.datapoints.Count));
                        result.datapoints.AddRange(dataset.SelectMany(series => series.datapoints).Reverse().Take(count));
                        break;
                    case SeriesFunction.Random:
                        currentSeries = values.ToArray();
                        currentTimes = times.ToArray();
                        count = ParseCount(parameters[0], currentSeries.Length);

                        if (count > currentSeries.Length)
                            count = currentSeries.Length;

                        bool normalizeTime = parameters[1].Trim().ParseBoolean();
                        double timeStep = (currentTimes[currentTimes.Length - 1] - currentTimes[0]) / count;
                        List<int> indexes = new List<int>(Enumerable.Range(0, currentSeries.Length));

                        indexes.Scramble();
                        result.datapoints.AddRange(indexes.Take(count).Select((index, i) => new[] { currentSeries[index], normalizeTime ? currentTimes[0] + i * timeStep : currentTimes[index] }));
                        break;
                    case SeriesFunction.First:
                        result = dataset.First();
                        result.datapoints = new List<double[]>(new[] { result.datapoints.First() });    // First point of first series
                        result.target = $"SetFirst = {result.target}";
                        break;
                    case SeriesFunction.Last:
                        result = dataset.Last();
                        result.datapoints = new List<double[]>(new[] { result.datapoints.Last() });     // Last point of last series
                        result.target = $"SetLast = {result.target}";
                        break;
                    case SeriesFunction.Percentile:
                        percent = ParsePercentage(parameters[0]);
                        List<Tuple<TimeSeriesValues, double>> combinedSet = valuesWithSource.ToList();
                        combinedSet.Sort((a, b) => a.Item2 < b.Item2 ? -1 : (a.Item2 > b.Item2 ? 1 : 0));

                        if (percent == 0.0D)
                        {
                            result = combinedSet.First().Item1;
                            result.datapoints = new List<double[]>(new[] { result.datapoints.First() });    // First point of first series
                            result.target = $"SetPercentile = {result.target}";
                        }
                        else if (percent == 100.0D)
                        {
                            result = combinedSet.Last().Item1;
                            result.datapoints = new List<double[]>(new[] { result.datapoints.Last() });     // Last point of last series
                            result.target = $"SetPercentile = {result.target}";
                        }
                        else
                        {
                            double n = (combinedSet.Count - 1) * (percent / 100.0D) + 1.0D;
                            int k = (int)n;
                            double d = n - k;
                            double k0 = combinedSet[k - 1].Item2;
                            double k1 = combinedSet[k].Item2;
                            List<double[]> kvals = combinedSet[k].Item1.datapoints;
                            IEnumerable<double> ktimes = kvals.Select(points => points[TimeSeriesValues.Time]);

                            result.datapoints.Add(new[] { k0 + d * (k1 - k0), ktimes.ElementAt((int)((kvals.Count - 1) * (percent / 100.0D))) });
                            result.target = $"SetPercentile = {combinedSet[k].Item1.target}";
                        }
                        break;
                    case SeriesFunction.Difference:
                        currentSeries = values.ToArray();
                        currentTimes = times.ToArray();

                        for (int i = 1; i < currentSeries.Length; i++)
                            result.datapoints.Add(new[] { currentSeries[i] - currentSeries[i - 1], currentTimes[i] });

                        break;
                    case SeriesFunction.TimeDifference:
                        currentTimes = times.ToArray();

                        for (int i = 1; i < currentTimes.Length; i++)
                            result.datapoints.Add(new[] { currentTimes[i] - currentTimes[i - 1], currentTimes[i] });

                        break;
                    case SeriesFunction.Derivative:
                        currentSeries = values.ToArray();
                        currentTimes = times.ToArray();

                        for (int i = 1; i < currentSeries.Length; i++)
                            result.datapoints.Add(new[] { (currentSeries[i] - currentSeries[i - 1]) / (currentTimes[i] - currentTimes[i - 1]), currentTimes[i] });

                        break;
                    case SeriesFunction.TimeIntegration:
                        double integratedValue = 0.0D;
                        currentSeries = values.ToArray();
                        currentTimes = times.ToArray();

                        for (int i = 1; i < currentSeries.Length; i++)
                            integratedValue += currentSeries[i] * (currentTimes[i] - currentTimes[i - 1]);

                        result.datapoints.Add(new[] { integratedValue, times.Max() });
                        break;
                }

                results.Add(result);
            }
            else
            {
                foreach (TimeSeriesValues series in dataset)
                {
                    result = new TimeSeriesValues
                    {
                        target = $"{seriesFunction}({string.Join(", ", parameters)}{(parameters.Length > 0 ? ", " : "")}{series.target})",
                        datapoints = new List<double[]>()
                    };

                    IEnumerable<double> values = series.datapoints.Select(points => points[TimeSeriesValues.Value]);
                    double lastTime = series.datapoints[series.datapoints.Count - 1][TimeSeriesValues.Time];
                    double value;

                    switch (seriesFunction)
                    {
                        case SeriesFunction.Minimum:
                            double minValue = double.MaxValue;
                            int minIndex = 0;

                            for (int i = 0; i < series.datapoints.Count; i++)
                            {
                                value = series.datapoints[i][TimeSeriesValues.Value];

                                if (value < minValue)
                                {
                                    minValue = value;
                                    minIndex = i;
                                }
                            }

                            result.datapoints.Add(new[] { minValue, series.datapoints[minIndex][TimeSeriesValues.Time] });
                            break;
                        case SeriesFunction.Maximum:
                            double maxValue = double.MinValue;
                            int maxIndex = 0;

                            for (int i = 0; i < series.datapoints.Count; i++)
                            {
                                value = series.datapoints[i][TimeSeriesValues.Value];

                                if (value > maxValue)
                                {
                                    maxValue = value;
                                    maxIndex = i;
                                }
                            }

                            result.datapoints.Add(new[] { maxValue, series.datapoints[maxIndex][TimeSeriesValues.Time] });
                            break;
                        case SeriesFunction.Average:
                            result.datapoints.Add(new[] { values.Average(), lastTime });
                            break;
                        case SeriesFunction.Total:
                            result.datapoints.Add(new[] { values.Sum(), lastTime });
                            break;
                        case SeriesFunction.Range:
                            result.datapoints.Add(new[] { values.Max() - values.Min(), lastTime });
                            break;
                        case SeriesFunction.Count:
                            result.datapoints.Add(new[] { series.datapoints.Count, lastTime });
                            break;
                        case SeriesFunction.Distinct:
                            result.datapoints.AddRange(series.datapoints.DistinctBy(point => point[TimeSeriesValues.Value], false));
                            break;
                        case SeriesFunction.AbsoluteValue:
                            result.datapoints.AddRange(series.datapoints.Select(point => new[] { Math.Abs(point[TimeSeriesValues.Value]), point[TimeSeriesValues.Time] }));
                            break;
                        case SeriesFunction.StandardDeviation:
                            result.datapoints.Add(new[] { values.StandardDeviation(), lastTime });
                            break;
                        case SeriesFunction.StandardDeviationSample:
                            result.datapoints.Add(new[] { values.StandardDeviation(true), lastTime });
                            break;
                        case SeriesFunction.Median:
                            result.datapoints.Add(new[] { values.Median().Average(), lastTime });
                            break;
                        case SeriesFunction.Mode:
                            double mode = values.Majority(values.Last(), false);
                            result.datapoints.Add(new[] { mode, series.datapoints.Reverse<double[]>().First(points => points[TimeSeriesValues.Value] == mode)[TimeSeriesValues.Time] });
                            break;
                        case SeriesFunction.Top:
                            count = ParseCount(parameters[0], series.datapoints.Count);
                            result.datapoints.AddRange(series.datapoints.Take(count));
                            break;
                        case SeriesFunction.Bottom:
                            count = ParseCount(parameters[0], series.datapoints.Count);
                            result.datapoints.AddRange(series.datapoints.Reverse<double[]>().Take(count));
                            break;
                        case SeriesFunction.Random:
                            count = ParseCount(parameters[0], series.datapoints.Count);

                            if (count > series.datapoints.Count)
                                count = series.datapoints.Count;

                            bool normalizeTime = parameters[1].Trim().ParseBoolean();
                            double timeStep = (series.datapoints[series.datapoints.Count - 1][TimeSeriesValues.Time] - series.datapoints[0][TimeSeriesValues.Time]) / count;
                            List<int> indexes = new List<int>(Enumerable.Range(0, series.datapoints.Count));

                            indexes.Scramble();
                            result.datapoints.AddRange(indexes.Take(count).Select((index, i) => new[] { series.datapoints[index][TimeSeriesValues.Value], normalizeTime ? series.datapoints[0][TimeSeriesValues.Time] + i * timeStep : series.datapoints[index][TimeSeriesValues.Time] }));
                            break;
                        case SeriesFunction.First:
                            result.datapoints.Add(series.datapoints.First());
                            break;
                        case SeriesFunction.Last:
                            result.datapoints.Add(series.datapoints.Last());
                            break;
                        case SeriesFunction.Percentile:
                            percent = ParsePercentage(parameters[0]);
                            series.datapoints.Sort((a, b) => a[TimeSeriesValues.Value] < b[TimeSeriesValues.Value] ? -1 : (a[TimeSeriesValues.Value] > b[TimeSeriesValues.Value] ? 1 : 0));
                            count = series.datapoints.Count;

                            if (percent == 0.0D)
                            {
                                result.datapoints.Add(series.datapoints.First());
                            }
                            else if (percent == 100.0D)
                            {
                                result.datapoints.Add(series.datapoints.Last());
                            }
                            else
                            {
                                double n = (count - 1) * (percent / 100.0D) + 1.0D;
                                int k = (int)n;
                                double d = n - k;
                                double k0 = series.datapoints[k - 1][TimeSeriesValues.Value];
                                double k1 = series.datapoints[k][TimeSeriesValues.Value];
                                result.datapoints.Add(new[] { k0 + d * (k1 - k0), series.datapoints[k][TimeSeriesValues.Time] });
                            }
                            break;
                        case SeriesFunction.Difference:
                            for (int i = 1; i < series.datapoints.Count; i++)
                                result.datapoints.Add(new[] { series.datapoints[i][TimeSeriesValues.Value] - series.datapoints[i - 1][TimeSeriesValues.Value], series.datapoints[i][TimeSeriesValues.Time] });

                            break;
                        case SeriesFunction.TimeDifference:
                            for (int i = 1; i < series.datapoints.Count; i++)
                                result.datapoints.Add(new[] { series.datapoints[i][TimeSeriesValues.Time] - series.datapoints[i - 1][TimeSeriesValues.Time], series.datapoints[i][TimeSeriesValues.Time] });

                            break;
                        case SeriesFunction.Derivative:
                            for (int i = 1; i < series.datapoints.Count; i++)
                                result.datapoints.Add(new[] { (series.datapoints[i][TimeSeriesValues.Value] - series.datapoints[i - 1][TimeSeriesValues.Value]) / (series.datapoints[i][TimeSeriesValues.Time] - series.datapoints[i - 1][TimeSeriesValues.Time]), series.datapoints[i][TimeSeriesValues.Time] });

                            break;
                        case SeriesFunction.TimeIntegration:
                            double integratedValue = 0.0D;

                            for (int i = 1; i < series.datapoints.Count; i++)
                                integratedValue += series.datapoints[i][TimeSeriesValues.Value] * (series.datapoints[i][TimeSeriesValues.Time] - series.datapoints[i - 1][TimeSeriesValues.Time]);

                            result.datapoints.Add(new[] { integratedValue, lastTime });
                            break;
                    }

                    results.Add(result);
                }
            }

            return results;
        }

        private int ParseCount(string parameter, int length)
        {
            int count;

            if (length > 0 && parameter.Contains("%"))
            {
                double percent = ParsePercentage(parameter, false);
                count = (int)(length * (percent / 100.0D));

                if (count == 0)
                    count = 1;
            }
            else
            {
                if (!int.TryParse(parameter, out count))
                    throw new FormatException($"Could not parse '{parameter}' as an integer value.");

                if (count < 1)
                    throw new ArgumentOutOfRangeException($"Count '{parameter}' is less than one.");
            }

            return count;
        }

        private double ParsePercentage(string parameter, bool includeZero = true)
        {
            double percent;

            parameter = parameter.Trim();

            if (parameter.EndsWith("%"))
                parameter = parameter.Substring(0, parameter.Length - 1);

            if (!double.TryParse(parameter, out percent))
                throw new FormatException($"Could not parse '{parameter}' as a floating-point value.");

            if (includeZero)
            {
                if (percent < 0.0D || percent > 100.0D)
                    throw new ArgumentOutOfRangeException($"Percentage '{parameter}' is outside range of 0 to 100, inclusive.");
            }
            else
            {
                if (percent <= 0.0D || percent > 100.0D)
                    throw new ArgumentOutOfRangeException($"Percentage '{parameter}' is outside range of greater than 0 and less than or equal to 100.");
            }

            return percent;
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly Regex s_seriesFunctions;
        private static readonly Regex s_averageExpression;
        private static readonly Regex s_minimumExpression;
        private static readonly Regex s_maximumExpression;
        private static readonly Regex s_totalExpression;
        private static readonly Regex s_rangeExpression;
        private static readonly Regex s_countExpression;
        private static readonly Regex s_distinctExpression;
        private static readonly Regex s_absoluteValueExpression;
        private static readonly Regex s_standardDeviationExpression;
        private static readonly Regex s_standardDeviationSampleExpression;
        private static readonly Regex s_medianExpression;
        private static readonly Regex s_modeExpression;
        private static readonly Regex s_topExpression;
        private static readonly Regex s_bottomExpression;
        private static readonly Regex s_randomExpression;
        private static readonly Regex s_firstExpression;
        private static readonly Regex s_lastExpression;
        private static readonly Regex s_percentileExpression;
        private static readonly Regex s_differenceExpression;
        private static readonly Regex s_timeDifferenceExpression;
        private static readonly Regex s_derivativeExpression;
        private static readonly Regex s_timeIntegrationExpression;
        private static readonly Dictionary<SeriesFunction, int> s_parameterCounts;

        // Static Constructor
        static GrafanaDataSourceBase()
        {
            const string GetExpression = @"{0}\s*\(\s*(?<Expression>.+)\s*\)";

            // RegEx instance to find all series functions
            s_seriesFunctions = new Regex(@"(SET)?\w+\s*\(([^)]+[\)\s]*)\)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            // RegEx instances to identify specific functions and extract internal expressions
            s_averageExpression = new Regex(string.Format(GetExpression, "(Average|Avg|Mean)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_minimumExpression = new Regex(string.Format(GetExpression, "(Minimum|Min)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_maximumExpression = new Regex(string.Format(GetExpression, "(Maximum|Max)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_totalExpression = new Regex(string.Format(GetExpression, "(Total|Sum)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_rangeExpression = new Regex(string.Format(GetExpression, "Range"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_countExpression = new Regex(string.Format(GetExpression, "Count"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_distinctExpression = new Regex(string.Format(GetExpression, "(Distinct|Unique)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_absoluteValueExpression = new Regex(string.Format(GetExpression, "(AbsoluteValue|Abs)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_standardDeviationExpression = new Regex(string.Format(GetExpression, "(StandardDeviation|StdDev)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_standardDeviationSampleExpression = new Regex(string.Format(GetExpression, "(StandardDeviationSample|StdDevSamp)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_medianExpression = new Regex(string.Format(GetExpression, "(Median|Med|Mid)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_modeExpression = new Regex(string.Format(GetExpression, "Mode"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_topExpression = new Regex(string.Format(GetExpression, "Top"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_bottomExpression = new Regex(string.Format(GetExpression, "(Bottom|Bot)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_randomExpression = new Regex(string.Format(GetExpression, "(Random|Rnd|Sample)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_firstExpression = new Regex(string.Format(GetExpression, "First"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_lastExpression = new Regex(string.Format(GetExpression, "Last"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_percentileExpression = new Regex(string.Format(GetExpression, "(Percentile|Pctl)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_differenceExpression = new Regex(string.Format(GetExpression, "(Difference|Diff)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_timeDifferenceExpression = new Regex(string.Format(GetExpression, "(TimeDifference|TimeDiff|Elapsed)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_derivativeExpression = new Regex(string.Format(GetExpression, "(Derivative|Der)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_timeIntegrationExpression = new Regex(string.Format(GetExpression, "(TimeIntegration|TimeInt)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);

            s_parameterCounts = new Dictionary<SeriesFunction, int>();

            s_parameterCounts[SeriesFunction.Average] = 0;
            s_parameterCounts[SeriesFunction.Minimum] = 0;
            s_parameterCounts[SeriesFunction.Maximum] = 0;
            s_parameterCounts[SeriesFunction.Total] = 0;
            s_parameterCounts[SeriesFunction.Range] = 0;
            s_parameterCounts[SeriesFunction.Count] = 0;
            s_parameterCounts[SeriesFunction.Distinct] = 0;
            s_parameterCounts[SeriesFunction.AbsoluteValue] = 0;
            s_parameterCounts[SeriesFunction.StandardDeviation] = 0;
            s_parameterCounts[SeriesFunction.StandardDeviationSample] = 0;
            s_parameterCounts[SeriesFunction.Median] = 0;
            s_parameterCounts[SeriesFunction.Mode] = 0;
            s_parameterCounts[SeriesFunction.Top] = 1;
            s_parameterCounts[SeriesFunction.Bottom] = 1;
            s_parameterCounts[SeriesFunction.Random] = 2;
            s_parameterCounts[SeriesFunction.First] = 0;
            s_parameterCounts[SeriesFunction.Last] = 0;
            s_parameterCounts[SeriesFunction.Percentile] = 1;
            s_parameterCounts[SeriesFunction.Difference] = 0;
            s_parameterCounts[SeriesFunction.TimeDifference] = 0;
            s_parameterCounts[SeriesFunction.Derivative] = 0;
            s_parameterCounts[SeriesFunction.TimeIntegration] = 0;
        }

        #endregion
    }
}
