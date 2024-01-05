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
using System.Linq;
using GrafanaAdapters.Functions;
using GrafanaAdapters.Model.Annotations;
using GSF.TimeSeries;

namespace GrafanaAdapters.DataSources;

// IDataSourceValue implementation for DataSourceValue
public partial struct DataSourceValue : IDataSourceValue<DataSourceValue>
{
    /// <inheritdoc />
    string IDataSourceValue.Target
    {
        readonly get => Target;
        init => Target = value;
    }

    /// <inheritdoc />
    double IDataSourceValue.Value
    {
        readonly get => Value;
        init => Value = value;
    }

    /// <inheritdoc />
    double IDataSourceValue.Time
    {
        readonly get => Time;
        init => Time = value;
    }

    /// <inheritdoc />
    MeasurementStateFlags IDataSourceValue.Flags
    {
        readonly get => Flags;
        init => Flags = value;
    }

    /// <inheritdoc />
    readonly double[] IDataSourceValue.TimeSeriesValue => new[] { Value, Time };

    /// <inheritdoc />
    public DataSourceValue TransposeCompute(Func<double, double> function)
    {
        return this with { Value = function(Value) };
    }

    /// <inheritdoc />
    public readonly DataRow[] LookupMetadata(DataSet metadata, string target)
    {
        // TODO: Cache this metadata lookup per target
        return metadata?.Tables["ActiveMeasurements"].Select($"PointTag = '{target}'") ?? Array.Empty<DataRow>();
    }

    /// <inheritdoc />
    readonly (Dictionary<ulong, string>, object) IDataSourceValue.GetIDTargetMap(DataSet metadata, HashSet<string> targetSet)
    {
        Dictionary<ulong, string> targetMap = new();

        // Reduce all targets down to a dictionary of point ID's mapped to point tags
        foreach (string target in targetSet)
        {
            // Check for point tag based target definition
            MeasurementKey key = TargetCache<MeasurementKey>.GetOrAdd(target, () => target.KeyFromTag(metadata));

            if (key == MeasurementKey.Undefined)
            {
                // Check for Guid based signal ID target definition
                (MeasurementKey, string) result = TargetCache<(MeasurementKey, string)>.GetOrAdd($"signalID@{target}", () => target.KeyAndTagFromSignalID(metadata));

                key = result.Item1;
                string pointTag = result.Item2;

                if (key == MeasurementKey.Undefined)
                {
                    // Check for measurement key based target definition
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

    /// <inheritdoc />
    DataSourceValueGroup<DataSourceValue> IDataSourceValue<DataSourceValue>.GetTargetDataSourceValueGroup
    (
        KeyValuePair<ulong, string> target,
        List<DataSourceValue> dataValues,
        DataSet metadata,
        QueryParameters queryParameters,
        object state
    )
    {
        // Extract data source values for the specified target - this is necessary since point tags are queried in bulk,
        // however, we use an enumerable to defer the actual filter processing to later in the pipeline
        IEnumerable<DataSourceValue> source = dataValues.Where(dataValue => dataValue.Target.Equals(target.Value));

        return new DataSourceValueGroup<DataSourceValue>
        {
            Target = target.Value,
            RootTarget = target.Value,
            SourceTarget = queryParameters.SourceTarget,
            Source = source,
            DropEmptySeries = queryParameters.DropEmptySeries,
            RefID = queryParameters.SourceTarget.refId,
            MetadataMap = metadata.GetMetadataMap<DataSourceValue>(target.Value, queryParameters)
        };
    }

    /// <inheritdoc />
    public readonly ParameterDefinition<IEnumerable<DataSourceValue>> DataSourceValuesParameterDefinition => s_dataSourceValuesParameterDefinition;

    private static readonly ParameterDefinition<IEnumerable<DataSourceValue>> s_dataSourceValuesParameterDefinition = DataSourceValuesParameterDefinition<DataSourceValue>();
}