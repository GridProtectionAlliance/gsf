//******************************************************************************************************
//  ReferenceMagnitude.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  11/22/2006 - J. Ritchie Carroll
//       Initial version of source generated
//  12/23/2009 - Jian R. Zuo
//       Converted code to C#;
//  04/12/2010 - J. Ritchie Carroll
//       Performed full code review, optimization and further abstracted code for magnitude calculation.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using GSF.PhasorProtocols;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;

namespace PowerCalculations
{
    /// <summary>
    /// Calculates an average magnitude associated with a composed reference angle.
    /// </summary>
    [Description("Reference Magnitude: calculates an average magnitude associated with a composed reference angle")]
    public class ReferenceMagnitude : ActionAdapterBase
    {
        #region [ Members ]

        // Fields
        private Guid m_referenceMagnitudeID;
        private double m_referenceMagnitude;
        private readonly Dictionary<Guid, double> m_lastValues = new Dictionary<Guid, double>();

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the signal ID for the reference magnitude output measurement.
        /// </summary>
        [ConnectionStringParameter,
        Description("Defines the output signal ID for the reference magnitude measurement of the reference magnitude calculator; can be one of a filter expression, measurement key, point tag or Guid."),
        CustomConfigurationEditor("GSF.TimeSeries.UI.WPF.dll", "GSF.TimeSeries.UI.Editors.MeasurementEditor", "selectable=false")]
        public Guid ReferenceMagnitudeID
        {
            get
            {
                return m_referenceMagnitudeID;
            }
            set
            {
                m_referenceMagnitudeID = value;
            }
        }

        // ReSharper disable RedundantOverridenMember
        /// <summary>
        /// Gets or sets output signals that the action adapter will produce, if any.
        /// </summary>
        /// <remarks>
        /// Overriding output signals to remove its attributes such that it will not show up
        /// in the connection string parameters list. User should manually assign the
        /// <see cref="ReferenceMagnitudeID"/> for the output of this calculator.
        /// </remarks>
        public override ISet<Guid> OutputSignalIDs
        {
            get
            {
                return base.OutputSignalIDs;
            }
            set
            {
                base.OutputSignalIDs = value;
            }
        }
        // ReSharper restore RedundantOverridenMember

        /// <summary>
        /// Gets the flag indicating if this adapter supports temporal processing.
        /// </summary>
        public override bool SupportsTemporalProcessing
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Returns the detailed status of the <see cref="ReferenceMagnitude"/> calculator.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.AppendFormat(" Last calculated magnitude: {0}", m_referenceMagnitude);
                status.AppendLine();
                status.Append(base.Status);

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes the <see cref="ReferenceMagnitude"/> calculator.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            // Validate input measurements
            SignalType type;

            Guid[] voltageMagnitudeIDs = InputSignalIDs.Where(id => this.TryGetSignalType(id, out type) && type == SignalType.VPHA).ToArray();
            Guid[] currentAngleIDs = InputSignalIDs.Where(id => this.TryGetSignalType(id, out type) && type == SignalType.IPHA).ToArray();

            if (voltageMagnitudeIDs.Length == 0 && currentAngleIDs.Length == 0)
                throw new InvalidOperationException("No valid phase magnitudes were specified as inputs to the reference magnitude calculator.");

            if (voltageMagnitudeIDs.Length > 0 && currentAngleIDs.Length > 0)
                throw new InvalidOperationException("A mixture of voltage and current phase magnitudes were specified as inputs to the reference magnitude calculator - you must specify one or the other: only voltage phase magnitudes or only current phase magnitudes.");

            // Make sure only phase magnitudes are used as input
            InputSignalIDs.Clear();
            InputSignalIDs.UnionWith(voltageMagnitudeIDs);
            InputSignalIDs.UnionWith(currentAngleIDs);

            // Get ID for the output measurement
            if (!this.TryParseSignalID("referenceMagnitudeID", out m_referenceMagnitudeID))
                throw new InvalidOperationException("No signal ID could be parsed for the reference magnitude output measurement.");

            // Assign output measurement
            OutputSignalIDs.Clear();
            OutputSignalIDs.UnionWith(new[] { m_referenceMagnitudeID });
        }

        /// <summary>
        /// Calculates the average reference magnitude.
        /// </summary>
        /// <param name="frame">Single frame of measurement data within a one second sample.</param>
        /// <param name="index">Index of frame within the one second sample.</param>
        protected override void PublishFrame(IFrame frame, int index)
        {
            if (frame.Entities.Count > 0)
            {
                // Calculate the average magnitude
                double magnitude;
                double lastValue;

                double total = 0.0D;
                int count = 0;

                foreach (IMeasurement<double> measurement in frame.Entities.Values.OfType<IMeasurement<double>>())
                {
                    magnitude = measurement.Value;

                    // Do some simple flat line avoidance...
                    if (m_lastValues.TryGetValue(measurement.ID, out lastValue))
                    {
                        if (lastValue == magnitude)
                            magnitude = double.NaN;
                        else
                            m_lastValues[measurement.ID] = magnitude;
                    }
                    else
                    {
                        m_lastValues.Add(measurement.ID, magnitude);
                    }

                    if (!double.IsNaN(magnitude))
                    {
                        total += magnitude;
                        count++;
                    }
                }

                if (count > 0)
                    m_referenceMagnitude = total / count;


                // Provide calculated measurement for external consumption
                OnNewEntities(new[] { new Measurement<double>(m_referenceMagnitudeID, frame.Timestamp, m_referenceMagnitude) });
            }
            else
            {
                m_referenceMagnitude = 0.0D;
            }
        }

        #endregion
    }
}