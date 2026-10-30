//******************************************************************************************************
//  BestValueSelector.cs - Gbtc
//
//  Copyright © 2017, Grid Protection Alliance.  All Rights Reserved.
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
//  11/06/2017 - Stephen C. Wills
//       Generated original version of source code.
//  10/30/2026 - J. Ritchie Carroll
//       Moved quality settings and evaluation into BestValueSelectorBase and removed per-frame
//       LINQ allocations from best measurement selection.
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using System.Text;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;

namespace DataQualityMonitoring
{
    /// <summary>
    /// Produces a new signal by selecting the best values
    /// from a collection of concentrated data points.
    /// </summary>
    [Description("Best Value: Produces a new signal by selecting only the best values from a collection of input signals.")]
    public class BestValueSelector : BestValueSelectorBase
    {
        #region [ Members ]

        // Fields
        private MeasurementKey m_lastSelectedMeasurement;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets output measurements that the action adapter will produce, if any.
        /// </summary>
        [ConnectionStringParameter,
        Description("Defines primary keys of output measurements the action adapter expects; can be one of a filter expression, measurement key, point tag or Guid."),
        CustomConfigurationEditor("GSF.TimeSeries.UI.WPF.dll", "GSF.TimeSeries.UI.Editors.MeasurementEditor")]
        public override IMeasurement[] OutputMeasurements
        {
            get
            {
                return base.OutputMeasurements;
            }
            set
            {
                base.OutputMeasurements = value;
            }
        }

        /// <summary>
        /// Returns the detailed status of the action adapter.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder statusBuilder = new StringBuilder(base.Status);

                statusBuilder.AppendLine($"   Last Selected Measurement: {m_lastSelectedMeasurement}");

                return statusBuilder.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes the <see cref="BestValueSelector"/>.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            if (OutputMeasurements?.Length != 1 && OutputMeasurements?.Length != 2)
                throw new ArgumentException($"Exactly one or two output measurement must be defined. Amount defined: {OutputMeasurements?.Length ?? 0}");
        }

        /// <summary>
        /// Publish <see cref="IFrame"/> of time-aligned collection of <see cref="IMeasurement"/> values that arrived within the
        /// concentrator's defined <see cref="ConcentratorBase.LagTime"/>.
        /// </summary>
        /// <param name="frame"><see cref="IFrame"/> of measurements with the same timestamp that arrived within <see cref="ConcentratorBase.LagTime"/> that are ready for processing.</param>
        /// <param name="index">Index of <see cref="IFrame"/> within a second ranging from zero to <c><see cref="ConcentratorBase.FramesPerSecond"/> - 1</c>.</param>
        protected override void PublishFrame(IFrame frame, int index)
        {
            IMeasurement bestMeasurement = GetBestMeasurement(frame, out int inputIndex);

            if ((object)bestMeasurement == null)
                return;

            IMeasurement[] newMeasurements = OutputMeasurements
                .Select(measurement => Measurement.Clone(measurement))
                .ToArray();

            newMeasurements[0].Timestamp = frame.Timestamp;
            newMeasurements[0].StateFlags = bestMeasurement.StateFlags;
            newMeasurements[0].Value = bestMeasurement.Value;

            if (newMeasurements.Length > 1)
            {
                newMeasurements[1].Timestamp = frame.Timestamp;
                newMeasurements[1].StateFlags = bestMeasurement.StateFlags;
                newMeasurements[1].Value = inputIndex + 1;
            }

            OnNewMeasurements(newMeasurements);

            m_lastSelectedMeasurement = bestMeasurement.Key;
        }

        // Selects the first good input in priority order; when no input is good and bad values are
        // published, selects the least bad input, with the first of equal scores winning
        private IMeasurement GetBestMeasurement(IFrame frame, out int inputIndex)
        {
            MeasurementKey[] inputKeys = InputMeasurementKeys;
            ConcurrentDictionary<MeasurementKey, IMeasurement> measurements = frame.Measurements;
            bool publishBadValues = PublishBadValues;
            IMeasurement leastBadMeasurement = null;
            int leastBadScore = int.MaxValue;

            inputIndex = -1;

            for (int i = 0; i < inputKeys.Length; i++)
            {
                if (!measurements.TryGetValue(inputKeys[i], out IMeasurement measurement) || (object)measurement == null)
                    continue;

                if (IsGood(measurement))
                {
                    inputIndex = i;
                    return measurement;
                }

                if (!publishBadValues)
                    continue;

                int score = GetBadnessScore(measurement);

                if (score < leastBadScore)
                {
                    leastBadScore = score;
                    leastBadMeasurement = measurement;
                    inputIndex = i;
                }
            }

            return leastBadMeasurement;
        }

        #endregion
    }
}
