//******************************************************************************************************
//  TsfDataAdapter.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  10/05/2012 - Adam Crain
//       Generated original version of source code.
//  10/8/2012 - Danyelle Gilliam
//        Modified Header
//
//******************************************************************************************************

using DNP3.Interface;
using GSF.TimeSeries;
using System.Collections.Generic;

namespace Dnp3Adapters
{
    /// <summary>
    /// This is the data adapter that converts data from the dnp3 world to the TSF
    /// </summary>
    class TsfDataObserver : IDataObserver
    {
        public delegate void OnNewMeasurements(ICollection<IMeasurement> measurements);
        public event OnNewMeasurements NewMeasurements;

        private MeasurementLookup m_lookup;
        private List<IMeasurement> m_Measurements = new List<IMeasurement>();

        public TsfDataObserver(MeasurementLookup lookup)
        {
            this.m_lookup = lookup;
        }

        public void End()
        {
            if (m_Measurements.Count > 0 && this.NewMeasurements != null)
            {
                this.NewMeasurements(m_Measurements);
            }
        }

        public void Start()
        {
            m_Measurements.Clear();
        }

        public void Update(SetpointStatus update, uint index)
        {
            IMeasurement maybeNull = this.m_lookup.LookupMaybeNull(update, index);
            if (maybeNull != null) this.m_Measurements.Add(maybeNull);

        }

        public void Update(ControlStatus update, uint index)
        {
            IMeasurement maybeNull = this.m_lookup.LookupMaybeNull(update, index);
            if (maybeNull != null) this.m_Measurements.Add(maybeNull);
        }

        public void Update(Counter update, uint index)
        {
            IMeasurement maybeNull = this.m_lookup.LookupMaybeNull(update, index);
            if (maybeNull != null) this.m_Measurements.Add(maybeNull);
        }

        public void Update(Analog update, uint index)
        {
            IMeasurement maybeNull = this.m_lookup.LookupMaybeNull(update, index);
            if (maybeNull != null) this.m_Measurements.Add(maybeNull);
        }

        public void Update(Binary update, uint index)
        {
            IMeasurement maybeNull = this.m_lookup.LookupMaybeNull(update, index);
            if (maybeNull != null) this.m_Measurements.Add(maybeNull);
        }


    }
}
