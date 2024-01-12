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

using System;
using GrafanaAdapters.Functions;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using GSF;
using GSF.Diagnostics;
using GSF.TimeSeries;

namespace GrafanaAdapters.DataSources;

internal static class MetadataExtensions
{
    private static readonly LogPublisher s_log = Logger.CreatePublisher(typeof(MetadataExtensions), MessageClass.Component);
    private static readonly Regex s_aliasedTagExpression = new(@"^\s*(?<Identifier>[A-Z_][A-Z0-9_]*)\s*\=\s*(?<Expression>.+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

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
        DataRow record = GetMetadata(source, "ActiveMeasurements", $"ID = '{key}'");
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
    internal static MeasurementKey ToMeasurement(this string pointTag, DataSet source, string table, string pointTagField, string signalIDField)
    {
        DataRow record = pointTag.MetadataRecordFromPointTag(source, table, pointTagField);

        if (record is null)
            return MeasurementKey.Undefined;

        try
        {
            return MeasurementKey.LookUpOrCreate(record[signalIDField].ToNonNullString(Guid.Empty.ToString()).ConvertToType<Guid>(), record["ID"].ToString());
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
    /// <param name="metadata">Source metadata.</param>
    /// <param name="table">Table to search.</param>
    /// <param name="signalIDField">Signal ID field name.</param>
    /// <returns>Measurement key from source metadata.</returns>
    /// <remarks>
    /// This function uses the <see cref="DataTable.Select(string)"/> function which uses a linear
    /// search algorithm that can be slow for large data sets, it is recommended that any results
    /// for calls to this function be cached to improve performance.
    /// </remarks>
    internal static (MeasurementKey, string) KeyAndTagFromSignalID(this string signalID, DataSet metadata, string table, string signalIDField)
    {
        DataRow record = signalID.MetadataRecordFromSignalID(metadata, table, signalIDField);
        string pointTag = "Undefined";

        if (record is null)
            return (MeasurementKey.Undefined, pointTag);

        try
        {
            MeasurementKey key = MeasurementKey.LookUpOrCreate(record["SignalID"].ToNonNullString(Guid.Empty.ToString()).ConvertToType<Guid>(), record["ID"].ToString());
            pointTag = record["PointTag"].ToNonNullString(key.ToString());
            return (key, pointTag);
        }
        catch (Exception ex)
        {
            Logger.SwallowException(ex);
            return (MeasurementKey.Undefined, pointTag);
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
    internal static DataRow MetadataRecordFromPointTag(this string pointTag, DataSet metadata, string table, string pointTagField)
    {
        return GetMetadata(metadata, table, $"{pointTagField} = '{SplitAlias(pointTag, out string _)}'");
    }

    /// <summary>
    /// Looks up metadata record from signal ID.
    /// </summary>
    /// <param name="signalID">Signal ID to lookup.</param>
    /// <param name="metadata">Source metadata.</param>
    /// <param name="table">Table to search.</param>
    /// <param name="signalIDField">Signal ID field name.</param>
    /// <returns>Metadata record from source metadata for provided point tag.</returns>
    /// <remarks>
    /// This function uses the <see cref="DataTable.Select(string)"/> function which uses a linear
    /// search algorithm that can be slow for large data sets, it is recommended that any results
    /// for calls to this function be cached to improve performance.
    /// </remarks>
    internal static DataRow MetadataRecordFromSignalID(this string signalID, DataSet metadata, string table, string signalIDField)
    {
        return GetMetadata(metadata, table, $"{signalIDField} = '{signalID}'");
    }

    internal static DataRow GetMetadata(DataSet metadata, string table, string expression)
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

    internal static MeasurementKey GetMeasurementKeyFromSignalID(string signalID)
    {
        return MeasurementKey.LookUpBySignalID(Guid.Parse(signalID));
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
        return metadata.GetMetadataMap<T>(rootTarget, queryParameters.MetadataSelection);
    }

    /// <summary>
    /// Gets metadata map for the specified target and selections.
    /// </summary>
    /// <param name="metadata">Source metadata.</param>
    /// <param name="rootTarget">Root target to use for metadata lookup.</param>
    /// <param name="metadataSelection">Metadata selections.</param>
    /// <returns>Mapped metadata for the specified target and selections.</returns>
    public static Dictionary<string, string> GetMetadataMap<T>(this DataSet metadata, string rootTarget, Dictionary<string, List<string>> metadataSelection) where T : struct, IDataSourceValue
    {
        // Create a new dictionary to hold the metadata values
        Dictionary<string, string> metadataMap = new();

        // Return an empty dictionary if metadataSelection is null or empty
        if (metadataSelection?.Count == 0)
            return metadataMap;

        // Iterate through selections
        foreach (KeyValuePair<string, List<string>> entry in metadataSelection!)
        {
            List<string> values = entry.Value;
            DataRow[] rows = default(T).LookupMetadata(metadata, rootTarget);

            // Populate the entry dictionary with the metadata values
            foreach (string value in values)
            {
                string metadataValue = string.Empty;

                if (rows.Length > 0)
                    metadataValue = rows[0][value].ToString();

                metadataMap[value] = metadataValue;
            }
        }

        return metadataMap;
    }
}