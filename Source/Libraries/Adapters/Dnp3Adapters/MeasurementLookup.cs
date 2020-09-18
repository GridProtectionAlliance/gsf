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
    /// Helper class that converts measurements and provides a lookup capbility
    /// </summary>
    class MeasurementLookup
    {
        public MeasurementLookup(MeasurementMap map)
        {
            map.binaryMap.ForEach(m => binaryMap.Add(m.dnpIndex, m));
            map.analogMap.ForEach(m => analogMap.Add(m.dnpIndex, m));
            map.counterMap.ForEach(m => counterMap.Add(m.dnpIndex, m));
            map.frozenCounterMap.ForEach(m => frozenCounterMap.Add(m.dnpIndex, m));
            map.controlStatusMap.ForEach(m => controlStatusMap.Add(m.dnpIndex, m));
            map.setpointStatusMap.ForEach(m => setpointStatusMap.Add(m.dnpIndex, m));
            map.doubleBitBinaryMap.ForEach(m => doubleBitBinaryMap.Add(m.dnpIndex, m));
        }

        public void Lookup(Binary meas, ushort index, Action<IMeasurement> action)
        {
            GenericLookup(meas, index, binaryMap, ConvertBinary, action);
        }

        public void Lookup(DoubleBitBinary meas, ushort index, Action<IMeasurement> action)
        {
            GenericLookup(meas, index, doubleBitBinaryMap, ConvertDoubleBinary, action);
        }

        public void Lookup(Analog meas, ushort index, Action<IMeasurement> action)
        {
            GenericLookup(meas, index, analogMap, ConvertAnalog, action);
        }

        public void Lookup(Counter meas, ushort index, Action<IMeasurement> action)
        {
            GenericLookup(meas, index, counterMap, ConvertCounter, action);
        }

        public void Lookup(FrozenCounter meas, ushort index, Action<IMeasurement> action)
        {
            GenericLookup(meas, index, frozenCounterMap, ConvertFrozenCounter, action);
        }

        public void Lookup(BinaryOutputStatus meas, ushort index, Action<IMeasurement> action)
        {
            GenericLookup(meas, index, controlStatusMap, ConvertBinaryOutputStatus, action);
        }

        public void Lookup(AnalogOutputStatus meas, ushort index, Action<IMeasurement> action)
        {
            GenericLookup(meas, index, setpointStatusMap, ConvertAnalogOutputStatus, action);
        }

        private Measurement ConvertBinary(Binary meas, uint id, string source)
        {
            var m = new Measurement();
            m.Metadata = MeasurementKey.LookUpOrCreate(source, id).Metadata;
            m.Value = meas.Value ? 1.0 : 0.0;
            m.Timestamp = meas.Timestamp.Value;
            return m;
        }

        private Measurement ConvertDoubleBinary(DoubleBitBinary meas, uint id, string source)
        {
            var m = new Measurement();
            m.Metadata = MeasurementKey.LookUpOrCreate(source, id).Metadata;
            switch (meas.Value)
            {
                case (DoubleBit.INDETERMINATE):
                    m.Value = 0.0;
                    break;
                case (DoubleBit.DETERMINED_OFF):
                    m.Value = 1.0;
                    break;
                case (DoubleBit.DETERMINED_ON):
                    m.Value = 2.0;
                    break;
                default:
                    m.Value = 3.0;
                    break;
            }
            m.Timestamp = meas.Timestamp.Value;
            return m;
        }

        private Measurement ConvertAnalog(Analog meas, uint id, string source)
        {
            var m = new Measurement();
            m.Metadata = MeasurementKey.LookUpOrCreate(source, id).Metadata;
            m.Value = meas.Value;
            m.Timestamp = meas.Timestamp.Value;
            return m;
        }

        private Measurement ConvertCounter(Counter meas, uint id, string source)
        {
            var m = new Measurement();
            m.Metadata = MeasurementKey.LookUpOrCreate(source, id).Metadata;
            m.Value = meas.Value;
            m.Timestamp = meas.Timestamp.Value;
            return m;
        }

        private Measurement ConvertFrozenCounter(FrozenCounter meas, uint id, string source)
        {
            var m = new Measurement();
            m.Metadata = MeasurementKey.LookUpOrCreate(source, id).Metadata;
            m.Value = meas.Value;
            m.Timestamp = meas.Timestamp.Value;
            return m;
        }

        private Measurement ConvertBinaryOutputStatus(BinaryOutputStatus meas, uint id, string source)
        {
            var m = new Measurement();
            m.Metadata = MeasurementKey.LookUpOrCreate(source, id).Metadata;
            m.Value = meas.Value ? 1.0 : 0.0;
            m.Timestamp = meas.Timestamp.Value;
            return m;
        }

        private Measurement ConvertAnalogOutputStatus(AnalogOutputStatus meas, uint id, string source)
        {
            var m = new Measurement();
            m.Metadata = MeasurementKey.LookUpOrCreate(source, id).Metadata;
            m.Value = meas.Value;
            m.Timestamp = meas.Timestamp.Value;
            return m;
        }

        private static void GenericLookup<T>(T meas, uint index, Dictionary<uint, Mapping> map, Func<T, uint, string, Measurement> converter, Action<IMeasurement> action)
        {
            Mapping id;
            if (map.TryGetValue(index, out id))
            {
                action(converter(meas, id.tsfId, id.tsfSource));
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
