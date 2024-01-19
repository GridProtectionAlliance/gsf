//******************************************************************************************************
//  DataSourceValue_AncillaryOperations.cs - Gbtc
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
//  12/14/2023 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using GrafanaAdapters.Functions;
using GrafanaAdapters.Metadata;
using GSF.TimeSeries;
using System;
using System.Collections.Generic;
using System.Data;

namespace GrafanaAdapters.DataSources.BuiltIn;

// IDataSourceValue implementation for DataSourceValue
public partial struct DataSourceValue : IDataSourceValue<DataSourceValue>
{
    /// <summary>
    /// Data point index for value.
    /// </summary>
    public const int ValueIndex = 0;

    /// <summary>
    /// Data point index for time.
    /// </summary>
    public const int TimeIndex = 1;

    string IDataSourceValue.Target
    {
        readonly get => Target;
        init => Target = value;
    }

    double IDataSourceValue.Value
    {
        readonly get => Value;
        init => Value = value;
    }

    double IDataSourceValue.Time
    {
        readonly get => Time;
        init => Time = value;
    }

    MeasurementStateFlags IDataSourceValue.Flags
    {
        readonly get => Flags;
        init => Flags = value;
    }

    readonly double[] IDataSourceValue.TimeSeriesValue => new[] { Value, Time };

    readonly string[] IDataSourceValue.TimeSeriesValueDefinition => new[] { nameof(Value), nameof(Time) };

    /// <inheritdoc />
    public readonly int CompareTo(DataSourceValue other)
    {
        int result = Value.CompareTo(other.Value);

        return result == 0 ? Time.CompareTo(other.Time) : result;
    }

    /// <inheritdoc />
    public readonly bool Equals(DataSourceValue other)
    {
        return CompareTo(other) == 0;
    }

    /// <inheritdoc />
    public readonly DataSourceValue TransposeCompute(Func<double, double> function)
    {
        return this with { Value = function(Value) };
    }

    readonly int IDataSourceValue.LoadOrder => 0;

    readonly string IDataSourceValue.MetadataTableName => MetadataTableName;

    readonly string[] IDataSourceValue.RequiredMetadataFieldNames => new[]
    {
        // These are fields as required by local GetIDTargetMap() method
        "ID",       // <string> Measurement key, e.g., PPA:101
        "SignalID", //  <Guid>  Signal ID
        "PointTag"  // <string> Point tag, e.g., GPA_SHELBY:FREQ
    };

    readonly Action<DataSet> IDataSourceValue.AugmentMetadata => null; // No augmentation needed

    /// <inheritdoc />
    public readonly DataRow LookupMetadata(DataSet metadata, string tableName, string target)
    {
        (DataRow, int) getRecordAndHashCode() =>
            (target.RecordFromTag(metadata, tableName), metadata.GetHashCode());

        string cacheKey = $"{TypeIndex}:{target}";

        (DataRow record, int hashCode) = TargetCache<(DataRow, int)>.GetOrAdd(cacheKey, getRecordAndHashCode);

        // If metadata hasn't changed, return cached record
        if (metadata.GetHashCode() == hashCode)
            return record;

        // Metadata has changed, remove cached record and re-query
        TargetCache<(DataRow, int)>.Remove(cacheKey);
        (record, _) = TargetCache<(DataRow, int)>.GetOrAdd(cacheKey, getRecordAndHashCode);

        return record;
    }

    readonly TargetIDSet IDataSourceValue.GetTargetIDSet(DataRow record)
    {
        // A target ID set is: (target, (measurementKey, pointTag)[])
        // For the simple DataSourceValue functionality the target is the point tag
        string pointTag = record["PointTag"].ToString();
        return (pointTag, new[] { (record.KeyFromRecord(), pointTag) });
    }

    readonly DataRow IDataSourceValue.RecordFromKey(MeasurementKey key, DataSet metadata)
    {
        return key.ToString().RecordFromKey(metadata);
    }

    readonly int IDataSourceValue.DataTypeIndex => TypeIndex;

    readonly void IDataSourceValue<DataSourceValue>.AssignToTimeValueMap(string pointTag, DataSourceValue dataValue, SortedList<double, DataSourceValue> timeValueMap, DataSet metadata)
    {
        timeValueMap[dataValue.Time] = dataValue;
    }

    /// <inheritdoc />
    public readonly ParameterDefinition<IAsyncEnumerable<DataSourceValue>> DataSourceValuesParameterDefinition => s_dataSourceValuesParameterDefinition;

    private static readonly ParameterDefinition<IAsyncEnumerable<DataSourceValue>> s_dataSourceValuesParameterDefinition = Common.DataSourceValuesParameterDefinition<DataSourceValue>();

    /// <summary>
    /// Gets the type index for <see cref="DataSourceValue"/>.
    /// </summary>
    public static readonly int TypeIndex = DataSourceValueCache.GetTypeIndex(nameof(DataSourceValue));
}