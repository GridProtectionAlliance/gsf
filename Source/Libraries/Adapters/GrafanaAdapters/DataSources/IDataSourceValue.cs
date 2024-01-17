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
//  11/20/2023 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using GrafanaAdapters.DataSources.BuiltIn;
using GrafanaAdapters.Functions;
using GSF.TimeSeries;
using System;
using System.Collections.Generic;
using System.Data;

namespace GrafanaAdapters.DataSources;

/// <summary>
/// Defines an interface for a data source value.
/// </summary>
/// <remarks>
/// Implementations of this interface must be structs.
/// </remarks>
public interface IDataSourceValue
{
    /// <summary>
    /// Gets the query target, e.g., a point-tag.
    /// </summary>
    /// <remarks>
    /// If data source value has multiple targets, this should be the primary target.
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
    /// Gets timestamp, in Unix epoch milliseconds, of data source value.
    /// </summary>
    double Time { get; init; }

    /// <summary>
    /// Gets measurement state and quality flags of data source value.
    /// </summary>
    MeasurementStateFlags Flags { get; init; }

    /// <summary>
    /// Gets time-series array values of data source value, e.g., [Value, Time].
    /// </summary>
    /// <remarks>
    /// To ensure data will work with Grafana data source, all values should
    /// precede single time value. Time is always the last value in the array.
    /// Time value should be in Unix epoch milliseconds.
    /// </remarks>
    double[] TimeSeriesValue { get; }

    /// <summary>
    /// Gets the format definition of a time-series array value, e.g., ["Value", "Time"].
    /// </summary>
    /// <remarks>
    /// These string values are used to define field value names that are used in
    /// a <see cref="TimeSeriesValue"/> result.
    /// </remarks>
    string[] TimeSeriesValueDefinition { get; }

    /// <summary>
    /// Gets the desired load order for the data source value type.
    /// </summary>
    /// <remarks>
    /// This value is used to determine the order in which data source value types are
    /// presented to the user in the Grafana data source configuration UI. If multiple
    /// data source value types have the same load order, they will use a secondary
    /// sort order, i.e., alphabetically by type name. Ideally, these values should be
    /// as unique as possible to avoid any secondary sorting.
    /// </remarks>
    int LoadOrder { get; }

    /// <summary>
    /// Gets the name of the primary metadata table for the data source.
    /// </summary>
    string MetadataTableName { get; }

    /// <summary>
    /// Gets the names of the required metadata fields for the data source.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This defines a list of the required metadata fields for a table in the data source. If any of
    /// these are missing, the data source table will not be available for use. This list should at
    /// least include key field names for the <see cref="MetadataTableName"/> that may be needed by
    /// the <see cref="GetIDTargetMap"/> or <see cref="IDataSourceValue{T}.AssignToTimeValueMap"/>
    /// functions. For example, in order to use a table named 'ActiveMeasurements', the required
    /// metadata field names might be: 'ID', 'SignalID', and 'PointTag'.
    /// </para>
    /// <para>
    /// Note that system generally assumes that a 'PointTag' field exists in the metadata table and
    /// to use some of the metadata functions, e.g., parsing a filter expression, then an 'ID' and
    /// 'SignalID' field will also be required. In these cases, pick a primary key field that will
    /// mirror to these required fields for needed values. These do not need to be marked as required
    /// metadata fields, but they will need to be included in the table as available fields.
    /// </para>
    /// </remarks>
    string[] RequiredMetadataFieldNames { get; }

    /// <summary>
    /// Gets function that augments metadata for the data source, or <c>null</c>
    /// if metadata augmentation is not needed.
    /// </summary>
    /// <remarks>
    /// Returned function augments metadata for the target data source value type. Metadata is a shared
    /// resource and may be long-lived, as such method should only add to existing metadata for needs of
    /// the data source value type and only perform needed augmentation once per provided instance of
    /// metadata. For example, if a new table is added to the metadata for the data source value type,
    /// first step should be to check if table already exists.
    /// </remarks>
    Action<DataSet> AugmentMetadata { get; }

    /// <summary>
    /// Looks up metadata record for the specified target.
    /// </summary>
    /// <param name="metadata">Metadata data set.</param>
    /// <param name="tableName">Table name to search.</param>
    /// <param name="target">Target to lookup.</param>
    /// <returns>Filtered metadata row for the specified target.</returns>
    /// <remarks>
    /// Implementations should cache metadata lookups for performance.
    /// </remarks>
    DataRow LookupMetadata(DataSet metadata, string tableName, string target);

    /// <summary>
    /// Gets the ID to target map for the specified metadata and targets along with any intermediate state.
    /// </summary>
    /// <param name="metadata">Source metadata.</param>
    /// <param name="targetSet">Target set to query.</param>
    /// <returns>Target map for the specified metadata and targets along with any intermediate state.</returns>
    /// <remarks>
    /// For a given data source type, this function will be called once per query to build a map of IDs to
    /// targets. Since this step involves the metadata lookups for targets, any intermediate state can be
    /// used with the <see cref="IDataSourceValue{T}.AssignToTimeValueMap"/> function to avoid any
    /// redundant metadata lookups after the data query operation.
    /// </remarks>
    (Dictionary<ulong, string> targetMap, object state) GetIDTargetMap(DataSet metadata, HashSet<string> targetSet);

    /// <summary>
    /// Gets the data source value type index.
    /// </summary>
    int DataTypeIndex { get; }
}

/// <summary>
/// Defines an interface for a typed data source value.
/// </summary>
/// <typeparam name="T">Target <see cref="IDataSourceValue"/> type.</typeparam>
public interface IDataSourceValue<T> : IDataSourceValue, IComparable<T>, IEquatable<T> where T : struct, IDataSourceValue
{
    /// <summary>
    /// Assign queried data source value to time-value map.
    /// </summary>
    /// <param name="dataValue">Queried data source value.</param>
    /// <param name="timeValueMap">Time-value map for specified <paramref name="dataValue"/>.</param>
    /// <param name="state">Optional intermediate state.</param>
    /// <remarks>
    /// Provided time-value map is specific to the queried data source value, by target, and is keyed by Unix
    /// epoch milliseconds timestamp. This function is used to assign the queried data source value to the
    /// time-value map. If the data source value type has multiple fields, this function will be called once
    /// per each field in the data source value for a given timestamp.
    /// </remarks>
    void AssignToTimeValueMap(DataSourceValue dataValue, SortedList<double, T> timeValueMap, object state);

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
    /// specified function operation to all value fields in the data source.
    /// </remarks>
    T TransposeCompute(Func<double, double> function);
}