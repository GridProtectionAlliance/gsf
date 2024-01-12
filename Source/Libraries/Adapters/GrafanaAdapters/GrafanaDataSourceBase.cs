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
// ReSharper disable AccessToModifiedClosure

using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using GSF.Units;
using GSF.Web;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using GrafanaAdapters.DataSources;
using GrafanaAdapters.Functions;
using GrafanaAdapters.Functions.BuiltIn;
using GrafanaAdapters.Model.Common;
using GSF.Collections;

namespace GrafanaAdapters;

/// <summary>
/// Represents a base implementation for Grafana data sources.
/// </summary>
[Serializable]
public abstract partial class GrafanaDataSourceBase
{
    private DataSet m_dataset;

    /// <summary>
    /// Gets or sets instance name for this <see cref="GrafanaDataSourceBase"/> implementation.
    /// </summary>
    public virtual string InstanceName { get; set; }

    /// <summary>
    /// Gets or sets <see cref="DataSet"/> based meta-data source available to this <see cref="GrafanaDataSourceBase"/> implementation.
    /// </summary>
    public virtual DataSet Metadata
    {
        get => m_dataset;
        set
        {
            m_dataset = value;
        }
    }

    /// <summary>
    /// Gets or sets maximum number of search targets to return during a search query.
    /// </summary>
    public virtual int MaximumSearchTargetsPerRequest { get; set; } = 200;

    /// <summary>
    /// Gets or sets maximum number of annotations to return during an annotations query.
    /// </summary>
    public virtual int MaximumAnnotationsPerRequest { get; set; } = 100;

    /// <summary>
    /// Starts a query that will read data source values, given a set of point IDs and targets, over a time range.
    /// </summary>
    /// <param name="queryParameters">Query parameters.</param>
    /// <param name="targetMap">Set of IDs with associated targets to query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Queried data source data in terms of value and time.</returns>
    protected abstract IAsyncEnumerable<DataSourceValue> QueryDataSourceValues(QueryParameters queryParameters, Dictionary<ulong, string> targetMap, CancellationToken cancellationToken);

    /// <summary>
    /// Queries data source returning data as Grafana time-series data set.
    /// </summary>
    /// <param name="request">Query request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task<IEnumerable<TimeSeriesValues>> Query(QueryRequest request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.format) && !request.format.Equals("json", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Only JSON formatted query requests are currently supported.");

        // TODO: JRC - suggest renaming 'request.isPhasor' parameter to 'request.dataType' and making it a string, then check value, e.g., "PhasorValue" or "DataSourceValue"

        return request.isPhasor ? 
            ProcessQueryRequestAsync<PhasorValue>(request, cancellationToken) : 
            ProcessQueryRequestAsync<DataSourceValue>(request, cancellationToken);

        // FUTURE: Dynamic types could be supported by loading all found implementations of 'IDataSourceValue' interface.
    }

    private async Task<IEnumerable<TimeSeriesValues>> ProcessQueryRequestAsync<T>(QueryRequest request, CancellationToken cancellationToken) where T : struct, IDataSourceValue<T>
    {
        DateTime startTime = request.range.from.ParseJsonTimestamp();
        DateTime stopTime = request.range.to.ParseJsonTimestamp();

        foreach (Target target in request.targets)
            target.target = target.target?.Trim() ?? string.Empty;

        List<QueryParameters> targetQueryParameters = new();

        // Create query parameters for each target
        foreach (Target target in request.targets)
        {
            // Parse any query commands in target
            (bool dropEmptySeries, bool includePeaks, bool fullResolutionQuery, string imports, target.target) = 
                ParseQueryCommands(target.target);

            targetQueryParameters.Add(new QueryParameters
            {
                SourceTarget = target,
                StartTime = startTime,
                StopTime = stopTime,
                Interval = fullResolutionQuery ? "0s" : request.interval,
                IncludePeaks = includePeaks && !fullResolutionQuery,
                DropEmptySeries = dropEmptySeries,
                Imports = imports,
                MetadataSelection = target.metadataSelection
            });
        }

        List<DataSourceValueGroup<T>> valueGroups = new();

        // Query each target -- each returned value group has a 'Source' value enumerable that may contain deferred
        // enumerations that need evaluation before the final result can be serialized and returned to Grafana
        foreach (QueryParameters queryParameters in targetQueryParameters)
            await foreach (DataSourceValueGroup<T> valueGroup in QueryTargetAsync<T>(queryParameters, queryParameters.SourceTarget.target, cancellationToken).ConfigureAwait(false))
                valueGroups.Add(valueGroup);

        // Establish one result series for each value group so that consistent order can be maintained between calls
        TimeSeriesValues[] seriesResults = new TimeSeriesValues[valueGroups.Count];

        // Process value group data in parallel
        IEnumerable<Task> processValueGroups = valueGroups.Select(async (valueGroup, index) =>
        {
            // Create a time series values query result instance that will be serialized as JSON and returned
            // to Grafana. A time series values result is created for each value group that will hold evaluated
            // value group results. Result series are added in the same sequence as the value groups so that
            // order remains consistent between Grafana query calls:
            TimeSeriesValues series = seriesResults[index] = new TimeSeriesValues
            {
                target = valueGroup.Target,
                rootTarget = valueGroup.RootTarget,
                meta = valueGroup.MetadataMap,
                dropEmptySeries = valueGroup.DropEmptySeries,
                refId = valueGroup.RefID
            };

            IAsyncEnumerable<T> values = valueGroup.Source;

            // Apply any requested flag filters applied for the data source
            if (valueGroup.SourceTarget?.excludeNormalFlags ?? false)
                values = values.Where(value => value.Flags != MeasurementStateFlags.Normal);

            if (valueGroup.SourceTarget?.excludedFlags > uint.MinValue)
                values = values.Where(value => ((uint)value.Flags & valueGroup.SourceTarget.excludedFlags) == 0);

            // For deferred enumerations, function operations are executed here by ToListAsync() at the last moment
            series.datapoints = await values.Select(dataValue => dataValue.TimeSeriesValue).ToListAsync(cancellationToken).ConfigureAwait(false);
        });

        await Task.WhenAll(processValueGroups).ConfigureAwait(false);

        // Drop any empty series, if requested
        IEnumerable<TimeSeriesValues> filteredResults = seriesResults.Where(values => !values.dropEmptySeries || values.datapoints.Count > 0);
        
        // Apply any encountered ad-hoc filters
        if (request.adhocFilters?.Count > 0)
            foreach (AdHocFilter filter in request.adhocFilters)
                filteredResults = filteredResults.Where(values => IsFilterMatch(values.rootTarget, filter));

        return filteredResults;
    }

    // This function is re-entrant so that it operates with functions as a depth-first recursive expression parser
    private async IAsyncEnumerable<DataSourceValueGroup<T>> QueryTargetAsync<T>(QueryParameters queryParameters, string queryExpression, [EnumeratorCancellation] CancellationToken cancellationToken) where T : struct, IDataSourceValue<T>
    {
        // A single target might look like the following - nested functions with multiple targets are supported:
        // PPA:15; STAT:20; SETSUM(COUNT(PPA:8; PPA:9; PPA:10)); FILTER ActiveMeasurements WHERE SignalType IN ('IPHA', 'VPHA'); RANGE(PPA:99; SUM(FILTER ActiveMeasurements WHERE SignalType = 'FREQ'; STAT:12))
        HashSet<string> targetSet = new(new[] { queryExpression }, StringComparer.OrdinalIgnoreCase); // Targets include user provided input, so casing is ignored

        // Parse out and cache any top-level grafana functions found in target expression
        (ParsedGrafanaFunction<T>[] grafanaFunctions, HashSet<string> reducedTargetSet) = TargetCache<(ParsedGrafanaFunction<T>[], HashSet<string>)>.GetOrAdd($"{queryExpression}-{typeof(T).FullName}", () =>
        {
            ParsedGrafanaFunction<T>[] matchedFunctions = FunctionParsing.MatchFunctions<T>(queryExpression);
            HashSet<string> reducedTargetSet = new(StringComparer.OrdinalIgnoreCase);

            if (matchedFunctions.Length > 0)
            {
                // Reduce target to non-function expressions - important so later split on ';' succeeds properly
                string reducedTarget = queryExpression;

                foreach (string functionExpression in matchedFunctions.Select(function => function.MatchedValue))
                    reducedTarget = reducedTarget.Replace(functionExpression, "");

                // FUTURE: as a convenience to the end user, it should be possible here to check for any named targets in the
                // function expressions and make sure they get added to the reduced target set -- note: because of the recursive
                // nature of this function, only the top-level named targets would be checked. Doing this would allow targets that
                // were only defined as named targets to be queried without the user having to additionally specify them in the
                // query expression. This would not, however, prevent the data from being trended. In order to prevent named
                // targets from appearing in the trended data (at least for those that were not already in the query results),
                // named targets would have to be tracked here and removed from the trended data before exiting this function.

                if (!string.IsNullOrWhiteSpace(reducedTarget))
                    reducedTargetSet.Add(reducedTarget);
            }
            else
            {
                reducedTargetSet.Add(queryExpression);
            }

            return (matchedFunctions, reducedTargetSet);
        });

        if (grafanaFunctions.Length > 0)
        {
            // Execute each top-level grafana function, sub-functions are executed in a recursive call (depth first)
            foreach (ParsedGrafanaFunction<T> parsedFunction in grafanaFunctions)
            {
                // ExecuteGrafanaFunctionAsync calls QueryTargetAsync (this function) in order to process sub-functions or query needed data at final depth
                await foreach (DataSourceValueGroup<T> valueGroup in ExecuteGrafanaFunctionAsync(parsedFunction, queryParameters, cancellationToken).ConfigureAwait(false))
                    yield return valueGroup;
            }

            // Further operations use reduced target set that exclude any now handled grafana functions
            targetSet = reducedTargetSet;
        }

        // Query any remaining targets
        if (targetSet.Count == 0)
            yield break;

        // Split remaining targets on semi-colon, this way even multiple filter expressions can be used as inputs to functions
        string[] allTargets = targetSet.Select(target => target.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)).SelectMany(currentTargets => currentTargets).ToArray();

        // Expand target set to include point tags for all parsed inputs
        foreach (string target in allTargets)
        {
            targetSet.UnionWith(TargetCache<string[]>.GetOrAdd(target, () =>
            {
                // TODO: JRC - it is likely that target data sources will simply have their own metadata tables that will work with the following parsing code
                // Each IDataSourceValue interface just needs to define its primary metadata table and pass it into the ParseInputMeasurementKeys function

                // Parse target expression into individual point tags - this will convert filter expressions into point tags
                MeasurementKey[] results = AdapterBase.ParseInputMeasurementKeys(Metadata, false, target.SplitAlias(out string alias));

                if (!string.IsNullOrWhiteSpace(alias) && results.Length == 1)
                    return new[] { $"{alias}={results[0].TagFromKey(Metadata)}" };

                return results.Select(key => key.TagFromKey(Metadata)).ToArray();
            }));
        }

        // Target set may now contain both original expressions, e.g., Guid (signal ID) or measurement key (source:ID), and newly
        // parsed individual point tags. For the final point list we are only interested in the point tags. Convert all remaining
        // targets to point tags, removing any duplicates, and create a map of the queryable data source point IDs to point tags:
        (Dictionary<ulong, string> targetMap, object state) = default(T).GetIDTargetMap(Metadata, targetSet);
        Dictionary<string, List<T>> targetValues = new(); 

        // Query underlying data source, assigning each value to its own data source target list, sorting results per target
        await foreach (DataSourceValue dataValue in QueryDataSourceValues(queryParameters, targetMap, cancellationToken).ConfigureAwait(false))
            default(T).AssignValueToTargetList(dataValue, targetValues.GetOrAdd(dataValue.Target, _ => new List<T>()), state);

        // Transpose each target into a data source value group along with its associated queried values
        foreach (KeyValuePair<string, List<T>> item in targetValues)
        {
            yield return new DataSourceValueGroup<T>
            {
                Target = item.Key,
                RootTarget = item.Key,
                SourceTarget = queryParameters.SourceTarget,
                Source = item.Value.ToAsyncEnumerable(),
                DropEmptySeries = queryParameters.DropEmptySeries,
                RefID = queryParameters.SourceTarget.refId,
                MetadataMap = Metadata.GetMetadataMap<T>(item.Key, queryParameters)
            };
        }
    }

    private async IAsyncEnumerable<DataSourceValueGroup<T>> ExecuteGrafanaFunctionAsync<T>(ParsedGrafanaFunction<T> parsedFunction, QueryParameters queryParameters, [EnumeratorCancellation] CancellationToken cancellationToken) where T : struct, IDataSourceValue<T>
    {
        IGrafanaFunction<T> function = parsedFunction.Function;
        GroupOperations groupOperation = parsedFunction.GroupOperation;
        string queryExpression = parsedFunction.Expression;

        // Parse out function parameters and remaining query expression (typically a filter expression)
        (string[] parsedParameters, queryExpression) = function.ParseParameters(queryParameters, queryExpression, groupOperation);

        // Reenter query function with remaining query expression to get data - at the bottom of the recursion,
        // this will return time-series data queried from the derived class using 'QueryDataSourceValues':
        IAsyncEnumerable<DataSourceValueGroup<T>> dataset = QueryTargetAsync<T>(queryParameters, queryExpression, cancellationToken);

        // Handle series renaming operations as a special case
        if (function is Label<T>)
        {
            await foreach (DataSourceValueGroup<T> valueGroup in dataset.RenameSeries(queryParameters, Metadata, parsedParameters[0], cancellationToken).ConfigureAwait(false))
                yield return valueGroup;

            yield break;
        }

        // Handle function based on selected group operation
        switch (groupOperation)
        {
            case GroupOperations.None:
            {
                await foreach (DataSourceValueGroup<T> valueGroup in dataset.ConfigureAwait(false))
                {
                    string rootTarget = valueGroup.RootTarget ?? valueGroup.Target;
                    Dictionary<string, string> metadataMap = Metadata.GetMetadataMap<T>(rootTarget, queryParameters);
                    Parameters parameters = await function.GenerateParametersAsync(parsedParameters, valueGroup.Source, rootTarget, Metadata, metadataMap, cancellationToken).ConfigureAwait(false);

                    yield return new DataSourceValueGroup<T>
                    {
                        Target = function.FormatTargetName(groupOperation, valueGroup.Target, parsedParameters),
                        RootTarget = rootTarget,
                        SourceTarget = queryParameters.SourceTarget,
                        Source = function.ComputeAsync(parameters, cancellationToken),
                        DropEmptySeries = queryParameters.DropEmptySeries,
                        RefID = queryParameters.SourceTarget.refId,
                        MetadataMap = metadataMap
                    };
                }

                break;
            }
            case GroupOperations.Slice:
            {
                if (!double.TryParse(parsedParameters[0], out double tolerance))
                    throw new InvalidOperationException($"Invalid slice interval specified for {function.Name} function.");

                TimeSliceScannerAsync<T> scanner = await TimeSliceScannerAsync<T>.Create(dataset, tolerance / SI.Milli, cancellationToken).ConfigureAwait(false);

                await foreach (DataSourceValueGroup<T> valueGroup in function.ComputeSliceAsync(scanner, queryParameters, queryExpression, Metadata, parsedParameters.Skip(1).ToArray(), cancellationToken).ConfigureAwait(false))
                {
                    string rootTarget = valueGroup.RootTarget ?? valueGroup.Target;

                    yield return new DataSourceValueGroup<T>
                    {
                        Target = function.FormatTargetName(groupOperation, rootTarget, parsedParameters),
                        RootTarget = rootTarget,
                        SourceTarget = queryParameters.SourceTarget,
                        Source = valueGroup.Source,
                        DropEmptySeries = queryParameters.DropEmptySeries,
                        RefID = queryParameters.SourceTarget.refId,
                        MetadataMap = Metadata.GetMetadataMap<T>(rootTarget, queryParameters)
                    };
                }

                break;
            }
            case GroupOperations.Set:
            {
                // Flatten all series into a single enumerable
                IAsyncEnumerable<T> dataSourceValues = dataset.SelectMany(source => source.Source);
                Dictionary<string, string> metadataMap = Metadata.GetMetadataMap<T>(queryExpression, queryParameters);
                Parameters parameters = await function.GenerateParametersAsync(parsedParameters, dataSourceValues, queryExpression, Metadata, metadataMap, cancellationToken).ConfigureAwait(false);

                DataSourceValueGroup<T> valueGroup = new()
                {
                    Target = function.FormatTargetName(groupOperation, queryExpression, parsedParameters),
                    RootTarget = queryExpression,
                    SourceTarget = queryParameters.SourceTarget,
                    Source = function.ComputeSetAsync(parameters, cancellationToken),
                    DropEmptySeries = queryParameters.DropEmptySeries,
                    RefID = queryParameters.SourceTarget.refId,
                    MetadataMap = metadataMap
                };

                // Handle set operations for functions where there is data in the target series as well, e.g., Min or Max
                if (function.ResultIsSetTargetSeries)
                {
                    T dataValue = await valueGroup.Source.FirstAsync(cancellationToken).ConfigureAwait(false);
                    valueGroup.Target = $"Set{function.Name} = {dataValue.Target}";
                    valueGroup.RootTarget = dataValue.Target;
                }

                yield return valueGroup;

                break;
            }
            default:
            {
                throw new ArgumentOutOfRangeException(nameof(groupOperation), $"Unsupported group operation encountered: {groupOperation}");
            }
        }
    }

    private bool IsFilterMatch(string target, AdHocFilter filter)
    {
        // Default to positive match on failures
        return TargetCache<bool>.GetOrAdd($"filter!{filter.key}{filter.@operator}{filter.value}", () =>
        {
            try
            {
                DataRow metadata = lookupTargetMetadata(target);

                if (metadata is null)
                    return true;

                dynamic left = metadata[filter.key];
                dynamic right = Convert.ChangeType(filter.value, metadata.Table.Columns[filter.key].DataType);

                return filter.@operator switch
                {
                    "=" => left == right,
                    "==" => left == right,
                    "!=" => left != right,
                    "<>" => left != right,
                    "<" => left < right,
                    "<=" => left <= right,
                    ">" => left > right,
                    ">=" => left >= right,
                    _ => true
                };
            }
            catch
            {
                return true;
            }
        });

        DataRow lookupTargetMetadata(string pointTag)
        {
            return TargetCache<DataRow>.GetOrAdd(pointTag, () =>
            {
                try
                {
                    return Metadata.Tables["ActiveMeasurements"].Select($"PointTag = '{pointTag}'").FirstOrDefault();
                }
                catch
                {
                    return null;
                }
            });
        }
    }

    private static (bool, bool, bool, string, string) ParseQueryCommands(string expression)
    {
        // Parse and cache any query commands found in target expression
        return TargetCache<(bool, bool, bool, string, string)>.GetOrAdd(expression, () =>
        {
            bool dropEmptySeries = false;
            bool includePeaks = false;
            bool fullResolutionQuery = false;
            string imports = string.Empty;

            Match commandMatch = s_dropEmptySeriesCommand.Match(expression);

            if (commandMatch.Success)
            {
                dropEmptySeries = true;
                expression = expression.Replace(commandMatch.Value, "");
            }

            commandMatch = s_includePeaksCommand.Match(expression);

            if (commandMatch.Success)
            {
                includePeaks = true;
                expression = expression.Replace(commandMatch.Value, "");
            }

            commandMatch = s_fullResolutionQueryCommand.Match(expression);

            if (commandMatch.Success)
            {
                fullResolutionQuery = true;
                expression = expression.Replace(commandMatch.Value, "");
            }

            commandMatch = s_importsCommand.Match(expression);

            if (commandMatch.Success)
            {
                string result = commandMatch.Result("${Expression}");
                imports = result.Trim();
                expression = expression.Replace(commandMatch.Value, "");
            }

            return (dropEmptySeries, includePeaks, fullResolutionQuery, imports, expression);
        });
    }

    // Query command regular expressions include a semi-colon prefix to help prevent possible name matches that may occur on other expressions
    private static readonly Regex s_dropEmptySeriesCommand = new(@";\s*DropEmptySeries", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex s_includePeaksCommand = new(@";\s*IncludePeaks", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex s_fullResolutionQueryCommand = new(@";\s*FullResolutionQuery", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex s_importsCommand = new(@";\s*Imports\s*=\s*\{(?<Expression>.+)\}", RegexOptions.Compiled | RegexOptions.IgnoreCase);
}