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

using GSF.TimeSeries;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using GrafanaAdapters.GrafanaFunctionsCore;

namespace GrafanaAdapters;

// IDataSourceValue implementation for DataSourceValue
public partial struct DataSourceValue : IDataSourceValue<DataSourceValue>
{
    private static readonly DataSourceValue s_default = default;
    private static readonly ConcurrentDictionary<Type, IDataSourceValue> s_dataSourceValueTypeMap = new();
    private static readonly ConcurrentDictionary<string, IDataSourceValue> s_dataSourceValueTypeNameMap = new();

    /// <inheritdoc />
    readonly string IDataSourceValue.Target => Target;

    /// <inheritdoc />
    readonly double IDataSourceValue.Time => Time;

    /// <inheritdoc />
    readonly double[] IDataSourceValue.TimeSeriesValue => new[] { Value, Time };

    /// <inheritdoc />
    readonly MeasurementStateFlags IDataSourceValue.Flags => Flags;

    /// <inheritdoc />
    readonly IDataSourceValue IDataSourceValue.Default => s_default;

    /// <inheritdoc />
    public readonly DataRow[] LookupMetadata(DataSet metadata, string target)
    {
        return metadata?.Tables["ActiveMeasurements"].Select($"PointTag = '{target}'") ?? Array.Empty<DataRow>();
    }

    /// <inheritdoc />
    readonly (Dictionary<ulong, string>, object) IDataSourceValue.GetIDTargetMap(DataSet metadata, string[] targets)
    {
        Dictionary<ulong, string> targetMap = new();

        // Expand target set to include point tags for all parsed inputs
        HashSet<string> targetSet = new();

        foreach (string target in targets)
            targetSet.UnionWith(TargetCache<string[]>.GetOrAdd(target, () => FunctionParser.GetPointTags(target, metadata)));

        // Reduce all targets down to a dictionary of point ID's mapped to point tags
        foreach (string target in targetSet)
        {
            MeasurementKey key = TargetCache<MeasurementKey>.GetOrAdd(target, () => target.KeyFromTag(metadata));

            if (key == MeasurementKey.Undefined)
            {
                Tuple<MeasurementKey, string> result = TargetCache<Tuple<MeasurementKey, string>>.GetOrAdd($"signalID@{target}", () => target.KeyAndTagFromSignalID(metadata));

                key = result.Item1;
                string pointTag = result.Item2;

                if (key == MeasurementKey.Undefined)
                {
                    result = TargetCache<Tuple<MeasurementKey, string>>.GetOrAdd($"key@{target}", () =>
                    {
                        MeasurementKey.TryParse(target, out MeasurementKey parsedKey);

                        return new Tuple<MeasurementKey, string>(parsedKey, parsedKey.TagFromKey(metadata));
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
    Dictionary<string, DataSourceValueGroup<DataSourceValue>> IDataSourceValue<DataSourceValue>.GetTargetDataSourceValueMap
    (
        List<DataSourceValue> dataValues,
        DataSet metadata,
        QueryParameters parameters,
        Dictionary<ulong, string> targetMap,
        object state
    )
    {
        return targetMap.ToDictionary(kvp => kvp.Value, kvp =>
        {
            IEnumerable<DataSourceValue> filteredValues = dataValues.Where(dataValue => dataValue.Target.Equals(kvp.Value));

            return new DataSourceValueGroup<DataSourceValue>
            {
                Target = kvp.Value,
                RootTarget = kvp.Value,
                SourceTarget = parameters.SourceTarget,
                Source = filteredValues,
                DropEmptySeries = parameters.DropEmptySeries,
                RefID = parameters.SourceTarget.refId,
                MetadataMap = FunctionParser.GetMetadata<DataSourceValue>(metadata, kvp.Value, parameters.MetadataSelection)
            };
        });
    }

    /// <summary>
    /// Gets default instance of specified data source type.
    /// </summary>
    /// <typeparam name="T">data source type.</typeparam>
    /// <returns>Default instance of specified data source type.</returns>
    public static IDataSourceValue<T> Default<T>() where T : IDataSourceValue, new()
    {
        // Caching default instances of data source value types so expensive reflection is only done once
        return s_dataSourceValueTypeMap.GetOrAdd(typeof(T), _ =>
            typeof(IDataSourceValue).GetProperty("Default")!.GetValue(new T()) as IDataSourceValue) as IDataSourceValue<T>;
    }

    /// <summary>
    /// Gets default instance of specified data source type name.
    /// </summary>
    /// <param name="typeName">data source type name.</param>
    /// <returns>Default instance of specified data source type name.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="typeName"/> is <c>null</c>.</exception>
    public static IDataSourceValue Default(string typeName)
    {
        if (typeName is null)
            throw new ArgumentNullException(nameof(typeName));

        // Caching default instances of data source value types so expensive reflection is only done once
        return s_dataSourceValueTypeNameMap.GetOrAdd(typeName, _ =>
            typeof(IDataSourceValue).GetProperty("Default")!.GetValue(Activator.CreateInstance(GetLocalType(typeName))) as IDataSourceValue);
    }
}