//******************************************************************************************************
//  IDataSourceValue.cs - Gbtc
//
//  Copyright © 2023, Grid Protection Alliance.  All Rights Reserved.
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
//  11/20/2023 - Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Collections.Generic;
using System.Data;
using GrafanaAdapters.FunctionParsing;
using GSF.TimeSeries;

namespace GrafanaAdapters.DataSources;

/// <summary>
/// Defines an interface for a data source value.
/// </summary>
public interface IDataSourceValue
{
    /// <summary>
    /// Gets the query target, e.g., a point-tag.
    /// </summary>
    /// <remarks>
    /// If data source value has multiple targets, this should be the primary target.
    /// </remarks>
    string Target { get; }

    /// <summary>
    /// Gets timestamp, in Unix epoch milliseconds, of data source value.
    /// </summary>
    double Time { get; }

    /// <summary>
    /// Gets time-series array values of data source value, e.g., [Value, Time].
    /// </summary>
    double[] TimeSeriesValue { get; }

    /// <summary>
    /// Gets flags of data source value.
    /// </summary>
    MeasurementStateFlags Flags { get; }

    /// <summary>
    /// Looks up metadata for the specified target.
    /// </summary>
    /// <param name="metadata">Metadata data set.</param>
    /// <param name="target">Target to lookup.</param>
    /// <returns>Filtered metadata rows for the specified target.</returns>
    // TODO: JRC - results of this function should be cached for performance (caching internal to function is OK)
    DataRow[] LookupMetadata(DataSet metadata, string target);

    /// <summary>
    /// Gets the ID to target map for the specified metadata and targets along with any intermediate state.
    /// </summary>
    /// <param name="metadata">Source metadata.</param>
    /// <param name="targetSet">Target set to query.</param>
    /// <returns>Target map for the specified metadata and targets along with any intermediate state.</returns>
    /// <remarks>
    /// For a given data source type, this function will be called once per query to build a map of IDs to
    /// targets. Since this step involves the metadata lookups for targets, any intermediate state can be
    /// used with the <see cref="IDataSourceValue{T}.GetTargetDataSourceValueGroup"/> function to avoid any
    /// redundant metadata lookups after the data query operation.
    /// </remarks>
    (Dictionary<ulong, string> targetMap, object state) GetIDTargetMap(DataSet metadata, HashSet<string> targetSet);
}

/// <summary>
/// Defines an interface for a typed data source value.
/// </summary>
/// <typeparam name="T">Target <see cref="IDataSourceValue"/> type.</typeparam>
public interface IDataSourceValue<T> : IDataSourceValue where T : struct, IDataSourceValue
{
    /// <summary>
    /// Gets the target to data source value group mpa for the specified queried data values, metadata,
    /// query parameters, target map and any intermediate state.
    /// </summary>
    /// <param name="target">Target for data source value group.</param>
    /// <param name="dataValues">Queried time-series data values.</param>
    /// <param name="metadata">Source metadata.</param>
    /// <param name="queryParameters">Query parameters.</param>
    /// <param name="state">Intermediate state.</param>
    /// <returns>Data source value group for the specified target.</returns>
    DataSourceValueGroup<T> GetTargetDataSourceValueGroup
    (
        KeyValuePair<ulong, string> target,
        List<DataSourceValue> dataValues,
        DataSet metadata,
        QueryParameters queryParameters, 
        object state
    );
}
