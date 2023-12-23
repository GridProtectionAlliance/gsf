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
using GSF.TimeSeries;
using GSF.Web;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GrafanaAdapters.GrafanaFunctionsCore;
using static GrafanaAdapters.GrafanaFunctionsCore.FunctionParser;

namespace GrafanaAdapters;

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
    private const string FullResolutionQueryCommand = "fullresolutionquery";

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
    protected internal abstract IEnumerable<DataSourceValue> QueryDataSourceValues(DateTime startTime, DateTime stopTime, string interval, bool includePeaks, Dictionary<ulong, string> targetMap);

    /// <summary>
    /// Queries data source returning data as Grafana time-series data set.
    /// </summary>
    /// <param name="request">Query request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task<List<TimeSeriesValues>> Query(QueryRequest request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.format) && !request.format.Equals("json", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Only JSON formatted query requests are currently supported.");

        // TODO: JRC - suggest renaming 'request.isPhasor' parameter to 'request.dataType' and making it a string, then check value, e.g., "PhasorValue" or "DataSourceValue"

        // Task allows processing of multiple simultaneous queries
        return Task.Factory.StartNew(() => request.isPhasor ? 
                ProcessQuery<PhasorValue>(request, cancellationToken) : 
                ProcessQuery<DataSourceValue>(request, cancellationToken),
        cancellationToken);

        // FUTURE: Dynamic types could be supported by loading all found implementations of
        // 'IDataSourceValue' interface. In this case call to ProcessQuery<T> would be made
        // based on 'Type' of 'dataType', i.e., DataSourceValue.Default(string), by creating
        // a static delegate like 'Action<Type, QueryRequest, CancellationToken>' that would
        // call 'ProcessQuery<T>' with the appropriate type. Note that some functions used
        // in 'FunctionParser', e.g., 'GetPointTags', would need to be made public.
    }

    private List<TimeSeriesValues> ProcessQuery<T>(QueryRequest request, CancellationToken cancellationToken) where T : IDataSourceValue
    {
        DateTime startTime = request.range.from.ParseJsonTimestamp();
        DateTime stopTime = request.range.to.ParseJsonTimestamp();

        foreach (Target target in request.targets)
            target.target = target.target?.Trim() ?? "";

        List<DataSourceValueGroup<T>> valueGroups = new();

        foreach (Target target in request.targets)
        {
            bool dropEmptySeries = false;
            bool includePeaks = false;
            bool fullResolutionQuery = false;

            // Handle query commands
            if (target.target.ToLowerInvariant().Contains(DropEmptySeriesCommand))
            {
                dropEmptySeries = true;
                target.target = target.target.ReplaceCaseInsensitive(DropEmptySeriesCommand, "");
            }

            if (target.target.ToLowerInvariant().Contains(IncludePeaksCommand))
            {
                includePeaks = true;
                target.target = target.target.ReplaceCaseInsensitive(IncludePeaksCommand, "");
            }

            if (target.target.ToLowerInvariant().Contains(FullResolutionQueryCommand))
            {
                fullResolutionQuery = true;
                target.target = target.target.ReplaceCaseInsensitive(FullResolutionQueryCommand, "");
            }

            QueryParameters parameters = new()
            {
                SourceTarget = target,
                StartTime = startTime,
                StopTime = stopTime,
                Interval = fullResolutionQuery ? "0s" : request.interval,
                IncludePeaks = includePeaks,
                DropEmptySeries = dropEmptySeries,
                MetadataSelection = target.metadataSelection,
                CancellationToken = cancellationToken
            };

            // Parse target expressions
            ParsedTarget<T>[] targets = ParseTargets<T>(target.target);

            // Query data source returning data as a map of point tags to data
            Dictionary<string, DataSourceValueGroup<T>> pointData = QueryDataSourceGroupingByTarget(targets, parameters);

            // Execute series functions against queried data for each target
            IEnumerable<DataSourceValueGroup<T>> groups = ExecuteSeriesFunctions(targets, pointData, Metadata, cancellationToken);

            // Add each group to the overall list
            valueGroups.AddRange(groups);
        }

        // Establish result series sequentially so that order remains consistent between calls
        List<TimeSeriesValues> result = valueGroups.Select(valueGroup => new TimeSeriesValues
        {
            target = valueGroup.Target,
            rootTarget = valueGroup.RootTarget,
            meta = valueGroup.MetadataMap,
            dropEmptySeries = valueGroup.DropEmptySeries,
            refId = valueGroup.RefID
        }).ToList();

        // Apply any encountered ad-hoc filters
        if (request.adhocFilters?.Count > 0)
        {
            foreach (AdHocFilter filter in request.adhocFilters)
                result = result.Where(values => isFilterMatch(values.rootTarget, filter)).ToList();
        }

        // Process series data in parallel
        Parallel.ForEach(result, new ParallelOptions { CancellationToken = cancellationToken }, series =>
        {
            // For deferred enumerations, any work to be done is left till last moment - in this case "ToList()" invokes actual operation                    
            DataSourceValueGroup<T> valueGroup = valueGroups.First(group => group.Target.Equals(series.target));
            IEnumerable<T> values = valueGroup.Source;

            if (valueGroup.SourceTarget?.excludeNormalFlags ?? false)
                values = values.Where(value => value.Flags != MeasurementStateFlags.Normal);

            if (valueGroup.SourceTarget?.excludedFlags > uint.MinValue)
                values = values.Where(value => ((uint)value.Flags & valueGroup.SourceTarget.excludedFlags) == 0);

            series.datapoints = values.Select(dataValue => dataValue.TimeSeriesValue).ToList();
        });

        return result.Where(values => !values.dropEmptySeries || values.datapoints.Count > 0).ToList();

        bool isFilterMatch(string target, AdHocFilter filter)
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
        }

        DataRow lookupTargetMetadata(string target)
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
    }

    private Dictionary<string, DataSourceValueGroup<T>> QueryDataSourceGroupingByTarget<T>(ParsedTarget<T>[] parsedTargets, QueryParameters parameters) where T : IDataSourceValue
    {
        if (parsedTargets.Length == 0)
            return new Dictionary<string, DataSourceValueGroup<T>>();

        IDataSourceValue<T> instance = DataSourceValue.Default<T>();

        // Get ID to target map for all parsed targets used to query data source
        (Dictionary<ulong, string> targetMap, object state) = instance.GetIDTargetMap(Metadata, parsedTargets.GetDataTargets());

        // Query underlying data source for each target - to prevent parallel read from data source we enumerate immediately
        List<DataSourceValue> dataValues = QueryDataSourceValues(parameters.StartTime, parameters.StopTime, parameters.Interval, parameters.IncludePeaks, targetMap)
            .TakeWhile(_ => !parameters.CancellationToken.IsCancellationRequested).ToList();

        // Get map of targets to data source value groups
        return instance.GetTargetDataSourceValueMap(dataValues, Metadata, parameters, targetMap, state);
    }

    #endregion
}