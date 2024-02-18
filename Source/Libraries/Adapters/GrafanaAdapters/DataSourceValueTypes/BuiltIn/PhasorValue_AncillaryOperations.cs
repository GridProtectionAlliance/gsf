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

using GrafanaAdapters.Metadata;
using GrafanaAdapters.Model.Common;
using GSF;
using GSF.Collections;
using GSF.Data;
using GSF.Diagnostics;
using GSF.FuzzyStrings;
using GSF.TimeSeries;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

namespace GrafanaAdapters.DataSourceValueTypes.BuiltIn;

// IDataSourceValueType implementation for PhasorValue
public partial struct PhasorValue : IDataSourceValueType<PhasorValue>
{
    string IDataSourceValueType.Target
    {
        readonly get => Target;
        init => Target = value;
    }

    double IDataSourceValueType.Value
    {
        readonly get => PrimaryValueTarget == PhasorValueTarget.Magnitude ? Magnitude : Angle;
        init
        {
            if (PrimaryValueTarget == PhasorValueTarget.Magnitude)
                Magnitude = value;
            else
                Angle = value;
        }
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

    readonly double[] IDataSourceValueType.TimeSeriesValue => new[] { Magnitude, Angle, Time };

    readonly string[] IDataSourceValueType.TimeSeriesValueDefinition => new[] { nameof(Magnitude), nameof(Angle), nameof(Time) };

    readonly int IDataSourceValueType.ValueIndex => (int)PrimaryValueTarget;

    /// <inheritdoc />
    public readonly int CompareTo(PhasorValue other)
    {
        int result = Magnitude.CompareTo(other.Magnitude);

        if (result != 0)
            return result;

        result = Angle.CompareTo(other.Angle);
        return result == 0 ? Time.CompareTo(other.Time) : result;
    }

    /// <inheritdoc />
    public readonly bool Equals(PhasorValue other)
    {
        return CompareTo(other) == 0;
    }

    /// <inheritdoc />
    public readonly PhasorValue TransposeCompute(Func<double, double> function)
    {
        return this with
        {
            Magnitude = function(Magnitude),
            Angle = function(Angle)
        };
    }

    readonly int IDataSourceValueType.LoadOrder => 1;

    readonly string IDataSourceValueType.MetadataTableName => MetadataTableName;

    readonly string[] IDataSourceValueType.RequiredMetadataFieldNames => new[]
    {
        "MagnitudeID",       // <string> Measurement key representing magnitude, e.g., PPA:101
        "AngleID",           // <string> Measurement key representing angle, e.g., PPA:102
        "MagnitudeSignalID", //  <Guid>  Signal ID representing magnitude
        "AngleSignalID",     //  <Guid>  Signal ID representing angle
        "MagnitudePointTag", // <string> Point tag representing magnitude, e.g, GPA_SHELBY:BUS1.MAG
        "AnglePointTag",     // <string> Point tag representing angle, e.g, GPA_SHELBY:BUS1.ANG
        "PointTag"           // <string> Point tag representing phasor, e.g, GPA_SHELBY:BUS1
    };

    readonly Action<DataSet> IDataSourceValueType.AugmentMetadata => AugmentMetadata;

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
        return
        (
            record["PointTag"].ToString(), new[]
            {
                (record.KeyFromRecord("MagnitudeID", "MagnitudeSignalID"), record["MagnitudePointTag"].ToString()),
                (record.KeyFromRecord("AngleID", "AngleSignalID"), record["AnglePointTag"].ToString())
            }
        );
    }

    readonly DataRow IDataSourceValueType.RecordFromKey(MeasurementKey key, DataSet metadata)
    {
        string keyName = key.ToString();

        return keyName.RecordFromKey(metadata, MetadataTableName, "MagnitudeID") ??
               keyName.RecordFromKey(metadata, MetadataTableName, "AngleID");
    }

    readonly int IDataSourceValueType.DataTypeIndex => TypeIndex;

    readonly void IDataSourceValueType<PhasorValue>.AssignToTimeValueMap(DataSourceValue dataSourceValue, SortedList<double, PhasorValue> timeValueMap, DataSet metadata)
    {
        string pointTag = dataSourceValue.ID.pointTag;
        string target = dataSourceValue.ID.target;

        // Lookup queried data source value target in 'PhasorValues' metadata
        (string phasorTarget, string magnitudeTarget, string angleTarget, bool isMagnitudeValue) = TargetCache<(string, string, string, bool)>.GetOrAdd(pointTag, () =>
        {
            // Lookup queried data source target as a point tag for either magnitude or angle
            DataRow record = pointTag.RecordFromTag(metadata, MetadataTableName, "MagnitudePointTag") ??
                             pointTag.RecordFromTag(metadata, MetadataTableName, "AnglePointTag");

            Debug.Assert(record is not null, $"Unexpected null metadata record for '{target}'");

            string phasorTarget = record["PointTag"].ToString();
            string magnitudeTarget = record["MagnitudePointTag"].ToString();
            string angleTarget = record["AnglePointTag"].ToString();
            bool isMagnitudeValue = pointTag.Equals(magnitudeTarget, StringComparison.OrdinalIgnoreCase);

            // Since the record lookup results will be the same for both magnitude and angle, we pre-cache the results for the other target
            TargetCache<(string, string, string, bool)>.GetOrAdd(isMagnitudeValue ? angleTarget : magnitudeTarget, () => (phasorTarget, magnitudeTarget, angleTarget, !isMagnitudeValue));

            return (phasorTarget, magnitudeTarget, angleTarget, isMagnitudeValue);
        });

        Debug.Assert(phasorTarget is not null, $"Unexpected null phasor target for '{target}'");
        Debug.Assert(magnitudeTarget is not null, $"Unexpected null magnitude target for '{target}'");
        Debug.Assert(angleTarget is not null, $"Unexpected null angle target for '{target}'");

        // See if phasor value already exists in time-value map
        if (timeValueMap.TryGetValue(dataSourceValue.Time, out PhasorValue phasorValue))
        {
            // Update phasor field values from queried data source value
            timeValueMap[dataSourceValue.Time] = phasorValue with
            {
                Magnitude = isMagnitudeValue ? dataSourceValue.Value : phasorValue.Magnitude,
                Angle = !isMagnitudeValue ? dataSourceValue.Value : phasorValue.Angle,
                // Assign actual measurement flags only when both values have been received
                Flags = isMagnitudeValue && double.IsNaN(phasorValue.Angle) || !isMagnitudeValue && double.IsNaN(phasorValue.Magnitude) ?
                    phasorValue.Flags : dataSourceValue.Flags
            };
        }
        else
        {
            // Create new phasor value from queried data source value
            timeValueMap.Add(dataSourceValue.Time, new PhasorValue
            {
                Target = phasorTarget,
                MagnitudeTarget = magnitudeTarget,
                AngleTarget = angleTarget,
                Magnitude = isMagnitudeValue ? dataSourceValue.Value : double.NaN,
                Angle = !isMagnitudeValue ? dataSourceValue.Value : double.NaN,
                Time = dataSourceValue.Time,
                // Until both values are received, set flags to suspect data
                Flags = MeasurementStateFlags.SuspectData
            });
        }
    }

    /// <summary>
    /// Gets the type index for <see cref="PhasorValue"/>.
    /// </summary>
    public static readonly int TypeIndex = DataSourceValueTypeCache.GetTypeIndex(nameof(PhasorValue));

    /// <summary>
    /// Update phasor value primary target to operate on angle values.
    /// </summary>
    /// <param name="dataValue">Source phasor value.</param>
    /// <returns>Phasor value updated to operate on angle values.</returns>
    public static PhasorValue AngleAsTarget(PhasorValue dataValue) => dataValue with
    {
        PrimaryValueTarget = PhasorValueTarget.Angle
    };

    // Augments metadata for PhasorValue data source
    private static void AugmentMetadata(DataSet metadata)
    {
        const string EventName = $"{nameof(PhasorValue)} Metadata Augmentation";

        // Check if phasor metadata augmentation has already been performed for this dataset
        if (metadata.Tables.Contains(MetadataTableName))
            return;

        lock (typeof(PhasorValue))
        {
            // Check again after lock in case another thread already performed augmentation
            if (metadata.Tables.Contains(MetadataTableName))
                return;

            try
            {
                s_log.Publish(MessageLevel.Info, EventName, $"Starting metadata augmentation for {nameof(PhasorValue)} data source value type...");
                long startTime = DateTime.UtcNow.Ticks;

                // Extract phasor rows from active measurements table in current metadata
                DataTable activeMeasurements = metadata.Tables[MeasurementValue.MetadataTableName];
                DataRow[] phasorRows = activeMeasurements.Select("PhasorID IS NOT NULL", "PhasorID ASC");

                // Group phase angle and magnitudes together by phasor ID
                Dictionary<int, (DataRow magnitude, DataRow angle)> phasorTargets = new();

                foreach (DataRow row in phasorRows)
                {
                    int phasorID = Convert.ToInt32(row["PhasorID"]);
                    (DataRow magnitude, DataRow angle) = phasorTargets.GetOrAdd(phasorID, _ => (null, null));

                    string signalType = row["SignalType"].ToString().Trim();

                    if (signalType.EndsWith("PHA", StringComparison.OrdinalIgnoreCase))
                        angle = row;
                    else if (signalType.EndsWith("PHM", StringComparison.OrdinalIgnoreCase))
                        magnitude = row;

                    phasorTargets[phasorID] = (magnitude, angle);
                }

                // Create new metadata table for phasor values - this becomes a filterable data source table that can be queried, for example:
                // FILTER TOP 20 PhasorValues WHERE Type = 'V' AND Phase = 'A' AND Label LIKE '%BUS%' AND BaseKV >= 230
                DataTable phasorValues = metadata.Tables.Add(MetadataTableName);

                // Add columns to phasor metadata table
                phasorValues.Columns.Add("Device", typeof(string));

                // These are standard required fields for metadata lookup functions,
                // especially as related to AdapterBase.ParseFilterExpression
                phasorValues.Columns.Add("PointTag", typeof(string));   // Unique point tag for phasor
                phasorValues.Columns.Add("ID", typeof(string));         // Mapped to magnitude ID
                phasorValues.Columns.Add("SignalID", typeof(Guid));     // Mapped to magnitude SignalID

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
                phasorValues.Columns.Add("Phase", typeof(char));
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

                    string magnitudePointTag = null, anglePointTag = null;

                    try
                    {
                        // Create a new row that will reference phasor magnitude and angle metadata
                        DataRow phasorRow = phasorValues.NewRow();

                        magnitudePointTag = magnitude["PointTag"].ToString();
                        anglePointTag = angle["PointTag"].ToString();

                        // Find overlapping point tag name that will become primary phasor point tag
                        string pointTag = magnitudePointTag.LongestCommonSubstring(anglePointTag);

                        // Remove any trailing non-alphanumeric characters from point tag
                        while (pointTag.Length > 0 && !char.IsLetterOrDigit(pointTag[pointTag.Length - 1]))
                            pointTag = pointTag.Substring(0, pointTag.Length - 1);

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
                        phasorRow["Type"] = magnitude["PhasorType"].ToString().Trim()[0];
                        phasorRow["Phase"] = magnitude["Phase"].ToString().Trim()[0];
                        phasorRow["BaseKV"] = magnitude["BaseKV"];
                        phasorRow["Longitude"] = Convert.ToDecimal(magnitude["Longitude"]);
                        phasorRow["Latitude"] = Convert.ToDecimal(magnitude["Latitude"]);
                        phasorRow["Company"] = magnitude["Company"];
                        phasorRow["UpdatedOn"] = magnitude["UpdatedOn"];

                        // Use magnitude values for standard field values
                        phasorRow["ID"] = phasorRow["MagnitudeID"];
                        phasorRow["SignalID"] = phasorRow["MagnitudeSignalID"];

                        phasorValues.Rows.Add(phasorRow);
                    }
                    catch (Exception ex)
                    {
                        s_log.Publish(MessageLevel.Error, EventName, $"Failed while attempting to add augmented metadata row for {nameof(PhasorValue)} data source value type (magnitudePointTag = '{magnitudePointTag}' / anglePointTag = '{anglePointTag}'): {ex.Message}", exception: ex);
                    }
                }

                string elapsedTime = new TimeSpan(DateTime.UtcNow.Ticks - startTime).ToElapsedTimeString(3);
                s_log.Publish(MessageLevel.Info, EventName, $"Completed metadata augmentation for {nameof(PhasorValue)} data source value type: added {phasorValues.Rows.Count:N0} rows to '{MetadataTableName}' table in {elapsedTime}.");
            }
            catch (Exception ex)
            {
                s_log.Publish(MessageLevel.Error, EventName, $"Failed while attempting to augment metadata for {nameof(PhasorValue)} data source value type: {ex.Message}", exception: ex);
            }
        }
    }

    private static readonly LogPublisher s_log = Logger.CreatePublisher(typeof(PhasorValue), MessageClass.Component);
}