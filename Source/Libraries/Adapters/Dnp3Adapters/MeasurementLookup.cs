//******************************************************************************************************
//  MeasurementLookup.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  10/05/2012 - Adam Crain
//       Generated original version of source code.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************
// ReSharper disable InconsistentNaming

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using Automatak.DNP3.Interface;
using GSF;
using GSF.Collections;
using GSF.Diagnostics;
using GSF.TimeSeries;
using GSF.TimeSeries.Data;
using Measurement = GSF.TimeSeries.Measurement;

namespace DNP3Adapters;

/// <summary>
/// Helper class that converts measurements and provides a lookup capability.
/// </summary>
internal class MeasurementLookup
{
    private readonly Dictionary<uint, Mapping> m_binaryMap = [];
    private readonly Dictionary<uint, Mapping> m_analogMap = [];
    private readonly Dictionary<uint, Mapping> m_counterMap = [];
    private readonly Dictionary<uint, Mapping> m_frozenCounterMap = [];
    private readonly Dictionary<uint, Mapping> m_controlStatusMap = [];
    private readonly Dictionary<uint, Mapping> m_setpointStatusMap = [];
    private readonly Dictionary<uint, Mapping> m_doubleBitBinaryMap = [];
    private readonly Dictionary<MeasurementKey, MeasurementKey> m_tagQualityMap = [];

    public Func<DataSet> GetDataSource { get; init; }

    public bool MapQualityToStateFlags { get; init; }

    public bool AddQualityToMeasurementOutputs { get; init; }

    public Regex TagMatchRegex { get; init; }

    public string QualityTagSuffix { get; init; }

    public MeasurementLookup(MeasurementMap map)
    {
        map.binaryMap.ForEach(mapping => m_binaryMap.Add(mapping.dnpIndex, mapping));
        map.analogMap.ForEach(mapping => m_analogMap.Add(mapping.dnpIndex, mapping));
        map.counterMap.ForEach(mapping => m_counterMap.Add(mapping.dnpIndex, mapping));
        map.frozenCounterMap.ForEach(mapping => m_frozenCounterMap.Add(mapping.dnpIndex, mapping));
        map.controlStatusMap.ForEach(mapping => m_controlStatusMap.Add(mapping.dnpIndex, mapping));
        map.setpointStatusMap.ForEach(mapping => m_setpointStatusMap.Add(mapping.dnpIndex, mapping));
        map.doubleBitBinaryMap.ForEach(mapping => m_doubleBitBinaryMap.Add(mapping.dnpIndex, mapping));
    }

    public void Lookup(Binary measurement, ushort index, Action<IMeasurement> action)
    {
        GenericLookup(measurement, index, m_binaryMap, ConvertBinary, action);
    }

    public void Lookup(DoubleBitBinary measurement, ushort index, Action<IMeasurement> action)
    {
        GenericLookup(measurement, index, m_doubleBitBinaryMap, ConvertDoubleBitBinary, action);
    }

    public void Lookup(Analog measurement, ushort index, Action<IMeasurement> action)
    {
        GenericLookup(measurement, index, m_analogMap, ConvertAnalog, action);
    }

    public void Lookup(Counter measurement, ushort index, Action<IMeasurement> action)
    {
        GenericLookup(measurement, index, m_counterMap, ConvertCounter, action);
    }

    public void Lookup(FrozenCounter measurement, ushort index, Action<IMeasurement> action)
    {
        GenericLookup(measurement, index, m_frozenCounterMap, ConvertFrozenCounter, action);
    }

    public void Lookup(BinaryOutputStatus measurement, ushort index, Action<IMeasurement> action)
    {
        GenericLookup(measurement, index, m_controlStatusMap, ConvertBinaryOutputStatus, action);
    }

    public void Lookup(AnalogOutputStatus measurement, ushort index, Action<IMeasurement> action)
    {
        GenericLookup(measurement, index, m_setpointStatusMap, ConvertAnalogOutputStatus, action);
    }

    private Measurement ConvertBinary(Binary measurement, uint id, string source)
    {
        return new Measurement
        {
            Metadata = MeasurementKey.LookUpOrCreate(source, id).Metadata,
            Value = measurement.Value ? 1.0 : 0.0,
            Timestamp = measurement.Timestamp.Value,
            StateFlags = deriveStateFlags()
        };

        MeasurementStateFlags deriveStateFlags()
        {
            if (!MapQualityToStateFlags)
                return MeasurementStateFlags.Normal;

            Flags qualityFlags = measurement.Quality;
            MeasurementStateFlags stateFlags = MapCommonStateFlags(qualityFlags.Value);

            if (qualityFlags.IsSet(BinaryQuality.CHATTER_FILTER))
                stateFlags |= MeasurementStateFlags.SuspectData;

            if (qualityFlags.IsSet(BinaryQuality.RESERVED))
                stateFlags |= MeasurementStateFlags.UserDefinedFlag1;

            if (qualityFlags.IsSet(BinaryQuality.STATE))
                stateFlags |= MeasurementStateFlags.AlarmHigh;

            return stateFlags;
        }
    }

    private Measurement ConvertDoubleBitBinary(DoubleBitBinary measurement, uint id, string source)
    {
        Measurement convertedMeasurement = new()
        {
            Metadata = MeasurementKey.LookUpOrCreate(source, id).Metadata,
            Timestamp = measurement.Timestamp.Value,
            StateFlags = deriveStateFlags(),
            Value = measurement.Value switch
            {
                DoubleBit.INDETERMINATE => 0.0D,
                DoubleBit.DETERMINED_OFF => 1.0D,
                DoubleBit.DETERMINED_ON => 2.0D,
                _ => 3.0D
            }
        };

        return convertedMeasurement;

        MeasurementStateFlags deriveStateFlags()
        {
            if (!MapQualityToStateFlags)
                return MeasurementStateFlags.Normal;

            Flags qualityFlags = measurement.Quality;
            MeasurementStateFlags stateFlags = MapCommonStateFlags(qualityFlags.Value);

            if (qualityFlags.IsSet(DoubleBitBinaryQuality.CHATTER_FILTER))
                stateFlags |= MeasurementStateFlags.SuspectData;

            if (qualityFlags.IsSet(DoubleBitBinaryQuality.STATE1))
                stateFlags |= MeasurementStateFlags.AlarmHigh;

            if (!qualityFlags.IsSet(DoubleBitBinaryQuality.STATE2))
                stateFlags |= MeasurementStateFlags.AlarmLow;

            return stateFlags;
        }
    }

    private Measurement ConvertAnalog(Analog measurement, uint id, string source)
    {
        return new Measurement
        {
            Metadata = MeasurementKey.LookUpOrCreate(source, id).Metadata,
            Value = measurement.Value,
            Timestamp = measurement.Timestamp.Value,
            StateFlags = deriveStateFlags()
        };

        MeasurementStateFlags deriveStateFlags()
        {
            if (!MapQualityToStateFlags)
                return MeasurementStateFlags.Normal;

            Flags qualityFlags = measurement.Quality;
            MeasurementStateFlags stateFlags = MapCommonStateFlags(qualityFlags.Value);

            if (qualityFlags.IsSet(AnalogQuality.OVERRANGE))
                stateFlags |= MeasurementStateFlags.OverRangeError;

            if (qualityFlags.IsSet(AnalogQuality.REFERENCE_ERR))
                stateFlags |= MeasurementStateFlags.MeasurementError;

            if (qualityFlags.IsSet(AnalogQuality.RESERVED))
                stateFlags |= MeasurementStateFlags.UserDefinedFlag1;

            return stateFlags;
        }
    }

    private Measurement ConvertCounter(Counter measurement, uint id, string source)
    {
        return new Measurement
        {
            Metadata = MeasurementKey.LookUpOrCreate(source, id).Metadata,
            Value = measurement.Value,
            Timestamp = measurement.Timestamp.Value,
            StateFlags = deriveStateFlags()
        };

        MeasurementStateFlags deriveStateFlags()
        {
            if (!MapQualityToStateFlags)
                return MeasurementStateFlags.Normal;

            Flags qualityFlags = measurement.Quality;
            MeasurementStateFlags stateFlags = MapCommonStateFlags(qualityFlags.Value);

            if (qualityFlags.IsSet(CounterQuality.ROLLOVER))
                stateFlags |= MeasurementStateFlags.OverRangeError;

            if (qualityFlags.IsSet(CounterQuality.DISCONTINUITY))
                stateFlags |= MeasurementStateFlags.DiscardedValue;

            if (qualityFlags.IsSet(CounterQuality.RESERVED))
                stateFlags |= MeasurementStateFlags.UserDefinedFlag1;

            return stateFlags;
        }
    }

    private Measurement ConvertFrozenCounter(FrozenCounter measurement, uint id, string source)
    {
        return new Measurement
        {
            Metadata = MeasurementKey.LookUpOrCreate(source, id).Metadata,
            Value = measurement.Value,
            Timestamp = measurement.Timestamp.Value,
            StateFlags = deriveStateFlags()
        };

        MeasurementStateFlags deriveStateFlags()
        {
            if (!MapQualityToStateFlags)
                return MeasurementStateFlags.Normal;

            Flags qualityFlags = measurement.Quality;
            MeasurementStateFlags stateFlags = MapCommonStateFlags(qualityFlags.Value);

            if (qualityFlags.IsSet(FrozenCounterQuality.ROLLOVER))
                stateFlags |= MeasurementStateFlags.OverRangeError;

            if (qualityFlags.IsSet(FrozenCounterQuality.DISCONTINUITY))
                stateFlags |= MeasurementStateFlags.DiscardedValue;

            if (qualityFlags.IsSet(FrozenCounterQuality.RESERVED))
                stateFlags |= MeasurementStateFlags.UserDefinedFlag1;

            return stateFlags;
        }
    }

    private Measurement ConvertBinaryOutputStatus(BinaryOutputStatus measurement, uint id, string source)
    {
        return new Measurement 
        {
            Metadata = MeasurementKey.LookUpOrCreate(source, id).Metadata,
            Value = measurement.Value ? 1.0D : 0.0D,
            Timestamp = measurement.Timestamp.Value,
            StateFlags = deriveStateFlags()
        };

        MeasurementStateFlags deriveStateFlags()
        {
            if (!MapQualityToStateFlags)
                return MeasurementStateFlags.Normal;

            Flags qualityFlags = measurement.Quality;
            MeasurementStateFlags stateFlags = MapCommonStateFlags(qualityFlags.Value);

            if (qualityFlags.IsSet(BinaryOutputStatusQuality.RESERVED1))
                stateFlags |= MeasurementStateFlags.UserDefinedFlag1;

            if (qualityFlags.IsSet(BinaryOutputStatusQuality.RESERVED2))
                stateFlags |= MeasurementStateFlags.UserDefinedFlag2;

            if (qualityFlags.IsSet(BinaryOutputStatusQuality.STATE))
                stateFlags |= MeasurementStateFlags.AlarmHigh;

            return stateFlags;
        }
    }

    private Measurement ConvertAnalogOutputStatus(AnalogOutputStatus measurement, uint id, string source)
    {
        return new Measurement
        {
            Metadata = MeasurementKey.LookUpOrCreate(source, id).Metadata,
            Value = measurement.Value,
            Timestamp = measurement.Timestamp.Value,
            StateFlags = deriveStateFlags()
        };

        MeasurementStateFlags deriveStateFlags()
        {
            if (!MapQualityToStateFlags)
                return MeasurementStateFlags.Normal;

            Flags qualityFlags = measurement.Quality;
            MeasurementStateFlags stateFlags = MapCommonStateFlags(qualityFlags.Value);

            if (qualityFlags.IsSet(AnalogOutputStatusQuality.OVERRANGE))
                stateFlags |= MeasurementStateFlags.OverRangeError;

            if (qualityFlags.IsSet(AnalogOutputStatusQuality.REFERENCE_ERR))
                stateFlags |= MeasurementStateFlags.MeasurementError;

            if (qualityFlags.IsSet(AnalogOutputStatusQuality.RESERVED))
                stateFlags |= MeasurementStateFlags.UserDefinedFlag1;

            return stateFlags;
        }
    }

    private void GenericLookup<T>
    (
        T typedMeasurement, 
        uint index, 
        Dictionary<uint, Mapping> map, 
        Func<T, uint, string, Measurement> converter, 
        Action<IMeasurement> action
    )
    where T : MeasurementBase
    {
        if (!map.TryGetValue(index, out Mapping id))
            return;
        
        Measurement measurement = converter(typedMeasurement, id.tsfId, id.tsfSource);
        action(measurement);

        MeasurementKey valueKey = measurement.Key;

        if (valueKey is null || !AddQualityToMeasurementOutputs)
            return;

        // Process quality flags as an output measurement - we cache these results since
        // metadata lookups can be expensive, and we don't want to repeat this process
        MeasurementKey qualityKey = m_tagQualityMap.GetOrAdd(valueKey, _ =>
        {
            // The goal here is to look up the associated quality measurement metadata based on the
            // DNP3 value measurement tag name. This assumes that the quality measurement tag name
            // format is a variation of the value measurement tag name with a suffix appended to it.
            //
            // For example:
            //       Value Tag Name: DEVICEA!DNP3-BAJO-ACME_500KV_MVAR#1:ALOG2
            //     Quality Tag Name: DEVICEA!DNP3-BAJO-ACME_500KV_MVAR#1!FLAGS:ALOG3
            // 
            //       Value Tag Name: DEVICEB!DNP3-BUS2_BREAKER_STATE#10:DIGI1
            //     Quality Tag Name: DEVICEB!DNP3-BUS2_BREAKER_STATE#10!FLAGS:ALOG4
            try
            {
                DataSet dataSource = GetDataSource();

                // Look up metadata for the DNP3 measurement value
                DataRow record = dataSource?.LookupMetadata(valueKey.SignalID);

                if (record is null)
                    return MeasurementKey.Undefined;

                // Verify point tag matches expected format
                string pointTag = record["PointTag"].ToString();
                Match match = TagMatchRegex.Match(pointTag);

                if (!match.Success)
                    return MeasurementKey.Undefined;

                // Get root tag name from DNP3 measurement value point tag
                string rootTagName = match.Groups["TagName"].Value;

                // All flag measurements should be analog, so replace DIGI with ALOG
                string signalType = match.Groups["SignalType"].Value.Replace("DIGI", "ALOG");

                // Generate point tag lookup expression for DNP3 quality measurement (can't predict index)
                string qualityTagName = $"{rootTagName}{QualityTagSuffix}{signalType}%";

                // Attempt to look up quality measurement metadata
                record = dataSource.Tables["ActiveMeasurements"].Select($"PointTag LIKE '{qualityTagName}'").FirstOrDefault();

                if (record is not null && Guid.TryParse(record["SignalID"].ToNonNullString(), out Guid signalID))
                    return MeasurementKey.LookUpBySignalID(signalID);
            }
            catch (Exception ex)
            {
                Logger.SwallowException(ex);
            }

            return MeasurementKey.Undefined;
        });

        // Undefined response indicates that the quality key was not found - also verify that
        // quality key is not the same as the original measurement value key as a safety check
        if (qualityKey == MeasurementKey.Undefined || qualityKey == valueKey)
            return;

        action(new Measurement
        {
            Metadata = qualityKey.Metadata,
            Value = typedMeasurement.Quality.Value,
            Timestamp = measurement.Timestamp,
            StateFlags = MeasurementStateFlags.Normal
        });
    }

    private static MeasurementStateFlags MapCommonStateFlags(byte qualityFlags)
    {
        const byte ONLINE = (byte)Bits.Bit00;
        const byte RESTART = (byte)Bits.Bit01;
        const byte COMM_LOST = (byte)Bits.Bit02;
        const byte REMOTE_FORCED = (byte)Bits.Bit03;
        const byte LOCAL_FORCED = (byte)Bits.Bit04;

        MeasurementStateFlags stateFlags = MeasurementStateFlags.Normal;

        // The following bit flags are shared by all DNP3 quality flags
        if ((qualityFlags & ONLINE) == 0) // 0 == NOT ONLINE
            stateFlags |= MeasurementStateFlags.BadData;

        if ((qualityFlags & RESTART) > 0)
            stateFlags |= MeasurementStateFlags.FlatlineAlarm;

        if ((qualityFlags & COMM_LOST) > 0)
            stateFlags |= MeasurementStateFlags.ReceivedAsBad;

        if ((qualityFlags & REMOTE_FORCED) > 0)
            stateFlags |= MeasurementStateFlags.WarningHigh;

        if ((qualityFlags & LOCAL_FORCED) > 0)
            stateFlags |= MeasurementStateFlags.WarningLow;

        return stateFlags;
    }
}