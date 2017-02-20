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
using GSF.Units;
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
        /// Execution: Immediate enumeration.
        /// </remarks>
        Average,
        /// <summary>
        /// Returns a single value that is the minimum of the values in the source series.
        /// </summary>
        /// <remarks>
        /// Example: <c>Minimum(FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
        /// Variants: Minimum, Min
        /// Execution: Immediate enumeration.
        /// </remarks>
        Minimum,
        /// <summary>
        /// Returns a single value that is the maximum of the values in the source series.
        /// </summary>
        /// <remarks>
        /// Example: <c>Maximum(FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
        /// Variants: Maximum, Max
        /// Execution: Immediate enumeration.
        /// </remarks>
        Maximum,
        /// <summary>
        /// Returns a single value that represents the sum of the values in the source series.
        /// </summary>
        /// <remarks>
        /// Example: <c>Total(FILTER ActiveMeasurements WHERE SignalType='IPHM')</c><br/>
        /// Variants: Total, Sum
        /// Execution: Immediate enumeration.
        /// </remarks>
        Total,
        /// <summary>
        /// Returns a single value that represents the range, i.e., maximum - minimum, of the values in the source series.
        /// </summary>
        /// <remarks>
        /// Example: <c>Range(FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
        /// Variants: Range
        /// Execution: Immediate enumeration.
        /// </remarks>
        Range,
        /// <summary>
        /// Returns a single value that is the count of the values in the source series.
        /// </summary>
        /// <remarks>
        /// Example: <c>Count(PPA:1; PPA:2; PPA:3)</c><br/>
        /// Variants: Count
        /// Execution: Immediate enumeration.
        /// </remarks>
        Count,
        /// <summary>
        /// Returns a series of values that represent the unique set of values in the source series.
        /// </summary>
        /// <remarks>
        /// Example: <c>Distinct(FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
        /// Variants: Distinct, Unique
        /// Execution: Deferred enumeration.
        /// </remarks>
        Distinct,
        /// <summary>
        /// Returns a series of values that represent the absolute value each of the values in the source series.
        /// </summary>
        /// <remarks>
        /// Example: <c>AbsoluteValue(FILTER ActiveMeasurements WHERE SignalType='CALC')</c><br/>
        /// Variants: AbsoluteValue, Abs
        /// Execution: Deferred enumeration.
        /// </remarks>
        AbsoluteValue,
        /// <summary>
        /// Returns a series of values that represent each of the values in the source series added with N.
        /// N is a floating point value representing an additive offset to be applied to each value the source series.
        /// </summary>
        /// <remarks>
        /// Example: <c>Add(-1.5, FILTER ActiveMeasurements WHERE SignalType='CALC')</c><br/>
        /// Variants: Add
        /// Execution: Deferred enumeration.
        /// </remarks>
        Add,
        /// <summary>
        /// Returns a series of values that represent each of the values in the source series multiplied by N.
        /// N is a floating point value representing a multiplicative factor to be applied to each value the source series.
        /// </summary>
        /// <remarks>
        /// Example: <c>Multiply(0.5, FILTER ActiveMeasurements WHERE SignalType='CALC')</c><br/>
        /// Variants: Multiply
        /// Execution: Deferred enumeration.
        /// </remarks>
        Multiply,
        /// <summary>
        /// Returns a series of values that represent the rounded value, with N fractional digits, of each of the values in the source series.
        /// N, optional, is a positive integer value representing the number of decimal places in the return value - defaults to 0.
        /// </summary>
        /// <remarks>
        /// Example: <c>Round(3, FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
        /// Variants: Round
        /// Execution: Deferred enumeration.
        /// </remarks>
        Round,
        /// <summary>
        /// Returns a series of values that represent the smallest integral value that is less than or equal to each of the values in the source series.
        /// </summary>
        /// <remarks>
        /// Example: <c>Floor(FILTER ActiveMeasurements WHERE SignalType='IPHM')</c><br/>
        /// Variants: Floor
        /// Execution: Deferred enumeration.
        /// </remarks>
        Floor,
        /// <summary>
        /// Returns a series of values that represent the smallest integral value that is greater than or equal to each of the values in the source series.
        /// </summary>
        /// <remarks>
        /// Example: <c>Ceiling(FILTER ActiveMeasurements WHERE SignalType='IPHM')</c><br/>
        /// Variants: Ceiling, Ceil
        /// Execution: Deferred enumeration.
        /// </remarks>
        Ceiling,
        /// <summary>
        /// Returns a series of values that represent the integral part of each of the values in the source series.
        /// </summary>
        /// <remarks>
        /// Example: <c>Truncate(FILTER ActiveMeasurements WHERE SignalType='IPHM')</c><br/>
        /// Variants: Truncate, Trunc
        /// Execution: Deferred enumeration.
        /// </remarks>
        Truncate,
        /// <summary>
        /// Returns a single value that represents the standard deviation of the values in the source series.
        /// </summary>
        /// <remarks>
        /// Example: <c>StandardDeviation(FILTER ActiveMeasurements WHERE SignalType='VPHM')</c><br/>
        /// Variants: StandardDeviation, StdDev
        /// Execution: Immediate in-memory array load.
        /// </remarks>
        StandardDeviation,
        /// <summary>
        /// Returns a single value that represents the standard deviation, using sample calculation, of the values in the source series.
        /// </summary>
        /// <remarks>
        /// Example: <c>StandardDeviationSample(FILTER ActiveMeasurements WHERE SignalType='VPHM')</c><br/>
        /// Variants: StandardDeviationSample, StdDevSamp
        /// Execution: Immediate in-memory array load.
        /// </remarks>
        StandardDeviationSample,
        /// <summary>
        /// Returns a single value that represents the median of the values in the source series.
        /// </summary>
        /// <remarks>
        /// Example: <c>Median(FILTER ActiveMeasurements WHERE SignalType='ALOG')</c><br/>
        /// Variants: Median, Med, Mid
        /// Execution: Immediate in-memory array load.
        /// </remarks>
        Median,
        /// <summary>
        /// Returns a single value that represents the mode of the values in the source series.
        /// </summary>
        /// <remarks>
        /// Example: <c>Mode(FILTER TOP 5 ActiveMeasurements WHERE SignalType='DIGI')</c><br/>
        /// Variants: Mode
        /// Execution: Immediate in-memory array load.
        /// </remarks>
        Mode,
        /// <summary>
        /// Returns a series of N, or N% of total, values that are the largest in the source series.
        /// N is either a positive integer value, representing a total, that is greater than zero - or - a floating point value,
        /// representing a percentage, that must range from greater than 0 to less than or equal to 100.
        /// Second parameter, optional, is a boolean flag representing if time in dataset should be normalized - defaults to true.
        /// </summary>
        /// <remarks>
        /// Example: <c>Top(50%, FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
        /// Variants: Top
        /// Execution: Immediate in-memory array load.
        /// </remarks>
        Top,
        /// <summary>
        /// Returns a series of N, or N% of total, values that are the smallest in the source series.
        /// N is either a positive integer value, representing a total, that is greater than zero - or - a floating point value,
        /// representing a percentage, that must range from greater than 0 to less than or equal to 100.
        /// Second parameter, optional, is a boolean flag representing if time in dataset should be normalized - defaults to true.
        /// </summary>
        /// <remarks>
        /// Example: <c>Bottom(100, false, FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
        /// Variants: Bottom, Bot
        /// Execution: Immediate in-memory array load.
        /// </remarks>
        Bottom,
        /// <summary>
        /// Returns a series of N, or N% of total, values that are a random sample of the values in the source series.
        /// N is either a positive integer value, representing a total, that is greater than zero - or - a floating point value,
        /// representing a percentage, that must range from greater than 0 to less than or equal to 100.
        /// Second parameter, optional, is a boolean flag representing if time in dataset should be normalized - defaults to true.
        /// </summary>
        /// <remarks>
        /// Example: <c>Random(25%, FILTER ActiveMeasurements WHERE SignalType='VPHM')</c><br/>
        /// Variants: Random, Rand, Sample
        /// Execution: Immediate in-memory array load.
        /// </remarks>
        Random,
        /// <summary>
        /// Returns a series of N, or N% of total, values from the start of the source series.
        /// N, optional, is either a positive integer value, representing a total, that is greater than zero - or - a floating point value,
        /// representing a percentage, that must range from greater than 0 to less than or equal to 100 - defaults to 1.
        /// </summary>
        /// <remarks>
        /// Example: <c>First(5%, FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
        /// Variants: First
        /// Execution: Immediate in-memory array load.
        /// </remarks>
        First,
        /// <summary>
        /// Returns a series of N, or N% of total, values from the end of the source series.
        /// N, optional, is either a positive integer value, representing a total, that is greater than zero - or - a floating point value,
        /// representing a percentage, that must range from greater than 0 to less than or equal to 100 - defaults to 1.
        /// </summary>
        /// <remarks>
        /// Example: <c>Last(150, FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
        /// Variants: Last
        /// Execution: Immediate in-memory array load.
        /// </remarks>
        Last,
        /// <summary>
        /// Returns a single value that represents the Nth order percentile for the sorted values in the source series.
        /// N is a floating point value, representing a percentage, that must range from 0 to 100.
        /// </summary>
        /// <remarks>
        /// Example: <c>Percentile(10%, FILTER ActiveMeasurements WHERE SignalType='VPHM')</c><br/>
        /// Variants: Percentile, Pctl
        /// Execution: Immediate in-memory array load.
        /// </remarks>
        Percentile,
        /// <summary>
        /// Returns a series of values that represent the difference between consecutive values in the source series.
        /// </summary>
        /// <remarks>
        /// Example: <c>Difference(FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
        /// Variants: Difference, Diff
        /// Execution: Deferred enumeration.
        /// </remarks>
        Difference,
        /// <summary>
        /// Returns a series of values that represent the time difference, in seconds, between consecutive values in the source series.
        /// </summary>
        /// <remarks>
        /// Example: <c>TimeDifference(FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
        /// Variants: TimeDifference, TimeDiff
        /// Execution: Deferred enumeration.
        /// </remarks>
        TimeDifference,
        /// <summary>
        /// Returns a series of values that represent the rate of change, per second, for the difference between consecutive values in the source series.
        /// </summary>
        /// <remarks>
        /// Example: <c>Derivative(FILTER ActiveMeasurements WHERE SignalType='FREQ')</c><br/>
        /// Variants: Derivative, Der
        /// Execution: Deferred enumeration.
        /// </remarks>
        Derivative,
        /// <summary>
        /// Returns a single value that represents the time-based integration, i.e., the sum of V(n) * (T(n) - T(n-1)), of the values in the source series.
        /// </summary>
        /// <remarks>
        /// Example: <c>TimeIntegration(FILTER ActiveMeasurements WHERE SignalType='CALC' AND PointTag LIKE '%-MW:%')</c><br/>
        /// Variants: TimeIntegration, TimeInt
        /// Execution: Immediate enumeration.
        /// </remarks>
        TimeIntegration,
        /// <summary>
        /// Returns a series of values that represent a decimated set of the values in the source series based on the specified interval N, in seconds.
        /// N is a floating-point value that must be greater than or equal to zero that represents the desired time interval, in seconds, for the returned data.
        /// Setting N value to zero will request non-decimated, full resolution data from the data source. A zero value will always produce the most accurate
        /// aggregation calculation results but will increase query burden for large time ranges.
        /// </summary>
        /// <remarks>
        /// Example: <c>Sum(Interval(0, FILTER ActiveMeasurements WHERE SignalType LIKE '%PHM'))</c><br/>
        /// Variants: Interval, Int
        /// Execution: Deferred enumeration.
        /// </remarks>
        Interval,
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
                int maxDataPoints = (int)(request.maxDataPoints * 1.05D);

                DataSourceValueGroup[] valueGroups = QueryTargets(request.targets.Select(target => target.target.Trim()), startTime, stopTime, true, cancellationToken).ToArray();

                // Establish result series sequentially so that order remains consistent between calls
                List<TimeSeriesValues> result = valueGroups.Select(valueGroup => new TimeSeriesValues { target = valueGroup.Target }).ToList();

                Parallel.ForEach(result, new ParallelOptions { CancellationToken = cancellationToken }, series =>
                {
                    series.datapoints = valueGroups.First(group => group.Target.Equals(series.target)).Source.Select(dataValue => new[] { dataValue.Value, dataValue.Time }).ToList();
                });

                // Make a final pass through data to decimate returned point volume (for graphing purposes), if needed
                foreach (TimeSeriesValues series in result)
                {
                    if (series.datapoints.Count > maxDataPoints)
                    {
                        double indexFactor = series.datapoints.Count / (double)request.maxDataPoints;
                        series.datapoints = Enumerable.Range(0, request.maxDataPoints).Select(index => series.datapoints[(int)(index * indexFactor)]).ToList();
                    }
                }

                return result;
            },
            cancellationToken);
        }

        /// <summary>
        /// Starts a query that will read data source values, given a point ID and target, over a time range.
        /// </summary>
        /// <param name="startTime">Start-time for query.</param>
        /// <param name="stopTime">Stop-time for query.</param>
        /// <param name="decimate">Flag that determines if data should be decimated over provided time range.</param>
        /// <param name="targetMap">Set of IDs with associated targets to query.</param>
        /// <returns>Queried data source data in terms of value and time.</returns>
        protected abstract IEnumerable<DataSourceValue> QueryDataSourceValues(DateTime startTime, DateTime stopTime, bool decimate, Dictionary<ulong, string> targetMap);

        private IEnumerable<DataSourceValueGroup> QueryTargets(IEnumerable<string> targets, DateTime startTime, DateTime stopTime, bool decimate, CancellationToken cancellationToken)
        {
            // A single target might look like the following:
            // PPA:15; STAT:20; SETSUM(COUNT(PPA:8; PPA:9; PPA:10)); FILTER ActiveMeasurements WHERE SignalType IN ('IPHA', 'VPHA'); RANGE(PPA:99; SUM(FILTER ActiveMeasurements WHERE SignalType = 'FREQ'; STAT:12))

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
                // Execute series functions
                foreach (Tuple<SeriesFunction, string, bool> parsedFunction in seriesFunctions.Select(ParseSeriesFunction))
                    foreach (DataSourceValueGroup valueGroup in ExecuteSeriesFunction(parsedFunction, startTime, stopTime, decimate, cancellationToken))
                        yield return valueGroup;

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

                long readCount = 0;

                // Query underlying data source for each target
                List<DataSourceValue> dataValues = QueryDataSourceValues(startTime, stopTime, decimate, targetMap)
                    .TakeWhile(dataValue => readCount++ % 10000 != 0 || !cancellationToken.IsCancellationRequested).ToList();

                foreach (KeyValuePair<ulong, string> target in targetMap)
                    yield return new DataSourceValueGroup
                    {
                        Target = target.Value,
                        Source = dataValues.Where(dataValue => dataValue.Target.Equals(target.Value))
                    };
            }
        }

        private IEnumerable<DataSourceValueGroup> ExecuteSeriesFunction(Tuple<SeriesFunction, string, bool> parsedFunction, DateTime startTime, DateTime stopTime, bool decimate, CancellationToken cancellationToken)
        {
            SeriesFunction seriesFunction = parsedFunction.Item1;
            string expression = parsedFunction.Item2;
            bool setOperation = parsedFunction.Item3;

            // Parse out function parameters and target expression
            Tuple<string[], string> expressionParameters = TargetCache<Tuple<string[], string>>.GetOrAdd(expression, () =>
            {
                List<string> parsedParameters = new List<string>();

                // Extract any required function parameters
                int requiredParameters = s_requiredParameters[seriesFunction]; // Safe: no lock needed since content doesn't change

                if (requiredParameters > 0)
                {
                    int index = 0;

                    for (int i = 0; i < requiredParameters && index > -1; i++)
                        index = expression.IndexOf(',', index + 1);

                    if (index > -1)
                        parsedParameters.AddRange(expression.Substring(0, index).Split(','));

                    if (parsedParameters.Count == requiredParameters)
                        expression = expression.Substring(index + 1).Trim();
                    else
                        throw new FormatException($"Expected {requiredParameters + 1} parameters, received {parsedParameters.Count + 1} in: {seriesFunction}({expression})");
                }

                // Extract any provided optional function parameters
                int optionalParameters = s_optionalParameters[seriesFunction]; // Safe: no lock needed since content doesn't change

                Func<string, bool> hasSubExpression = target => target.StartsWith("FILTER", StringComparison.OrdinalIgnoreCase) || target.Contains("(");

                if (optionalParameters > 0)
                {
                    int index = expression.IndexOf(',');
                    int lastIndex;

                    if (index > -1 && !hasSubExpression(expression.Substring(0, index)))
                    {
                        lastIndex = index;

                        for (int i = 1; i < optionalParameters && index > -1; i++)
                        {
                            index = expression.IndexOf(',', index + 1);

                            if (index > -1 && hasSubExpression(expression.Substring(lastIndex + 1, index - lastIndex - 1).Trim()))
                            {
                                index = lastIndex;
                                break;
                            }

                            lastIndex = index;
                        }

                        if (index > -1)
                        {
                            parsedParameters.AddRange(expression.Substring(0, index).Split(','));
                            expression = expression.Substring(index + 1).Trim();
                        }
                    }
                }

                return new Tuple<string[], string>(parsedParameters.ToArray(), expression);
            });

            string[] parameters = expressionParameters.Item1;
            string targetExpression = expressionParameters.Item2;   // Final function parameter is always target expression

            // When accurate calculation results are requested, query data source at full resolution
            if (seriesFunction == SeriesFunction.Interval && ParseFloat(parameters[0]) == 0.0D)
                decimate = false;

            // Query function expression to get series data
            IEnumerable<DataSourceValueGroup> dataset = QueryTargets(new[] { targetExpression }, startTime, stopTime, decimate, cancellationToken);

            if (setOperation)
            {
                // Flatten all series into a single enumerable for set operations
                DataSourceValueGroup result = new DataSourceValueGroup
                {
                    Target = $"Set{seriesFunction}({string.Join(", ", parameters)}{(parameters.Length > 0 ? ", " : "")}{targetExpression})",
                    Source = ExecuteSeriesFunctionOverSource(dataset.AsParallel().SelectMany(source => source.Source), seriesFunction, parameters)
                };

                // Handle edge-case set operations - for these functions there is data in the target series as well
                if (seriesFunction == SeriesFunction.Minimum || seriesFunction == SeriesFunction.Maximum || seriesFunction == SeriesFunction.Median || seriesFunction == SeriesFunction.Mode)
                {
                    DataSourceValue dataValue = result.Source.First();
                    result.Target = $"Set{seriesFunction} = {dataValue.Target}";
                }

                yield return result;
            }
            else
            {
                foreach (DataSourceValueGroup dataValues in dataset)
                    yield return new DataSourceValueGroup
                    {
                        Target = $"{seriesFunction}({string.Join(", ", parameters)}{(parameters.Length > 0 ? ", " : "")}{dataValues.Target})",
                        Source = ExecuteSeriesFunctionOverSource(dataValues.Source, seriesFunction, parameters)
                    };
            }
        }

        // Design philosophy: whenever possible this function should delay source enumeration since source data sets could be very large.
        private static IEnumerable<DataSourceValue> ExecuteSeriesFunctionOverSource(IEnumerable<DataSourceValue> source, SeriesFunction seriesFunction, string[] parameters)
        {
            DataSourceValue[] values;
            DataSourceValue result;
            double lastValue = double.NaN;
            double lastTime = 0.0D;
            string lastTarget = null;

            IEnumerable<double> trackedValues = source.Select(dataValue =>
            {
                lastTime = dataValue.Time;
                lastTarget = dataValue.Target;
                return dataValue.Value;
            });

            double baseTime, timeStep, value;
            bool normalizeTime;
            int count;

            switch (seriesFunction)
            {
                case SeriesFunction.Minimum:
                    DataSourceValue minValue = new DataSourceValue { Value = double.MaxValue };

                    foreach (DataSourceValue dataValue in source)
                    {
                        if (dataValue.Value <= minValue.Value)
                            minValue = dataValue;
                    }

                    if (minValue.Time > 0.0D)
                        yield return minValue;

                    break;
                case SeriesFunction.Maximum:
                    DataSourceValue maxValue = new DataSourceValue { Value = double.MinValue };

                    foreach (DataSourceValue dataValue in source)
                    {
                        if (dataValue.Value >= maxValue.Value)
                            maxValue = dataValue;
                    }

                    if (maxValue.Time > 0.0D)
                        yield return maxValue;

                    break;
                case SeriesFunction.Average:
                    result.Value = trackedValues.Average();
                    result.Time = lastTime;
                    result.Target = lastTarget;
                    yield return result;
                    break;
                case SeriesFunction.Total:
                    result.Value = trackedValues.Sum();
                    result.Time = lastTime;
                    result.Target = lastTarget;
                    yield return result;
                    break;
                case SeriesFunction.Range:
                    DataSourceValue rangeMin = new DataSourceValue { Value = double.MaxValue };
                    DataSourceValue rangeMax = new DataSourceValue { Value = double.MinValue };

                    foreach (DataSourceValue dataValue in source)
                    {
                        if (dataValue.Value <= rangeMin.Value)
                            rangeMin = dataValue;

                        if (dataValue.Value >= rangeMax.Value)
                            rangeMax = dataValue;
                    }

                    if (rangeMin.Time > 0.0D && rangeMax.Time > 0.0D)
                    {
                        result = rangeMax;
                        result.Value = rangeMax.Value - rangeMin.Value;
                        yield return result;
                    }
                    break;
                case SeriesFunction.Count:
                    result.Value = trackedValues.Count();
                    result.Time = lastTime;
                    result.Target = lastTarget;
                    yield return result;
                    break;
                case SeriesFunction.Distinct:
                    foreach (DataSourceValue dataValue in source.DistinctBy(dataValue => dataValue.Value))
                        yield return dataValue;

                    break;
                case SeriesFunction.AbsoluteValue:
                    foreach (DataSourceValue dataValue in source.Select(dataValue => new DataSourceValue { Value = Math.Abs(dataValue.Value), Time = dataValue.Time, Target = dataValue.Target }))
                        yield return dataValue;

                    break;
                case SeriesFunction.Add:
                    value = ParseFloat(parameters[0], false);

                    foreach (DataSourceValue dataValue in source.Select(dataValue => new DataSourceValue { Value = dataValue.Value + value, Time = dataValue.Time, Target = dataValue.Target }))
                        yield return dataValue;

                    break;
                case SeriesFunction.Multiply:
                    value = ParseFloat(parameters[0], false);

                    foreach (DataSourceValue dataValue in source.Select(dataValue => new DataSourceValue { Value = dataValue.Value * value, Time = dataValue.Time, Target = dataValue.Target }))
                        yield return dataValue;

                    break;
                case SeriesFunction.Round:
                    count = parameters.Length == 0 ? 0 : ParseInt(parameters[0]);

                    foreach (DataSourceValue dataValue in source.Select(dataValue => new DataSourceValue { Value = Math.Round(dataValue.Value, count), Time = dataValue.Time, Target = dataValue.Target }))
                        yield return dataValue;

                    break;
                case SeriesFunction.Floor:
                    foreach (DataSourceValue dataValue in source.Select(dataValue => new DataSourceValue { Value = Math.Floor(dataValue.Value), Time = dataValue.Time, Target = dataValue.Target }))
                        yield return dataValue;

                    break;
                case SeriesFunction.Ceiling:
                    foreach (DataSourceValue dataValue in source.Select(dataValue => new DataSourceValue { Value = Math.Ceiling(dataValue.Value), Time = dataValue.Time, Target = dataValue.Target }))
                        yield return dataValue;

                    break;
                case SeriesFunction.Truncate:
                    foreach (DataSourceValue dataValue in source.Select(dataValue => new DataSourceValue { Value = Math.Truncate(dataValue.Value), Time = dataValue.Time, Target = dataValue.Target }))
                        yield return dataValue;

                    break;
                case SeriesFunction.StandardDeviation:
                    result.Value = trackedValues.StandardDeviation();
                    result.Time = lastTime;
                    result.Target = lastTarget;
                    yield return result;
                    break;
                case SeriesFunction.StandardDeviationSample:
                    result.Value = trackedValues.StandardDeviation(true);
                    result.Time = lastTime;
                    result.Target = lastTarget;
                    yield return result;
                    break;
                case SeriesFunction.Median:
                    values = source.Median();

                    if (values.Length == 0)
                        yield break;

                    result = values.Last();

                    if (values.Length > 1)
                        result.Value = values.Select(dataValue => dataValue.Value).Average();

                    yield return result;
                    break;
                case SeriesFunction.Mode:
                    values = source.ToArray();
                    yield return values.MajorityBy(values.Last(), dataValue => dataValue.Value, false);
                    break;
                case SeriesFunction.Top:
                    values = source.ToArray();

                    if (values.Length == 0)
                        yield break;

                    count = ParseCount(parameters[0], values.Length);

                    if (count > values.Length)
                        count = values.Length;

                    normalizeTime = parameters.Length == 1 || parameters[1].Trim().ParseBoolean();
                    baseTime = values[0].Time;
                    timeStep = (values[values.Length - 1].Time - baseTime) / (count - 1).NotZero(1);
                    Array.Sort(values, (a, b) => a.Value < b.Value ? -1 : (a.Value > b.Value ? 1 : 0));

                    foreach (DataSourceValue dataValue in values.Take(count).Select((dataValue, i) => new DataSourceValue { Value = dataValue.Value, Time = normalizeTime ? baseTime + i * timeStep : dataValue.Time }))
                        yield return dataValue;

                    break;
                case SeriesFunction.Bottom:
                    values = source.ToArray();

                    if (values.Length == 0)
                        yield break;

                    count = ParseCount(parameters[0], values.Length);

                    if (count > values.Length)
                        count = values.Length;

                    normalizeTime = parameters.Length == 1 || parameters[1].Trim().ParseBoolean();
                    baseTime = values[0].Time;
                    timeStep = (values[values.Length - 1].Time - baseTime) / (count - 1).NotZero(1);
                    Array.Sort(values, (a, b) => a.Value > b.Value ? -1 : (a.Value < b.Value ? 1 : 0));

                    foreach (DataSourceValue dataValue in values.Take(count).Select((dataValue, i) => new DataSourceValue { Value = dataValue.Value, Time = normalizeTime ? baseTime + i * timeStep : dataValue.Time }))
                        yield return dataValue;

                    break;
                case SeriesFunction.Random:
                    values = source.ToArray();

                    if (values.Length == 0)
                        yield break;

                    count = ParseCount(parameters[0], values.Length);

                    if (count > values.Length)
                        count = values.Length;

                    normalizeTime = parameters.Length == 1 || parameters[1].Trim().ParseBoolean();
                    baseTime = values[0].Time;
                    timeStep = (values[values.Length - 1].Time - baseTime) / (count - 1).NotZero(1);
                    List<int> indexes = new List<int>(Enumerable.Range(0, values.Length));
                    indexes.Scramble();

                    foreach (DataSourceValue dataValue in indexes.Take(count).Select((index, i) => new DataSourceValue { Value = values[index].Value, Time = normalizeTime ? baseTime + i * timeStep : values[index].Time }))
                        yield return dataValue;

                    break;
                case SeriesFunction.First:
                    values = source.ToArray();

                    if (values.Length == 0)
                        yield break;

                    count = parameters.Length == 0 ? 1 : ParseCount(parameters[0], values.Length);

                    if (count > values.Length)
                        count = values.Length;

                    for (int i = 0; i < count; i++)
                        yield return values[i];

                    break;
                case SeriesFunction.Last:
                    values = source.ToArray();

                    if (values.Length == 0)
                        yield break;

                    count = parameters.Length == 0 ? 1 : ParseCount(parameters[0], values.Length);

                    if (count > values.Length)
                        count = values.Length;

                    for (int i = 0; i < count; i++)
                        yield return values[values.Length - 1 - i];

                    break;
                case SeriesFunction.Percentile:
                    double percent = ParsePercentage(parameters[0]);
                    values = source.ToArray();

                    if (values.Length == 0)
                        yield break;

                    Array.Sort(values, (a, b) => a.Value < b.Value ? -1 : (a.Value > b.Value ? 1 : 0));
                    count = values.Length;

                    if (percent == 0.0D)
                    {
                        yield return values.First();
                    }
                    else if (percent == 100.0D)
                    {
                        yield return values.Last();
                    }
                    else
                    {
                        double n = (count - 1) * (percent / 100.0D) + 1.0D;
                        int k = (int)n;
                        DataSourceValue kData = values[k];
                        double d = n - k;
                        double k0 = values[k - 1].Value;
                        double k1 = kData.Value;

                        result.Value = k0 + d * (k1 - k0);
                        result.Time = kData.Time;
                        result.Target = kData.Target;
                        yield return result;
                    }
                    break;
                case SeriesFunction.Difference:
                    foreach (DataSourceValue dataValue in source)
                    {
                        if (lastTime > 0.0D)
                            yield return new DataSourceValue { Value = dataValue.Value - lastValue, Time = dataValue.Time};

                        lastValue = dataValue.Value;
                        lastTime = dataValue.Time;
                    }
                    break;
                case SeriesFunction.TimeDifference:
                    foreach (DataSourceValue dataValue in source)
                    {
                        if (lastTime > 0.0D)
                            yield return new DataSourceValue { Value = dataValue.Time - lastTime, Time = dataValue.Time};

                        lastTime = dataValue.Time;
                    }
                    break;
                case SeriesFunction.Derivative:
                    foreach (DataSourceValue dataValue in source)
                    {
                        if (lastTime > 0.0D)
                            yield return new DataSourceValue { Value = (dataValue.Value - lastValue) / (dataValue.Time - lastTime), Time = dataValue.Time};

                        lastValue = dataValue.Value;
                        lastTime = dataValue.Time;
                    }
                    break;
                case SeriesFunction.TimeIntegration:
                    result.Value = 0.0D;

                    foreach (DataSourceValue dataValue in source)
                    {
                        if (lastTime > 0.0D)
                            result.Value += dataValue.Value * (dataValue.Time - lastTime);

                        lastTime = dataValue.Time;
                        lastTarget = dataValue.Target;
                    }

                    if (lastTime > 0.0D)
                    {
                        result.Time = lastTime;
                        result.Target = lastTarget;
                        yield return result;
                    }
                    break;
                case SeriesFunction.Interval:
                    value = ParseFloat(parameters[0]) / SI.Milli;

                    foreach (DataSourceValue dataValue in source)
                    {
                        if (lastTime > 0.0D)
                        {
                            if (dataValue.Time - lastTime > value)
                            {
                                lastTime = dataValue.Time;
                                yield return dataValue;
                            }
                        }
                        else
                        {
                            lastTime = dataValue.Time;
                            yield return dataValue;
                        }
                    }
                    break;
            }
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
        private static readonly Regex s_addExpression;
        private static readonly Regex s_multiplyExpression;
        private static readonly Regex s_roundExpression;
        private static readonly Regex s_floorExpression;
        private static readonly Regex s_ceilingExpression;
        private static readonly Regex s_truncateExpression;
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
        private static readonly Regex s_intervalExpression;
        private static readonly Dictionary<SeriesFunction, int> s_requiredParameters;
        private static readonly Dictionary<SeriesFunction, int> s_optionalParameters;

        // Static Constructor
        static GrafanaDataSourceBase()
        {
            const string GetExpression = @"^{0}\s*\(\s*(?<Expression>.+)\s*\)";

            // RegEx instance to find all series functions
            s_seriesFunctions = new Regex(@"(SET)?\w+\s*(?<!\s+IN\s+)\((([^\(\)]|(?<counter>\()|(?<-counter>\)))*(?(counter)(?!)))\)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            // RegEx instances to identify specific functions and extract internal expressions
            s_averageExpression = new Regex(string.Format(GetExpression, "(Average|Avg|Mean)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_minimumExpression = new Regex(string.Format(GetExpression, "(Minimum|Min)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_maximumExpression = new Regex(string.Format(GetExpression, "(Maximum|Max)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_totalExpression = new Regex(string.Format(GetExpression, "(Total|Sum)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_rangeExpression = new Regex(string.Format(GetExpression, "Range"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_countExpression = new Regex(string.Format(GetExpression, "Count"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_distinctExpression = new Regex(string.Format(GetExpression, "(Distinct|Unique)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_absoluteValueExpression = new Regex(string.Format(GetExpression, "(AbsoluteValue|Abs)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_addExpression = new Regex(string.Format(GetExpression, "Add"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_multiplyExpression = new Regex(string.Format(GetExpression, "Multiply"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_roundExpression = new Regex(string.Format(GetExpression, "Round"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_floorExpression = new Regex(string.Format(GetExpression, "Floor"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_ceilingExpression = new Regex(string.Format(GetExpression, "(Ceiling|Ceil)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_truncateExpression = new Regex(string.Format(GetExpression, "(Truncate|Trunc)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_standardDeviationExpression = new Regex(string.Format(GetExpression, "(StandardDeviation|StdDev)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_standardDeviationSampleExpression = new Regex(string.Format(GetExpression, "(StandardDeviationSample|StdDevSamp)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_medianExpression = new Regex(string.Format(GetExpression, "(Median|Med|Mid)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_modeExpression = new Regex(string.Format(GetExpression, "Mode"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_topExpression = new Regex(string.Format(GetExpression, "Top"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_bottomExpression = new Regex(string.Format(GetExpression, "(Bottom|Bot)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_randomExpression = new Regex(string.Format(GetExpression, "(Random|Rand|Sample)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_firstExpression = new Regex(string.Format(GetExpression, "First"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_lastExpression = new Regex(string.Format(GetExpression, "Last"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_percentileExpression = new Regex(string.Format(GetExpression, "(Percentile|Pctl)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_differenceExpression = new Regex(string.Format(GetExpression, "(Difference|Diff)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_timeDifferenceExpression = new Regex(string.Format(GetExpression, "(TimeDifference|TimeDiff|Elapsed)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_derivativeExpression = new Regex(string.Format(GetExpression, "(Derivative|Der)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_timeIntegrationExpression = new Regex(string.Format(GetExpression, "(TimeIntegration|TimeInt)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);
            s_intervalExpression = new Regex(string.Format(GetExpression, "(Interval|Int)"), RegexOptions.Compiled | RegexOptions.IgnoreCase);

            // Define required parameter counts for each function
            s_requiredParameters = new Dictionary<SeriesFunction, int>
            {
                [SeriesFunction.Average] = 0,
                [SeriesFunction.Minimum] = 0,
                [SeriesFunction.Maximum] = 0,
                [SeriesFunction.Total] = 0,
                [SeriesFunction.Range] = 0,
                [SeriesFunction.Count] = 0,
                [SeriesFunction.Distinct] = 0,
                [SeriesFunction.AbsoluteValue] = 0,
                [SeriesFunction.Add] = 1,
                [SeriesFunction.Multiply] = 1,
                [SeriesFunction.Round] = 0,
                [SeriesFunction.Floor] = 0,
                [SeriesFunction.Ceiling] = 0,
                [SeriesFunction.Truncate] = 0,
                [SeriesFunction.StandardDeviation] = 0,
                [SeriesFunction.StandardDeviationSample] = 0,
                [SeriesFunction.Median] = 0,
                [SeriesFunction.Mode] = 0,
                [SeriesFunction.Top] = 1,
                [SeriesFunction.Bottom] = 1,
                [SeriesFunction.Random] = 1,
                [SeriesFunction.First] = 0,
                [SeriesFunction.Last] = 0,
                [SeriesFunction.Percentile] = 1,
                [SeriesFunction.Difference] = 0,
                [SeriesFunction.TimeDifference] = 0,
                [SeriesFunction.Derivative] = 0,
                [SeriesFunction.TimeIntegration] = 0,
                [SeriesFunction.Interval] = 1
            };

            // Define optional parameter counts for each function
            s_optionalParameters = new Dictionary<SeriesFunction, int>
            {
                [SeriesFunction.Average] = 0,
                [SeriesFunction.Minimum] = 0,
                [SeriesFunction.Maximum] = 0,
                [SeriesFunction.Total] = 0,
                [SeriesFunction.Range] = 0,
                [SeriesFunction.Count] = 0,
                [SeriesFunction.Distinct] = 0,
                [SeriesFunction.AbsoluteValue] = 0,
                [SeriesFunction.Add] = 0,
                [SeriesFunction.Multiply] = 0,
                [SeriesFunction.Round] = 1,
                [SeriesFunction.Floor] = 0,
                [SeriesFunction.Ceiling] = 0,
                [SeriesFunction.Truncate] = 0,
                [SeriesFunction.StandardDeviation] = 0,
                [SeriesFunction.StandardDeviationSample] = 0,
                [SeriesFunction.Median] = 0,
                [SeriesFunction.Mode] = 0,
                [SeriesFunction.Top] = 1,
                [SeriesFunction.Bottom] = 1,
                [SeriesFunction.Random] = 1,
                [SeriesFunction.First] = 1,
                [SeriesFunction.Last] = 1,
                [SeriesFunction.Percentile] = 0,
                [SeriesFunction.Difference] = 0,
                [SeriesFunction.TimeDifference] = 0,
                [SeriesFunction.Derivative] = 0,
                [SeriesFunction.TimeIntegration] = 0,
                [SeriesFunction.Interval] = 0
            };
        }

        // Static Methods
        private static Tuple<SeriesFunction, string, bool> ParseSeriesFunction(Match matchedFunction)
        {
            Tuple<SeriesFunction, string, bool> result = TargetCache<Tuple<SeriesFunction, string, bool>>.GetOrAdd(matchedFunction.Value, () =>
            {
                bool setOperation = matchedFunction.Groups[1].Success;
                string expression = setOperation ? matchedFunction.Value.Substring(3) : matchedFunction.Value;
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

                // Look for add function
                lock (s_addExpression)
                    filterMatch = s_addExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, bool>(SeriesFunction.Add, filterMatch.Result("${Expression}").Trim(), setOperation);

                // Look for multiply function
                lock (s_multiplyExpression)
                    filterMatch = s_multiplyExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, bool>(SeriesFunction.Multiply, filterMatch.Result("${Expression}").Trim(), setOperation);

                // Look for round function
                lock (s_roundExpression)
                    filterMatch = s_roundExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, bool>(SeriesFunction.Round, filterMatch.Result("${Expression}").Trim(), setOperation);

                // Look for floor function
                lock (s_floorExpression)
                    filterMatch = s_floorExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, bool>(SeriesFunction.Floor, filterMatch.Result("${Expression}").Trim(), setOperation);

                // Look for ceiling function
                lock (s_ceilingExpression)
                    filterMatch = s_ceilingExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, bool>(SeriesFunction.Ceiling, filterMatch.Result("${Expression}").Trim(), setOperation);

                // Look for truncate function
                lock (s_truncateExpression)
                    filterMatch = s_truncateExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, bool>(SeriesFunction.Truncate, filterMatch.Result("${Expression}").Trim(), setOperation);

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

                // Look for interval function
                lock (s_intervalExpression)
                    filterMatch = s_intervalExpression.Match(expression);

                if (filterMatch.Success)
                    return new Tuple<SeriesFunction, string, bool>(SeriesFunction.Interval, filterMatch.Result("${Expression}").Trim(), setOperation);

                // Target is not a recognized function
                return new Tuple<SeriesFunction, string, bool>(SeriesFunction.None, expression, false);
            });

            if (result.Item1 == SeriesFunction.None)
                throw new InvalidOperationException($"Unrecognized series function '{matchedFunction.Value}'");

            return result;
        }

        private static int ParseInt(string parameter, bool includeZero = true)
        {
            int value;

            parameter = parameter.Trim();

            if (!int.TryParse(parameter, out value))
                throw new FormatException($"Could not parse '{parameter}' as an integer value.");

            if (includeZero)
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException($"Value '{parameter}' is less than zero.");
            }
            else
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException($"Value '{parameter}' is less than or equal to zero.");
            }

            return value;
        }

        private static double ParseFloat(string parameter, bool validateGTEZero = true)
        {
            double value;

            parameter = parameter.Trim();

            if (!double.TryParse(parameter, out value))
                throw new FormatException($"Could not parse '{parameter}' as a floating-point value.");

            if (validateGTEZero && value < 0.0D)
                throw new ArgumentOutOfRangeException($"Value '{parameter}' is less than zero.");

            return value;
        }

        private static int ParseCount(string parameter, int length)
        {
            int count;

            if (length == 0)
                return 0;

            if (parameter.Contains("%"))
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

        private static double ParsePercentage(string parameter, bool includeZero = true)
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
    }
}