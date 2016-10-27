//******************************************************************************************************
//  ImpedanceCalculator.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  10/26/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using GSF;
using GSF.TimeSeries;
using GSF.Units;
using GSF.Units.EE;
using PhasorProtocolAdapters;

namespace PowerCalculations
{
    /// <summary>
    /// Represents an algorithm that calculates power and stability from a synchrophasor device.
    /// </summary>
    [Description("Impedance Calculator: Calculates impedance from phasors on two ends of a line - specify Vs/Is phasors followed by Vr/Ir phasors.")]
    public class ImpedanceCalculator : CalculatedMeasurementBase
    {
        #region [ Members ]

        // Fields
        private double m_lastResistance;
        private double m_lastReactance;
        private double m_lastConductance;
        private double m_lastSusceptance;
        private MeasurementKey[] m_voltageAngles;
        private MeasurementKey[] m_voltageMagnitudes;
        private MeasurementKey[] m_currentAngles;
        private MeasurementKey[] m_currentMagnitudes;

        // Important: Make sure output definition defines points in the following order
        private enum Output
        {
            Resistance,
            Reactance,
            Conductance,
            Susceptance
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Returns the detailed status of the <see cref="ImpedanceCalculator"/> monitor.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.AppendLine();
                status.AppendFormat("         Latest Resistance: {0} Ohm", m_lastResistance);
                status.AppendLine();
                status.AppendFormat("          Latest Reactance: {0} Ohm", m_lastReactance);
                status.AppendLine();
                status.AppendFormat("        Latest Conductance: {0} Mho", m_lastConductance);
                status.AppendLine();
                status.AppendFormat("        Latest Susceptance: {0} Mho", m_lastSusceptance);
                status.AppendLine();
                status.Append(base.Status);

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes the <see cref="ImpedanceCalculator"/> monitor.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            // Load needed phase angle and magnitude measurement keys from defined InputMeasurementKeys
            m_voltageAngles = InputMeasurementKeys.Where((key, index) => InputMeasurementKeyTypes[index] == SignalType.VPHA).ToArray();
            m_voltageMagnitudes = InputMeasurementKeys.Where((key, index) => InputMeasurementKeyTypes[index] == SignalType.VPHM).ToArray();
            m_currentAngles = InputMeasurementKeys.Where((key, index) => InputMeasurementKeyTypes[index] == SignalType.IPHA).ToArray();
            m_currentMagnitudes = InputMeasurementKeys.Where((key, index) => InputMeasurementKeyTypes[index] == SignalType.IPHM).ToArray();

            if (m_voltageAngles.Length != 2)
                throw new InvalidOperationException("Exactly two voltage angle input measurements are required for the impedance calculator, note that \"Vs\" angle/magnitude pair should be specified first followed by \"Vr\" angle/magnitude pair second.");

            if (m_voltageMagnitudes.Length != 2)
                throw new InvalidOperationException("Exactly two voltage magnitude input measurements are required for the impedance calculator, note that \"Vs\" angle/magnitude pair should be specified first followed by \"Vr\" angle/magnitude pair second.");

            if (m_currentAngles.Length != 2)
                throw new InvalidOperationException("Exactly two current angle input measurements are required for the impedance calculator, note that \"Is\" angle/magnitude pair should be specified first followed by \"Ir\" angle/magnitude pair second.");

            if (m_currentMagnitudes.Length != 2)
                throw new InvalidOperationException("Exactly two current magnitude input measurements are required for the impedance calculator, note that \"Is\" angle/magnitude pair should be specified first followed by \"Ir\" angle/magnitude pair second.");

            if (m_voltageAngles.Length != m_voltageMagnitudes.Length)
                throw new InvalidOperationException("A different number of voltage magnitude and angle input measurement keys were supplied - the angles and magnitudes must be supplied in pairs, i.e., one voltage magnitude input measurement must be supplied for each voltage angle input measurement in a consecutive sequence (e.g., VA1;VM1; VA2;VM2)");

            if (m_currentAngles.Length != m_currentMagnitudes.Length)
                throw new InvalidOperationException("A different number of current magnitude and angle input measurement keys were supplied - the angles and magnitudes must be supplied in pairs, i.e., one current magnitude input measurement must be supplied for each current angle input measurement in a consecutive sequence (e.g., IA1;IM1; IA2;IM2)");

            // Make sure only these phasor measurements are used as input
            InputMeasurementKeys = m_voltageAngles.Concat(m_voltageMagnitudes).Concat(m_currentAngles).Concat(m_currentMagnitudes).ToArray();

            // Validate output measurements
            if (OutputMeasurements.Length < Enum.GetValues(typeof(Output)).Length)
                throw new InvalidOperationException("Not enough output measurements were specified for the impedance calculator, expecting measurements for the \"Resistance\", \"Reactance\", \"Conductance\" and \"Susceptance\" - in this order.");
        }

        /// <summary>
        /// Publishes the <see cref="IFrame"/> of time-aligned collection of <see cref="IMeasurement"/> values that arrived within the
        /// adapter's defined <see cref="ConcentratorBase.LagTime"/>.
        /// </summary>
        /// <param name="frame"><see cref="IFrame"/> of measurements with the same timestamp that arrived within <see cref="ConcentratorBase.LagTime"/> that are ready for processing.</param>
        /// <param name="index">Index of <see cref="IFrame"/> within a second ranging from zero to <c><see cref="ConcentratorBase.FramesPerSecond"/> - 1</c>.</param>
        protected override void PublishFrame(IFrame frame, int index)
        {
            IDictionary<MeasurementKey, IMeasurement> measurements = frame.Measurements;
            IMeasurement magnitude, angle;
            ComplexNumber Zl, Yl, Vs = 0.0, Vr = 0.0, Is = 0.0, Ir = 0.0;
            double resistance, reactance, conductance, susceptance;
            int i, count = 0;

            // Get voltage magnitude and angle pairs
            for (i = 0; i < 2; i++)
            {
                if (measurements.TryGetValue(m_voltageMagnitudes[i], out magnitude) && measurements.TryGetValue(m_voltageAngles[i], out angle))
                {
                    if (i == 0)
                        Vs = new ComplexNumber(Angle.FromDegrees(angle.AdjustedValue), magnitude.AdjustedValue);
                    else
                        Vr = new ComplexNumber(Angle.FromDegrees(angle.AdjustedValue), magnitude.AdjustedValue);

                    count++;
                }
            }

            // Get current magnitude and angle pairs
            for (i = 0; i < 2; i++)
            {
                if (measurements.TryGetValue(m_currentMagnitudes[i], out magnitude) && measurements.TryGetValue(m_currentAngles[i], out angle))
                {
                    if (i == 0)
                        Is = new ComplexNumber(Angle.FromDegrees(angle.AdjustedValue), magnitude.AdjustedValue);
                    else
                        Ir = new ComplexNumber(Angle.FromDegrees(angle.AdjustedValue), magnitude.AdjustedValue);

                    count++;
                }
            }

            // Exit if all measurements were not available for calculation
            if (count != 4)
                return;

            // Calculate resistance and reactance
            Zl = (ComplexNumber.Pow(Vs, 2) - ComplexNumber.Pow(Vr, 2)) / (Vs * Ir + Vr * Is);

            resistance = Zl.Real;
            reactance = Zl.Imaginary;

            // Calculate conductance and susceptance
            Yl = 2 * (Is - Ir) / (Vs + Vr);

            conductance = Yl.Real;
            susceptance = Yl.Imaginary;

            // Provide calculated measurements for external consumption
            IMeasurement[] outputMeasurements = OutputMeasurements;

            Measurement resistsanceMeasurement = Measurement.Clone(outputMeasurements[(int)Output.Resistance], resistance, frame.Timestamp);
            Measurement reactanceMeasurement = Measurement.Clone(outputMeasurements[(int)Output.Reactance], reactance, frame.Timestamp);
            Measurement conductanceMeasurement = Measurement.Clone(outputMeasurements[(int)Output.Conductance], conductance, frame.Timestamp);
            Measurement susceptanceMeasurement = Measurement.Clone(outputMeasurements[(int)Output.Susceptance], susceptance, frame.Timestamp);

            OnNewMeasurements(new IMeasurement[] { resistsanceMeasurement, reactanceMeasurement, conductanceMeasurement, susceptanceMeasurement });

            // Track last calculated values...
            m_lastResistance = resistsanceMeasurement.AdjustedValue;
            m_lastReactance = reactanceMeasurement.AdjustedValue;
            m_lastConductance = conductanceMeasurement.AdjustedValue;
            m_lastSusceptance = susceptanceMeasurement.AdjustedValue;
        }

        #endregion
    }
}