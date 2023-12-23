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
using GrafanaAdapters.GrafanaFunctionsCore;
using GSF.TimeSeries;

namespace GrafanaAdapters;

/// <summary>
/// Defines an interface for a data source value.
/// </summary>
public interface IDataSourceValue
{
    /// <summary>
    /// Gets default instance of <see cref="IDataSourceValue"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This should normally reference a static default value for the data source.
    /// </para>
    /// <para>
    /// Instance is used to access type methods in lieu of missing support for
    /// static abstract interface methods in .NET Framework.
    /// </para>
    /// </remarks>
    IDataSourceValue Default { get; }

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

    // TODO: JRC - when converting to .NET core, change interface methods to be static abstract

    /// <summary>
    /// Looks up metadata for the specified target.
    /// </summary>
    /// <param name="metadata">Metadata data set.</param>
    /// <param name="target">Target to lookup.</param>
    /// <returns>Filtered metadata rows for the specified target.</returns>
    DataRow[] LookupMetadata(DataSet metadata, string target);

    /// <summary>
    /// Gets the ID to target map for the specified metadata and targets along with any intermediate state.
    /// </summary>
    /// <param name="metadata">Source metadata.</param>
    /// <param name="targets">Targets to lookup.</param>
    /// <returns>Target map for the specified metadata and targets along with any intermediate state.</returns>
    /// <remarks>
    /// For a given data source type, this function will be called once per query to build a map of IDs to
    /// targets. Since this step involves the metadata lookups for targets, any intermediate state can be
    /// used with the <see cref="IDataSourceValue{T}.GetTargetDataSourceValueMap"/> function to avoid any
    /// redundant metadata lookups after the data query operation.
    /// </remarks>
    (Dictionary<ulong, string> targetMap, object state) GetIDTargetMap(DataSet metadata, string[] targets);
}

/// <summary>
/// Defines an interface for a typed data source value.
/// </summary>
/// <typeparam name="T">Target <see cref="IDataSourceValue"/> type.</typeparam>
public interface IDataSourceValue<T> : IDataSourceValue where T : IDataSourceValue
{
    /// <summary>
    /// Gets the target to data source value group mpa for the specified queried data values, metadata,
    /// query parameters, target map and any intermediate state.
    /// </summary>
    /// <param name="dataValues">Queried time-series data values.</param>
    /// <param name="metadata">Source metadata.</param>
    /// <param name="parameters">Query parameters.</param>
    /// <param name="targetMap">Target map.</param>
    /// <param name="state">Intermediate state.</param>
    /// <returns></returns>
    Dictionary<string, DataSourceValueGroup<T>> GetTargetDataSourceValueMap
    (
        List<DataSourceValue> dataValues,
        DataSet metadata,
        QueryParameters parameters, 
        Dictionary<ulong, string> targetMap, 
        object state
    );
}
