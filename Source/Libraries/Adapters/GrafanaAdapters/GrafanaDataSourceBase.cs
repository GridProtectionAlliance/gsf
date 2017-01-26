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
using Random = GSF.Security.Cryptography.Random;

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
        Average,
        /// <summary>
        /// Returns a single value that is the minimum of the values in the source series.
        /// </summary>
        Minimum,
        /// <summary>
        /// Returns a single value that is the maximum of the values in the source series.
        /// </summary>
        Maximum,
        /// <summary>
        /// Returns a single value that represents the sum of the values in the source series.
        /// </summary>
        Total,
        /// <summary>
        /// Returns a single value that represents the range, i.e., maximum - minimum, of the values in the source series.
        /// </summary>
        Range,
        /// <summary>
        /// Returns a single value that is the count of the values in the source series.
        /// </summary>
        Count,
        /// <summary>
        /// Returns a single value that represents the standard deviation of the values in the source series.
        /// </summary>
        StdDev,
        /// <summary>
        /// Returns a single value that represents the standard deviation, using sample calculation, of the values in the source series.
        /// </summary>
        StDevSamp,
        /// <summary>
        /// Returns a single value that represents the median of the values in the source series.
        /// </summary>
        Median,
        /// <summary>
        /// Returns a single value that represents the mode of the values in the source series.
        /// </summary>
        Mode,
        /// <summary>
        /// Returns a series of N values that are the largest in the source series. N is a positive integer value greater than zero.
        /// </summary>
        Top,
        /// <summary>
        /// Returns a series of N values that are the smallest in the source series. N is a positive integer value greater than zero.
        /// </summary>
        Bottom,
        /// <summary>
        /// Returns a series of N values that are a random sample of the values in the source series. N is a positive integer value greater than zero.
        /// </summary>
        Random,
        /// <summary>
        /// Returns a single value that is the first value, as sorted by time, in the source series.
        /// </summary>
        First,
        /// <summary>
        /// Returns a single value that is the last value, as sorted by time, in the source series.
        /// </summary>
        Last,
        /// <summary>
        /// Returns a single value that represents the Nth order percentile for the sorted values in the source series. N is a floating point value that must range from 0 to 100.
        /// </summary>
        Percentile,
        /// <summary>
        /// Returns a series of values that represent the difference between consecutive values in the source series.
        /// </summary>
        Difference,
        /// <summary>
        /// Returns a series of values that represent the time difference, in seconds, between consecutive values in the source series.
        /// </summary>
        TimeDifference,
        /// <summary>
        /// Returns a series of values that represent the rate of change, per second, for the difference between consecutive values in the source series.
        /// </summary>
        Derivative,
        /// <summary>
        /// Returns a single value that represents the time-based integration, i.e., the sum of V(n) * (T(n) - T(n-1)), of the values in the source series.
        /// </summary>
        TimeInt,
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

                // Look for standard deviation function
                lock (s_stdDevExpression)
                    filterMatch = s_stdDevExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, bool>(SeriesFunction.StdDev, filterMatch.Result("${Expression}").Trim(), setOperation);

                // Look for sampled-based standard deviation function
                lock (s_stdDevSampExpression)
                    filterMatch = s_stdDevSampExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, bool>(SeriesFunction.StDevSamp, filterMatch.Result("${Expression}").Trim(), setOperation);

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
                lock (s_timeIntExpression)
                    filterMatch = s_timeIntExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, bool>(SeriesFunction.TimeInt, filterMatch.Result("${Expression}").Trim(), setOperation);

                // Target is not a recognized function
                return new Tuple<SeriesFunction, string, bool>(SeriesFunction.None, expression, false);
            });

            if (result.Item1 == SeriesFunction.None)
                throw new InvalidOperationException($"Unrecognized series function \"{expression}\"");

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
            int index;

            if (parameterCount > 1)
            {
                index = 0;

                for (int i = 0; i < parameterCount && index > -1; i++)
                    index = expression.IndexOf(',', index);

                if (index > -1)
                    parameters = expression.Substring(0, index).Split(',');

                if (parameters.Length == parameterCount)
                    expression = expression.Substring(index + 1).Trim();
                else
                    throw new FormatException($"Expected {parameterCount} parameter{(parameterCount == 1 ? "" : "s")}, received {parameters.Length} in: \"{expression}\"");
            }

            // Query function expression to get series data
            dataset = QueryTimeSeriesValuesFromTargets(new[] { expression }, startTime, stopTime, maxDataPoints, cancellationToken);

            if (dataset.Count == 0 || cancellationToken.IsCancellationRequested)
                return results;

            HashSet<int> indexes;
            double[] currentSeries, currentTimes;
            double percent;
            int count;

            if (setOperation)
            {
                Func<TimeSeriesValues> createNewResult = () => new TimeSeriesValues
                {
                    target = $"Set{seriesFunction}({expression})",
                    datapoints = new List<double[]>()
                };

                IEnumerable<double> values = dataset.Select(series => series.datapoints).SelectMany(points => points[TimeSeriesValues.Value]);
                IEnumerable<double> times = dataset.Select(series => series.datapoints).SelectMany(points => points[TimeSeriesValues.Time]);
                IEnumerable<Tuple<TimeSeriesValues, double>> valuesWithSource = dataset.SelectMany(series => series.datapoints.Select(points => new Tuple<TimeSeriesValues, double>(series, points[TimeSeriesValues.Value])));

                result = null;

                switch (seriesFunction)
                {
                    case SeriesFunction.Average:
                        result = createNewResult();
                        result.datapoints.Add(new[] { values.Average(), times.Max() });
                        break;
                    case SeriesFunction.Total:
                        result = createNewResult();
                        result.datapoints.Add(new[] { values.Sum(), times.Max() });
                        break;
                    case SeriesFunction.Range:
                        result = createNewResult();
                        result.datapoints.Add(new[] { values.Max() - values.Min(), times.Max() });
                        break;
                    case SeriesFunction.Count:
                        result = createNewResult();
                        result.datapoints.Add(new[] { values.Count(), times.Max() });
                        break;
                    case SeriesFunction.StdDev:
                        result = createNewResult();
                        result.datapoints.Add(new[] { values.StandardDeviation(), times.Max() });
                        break;
                    case SeriesFunction.StDevSamp:
                        result = createNewResult();
                        result.datapoints.Add(new[] { values.StandardDeviation(true), times.Max() });
                        break;
                    case SeriesFunction.Median:
                        result = dataset.Median().Last();
                        result.target = $"SetMedian = {result.target}";
                        break;
                    case SeriesFunction.Mode:
                        Tuple<TimeSeriesValues, double> mode = valuesWithSource.MajorityBy(valuesWithSource.Last(), key => key.Item2, false);
                        result = createNewResult();
                        result.datapoints.Add(new[] { mode.Item2, mode.Item1.datapoints.Reverse<double[]>().First(points => points[TimeSeriesValues.Value] == mode.Item2)[TimeSeriesValues.Time] });
                        result.target = $"SetMode = {result.target}";
                        break;
                    case SeriesFunction.Top:
                        if (!int.TryParse(parameters[0], out count) || count < 1)
                            throw new FormatException($"Could not parse \"{parameters[0]}\" as an integer or value is less than one.");

                        results.AddRange(dataset.Take(count));
                        break;
                    case SeriesFunction.Bottom:
                        if (!int.TryParse(parameters[0], out count) || count < 1)
                            throw new FormatException($"Could not parse \"{parameters[0]}\" as an integer or value is less than one.");

                        results.AddRange(dataset.Reverse<TimeSeriesValues>().Take(count));
                        break;
                    case SeriesFunction.Random:
                        if (!int.TryParse(parameters[0], out count) || count < 1)
                            throw new FormatException($"Could not parse \"{parameters[0]}\" as an integer or value is less than one.");

                        if (count > dataset.Count)
                            count = dataset.Count;

                        indexes = new HashSet<int>();

                        while (indexes.Count < count)
                        {
                            index = Random.Int32Between(0, dataset.Count - 1);

                            if (!indexes.Contains(index))
                            {
                                indexes.Add(index);
                                results.Add(dataset[index]);
                            }
                        }
                        break;
                    case SeriesFunction.First:
                        result = dataset.First();
                        result.target = $"SetFirst = {result.target}";
                        break;
                    case SeriesFunction.Last:
                        result = dataset.Last();
                        result.target = $"SetLast = {result.target}";
                        break;
                    case SeriesFunction.Percentile:
                        if (!double.TryParse(parameters[0], out percent) || percent <= 0.0D || percent >= 100.0D)
                            throw new FormatException($"Could not parse \"{parameters[0]}\" as a floating-point number or value is outside range of 0 to 100.");

                        // TODO: Do percentile function...

                        break;
                    case SeriesFunction.Difference:
                        result = createNewResult();
                        currentSeries = values.ToArray();
                        currentTimes = times.ToArray();

                        for (int i = 1; i < currentSeries.Length; i++)
                            result.datapoints.Add(new[] { currentSeries[i] - currentSeries[i - 1], currentTimes[i] });

                        break;
                    case SeriesFunction.TimeDifference:
                        result = createNewResult();
                        currentTimes = times.ToArray();

                        for (int i = 1; i < currentTimes.Length; i++)
                            result.datapoints.Add(new[] { currentTimes[i] - currentTimes[i - 1], currentTimes[i] });

                        break;
                    case SeriesFunction.Derivative:
                        result = createNewResult();
                        currentSeries = values.ToArray();
                        currentTimes = times.ToArray();

                        for (int i = 1; i < currentSeries.Length; i++)
                            result.datapoints.Add(new[] { (currentSeries[i] - currentSeries[i - 1]) / (currentTimes[i] - currentTimes[i - 1]), currentTimes[i] });

                        break;
                    case SeriesFunction.TimeInt:
                        double integratedValue = 0.0D;

                        result = createNewResult();
                        currentSeries = values.ToArray();
                        currentTimes = times.ToArray();

                        for (int i = 1; i < currentSeries.Length; i++)
                            integratedValue += currentSeries[i] * (currentTimes[i] - currentTimes[i - 1]);

                        result.datapoints.Add(new[] { integratedValue, times.Max() });
                        break;
                }

                if ((object)result != null)
                    results.Add(result);
            }
            else
            {
                foreach (TimeSeriesValues source in dataset)
                {
                    result = new TimeSeriesValues
                    {
                        target = $"{seriesFunction}({source.target})",
                        datapoints = new List<double[]>()
                    };

                    IEnumerable<double> values = source.datapoints.Select(points => points[TimeSeriesValues.Value]);
                    double lastTime = source.datapoints[source.datapoints.Count - 1][TimeSeriesValues.Time];
                    double value;

                    switch (seriesFunction)
                    {
                        case SeriesFunction.Minimum:
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
                        case SeriesFunction.Maximum:
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
                            result.datapoints.Add(new[] { source.datapoints.Count, lastTime });
                            break;
                        case SeriesFunction.StdDev:
                            result.datapoints.Add(new[] { values.StandardDeviation(), lastTime });
                            break;
                        case SeriesFunction.StDevSamp:
                            result.datapoints.Add(new[] { values.StandardDeviation(true), lastTime });
                            break;
                        case SeriesFunction.Median:
                            result.datapoints.Add(new[] { values.Median().Average(), lastTime });
                            break;
                        case SeriesFunction.Mode:
                            double mode = values.Majority(values.Last(), false);
                            result.datapoints.Add(new[] { mode, source.datapoints.Reverse<double[]>().First(points => points[TimeSeriesValues.Value] == mode)[TimeSeriesValues.Time] });
                            break;
                        case SeriesFunction.Top:
                            if (!int.TryParse(parameters[0], out count) || count < 1)
                                throw new FormatException($"Could not parse \"{parameters[0]}\" as an integer or value is less than one.");

                            result.datapoints.AddRange(source.datapoints.Take(count));
                            break;
                        case SeriesFunction.Bottom:
                            if (!int.TryParse(parameters[0], out count) || count < 1)
                                throw new FormatException($"Could not parse \"{parameters[0]}\" as an integer or value is less than one.");

                            result.datapoints.AddRange(source.datapoints.Reverse<double[]>().Take(count));
                            break;
                        case SeriesFunction.Random:
                            if (!int.TryParse(parameters[0], out count) || count < 1)
                                throw new FormatException($"Could not parse \"{parameters[0]}\" as an integer or value is less than one.");

                            if (count > source.datapoints.Count)
                                count = source.datapoints.Count;

                            indexes = new HashSet<int>();

                            while (indexes.Count < count)
                            {
                                index = Random.Int32Between(0, source.datapoints.Count - 1);

                                if (!indexes.Contains(index))
                                {
                                    indexes.Add(index);
                                    result.datapoints.Add(source.datapoints[index]);
                                }
                            }
                            break;
                        case SeriesFunction.First:
                            result.datapoints.Add(source.datapoints.First());
                            break;
                        case SeriesFunction.Last:
                            result.datapoints.Add(source.datapoints.Last());
                            break;
                        case SeriesFunction.Percentile:
                            if (!double.TryParse(parameters[0], out percent) || percent <= 0.0D || percent >= 100.0D)
                                throw new FormatException($"Could not parse \"{parameters[0]}\" as a floating-point number or value is outside range of 0 to 100.");

                            // TODO: Do percentile function...

                            break;
                        case SeriesFunction.Difference:
                            for (int i = 1; i < source.datapoints.Count; i++)
                                result.datapoints.Add(new[] { source.datapoints[i][TimeSeriesValues.Value] - source.datapoints[i - 1][TimeSeriesValues.Value], source.datapoints[i][TimeSeriesValues.Time] });

                            break;
                        case SeriesFunction.TimeDifference:
                            for (int i = 1; i < source.datapoints.Count; i++)
                                result.datapoints.Add(new[] { source.datapoints[i][TimeSeriesValues.Time] - source.datapoints[i - 1][TimeSeriesValues.Time], source.datapoints[i][TimeSeriesValues.Time] });

                            break;
                        case SeriesFunction.Derivative:
                            for (int i = 1; i < source.datapoints.Count; i++)
                                result.datapoints.Add(new[] { (source.datapoints[i][TimeSeriesValues.Value] - source.datapoints[i - 1][TimeSeriesValues.Value]) / source.datapoints[i][TimeSeriesValues.Time] - source.datapoints[i - 1][TimeSeriesValues.Time], source.datapoints[i][TimeSeriesValues.Time] });

                            break;
                        case SeriesFunction.TimeInt:
                            double integratedValue = 0.0D;

                            for (int i = 1; i < source.datapoints.Count; i++)
                                integratedValue += source.datapoints[i][TimeSeriesValues.Value] * (source.datapoints[i][TimeSeriesValues.Time] - source.datapoints[i - 1][TimeSeriesValues.Time]);

                            result.datapoints.Add(new[] { integratedValue, lastTime });
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
        /// Search data source for a target.
        /// </summary>
        /// <param name="request">Search target.</param>
        public Task<string[]> Search(Target request)
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
        private static readonly Regex s_seriesFunctions;
        private static readonly Regex s_averageExpression;
        private static readonly Regex s_minimumExpression;
        private static readonly Regex s_maximumExpression;
        private static readonly Regex s_totalExpression;
        private static readonly Regex s_rangeExpression;
        private static readonly Regex s_countExpression;
        private static readonly Regex s_stdDevExpression;
        private static readonly Regex s_stdDevSampExpression;
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
        private static readonly Regex s_timeIntExpression;
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
            s_stdDevExpression = new Regex(string.Format(GetExpression, "(StandardDeviation|StdDev)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_stdDevSampExpression = new Regex(string.Format(GetExpression, "(StandardDeviationSample|StdDevSamp)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_medianExpression = new Regex(string.Format(GetExpression, "(Median|Med|Mid)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_modeExpression = new Regex(string.Format(GetExpression, "Mode"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_topExpression = new Regex(string.Format(GetExpression, "Top"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_bottomExpression = new Regex(string.Format(GetExpression, "(Bottom|Bot)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_randomExpression = new Regex(string.Format(GetExpression, "(Random|Sample)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_firstExpression = new Regex(string.Format(GetExpression, "First"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_lastExpression = new Regex(string.Format(GetExpression, "Last"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_percentileExpression = new Regex(string.Format(GetExpression, "(Percentile|Pctl)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_differenceExpression = new Regex(string.Format(GetExpression, "(Difference|Diff)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_timeDifferenceExpression = new Regex(string.Format(GetExpression, "(TimeDifference|TimeDiff|Elapsed)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_derivativeExpression = new Regex(string.Format(GetExpression, "(Derivative|Der)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_timeIntExpression = new Regex(string.Format(GetExpression, "(TimeIntegration|TimeInt)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);

            s_parameterCounts = new Dictionary<SeriesFunction, int>();

            s_parameterCounts[SeriesFunction.Average] = 0;
            s_parameterCounts[SeriesFunction.Minimum] = 0;
            s_parameterCounts[SeriesFunction.Maximum] = 0;
            s_parameterCounts[SeriesFunction.Total] = 0;
            s_parameterCounts[SeriesFunction.Range] = 0;
            s_parameterCounts[SeriesFunction.Count] = 0;
            s_parameterCounts[SeriesFunction.StdDev] = 0;
            s_parameterCounts[SeriesFunction.StDevSamp] = 0;
            s_parameterCounts[SeriesFunction.Median] = 0;
            s_parameterCounts[SeriesFunction.Mode] = 0;
            s_parameterCounts[SeriesFunction.Top] = 1;
            s_parameterCounts[SeriesFunction.Bottom] = 1;
            s_parameterCounts[SeriesFunction.Random] = 1;
            s_parameterCounts[SeriesFunction.First] = 0;
            s_parameterCounts[SeriesFunction.Last] = 0;
            s_parameterCounts[SeriesFunction.Percentile] = 1;
            s_parameterCounts[SeriesFunction.Difference] = 0;
            s_parameterCounts[SeriesFunction.TimeDifference] = 0;
            s_parameterCounts[SeriesFunction.Derivative] = 0;
            s_parameterCounts[SeriesFunction.TimeInt] = 0;
        }

        #endregion
    }
}
