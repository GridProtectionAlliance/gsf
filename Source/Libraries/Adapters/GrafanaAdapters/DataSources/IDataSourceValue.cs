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

using System;
using System.Collections.Generic;
using System.Data;
using GrafanaAdapters.Functions;
using GSF.TimeSeries;

namespace GrafanaAdapters.DataSources;

/// <summary>
/// Defines an interface for a data source value.
/// </summary>
public interface IDataSourceValue
{
    /// <summary>
    /// Gets the query targetValues, e.g., a point-tag.
    /// </summary>
    /// <remarks>
    /// If data source value has multiple targets, this should be the primary targetValues.
    /// </remarks>
    string Target { get; init; }

    /// <summary>
    /// Gets the value of data source value.
    /// </summary>
    /// <remarks>
    /// If data source has more than one value, this should be the primary value.
    /// </remarks>
    double Value { get; init; }

    /// <summary>
    /// Gets or sets timestamp, in Unix epoch milliseconds, of data source value.
    /// </summary>
    double Time { get; init; }

    /// <summary>
    /// Gets flags of data source value.
    /// </summary>
    MeasurementStateFlags Flags { get; init; }

    /// <summary>
    /// Gets time-series array values of data source value, e.g., [Value, Time].
    /// </summary>
    double[] TimeSeriesValue { get; }

    /// <summary>
    /// Looks up metadata for the specified targetValues.
    /// </summary>
    /// <param name="metadata">Metadata data set.</param>
    /// <param name="target">Target to lookup.</param>
    /// <returns>Filtered metadata rows for the specified targetValues.</returns>
    // TODO: JRC - results of this function should be cached for performance (caching internal to function is OK)
    DataRow[] LookupMetadata(DataSet metadata, string target);

    /// <summary>
    /// Gets the ID to targetValues map for the specified metadata and targets along with any intermediate state.
    /// </summary>
    /// <param name="metadata">Source metadata.</param>
    /// <param name="targetSet">Target set to query.</param>
    /// <returns>Target map for the specified metadata and targets along with any intermediate state.</returns>
    /// <remarks>
    /// For a given data source type, this function will be called once per query to build a map of IDs to
    /// targets. Since this step involves the metadata lookups for targets, any intermediate state can be
    /// used with the <see cref="IDataSourceValue{T}.AssignValueToTargetList"/> function to avoid any
    /// redundant metadata lookups after the data query operation.
    /// </remarks>
    (Dictionary<ulong, string> targetMap, object state) GetIDTargetMap(DataSet metadata, HashSet<string> targetSet);
}

/// <summary>
/// Defines an interface for a typed data source value.
/// </summary>
/// <typeparam name="T">Target <see cref="IDataSourceValue"/> type.</typeparam>
public interface IDataSourceValue<T> : IDataSourceValue, IComparable<T>, IEquatable<T> where T : struct, IDataSourceValue
{
    /// <summary>
    /// Assign queried data source value to target values list.
    /// </summary>
    /// <param name="dataValue"></param>
    /// <param name="targetValues"></param>
    /// <param name="state"></param>
    void AssignValueToTargetList(DataSourceValue dataValue, List<T> targetValues, object state);

    /// <summary>
    /// Gets the parameter definition for data source values.
    /// </summary>
    ParameterDefinition<IAsyncEnumerable<T>> DataSourceValuesParameterDefinition { get; }

    /// <summary>
    /// Executes provided function for data source fields, applying the results
    /// to a copy of the data source value and returns the new result.
    /// </summary>
    /// <param name="function">Function to compute.</param>
    /// <returns>Computed result.</returns>
    /// <remarks>
    /// This function is used to compute a new data source value, applying the
    /// values to all fields in the data source.
    /// </remarks>
    T TransposeCompute(Func<double, double> function);
}