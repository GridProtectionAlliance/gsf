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
// ReSharper disable StaticMemberInGenericType

using GrafanaAdapters.DataSources;
using GrafanaAdapters.DataSources.BuiltIn;
using GrafanaAdapters.Functions;
using GrafanaAdapters.Functions.BuiltIn;
using GrafanaAdapters.Model.Common;
using GSF.Collections;
using GSF.Diagnostics;
using GSF.TimeSeries;
using GSF.Units;
using GSF.Web;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace GrafanaAdapters;

/// <summary>
/// Represents a base implementation for Grafana data sources.
/// </summary>
[Serializable]
public abstract partial class GrafanaDataSourceBase
{
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
        // TODO: JRC - drop this temporary code once UI supports 'request.dataTypeIndex':
        if (request.dataTypeIndex == -1)
            request.dataTypeIndex = request.isPhasor ? PhasorValue.TypeIndex : DataSourceValue.TypeIndex;

        if (request.dataTypeIndex < 0 || request.dataTypeIndex >= DataSourceValueCache.LoadedTypes.Count)
            throw new IndexOutOfRangeException("Query request must specify a valid data type index.");

        // Execute specific process query request handler function for the requested data type
        return ProcessQueryRequestFunctions[request.dataTypeIndex](this, request, cancellationToken);
    }

    private static async Task<IEnumerable<TimeSeriesValues>> ProcessQueryRequestAsync<T>(GrafanaDataSourceBase instance, QueryRequest request, CancellationToken cancellationToken) where T : struct, IDataSourceValue<T>
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
                MetadataSelections = target.metadataSelections.Select(selection => (selection.tableName, selection.fieldNames)).ToArray()
            });
        }

        // Handle metadata augmentation for data source value type
        default(T).AugmentMetadata?.Invoke(instance.Metadata);

        List<DataSourceValueGroup<T>> valueGroups = new();

        // Query each target -- each returned value group has a 'Source' value enumerable that may contain deferred
        // enumerations that need evaluation before the final result can be serialized and returned to Grafana
        foreach (QueryParameters queryParameters in targetQueryParameters)
            await foreach (DataSourceValueGroup<T> valueGroup in QueryTargetAsync<T>(instance, queryParameters, queryParameters.SourceTarget.target, cancellationToken).ConfigureAwait(false))
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
            if (request.excludeNormalFlags)
                values = values.Where(value => value.Flags != MeasurementStateFlags.Normal);

            if (request.excludedFlags > 0)
                values = values.Where(value => ((uint)value.Flags & request.excludedFlags) == 0);

            // For deferred enumerations, function operations are executed here by ToArrayAsync() at the last moment
            series.datapoints = await values.Select(dataValue => dataValue.TimeSeriesValue).ToArrayAsync(cancellationToken).ConfigureAwait(false);
        });

        await Task.WhenAll(processValueGroups).ConfigureAwait(false);

        // Drop any empty series, if requested
        IEnumerable<TimeSeriesValues> filteredResults = seriesResults.Where(values => !values.dropEmptySeries || values.datapoints.Length > 0);

        // Apply any encountered ad-hoc filters
        if (request.adhocFilters?.Length > 0)
            foreach (AdHocFilter filter in request.adhocFilters)
                filteredResults = filteredResults.Where(values => IsFilterMatch<T>(values.rootTarget, filter, instance.Metadata));

        return filteredResults;
    }

    // This function is re-entrant so that it operates with functions as a depth-first recursive expression parser
    private static async IAsyncEnumerable<DataSourceValueGroup<T>> QueryTargetAsync<T>(GrafanaDataSourceBase instance, QueryParameters queryParameters, string queryExpression, [EnumeratorCancellation] CancellationToken cancellationToken) where T : struct, IDataSourceValue<T>
    {
        DataSet metadata = instance.Metadata;

        // A single target might look like the following - nested functions with multiple targets are supported:
        // PPA:15; STAT:20; SETSUM(COUNT(PPA:8; PPA:9; PPA:10)); FILTER ActiveMeasurements WHERE SignalType IN ('IPHA', 'VPHA'); RANGE(PPA:99; SUM(FILTER ActiveMeasurements WHERE SignalType = 'FREQ'; STAT:12))
        HashSet<string> targetSet = new(new[] { queryExpression }, StringComparer.OrdinalIgnoreCase); // Targets include user provided input, so casing is ignored

        // Parse out and cache any top-level grafana functions found in target expression
        (ParsedGrafanaFunction<T>[] grafanaFunctions, HashSet<string> reducedTargetSet) = TargetCache<(ParsedGrafanaFunction<T>[], HashSet<string>)>.GetOrAdd($"{queryExpression}-{typeof(T).FullName}", () =>
        {
            // Match any top-level grafana functions in target query expression
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

            return (matchedFunctions, reducedTargetSet);
        });

        if (grafanaFunctions.Length > 0)
        {
            // Execute each top-level grafana function, sub-functions are executed in a recursive call (depth first)
            foreach (ParsedGrafanaFunction<T> parsedFunction in grafanaFunctions)
            {
                // ExecuteGrafanaFunctionAsync calls QueryTargetAsync (this function) in order to process sub-functions or query needed data at final depth
                await foreach (DataSourceValueGroup<T> valueGroup in ExecuteGrafanaFunctionAsync(instance, parsedFunction, queryParameters, cancellationToken).ConfigureAwait(false))
                    yield return valueGroup;
            }

            // Further operations use reduced target set that exclude any now handled grafana functions
            targetSet = reducedTargetSet;
        }

        // Continue when there are any remaining targets to query
        if (targetSet.Count == 0)
            yield break;

        // Split remaining targets on semi-colon, this way even multiple filter expressions can be used as inputs to functions
        string[] allTargets = targetSet.Select(target => target.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)).SelectMany(currentTargets => currentTargets).ToArray();

        // Expand target set to include point tags for all parsed inputs, parsing target expression into individual point tags,
        // this step will convert any defined filter expressions into point tags:
        foreach (string target in allTargets)
            targetSet.UnionWith(TargetCache<string[]>.GetOrAdd(target, () => target.ParseExpressionAsTags<T>(metadata)));

        // Target set may now contain both original expressions, e.g., Guid (signal ID) or measurement key (source:ID), and newly
        // parsed individual point tags. For the final point list we are only interested in the point tags. Convert all remaining
        // targets to point tags, removing any duplicates, and create a map of the queryable data source point IDs to point tags:
        (Dictionary<ulong, string> targetMap, object state) = default(T).GetIDTargetMap(metadata, targetSet);
        Dictionary<string, SortedList<double, T>> targetValues = new(StringComparer.OrdinalIgnoreCase);

        // Query underlying data source, assigning each value to its own data source target time-value map, thus sorting values per target and time
        await foreach (DataSourceValue dataValue in instance.QueryDataSourceValues(queryParameters, targetMap, cancellationToken).ConfigureAwait(false))
            default(T).AssignToTimeValueMap(dataValue, targetValues.GetOrAdd(dataValue.Target, _ => new SortedList<double, T>()), state);

        // Transpose each target into a data source value group along with its associated queried values
        foreach (KeyValuePair<string, SortedList<double, T>> item in targetValues)
        {
            yield return new DataSourceValueGroup<T>
            {
                Target = item.Key,
                RootTarget = item.Key,
                SourceTarget = queryParameters.SourceTarget,
                Source = item.Value.Values.ToAsyncEnumerable(),
                DropEmptySeries = queryParameters.DropEmptySeries,
                RefID = queryParameters.SourceTarget.refId,
                MetadataMap = metadata.GetMetadataMap<T>(item.Key, queryParameters)
            };
        }
    }

    private static async IAsyncEnumerable<DataSourceValueGroup<T>> ExecuteGrafanaFunctionAsync<T>(GrafanaDataSourceBase instance, ParsedGrafanaFunction<T> parsedFunction, QueryParameters queryParameters, [EnumeratorCancellation] CancellationToken cancellationToken) where T : struct, IDataSourceValue<T>
    {
        IGrafanaFunction<T> function = parsedFunction.Function;
        GroupOperations groupOperation = parsedFunction.GroupOperation;
        string queryExpression = parsedFunction.Expression;
        DataSet metadata = instance.Metadata;

        // Parse out function parameters and remaining query expression (typically a filter expression)
        (string[] parsedParameters, queryExpression) = function.ParseParameters(queryParameters, queryExpression, groupOperation);

        // Reenter query function with remaining query expression to get data - at the bottom of the recursion,
        // this will return time-series data queried from the derived class using 'QueryDataSourceValues':
        IAsyncEnumerable<DataSourceValueGroup<T>> dataset = QueryTargetAsync<T>(instance, queryParameters, queryExpression, cancellationToken);

        // Handle series renaming operations as a special case
        if (function is Label<T>)
        {
            await foreach (DataSourceValueGroup<T> valueGroup in dataset.RenameSeries(queryParameters, metadata, parsedParameters[0], cancellationToken).ConfigureAwait(false))
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
                    Dictionary<string, string> metadataMap = metadata.GetMetadataMap<T>(rootTarget, queryParameters);
                    Parameters parameters = await function.GenerateParametersAsync(parsedParameters, valueGroup.Source, rootTarget, metadata, metadataMap, cancellationToken).ConfigureAwait(false);

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

                await foreach (DataSourceValueGroup<T> valueGroup in function.ComputeSliceAsync(scanner, queryParameters, queryExpression, metadata, parsedParameters.Skip(1).ToArray(), cancellationToken).ConfigureAwait(false))
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
                        MetadataMap = metadata.GetMetadataMap<T>(rootTarget, queryParameters)
                    };
                }

                break;
            }
            case GroupOperations.Set:
            {
                // Flatten all series into a single enumerable
                IAsyncEnumerable<T> dataSourceValues = dataset.SelectMany(source => source.Source);
                Dictionary<string, string> metadataMap = metadata.GetMetadataMap<T>(queryExpression, queryParameters);
                Parameters parameters = await function.GenerateParametersAsync(parsedParameters, dataSourceValues, queryExpression, metadata, metadataMap, cancellationToken).ConfigureAwait(false);

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
        #if DEBUG
            default:
            {
                Debug.Fail($"Unsupported group operation encountered: {groupOperation}");
                break;
            }
        #endif
        }
    }

    private static bool IsFilterMatch<T>(string target, AdHocFilter filter, DataSet metadata) where T : struct, IDataSourceValue<T>
    {
        // Default to positive match on failures
        return TargetCache<bool>.GetOrAdd($"filter!{filter.key}{filter.@operator}{filter.value}", () =>
        {
            try
            {
                DataRow record = default(T).LookupMetadata(metadata, default(T).MetadataTableName, target);

                if (record is null)
                    return true;

                dynamic left = record[filter.key];
                dynamic right = Convert.ChangeType(filter.value, record.Table.Columns[filter.key].DataType);

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
            catch (Exception ex)
            {
                Logger.SwallowException(ex, $"Failed to evaluate ad-hoc filter for target '{target}': {ex.Message}");
                return true;
            }
        });
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