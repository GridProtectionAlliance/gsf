//******************************************************************************************************
//  MetadataExtensions.cs - Gbtc
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
//  08/23/2023 - Timothy Liakh
//       Generated original version of source code.
//
//******************************************************************************************************

using GrafanaAdapters.DataSources.BuiltIn;
using GrafanaAdapters.Functions;
using GSF;
using GSF.Diagnostics;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace GrafanaAdapters.DataSources;

internal static class MetadataExtensions
{
    private static readonly LogPublisher s_log = Logger.CreatePublisher(typeof(MetadataExtensions), MessageClass.Component);
    private static readonly Regex s_aliasedTagExpression = new(@"^\s*(?<Identifier>[A-Z_][A-Z0-9_]*)\s*\=\s*(?<Expression>.+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Splits any defined alias from a point tag expression.
    /// </summary>
    /// <param name="tagExpression">Source point tag expression that can contain an alias.</param>
    /// <param name="alias">Alias, if defined.</param>
    /// <returns>Point tag name without any alias.</returns>
    public static string SplitAlias(this string tagExpression, out string alias)
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
    /// Looks up point tag from measurement <paramref name="key"/> value.
    /// </summary>
    /// <param name="key"><see cref="MeasurementKey"/> to lookup.</param>
    /// <param name="source">Source metadata.</param>
    /// <param name="table">Table to search.</param>
    /// <param name="pointTagField">Point tag field name.</param>
    /// <param name="idField">Measurement key-based ID field name.</param>
    /// <returns>Point tag name from source metadata.</returns>
    /// <remarks>
    /// This function uses the <see cref="DataTable.Select(string)"/> function which uses a linear
    /// search algorithm that can be slow for large data sets, it is recommended that any results
    /// for calls to this function be cached to improve performance.
    /// </remarks>
    public static string TagFromKey(this MeasurementKey key, DataSet source, string table = DataSourceValue.MetadataTableName, string pointTagField = "PointTag", string idField = "ID")
    {
        DataRow record = key.ToString().RecordFromKey(source, table, idField);
        return record is null ? key.ToString() : record[pointTagField].ToNonNullString(key.ToString());
    }

    /// <summary>
    /// Looks up measurement key from point tag.
    /// </summary>
    /// <param name="pointTag">Point tag to lookup.</param>
    /// <param name="source">Source metadata.</param>
    /// <param name="table">Table to search.</param>
    /// <param name="pointTagField">Point tag field name.</param>
    /// <param name="signalIDField">Guid-based signal ID field name.</param>
    /// <param name="idField">Measurement key-based ID field name.</param>
    /// <returns>Measurement key from source metadata.</returns>
    /// <remarks>
    /// This function uses the <see cref="DataTable.Select(string)"/> function which uses a linear
    /// search algorithm that can be slow for large data sets, it is recommended that any results
    /// for calls to this function be cached to improve performance.
    /// </remarks>
    public static MeasurementKey KeyFromTag(this string pointTag, DataSet source, string table = DataSourceValue.MetadataTableName, string pointTagField = "PointTag", string signalIDField = "SignalID", string idField = "ID")
    {
        DataRow record = pointTag.RecordFromTag(source, table, pointTagField);

        if (record is null)
            return MeasurementKey.Undefined;

        try
        {
            return MeasurementKey.LookUpOrCreate(record[signalIDField].ToNonNullString(Guid.Empty.ToString()).ConvertToType<Guid>(), record[idField].ToString());
        }
        catch (Exception ex)
        {
            Logger.SwallowException(ex);
            return MeasurementKey.Undefined;
        }
    }

    /// <summary>
    /// Looks up measurement key and point tag from signal ID.
    /// </summary>
    /// <param name="signalID">Signal ID to lookup.</param>
    /// <param name="metadata">Source metadata.</param>
    /// <param name="table">Table to search.</param>
    /// <param name="signalIDField">Guid-based signal ID field name.</param>
    /// <param name="pointTagField">Point tag field name.</param>
    /// <param name="idField">Measurement key-based ID field name.</param>
    /// <returns>Measurement key and point tag from source metadata.</returns>
    /// <remarks>
    /// This function uses the <see cref="DataTable.Select(string)"/> function which uses a linear
    /// search algorithm that can be slow for large data sets, it is recommended that any results
    /// for calls to this function be cached to improve performance.
    /// </remarks>
    public static (MeasurementKey, string) KeyAndTagFromSignalID(this string signalID, DataSet metadata, string table = DataSourceValue.MetadataTableName, string signalIDField = "SignalID", string pointTagField = "PointTag", string idField = "ID")
    {
        DataRow record = signalID.RecordFromSignalID(metadata, table, signalIDField);
        return record?.KeyAndTagFromRecord(signalIDField, pointTagField, idField) ?? (MeasurementKey.Undefined, nameof(MeasurementKey.Undefined));
    }

    /// <summary>
    /// Looks up measurement key and point tag from record.
    /// </summary>
    /// <param name="record">Record with data values lookup.</param>
    /// <param name="signalIDField">Guid-based signal ID field name.</param>
    /// <param name="pointTagField">Point tag field name.</param>
    /// <param name="idField">Measurement key-based ID field name.</param>
    /// <returns>Measurement key and point tag from record.</returns>
    public static (MeasurementKey, string) KeyAndTagFromRecord(this DataRow record, string signalIDField = "SignalID", string pointTagField = "PointTag", string idField = "ID")
    {
        try
        {
            MeasurementKey key = MeasurementKey.LookUpOrCreate(record[signalIDField].ToNonNullString(Guid.Empty.ToString()).ConvertToType<Guid>(), record[idField].ToString());
            string pointTag = record[pointTagField].ToNonNullString(key.ToString());
            return (key, pointTag);
        }
        catch (Exception ex)
        {
            Logger.SwallowException(ex);
            return (MeasurementKey.Undefined, nameof(MeasurementKey.Undefined));
        }
    }

    /// <summary>
    /// Looks up metadata record from point tag.
    /// </summary>
    /// <param name="pointTag">Point tag to lookup.</param>
    /// <param name="metadata">Source metadata.</param>
    /// <param name="table">Table to search.</param>
    /// <param name="pointTagField">Point tag field name.</param>
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
    public static DataRow RecordFromTag(this string pointTag, DataSet metadata, string table = DataSourceValue.MetadataTableName, string pointTagField = "PointTag")
    {
        return GetMetadata(metadata, table, $"{pointTagField} = '{SplitAlias(pointTag, out string _)}'");
    }

    /// <summary>
    /// Looks up metadata record from signal ID.
    /// </summary>
    /// <param name="signalID">Signal ID, as string, to lookup.</param>
    /// <param name="metadata">Source metadata.</param>
    /// <param name="table">Table to search.</param>
    /// <param name="signalIDField">Signal ID field name.</param>
    /// <returns>Metadata record from source metadata for provided point tag.</returns>
    /// <remarks>
    /// This function uses the <see cref="DataTable.Select(string)"/> function which uses a linear
    /// search algorithm that can be slow for large data sets, it is recommended that any results
    /// for calls to this function be cached to improve performance.
    /// </remarks>
    public static DataRow RecordFromSignalID(this string signalID, DataSet metadata, string table = DataSourceValue.MetadataTableName, string signalIDField = "SignalID")
    {
        return GetMetadata(metadata, table, $"{signalIDField} = '{signalID}'");
    }

    /// <summary>
    /// Looks up metadata record from measurement key.
    /// </summary>
    /// <param name="key">Measurement key, as string, to lookup.</param>
    /// <param name="metadata">Source metadata.</param>
    /// <param name="table">Table to search.</param>
    /// <param name="idField">Measurement key-based ID field name.</param>
    /// <returns>Metadata record from source metadata for provided measurement key.</returns>
    public static DataRow RecordFromKey(this string key, DataSet metadata, string table = DataSourceValue.MetadataTableName, string idField = "ID")
    {
        return GetMetadata(metadata, table, $"{idField} = '{key}'");
    }

    /// <summary>
    /// Gets metadata record from source metadata.
    /// </summary>
    /// <param name="metadata">Source metadata.</param>
    /// <param name="table">Table to search.</param>
    /// <param name="expression">Expression to filter metadata.</param>
    /// <returns>Metadata record from source metadata, if expression is found; otherwise <c>null</c>.</returns>
    public static DataRow GetMetadata(DataSet metadata, string table, string expression)
    {
        try
        {
            DataRow[] filteredRows = metadata.Tables[table].Select(expression);

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

    /// <summary>
    /// Gets measurement key from signal ID.
    /// </summary>
    /// <param name="signalID">Signal ID to lookup.</param>
    /// <returns>
    /// Measurement key associated with specified signal ID, if found; otherwise <see cref="MeasurementKey.Undefined"/>.
    /// </returns>
    public static MeasurementKey KeyFromSignalID(string signalID)
    {
        return MeasurementKey.LookUpBySignalID(Guid.Parse(signalID));
    }

    /// <summary>
    /// Parses target as table and field name.
    /// </summary>
    /// <param name="target">Target to parse.</param>
    /// <returns>Target parsed as table and field name.</returns>
    public static (string tableName, string fieldName) ParseAsTableAndField<T>(this string target) where T : struct, IDataSourceValue
    {
        string[] parts = target.Split('.');

        return parts.Length switch
        {
            1 => (default(T).MetadataTableName, parts[0]),
            2 => (parts[0], parts[1]),
            _ => throw new InvalidOperationException($"Invalid target \"{target}\" encountered, expected format as \"FieldName\" or \"TableName.FieldName\".")
        };
    }

    /// <summary>
    /// Determines if metadata table is valid for the target data source value type.
    /// </summary>
    /// <param name="metadata">Source metadata.</param>
    /// <param name="tableName">Target table name.</param>
    /// <returns><c>true</c> if metadata table is valid for the specified data source value type instance; otherwise, <c>false</c>.</returns>
    public static bool MetadataTableIsValid<T>(this DataSet metadata, string tableName) where T : struct, IDataSourceValue
    {
        return default(T).MetadataTableIsValid(metadata, tableName);
    }

    /// <summary>
    /// Determines if metadata table is valid for the specified data source value type instance.
    /// </summary>
    /// <param name="instance">Data source value type instance.</param>
    /// <param name="metadata">Source metadata.</param>
    /// <param name="tableName">Target table name.</param>
    /// <returns><c>true</c> if metadata table is valid for the specified data source value type instance; otherwise, <c>false</c>.</returns>
    public static bool MetadataTableIsValid(this IDataSourceValue instance, DataSet metadata, string tableName)
    {
        string[] requiredFields = instance.RequiredMetadataFieldNames;
        return metadata.Tables.Contains(tableName) && requiredFields.All(field => metadata.Tables[tableName].Columns.Contains(field));
    }

    /// <summary>
    /// Gets metadata map for the specified target and selections.
    /// </summary>
    /// <param name="metadata">Source metadata.</param>
    /// <param name="rootTarget">Root target to use for metadata lookup.</param>
    /// <param name="queryParameters">Query parameters.</param>
    /// <returns>Mapped metadata for the specified target and selections.</returns>
    public static Dictionary<string, string> GetMetadataMap<T>(this DataSet metadata, string rootTarget, QueryParameters queryParameters) where T : struct, IDataSourceValue
    {
        return metadata.GetMetadataMap<T>(rootTarget, queryParameters.MetadataSelections);
    }

    /// <summary>
    /// Gets metadata map for the specified target and selections.
    /// </summary>
    /// <param name="metadata">Source metadata.</param>
    /// <param name="rootTarget">Root target to use for metadata lookup.</param>
    /// <param name="metadataSelections">Metadata selections.</param>
    /// <returns>Mapped metadata for the specified target and selections.</returns>
    public static Dictionary<string, string> GetMetadataMap<T>(this DataSet metadata, string rootTarget, IList<(string tableName, string[] fieldNames)> metadataSelections) where T : struct, IDataSourceValue
    {
        // Create a new dictionary to hold the metadata values
        Dictionary<string, string> metadataMap = new();

        // Return an empty dictionary if metadataSelection is null or empty
        if (metadataSelections?.Count == 0)
            return metadataMap;

        // Iterate through selections
        foreach ((string tableName, string[] fieldNames) in metadataSelections!)
        {
            DataRow row = default(T).LookupMetadata(metadata, tableName, rootTarget);

            if (row is null)
                continue;

            // Populate the entry dictionary with the metadata values
            foreach (string fieldName in fieldNames)
            {
                // Returned field name will not include table name if it is unique in the return set,
                // otherwise, field name will be formatted as "{tableName}.{fieldName}":
                if (metadataMap.ContainsKey(fieldName))
                    metadataMap[$"{tableName}.{fieldName}"] = row[fieldName].ToString();
                else
                    metadataMap[fieldName] = row[fieldName].ToString();
            }
        }

        return metadataMap;
    }

    /// <summary>
    /// Parses a user provided expression as tags.
    /// </summary>
    /// <param name="target">Target expression to parse.</param>
    /// <param name="metadata">Source metadata.</param>
    /// <returns>Point tags parsed from expression.</returns>
    public static string[] ParseExpressionAsTags<T>(this string target, DataSet metadata) where T : struct, IDataSourceValue
    {
        static Guid convertToGuid(object value) =>
            value.ToNonNullString(Guid.Empty.ToString()).ConvertToType<Guid>();

        string aliasTarget = target.SplitAlias(out string alias);

        MeasurementKey[] keys;
        
        // Attempt to parse expression as a filter expression first as this will provide target table name for tag to key lookups
        if (AdapterBase.ParseFilterExpression(aliasTarget, out string tableName, out string expression, out string sortField, out int takeCount))
        {
            keys = metadata.Tables[tableName]
                .Select(expression, sortField)
                .Take(takeCount)
                .Select(row => MeasurementKey.LookUpOrCreate(convertToGuid(row["SignalID"]), row["ID"].ToString()))
                .Where(key => key != MeasurementKey.Undefined)
                .ToArray();
        }
        else
        {
            // Fall back on standard tag expression parsing which will attempt to parse target as measurement keys or signal IDs
            keys = AdapterBase.ParseInputMeasurementKeys(metadata, false, aliasTarget, default(T).MetadataTableName);
        }

        if (string.IsNullOrWhiteSpace(tableName))
            tableName = default(T).MetadataTableName;

        if (!string.IsNullOrWhiteSpace(alias) && keys.Length == 1)
            return new[] { $"{alias}={keys[0].TagFromKey(metadata, tableName)}" };

        return keys.Select(key => key.TagFromKey(metadata, tableName)).ToArray();
    }
}