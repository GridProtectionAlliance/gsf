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

using System;
using System.Collections.Generic;
using Automatak.DNP3.Interface;
using GSF.TimeSeries;

namespace DNP3Adapters
{
    /// <summary>
    /// Helper class that converts measurements and provides a lookup capability.
    /// </summary>
    class MeasurementLookup
    {
        public MeasurementLookup(MeasurementMap map)
        {
            map.binaryMap.ForEach(mapping => binaryMap.Add(mapping.dnpIndex, mapping));
            map.analogMap.ForEach(mapping => analogMap.Add(mapping.dnpIndex, mapping));
            map.counterMap.ForEach(mapping => counterMap.Add(mapping.dnpIndex, mapping));
            map.frozenCounterMap.ForEach(mapping => frozenCounterMap.Add(mapping.dnpIndex, mapping));
            map.controlStatusMap.ForEach(mapping => controlStatusMap.Add(mapping.dnpIndex, mapping));
            map.setpointStatusMap.ForEach(mapping => setpointStatusMap.Add(mapping.dnpIndex, mapping));
            map.doubleBitBinaryMap.ForEach(mapping => doubleBitBinaryMap.Add(mapping.dnpIndex, mapping));
        }

        public void Lookup(Binary measurement, ushort index, Action<IMeasurement> action)
        {
            GenericLookup(measurement, index, binaryMap, ConvertBinary, action);
        }

        public void Lookup(DoubleBitBinary measurement, ushort index, Action<IMeasurement> action)
        {
            GenericLookup(measurement, index, doubleBitBinaryMap, ConvertDoubleBinary, action);
        }

        public void Lookup(Analog measurement, ushort index, Action<IMeasurement> action)
        {
            GenericLookup(measurement, index, analogMap, ConvertAnalog, action);
        }

        public void Lookup(Counter measurement, ushort index, Action<IMeasurement> action)
        {
            GenericLookup(measurement, index, counterMap, ConvertCounter, action);
        }

        public void Lookup(FrozenCounter measurement, ushort index, Action<IMeasurement> action)
        {
            GenericLookup(measurement, index, frozenCounterMap, ConvertFrozenCounter, action);
        }

        public void Lookup(BinaryOutputStatus measurement, ushort index, Action<IMeasurement> action)
        {
            GenericLookup(measurement, index, controlStatusMap, ConvertBinaryOutputStatus, action);
        }

        public void Lookup(AnalogOutputStatus measurement, ushort index, Action<IMeasurement> action)
        {
            GenericLookup(measurement, index, setpointStatusMap, ConvertAnalogOutputStatus, action);
        }

        private Measurement ConvertBinary(Binary measurement, uint id, string source)
        {
            return new Measurement
            {
                Metadata = MeasurementKey.LookUpOrCreate(source, id).Metadata,
                Value = measurement.Value ? 1.0 : 0.0,
                Timestamp = measurement.Timestamp.Value
            };
        }

        private Measurement ConvertDoubleBinary(DoubleBitBinary measurement, uint id, string source)
        {
            Measurement convertedMeasurement = new Measurement
            {
                Metadata = MeasurementKey.LookUpOrCreate(source, id).Metadata,
                Timestamp = measurement.Timestamp.Value
            };
            
            switch (measurement.Value)
            {
                case DoubleBit.INDETERMINATE:
                    convertedMeasurement.Value = 0.0D;
                    break;
                case DoubleBit.DETERMINED_OFF:
                    convertedMeasurement.Value = 1.0D;
                    break;
                case DoubleBit.DETERMINED_ON:
                    convertedMeasurement.Value = 2.0D;
                    break;
                default:
                    convertedMeasurement.Value = 3.0D;
                    break;
            }
            
            return convertedMeasurement;
        }

        private Measurement ConvertAnalog(Analog measurement, uint id, string source)
        {
            return new Measurement
            {
                Metadata = MeasurementKey.LookUpOrCreate(source, id).Metadata,
                Value = measurement.Value,
                Timestamp = measurement.Timestamp.Value
            };
        }

        private Measurement ConvertCounter(Counter measurement, uint id, string source)
        {
            return new Measurement
            {
               Metadata = MeasurementKey.LookUpOrCreate(source, id).Metadata,
               Value = measurement.Value,
               Timestamp = measurement.Timestamp.Value
            };
        }

        private Measurement ConvertFrozenCounter(FrozenCounter measurement, uint id, string source)
        {
            return new Measurement
            {
                Metadata = MeasurementKey.LookUpOrCreate(source, id).Metadata,
                Value = measurement.Value,
                Timestamp = measurement.Timestamp.Value
            };
        }

        private Measurement ConvertBinaryOutputStatus(BinaryOutputStatus measurement, uint id, string source)
        {
            return new Measurement 
            {
                Metadata = MeasurementKey.LookUpOrCreate(source, id).Metadata,
                Value = measurement.Value ? 1.0D : 0.0D,
                Timestamp = measurement.Timestamp.Value
            };
        }

        private Measurement ConvertAnalogOutputStatus(AnalogOutputStatus measurement, uint id, string source)
        {
            return new Measurement
            {
                Metadata = MeasurementKey.LookUpOrCreate(source, id).Metadata,
                Value = measurement.Value,
                Timestamp = measurement.Timestamp.Value
            };
        }

        private static void GenericLookup<T>(T measurement, uint index, Dictionary<uint, Mapping> map, Func<T, uint, string, Measurement> converter, Action<IMeasurement> action)
        {
            if (map.TryGetValue(index, out Mapping id))
            {
                action(converter(measurement, id.tsfId, id.tsfSource));
            }
        }

        private readonly Dictionary<uint, Mapping> binaryMap = new Dictionary<uint, Mapping>();
        private readonly Dictionary<uint, Mapping> analogMap = new Dictionary<uint, Mapping>();
        private readonly Dictionary<uint, Mapping> counterMap = new Dictionary<uint, Mapping>();
        private readonly Dictionary<uint, Mapping> frozenCounterMap = new Dictionary<uint, Mapping>();
        private readonly Dictionary<uint, Mapping> controlStatusMap = new Dictionary<uint, Mapping>();
        private readonly Dictionary<uint, Mapping> setpointStatusMap = new Dictionary<uint, Mapping>();
        private readonly Dictionary<uint, Mapping> doubleBitBinaryMap = new Dictionary<uint, Mapping>();
    }
}
