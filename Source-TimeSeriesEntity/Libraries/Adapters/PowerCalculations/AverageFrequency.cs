//******************************************************************************************************
//  AverageFrequency.cs - Gbtc
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
//  12/24/2009 - Jian R. Zuo
//       Converted code to C#
//  04/12/2010 - J. Ritchie Carroll
//       Performed full code review, optimization and further abstracted code for average calculation.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
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
    /// Calculates a real-time average frequency reporting the average, maximum and minimum values.
    /// </summary>
    [Description("Average Frequency: calculates a real-time average frequency reporting the average, maximum, and minimum values")]
    public class AverageFrequency : ActionAdapterBase
    {
        #region [ Members ]

        // Constants
        private const double LoFrequency = 57.0D;
        private const double HiFrequency = 62.0D;

        // Fields
        private Guid m_averageFrequencyID;
        private Guid m_maximumFrequencyID;
        private Guid m_minimumFrequencyID;

        private double m_averageFrequency;
        private double m_maximumFrequency;
        private double m_minimumFrequency;

        private readonly ConcurrentDictionary<Guid, int> m_lastValues = new ConcurrentDictionary<Guid, int>();

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the signal ID for the average frequency output measurement.
        /// </summary>
        [ConnectionStringParameter,
        Description("Defines the output signal ID for the average frequency measurement of the average frequency calculator; can be one of a filter expression, measurement key, point tag or Guid."),
        CustomConfigurationEditor("GSF.TimeSeries.UI.WPF.dll", "GSF.TimeSeries.UI.Editors.MeasurementEditor", "selectable=false")]
        public Guid AverageFrequencyID
        {
            get
            {
                return m_averageFrequencyID;
            }
            set
            {
                m_averageFrequencyID = value;
            }
        }

        /// <summary>
        /// Gets or sets the signal ID for the maximum frequency output measurement.
        /// </summary>
        [ConnectionStringParameter,
        Description("Defines the output signal ID for the maximum frequency measurement of the average frequency calculator; can be one of a filter expression, measurement key, point tag or Guid."),
        CustomConfigurationEditor("GSF.TimeSeries.UI.WPF.dll", "GSF.TimeSeries.UI.Editors.MeasurementEditor", "selectable=false")]
        public Guid MaximumFrequencyID
        {
            get
            {
                return m_maximumFrequencyID;
            }
            set
            {
                m_maximumFrequencyID = value;
            }
        }

        /// <summary>
        /// Gets or sets the signal ID for the minimum frequency output measurement.
        /// </summary>
        [ConnectionStringParameter,
        Description("Defines the output signal ID for the minimum frequency measurement of the average frequency calculator; can be one of a filter expression, measurement key, point tag or Guid."),
        CustomConfigurationEditor("GSF.TimeSeries.UI.WPF.dll", "GSF.TimeSeries.UI.Editors.MeasurementEditor", "selectable=false")]
        public Guid MinimumFrequencyID
        {
            get
            {
                return m_minimumFrequencyID;
            }
            set
            {
                m_minimumFrequencyID = value;
            }
        }

        // ReSharper disable RedundantOverridenMember
        /// <summary>
        /// Gets or sets output signals that the action adapter will produce, if any.
        /// </summary>
        /// <remarks>
        /// Overriding output signals to remove its attributes such that it will not show up
        /// in the connection string parameters list. User should manually assign the
        /// <see cref="AverageFrequencyID"/>, <see cref="MaximumFrequencyID"/> and
        /// <see cref="MinimumFrequencyID"/> for the outputs of this calculator.
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
        /// Returns the detailed status of the <see cref="AverageFrequency"/> calculator.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.AppendFormat("    Last average frequency: {0}", m_averageFrequency);
                status.AppendLine();
                status.AppendFormat("    Last maximum frequency: {0}", m_maximumFrequency);
                status.AppendLine();
                status.AppendFormat("    Last minimum frequency: {0}", m_minimumFrequency);
                status.AppendLine();
                status.Append(base.Status);

                return status.ToString();
            }
        }
        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes the <see cref="AverageFrequency"/> calculator.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            // Validate input measurements
            SignalType type;
            IEnumerable<Guid> validInputSignalIDs = InputSignalIDs.Where(id => this.TryGetSignalType(id, out type) && type == SignalType.FREQ);

            if (!validInputSignalIDs.Any())
                throw new InvalidOperationException("No valid frequency measurements were specified as inputs to the average frequency calculator.");

            // Make sure only frequencies are used as input
            InputSignalIDs.Clear();
            InputSignalIDs.UnionWith(validInputSignalIDs);

            // Get ID's for output measurements
            if (!this.TryParseSignalID("averageFrequencyID", out m_averageFrequencyID))
                throw new InvalidOperationException("No signal ID could be parsed for the average frequency output measurement.");

            if (!this.TryParseSignalID("maximumFrequencyID", out m_maximumFrequencyID))
                throw new InvalidOperationException("No signal ID could be parsed for the maximum frequency output measurement.");

            if (!this.TryParseSignalID("minimumFrequencyID", out m_minimumFrequencyID))
                throw new InvalidOperationException("No signal ID could be parsed for the minimum frequency output measurement.");

            // Assign output measurements
            OutputSignalIDs.Clear();
            OutputSignalIDs.UnionWith(new[] { m_averageFrequencyID, m_maximumFrequencyID, m_minimumFrequencyID });
        }

        /// <summary>
        /// Calculates the average frequency for all frequencies that have reported in the specified lag time.
        /// </summary>
        /// <param name="frame">Single frame of measurement data within a one second sample.</param>
        /// <param name="index">Index of frame within the one second sample.</param>
        protected override void PublishFrame(IFrame frame, int index)
        {
            if (frame.Entities.Count > 0)
            {
                const double hzResolution = 1000.0; // three decimal places

                double frequency;
                double frequencyTotal;
                double maximumFrequency = LoFrequency;
                double minimumFrequency = HiFrequency;
                int adjustedFrequency;
                int lastValue;
                int total;

                frequencyTotal = 0.0D;
                total = 0;

                foreach (IMeasurement<double> measurement in frame.Entities.Values.OfType<IMeasurement<double>>())
                {
                    frequency = measurement.Value;
                    adjustedFrequency = (int)(frequency * hzResolution);

                    // Do some simple flat line avoidance...
                    if (m_lastValues.TryGetValue(measurement.ID, out lastValue))
                    {
                        if (lastValue == adjustedFrequency)
                            frequency = 0.0D;
                        else
                            m_lastValues[measurement.ID] = adjustedFrequency;
                    }
                    else
                    {
                        m_lastValues[measurement.ID] = adjustedFrequency;
                    }

                    // Validate frequency
                    if (frequency > LoFrequency && frequency < HiFrequency)
                    {
                        frequencyTotal += frequency;

                        if (frequency > maximumFrequency)
                            maximumFrequency = frequency;

                        if (frequency < minimumFrequency)
                            minimumFrequency = frequency;

                        total++;
                    }
                }

                if (total > 0)
                {
                    m_averageFrequency = (frequencyTotal / total);
                    m_maximumFrequency = maximumFrequency;
                    m_minimumFrequency = minimumFrequency;
                }

                // Provide calculated measurements for external consumption
                OnNewEntities(new[]
                {
                    new Measurement<double>(m_averageFrequencyID, frame.Timestamp, m_averageFrequency),
                    new Measurement<double>(m_maximumFrequencyID, frame.Timestamp, m_maximumFrequency),
                    new Measurement<double>(m_minimumFrequencyID, frame.Timestamp, m_minimumFrequency)
                });
            }
            else
            {
                m_averageFrequency = 0.0D;
                m_maximumFrequency = 0.0D;
                m_minimumFrequency = 0.0D;
            }
        }

        #endregion
    }
}