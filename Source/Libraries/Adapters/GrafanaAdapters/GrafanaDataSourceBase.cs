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
using GSF.Data.Model;
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

// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable AccessToModifiedClosure
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
        /// <param name="decimate">Flag that determines if data should be decimated over provided time range.</param>
        /// <param name="targetMap">Set of IDs with associated targets to query.</param>
        /// <returns>Queried data source data in terms of value and time.</returns>
        protected abstract IEnumerable<DataSourceValue> QueryDataSourceValues(DateTime startTime, DateTime stopTime, string interval, bool decimate, Dictionary<ulong, string> targetMap);

        /// <summary>
        /// Search data source meta-data for a target.
        /// </summary>
        /// <param name="request">Search target.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public virtual Task<string[]> Search(Target request, CancellationToken cancellationToken)
        {
            string target = request.target == "select metric" ? "" : request.target;

            return Task.Factory.StartNew(() =>
            {
                return TargetCache<string[]>.GetOrAdd($"search!{target}", () =>
                {
                    if (!(request.target is null))
                    {
                        // Attempt to parse search target as a SQL SELECT statement that will operate as a filter for in memory metadata (not a database query)
                        if (ParseSelectExpression(request.target.Trim(), out string tableName, out string[] fieldNames, out string expression, out string sortField, out int takeCount))
                        {
                            DataTableCollection tables = Metadata.Tables;
                            List<string> results = new List<string>();

                            if (tables.Contains(tableName))
                            {
                                DataTable table = tables[tableName];
                                List<string> validFieldNames = new List<string>();

                                for (int i = 0; i < fieldNames?.Length; i++)
                                {
                                    string fieldName = fieldNames[i].Trim();

                                    if (table.Columns.Contains(fieldName))
                                        validFieldNames.Add(fieldName);
                                }

                                fieldNames = validFieldNames.ToArray();

                                if (fieldNames.Length == 0)
                                    fieldNames = table.Columns.Cast<DataColumn>().Select(column => column.ColumnName).ToArray();

                                // If no filter expression or take count was specified, limit search target results - user can
                                // still request larger results sets by specifying desired TOP count.
                                if (takeCount == int.MaxValue && string.IsNullOrWhiteSpace(expression))
                                    takeCount = MaximumSearchTargetsPerRequest;

                                void executeSelect(IEnumerable<DataRow> queryOperation)
                                {
                                    results.AddRange(queryOperation.Take(takeCount).Select(row => string.Join(",", fieldNames.Select(fieldName => row[fieldName].ToString()))));
                                }

                                if (string.IsNullOrWhiteSpace(expression))
                                {
                                    if (string.IsNullOrWhiteSpace(sortField))
                                    {
                                        executeSelect(table.Select());
                                    }
                                    else
                                    {
                                        if (Common.IsNumericType(table.Columns[sortField].DataType))
                                        {
                                            decimal parseAsNumeric(DataRow row)
                                            {
                                                decimal.TryParse(row[sortField].ToString(), out decimal result);
                                                return result;
                                            }

                                            executeSelect(table.Select().OrderBy(parseAsNumeric));
                                        }
                                        else
                                        {
                                            executeSelect(table.Select().OrderBy(row => row[sortField].ToString()));
                                        }
                                    }
                                }
                                else
                                {
                                    executeSelect(table.Select(expression, sortField));
                                }

                                foreach (DataRow row in table.Select(expression, sortField).Take(takeCount))
                                    results.Add(string.Join(",", fieldNames.Select(fieldName => row[fieldName].ToString())));
                            }

                            return results.ToArray();
                        }
                    }

                    // Non "SELECT" style expressions default to searches on ActiveMeasurements meta-data table
                    return Metadata.Tables["ActiveMeasurements"].Select($"ID LIKE '{InstanceName}:%' AND PointTag LIKE '%{target}%'").Take(MaximumSearchTargetsPerRequest).Select(row => $"{row["PointTag"]}").ToArray();
                });
            }, cancellationToken);
        }

        /// <summary>
        /// Search data source meta-data for a list of columns from a specific table.
        /// </summary>
        /// <param name="request">Table Name.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public virtual Task<string[]> SearchFields(Target request, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(() =>
            {
                return TargetCache<string[]>.GetOrAdd($"search!fields!{request.target}", () => Metadata.Tables[request.target].Columns.Cast<DataColumn>().Select(column => column.ColumnName).ToArray());
            }, cancellationToken);
        }

        /// <summary>
        /// Search data source meta-data for a list of tables.
        /// </summary>
        /// <param name="request">Request - ignored.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public virtual Task<string[]> SearchFilters(Target request, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(() =>
            {
                // Any table that includes columns for ID, SignalID, PointTag, Adder and Multiplier can be used as measurement sources for filter expressions
                return TargetCache<string[]>.GetOrAdd("search!filters!{63F7E9F6B334}", () => Metadata.Tables.Cast<DataTable>().Where(table => new[] { "ID", "SignalID", "PointTag", "Adder", "Multiplier" }.All(fieldName => table.Columns.Contains(fieldName))).Select(table => table.TableName).ToArray());
            }, cancellationToken);
        }

        /// <summary>
        /// Search data source meta-data for a list of columns from a specific table to use for ORDER BY expression.
        /// </summary>
        /// <param name="request">Table Name.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public virtual Task<string[]> SearchOrderBys(Target request, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(() =>
            {
                // Result will typically be the same list as SearchFields but allows ability to deviate in case certain fields are not suitable for ORDER BY expression
                return TargetCache<string[]>.GetOrAdd($"search!orderbys!{request.target}", () => Metadata.Tables[request.target].Columns.Cast<DataColumn>().Select(column => column.ColumnName).ToArray());
            }, cancellationToken);
        }

        /// <summary>
        /// Queries data source for annotations in a time-range (e.g., Alarms).
        /// </summary>
        /// <param name="request">Annotation request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Queried annotations from data source.</returns>
        public virtual async Task<List<AnnotationResponse>> Annotations(AnnotationRequest request, CancellationToken cancellationToken)
        {
            AnnotationType type = request.ParseQueryType(out bool useFilterExpression);
            Dictionary<string, DataRow> definitions = request.ParseSourceDefinitions(type, Metadata, useFilterExpression);
            List<TimeSeriesValues> annotationData = await Query(request.ExtractQueryRequest(definitions.Keys, MaximumAnnotationsPerRequest), cancellationToken);
            List<AnnotationResponse> responses = new List<AnnotationResponse>();

            foreach (TimeSeriesValues values in annotationData)
            {
                string[] parts = values.target.Split(',');
                string target;

                // Remove "Interval(0, {target})" from target if defined
                if (parts.Length > 1)
                {
                    target = parts[1].Trim();
                    target = target.Length > 1 ? target.Substring(0, target.Length - 1).Trim() : parts[0].Trim();
                }
                else
                {
                    target = parts[0].Trim();
                }

                if (definitions.TryGetValue(target, out DataRow definition))
                {
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
            }

            return responses;
        }

        /// <summary>
        /// Queries current alarm device state.
        /// </summary>
        /// <param name="request">Alarm request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns> Queried device alarm states.</returns>
        public Task<IEnumerable<AlarmDeviceStateView>> GetAlarmState(QueryRequest request, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(() =>
            {
                using (AdoDataConnection connection = new AdoDataConnection("systemSettings"))
                {
                    return new TableOperations<AlarmDeviceStateView>(connection).QueryRecords("Name");
                }
            },
            cancellationToken);
        }

        /// <summary>
        /// Queries All Available Device Alarm states.
        /// </summary>
        /// <param name="request">Alarm request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns> Queried device alarm states.</returns>
        public Task<IEnumerable<AlarmState>> GetDeviceAlarms(QueryRequest request, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(() =>
            {
                using (AdoDataConnection connection = new AdoDataConnection("systemSettings"))
                {
                    return new TableOperations<AlarmState>(connection).QueryRecords("ID");
                }
            },
            cancellationToken);
        }

        /// <summary>
        /// Queries All Available Device Groups.
        /// </summary>
        /// <param name="request">request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns> List of Device Groups.</returns>
        public Task<IEnumerable<DeviceGroup>> GetDeviceGroups(QueryRequest request, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(() =>
            {
                using (AdoDataConnection connection = new AdoDataConnection("systemSettings"))
                {
                    IEnumerable<GSF.TimeSeries.Model.Device> groups = (new TableOperations<GSF.TimeSeries.Model.Device>(connection)).QueryRecordsWhere("AccessID = -99999");

                    return groups.Select(item => new DeviceGroup
                    {
                        ID = item.ID,
                        Name = item.Name,
                        Devices = ProcessDeviceGroup(item.ConnectionString).ToList()

                    });

                }
            },
            cancellationToken);
        }

        private IEnumerable<int> ProcessDeviceGroup(string connectionString)
        {
            // Parse the connection string into a dictionary of key-value pairs for easy lookups
            Dictionary<string, string> settings = connectionString.ParseKeyValuePairs();

            if (!settings.ContainsKey("DeviceIDs"))
                return new List<int>();

            return settings["DeviceIDs"].Split(',').Select(item => ParseInt(item));
        }

        /// <summary>
        /// Query Current Alarms Based on list of Points.
        /// </summary>
        /// <param name="request">Alarm request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns> Queried alarm states.</returns>
        public Task<List<GrafanaAlarm>> GetAlarms(QueryRequest request, CancellationToken cancellationToken)
        {
            HashSet<string> removeSeriesFunctions(string queryExpression)
            {
                HashSet<string> targetSet = new HashSet<string>(new[] { queryExpression }, StringComparer.OrdinalIgnoreCase); // Targets include user provided input, so casing should be ignored
                HashSet<string> reducedTargetSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (string target in targetSet)
                {
                    // Find any series functions in target
                    Match[] matchedFunctions = TargetCache<Match[]>.GetOrAdd(target, () =>
                        s_seriesFunctions.Matches(target).Cast<Match>().ToArray());

                    if (matchedFunctions.Length > 0)
                    {
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

                return reducedTargetSet;
            }

            return Task.Factory.StartNew(() =>
            {
                const string SignalIDQuery = "SELECT TOP 1 SignalID FROM ActiveMeasurement WHERE PointTag = '{0}'";

                foreach (Target target in request.targets)
                    target.target = target.target?.Trim() ?? "";

                if (request.targets.All(item => string.IsNullOrWhiteSpace(item.target)))
                    return new List<GrafanaAlarm>();

               
                List<string> pointTags = removeSeriesFunctions(request.targets[0].target).SelectMany(item => item.Split(';'))
                    .SelectMany(targetQuery => AdapterBase.ParseInputMeasurementKeys(Metadata, false, targetQuery))
                    .Select(item => item.Metadata.TagName).ToList();
               
                string query = string.Join("),(", pointTags.Select(item => string.Format(SignalIDQuery, item.Trim())));

                query = "((" + query + "))";

                using (AdoDataConnection connection = new AdoDataConnection("systemSettings"))
                {
                    query = $"SignalID in {query}";
                    return new TableOperations<GrafanaAlarm>(connection).QueryRecordsWhere(query).ToList();
                }
            },
            cancellationToken);
        }

        /// <summary>
        /// Requests available tag keys.
        /// </summary>
        /// <param name="_">Tag keys request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public Task<TagKeysResponse[]> TagKeys(TagKeysRequest _, CancellationToken cancellationToken)
        {
            string getType(Type type) => 
                type == typeof(bool) ? "boolean" : type.IsNumeric() ? "number" : "string";

            return Task.Factory.StartNew(() =>
            {
                return TargetCache<TagKeysResponse[]>.GetOrAdd("tagkeys", () => 
                    Metadata.Tables["ActiveMeasurements"].Columns.Cast<DataColumn>().Select(column => 
                        new TagKeysResponse
                        { 
                            type = getType(column.DataType),
                            text = column.ColumnName
                        }).ToArray());
            },
            cancellationToken);
        }

        /// <summary>
        /// Requests available tag values.
        /// </summary>
        /// <param name="request">Tag values request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public Task<TagValuesResponse[]> TagValues(TagValuesRequest request, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(() =>
            {
                return TargetCache<TagValuesResponse[]>.GetOrAdd($"tagvalues!{request.key}", () =>
                {
                    DataTable table = Metadata.Tables["ActiveMeasurements"];
                    int columnIndex = table.Columns[request.key].Ordinal;
                    return table.AsEnumerable().Select(row =>
                        new TagValuesResponse
                        {
                            text = row[columnIndex].ToString()
                        }).ToArray();
                });
            },
            cancellationToken);
        }

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

                DataSourceValueGroup[] valueGroups = request.targets.Select(target => QueryTarget(target, target.target, startTime, stopTime, request.interval, true, false, cancellationToken)).SelectMany(groups => groups).ToArray();

                // Establish result series sequentially so that order remains consistent between calls
                List<TimeSeriesValues> result = valueGroups.Select(valueGroup => new TimeSeriesValues
                {
                    target = valueGroup.Target,
                    rootTarget = valueGroup.RootTarget,
                    latitude = lookupTargetCoordinate(valueGroup.RootTarget, "Latitude"),
                    longitude = lookupTargetCoordinate(valueGroup.RootTarget, "Longitude"),
                    dropEmptySeries = valueGroup.DropEmptySeries
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

        private IEnumerable<DataSourceValueGroup> QueryTarget(Target sourceTarget, string queryExpression, DateTime startTime, DateTime stopTime, string interval, bool decimate, bool dropEmptySeries, CancellationToken cancellationToken)
        {
            if (queryExpression.ToLowerInvariant().Contains(DropEmptySeriesCommand))
            {
                dropEmptySeries = true;
                queryExpression = queryExpression.ReplaceCaseInsensitive(DropEmptySeriesCommand, "");
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
                foreach (Tuple<SeriesFunction, string, GroupOperation> parsedFunction in seriesFunctions.Select(ParseSeriesFunction))
                    foreach (DataSourceValueGroup valueGroup in ExecuteSeriesFunction(sourceTarget, parsedFunction, startTime, stopTime, interval, decimate, dropEmptySeries, cancellationToken))
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
                List<DataSourceValue> dataValues = QueryDataSourceValues(startTime, stopTime, interval, decimate, targetMap)
                    .TakeWhile(_ => !cancellationToken.IsCancellationRequested).ToList();

                foreach (KeyValuePair<ulong, string> target in targetMap)
                    yield return new DataSourceValueGroup
                    {
                        Target = target.Value,
                        RootTarget = target.Value,
                        SourceTarget = sourceTarget,
                        Source = dataValues.Where(dataValue => dataValue.Target.Equals(target.Value)),
                        DropEmptySeries = dropEmptySeries
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