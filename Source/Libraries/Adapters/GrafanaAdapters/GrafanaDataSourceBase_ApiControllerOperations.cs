//******************************************************************************************************
//  GrafanaDataSourceBase_ApiControllerOperations.cs - Gbtc
//
//  Copyright © 2024, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  01/14/2024 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using GrafanaAdapters.DataSources;
using GrafanaAdapters.DataSources.BuiltIn;
using GrafanaAdapters.Functions;
using GrafanaAdapters.Metadata;
using GrafanaAdapters.Model.Annotations;
using GrafanaAdapters.Model.Common;
using GrafanaAdapters.Model.Database;
using GrafanaAdapters.Model.Functions;
using GrafanaAdapters.Model.Metadata;
using GSF;
using GSF.Data;
using GSF.Data.Model;
using GSF.TimeSeries.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static GrafanaAdapters.Functions.Common;

namespace GrafanaAdapters;

// ApiController specific Grafana functionality is defined here
partial class GrafanaDataSourceBase
{
    /// <summary>
    /// Gets the data source value types, i.e., any type that has implemented <see cref="IDataSourceValue"/>,
    /// that have been loaded into the application domain.
    /// </summary>
    public virtual IEnumerable<DataSourceValueType> GetValueTypes()
    {
        return DataSourceValueCache.DefaultInstances.Select((value, index) => new DataSourceValueType
        {
            name = value.GetType().Name,
            index = index,
            timeSeriesDefinition = string.Join(", ", value.TimeSeriesValueDefinition),
            metadataTableName = value.MetadataTableName
        });
    }

    /// <summary>
    /// Gets the table names that, at a minimum, contain all the fields that the value type has defined as
    /// required, see <see cref="IDataSourceValue.RequiredMetadataFieldNames"/>.
    /// </summary>
    /// <param name="request">Search request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public virtual Task<IEnumerable<string>> GetValueTypeTables(SearchRequest request, CancellationToken cancellationToken)
    {
        return Task.Run(() =>
        {
            return TargetCache<IEnumerable<string>>.GetOrAdd($"{request.dataTypeIndex}", () =>
            {
                IDataSourceValue dataSourceValue = DataSourceValueCache.GetDefaultInstance(request.dataTypeIndex);
                DataSet metadata = Metadata.GetAugmentedDataSet(dataSourceValue);
                return metadata.Tables.Cast<DataTable>().Where(table => dataSourceValue.MetadataTableIsValid(metadata, table.TableName)).Select(table => table.TableName);
            });
        },
        cancellationToken);
    }

    /// <summary>
    /// Gets the field names for a given table.
    /// </summary>
    /// <param name="request">Search request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public virtual Task<IEnumerable<FieldDescription>> GetValueTypeTableFields(SearchRequest request, CancellationToken cancellationToken)
    {
        return Task.Run(() =>
        {
            return TargetCache<IEnumerable<FieldDescription>>.GetOrAdd($"{request.dataTypeIndex}:{request.expression}", () =>
            {
                IDataSourceValue dataSourceValue = DataSourceValueCache.GetDefaultInstance(request.dataTypeIndex);
                DataSet metadata = Metadata.GetAugmentedDataSet(dataSourceValue);
                string tableName = request.expression.Trim();

                if (!dataSourceValue.MetadataTableIsValid(metadata, tableName))
                    return Enumerable.Empty<FieldDescription>();

                return metadata.Tables[tableName].Columns.Cast<DataColumn>().Select(column => new FieldDescription
                {
                    name = column.ColumnName,
                    type = column.DataType.GetReflectedTypeName(false),
                    required = dataSourceValue.RequiredMetadataFieldNames.Contains(column.ColumnName, StringComparer.OrdinalIgnoreCase)
                });
            });
        },
        cancellationToken);
    }

    /// <summary>
    /// Gets the functions that are available for a given data source value type.
    /// </summary>
    /// <param name="request">Search request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public virtual Task<IEnumerable<FunctionDescription>> GetValueTypeFunctions(SearchRequest request, CancellationToken cancellationToken)
    {
        return Task.Run(() =>
        {
            return TargetCache<IEnumerable<FunctionDescription>>.GetOrAdd( $"{request.dataTypeIndex}:{request.expression}", () =>
            {
                // Parse out any requested group operation filter from search expression
                GroupOperations filteredGroupOperations = Enum.TryParse(request.expression.Trim(), out GroupOperations groupOperations) ?
                    groupOperations :
                    DefaultGroupOperations;

                // Assume no group operation is desired if filtered operations are undefined
                if (filteredGroupOperations == GroupOperations.Undefined)
                    filteredGroupOperations = GroupOperations.None;

                return FunctionParsing.GetGrafanaFunctions(request.dataTypeIndex).SelectMany(function =>
                {
                    GroupOperations publishedGroupOperations = function.PublishedGroupOperations;
                    GroupOperations allowedGroupOperations = function.AllowedGroupOperations;
                    GroupOperations requestedGroupOperations = filteredGroupOperations; // Value copied as it can be modified
                    GroupOperations? groupOperationNameOverride = null;

                    if (publishedGroupOperations == GroupOperations.Undefined)
                        publishedGroupOperations = GroupOperations.None;

                    if (allowedGroupOperations == GroupOperations.Undefined)
                        allowedGroupOperations = GroupOperations.None;

                    // Check for group operation exceptions where published group operation is not a subset of
                    // allowed group operations. For example, this accommodates cases like the "Evaluate" function
                    // which is forced to be a "Slice" operation, but only publishes "None". Additionally, only 
                    // apply this override if the requested group operation is a subset of either the allowed or
                    // published group operations
                    if ((publishedGroupOperations & allowedGroupOperations) == 0 &&
                        ((requestedGroupOperations & publishedGroupOperations) > 0 || (requestedGroupOperations & allowedGroupOperations) > 0))
                    {
                        // Override naming target group operation with original published group operation,
                        // for example, for the "Evaluate" function, name will not be "SliceEvaluate" but
                        // rather just "Evaluate" since original published group operation was "None"
                        groupOperationNameOverride = publishedGroupOperations;

                        // Published group operation is not a subset of allowed group operations, so
                        // we use the allowed group operations as the published group operations, for
                        // example, in the case of "Evaluate" function, we will publish the "Slice"
                        // group operation which will insert the required slice parameter
                        publishedGroupOperations = allowedGroupOperations;

                        // If the requested group operation is not a subset of the allowed group operations,
                        // then we use the allowed group operations as the requested group operations, for
                        // example, in the case of "Evaluate" function, we will request the "Slice" group
                        // even when the user requested group operation was "None"
                        if ((requestedGroupOperations & allowedGroupOperations) == 0)
                            requestedGroupOperations = allowedGroupOperations;
                    }

                    List<FunctionDescription> descriptions = new();

                    void addFunctionDescription(GroupOperations targetGroupOperation, IReadOnlyList<IParameter> definitions)
                    {
                        string formatFunctionName()
                        {
                            return (groupOperationNameOverride ?? targetGroupOperation) switch
                            {
                                GroupOperations.Slice => $"Slice{function.Name}",
                                GroupOperations.Set => $"Set{function.Name}",
                                _ => function.Name
                            };
                        }

                        static string formatDefaultValue(IParameter parameter)
                        {
                            if (parameter.Name.Equals("expression"))
                                return string.Empty;

                            return parameter.Default switch
                            {
                                string strVal => strVal,
                                DateTime dateTime => dateTime.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                                null => "null",
                                _ => parameter.Default.ToString()
                            };
                        }

                        static bool published(IParameter parameter) => !parameter.Internal;

                        descriptions.Add(new FunctionDescription
                        {
                            name = formatFunctionName(),
                            description = function.Description,
                            aliases = function.Aliases,
                            allowedGroupOperations = function.AllowedGroupOperations.ToString(),
                            publishedGroupOperations = function.PublishedGroupOperations.ToString(),
                            parameters = definitions.Where(published).Select(parameter => new ParameterDescription
                            {
                                name = parameter.Name,
                                description = parameter.Description,
                                type = parameter.Type.GetReflectedTypeName(false),
                                required = parameter.Required,
                                @default = formatDefaultValue(parameter)
                            })
                            .ToArray()
                        });
                    }

                    // Apply any requested group operation filters
                    publishedGroupOperations &= requestedGroupOperations;

                    if (publishedGroupOperations.HasFlag(GroupOperations.None))
                        addFunctionDescription(GroupOperations.None, function.ParameterDefinitions);

                    if (publishedGroupOperations.HasFlag(GroupOperations.Slice))
                        addFunctionDescription(GroupOperations.Slice, function.ParameterDefinitions.WithRequiredSliceParameter);

                    if (publishedGroupOperations.HasFlag(GroupOperations.Set))
                        addFunctionDescription(GroupOperations.Set, function.ParameterDefinitions);

                    return descriptions;
                });
            });
        },
        cancellationToken);
    }

    /// <summary>
    /// Search data source meta-data for a target.
    /// </summary>
    /// <param name="request">Search request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public virtual Task<string[]> Search(SearchRequest request, CancellationToken cancellationToken)
    {
        return Task.Factory.StartNew(() =>
        {
            return TargetCache<string[]>.GetOrAdd($"search!{request.dataTypeIndex}:{request.expression}", () =>
            {
                IDataSourceValue dataSourceValue = DataSourceValueCache.GetDefaultInstance(request.dataTypeIndex);
                DataSet metadata = Metadata.GetAugmentedDataSet(dataSourceValue);

                // Attempt to parse search target as a "SELECT" statement
                if (!parseSelectExpression(request.expression.Trim(), out string tableName, out bool distinct, out string[] fieldNames, out string expression, out string sortField, out int takeCount))
                {
                    // Expression was not a 'SELECT' statement, execute a 'LIKE' statement against primary meta-data table for data source value type
                    // returning matching point tags - this can be a slow operation for large meta-data sets, so results are cached by expression
                    return metadata.Tables[dataSourceValue.MetadataTableName]
                        .Select($"ID LIKE '{InstanceName}:%' AND PointTag LIKE '%{request.expression}%'")
                        .Take(MaximumSearchTargetsPerRequest)
                        .Select(row => $"{row["PointTag"]}")
                        .ToArray();
                }

                // Search target is a 'SELECT' statement, this operates as a filter for in memory metadata (not a database query)
                List<string> results = new();

                // If meta-data table does not contain the field names required by the data source value type,
                // return empty results - this is not a table supported by the data source value type
                if (!dataSourceValue.MetadataTableIsValid(metadata, tableName))
                    return results.ToArray();

                DataTable table = metadata.Tables[tableName];
                List<string> validFieldNames = new();

                for (int i = 0; i < fieldNames?.Length; i++)
                {
                    string fieldName = fieldNames[i].Trim();

                    if (table.Columns.Contains(fieldName))
                        validFieldNames.Add(fieldName);
                }

                // If no specific fields names were selected, assume "*" and select all fields
                fieldNames = validFieldNames.Count == 0 ? 
                    table.Columns.Cast<DataColumn>().Select(column => column.ColumnName).ToArray() : 
                    validFieldNames.ToArray();

                // If no filter expression or take count was specified, limit search target results - user can
                // still request larger results sets by specifying desired TOP count.
                if (takeCount == int.MaxValue && string.IsNullOrWhiteSpace(expression))
                    takeCount = MaximumSearchTargetsPerRequest;

                void executeSelect(IEnumerable<DataRow> queryOperation)
                {
                    results.AddRange(queryOperation.Take(takeCount).Select(row =>
                        string.Join(",", fieldNames.Select(fieldName => row[fieldName].ToString()))));
                }

                if (string.IsNullOrWhiteSpace(expression))
                    executeSelect(string.IsNullOrWhiteSpace(sortField) ? table.Select() : table.Select("true", sortField));
                else
                    executeSelect(table.Select(expression, sortField));

                return distinct ? 
                    results.Distinct(StringComparer.OrdinalIgnoreCase).ToArray() : 
                    results.ToArray();
            });
        },
        cancellationToken);

        // Attempt to parse an expression that has SQL SELECT syntax
        bool parseSelectExpression(string selectExpression, out string tableName, out bool distinct, out string[] fieldNames, out string expression, out string sortField, out int topCount)
        {
            tableName = null;
            distinct = false;
            fieldNames = null;
            expression = null;
            sortField = null;
            topCount = 0;

            if (string.IsNullOrWhiteSpace(selectExpression))
                return false;

            // RegEx instance used to parse meta-data for target search queries using a reduced SQL SELECT statement syntax
            s_selectExpression ??= new Regex(
                @"(SELECT\s+((?<Distinct>DISTINCT)\s+)?(TOP\s+(?<MaxRows>\d+)\s+)?(\s*((?<FieldName>\*)|((?<FieldName>\w+)(\s*,\s*(?<FieldName>\w+))*)))?\s*FROM\s+(?<TableName>\w+)\s+WHERE\s+(?<Expression>.+)\s+ORDER\s+BY\s+(?<SortField>\w+))|(SELECT\s+((?<Distinct>DISTINCT)\s+)?(TOP\s+(?<MaxRows>\d+)\s+)?(\s*((?<FieldName>\*)|((?<FieldName>\w+)(\s*,\s*(?<FieldName>\w+))*)))?\s*FROM\s+(?<TableName>\w+)\s+WHERE\s+(?<Expression>.+))|(SELECT\s+((?<Distinct>DISTINCT)\s+)?(TOP\s+(?<MaxRows>\d+)\s+)?((\s*((?<FieldName>\*)|((?<FieldName>\w+)(\s*,\s*(?<FieldName>\w+))*)))?)?\s*FROM\s+(?<TableName>\w+))",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

            Match match = s_selectExpression.Match(selectExpression.ReplaceControlCharacters());

            if (!match.Success)
                return false;

            tableName = match.Result("${TableName}").Trim();
            distinct = match.Result("${Distinct}").Trim().Length > 0;
            fieldNames = match.Groups["FieldName"].Captures.Cast<Capture>().Select(capture => capture.Value).ToArray();
            expression = match.Result("${Expression}").Trim();
            sortField = match.Result("${SortField}").Trim();

            string maxRows = match.Result("${MaxRows}").Trim();

            if (string.IsNullOrEmpty(maxRows) || !int.TryParse(maxRows, out topCount))
                topCount = int.MaxValue;

            return true;
        }
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
        DataSet metadata = Metadata.GetAugmentedDataSet<DataSourceValue>();
        Dictionary<string, DataRow> definitions = request.ParseSourceDefinitions(type, metadata, useFilterExpression);
        IEnumerable<TimeSeriesValues> annotationData = await Query(request.ExtractQueryRequest(definitions.Keys, MaximumAnnotationsPerRequest), cancellationToken).ConfigureAwait(false);
        List<AnnotationResponse> responses = new();

        foreach (TimeSeriesValues values in annotationData)
        {
            string[] parts = values.target.Split(',');
            string target;

            // Remove "Interval(0, {target})" from target if defined
            if (parts.Length > 1)
            {
                target = parts[1].Trim();
                target = target.Length > 1 ? target.Trim() : parts[0].Trim();
            }
            else
            {
                target = parts[0].Trim();
            }

            if (!definitions.TryGetValue(target, out DataRow definition))
                continue;

            int index = 0;

            foreach (double[] datapoint in values.datapoints)
            {
                if (!type.IsApplicable(datapoint))
                {
                    index++;
                    continue;
                }

                AnnotationResponse response;

                if (index == values.datapoints.Length - 1)
                {
                    response = new AnnotationResponse
                    {
                        time = datapoint[DataSourceValue.TimeIndex],
                        endTime = datapoint[DataSourceValue.TimeIndex]
                    };
                }
                else
                {
                    response = new AnnotationResponse
                    {
                        time = datapoint[DataSourceValue.TimeIndex],
                        endTime = values.datapoints[index + 1][DataSourceValue.TimeIndex]
                    };
                }

                type.PopulateResponse(response, target, definition, datapoint, metadata);
                responses.Add(response);

                index++;
            }
        }

        return responses;
    }

    /// <summary>
    /// Queries current alarm device state.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns> Queried device alarm states.</returns>
    public virtual Task<IEnumerable<AlarmDeviceStateView>> GetAlarmState(CancellationToken cancellationToken)
    {
        return Task.Factory.StartNew(() =>
        {
            using AdoDataConnection connection = new("systemSettings");
            return new TableOperations<AlarmDeviceStateView>(connection).QueryRecords("Name");
        },
        cancellationToken);
    }

    /// <summary>
    /// Queries All Available Device Alarm states.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns> Queried device alarm states.</returns>
    public virtual Task<IEnumerable<AlarmState>> GetDeviceAlarms(CancellationToken cancellationToken)
    {
        return Task.Factory.StartNew(() =>
        {
            using AdoDataConnection connection = new("systemSettings");
            return new TableOperations<AlarmState>(connection).QueryRecords("ID");
        },
        cancellationToken);
    }

    /// <summary>
    /// Queries All Available Device Groups.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns> List of Device Groups.</returns>
    public virtual Task<IEnumerable<DeviceGroup>> GetDeviceGroups(CancellationToken cancellationToken)
    {
        return Task.Factory.StartNew(() =>
        {
            using AdoDataConnection connection = new("systemSettings");

            IEnumerable<Device> groups = new TableOperations<Device>(connection).QueryRecordsWhere("AccessID = -99999");

            return groups.Select(item => new DeviceGroup
            {
                ID = item.ID,
                Name = item.Name,
                Devices = processDeviceGroup(item.ConnectionString).ToList()

            });
        },
        cancellationToken);

        IEnumerable<int> processDeviceGroup(string connectionString)
        {
            // Parse the connection string into a dictionary of key-value pairs for easy lookups
            Dictionary<string, string> settings = connectionString.ParseKeyValuePairs();

            return !settings.ContainsKey("DeviceIDs") ?
                new List<int>() :
                settings["DeviceIDs"].Split(',').Select(int.Parse);
        }
    }
}