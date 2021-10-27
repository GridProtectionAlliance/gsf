//******************************************************************************************************
//  AnnotationExtensions.cs - Gbtc
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

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using GSF;
using GSF.Diagnostics;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;

// ReSharper disable CompareOfFloatsByEqualityOperator
namespace GrafanaAdapters
{
    /// <summary>
    /// Grafana <see cref="Annotation"/> extensions class.
    /// </summary>
    public static class AnnotationExtensions
    {
        private static readonly LogPublisher s_log = Logger.CreatePublisher(typeof(AnnotationExtensions), MessageClass.Component);
        private static readonly Regex s_aliasedTagExpression = new(@"^\s*(?<Identifier>[A-Z_][A-Z0-9_]*)\s*\=\s*(?<Expression>.+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Gets table name for specified annotation <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Annotation type.</param>
        /// <returns>Table name for specified annotation <paramref name="type"/>.</returns>
        public static string TableName(this AnnotationType type)
        {
            switch (type)
            {
                case AnnotationType.RaisedAlarms:
                    return "RaisedAlarms";
                case AnnotationType.ClearedAlarms:
                    return "ClearedAlarms";
                default:
                    return "Undefined";
            }
        }

        private static string Translate(this string tableName)
        {
            // Source metadata for raised and cleared alarms is the same
            if (tableName.Equals("RaisedAlarms", StringComparison.OrdinalIgnoreCase))
                return "Alarms";

            if (tableName.Equals("ClearedAlarms", StringComparison.OrdinalIgnoreCase))
                return "Alarms";

            return tableName;
        }

        /// <summary>
        /// Gets the target field name for Guid based point IDs for table used with specified annotation <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Annotation type.</param>
        /// <returns>Target field name for Guid based point IDs for specified annotation <paramref name="type"/>.</returns>
        public static string TargetFieldName(this AnnotationType type)
        {
            switch (type)
            {
                case AnnotationType.RaisedAlarms:
                case AnnotationType.ClearedAlarms:
                    return "AssociatedMeasurementID";
                default:
                    throw new InvalidOperationException("Cannot extract target for specified annotation type.");
            }
        }

        /// <summary>
        /// Determines if the data point is applicable for specified annotation <paramref name="type"/>. 
        /// </summary>
        /// <param name="type">Annotation type.</param>
        /// <param name="datapoint">Time series values data point.</param>
        /// <returns><c>true</c> if the data point is applicable for specified annotation <paramref name="type"/>; otherwise, <c>false</c>.</returns>
        public static bool IsApplicable(this AnnotationType type, double[] datapoint)
        {
            if (datapoint is null)
                throw new ArgumentNullException(nameof(datapoint));

            double value = datapoint[TimeSeriesValues.Value];

            switch (type)
            {
                case AnnotationType.RaisedAlarms:
                    return value != 0.0D;
                case AnnotationType.ClearedAlarms:
                    return value == 0.0D;
                default:
                    throw new InvalidOperationException("Cannot determine data point applicability for specified annotation type.");
            }
        }

        /// <summary>
        /// Populates an annotation response title, text and tags for specified annotation <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Annotation type.</param>
        /// <param name="response">Annotation response.</param>
        /// <param name="target">Target of annotation response.</param>
        /// <param name="definition">Associated metadata definition for response.</param>
        /// <param name="datapoint">Time series values data point for response.</param>
        /// <param name="source">Metadata of source definitions.</param>
        /// <returns>Populates an annotation response title, text and tags for specified annotation <paramref name="type"/>.</returns>
        public static void PopulateResponse(this AnnotationType type, AnnotationResponse response, string target, DataRow definition, double[] datapoint, DataSet source)
        {
            if (response is null)
                throw new ArgumentNullException(nameof(response));

            if (target is null)
                throw new ArgumentNullException(nameof(target));

            if (definition is null)
                throw new ArgumentNullException(nameof(definition));

            if (datapoint is null)
                throw new ArgumentNullException(nameof(datapoint));

            switch (type)
            {
                case AnnotationType.RaisedAlarms:
                case AnnotationType.ClearedAlarms:
                    DataRow metadata = GetTargetMetaData(source, definition["SignalID"]);
                    response.title = $"Alarm {(type == AnnotationType.RaisedAlarms ? "Raised" : "Cleared")}";
                    response.text = $"{definition["Description"]}<br/>Condition:&nbsp;{GetAlarmCondition(definition)}<br/>Severity:&nbsp;{definition["Severity"]}<br/>[{metadata["ID"]}]:&nbsp;{metadata["SignalReference"]}";
                    response.tags = $"{metadata["PointTag"]},{target}";
                    break;
                default:
                    throw new InvalidOperationException("Cannot populate response information for specified annotation type.");
            }
        }

        /// <summary>
        /// Extracts a Grafana <see cref="QueryRequest"/> from an <see cref="AnnotationRequest"/>.
        /// </summary>
        /// <param name="request">Annotation request.</param>
        /// <param name="targets">List of desired targets.</param>
        /// <param name="maxDataPoints">Maximum points to return.</param>
        /// <returns>Grafana query request object from an annotation <paramref name="request"/>.</returns>
        public static QueryRequest ExtractQueryRequest(this AnnotationRequest request, IEnumerable<string> targets, int maxDataPoints)
        {
            if (targets is null)
                throw new ArgumentNullException(nameof(targets));

            // Create annotation query request for full resolution data using "Interval(0, {target})"
            // function so that any encountered alarms not will not be down-sampled
            return new()
            {
                range = request.range,
                rangeRaw = request.rangeRaw,
                interval = "*",
                targets = targets.Select((target, index) => new Target { refId = $"ID{index}", target = $"Interval(0, {target})" }).ToList(),
                format = "json",
                maxDataPoints = maxDataPoints
            };
        }

        /// <summary>
        /// Parses query expression from annotation for annotation type.
        /// </summary>
        /// <param name="annotation">Grafana annotation.</param>
        /// <param name="useFilterExpression">Determines if query is using a filter expression.</param>
        /// <returns>Parsed annotation type for query expression from <paramref name="annotation"/>.</returns>
        public static AnnotationType ParseQueryType(this Annotation annotation, out bool useFilterExpression)
        {
            if (annotation is null)
                throw new ArgumentNullException(nameof(annotation));

            string query = annotation.query ?? "";

            Tuple<AnnotationType, bool> result = TargetCache<Tuple<AnnotationType, bool>>.GetOrAdd(query, () =>
            {
                AnnotationType type = AnnotationType.Undefined;
                bool parsedFilterExpression = false;

                if (AdapterBase.ParseFilterExpression(query, out string tableName, out string _, out string _, out int _))
                {
                    parsedFilterExpression = true;

                    switch (tableName.ToUpperInvariant())
                    {
                        case "RAISEDALARMS":
                            type = AnnotationType.RaisedAlarms;
                            break;
                        case "CLEAREDALARMS":
                            type = AnnotationType.ClearedAlarms;
                            break;
                        default:
                            throw new InvalidOperationException("Invalid FILTER table for annotation query expression.");
                    }
                }
                else if (query.StartsWith("#RaisedAlarms", StringComparison.OrdinalIgnoreCase))
                {
                    type = AnnotationType.RaisedAlarms;
                }
                else if (query.StartsWith("#ClearedAlarms", StringComparison.OrdinalIgnoreCase))
                {
                    type = AnnotationType.ClearedAlarms;
                }

                if (type == AnnotationType.Undefined)
                    throw new InvalidOperationException("Unrecognized type or syntax for annotation query expression.");

                return new(type, parsedFilterExpression);
            });

            useFilterExpression = result.Item2;

            return result.Item1;
        }

        /// <summary>
        /// Parses query expression from annotation request for annotation type.
        /// </summary>
        /// <param name="request">Grafana annotation request.</param>
        /// <param name="useFilterExpression">Determines if query is using a filter expression.</param>
        /// <returns>Parsed annotation type for query expression from annotation <paramref name="request"/>.</returns>
        public static AnnotationType ParseQueryType(this AnnotationRequest request, out bool useFilterExpression)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            return request.annotation.ParseQueryType(out useFilterExpression);
        }

        /// <summary>
        /// Parses source definitions for an annotation query.
        /// </summary>
        /// <param name="annotation">Grafana annotation.</param>
        /// <param name="type">Annotation type.</param>
        /// <param name="source">Metadata of source definitions.</param>
        /// <param name="useFilterExpression">Determines if query is using a filter expression.</param>
        /// <returns>Parsed source definitions from <paramref name="annotation"/>.</returns>
        public static Dictionary<string, DataRow> ParseSourceDefinitions(this Annotation annotation, AnnotationType type, DataSet source, bool useFilterExpression)
        {
            if (annotation is null)
                throw new ArgumentNullException(nameof(annotation));

            if (source is null)
                throw new ArgumentNullException(nameof(source));

            if (type == AnnotationType.Undefined)
                throw new InvalidOperationException("Unrecognized type or syntax for annotation query expression.");

            string query = annotation.query ?? "";

            return TargetCache<Dictionary<string, DataRow>>.GetOrAdd(query, () =>
            {
                DataRow[] rows;

                if (useFilterExpression)
                {
                    if (AdapterBase.ParseFilterExpression(query, out string tableName, out string expression, out string sortField, out int takeCount))
                        rows = source.Tables[tableName.Translate()].Select(expression, sortField).Take(takeCount).ToArray();
                    else
                        throw new InvalidOperationException("Invalid FILTER syntax for annotation query expression.");
                }
                else
                {
                    // Assume all records if no filter expression was provided
                    rows = source.Tables[type.TableName().Translate()].Rows.Cast<DataRow>().ToArray();
                }

                Dictionary<string, DataRow> definitions = new Dictionary<string, DataRow>(StringComparer.OrdinalIgnoreCase);

                foreach (DataRow row in rows)
                {
                    MeasurementKey key = GetTargetFromGuid(row[type.TargetFieldName()].ToString());

                    if (key != MeasurementKey.Undefined)
                        definitions[key.TagFromKey(source)] = row;
                }

                return definitions;
            });
        }

        /// <summary>
        /// Parses source definitions for an annotation query.
        /// </summary>
        /// <param name="request">Grafana annotation request.</param>
        /// <param name="type">Annotation type.</param>
        /// <param name="source">Metadata of source definitions.</param>
        /// <param name="useFilterExpression">Determines if query is using a filter expression.</param>
        /// <returns>Parsed source definitions from annotation <paramref name="request"/>.</returns>
        public static Dictionary<string, DataRow> ParseSourceDefinitions(this AnnotationRequest request, AnnotationType type, DataSet source, bool useFilterExpression)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            return request.annotation.ParseSourceDefinitions(type, source, useFilterExpression);
        }

        /// <summary>
        /// Looks up point tag from measurement <paramref name="key"/> value.
        /// </summary>
        /// <param name="key"><see cref="MeasurementKey"/> to lookup.</param>
        /// <param name="source">Source metadata.</param>
        /// <returns>Point tag name from source metadata.</returns>
        /// <remarks>
        /// This function uses the <see cref="DataTable.Select(string)"/> function which uses a linear
        /// search algorithm that can be slow for large data sets, it is recommended that any results
        /// for calls to this function be cached to improve performance.
        /// </remarks>
        internal static string TagFromKey(this MeasurementKey key, DataSet source)
        {
            DataRow record = GetMetaData(source, "ActiveMeasurements", $"ID = '{key}'");
            return record is null ? key.ToString() : record["PointTag"].ToNonNullString(key.ToString());
        }

        /// <summary>
        /// Looks up measurement key from point tag.
        /// </summary>
        /// <param name="pointTag">Point tag to lookup.</param>
        /// <param name="source">Source metadata.</param>
        /// <param name="table">Table to search.</param>
        /// <returns>Measurement key from source metadata.</returns>
        /// <remarks>
        /// This function uses the <see cref="DataTable.Select(string)"/> function which uses a linear
        /// search algorithm that can be slow for large data sets, it is recommended that any results
        /// for calls to this function be cached to improve performance.
        /// </remarks>
        internal static MeasurementKey KeyFromTag(this string pointTag, DataSet source, string table = "ActiveMeasurements")
        {
            DataRow record = pointTag.MetadataRecordFromTag(source, table);

            if (record is null)
                return MeasurementKey.Undefined;

            try
            {
                return MeasurementKey.LookUpOrCreate(record["SignalID"].ToNonNullString(Guid.Empty.ToString()).ConvertToType<Guid>(), record["ID"].ToString());
            }
            catch (Exception ex)
            {
                Logger.SwallowException(ex);
                return MeasurementKey.Undefined;
            }
        }

        /// <summary>
        /// Looks up measurement key from signal ID.
        /// </summary>
        /// <param name="signalID">Signal ID to lookup.</param>
        /// <param name="source">Source metadata.</param>
        /// <param name="table">Table to search.</param>
        /// <returns>Measurement key from source metadata.</returns>
        /// <remarks>
        /// This function uses the <see cref="DataTable.Select(string)"/> function which uses a linear
        /// search algorithm that can be slow for large data sets, it is recommended that any results
        /// for calls to this function be cached to improve performance.
        /// </remarks>
        internal static Tuple<MeasurementKey, string> KeyAndTagFromSignalID(this string signalID, DataSet source, string table = "ActiveMeasurements")
        {
            DataRow record = signalID.MetadataRecordFromSignalID(source, table);
            string pointTag = "Undefined";

            if (record is null)
                return new(MeasurementKey.Undefined, pointTag);

            try
            {
                MeasurementKey key = MeasurementKey.LookUpOrCreate(record["SignalID"].ToNonNullString(Guid.Empty.ToString()).ConvertToType<Guid>(), record["ID"].ToString());
                pointTag = record["PointTag"].ToNonNullString(key.ToString());
                return new(key, pointTag);
            }
            catch (Exception ex)
            {
                Logger.SwallowException(ex);
                return new(MeasurementKey.Undefined, pointTag);
            }
        }

        /// <summary>
        /// Splits any defined alias from a point tag expression.
        /// </summary>
        /// <param name="tagExpression">Source point tag expression that can contain an alias.</param>
        /// <param name="alias">Alias, if defined.</param>
        /// <returns>Point tag name without any alias.</returns>
        internal static string SplitAlias(this string tagExpression, out string alias)
        {
            Match match = s_aliasedTagExpression.Match(tagExpression);

            if (match.Success)
            {
                alias = match.Result("${Identifier}");
                return match.Result("${Expression}").Trim();
            }

            alias = null;
            return tagExpression;
        }

        /// <summary>
        /// Looks up metadata record from point tag.
        /// </summary>
        /// <param name="pointTag">Point tag to lookup.</param>
        /// <param name="source">Source metadata.</param>
        /// <param name="table">Table to search.</param>
        /// <returns>Metadata record from source metadata for provided point tag.</returns>
        /// <remarks>
        /// <para>
        /// Use "table.pointTag" format to specify which table to pull point tag from.
        /// </para>
        /// <para>
        /// This function uses the <see cref="DataTable.Select(string)"/> function which uses a linear
        /// search algorithm that can be slow for large data sets, it is recommended that any results
        /// for calls to this function be cached to improve performance.
        /// </para>
        /// </remarks>
        internal static DataRow MetadataRecordFromTag(this string pointTag, DataSet source, string table) => 
            GetMetaData(source, table, $"PointTag = '{SplitAlias(pointTag, out string _)}'");

        /// <summary>
        /// Looks up metadata record from signal ID.
        /// </summary>
        /// <param name="signalID">Signal ID to lookup.</param>
        /// <param name="source">Source metadata.</param>
        /// <param name="table">Table to search.</param>
        /// <returns>Metadata record from source metadata for provided point tag.</returns>
        /// <remarks>
        /// This function uses the <see cref="DataTable.Select(string)"/> function which uses a linear
        /// search algorithm that can be slow for large data sets, it is recommended that any results
        /// for calls to this function be cached to improve performance.
        /// </remarks>
        internal static DataRow MetadataRecordFromSignalID(this string signalID, DataSet source, string table) => 
            GetMetaData(source, table, $"SignalID = '{signalID}'");

        private static DataRow GetMetaData(DataSet source, string table, string expression)
        {
            try
            {
                DataRow[] filteredRows = source.Tables[table].Select(expression);

                if (filteredRows.Length > 1)
                    s_log.Publish(MessageLevel.Warning, "Duplicate Tag Names", $"Grafana query for \"{expression}\" produced {filteredRows.Length:N0} records. Key values for meta-data are expected to be unique, invalid meta-data results may be returned.");

                return filteredRows.Length > 0 ? filteredRows[0] : null;
            }
            catch (Exception ex)
            {
                Logger.SwallowException(ex);
                return null;
            }
        }

        private static MeasurementKey GetTargetFromGuid(string guidID) => 
            MeasurementKey.LookUpBySignalID(Guid.Parse(guidID));

        private static DataRow GetTargetMetaData(DataSet source, object value)
        {
            string target = value.ToNonNullNorWhiteSpace(Guid.Empty.ToString());
            return TargetCache<DataRow>.GetOrAdd(target, () => GetMetaData(source, "ActiveMeasurements", $"ID = '{GetTargetFromGuid(target)}'"));
        }

        private static string GetAlarmCondition(DataRow defintion)
        {
            StringBuilder description;

            description = new("value");

            if (!Enum.TryParse(defintion["Operation"].ToNonNullNorWhiteSpace(AlarmOperation.Equal.ToString()), out AlarmOperation operation))
                operation = AlarmOperation.Equal;

            switch (operation)
            {
                case AlarmOperation.Equal:
                    description.Append(" = ");
                    break;

                case AlarmOperation.NotEqual:
                    description.Append(" != ");
                    break;

                case AlarmOperation.GreaterOrEqual:
                    description.Append(" >= ");
                    break;

                case AlarmOperation.LessOrEqual:
                    description.Append(" <= ");
                    break;

                case AlarmOperation.GreaterThan:
                    description.Append(" > ");
                    break;

                case AlarmOperation.LessThan:
                    description.Append(" < ");
                    break;

                case AlarmOperation.Flatline:
                    description.Append(" flat-lined for ");
                    description.Append(defintion["Delay"]);
                    description.Append(" seconds");
                    return description.ToString();

                default:
                    description.Append(operation.GetDescription());
                    break;
            }

            string setPoint = defintion["SetPoint"].ToNonNullString();

            description.Append(string.IsNullOrWhiteSpace(setPoint) ? "undefined" : setPoint);

            return description.ToString();
        }
    }
}