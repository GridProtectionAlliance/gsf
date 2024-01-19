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
// ReSharper disable CompareOfFloatsByEqualityOperator

using GrafanaAdapters.DataSources.BuiltIn;
using GrafanaAdapters.Metadata;
using GrafanaAdapters.Model.Common;
using GSF;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace GrafanaAdapters.Model.Annotations;

/// <summary>
/// Grafana <see cref="AnnotationRequest"/> extensions class.
/// </summary>
public static class AnnotationRequestExtensions
{
    /// <summary>
    /// Gets table name for specified annotation <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Annotation type.</param>
    /// <returns>Table name for specified annotation <paramref name="type"/>.</returns>
    public static string TableName(this AnnotationType type)
    {
        return type switch
        {
            AnnotationType.RaisedAlarms => "RaisedAlarms",
            AnnotationType.ClearedAlarms => "ClearedAlarms",
            AnnotationType.Alarms => "Alarms",
            _ => "Undefined"
        };
    }

    private static string Translate(this string tableName)
    {
        // Source metadata for raised and cleared alarms is the same
        if (tableName.Equals("RaisedAlarms", StringComparison.OrdinalIgnoreCase))
            return "Alarms";

        if (tableName.Equals("ClearedAlarms", StringComparison.OrdinalIgnoreCase))
            return "Alarms";

        if (tableName.Equals("Alarms", StringComparison.OrdinalIgnoreCase))
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
        return type switch
        {
            AnnotationType.RaisedAlarms => "AssociatedMeasurementID",
            AnnotationType.ClearedAlarms => "AssociatedMeasurementID",
            AnnotationType.Alarms => "AssociatedMeasurementID",
            _ => throw new InvalidOperationException("Cannot extract target for specified annotation type.")
        };
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

        double value = datapoint[DataSourceValue.ValueIndex];

        return type switch
        {
            AnnotationType.RaisedAlarms => value != 0.0D,
            AnnotationType.ClearedAlarms => value == 0.0D,
            AnnotationType.Alarms => value == 0.0D,
            _ => throw new InvalidOperationException("Cannot determine data point applicability for specified annotation type.")
        };
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

        DataRow metadata;

        switch (type)
        {
            case AnnotationType.RaisedAlarms:
            case AnnotationType.ClearedAlarms:
                metadata = GetTargetMetaData(source, definition["SignalID"]);
                response.title = $"Alarm {(type == AnnotationType.RaisedAlarms ? "Raised" : "Cleared")}";
                response.text = $"{definition["Description"]}<br/>Condition:&nbsp;{GetAlarmCondition(definition)}<br/>Severity:&nbsp;{definition["Severity"]}<br/>[{metadata["ID"]}]:&nbsp;{metadata["SignalReference"]}";
                response.tags = $"{metadata["PointTag"]},{target}";
                response.endTime = response.time;
                break;
            case AnnotationType.Alarms:
                metadata = GetTargetMetaData(source, definition["SignalID"]);
                response.title = "Alarm";
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

        // Create annotation query requesting full resolution data
        // function so that any encountered alarms not will not be down-sampled
        return new QueryRequest
        {
            dataTypeIndex = DataSourceValue.TypeIndex,
            range = request.range,
            interval = "*",
            targets = targets.Select((target, index) => new Target { refID = $"ID{index}", target = $"{target}; fullResolutionQuery" }).ToArray(),
            maxDataPoints = maxDataPoints
        };
    }

    /// <summary>
    /// Parses query expression from annotation for annotation type.
    /// </summary>
    /// <param name="request">Grafana annotation.</param>
    /// <param name="useFilterExpression">Determines if query is using a filter expression.</param>
    /// <returns>Parsed annotation type for query expression from <paramref name="request"/>.</returns>
    public static AnnotationType ParseQueryType(this AnnotationRequest request, out bool useFilterExpression)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        string query = request.annotationQuery ?? "";

        Tuple<AnnotationType, bool> result = TargetCache<Tuple<AnnotationType, bool>>.GetOrAdd(query, () =>
        {
            AnnotationType type = AnnotationType.Undefined;
            bool parsedFilterExpression = false;

            if (AdapterBase.ParseFilterExpression(query, out string tableName, out string _, out string _, out int _))
            {
                parsedFilterExpression = true;

                type = tableName.ToUpperInvariant() switch
                {
                    "RAISEDALARMS" => AnnotationType.RaisedAlarms,
                    "CLEAREDALARMS" => AnnotationType.ClearedAlarms,
                    "EVENTS" => AnnotationType.Events,
                    "ALARMS" => AnnotationType.Alarms,
                    _ => throw new InvalidOperationException("Invalid FILTER table for annotation query expression.")
                };
            }
            else if (query.StartsWith("#RaisedAlarms", StringComparison.OrdinalIgnoreCase))
            {
                type = AnnotationType.RaisedAlarms;
            }
            else if (query.StartsWith("#ClearedAlarms", StringComparison.OrdinalIgnoreCase))
            {
                type = AnnotationType.ClearedAlarms;
            }
            else if (query.StartsWith("#Alarms", StringComparison.OrdinalIgnoreCase))
            {
                type = AnnotationType.Alarms;
            }
            else if (query.StartsWith("#Events", StringComparison.OrdinalIgnoreCase))
            {
                type = AnnotationType.Events;
            }
            if (type == AnnotationType.Undefined)
            {
                throw new InvalidOperationException("Unrecognized type or syntax for annotation query expression.");
            }

            return new Tuple<AnnotationType, bool>(type, parsedFilterExpression);
        });

        useFilterExpression = result.Item2;

        return result.Item1;
    }

    /// <summary>
    /// Parses source definitions for an annotation query.
    /// </summary>
    /// <param name="request">Grafana annotation request.</param>
    /// <param name="type">Annotation type.</param>
    /// <param name="source">Metadata of source definitions.</param>
    /// <param name="useFilterExpression">Determines if query is using a filter expression.</param>
    /// <returns>Parsed source definitions from <paramref name="request"/>.</returns>
    public static Dictionary<string, DataRow> ParseSourceDefinitions(this AnnotationRequest request, AnnotationType type, DataSet source, bool useFilterExpression)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        if (source is null)
            throw new ArgumentNullException(nameof(source));

        if (type == AnnotationType.Undefined)
            throw new InvalidOperationException("Unrecognized type or syntax for annotation query expression.");

        string query = request.annotationQuery ?? "";

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

            Dictionary<string, DataRow> definitions = new(StringComparer.OrdinalIgnoreCase);

            foreach (DataRow row in rows)
            {
                MeasurementKey key = row[type.TargetFieldName()].ToString().KeyFromSignalID();

                if (key != MeasurementKey.Undefined)
                    definitions[key.TagFromKey(source)] = row;
            }

            return definitions;
        });
    }

    private static string GetAlarmCondition(DataRow definition)
    {
        StringBuilder description = new("value");

        if (!Enum.TryParse(definition["Operation"].ToNonNullNorWhiteSpace(AlarmOperation.Equal.ToString()), out AlarmOperation operation))
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
                description.Append(definition["Delay"]);
                description.Append(" seconds");
                return description.ToString();

            default:
                description.Append(operation.GetDescription());
                break;
        }

        string setPoint = definition["SetPoint"].ToNonNullString();

        description.Append(string.IsNullOrWhiteSpace(setPoint) ? "undefined" : setPoint);

        return description.ToString();
    }

    internal static DataRow GetTargetMetaData(DataSet source, object signalIDFieldValue)
    {
        string signalID = signalIDFieldValue.ToNonNullNorWhiteSpace(Guid.Empty.ToString());
        return TargetCache<DataRow>.GetOrAdd(signalID, () => source.GetMetadata(DataSourceValue.MetadataTableName, $"ID = '{signalID.KeyFromSignalID()}'"));
    }
}