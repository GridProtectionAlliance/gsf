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
using GSF.TimeSeries;
using System;
using System.Collections.Generic;
using System.Data;

namespace GrafanaAdapters.DataSources.BuiltIn;

// IDataSourceValue implementation for DataSourceValue
public partial struct DataSourceValue : IDataSourceValue<DataSourceValue>
{
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
        // These are fields as required by GetIDTargetMap() method
        "ID",       // Measurement key, e.g., PPA:101
        "SignalID", // Guid-based signal ID
        "PointTag"  // Point tag, e.g., GPA_SHELBY:FREQ
    };

    readonly Action<DataSet> IDataSourceValue.AugmentMetadata => null; // No augmentation needed

    /// <inheritdoc />
    public readonly DataRow LookupMetadata(DataSet metadata, string tableName, string target)
    {
        (DataRow, int) getRecordAndHashCode() =>
            (target.RecordFromTag(metadata, tableName), metadata.GetHashCode());

        string cacheKey = $"{nameof(DataSourceValue)}-{target}";

        (DataRow record, int hashCode) = TargetCache<(DataRow, int)>.GetOrAdd(cacheKey, getRecordAndHashCode);

        // If metadata hasn't changed, return cached record
        if (metadata.GetHashCode() == hashCode)
            return record;

        // Metadata has changed, remove cached record and re-query
        TargetCache<(DataRow, int)>.Remove(cacheKey);
        (record, _) = TargetCache<(DataRow, int)>.GetOrAdd(cacheKey, getRecordAndHashCode);

        return record;
    }

    readonly (Dictionary<ulong, string>, object) IDataSourceValue.GetIDTargetMap(DataSet metadata, HashSet<string> targetSet)
    {
        Dictionary<ulong, string> targetMap = new();

        // Reduce all targets down to a dictionary of data source point ID's mapped to point tags
        foreach (string target in targetSet)
        {
            // Check if target is already in the form of a point tag
            MeasurementKey key = TargetCache<MeasurementKey>.GetOrAdd(target, () => target.KeyFromTag(metadata));

            if (key == MeasurementKey.Undefined)
            {
                // Check if target is in the form of a Guid-based signal ID, e.g., {00000000-0000-0000-0000-000000000000}
                (key, string pointTag) = TargetCache<(MeasurementKey, string)>.GetOrAdd($"signalID@{target}", () => target.KeyAndTagFromSignalID(metadata));

                if (key == MeasurementKey.Undefined)
                {
                    // Check if target is in the form of a measurement key, e.g., PPA:101
                    (key, pointTag) = TargetCache<(MeasurementKey, string)>.GetOrAdd($"key@{target}", () =>
                    {
                        MeasurementKey.TryParse(target, out MeasurementKey parsedKey);
                        return (parsedKey, parsedKey.TagFromKey(metadata));
                    });

                    if (key != MeasurementKey.Undefined)
                        targetMap[key.ID] = pointTag;
                }
                else
                {
                    targetMap[key.ID] = pointTag;
                }
            }
            else
            {
                targetMap[key.ID] = target;
            }
        }

        // Return target map and null state
        return (targetMap, null);
    }

    readonly void IDataSourceValue<DataSourceValue>.AssignToTimeValueMap(DataSourceValue dataValue, SortedList<double, DataSourceValue> timeValueMap, object state)
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