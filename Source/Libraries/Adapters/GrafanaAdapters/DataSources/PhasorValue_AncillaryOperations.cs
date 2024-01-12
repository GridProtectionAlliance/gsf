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
using GSF.Collections;
using GSF.Data;
using GSF.FuzzyStrings;
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

    string IDataSourceValue.Target
    {
        readonly get => PrimaryTarget == PhasorValueTarget.Magnitude ? MagnitudeTarget : AngleTarget;
        init
        {
            if (PrimaryTarget == PhasorValueTarget.Magnitude)
                MagnitudeTarget = value;
            else
                AngleTarget = value;
        }
    }

    double IDataSourceValue.Value
    {
        readonly get => PrimaryTarget == PhasorValueTarget.Magnitude ? Magnitude : Angle;
        init
        {
            if (PrimaryTarget == PhasorValueTarget.Magnitude)
                Magnitude = value;
            else
                Angle = value;
        }
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

    readonly double[] IDataSourceValue.TimeSeriesValue => new[] { Magnitude, Angle, Time };

    /// <inheritdoc />
    public int CompareTo(PhasorValue other)
    {
        int result = Magnitude.CompareTo(other.Magnitude);

        if (result == 0)
        {
            result = Angle.CompareTo(other.Angle);
            return result == 0 ? Time.CompareTo(other.Time) : result;
        }

        return result;
    }

    /// <inheritdoc />
    public bool Equals(PhasorValue other)
    {
        return CompareTo(other) == 0;
    }

    /// <inheritdoc />
    public PhasorValue TransposeCompute(Func<double, double> function)
    {
        return this with
        {
            Magnitude = function(Magnitude),
            Angle = function(Angle)
        };
    }

    /// <summary>
    /// Update phasor value primary target to operate on angle components.
    /// </summary>
    /// <param name="dataValue">Source phasor value.</param>
    /// <returns>Phasor value updated to operate on angle components.</returns>
    public static PhasorValue AngleAsTarget(PhasorValue dataValue) => dataValue with
    {
        PrimaryTarget = PhasorValueTarget.Angle
    };

    /// <inheritdoc />
    public DataSet UpdateMetadata(DataSet metadata)
    {
        // Extract phasor rows from active measurements table in current metadata
        DataTable activeMeasurements = metadata.Tables["ActiveMeasurements"];
        DataRow[] phasorRows = activeMeasurements.Select("PhasorID IS NOT NULL", "PhasorID ASC");

        // Group phase angle and magnitudes together by phasor ID
        Dictionary<int, (DataRow magnitude, DataRow angle)> phasorTargets = new();

        foreach (DataRow row in phasorRows)
        {
            int phasorID = Convert.ToInt32(row["PhasorID"]);
            (DataRow magnitude, DataRow angle) = phasorTargets.GetOrAdd(phasorID, _ => (null, null));

            string signalType = row["SignalType"].ToString();

            if (signalType.EndsWith("PHA"))
                angle = row;
            else if (signalType.EndsWith("PHM"))
                magnitude = row;

            phasorTargets[phasorID] = (magnitude, angle);
        }

        // Create a new data set for phasor metadata
        DataSet phasorMetadata = new("PhasorMetadata");

        // Create new metadata table for phasor values
        DataTable phasorValues = phasorMetadata.Tables.Add("PhasorValues");

        // Add columns to phasor metadata table
        phasorValues.Columns.Add("Device", typeof(string));
        phasorValues.Columns.Add("PointTag", typeof(string));
        phasorValues.Columns.Add("MagnitudePointTag", typeof(string));
        phasorValues.Columns.Add("AnglePointTag", typeof(string));
        phasorValues.Columns.Add("MagnitudeID", typeof(string));
        phasorValues.Columns.Add("AngleID", typeof(string));
        phasorValues.Columns.Add("MagnitudeSignalID", typeof(Guid));
        phasorValues.Columns.Add("AngleSignalID", typeof(Guid));
        phasorValues.Columns.Add("MagnitudeSignalReference", typeof(string));
        phasorValues.Columns.Add("AngleSignalReference", typeof(string));
        phasorValues.Columns.Add("Label", typeof(string));
        phasorValues.Columns.Add("Type", typeof(char));
        phasorValues.Columns.Add("Phase", typeof(string));
        phasorValues.Columns.Add("BaseKV", typeof(int));
        phasorValues.Columns.Add("Longitude", typeof(decimal));
        phasorValues.Columns.Add("Latitude", typeof(decimal));
        phasorValues.Columns.Add("Company", typeof(string));
        phasorValues.Columns.Add("UpdatedOn", typeof(DateTime));

        // Copy phasor metadata from magnitude and angle rows in active measurements table
        foreach ((DataRow magnitude, DataRow angle) in phasorTargets.Values)
        {
            if (magnitude is null || angle is null)
                continue;

            // Create a new row that will reference phasor magnitude and angle metadata
            DataRow phasorRow = phasorValues.NewRow();

            string magnitudePointTag = magnitude["PointTag"].ToString();
            string anglePointTag = angle["PointTag"].ToString();

            // Find overlapping point tag name that will become primary phasor point tag
            string pointTag = magnitudePointTag.LongestCommonSubstring(anglePointTag);

            // Copy in specific magnitude and angle phasor metadata, default to magnitude metadata for common values
            phasorRow["Device"] = magnitude["Device"];
            phasorRow["PointTag"] = pointTag;
            phasorRow["MagnitudePointTag"] = magnitudePointTag;
            phasorRow["AnglePointTag"] = anglePointTag;
            phasorRow["MagnitudeID"] = magnitude["ID"];
            phasorRow["AngleID"] = angle["ID"];
            phasorRow["MagnitudeSignalID"] = magnitude.ConvertGuidField("SignalID");
            phasorRow["AngleSignalID"] = angle.ConvertGuidField("SignalID");
            phasorRow["MagnitudeSignalReference"] = magnitude["SignalReference"];
            phasorRow["AngleSignalReference"] = angle["SignalReference"];
            phasorRow["Label"] = magnitude["PhasorLabel"];
            phasorRow["Type"] = magnitude["PhasorType"].ToString()[0];
            phasorRow["Phase"] = magnitude["Phase"];
            phasorRow["BaseKV"] = magnitude["BaseKV"];
            phasorRow["Longitude"] = Convert.ToDecimal(magnitude["Longitude"]);
            phasorRow["Latitude"] = Convert.ToDecimal(magnitude["Latitude"]);
            phasorRow["Company"] = magnitude["Company"];
            phasorRow["UpdatedOn"] = magnitude["UpdatedOn"];

            phasorValues.Rows.Add(phasorRow);
        }

        return phasorMetadata;
    }

    /// <inheritdoc />
    public readonly DataRow[] LookupMetadata(DataSet metadata, string target)
    {
        // TODO: Cache this metadata lookup per target
        return metadata?.Tables["PhasorValues"].Select($"PointTag = '{target}'") ?? Array.Empty<DataRow>();
    }

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
                            Magnitude = phasorTargets[targetID].Magnitude
                        };
                    }

                    if (pointRow["SignalType"].ToString().EndsWith("PM"))
                    {
                        phasorTargets[targetID] = new PhasorInfo
                        {
                            Label = phasorTargets[targetID].Label,
                            Magnitude = pointTag,
                            Phase = phasorTargets[targetID].Phase
                        };
                    }
                }
            }
        }

        return (targetMap, phasorTargets);
    }

    void IDataSourceValue<PhasorValue>.AssignValueToTargetList(DataSourceValue dataValue, List<PhasorValue> targetValues, object state)
    {
        // TODO: JRC - associate same queried angle measurement values to their its associated magnitudes - only add a single grouped pair to target values collection
    }

    //DataSourceValueGroup<PhasorValue> IDataSourceValue<PhasorValue>.GetTargetDataSourceValueGroup
    //(
    //    KeyValuePair<ulong, string> target,
    //    IAsyncEnumerable<DataSourceValue> dataValues,
    //    DataSet metadata,
    //    QueryParameters queryParameters,
    //    object state,
    //    CancellationToken cancellationToken
    //)
    //{
    //    //if (state is not Dictionary<int, PhasorInfo> phasorTargets)
    //    //    throw new NullReferenceException("Invalid state type or phasor targets are null");

    //    //if (!phasorTargets.TryGetValue(Convert.ToInt32(target.Key), out PhasorInfo phasorInfo))
    //    //    throw new NullReferenceException($"Unable to find phasor info for target '{target.Value}'");

    //    //DataSourceValue[] dataSourceValues = await dataValues.ToArrayAsync(cancellationToken);
    //    //IEnumerable<DataSourceValue> filteredMagnitudes = dataSourceValues.Where(dataValue => dataValue.Target.Equals(phasorInfo.Magnitude));
    //    //DataSourceValue[] filteredPhases = dataSourceValues.Where(dataValue => dataValue.Target.Equals(phasorInfo.Phase)).ToArray();
    //    //IEnumerable<PhasorValue> phasorValues = generatePhasorValues();

    //    //return new DataSourceValueGroup<PhasorValue>
    //    //{
    //    //    Target = $"{phasorInfo.Magnitude};{phasorInfo.Phase}",
    //    //    RootTarget = phasorInfo.Label,
    //    //    SourceTarget = queryParameters.SourceTarget,
    //    //    Source = phasorValues.ToAsyncEnumerable(),
    //    //    DropEmptySeries = queryParameters.DropEmptySeries,
    //    //    RefID = queryParameters.SourceTarget.refId,
    //    //    MetadataMap = metadata.GetMetadataMap<PhasorValue>(phasorInfo.Label, queryParameters)
    //    //};

    //    //List<PhasorValue> generatePhasorValues()
    //    //{
    //    //    List<PhasorValue> values = new();
    //    //    int index = 0;

    //    //    foreach (DataSourceValue mag in filteredMagnitudes)
    //    //    {
    //    //        if (index >= filteredPhases.Length)
    //    //            continue;

    //    //        PhasorValue phasor = new()
    //    //        {
    //    //            MagnitudeTarget = phasorInfo.Magnitude,
    //    //            AngleTarget = phasorInfo.Phase,
    //    //            Flags = mag.Flags,
    //    //            Time = mag.Time,
    //    //            Magnitude = mag.Value,
    //    //            Angle = filteredPhases[index].Value
    //    //        };

    //    //        index++;

    //    //        values.Add(phasor);
    //    //    }
    //    //    return values;
    //    //}
    //}

    /// <inheritdoc />
    public readonly ParameterDefinition<IAsyncEnumerable<PhasorValue>> DataSourceValuesParameterDefinition => s_dataSourceValuesParameterDefinition;

    private static readonly ParameterDefinition<IAsyncEnumerable<PhasorValue>> s_dataSourceValuesParameterDefinition = Common.DataSourceValuesParameterDefinition<PhasorValue>();
}