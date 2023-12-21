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

using GSF.TimeSeries;
using GSF.Web;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GrafanaAdapters.GrafanaFunctionsCore;


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
        bool isFilterMatch(string target, AdHocFilter filter)
        {
            // Default to positive match on failures
            return TargetCache<bool>.GetOrAdd($"filter!{filter.key}{filter.@operator}{filter.value}", () => {
                try
                {
                    DataRow metadata = LookupTargetMetadata(target);

                    if (metadata is null)
                        return true;

                    dynamic left = metadata[filter.key];
                    dynamic right = Convert.ChangeType(filter.value, metadata.Table.Columns[filter.key].DataType);

                    return filter.@operator switch
                    {
                        "="  => left == right,
                        "==" => left == right,
                        "!=" => left != right,
                        "<>" => left != right,
                        "<"  => left < right,
                        "<=" => left <= right,
                        ">"  => left > right,
                        ">=" => left >= right,
                        _    => true
                    };
                }
                catch
                {
                    return true;
                }
            });
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

                List<TimeSeriesValues> handleQuery<T>()
                {
                    List<DataSourceValueGroup<T>> allGroups = new();

                    foreach (Target target in request.targets)
                    {
                        QueryDataHolder queryData = new()
                        {
                            SourceTarget = target,
                            StartTime = startTime,
                            StopTime = stopTime,
                            Interval = request.interval,
                            IncludePeaks = false,
                            DropEmptySeries = false,
                            IsPhasor = request.isPhasor,
                            MetadataSelection = target.metadataSelection,
                            CancellationToken = cancellationToken
                        };
                        DataSourceValueGroup<T>[] groups = FunctionParser.Parse<T>(target.target, this, queryData);
                        allGroups.AddRange(groups);  // adding each group to the overall list
                    }

                    DataSourceValueGroup<T>[] valueGroups = allGroups.ToArray();

                   
                    // Establish result series sequentially so that order remains consistent between calls
                    List<TimeSeriesValues> result = valueGroups.Select(valueGroup => new TimeSeriesValues
                    {
                        target = valueGroup.Target,
                        rootTarget = valueGroup.RootTarget,
                        meta = valueGroup.metadata,
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
                    Parallel.ForEach(result, new ParallelOptions { CancellationToken = cancellationToken }, series =>
                    {
                        // For deferred enumerations, any work to be done is left till last moment - in this case "ToList()" invokes actual operation                    
                        DataSourceValueGroup<T> valueGroup = valueGroups.First(group => group.Target.Equals(series.target));
                        if (typeof(T) == typeof(DataSourceValue))
                        {
                            IEnumerable<DataSourceValue> values = valueGroup.Source.Cast<DataSourceValue>();

                            if (valueGroup.SourceTarget?.excludeNormalFlags ?? false)
                                values = values.Where(value => value.Flags != MeasurementStateFlags.Normal);

                            if (valueGroup.SourceTarget?.excludedFlags > uint.MinValue)
                                values = values.Where(value => ((uint)value.Flags & valueGroup.SourceTarget.excludedFlags) == 0);

                            series.datapoints = values.Select(dataValue => new[] { dataValue.Value, dataValue.Time }).ToList();
                        }

                        else if (typeof(T) == typeof(PhasorValue))
                        {
                            IEnumerable<PhasorValue> values = valueGroup.Source.Cast<PhasorValue>();

                            if (valueGroup.SourceTarget?.excludeNormalFlags ?? false)
                                values = values.Where(value => value.Flags != MeasurementStateFlags.Normal);

                            if (valueGroup.SourceTarget?.excludedFlags > uint.MinValue)
                                values = values.Where(value => ((uint)value.Flags & valueGroup.SourceTarget.excludedFlags) == 0);

                            series.datapoints = values.Select(dataValue => new[] { dataValue.Magnitude, dataValue.Angle, dataValue.Time }).ToList();
                        }
                    });

                    return result.Where(values => !values.dropEmptySeries || values.datapoints.Count > 0).ToList();
                }

                return (request is not null && request.isPhasor) ? 
                    handleQuery<PhasorValue>() :
                    handleQuery<DataSourceValue>();
            },
            cancellationToken);
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