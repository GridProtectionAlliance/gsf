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

using GSF;
using GSF.Data;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using GSF.Web;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ParsedFunction = System.Tuple<GrafanaAdapters.SeriesFunction, string, GrafanaAdapters.GroupOperation, string>;

namespace GrafanaAdapters
{
    /// <summary>
    /// Represents a base implementation for Grafana data sources.
    /// </summary>
    [Serializable]
    public abstract partial class GrafanaDataSourceBase
    {
        #region [ Members ]

        // Constants
        private const string DropEmptySeriesCommand = "dropemptyseries";
        private const string IncludePeaksCommand = "includepeaks";

        #endregion

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
        /// Starts a query that will read data source values, given a set of point IDs and targets, over a time range.
        /// </summary>
        /// <param name="startTime">Start-time for query.</param>
        /// <param name="stopTime">Stop-time for query.</param>
        /// <param name="interval">Interval from Grafana request.</param>
        /// <param name="includePeaks">Flag that determines if decimated data should include min/max interval peaks over provided time range.</param>
        /// <param name="targetMap">Set of IDs with associated targets to query.</param>
        /// <returns>Queried data source data in terms of value and time.</returns>
        protected abstract IEnumerable<DataSourceValue> QueryDataSourceValues(DateTime startTime, DateTime stopTime, string interval, bool includePeaks, Dictionary<ulong, string> targetMap);

        /// <summary>
        /// Queries data source returning data as Grafana time-series data set.
        /// </summary>
        /// <param name="request">Query request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public Task<List<TimeSeriesValues>> Query(QueryRequest request, CancellationToken cancellationToken)
        {
            bool isFilterMatch(string target, AdHocFilter filter)
            {
                // Default to positive match on failures
                return TargetCache<bool>.GetOrAdd($"filter!{filter.key}{filter.@operator}{filter.value}", () => {
                    try
                    {
                        DataRow metadata = LookupTargetMetadata(target);

                        if (metadata == null)
                            return true;

                        dynamic left = metadata[filter.key];
                        dynamic right = Convert.ChangeType(filter.value, metadata.Table.Columns[filter.key].DataType);

                        switch (filter.@operator)
                        {
                            case "=":
                            case "==":
                                return left == right;
                            case "!=":
                            case "<>":
                                return left != right;
                            case "<":
                                return left < right;
                            case "<=":
                                return left <= right;
                            case ">":
                                return left > right;
                            case ">=":
                                return left >= right;
                        }

                        return true;
                    }
                    catch
                    {
                        return true;
                    }
                });
            }

            float lookupTargetCoordinate(string target, string field)
            {
                return TargetCache<float>.GetOrAdd($"{target}_{field}", () =>
                    LookupTargetMetadata(target)?.ConvertNullableField<float>(field) ?? 0.0F);
            }

            // Task allows processing of multiple simultaneous queries
            return Task.Factory.StartNew(() =>
            {
                if (!string.IsNullOrWhiteSpace(request.format) && !request.format.Equals("json", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Only JSON formatted query requests are currently supported.");

                DateTime startTime = request.range.from.ParseJsonTimestamp();
                DateTime stopTime = request.range.to.ParseJsonTimestamp();

                foreach (Target target in request.targets)
                    target.target = target.target?.Trim() ?? "";

                DataSourceValueGroup[] valueGroups = request.targets.Select(target => QueryTarget(target, target.target, startTime, stopTime, request.interval, false, false, null, cancellationToken)).SelectMany(groups => groups).ToArray();

                // Establish result series sequentially so that order remains consistent between calls
                List<TimeSeriesValues> result = valueGroups.Select(valueGroup => new TimeSeriesValues
                {
                    target = valueGroup.Target,
                    rootTarget = valueGroup.RootTarget,
                    meta = new()
                    {
                        custom = new()
                        {
                            Latitude = lookupTargetCoordinate(valueGroup.RootTarget, "Latitude"),
                            Longitude = lookupTargetCoordinate(valueGroup.RootTarget, "Longitude")
                        }
                    },
                    dropEmptySeries = valueGroup.DropEmptySeries,
                    refId = valueGroup.refId
                }).ToList();

                // Apply any encountered ad-hoc filters
                if (request.adhocFilters?.Count > 0)
                {
                    foreach (AdHocFilter filter in request.adhocFilters)
                        result = result.Where(values => isFilterMatch(values.rootTarget, filter)).ToList();
                }

                // Process series data in parallel
                Parallel.ForEach(result, new() { CancellationToken = cancellationToken }, series =>
                {
                    // For deferred enumerations, any work to be done is left till last moment - in this case "ToList()" invokes actual operation                    
                    DataSourceValueGroup valueGroup = valueGroups.First(group => group.Target.Equals(series.target));
                    IEnumerable<DataSourceValue> values = valueGroup.Source;

                    if (valueGroup.SourceTarget?.excludeNormalFlags ?? false)
                        values = values.Where(value => value.Flags != MeasurementStateFlags.Normal);

                    if (valueGroup.SourceTarget?.excludedFlags > uint.MinValue)
                        values = values.Where(value => ((uint)value.Flags & valueGroup.SourceTarget.excludedFlags) == 0);

                    series.datapoints = values.Select(dataValue => new[] { dataValue.Value, dataValue.Time }).ToList();
                });

                #region [ Original "request.maxDataPoints" Implementation ]

                //int maxDataPoints = (int)(request.maxDataPoints * 1.1D);

                //// Make a final pass through data to decimate returned point volume (for graphing purposes), if needed

                //foreach (TimeSeriesValues series in result)
                //{
                //    if (series.datapoints.Count > maxDataPoints)
                //    {
                //        double indexFactor = series.datapoints.Count / (double)request.maxDataPoints;
                //        series.datapoints = Enumerable.Range(0, request.maxDataPoints).Select(index => series.datapoints[(int)(index * indexFactor)]).ToList();
                //    }
                //}

                #endregion

                return result.Where(values => !values.dropEmptySeries || values.datapoints.Count > 0).ToList();
            },
            cancellationToken);
        }

        private IEnumerable<DataSourceValueGroup> QueryTarget(Target sourceTarget, string queryExpression, DateTime startTime, DateTime stopTime, string interval, bool includePeaks, bool dropEmptySeries, string imports, CancellationToken cancellationToken)
        {
            // Handle query commands
            if (queryExpression.ToLowerInvariant().Contains(DropEmptySeriesCommand))
            {
                dropEmptySeries = true;
                queryExpression = queryExpression.ReplaceCaseInsensitive(DropEmptySeriesCommand, "");
            }

            if (queryExpression.ToLowerInvariant().Contains(IncludePeaksCommand))
            {
                includePeaks = true;
                queryExpression = queryExpression.ReplaceCaseInsensitive(IncludePeaksCommand, "");
            }

            Match importsCommandMatch = s_importsCommand.Match(queryExpression);

            if (importsCommandMatch.Success)
            {
                string result = importsCommandMatch.Result("${Expression}");
                imports = result.Trim();
                queryExpression = queryExpression.Replace(result, "");
            }

            // A single target might look like the following:
            // PPA:15; STAT:20; SETSUM(COUNT(PPA:8; PPA:9; PPA:10)); FILTER ActiveMeasurements WHERE SignalType IN ('IPHA', 'VPHA'); RANGE(PPA:99; SUM(FILTER ActiveMeasurements WHERE SignalType = 'FREQ'; STAT:12))

            HashSet<string> targetSet = new HashSet<string>(new[] { queryExpression }, StringComparer.OrdinalIgnoreCase); // Targets include user provided input, so casing should be ignored
            HashSet<string> reducedTargetSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            List<Match> seriesFunctions = new List<Match>();

            foreach (string target in targetSet)
            {
                // Find any series functions in target
                Match[] matchedFunctions = TargetCache<Match[]>.GetOrAdd(target, () =>
                    s_seriesFunctions.Matches(target).Cast<Match>().ToArray());

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
                foreach (ParsedFunction parsedFunction in seriesFunctions.Select(match => ParseSeriesFunction(match, imports)))
                    foreach (DataSourceValueGroup valueGroup in ExecuteSeriesFunction(sourceTarget, parsedFunction, startTime, stopTime, interval, includePeaks, dropEmptySeries, cancellationToken))
                        yield return valueGroup;

                // Use reduced target set that excludes any series functions
                targetSet = reducedTargetSet;
            }

            // Query any remaining targets
            if (targetSet.Count > 0)
            {
                // Split remaining targets on semi-colon, this way even multiple filter expressions can be used as inputs to functions
                string[] allTargets = targetSet.Select(target => target.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)).SelectMany(currentTargets => currentTargets).ToArray();

                Dictionary<ulong, string> targetMap = new Dictionary<ulong, string>();

                // Expand target set to include point tags for all parsed inputs
                foreach (string target in allTargets)
                {
                    targetSet.UnionWith(TargetCache<string[]>.GetOrAdd(target, () =>
                    {
                        MeasurementKey[] results = AdapterBase.ParseInputMeasurementKeys(Metadata, false, target.SplitAlias(out string alias));

                        if (!string.IsNullOrWhiteSpace(alias) && results.Length == 1)
                            return new[] { $"{alias}={results[0].TagFromKey(Metadata)}" };

                        return results.Select(key => key.TagFromKey(Metadata)).ToArray();
                    }));
                }

                // Target set now contains both original expressions and newly parsed individual point tags - to create final point list we
                // are only interested in the point tags, provided either by direct user entry or derived by parsing filter expressions
                foreach (string target in targetSet)
                {
                    // Reduce all targets down to a dictionary of point ID's mapped to point tags
                    MeasurementKey key = TargetCache<MeasurementKey>.GetOrAdd(target, () => target.KeyFromTag(Metadata));

                    if (key == MeasurementKey.Undefined)
                    {
                        Tuple<MeasurementKey, string> result = TargetCache<Tuple<MeasurementKey, string>>.GetOrAdd($"signalID@{target}", () => target.KeyAndTagFromSignalID(Metadata));

                        key = result.Item1;
                        string pointTag = result.Item2;

                        if (key == MeasurementKey.Undefined)
                        {
                            result = TargetCache<Tuple<MeasurementKey, string>>.GetOrAdd($"key@{target}", () =>
                            {
                                MeasurementKey.TryParse(target, out MeasurementKey parsedKey);
                                return new(parsedKey, parsedKey.TagFromKey(Metadata));
                            });

                            key = result.Item1;
                            pointTag = result.Item2;

                            if (key != MeasurementKey.Undefined)
                                targetMap[key.ID] = pointTag;
                        }
                        else
                        {
                            targetMap[key.ID] = pointTag;
                        }
                    }
                    else
                    {
                        targetMap[key.ID] = target;
                    }
                }

                // Query underlying data source for each target - to prevent parallel read from data source we enumerate immediately
                List<DataSourceValue> dataValues = QueryDataSourceValues(startTime, stopTime, interval, includePeaks, targetMap)
                    .TakeWhile(_ => !cancellationToken.IsCancellationRequested).ToList();

                foreach (KeyValuePair<ulong, string> target in targetMap)
                    yield return new()
                    {
                        Target = target.Value,
                        RootTarget = target.Value,
                        SourceTarget = sourceTarget,
                        Source = dataValues.Where(dataValue => dataValue.Target.Equals(target.Value)),
                        DropEmptySeries = dropEmptySeries,
                        refId = sourceTarget.refId
                    };
            }
        }

        private DataRow LookupTargetMetadata(string target)
        {
            return TargetCache<DataRow>.GetOrAdd(target, () =>
            {
                try
                {
                    return Metadata.Tables["ActiveMeasurements"].Select($"PointTag = '{target}'").FirstOrDefault();
                }
                catch
                {
                    return null;
                }
            });
        }

        #endregion
    }
}