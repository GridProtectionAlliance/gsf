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

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using GrafanaAdapters.Functions;
using GrafanaAdapters.Model.Annotations;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;

namespace GrafanaAdapters.DataSources;

// IDataSourceValue implementation for PhasorValue
public partial struct PhasorValue : IDataSourceValue<PhasorValue>
{
    private struct PhasorInfo
    {
        public string Label;
        public string Magnitude;
        public string Phase;
    };

    /// <inheritdoc />
    readonly string IDataSourceValue.Target => MagnitudeTarget;

    /// <inheritdoc />
    readonly double IDataSourceValue.Time => Time;

    /// <inheritdoc />
    readonly double[] IDataSourceValue.TimeSeriesValue => new[] { Magnitude, Angle, Time };

    /// <inheritdoc />
    readonly MeasurementStateFlags IDataSourceValue.Flags => Flags;

    /// <inheritdoc />
    // TODO: JRC - ERROR - this lookup is problematic, phasor labels are not unique, need to lookup with an associated device ID
    // or device acronym. Actually, even device phasor labels are not unique, can have same label for different phases. So you
    // need a device acronym (or ID) and the phasor ID, e.g., <DeviceAcronym>!<PhasorID>. How does this even work from UI?
    // Do you select a device and then a phasor? I would think a simpler solution would be to filter ActiveMeasurements by phasor
    // types, perhaps just magnitudes, then use then just use that measurement's unique point tag for metadata lookups. You could
    // then use the PhasorID to lookup associated angle measurement as well as phasor metadata, e.g., label, phase, etc. as needed.
    public readonly DataRow[] LookupMetadata(DataSet metadata, string target)
    {
        // TODO: Cache this metadata lookup per target
        return metadata?.Tables["Phasor"].Select($"Label = '{target}'") ?? Array.Empty<DataRow>();
    }

    /// <inheritdoc />
    readonly (Dictionary<ulong, string>, object) IDataSourceValue.GetIDTargetMap(DataSet metadata, HashSet<string> targetSet)
    {
        Dictionary<int, PhasorInfo> phasorTargets = new();
        Dictionary<ulong, string> targetMap = new();

        // TODO: JRC - based on logic issue with LookupMetadata, the following needs to be verified and reworked
        foreach (string targetLabel in targetSet)
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
    DataSourceValueGroup<PhasorValue> IDataSourceValue<PhasorValue>.GetTargetDataSourceValueGroup
    (
        KeyValuePair<ulong, string> target,
        List<DataSourceValue> dataValues,
        DataSet metadata,
        QueryParameters queryParameters,
        object state
    )
    {
        if (state is not Dictionary<int, PhasorInfo> phasorTargets)
            throw new NullReferenceException("Invalid state type or phasor targets are null");

        // TODO: JRC - validate that this phasor info lookup is correct
        if (!phasorTargets.TryGetValue(Convert.ToInt32(target.Key), out PhasorInfo phasorInfo))
            throw new NullReferenceException($"Unable to find phasor info for target '{target.Value}'");

        IEnumerable<DataSourceValue> filteredMagnitudes = dataValues.Where(dataValue => dataValue.Target.Equals(phasorInfo.Magnitude));
        List<DataSourceValue> filteredPhases = dataValues.Where(dataValue => dataValue.Target.Equals(phasorInfo.Phase)).ToList();
        IEnumerable<PhasorValue> phasorValues = generatePhasorValues();

        return new DataSourceValueGroup<PhasorValue>
        {
            Target = $"{phasorInfo.Magnitude};{phasorInfo.Phase}",
            RootTarget = phasorInfo.Label,
            SourceTarget = queryParameters.SourceTarget,
            Source = phasorValues,
            DropEmptySeries = queryParameters.DropEmptySeries,
            RefID = queryParameters.SourceTarget.refId,
            MetadataMap = metadata.GetMetadataMap<PhasorValue>(phasorInfo.Label, queryParameters)
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
                    MagnitudeTarget = phasorInfo.Magnitude,
                    AngleTarget = phasorInfo.Phase,
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
    }

    /// <inheritdoc />
    public readonly ParameterDefinition<IEnumerable<PhasorValue>> DataSourceValuesParameterDefinition => s_dataSourceValuesParameterDefinition;

    private static readonly ParameterDefinition<IEnumerable<PhasorValue>> s_dataSourceValuesParameterDefinition = DataSourceValuesParameterDefinition<PhasorValue>();
}