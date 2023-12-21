//******************************************************************************************************
//  GrafanaDataSourceBase_AncillaryOperations.cs - Gbtc
//
//  Copyright © 2020, Grid Protection Alliance.  All Rights Reserved.
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
//  09/25/2020 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using GrafanaAdapters.GrafanaFunctionsCore;
using GSF;
using GSF.Data;
using GSF.Data.Model;
using Newtonsoft.Json;
using static GrafanaAdapters.GrafanaFunctionsCore.FunctionParser;

#pragma warning disable IDE0060 // Remove unused parameter

namespace GrafanaAdapters;

// Non-query specific Grafana functionality is defined here
partial class GrafanaDataSourceBase
{
    private static Regex s_selectExpression;

    /// <summary>
    /// Search data source meta-data for a target.
    /// </summary>
    /// <param name="request">Search target.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public virtual Task<string[]> Search(Target request, CancellationToken cancellationToken)
    {
        string target = request.target == "select metric" ? "" : request.target;

        // Attempt to parse an expression that has SQL SELECT syntax
        bool parseSelectExpression(string selectExpression, out string tableName, out string[] fieldNames, out string expression, out string sortField, out int topCount)
        {
            tableName = null;
            fieldNames = null;
            expression = null;
            sortField = null;
            topCount = 0;

            if (string.IsNullOrWhiteSpace(selectExpression))
                return false;

            // RegEx instance used to parse meta-data for target search queries using a reduced SQL SELECT statement syntax
            s_selectExpression ??= new Regex(@"(SELECT\s+(TOP\s+(?<MaxRows>\d+)\s+)?(\s*((?<FieldName>\*)|((?<FieldName>\w+)(\s*,\s*(?<FieldName>\w+))*)))?\s*FROM\s+(?<TableName>\w+)\s+WHERE\s+(?<Expression>.+)\s+ORDER\s+BY\s+(?<SortField>\w+))|(SELECT\s+(TOP\s+(?<MaxRows>\d+)\s+)?(\s*((?<FieldName>\*)|((?<FieldName>\w+)(\s*,\s*(?<FieldName>\w+))*)))?\s*FROM\s+(?<TableName>\w+)\s+WHERE\s+(?<Expression>.+))|(SELECT\s+(TOP\s+(?<MaxRows>\d+)\s+)?((\s*((?<FieldName>\*)|((?<FieldName>\w+)(\s*,\s*(?<FieldName>\w+))*)))?)?\s*FROM\s+(?<TableName>\w+))", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            Match match = s_selectExpression.Match(selectExpression.ReplaceControlCharacters());

            if (!match.Success)
                return false;

            tableName = match.Result("${TableName}").Trim();
            fieldNames = match.Groups["FieldName"].Captures.Cast<Capture>().Select(capture => capture.Value).ToArray();
            expression = match.Result("${Expression}").Trim();
            sortField = match.Result("${SortField}").Trim();

            string maxRows = match.Result("${MaxRows}").Trim();

            if (string.IsNullOrEmpty(maxRows) || !int.TryParse(maxRows, out topCount))
                topCount = int.MaxValue;

            return true;
        }

        return Task.Factory.StartNew(() =>
            {
                return TargetCache<string[]>.GetOrAdd($"search!{target}", () =>
                {
                    if (request.target is not null)
                    {
                        // Attempt to parse search target as a SQL SELECT statement that will operate as a filter for in memory metadata (not a database query)
                        if (parseSelectExpression(request.target.Trim(), out string tableName, out string[] fieldNames, out string expression, out string sortField, out int takeCount))
                        {
                            DataTableCollection tables = Metadata.Tables;
                            List<string> results = new();

                            if (tables.Contains(tableName))
                            {
                                DataTable table = tables[tableName];
                                List<string> validFieldNames = new();

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
                                    results.AddRange(queryOperation.Take(takeCount).Select(row =>
                                        string.Join(",", fieldNames.Select(fieldName => row[fieldName].ToString()))));
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
                            }

                            return results.ToArray();
                        }
                    }

                    // Non "SELECT" style expressions default to searches on ActiveMeasurements meta-data table
                    if (request.isPhasor)
                    {
                        return Metadata.Tables["Phasor"].AsEnumerable()
                            .Select(row => row["Label"].ToString())
                            .ToArray();
                    }
                    // Non "SELECT" style expressions default to searches on ActiveMeasurements meta-data table
                    else
                    {
                        return Metadata.Tables["ActiveMeasurements"].Select($"ID LIKE '{InstanceName}:%' AND PointTag LIKE '%{target}%'").Take(MaximumSearchTargetsPerRequest).Select(row => $"{row["PointTag"]}").ToArray();
                    }
                });
            },
            cancellationToken);
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
                if (index == values.datapoints.Count-1 )
                    response = new AnnotationResponse
                    {
                        time = datapoint[TimeSeriesValues.Time],
                        endTime = datapoint[TimeSeriesValues.Time],
                    };
                else
                    response = new AnnotationResponse
                    {
                        time = datapoint[TimeSeriesValues.Time],
                        endTime = values.datapoints[index + 1][TimeSeriesValues.Time],
                    };

                type.PopulateResponse(response, target, definition, datapoint, Metadata);

                responses.Add(response);
                index++;
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
                using AdoDataConnection connection = new("systemSettings"); 
                return new TableOperations<AlarmDeviceStateView>(connection).QueryRecords("Name");
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
                using AdoDataConnection connection = new("systemSettings");
                return new TableOperations<AlarmState>(connection).QueryRecords("ID");
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
                using AdoDataConnection connection = new("systemSettings");

                IEnumerable<GSF.TimeSeries.Model.Device> groups = (new TableOperations<GSF.TimeSeries.Model.Device>(connection)).QueryRecordsWhere("AccessID = -99999");

                return groups.Select(item => new DeviceGroup
                {
                    ID = item.ID,
                    Name = item.Name,
                    Devices = ProcessDeviceGroup(item.ConnectionString).ToList()

                });
            },
            cancellationToken);
    }

    private IEnumerable<int> ProcessDeviceGroup(string connectionString)
    {
        // Parse the connection string into a dictionary of key-value pairs for easy lookups
        Dictionary<string, string> settings = connectionString.ParseKeyValuePairs();

        return !settings.ContainsKey("DeviceIDs") ? 
            new List<int>() : 
            settings["DeviceIDs"].Split(',').Select(item => ParseInt(item));
    }

    /// <summary>
    /// Queries available MetaData Options.
    /// </summary>
    /// <param name="isPhasor">A boolean indicating whether the data is a phasor.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns> Available MetaData Table options.</returns>
    public Task<string[]> GetTableOptions(bool isPhasor, CancellationToken cancellationToken)
    {
        Func<DataTable, bool> hasCollumns = (tbl) => tbl.Columns.Contains("ID") &&
                                                     tbl.Columns.Contains("SignalID") &&
                                                     tbl.Columns.Contains("PointTag") &&
                                                     tbl.Columns.Contains("Adder") &&
                                                     tbl.Columns.Contains("Multiplier");

        return Task.Factory.StartNew(() =>
            {
                // Phasor mode only has one table for now
                if (isPhasor)
                    return new string[1] { "Phasor" };

                List<string> tables = new();    

                foreach (DataTable table in Metadata.Tables)
                {
                    if (hasCollumns(table) && table.Rows.Count > 0)
                        tables.Add(table.TableName); 
                }
                return tables.ToArray();
            },
            cancellationToken);
    }

    private static int ParseInt(string parameter, bool includeZero = true)
    {
        parameter = parameter.Trim();

        if (!int.TryParse(parameter, out int value))
            throw new FormatException($"Could not parse '{parameter}' as an integer value.");

        if (includeZero)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException($"Value '{parameter}' is less than zero.");
        }
        else
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException($"Value '{parameter}' is less than or equal to zero.");
        }

        return value;
    }

    /// <summary>
    /// Queries description of available functions.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task<FunctionDescription[]> GetFunctionDescription(CancellationToken cancellationToken)
    {
        return Task.Factory.StartNew(() => FunctionParser.GetFunctionDescription().ToArray(),            
            cancellationToken);
            
    }

    /// <summary>
    /// Requests Grafana Metadata source for multiple targets.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <param name="request"> The targets and the meta data requested</param>
    /// <returns> Queried metadata.</returns>
    public Task<Dictionary<string, string[]>> GetMetadataOptions(MetadataOptionsRequest request, CancellationToken cancellationToken)
    {
        return Task.Factory.StartNew(() =>
            {
                Dictionary<string, string[]> tableColumnNames = new();

                foreach (string table in request.Tables)
                {
                    if (Metadata.Tables.Contains(table))
                    {
                        DataColumnCollection columns = Metadata.Tables[table].Columns;

                        List<string> columnNames = new();

                        for (int i = 0; i < columns.Count; i++)
                        {
                            columnNames.Add(columns[i].ColumnName);
                        }

                        tableColumnNames[table] = columnNames.ToArray();
                    }
                }
                return tableColumnNames;

            },
            cancellationToken);
    }

    /// <summary>
    /// Queries openHistorian as a Grafana Metadata source.
    /// </summary>
    /// <param name="request">Query request.</param>
    public Task<string> GetMetadata(Target request)
    {
        return Task.Factory.StartNew(() =>
        {
            HashSet<string> result = new();

            if (string.IsNullOrWhiteSpace(request.target))
            {

                string table = request.metadataSelection.FirstOrDefault().Key;
                string field = request.metadataSelection.FirstOrDefault().Value.FirstOrDefault();

                DataRow[] rows = Metadata.Tables[table].Select() ?? Array.Empty<DataRow>();
                foreach (DataRow row in rows)
                {
                    result.Add(row[field].ToString());
                }
                return JsonConvert.SerializeObject(result);
            }

            IEnumerable<ParsedTarget<DataSourceValue>> targets = ParsedTarget<DataSourceValue>.ParseTargets(request.target.Trim());
            string[] rootTargets = targets.SelectMany(pt => pt.DataTargets).ToArray();

            foreach (string t in rootTargets)
            {
                KeyValuePair<string, string> data = FunctionParser
                    .GetMetadata(this, t, request.metadataSelection)
                    .FirstOrDefault();
                result.Add(data.Value);
            }

            return JsonConvert.SerializeObject(result);
        });
    }

}