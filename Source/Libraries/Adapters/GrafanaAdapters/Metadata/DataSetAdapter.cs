//******************************************************************************************************
//  DataSetAdapter.cs - Gbtc
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
//  01/16/2024 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using GrafanaAdapters.DataSourceValueTypes;
using System.Data;

namespace GrafanaAdapters.Metadata;

/// <summary>
/// Represents an adapter that holds the Grafana data source's metadata
/// and is used to augment data source on demand.
/// </summary>
public class DataSetAdapter
{
    private readonly DataSet m_metadata;

    private DataSetAdapter(DataSet metadata)
    {
        m_metadata = metadata;
    }

    /// <summary>
    /// Gets the Grafana data source metadata, augmented as needed for the target data
    /// source value type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Data source value type for metadata augmentation.</typeparam>
    /// <returns>
    /// Grafana data source metadata, augmented as needed for the target data source
    /// value type <typeparamref name="T"/>.
    /// </returns>
    public DataSet GetAugmentedDataSet<T>() where T : struct, IDataSourceValueType<T>
    {
        return GetAugmentedDataSet(default(T));
    }

    /// <summary>
    /// Gets the Grafana data source metadata, augmented as needed for the target data
    /// source value type instance <paramref name="value"/>.
    /// </summary>
    /// <param name="value">Instance of data source value type to use for metadata augmentation.</param>
    /// <returns>
    /// Grafana data source metadata, augmented as needed for the target data source
    /// </returns>
    public DataSet GetAugmentedDataSet(IDataSourceValueType value)
    {
        // Handle metadata augmentation for the target data source value type
        value.AugmentMetadata?.Invoke(m_metadata);

        return m_metadata;
    }

    /// <summary>
    /// Implicitly converts a <see cref="DataSet"/> to a <see cref="DataSetAdapter"/>.
    /// </summary>
    /// <param name="source">Source <see cref="DataSet"/> to convert.</param>
    public static implicit operator DataSetAdapter(DataSet source)
    {
        return new DataSetAdapter(source);
    }
}