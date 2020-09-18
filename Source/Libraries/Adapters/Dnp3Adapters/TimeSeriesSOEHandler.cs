//******************************************************************************************************
//  TsfDataAdapter.cs - Gbtc
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
//  03/02/2014 - Adam Crain
//       Migrated to new ISOEHandler interface
//  09/18/2020 - J. Ritchie Carroll
//       Updated to accommodate recent changes in ISOEHandler interface
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using Automatak.DNP3.Interface;
using GSF.TimeSeries;

namespace DNP3Adapters
{
    /// <summary>
    /// This is the data adapter that converts data from the dnp3 world to the Time Series Framework.
    /// </summary>
    internal class TimeSeriesSOEHandler : ISOEHandler
    {
        public delegate void OnNewMeasurements(ICollection<IMeasurement> measurements);
        public event OnNewMeasurements NewMeasurements;

        private readonly MeasurementLookup m_lookup;
        private readonly List<IMeasurement> m_measurements = new List<IMeasurement>();

        public TimeSeriesSOEHandler(MeasurementLookup lookup)
        {
            TimestampDifferentiation = TimeSpan.FromMilliseconds(1.0D);
            m_lookup = lookup;
        }

        public TimeSpan TimestampDifferentiation { get; set; }

        private void PutTimestamps<T>(IEnumerable<IndexedValue<T>> values) where T : MeasurementBase
        {
            DateTime now = DateTime.UtcNow;

            foreach (IGrouping<ushort, IndexedValue<T>> grouping in values.GroupBy(indexedValue => indexedValue.Index))
            {
                int count = grouping.Count();

                foreach (IndexedValue<T> indexedValue in grouping)
                {
                    TimeSpan offset = TimeSpan.FromTicks(count * TimestampDifferentiation.Ticks);
                    indexedValue.Value.Timestamp = new DNPTime(now - offset);
                    count--;
                }
            }
        }

        void ISOEHandler.BeginFragment(ResponseInfo info)
        {
            m_measurements.Clear();
        }

        void ISOEHandler.EndFragment(ResponseInfo info)
        {
            if (m_measurements.Count > 0 && NewMeasurements != null)
            {
                NewMeasurements(m_measurements);
            }
        }

        void ISOEHandler.Process(HeaderInfo info, IEnumerable<IndexedValue<Binary>> values)
        {
            IEnumerable<IndexedValue<Binary>> indexedValues = values.ToArray();

            PutTimestamps(indexedValues);

            foreach (IndexedValue<Binary> indexedValue in indexedValues)
                m_lookup.Lookup(indexedValue.Value, indexedValue.Index, m_measurements.Add);
        }

        void ISOEHandler.Process(HeaderInfo info, IEnumerable<IndexedValue<DoubleBitBinary>> values)
        {
            IEnumerable<IndexedValue<DoubleBitBinary>> indexedValues = values.ToArray();

            PutTimestamps(indexedValues);

            foreach (IndexedValue<DoubleBitBinary> indexedValue in indexedValues)
                m_lookup.Lookup(indexedValue.Value, indexedValue.Index, m_measurements.Add);
        }

        void ISOEHandler.Process(HeaderInfo info, IEnumerable<IndexedValue<Analog>> values)
        {
            IEnumerable<IndexedValue<Analog>> indexedValues = values.ToArray();

            PutTimestamps(indexedValues);

            foreach (IndexedValue<Analog> indexedValue in indexedValues)
                m_lookup.Lookup(indexedValue.Value, indexedValue.Index, m_measurements.Add);
        }

        void ISOEHandler.Process(HeaderInfo info, IEnumerable<IndexedValue<Counter>> values)
        {
            IEnumerable<IndexedValue<Counter>> indexedValues = values.ToArray();

            PutTimestamps(indexedValues);

            foreach (IndexedValue<Counter> indexedValue in indexedValues)
                m_lookup.Lookup(indexedValue.Value, indexedValue.Index, m_measurements.Add);
        }

        void ISOEHandler.Process(HeaderInfo info, IEnumerable<IndexedValue<FrozenCounter>> values)
        {
            IEnumerable<IndexedValue<FrozenCounter>> indexedValues = values.ToArray();

            PutTimestamps(indexedValues);

            foreach (IndexedValue<FrozenCounter> indexedValue in indexedValues)
                m_lookup.Lookup(indexedValue.Value, indexedValue.Index, m_measurements.Add);
        }

        void ISOEHandler.Process(HeaderInfo info, IEnumerable<IndexedValue<BinaryOutputStatus>> values)
        {
            IEnumerable<IndexedValue<BinaryOutputStatus>> indexedValues = values.ToArray();

            PutTimestamps(indexedValues);

            foreach (IndexedValue<BinaryOutputStatus> indexedValue in indexedValues)
                m_lookup.Lookup(indexedValue.Value, indexedValue.Index, m_measurements.Add);
        }

        void ISOEHandler.Process(HeaderInfo info, IEnumerable<IndexedValue<AnalogOutputStatus>> values)
        {
            IEnumerable<IndexedValue<AnalogOutputStatus>> indexedValues = values.ToArray();

            PutTimestamps(indexedValues);

            foreach (IndexedValue<AnalogOutputStatus> indexedValue in indexedValues)
                m_lookup.Lookup(indexedValue.Value, indexedValue.Index, m_measurements.Add);
        }

        void ISOEHandler.Process(HeaderInfo info, IEnumerable<IndexedValue<OctetString>> values)
        {
        }

        void ISOEHandler.Process(HeaderInfo info, IEnumerable<IndexedValue<TimeAndInterval>> values)
        {
        }

        void ISOEHandler.Process(HeaderInfo info, IEnumerable<IndexedValue<BinaryCommandEvent>> values)
        {
        }

        void ISOEHandler.Process(HeaderInfo info, IEnumerable<IndexedValue<AnalogCommandEvent>> values)
        {
        }

        void ISOEHandler.Process(HeaderInfo info, IEnumerable<IndexedValue<SecurityStat>> values)
        {
        }
    }
}
