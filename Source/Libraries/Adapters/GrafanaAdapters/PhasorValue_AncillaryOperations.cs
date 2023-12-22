//******************************************************************************************************
//  PhasorValue_AncillaryOperations.cs - Gbtc
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

using GSF.TimeSeries.Adapters;
using GSF.TimeSeries;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using GrafanaAdapters.GrafanaFunctionsCore;

namespace GrafanaAdapters;

// IDataSourceValue implementation for PhasorValue
public partial struct PhasorValue : IDataSourceValue<PhasorValue>
{
    private static readonly PhasorValue s_default = default;

    private struct PhasorInfo
    {
        public string Label;
        public string Magnitude;
        public string Phase;
    };

    /// <inheritdoc />
    readonly IDataSourceValue IDataSourceValue.Default => s_default;

    /// <inheritdoc />
    readonly string IDataSourceValue.Target => MagnitudeTarget;

    /// <inheritdoc />
    readonly double IDataSourceValue.Time => Time;

    /// <inheritdoc />
    readonly double[] IDataSourceValue.TimeSeriesValue => new[] { Magnitude, Angle, Time };

    /// <inheritdoc />
    readonly MeasurementStateFlags IDataSourceValue.Flags => Flags;

    /// <inheritdoc />
    // TODO: JRC - this lookup is problematic, phasor labels are not unique, need to lookup with an associated device ID - how does this work from UI?
    public readonly DataRow[] LookupMetadata(DataSet metadata, string target)
    {
        return metadata?.Tables["Phasor"].Select($"Label = '{target}'") ?? Array.Empty<DataRow>();
    }

    /// <inheritdoc />
    readonly (Dictionary<ulong, string>, object) IDataSourceValue.GetIDTargetMap(DataSet metadata, string[] targets)
    {
        Dictionary<int, PhasorInfo> phasorTargets = new();
        Dictionary<ulong, string> targetMap = new();

        foreach (string targetLabel in targets)
        {
            if (targetLabel.StartsWith("FILTER ", StringComparison.OrdinalIgnoreCase) && AdapterBase.ParseFilterExpression(targetLabel.SplitAlias(out _), out string tableName, out string exp, out string sortField, out int takeCount))
            {
                foreach (DataRow row in metadata.Tables[tableName].Select(exp, sortField).Take(takeCount))
                {
                    int targetId = Convert.ToInt32(row["ID"]);

                    phasorTargets[Convert.ToInt32(targetId)] = new PhasorInfo
                    {
                        Label = row["Label"].ToString(),
                        Phase = "",
                        Magnitude = ""
                    };
                }

                continue;
            }

            // Get phasor ID
            DataRow[] phasorRows = LookupMetadata(metadata, targetLabel);

            if (phasorRows.Length == 0)
                throw new Exception($"Unable to find phasor label '{targetLabel}'");

            foreach (DataRow row in phasorRows)
            {
                int targetID = Convert.ToInt32(phasorRows[0]["ID"]);

                phasorTargets[targetID] = new PhasorInfo
                {
                    Label = row["Label"].ToString(),
                    Phase = "",
                    Magnitude = ""
                };

                DataRow[] measurementRows = metadata.Tables["ActiveMeasurements"].Select($"PhasorID = '{targetID}'");

                if (measurementRows.Length < 2)
                    throw new Exception($"Did not locate both magnitude and angle measurements for '{phasorTargets[targetID].Label}' with ID '{targetID}'");

                foreach (DataRow pointRow in measurementRows)
                {
                    ulong id = Convert.ToUInt64(pointRow["ID"].ToString().Split(':')[1]);
                    string pointTag = pointRow["PointTag"].ToString();

                    targetMap[id] = pointTag;

                    if (pointRow["SignalType"].ToString().EndsWith("PH"))
                    {
                        phasorTargets[targetID] = new PhasorInfo
                        {
                            Label = phasorTargets[targetID].Label,
                            Phase = pointTag,
                            Magnitude = phasorTargets[targetID].Magnitude,
                        };
                    }

                    if (pointRow["SignalType"].ToString().EndsWith("PM"))
                    {
                        phasorTargets[targetID] = new PhasorInfo
                        {
                            Label = phasorTargets[targetID].Label,
                            Magnitude = pointTag,
                            Phase = phasorTargets[targetID].Phase,
                        };
                    }
                }
            }
        }

        return (targetMap, phasorTargets);
    }

    /// <inheritdoc />
    Dictionary<string, DataSourceValueGroup<PhasorValue>> IDataSourceValue<PhasorValue>.GetTargetDataSourceValueMap
    (
        List<DataSourceValue> dataValues,
        DataSet metadata,
        QueryParameters parameters, 
        Dictionary<ulong, string> targetMap, 
        object state
    )
    {
        if (state is not Dictionary<int, PhasorInfo> phasorTargets)
            throw new NullReferenceException("Invalid state type or phasor targets are null");

        return phasorTargets.ToDictionary(target => target.Value.Label, target =>
        {
            IEnumerable<DataSourceValue> filteredMagnitudes = dataValues.Where(dataValue => dataValue.Target.Equals(target.Value.Magnitude));
            List<DataSourceValue> filteredPhases = dataValues.Where(dataValue => dataValue.Target.Equals(target.Value.Phase)).ToList();
            IEnumerable<PhasorValue> phasorValues = generatePhasorValues();

            return new DataSourceValueGroup<PhasorValue>
            {
                Target = $"{target.Value.Magnitude};{target.Value.Phase}",
                RootTarget = target.Value.Label,
                SourceTarget = parameters.SourceTarget,
                Source = phasorValues,
                DropEmptySeries = parameters.DropEmptySeries,
                refId = parameters.SourceTarget.refId,
                metadata = FunctionParser.GetMetadata<PhasorValue>(metadata, target.Value.Label, parameters.MetadataSelection)
            };

            List<PhasorValue> generatePhasorValues()
            {
                List<PhasorValue> values = new();
                int index = 0;

                foreach (DataSourceValue mag in filteredMagnitudes)
                {
                    if (index >= filteredPhases.Count)
                        continue;

                    PhasorValue phasor = new()
                    {
                        MagnitudeTarget = target.Value.Magnitude,
                        AngleTarget = target.Value.Phase,
                        Flags = mag.Flags,
                        Time = mag.Time,
                        Magnitude = mag.Value,
                        Angle = filteredPhases[index].Value
                    };

                    index++;

                    values.Add(phasor);
                }
                return values;
            }
        });

    }
}