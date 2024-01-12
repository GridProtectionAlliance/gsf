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

using System;
using System.Collections.Generic;
using System.Data;
using GrafanaAdapters.Functions;
using GSF.TimeSeries;
using static GrafanaAdapters.Functions.Common;

namespace GrafanaAdapters.DataSources;

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

    /// <inheritdoc />
    public int CompareTo(DataSourceValue other)
    {
        int result = Value.CompareTo(other.Value);

        return result == 0 ? Time.CompareTo(other.Time) : result;
    }

    /// <inheritdoc />
    public bool Equals(DataSourceValue other)
    {
        return CompareTo(other) == 0;
    }

    /// <inheritdoc />
    public DataSourceValue TransposeCompute(Func<double, double> function)
    {
        return this with { Value = function(Value) };
    }

    /// <inheritdoc />
    public DataSet UpdateMetadata(DataSet metadata)
    {
        return metadata;
    }

    /// <inheritdoc />
    public readonly DataRow[] LookupMetadata(DataSet metadata, string target)
    {
        // TODO: Cache this metadata lookup per targetValues
        return metadata?.Tables["ActiveMeasurements"].Select($"PointTag = '{target}'") ?? Array.Empty<DataRow>();
    }

    readonly (Dictionary<ulong, string>, object) IDataSourceValue.GetIDTargetMap(DataSet metadata, HashSet<string> targetSet)
    {
        Dictionary<ulong, string> targetMap = new();

        // Reduce all targets down to a dictionary of point ID's mapped to point tags
        foreach (string target in targetSet)
        {
            // Check for point tag based targetValues definition
            MeasurementKey key = TargetCache<MeasurementKey>.GetOrAdd(target, () => target.ToMeasurement(metadata, "ActiveMeasurements", "PointTag", "SignalID"));

            if (key == MeasurementKey.Undefined)
            {
                // Check for Guid based signal ID targetValues definition
                (MeasurementKey, string) result = TargetCache<(MeasurementKey, string)>.GetOrAdd($"signalID@{target}", () => target.KeyAndTagFromSignalID(metadata, "ActiveMeasurements", "SignalID"));

                key = result.Item1;
                string pointTag = result.Item2;

                if (key == MeasurementKey.Undefined)
                {
                    // Check for measurement key based targetValues definition
                    result = TargetCache<(MeasurementKey, string)>.GetOrAdd($"key@{target}", () =>
                    {
                        MeasurementKey.TryParse(target, out MeasurementKey parsedKey);
                        return (parsedKey, parsedKey.TagFromKey(metadata));
                    });

                    key = result.Item1;
                    pointTag = result.Item2;

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

        return (targetMap, null);
    }

    void IDataSourceValue<DataSourceValue>.AssignValueToTargetList(DataSourceValue dataValue, List<DataSourceValue> targetValues, object state)
    {
        targetValues.Add(dataValue);
    }

    /// <inheritdoc />
    public readonly ParameterDefinition<IAsyncEnumerable<DataSourceValue>> DataSourceValuesParameterDefinition => s_dataSourceValuesParameterDefinition;

    private static readonly ParameterDefinition<IAsyncEnumerable<DataSourceValue>> s_dataSourceValuesParameterDefinition = DataSourceValuesParameterDefinition<DataSourceValue>();
}