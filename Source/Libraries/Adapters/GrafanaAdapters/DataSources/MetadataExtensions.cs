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

using GrafanaAdapters.Functions;
using System.Collections.Generic;
using System.Data;

namespace GrafanaAdapters.DataSources;

internal static class MetadataExtensions
{
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