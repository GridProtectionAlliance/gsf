//******************************************************************************************************
//  MeasurementValue_AncillaryOperations.cs - Gbtc
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

using GrafanaAdapters.Metadata;
using GrafanaAdapters.Model.Common;
using GSF.TimeSeries;
using System;
using System.Collections.Generic;
using System.Data;

namespace GrafanaAdapters.DataSourceValueTypes.BuiltIn;

// IDataSourceValueType implementation for MeasurementValue
public partial struct MeasurementValue : IDataSourceValueType<MeasurementValue>
{
    /// <summary>
    /// Data point index for value.
    /// </summary>
    public const int ValueIndex = 0;

    /// <summary>
    /// Data point index for time.
    /// </summary>
    public const int TimeIndex = 1;

    string IDataSourceValueType.Target
    {
        readonly get => Target;
        init => Target = value;
    }

    double IDataSourceValueType.Value
    {
        readonly get => Value;
        init => Value = value;
    }

    double IDataSourceValueType.Time
    {
        readonly get => Time;
        init => Time = value;
    }

    MeasurementStateFlags IDataSourceValueType.Flags
    {
        readonly get => Flags;
        init => Flags = value;
    }

    readonly double[] IDataSourceValueType.TimeSeriesValue => new[] { Value, Time };

    readonly string[] IDataSourceValueType.TimeSeriesValueDefinition => new[] { nameof(Value), nameof(Time) };

    readonly int IDataSourceValueType.ValueIndex => ValueIndex;

    /// <inheritdoc />
    public readonly int CompareTo(MeasurementValue other)
    {
        int result = Value.CompareTo(other.Value);

        return result == 0 ? Time.CompareTo(other.Time) : result;
    }

    /// <inheritdoc />
    public readonly bool Equals(MeasurementValue other)
    {
        return CompareTo(other) == 0;
    }

    /// <inheritdoc />
    public readonly MeasurementValue TransposeCompute(Func<double, double> function)
    {
        return this with { Value = function(Value) };
    }

    readonly int IDataSourceValueType.LoadOrder => 0;

    readonly string IDataSourceValueType.MetadataTableName => MetadataTableName;

    readonly string[] IDataSourceValueType.RequiredMetadataFieldNames => new[]
    {
        "ID",       // <string> Measurement key, e.g., PPA:101
        "SignalID", //  <Guid>  Signal ID
        "PointTag"  // <string> Point tag, e.g., GPA_SHELBY:FREQ
    };

    readonly Action<DataSet> IDataSourceValueType.AugmentMetadata => null; // No augmentation needed

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

    readonly TargetIDSet IDataSourceValueType.GetTargetIDSet(DataRow record)
    {
        // A target ID set is: (target, (measurementKey, pointTag)[])
        // For the simple MeasurementValue functionality the target is the point tag
        string pointTag = record["PointTag"].ToString();
        return (pointTag, new[] { (record.KeyFromRecord(), pointTag) });
    }

    readonly DataRow IDataSourceValueType.RecordFromKey(MeasurementKey key, DataSet metadata)
    {
        return key.ToString().RecordFromKey(metadata);
    }

    readonly int IDataSourceValueType.DataTypeIndex => TypeIndex;

    readonly void IDataSourceValueType<MeasurementValue>.AssignToTimeValueMap(DataSourceValue dataSourceValue, SortedList<double, MeasurementValue> timeValueMap, DataSet metadata)
    {
        timeValueMap[dataSourceValue.Time] = new MeasurementValue
        {
            Target = dataSourceValue.ID.target,
            Value = dataSourceValue.Value,
            Time = dataSourceValue.Time,
            Flags = dataSourceValue.Flags
        };
    }

    /// <summary>
    /// Gets the type index for <see cref="MeasurementValue"/>.
    /// </summary>
    public static readonly int TypeIndex = DataSourceValueTypeCache.GetTypeIndex(nameof(MeasurementValue));
}