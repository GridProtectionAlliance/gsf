//******************************************************************************************************
//  PowerStability.cs - Gbtc
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
//  11/08/2006 - J. Ritchie Carroll
//      Initial version of source generated
//  05/27/2008 - J. Ritchie Carroll
//       Added Montgomery line to power calculation
//  12/22/2009 - Jian R. Zuo
//       Converted code to C#;
//  04/12/2010 - J. Ritchie Carroll
//       Performed full code review, optimization and further abstracted code for stability calculator.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using GSF.Collections;
using GSF.NumericalAnalysis;
using GSF.PhasorProtocols;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using GSF.Units;

namespace PowerCalculations
{
    /// <summary>
    /// Represents an algorithm that calculates power and stability from a synchrophasor device.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This algorithm calculates power and its standard deviation in real-time that can be used to
    /// determine if there is an oscillatory signature in the power output.
    /// </para>
    /// <para>
    /// If multiple voltage phasors are provided as inputs to this algorithm, then they are assumed to be
    /// redundant values on the same bus, the first energized value will be the voltage phasor that is
    /// used in the calculation.<br/>
    /// If multiple current phasors are provided as inputs to this algorithm, then they are assumed to be
    /// cumulative inputs representing the desired power output summation of the generation source.
    /// </para>
    /// <para>
    /// Individual phase angle and magnitude phasor elements are expected to be defined consecutively.
    /// That is the definition order of angles and magnitudes must match so that the angle / magnitude
    /// pair can be matched up appropriately. For example: angle1;mag1;  angle2;mag2;  angle3;mag3.
    /// This can be accomplished in a filter expression by suffixing with "ORDER BY PhasorID".
    /// </para>
    /// </remarks>
    [Description("Power Stability: calculates power and stability for a synchrophasor device")]
    public class PowerStability : ActionAdapterBase
    {
        #region [ Members ]

        // Fields
        private Guid[] m_voltageAngleIDs;
        private Guid[] m_voltageMagnitudeIDs;
        private Guid[] m_currentAngleIDs;
        private Guid[] m_currentMagnitudeIDs;

        private Guid m_powerID;
        private Guid m_standardDeviationID;

        private int m_minimumSamples;
        private double m_energizedThreshold;
        private List<double> m_powerDataSample;
        private double m_lastStandardDeviation;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the signal IDs of the voltage angle input measurements.
        /// </summary>
        [ConnectionStringParameter,
        Description("Defines the input signal IDs for the voltage angle measurements of the power stability calculator - order must match voltage magnitude IDs; can be one of a filter expression, measurement keys, point tags or Guids."),
        CustomConfigurationEditor("GSF.TimeSeries.UI.WPF.dll", "GSF.TimeSeries.UI.Editors.MeasurementEditor")]
        public Guid[] VoltageAngleIDs
        {
            get
            {
                return m_voltageAngleIDs;
            }
            set
            {
                m_voltageAngleIDs = value;
            }
        }

        /// <summary>
        /// Gets or sets the signal IDs of the voltage magnitude input measurements.
        /// </summary>
        [ConnectionStringParameter,
        Description("Defines the input signal IDs for the voltage magnitude measurements of the power stability calculator - order must match voltage angle IDs; can be one of a filter expression, measurement keys, point tags or Guids."),
        CustomConfigurationEditor("GSF.TimeSeries.UI.WPF.dll", "GSF.TimeSeries.UI.Editors.MeasurementEditor")]
        public Guid[] VoltageMagnitudeIDs
        {
            get
            {
                return m_voltageMagnitudeIDs;
            }
            set
            {
                m_voltageMagnitudeIDs = value;
            }
        }

        /// <summary>
        /// Gets or sets the signal IDs of the current angle input measurements.
        /// </summary>
        [ConnectionStringParameter,
        Description("Defines the input signal IDs for the current angle measurements of the power stability calculator - order must match current magnitude IDs; can be one of a filter expression, measurement keys, point tags or Guids."),
        CustomConfigurationEditor("GSF.TimeSeries.UI.WPF.dll", "GSF.TimeSeries.UI.Editors.MeasurementEditor")]
        public Guid[] CurrentAngleIDs
        {
            get
            {
                return m_currentAngleIDs;
            }
            set
            {
                m_currentAngleIDs = value;
            }
        }

        /// <summary>
        /// Gets or sets the signal IDs of the current magnitude input measurements.
        /// </summary>
        [ConnectionStringParameter,
        Description("Defines the input signal IDs for the current magnitude measurements of the power stability calculator - order must match current angle IDs; can be one of a filter expression, measurement keys, point tags or Guids."),
        CustomConfigurationEditor("GSF.TimeSeries.UI.WPF.dll", "GSF.TimeSeries.UI.Editors.MeasurementEditor")]
        public Guid[] CurrentMagnitudeIDs
        {
            get
            {
                return m_currentMagnitudeIDs;
            }
            set
            {
                m_currentMagnitudeIDs = value;
            }
        }

        /// <summary>
        /// Gets or sets the signal ID of the power output measurement.
        /// </summary>
        [ConnectionStringParameter,
        Description("Defines the output signal ID for the power measurement of the power stability calculator; can be one of a filter expression, measurement key, point tag or Guid."),
        CustomConfigurationEditor("GSF.TimeSeries.UI.WPF.dll", "GSF.TimeSeries.UI.Editors.MeasurementEditor", "selectable=false")]
        public Guid PowerID
        {
            get
            {
                return m_powerID;
            }
            set
            {
                m_powerID = value;
            }
        }

        /// <summary>
        /// Gets or sets the signal ID of the standard deviation of power output measurement.
        /// </summary>
        [ConnectionStringParameter,
        Description("Defines the output signal ID for the standard deviation of power measurement of the power stability calculator; can be one of a filter expression, measurement key, point tag or Guid."),
        CustomConfigurationEditor("GSF.TimeSeries.UI.WPF.dll", "GSF.TimeSeries.UI.Editors.MeasurementEditor", "selectable=false")]
        public Guid StandardDeviationID
        {
            get
            {
                return m_standardDeviationID;
            }
            set
            {
                m_standardDeviationID = value;
            }
        }

        // ReSharper disable RedundantOverridenMember
        /// <summary>
        /// Gets or sets input signal IDs that the action adapter will produce, if any.
        /// </summary>
        /// <remarks>
        /// Overriding input signal IDs to remove its attributes such that it will not
        /// show up in the connection string parameters list. User should manually assign
        /// the <see cref="VoltageAngleIDs"/>, <see cref="VoltageMagnitudeIDs"/>,
        /// <see cref="CurrentAngleIDs"/> and <see cref="CurrentMagnitudeIDs"/> for the
        /// inputs of this calculator.
        /// </remarks>
        public override ISet<Guid> InputSignalIDs
        {
            get
            {
                return base.InputSignalIDs;
            }
            set
            {
                base.InputSignalIDs = value;
            }
        }

        /// <summary>
        /// Gets or sets output signal IDs that the action adapter will produce, if any.
        /// </summary>
        /// <remarks>
        /// Overriding output signal IDs to remove its attributes such that it will not
        /// show up in the connection string parameters list. User should manually assign
        /// the <see cref="PowerID"/> and <see cref="StandardDeviationID"/> for the
        /// outputs of this calculator.
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
        /// Gets or sets the sample size, in seconds, of the data to be monitored.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the sample size, in seconds, of the data to be monitored."),
        DefaultValue(15)]
        public int SampleSize
        {
            get
            {
                return m_minimumSamples / FramesPerSecond;
            }
            set
            {
                m_minimumSamples = value * FramesPerSecond;
            }
        }

        /// <summary>
        /// Gets or sets the energized bus threshold, in volts. The recommended value is 20% of nominal line-to-neutral voltage.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the energized bus threshold, in volts. The recommended value is 20% of nominal line-to-neutral voltage."),
        DefaultValue(58000.0D)]
        public double EnergizedThreshold
        {
            get
            {
                return m_energizedThreshold;
            }
            set
            {
                m_energizedThreshold = value;
            }
        }

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
        /// Returns the detailed status of the <see cref="PowerStability"/> monitor.
        /// </summary>
        public override string Status
        {
            get
            {
                const int ValuesToShow = 3;

                StringBuilder status = new StringBuilder();

                status.AppendFormat("          Data sample size: {0} seconds", (int)(m_minimumSamples / FramesPerSecond));
                status.AppendLine();
                status.AppendFormat("   Energized bus threshold: {0} volts", m_energizedThreshold.ToString("0.00"));
                status.AppendLine();
                status.AppendFormat("     Total voltage phasors: {0}", m_voltageMagnitudeIDs.Length);
                status.AppendLine();
                status.AppendFormat("     Total current phasors: {0}", m_currentMagnitudeIDs.Length);
                status.AppendLine();
                status.Append("         Last power values: ");

                lock (m_powerDataSample)
                {
                    // Display last several values
                    if (m_powerDataSample.Count > ValuesToShow)
                        status.Append(m_powerDataSample.GetRange(m_powerDataSample.Count - ValuesToShow - 1, ValuesToShow).Select(v => v.ToString("0.00MW")).ToDelimitedString(", "));
                    else
                        status.Append("Not enough values calculated yet...");
                }

                status.AppendLine();
                status.AppendFormat("   Latest std dev of power: {0}", m_lastStandardDeviation);
                status.AppendLine();
                status.Append(base.Status);

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes the <see cref="PowerStability"/> monitor.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            Dictionary<string, string> settings = Settings;
            string setting;

            // Load parameters
            if (settings.TryGetValue("sampleSize", out setting))            // Data sample size to monitor, in seconds
                m_minimumSamples = int.Parse(setting) * FramesPerSecond;
            else
                m_minimumSamples = 15 * FramesPerSecond;

            if (settings.TryGetValue("energizedThreshold", out setting))    // Energized bus threshold, in volts, recommended value
                m_energizedThreshold = double.Parse(setting);               // is 20% of nominal line-to-neutral voltage
            else
                m_energizedThreshold = 58000.0D;

            // TODO: This could greatly benefit from selecting inputs as complex numbers (i.e., source phasors) so angle and magnitude would be together

            // Get ID's and validate signal type for input measurements
            if (!this.TryParseSignalIDs(SignalType.VPHA, "voltageAngleIDs", out m_voltageAngleIDs))
                throw new InvalidOperationException("No signal IDs could be parsed for the voltage angle input measurement.");

            if (!this.TryParseSignalIDs(SignalType.VPHM, "voltageMagnitudeIDs", out m_voltageMagnitudeIDs))
                throw new InvalidOperationException("No signal IDs could be parsed for the voltage magnitude input measurement.");

            if (!this.TryParseSignalIDs(SignalType.IPHA, "currentAngleIDs", out m_currentAngleIDs))
                throw new InvalidOperationException("No signal IDs could be parsed for the current angle input measurement.");

            if (!this.TryParseSignalIDs(SignalType.IPHM, "currentMagnitudeIDs", out m_currentMagnitudeIDs))
                throw new InvalidOperationException("No signal IDs could be parsed for the current magnitude input measurement.");

            if (m_voltageAngleIDs.Length != m_voltageMagnitudeIDs.Length)
                throw new InvalidOperationException("A different number of voltage magnitude and angle input measurement keys were supplied - the angles and magnitudes must be supplied in pairs, i.e., one voltage magnitude input measurement must be supplied for each voltage angle input measurement in paired order (e.g., \"ORDER BY PhasorID\")");

            if (m_currentAngleIDs.Length != m_currentMagnitudeIDs.Length)
                throw new InvalidOperationException("A different number of current magnitude and angle input measurement keys were supplied - the angles and magnitudes must be supplied in pairs, i.e., one current magnitude input measurement must be supplied for each current angle input measurement in paired order (e.g., \"ORDER BY PhasorID\")");

            // Assign input measurements
            InputSignalIDs.Clear();
            InputSignalIDs.UnionWith(m_voltageAngleIDs);
            InputSignalIDs.UnionWith(m_voltageMagnitudeIDs);
            InputSignalIDs.UnionWith(m_currentAngleIDs);
            InputSignalIDs.UnionWith(m_currentMagnitudeIDs);

            // Get ID's for output measurements
            if (!this.TryParseSignalID("powerID", out m_powerID))
                throw new InvalidOperationException("No signal ID could be parsed for the power output measurement.");

            if (!this.TryParseSignalID("standardDeviationID", out m_standardDeviationID))
                throw new InvalidOperationException("No signal ID could be parsed for the standard deviation of power output measurement.");

            // Assign output measurements
            OutputSignalIDs.Clear();
            OutputSignalIDs.UnionWith(new[] { m_powerID, m_standardDeviationID });

            m_powerDataSample = new List<double>();
        }

        /// <summary>
        /// Publishes the <see cref="IFrame"/> of time-aligned collection of <see cref="IMeasurement"/> values that arrived within the
        /// adapter's defined <see cref="ConcentratorBase.LagTime"/>.
        /// </summary>
        /// <param name="frame"><see cref="IFrame"/> of measurements with the same timestamp that arrived within <see cref="ConcentratorBase.LagTime"/> that are ready for processing.</param>
        /// <param name="index">Index of <see cref="IFrame"/> within a second ranging from zero to <c><see cref="ConcentratorBase.FramesPerSecond"/> - 1</c>.</param>
        protected override void PublishFrame(IFrame frame, int index)
        {
            IMeasurement<double> magnitude, angle;
            double voltageMagnitude = double.NaN, voltageAngle = double.NaN, power = 0.0D;
            int i;

            // Get first voltage magnitude and angle value pair that is above the energized threshold
            for (i = 0; i < m_voltageMagnitudeIDs.Length; i++)
            {
                if (frame.TryGetEntity(m_voltageMagnitudeIDs[i], out magnitude) && frame.TryGetEntity(m_voltageAngleIDs[i], out angle))
                {
                    if (magnitude.Value > m_energizedThreshold)
                    {
                        voltageMagnitude = magnitude.Value;
                        voltageAngle = angle.Value;
                        break;
                    }
                }
            }

            // Exit if bus voltage measurements were not available for calculation
            if (double.IsNaN(voltageMagnitude))
                return;

            // Calculate the sum of the current phasors
            for (i = 0; i < m_currentMagnitudeIDs.Length; i++)
            {
                // Retrieve current magnitude and angle measurements as consecutive pairs
                if (frame.TryGetEntity(m_currentMagnitudeIDs[i], out magnitude) && frame.TryGetEntity(m_currentAngleIDs[i], out angle))
                    power += magnitude.Value * Math.Cos(Angle.FromDegrees(angle.Value - voltageAngle));
                else
                    return; // Exit if current measurements were not available for calculation
            }

            // Apply bus voltage and convert to 3-phase megawatts
            power = power * voltageMagnitude / (SI.Mega / 3.0D);

            // Add latest calculated power to data sample
            lock (m_powerDataSample)
            {
                m_powerDataSample.Add(power);

                // Maintain sample size
                while (m_powerDataSample.Count > m_minimumSamples)
                    m_powerDataSample.RemoveAt(0);
            }

            Measurement<double> powerMeasurement = new Measurement<double>(m_powerID, frame.Timestamp, power);

            // Check to see if the needed number of samples are available to begin producing the standard deviation output measurement
            if (m_powerDataSample.Count >= m_minimumSamples)
            {
                double standardDeviation;

                // Calculate standard deviation of power samples
                lock (m_powerDataSample)
                {
                    standardDeviation = m_powerDataSample.StandardDeviation();
                }

                // Provide calculated measurements for external consumption
                OnNewEntities(new[]
                {
                    powerMeasurement, 
                    new Measurement<double>(m_standardDeviationID, frame.Timestamp, standardDeviation)
                });

                // Track last standard deviation...
                m_lastStandardDeviation = standardDeviation;
            }
            else if (power > 0.0D)
            {
                // If not, we can still start publishing power calculation as soon as we have one...
                OnNewEntities(new[] { powerMeasurement });
            }
        }

        #endregion
    }
}