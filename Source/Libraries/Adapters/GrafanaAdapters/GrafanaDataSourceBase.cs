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
using GrafanaAdapters.FunctionParsing;
using GrafanaAdapters.Model.Annotations;
using GrafanaAdapters.Model.Common;

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
    protected internal abstract IEnumerable<DataSourceValue> QueryDataSourceValues(QueryParameters queryParameters, Dictionary<ulong, string> targetMap, CancellationToken cancellationToken);

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
            ProcessQueryRequest<PhasorValue>(request, cancellationToken) : 
            ProcessQueryRequest<DataSourceValue>(request, cancellationToken);

        // FUTURE: Dynamic types could be supported by loading all found implementations of 'IDataSourceValue' interface.
    }

    private async Task<IEnumerable<TimeSeriesValues>> ProcessQueryRequest<T>(QueryRequest request, CancellationToken cancellationToken) where T : struct, IDataSourceValue<T>
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
            (bool dropEmptySeries, bool includePeaks, bool fullResolutionQuery, string imports, string updatedTarget) = 
                ParseQueryCommands(target.target);

            target.target = updatedTarget;

            targetQueryParameters.Add(new QueryParameters
            {
                SourceTarget = target,
                StartTime = startTime,
                StopTime = stopTime,
                Interval = fullResolutionQuery ? "0s" : request.interval,
                IncludePeaks = includePeaks,
                DropEmptySeries = dropEmptySeries,
                Imports = imports,
                MetadataSelection = target.metadataSelection,
            });
        }

        List<DataSourceValueGroup<T>> valueGroups = new();

        // Query each target -- each returned value group has a 'Source' value enumerable that may contain deferred
        // enumerations that need evaluation before the final result can be serialized and returned to Grafana
        foreach (QueryParameters queryParameters in targetQueryParameters)
            await foreach (DataSourceValueGroup<T> valueGroup in QueryTarget<T>(queryParameters, cancellationToken))
                valueGroups.Add(valueGroup);

        // Establish one result series for each value group so that consistent order can be maintained between calls
        TimeSeriesValues[] seriesResults = new TimeSeriesValues[valueGroups.Count];

        // Process value group data in parallel
        Parallel.ForEach(valueGroups, new ParallelOptions { CancellationToken = cancellationToken }, (valueGroup, _, index) =>
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

            IEnumerable<T> values = valueGroup.Source;

            // Apply any requested flag filters applied for the data source
            if (valueGroup.SourceTarget?.excludeNormalFlags ?? false)
                values = values.Where(value => value.Flags != MeasurementStateFlags.Normal);

            if (valueGroup.SourceTarget?.excludedFlags > uint.MinValue)
                values = values.Where(value => ((uint)value.Flags & valueGroup.SourceTarget.excludedFlags) == 0);

            // For deferred enumerations, function operations are executed here by ToList() at the last moment
            series.datapoints = values.Select(dataValue => dataValue.TimeSeriesValue).ToList();
        });

        // Drop any empty series, if requested
        IEnumerable<TimeSeriesValues> filteredResults = seriesResults.Where(values => !values.dropEmptySeries || values.datapoints.Count > 0);
        
        // Apply any encountered ad-hoc filters
        if (request.adhocFilters?.Count > 0)
            foreach (AdHocFilter filter in request.adhocFilters)
                filteredResults = filteredResults.Where(values => IsFilterMatch(values.rootTarget, filter));

        return filteredResults;
    }

    // This function is re-entrant so that it operates with functions as a depth-first recursive expression parser
    private async IAsyncEnumerable<DataSourceValueGroup<T>> QueryTarget<T>(QueryParameters queryParameters, [EnumeratorCancellation] CancellationToken cancellationToken) where T : struct, IDataSourceValue<T>
    {
        // A single target might look like the following - nested functions with multiple targets are supported:
        // PPA:15; STAT:20; SETSUM(COUNT(PPA:8; PPA:9; PPA:10)); FILTER ActiveMeasurements WHERE SignalType IN ('IPHA', 'VPHA'); RANGE(PPA:99; SUM(FILTER ActiveMeasurements WHERE SignalType = 'FREQ'; STAT:12))
        string queryExpression = queryParameters.SourceTarget.target;

        HashSet<string> targetSet = new(new[] { queryExpression }, StringComparer.OrdinalIgnoreCase); // Targets include user provided input, so casing is ignored

        // Parse out and cache any top-level grafana functions found in target expression
        (ParsedGrafanaFunction<T>[] grafanaFunctions, HashSet<string> reducedTargetSet) = TargetCache<(ParsedGrafanaFunction<T>[], HashSet<string>)>.GetOrAdd($"{queryExpression}-{typeof(T).FullName}", () =>
        {
            ParsedGrafanaFunction<T>[] matchedFunctions = FunctionParser.MatchFunctions<T>(queryExpression);
            HashSet<string> reducedTargetSet = new(StringComparer.OrdinalIgnoreCase);

            if (matchedFunctions.Length > 0)
            {
                // Reduce target to non-function expressions - important so later split on ';' succeeds properly
                string reducedTarget = queryExpression;

                foreach (string functionExpression in matchedFunctions.Select(function => function.MatchedValue))
                    reducedTarget = reducedTarget.Replace(functionExpression, "");

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
                // ExecuteGrafanaFunction calls QueryTarget in order to process sub-functions or acquire needed data at final depth
                await foreach (DataSourceValueGroup<T> valueGroup in ExecuteGrafanaFunction(parsedFunction, queryParameters, cancellationToken))
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

        // Query underlying data source for each target enumerating immediately to prevent multiple enumerations as each yielded
        // call to GetTargetDataSourceValueGroup filters out data source values for the specified target. Note that filter logic
        // can be custom for the data source type, so moving any filtering code here is not an option. Also, since this function
        // result is being enumerated immediately, any benefit to using an IAsyncEnumerable here is lost.
        List<DataSourceValue> dataValues = QueryDataSourceValues(queryParameters, targetMap, cancellationToken).ToList();

        // Expose each target as a data source value group
        foreach (KeyValuePair<ulong, string> target in targetMap)
            yield return default(T).GetTargetDataSourceValueGroup(target, dataValues, Metadata, queryParameters, state);
    }

    private async IAsyncEnumerable<DataSourceValueGroup<T>> ExecuteGrafanaFunction<T>(ParsedGrafanaFunction<T> parsedFunction, QueryParameters queryParameters, [EnumeratorCancellation] CancellationToken cancellationToken) where T : struct, IDataSourceValue<T>
    {
        IGrafanaFunction<T> seriesFunction = parsedFunction.Function;
        string expression = parsedFunction.Expression;
        GroupOperations groupOperation = parsedFunction.GroupOperation;

        // Parse out function parameters and target expression
        (string[] functionParameters, string updatedExpression) = TargetCache<(string[], string)>.GetOrAdd(expression, () =>
        {
            // Check if function defines any custom parameter parsing
            List<string> parsedParameters = seriesFunction.ParseParameters(ref expression);

            if (parsedParameters is null)
            {
                parsedParameters = new List<string>();

                // Extract any required function parameters
                int requiredParameters = seriesFunction.RequiredParameterCount;

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
                        throw new FormatException($"Expected {requiredParameters + 1} parameters, received {parsedParameters.Count + 1} in: {seriesFunction.Name}({expression})");
                }

                // Extract any provided optional function parameters
                int optionalParameters = seriesFunction.OptionalParameterCount;

                bool hasSubExpression(string target) => target.StartsWith("FILTER", StringComparison.OrdinalIgnoreCase) || target.Contains("(");

                if (optionalParameters > 0)
                {
                    int index = expression.IndexOf(',');

                    if (index > -1 && !hasSubExpression(expression.Substring(0, index)))
                    {
                        int lastIndex = index;

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

            }

            return (parsedParameters.ToArray(), expression);
        });

        // Query function expression to get series data
        IAsyncEnumerable<DataSourceValueGroup<T>> dataset = QueryTarget<T>(queryParameters, cancellationToken);

        switch (groupOperation)
        {
            case GroupOperations.Standard:
                // Standard group operations are applied to each series in the data set
                await foreach (DataSourceValueGroup<T> valueGroup in dataset)
                {
                    yield return new DataSourceValueGroup<T>
                    {
                        Target = $"{seriesFunction}({string.Join(", ", functionParameters)}{(functionParameters.Length > 0 ? ", " : "")}{valueGroup.Target})",
                        RootTarget = valueGroup.RootTarget ?? valueGroup.Target,
                        SourceTarget = queryParameters.SourceTarget,
                        Source = seriesFunction.Compute(null, cancellationToken),
                        DropEmptySeries = queryParameters.DropEmptySeries,
                        RefID = queryParameters.SourceTarget.refId
                    };
                }
                break;
            case GroupOperations.Slice:
                // Slice group operations are applied to the entire data set
                break;
            case GroupOperations.Set:
                // Set group operations are applied to the entire data set
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(groupOperation), $"Unsupported group operation encountered: {groupOperation}");
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