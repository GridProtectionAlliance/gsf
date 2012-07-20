//******************************************************************************************************
//  FrequencyAverager.cs - Gbtc
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
//  06/22/2012 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Collections.Generic;
using TVA;
using TimeSeriesFramework;
using TimeSeriesFramework.Adapters;

namespace FrequencyAverager
{
    public class FrequencyAverager : ActionAdapterBase
    {
        #region [ Members ]

        // Constants
        private const bool DefaultSupportsTemporalProcessing = false;

        // Fields
        private bool m_supportsTemporalProcessing;

        #endregion

        #region [ Constructors ]

        #endregion

        #region [ Properties ]

        public override bool SupportsTemporalProcessing
        {
            get
            {
                return m_supportsTemporalProcessing;
            }
        }

        #endregion

        #region [ Methods ]

        public override void Initialize()
        {
            Dictionary<string, string> settings = Settings;
            string setting;

            settings["framesPerSecond"] = 1.ToString();
            settings["lagTime"] = 1.0D.ToString();
            settings["leadTime"] = 1.0D.ToString();
            settings["downsamplingMethod"] = "Filtered";
            settings["allowPreemptivePublishing"] = false.ToString();
            settings["performTimestampReasonabilityCheck"] = false.ToString();
            ConnectionString = settings.JoinKeyValuePairs();

            base.Initialize();

            if (settings.TryGetValue("supportsTemporalProcessing", out setting))
                m_supportsTemporalProcessing = setting.ParseBoolean();
            else
                m_supportsTemporalProcessing = DefaultSupportsTemporalProcessing;
        }

        protected override void PublishFrame(IFrame frame, int index)
        {
            IList<IMeasurement> newMeasurements = new List<IMeasurement>();

            IMeasurement inMeasurement;
            IMeasurement outMeasurement;

            for (int i = 0; i < InputMeasurementKeys.Length; i++)
            {
                if (!frame.Measurements.TryGetValue(InputMeasurementKeys[i], out inMeasurement))
                    continue;

                outMeasurement = Measurement.Clone(OutputMeasurements[i]);
                outMeasurement.Value = inMeasurement.Value;
                outMeasurement.Timestamp = frame.Timestamp;
                newMeasurements.Add(outMeasurement);
            }

            OnNewMeasurements(newMeasurements);
        }

        #endregion
    }
}
