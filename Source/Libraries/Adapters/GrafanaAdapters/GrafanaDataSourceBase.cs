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
// ReSharper disable PossibleMultipleEnumeration

using GrafanaAdapters.DataSources;
using GrafanaAdapters.DataSources.BuiltIn;
using GrafanaAdapters.Functions;
using GrafanaAdapters.Functions.BuiltIn;
using GrafanaAdapters.Metadata;
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
    /// Gets or sets <see cref="DataSetAdapter"/> used to hold <see cref="DataSet"/> based metadata
    /// source available to this <see cref="GrafanaDataSourceBase"/> implementation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Class is used to augment meta-data for the target data source, on demand, so that the augmentation
    /// process only occurs when a data source type is first used.
    /// </para>
    /// <para>
    /// Note that the <see cref="DataSetAdapter"/> is implicitly convertible from a <see cref="DataSet"/>
    /// so that derived classes can assign a <see cref="DataSet"/> source directly as needed.
    /// </para>
    /// </remarks>
    public virtual DataSetAdapter Metadata { get; set; }

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
        if (request.dataTypeIndex < 0 || request.dataTypeIndex >= DataSourceValueCache.LoadedTypes.Count)
            throw new IndexOutOfRangeException("Query request must specify a valid data type index.");

        // Execute specific process query request handler function for the requested data type
        return ProcessQueryRequestFunctions[request.dataTypeIndex](this, request, cancellationToken);
    }

    // This function is called by ProcessQueryRequestFunctions delegate array
    private static async Task<IEnumerable<TimeSeriesValues>> ProcessQueryRequestAsync<T>(GrafanaDataSourceBase instance, QueryRequest request, CancellationToken cancellationToken) where T : struct, IDataSourceValue<T>
    {
        DateTime startTime = request.range.from.ParseJsonTimestamp();
        DateTime stopTime = request.range.to.ParseJsonTimestamp();
        List<QueryParameters> targetQueryParameters = new();

        // Create query parameters for each target
        foreach (Target target in request.targets)
        {
            if (string.IsNullOrWhiteSpace(target.target))
                continue;

            // Parse any query commands in target
            (bool dropEmptySeries, bool includePeaks, bool fullResolutionQuery, string imports, string radialDistribution, target.target) =
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
                RadialDistribution = radialDistribution,
                MetadataSelections = target.metadataSelections?
                     .Where(selection =>
                         !string.IsNullOrWhiteSpace(selection.tableName) &&
                         selection.fieldNames.Length > 0 &&
                         selection.fieldNames.All(fieldName => !string.IsNullOrWhiteSpace(fieldName)))
                    .Select(selection => (selection.tableName, selection.fieldNames))
                    .ToArray()
                    ?? Array.Empty<(string, string[])>()
            });
        }

        // Get augmented metadata for data source value type
        DataSet metadata = instance.Metadata.GetAugmentedDataSet<T>();

        List<DataSourceValueGroup<T>> valueGroups = new();

        // Query each target -- each returned value group has a 'Source' value enumerable that may contain deferred
        // enumerations that need evaluation before the final result can be serialized and returned to Grafana
        foreach (QueryParameters queryParameters in targetQueryParameters)
        {
            // Organize value groups by given Grafana target data source query, i.e., all results with the same RefID
            List<DataSourceValueGroup<T>> queryValueGroups = new();

            try
            {
                // Query target data source for values
                await foreach (DataSourceValueGroup<T> valueGroup in QueryTargetAsync<T>(instance, queryParameters, metadata, queryParameters.SourceTarget.target, cancellationToken).ConfigureAwait(false))
                    queryValueGroups.Add(valueGroup);

                // Check if metadata selections are defined and radial distribution is requested for this data source query
                if (queryParameters.MetadataSelections.Length > 0 && queryParameters.RadialDistribution.Length > 0)
                    await ProcessRadialDistributionAsync(queryValueGroups, queryParameters, cancellationToken).ConfigureAwait(false);
            }
            catch (SyntaxErrorException ex)
            {
                // Each RefID is a separate query, so report syntax errors on a per-query basis
                queryValueGroups.Add(DataSourceValueGroup<T>.FromException(queryParameters, ex.Message));
            }

            valueGroups.AddRange(queryValueGroups);
        }

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
                metadata = valueGroup.MetadataMap,
                dropEmptySeries = valueGroup.DropEmptySeries,
                refID = valueGroup.RefID,
                syntaxError = valueGroup.SyntaxError
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
                filteredResults = filteredResults.Where(values => IsFilterMatch<T>(values.rootTarget, filter, metadata));

        return filteredResults;
    }

    // This function is re-entrant so that it operates with functions as a depth-first recursive expression parser
    private static async IAsyncEnumerable<DataSourceValueGroup<T>> QueryTargetAsync<T>(GrafanaDataSourceBase instance, QueryParameters queryParameters, DataSet metadata, string queryExpression, [EnumeratorCancellation] CancellationToken cancellationToken) where T : struct, IDataSourceValue<T>
    {
        // A single target might look like the following - nested functions with multiple targets are supported:
        // PPA:15; STAT:20; SETSUM(COUNT(PPA:8; PPA:9; PPA:10)); FILTER ActiveMeasurements WHERE SignalType IN ('IPHA', 'VPHA'); RANGE(PPA:99; SUM(FILTER ActiveMeasurements WHERE SignalType = 'FREQ'; STAT:12))

        // Parse out and cache any top-level grafana functions found in query expression
        (ParsedGrafanaFunction<T>[] grafanaFunctions, string reducedQueryExpression) = TargetCache<(ParsedGrafanaFunction<T>[], string)>.GetOrAdd($"{default(T).DataTypeIndex}:{queryExpression}", () =>
        {
            // Match any top-level grafana functions in target query expression
            ParsedGrafanaFunction<T>[] matchedFunctions = FunctionParsing.MatchFunctions<T>(queryExpression, queryParameters);

            if (matchedFunctions.Length == 0)
                return (matchedFunctions, string.Empty);

            // Reduce query expression to non-function expressions
            string reducedQueryExpression = queryExpression;

            foreach (string functionExpression in matchedFunctions.Select(function => function.MatchedValue))
                reducedQueryExpression = reducedQueryExpression.Replace(functionExpression, "");

            // FUTURE: as a convenience to the end user, it should be possible here to check for any named targets in the
            // function expressions and make sure they get added to the reduced query expression -- note: because of the
            // recursive nature of this function, only the top-level named targets would be checked. Doing this would allow
            // targets that were only defined as named targets to be queried without the user having to additionally
            // specify them in the query expression. This would not, however, prevent the data from being trended. In order
            // to prevent named targets from appearing in the trended data (at least for those that were not already in the
            // query results), named targets would have to be tracked here and removed from the trended data before exiting
            // this function.

            return (matchedFunctions, reducedQueryExpression);
        });

        if (grafanaFunctions.Length > 0)
        {
            // Execute each top-level grafana function, sub-functions are executed in a recursive call (depth first)
            foreach (ParsedGrafanaFunction<T> parsedFunction in grafanaFunctions)
            {
                // ExecuteGrafanaFunctionAsync calls QueryTargetAsync (this function) in order to process sub-functions or query needed data at final depth
                await foreach (DataSourceValueGroup<T> valueGroup in ExecuteGrafanaFunctionAsync(instance, parsedFunction, queryParameters, metadata, cancellationToken).ConfigureAwait(false))
                    yield return valueGroup;
            }

            // Any further operations use reduced query expression that excludes any now-handled grafana functions
            queryExpression = reducedQueryExpression;
        }

        // Continue only when there is any remaining query expression
        if (string.IsNullOrWhiteSpace(queryExpression))
            yield break;

        // Create a map of the queryable data source point IDs to point tags - this will be used to query the data source
        // for the needed data points in the derived class 'QueryDataSourceValues' implementation
        Dictionary<ulong, string> targetMap = CreateTargetMap<T>(metadata, queryExpression);

        // Create a map of each target to its own time-value map which will group target values that are sorted by time
        Dictionary<string, SortedList<double, T>> targetValues = new(StringComparer.OrdinalIgnoreCase);

        // Query underlying data source, assigning each value to its own data source target time-value map
        await foreach (DataSourceValue dataValue in instance.QueryDataSourceValues(queryParameters, targetMap, cancellationToken).ConfigureAwait(false))
        {
            // Split combined target into point tag and target
            (string pointTag, string target) = dataValue.Target.SplitComponents();

            // Assign data value, identified by pointTag, to its own time-value map based on its target
            default(T).AssignToTimeValueMap(pointTag, dataValue with { Target = target }, targetValues.GetOrAdd(target, _ => new SortedList<double, T>()), metadata);
        }

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
                RefID = queryParameters.SourceTarget.refID,
                MetadataMap = metadata.GetMetadataMap<T>(item.Key, queryParameters)
            };
        }
    }

    private static Dictionary<ulong, string> CreateTargetMap<T>(DataSet metadata, string queryExpression) where T : struct, IDataSourceValue<T>
    {
        // Caching map by data source value type index and queryExpression for improved performance
        return TargetCache<Dictionary<ulong, string>>.GetOrAdd($"{default(T).DataTypeIndex}:{queryExpression}", () =>
        {
            Dictionary<ulong, string> targetMap = new();

            // Split remaining targets on semi-colon which will include possible filter expressions being used
            // as inputs to functions, also since expression includes user provided input, casing is ignored.
            HashSet<string> targetSet = new(s_semiColonSplitter.Split(queryExpression).Where(NotEmpty), StringComparer.OrdinalIgnoreCase);

            // Parse each target expression converting any encountered filter expressions, measurement keys and
            // Guid-based signal IDs into point tags; any other encountered inputs will throw an exception.
            foreach (string target in targetSet)
            {
                (TargetIDSet[] targetIDSets, string alias) = target.Parse<T>(metadata);

                if (!string.IsNullOrWhiteSpace(alias))
                {
                    // Aliased tags take precedence over other target maps
                    (string target, (MeasurementKey key, string pointTag)[] idSet) targetIDSet = targetIDSets[0];

                    foreach ((MeasurementKey key, string pointTag) in targetIDSet.idSet)
                        targetMap[key.ID] = $"{pointTag}/{alias} = {targetIDSet.target}";
                }
                else
                {
                    foreach ((string target, (MeasurementKey, string)[] idSet) targetIDSet in targetIDSets)
                    {
                        foreach ((MeasurementKey key, string pointTag) in targetIDSet.idSet)
                        {
                            if (targetMap.ContainsKey(key.ID))
                                continue;

                            // Target may represent multiple point tags based on data value type composition,
                            // so we track point tag and target as a combined value in the target map:
                            targetMap[key.ID] = $"{pointTag}/{targetIDSet.target}";
                        }
                    }
                }
            }

            return targetMap;
        });
    }

    private static async IAsyncEnumerable<DataSourceValueGroup<T>> ExecuteGrafanaFunctionAsync<T>(GrafanaDataSourceBase instance, ParsedGrafanaFunction<T> parsedFunction, QueryParameters queryParameters, DataSet metadata, [EnumeratorCancellation] CancellationToken cancellationToken) where T : struct, IDataSourceValue<T>
    {
        IGrafanaFunction<T> function = parsedFunction.Function;
        GroupOperations groupOperation = parsedFunction.GroupOperation;
        string queryExpression = parsedFunction.Expression;

        // Parse out function parameters and remaining query expression (typically a filter expression)
        (string[] parsedParameters, queryExpression) = function.ParseParameters(queryParameters, queryExpression, groupOperation);

        // Reenter query function with remaining query expression to get data - at the bottom of the recursion,
        // this will return time-series data queried from the derived class using 'QueryDataSourceValues':
        IAsyncEnumerable<DataSourceValueGroup<T>> dataset = QueryTargetAsync<T>(instance, queryParameters, metadata, queryExpression, cancellationToken);

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
            case <= GroupOperations.None:
                {
                    await foreach (DataSourceValueGroup<T> valueGroup in dataset.ConfigureAwait(false))
                    {
                        string rootTarget = valueGroup.RootTarget ?? valueGroup.Target;
                        MetadataMap metadataMap = metadata.GetMetadataMap<T>(rootTarget, queryParameters);
                        Parameters parameters = await function.GenerateParametersAsync(parsedParameters, valueGroup.Source, rootTarget, metadata, metadataMap, cancellationToken).ConfigureAwait(false);

                        yield return new DataSourceValueGroup<T>
                        {
                            Target = function.FormatTargetName(groupOperation, valueGroup.Target, parsedParameters),
                            RootTarget = rootTarget,
                            SourceTarget = queryParameters.SourceTarget,
                            Source = function.ComputeAsync(parameters, cancellationToken),
                            DropEmptySeries = queryParameters.DropEmptySeries,
                            RefID = queryParameters.SourceTarget.refID,
                            MetadataMap = metadataMap
                        };
                    }

                    break;
                }
            case GroupOperations.Slice:
                {
                    double tolerance = await ParseSliceToleranceAsync(function.Name, parsedParameters[0], queryParameters.SourceTarget.target, dataset, metadata, cancellationToken).ConfigureAwait(false);
                    TimeSliceScannerAsync<T> scanner = await TimeSliceScannerAsync<T>.Create(dataset, tolerance / SI.Milli, cancellationToken).ConfigureAwait(false);

                    async IAsyncEnumerable<T> computeSliceAsync()
                    {
                        string[] normalizedParameters = parsedParameters.Skip(1).ToArray();

                        while (!scanner.DataReadComplete)
                        {
                            IAsyncEnumerable<T> dataSourceValues = await scanner.ReadNextTimeSliceAsync().ConfigureAwait(false);
                            Dictionary<string, string> metadataMap = metadata.GetMetadataMap<T>(queryExpression, queryParameters);
                            Parameters parameters = await function.GenerateParametersAsync(normalizedParameters, dataSourceValues, null, metadata, metadataMap, cancellationToken).ConfigureAwait(false);

                            await foreach (T dataValue in function.ComputeSliceAsync(parameters, cancellationToken).ConfigureAwait(false))
                                yield return dataValue;
                        }
                    }

                    if (function.ReturnType == ReturnType.Scalar)
                    {
                        // If function yields a scalar value, then we return a single value group with the computed value
                        yield return new DataSourceValueGroup<T>
                        {
                            Target = function.FormatTargetName(groupOperation, queryExpression, parsedParameters),
                            RootTarget = queryExpression,
                            SourceTarget = queryParameters.SourceTarget,
                            Source = computeSliceAsync(),
                            DropEmptySeries = queryParameters.DropEmptySeries,
                            RefID = queryParameters.SourceTarget.refID,
                            MetadataMap = metadata.GetMetadataMap<T>(queryExpression, queryParameters)
                        };
                    }
                    else
                    {
                        // Otherwise, function returns a series, so we need to return value groups by target
                        async IAsyncEnumerable<DataSourceValueGroup<T>> computeSliceByTargetsAsync()
                        {
                            await foreach (IAsyncGrouping<string, T> valueGroup in computeSliceAsync().GroupBy(dataValue => dataValue.Target).WithCancellation(cancellationToken).ConfigureAwait(false))
                            {
                                yield return new DataSourceValueGroup<T>
                                {
                                    Target = valueGroup.Key,
                                    RootTarget = valueGroup.Key,
                                    Source = valueGroup
                                };
                            }
                        }

                        await foreach (DataSourceValueGroup<T> valueGroup in computeSliceByTargetsAsync().ConfigureAwait(false))
                        {
                            string rootTarget = valueGroup.RootTarget ?? valueGroup.Target;

                            yield return new DataSourceValueGroup<T>
                            {
                                Target = function.FormatTargetName(groupOperation, rootTarget, parsedParameters),
                                RootTarget = rootTarget,
                                SourceTarget = queryParameters.SourceTarget,
                                Source = valueGroup.Source,
                                DropEmptySeries = queryParameters.DropEmptySeries,
                                RefID = queryParameters.SourceTarget.refID,
                                MetadataMap = metadata.GetMetadataMap<T>(rootTarget, queryParameters)
                            };
                        }
                    }

                    break;
                }
            case GroupOperations.Set:
                {
                    // Flatten all series into a single enumerable
                    IAsyncEnumerable<T> dataSourceValues = dataset.SelectMany(source => source.Source);
                    MetadataMap metadataMap = metadata.GetMetadataMap<T>(queryExpression, queryParameters);
                    Parameters parameters = await function.GenerateParametersAsync(parsedParameters, dataSourceValues, null, metadata, metadataMap, cancellationToken).ConfigureAwait(false);

                    DataSourceValueGroup<T> valueGroup = new()
                    {
                        Target = function.FormatTargetName(groupOperation, queryExpression, parsedParameters),
                        RootTarget = queryExpression,
                        SourceTarget = queryParameters.SourceTarget,
                        Source = function.ComputeSetAsync(parameters, cancellationToken),
                        DropEmptySeries = queryParameters.DropEmptySeries,
                        RefID = queryParameters.SourceTarget.refID,
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

    private static (bool, bool, bool, string, string, string) ParseQueryCommands(string expression)
    {
        // Parse and cache any query commands found in target expression
        return TargetCache<(bool, bool, bool, string, string, string)>.GetOrAdd(expression, () =>
        {
            bool dropEmptySeries = false;
            bool includePeaks = false;
            bool fullResolutionQuery = false;
            string imports = string.Empty;
            string radialDistribution = string.Empty;

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

            commandMatch = s_radialDistributionCommand.Match(expression);

            if (commandMatch.Success)
            {
                string result = commandMatch.Result("${Expression}");
                radialDistribution = result.Trim();
                expression = expression.Replace(commandMatch.Value, "");
            }

            return (dropEmptySeries, includePeaks, fullResolutionQuery, imports, radialDistribution, expression);
        });
    }

    private static async ValueTask<double> ParseSliceToleranceAsync<T>(string functionName, string value, string target, IAsyncEnumerable<DataSourceValueGroup<T>> dataset, DataSet metadata, CancellationToken cancellationToken) where T : struct, IDataSourceValue<T>
    {
        if (double.TryParse(value, out double result))
            return result;

        try
        {
            // Try standard parameter parsing if simple double parse fails
            IMutableParameter<double> sliceToleranceParameter = Common.DefaultSliceTolerance.CreateParameter();
            IAsyncEnumerable<T> dataSourceValues = (await dataset.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false)).Source;

            await sliceToleranceParameter.ConvertParsedValueAsync(value, target, dataSourceValues, metadata, null, cancellationToken).ConfigureAwait(false);

            return sliceToleranceParameter.Value;
        }
        catch (Exception ex)
        {
            throw new SyntaxErrorException($"Invalid slice interval specified for {functionName} function. Exception: {ex.Message}", ex);
        }
    }

    // Query command regular expressions include a semi-colon prefix to help prevent possible name matches that may occur on other expressions
    private static readonly Regex s_dropEmptySeriesCommand = new(@";\s*DropEmptySeries", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex s_includePeaksCommand = new(@";\s*IncludePeaks", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex s_fullResolutionQueryCommand = new(@";\s*FullResolution(Query|Data)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex s_importsCommand = new(@";\s*Imports\s*=\s*\{(?<Expression>.+)\}", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex s_radialDistributionCommand = new(@";\s*RadialDistribution\s*=\s*\{(?<Expression>.+)\}", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    // Define a regular expression that splits on semi-colon except when semi-colon is inside of a single-quoted string. A simple
    // string.Split on semi-colon will not work properly for cases where a semi-colon is used inside of a filter expression that
    // contains a string literal with a semi-colon, e.g., FILTER ActiveMeasurements WHERE Description LIKE '%A;B%', see Expresso
    // 'Documentation/SemiColonSplitterRegex.xso' for development details on regex
    private static readonly Regex s_semiColonSplitter = new(@";(?=(?:[^']*'[^']*')*[^']*$)", RegexOptions.Compiled);

    // To ensure RegEx split ignores empty entries, define a predicate function that returns true if string is not empty
    private static bool NotEmpty(string value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }
}