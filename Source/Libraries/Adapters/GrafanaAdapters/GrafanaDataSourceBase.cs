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
using GSF.TimeSeries.Adapters;
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

    // Nested Types
    private struct PhasorInfo
    {
        public string Label;
        public string Magnitude;
        public string Phase;
    };

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
        // Task allows processing of multiple simultaneous queries
        return Task.Factory.StartNew(() =>
        {
            if (!string.IsNullOrWhiteSpace(request.format) && !request.format.Equals("json", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Only JSON formatted query requests are currently supported.");

            DateTime startTime = request.range.from.ParseJsonTimestamp();
            DateTime stopTime = request.range.to.ParseJsonTimestamp();

            foreach (Target target in request.targets)
                target.target = target.target?.Trim() ?? "";

            List<TimeSeriesValues> processQuery<T>() where T : IDataSourceValue
            {
                List<DataSourceValueGroup<T>> valueGroups = new();

                foreach (Target target in request.targets)
                {
                    bool dropEmptySeries = false;
                    bool includePeaks = false;

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

                    QueryParameters parameters = new()
                    {
                        SourceTarget = target,
                        StartTime = startTime,
                        StopTime = stopTime,
                        Interval = request.interval,
                        IncludePeaks = includePeaks,
                        DropEmptySeries = dropEmptySeries,
                        IsPhasor = request.isPhasor,
                        MetadataSelection = target.metadataSelection,
                        CancellationToken = cancellationToken
                    };

                    // Parse target expressions
                    ParsedTarget<T>[] targets = ParseTargets<T>(target.target);

                    // Query data source with returned as data as a dictionary of point tags and data
                    Dictionary<string, DataSourceValueGroup<T>> pointData = request.isPhasor ?
                        GetTargetPhasorData(targets.GetDataTargets(), parameters) as Dictionary<string, DataSourceValueGroup<T>> :
                        GetTargetValueData(targets.GetDataTargets(), parameters) as Dictionary<string, DataSourceValueGroup<T>>;

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
                    IEnumerable<T> values = valueGroup.Source;

                    if (valueGroup.SourceTarget?.excludeNormalFlags ?? false)
                        values = values.Where(value => value.Flags != MeasurementStateFlags.Normal);

                    if (valueGroup.SourceTarget?.excludedFlags > uint.MinValue)
                        values = values.Where(value => ((uint)value.Flags & valueGroup.SourceTarget.excludedFlags) == 0);

                    series.datapoints = values.Select(dataValue => dataValue.TimeSeriesValue).ToList();
                });

                return result.Where(values => !values.dropEmptySeries || values.datapoints.Count > 0).ToList();
            }

            // TODO: JRC - suggest renaming 'isPhasor' parameter to 'dataType' and making it a string,
            // then check value, e.g., "PhasorValue" or "DataSourceValue", and query the following
            // from an updated IDataSourceValue interface function - can lookup type based on the
            // value of 'dataType' and then call the following generic processQuery with type:

            return request.isPhasor ? 
                processQuery<PhasorValue>() :
                processQuery<DataSourceValue>();
        },
        cancellationToken);

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

    private Dictionary<string, DataSourceValueGroup<DataSourceValue>> GetTargetValueData(string[] targets, QueryParameters queryData)
    {
        if (targets.Length == 0)
            return new Dictionary<string, DataSourceValueGroup<DataSourceValue>>();

        Dictionary<ulong, string> targetMap = new();

        // Expand target set to include point tags for all parsed inputs
        HashSet<string> targetSet = new();

        foreach (string target in targets)
            targetSet.UnionWith(TargetCache<string[]>.GetOrAdd(target, () => GetPointTags(target, Metadata)));

        // Reduce all targets down to a dictionary of point ID's mapped to point tags
        foreach (string target in targetSet)
        {
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

                        return new Tuple<MeasurementKey, string>(parsedKey, parsedKey.TagFromKey(Metadata));
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
        List<DataSourceValue> dataValues = QueryDataSourceValues(queryData.StartTime, queryData.StopTime, queryData.Interval, queryData.IncludePeaks, targetMap)
            .TakeWhile(_ => !queryData.CancellationToken.IsCancellationRequested).ToList();

        return targetMap.ToDictionary(kvp => kvp.Value, kvp => {
            IEnumerable<DataSourceValue> filteredValues = dataValues.Where(dataValue => dataValue.Target.Equals(kvp.Value));

            return new DataSourceValueGroup<DataSourceValue>
            {
                Target = kvp.Value,
                RootTarget = kvp.Value,
                SourceTarget = queryData.SourceTarget,
                Source = filteredValues,
                DropEmptySeries = queryData.DropEmptySeries,
                refId = queryData.SourceTarget.refId,
                metadata = GetMetadata(kvp.Value, queryData.MetadataSelection)
            };
        });
    }

    private Dictionary<string, DataSourceValueGroup<PhasorValue>> GetTargetPhasorData(string[] targets, QueryParameters queryData)
    {
        if (targets.Length == 0)
            return new Dictionary<string, DataSourceValueGroup<PhasorValue>>();

        Dictionary<int, PhasorInfo> phasorTargets = new();
        Dictionary<ulong, string> targetMap = new();

        foreach (string targetLabel in targets)
        {
            if (targetLabel.StartsWith("FILTER ", StringComparison.OrdinalIgnoreCase) && AdapterBase.ParseFilterExpression(targetLabel.SplitAlias(out _), out string tableName, out string exp, out string sortField, out int takeCount))
            {
                foreach (DataRow row in Metadata.Tables[tableName].Select(exp, sortField).Take(takeCount))
                {
                    int targetId = Convert.ToInt32(row["ID"]);

                    phasorTargets[Convert.ToInt32(targetId)] = new PhasorInfo
                    {
                        Label = row["Label"].ToString(),
                        Phase = "",
                        Magnitude = ""
                    };
                }

                continue;
            }

            // Get phasor id
            DataRow[] phasorRows = Metadata.Tables["Phasor"].Select($"Label = '{targetLabel}'");

            if (phasorRows.Length == 0)
                throw new Exception($"Unable to find label {targetLabel}");

            foreach (DataRow row in phasorRows)
            {
                int targetId = Convert.ToInt32(phasorRows[0]["ID"]);

                phasorTargets[targetId] = new PhasorInfo
                {
                    Label = row["Label"].ToString(),
                    Phase = "",
                    Magnitude = ""
                };

                DataRow[] measurementRows = Metadata.Tables["ActiveMeasurements"].Select($"PhasorID = '{targetId}'");

                if (measurementRows.Length < 2)
                    throw new Exception($"Did not locate both magnitude and phase for {phasorTargets[targetId].Label} with {targetId} id");

                foreach (DataRow pointRow in measurementRows)
                {
                    ulong id = Convert.ToUInt64(pointRow["ID"].ToString().Split(':')[1]);
                    string pointTag = pointRow["PointTag"].ToString();

                    targetMap[id] = pointTag;

                    if (pointRow["SignalType"].ToString().EndsWith("PH"))
                    {
                        phasorTargets[targetId] = new PhasorInfo
                        {
                            Label = phasorTargets[targetId].Label,
                            Phase = pointTag,
                            Magnitude = phasorTargets[targetId].Magnitude,
                        };
                    }

                    if (pointRow["SignalType"].ToString().EndsWith("PM"))
                    {
                        phasorTargets[targetId] = new PhasorInfo
                        {
                            Label = phasorTargets[targetId].Label,
                            Magnitude = pointTag,
                            Phase = phasorTargets[targetId].Phase,
                        };
                    }
                }
            }
        }

        // Query underlying data source for each target - to prevent parallel read from data source we enumerate immediately
        List<DataSourceValue> dataValues = QueryDataSourceValues(queryData.StartTime, queryData.StopTime, queryData.Interval, queryData.IncludePeaks, targetMap)
            .TakeWhile(_ => !queryData.CancellationToken.IsCancellationRequested).ToList();

        return phasorTargets.ToDictionary(target => target.Value.Label, target =>
        {
            IEnumerable<DataSourceValue> filteredMagnitudes = dataValues.Where(dataValue => dataValue.Target.Equals(target.Value.Magnitude));
            List<DataSourceValue> filteredPhases = dataValues.Where(dataValue => dataValue.Target.Equals(target.Value.Phase)).ToList();
            IEnumerable<PhasorValue> phasorValues = GeneratePhasorValues();

            List<PhasorValue> GeneratePhasorValues()
            {
                List<PhasorValue> values = new();
                int index = 0;

                foreach (DataSourceValue mag in filteredMagnitudes)
                {
                    if (index >= filteredPhases.Count)
                        continue;

                    PhasorValue phasor = new()
                    {
                        MagnitudeTarget = target.Value.Magnitude,
                        AngleTarget = target.Value.Phase,
                        Flags = mag.Flags,
                        Time = mag.Time,
                        Magnitude = mag.Value,
                        Angle = filteredPhases[index].Value
                    };

                    index++;

                    values.Add(phasor);
                }
                return values;
            }

            return new DataSourceValueGroup<PhasorValue>
            {
                Target = $"{target.Value.Magnitude};{target.Value.Phase}",
                RootTarget = target.Value.Label,
                SourceTarget = queryData.SourceTarget,
                Source = phasorValues,
                DropEmptySeries = queryData.DropEmptySeries,
                refId = queryData.SourceTarget.refId,
                metadata = GetMetadata(target.Value.Label, queryData.MetadataSelection, true)
            };
        });
    }

    private Dictionary<string, string> GetMetadata(string rootTarget, Dictionary<string, List<string>> metadataSelection, bool isPhasor = false)
    {
        // Create a new dictionary to hold the metadata values
        Dictionary<string, string> metadataDict = new();

        // Return an empty dictionary if metadataSelection is null or empty
        if (metadataSelection is null || metadataSelection.Count == 0)
        {
            return metadataDict;
        }

        // Iterate through selections
        foreach (KeyValuePair<string, List<string>> entry in metadataSelection)
        {
            string table = entry.Key;
            List<string> values = entry.Value;
            string selectQuery = isPhasor ? $"Label = '{rootTarget}'" : $"PointTag = '{rootTarget}'";
            DataRow[] rows = Metadata.Tables[table].Select(selectQuery);

            // Populate the entry dictionary with the metadata values
            foreach (string value in values)
            {
                string metadataValue = string.Empty;

                if (rows.Length > 0)
                    metadataValue = rows[0][value].ToString();

                metadataDict[value] = metadataValue;
            }
        }

        return metadataDict;
    }

    #endregion
}