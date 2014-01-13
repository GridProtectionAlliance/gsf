//******************************************************************************************************
//  LossOfField.cs - Gbtc
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
//  12/02/2009 - Jian R. Zuo
//       Generated original version of source code.
//  12/16/2009 - Jian R. Zuo
//       Reading parameters configuration from database
//  04/12/2010 - J. Ritchie Carroll
//       Performed full code review, optimization and further abstracted code for LOF detection.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using GSF.Collections;
using GSF.PhasorProtocols;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using GSF.Units;
using PhasorProtocolAdapters;

namespace PowerCalculations.EventDetection
{
    /// <summary>
    /// Represents an algorithm that detects Loss of Field from a synchrophasor device.
    /// </summary>
    [Description("Loss of Field: detects Loss of Field from a synchrophasor device")]
    public class LossOfField : ActionAdapterBase
    {
        #region [ Members ]

        // Constants
        private const double SqrtOf3 = 1.7320508075688772935274463415059D;

        // Fields
        private double m_pSet;                      // Threshold of Pset MW: default value -600 mW      
        private double m_qSet;                      // Threshold of Qset MVar: default value 200 mVar
        private double m_qAreaSet;                  // Threshold of Qarea MVar-sec: default value 500 mVar-sec
        private double m_voltageThreshold;          // Threshold of Voltage: default value 0.95 p.u. or 475 kV
        private double m_qAreamVar;                 // Calculated Q area value                 
        private int m_analysisInterval;             // Interval between adjacent calculations
        private long m_count;                       // Running frame count
        private long m_count1;                      // Last frame count
        private long m_count2;                      // Current frame count
        private Guid m_voltageMagnitudeID;          // Input signal ID for voltage magnitude
        private Guid m_voltageAngleID;              // Input signal ID for voltage angle
        private Guid m_currentMagnitudeID;          // Input signal ID for current magnitude
        private Guid m_currentAngleID;              // Input signal ID for current angle
        private Guid m_warningSignalID;             // Output signal ID for warning signal
        private Guid m_realPowerID;                 // Output signal ID for real power
        private Guid m_reactivePowerID;             // Output signal ID for reactive power
        private Guid m_qAreaValueID;                  // Output signal ID for Q-area value

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the signal ID of the voltage magnitude measurement.
        /// </summary>
        [ConnectionStringParameter,
        Description("Defines the input signal ID for the magnitude of the voltage phasor measurement; can be one of a filter expression, measurement key, point tag, or Guid.")]
        public Guid VoltageMagnitudeID
        {
            get
            {
                return m_voltageMagnitudeID;
            }
            set
            {
                m_voltageMagnitudeID = value;
            }
        }

        /// <summary>
        /// Gets or sets the signal ID of the voltage angle measurement.
        /// </summary>
        [ConnectionStringParameter,
        Description("Defines the input signal ID for the angle of the voltage phasor measurement; can be one of a filter expression, measurement key, point tag, or Guid.")]
        public Guid VoltageAngleID
        {
            get
            {
                return m_voltageAngleID;
            }
            set
            {
                m_voltageAngleID = value;
            }
        }

        /// <summary>
        /// Gets or sets the signal ID of the current magnitude measurement.
        /// </summary>
        [ConnectionStringParameter,
        Description("Defines the input signal ID for the angle of the voltage phasor measurement; can be one of a filter expression, measurement key, point tag, or Guid.")]
        public Guid CurrentMagnitudeID
        {
            get
            {
                return m_currentMagnitudeID;
            }
            set
            {
                m_currentMagnitudeID = value;
            }
        }

        /// <summary>
        /// Gets or sets the signal ID of the current angle measurement.
        /// </summary>
        [ConnectionStringParameter,
        Description("Defines the input signal ID for the angle of the current phasor measurement; can be one of a filter expression, measurement key, point tag, or Guid.")]
        public Guid CurrentAngleID
        {
            get
            {
                return m_currentAngleID;
            }
            set
            {
                m_currentAngleID = value;
            }
        }

        /// <summary>
        /// Gets or sets the signal ID of the current angle measurement.
        /// </summary>
        [ConnectionStringParameter,
        Description("Defines the input signal ID for the warning signal measurement; can be one of a filter expression, measurement key, point tag, or Guid.")]
        public Guid WarningSignalID
        {
            get
            {
                return m_warningSignalID;
            }
            set
            {
                m_warningSignalID = value;
            }
        }

        /// <summary>
        /// Gets or sets the signal ID of the current angle measurement.
        /// </summary>
        [ConnectionStringParameter,
        Description("Defines the input signal ID for the real component of the power measurement; can be one of a filter expression, measurement key, point tag, or Guid.")]
        public Guid RealPowerID
        {
            get
            {
                return m_realPowerID;
            }
            set
            {
                m_realPowerID = value;
            }
        }

        /// <summary>
        /// Gets or sets the signal ID of the current angle measurement.
        /// </summary>
        [ConnectionStringParameter,
        Description("Defines the input signal ID for the reactive component of the power measurement; can be one of a filter expression, measurement key, point tag, or Guid.")]
        public Guid ReactivePowerID
        {
            get
            {
                return m_reactivePowerID;
            }
            set
            {
                m_reactivePowerID = value;
            }
        }

        /// <summary>
        /// Gets or sets the signal ID of the current angle measurement.
        /// </summary>
        [ConnectionStringParameter,
        Description("Defines the input signal ID for the Q-area value measurement; can be one of a filter expression, measurement key, point tag, or Guid.")]
        public Guid QAreaValueID
        {
            get
            {
                return m_qAreaValueID;
            }
            set
            {
                m_qAreaValueID = value;
            }
        }

        /// <summary>
        /// Gets or sets the threshold of Pset MW.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the threshold of Pset MW."),
        DefaultValue(-600)]
        public double PSet
        {
            get
            {
                return m_pSet;
            }
            set
            {
                m_pSet = value;
            }
        }

        /// <summary>
        /// Gets or sets the threshold of Qset MVar.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the threshold of Qset MVar."),
        DefaultValue(200)]
        public double QSet
        {
            get
            {
                return m_qSet;
            }
            set
            {
                m_qSet = value;
            }
        }

        /// <summary>
        /// Gets or sets the threshold of Qarea MVar-sec.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the threshold of Qarea MVar-sec."),
        DefaultValue(500)]
        public double QAreaSet
        {
            get
            {
                return m_qAreaSet;
            }
            set
            {
                m_qAreaSet = value;
            }
        }

        /// <summary>
        /// Gets or sets the threshold of voltage, in volts.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the threshold of voltage, in volts."),
        DefaultValue(475000)]
        public double VoltageThreshold
        {
            get
            {
                return m_voltageThreshold;
            }
            set
            {
                m_voltageThreshold = value;
            }
        }

        /// <summary>
        /// Gets or sets the interval between adjacent calculations.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the interval between adjacent calculations. The default value is the frame-rate defined in the connection string for this Loss of Field."),
        DefaultValue("")]
        public int AnalysisInterval
        {
            get
            {
                return m_analysisInterval;
            }
            set
            {
                m_analysisInterval = value;
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
        /// Returns the detailed status of the <see cref="LossOfField"/> detector.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                status.AppendFormat("   Calculated Q-area value: {0}", m_qAreamVar);
                status.AppendLine();
                status.AppendFormat("               P-Set value: {0}", m_pSet);
                status.AppendLine();
                status.AppendFormat("               Q-Set value: {0}", m_qSet);
                status.AppendLine();
                status.AppendFormat("          Q-Area set value: {0}", m_qAreaSet);
                status.AppendLine();
                status.AppendFormat("         Voltage threshold: {0}", m_voltageThreshold);
                status.AppendLine();
                status.AppendFormat("      Calculation interval: {0}", m_analysisInterval);
                status.AppendLine();
                
                status.Append(base.Status);

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes the <see cref="LossOfField"/> detector.
        /// </summary>
        public override void Initialize()
        {
            Dictionary<string, string> settings;
            SignalType signalType;
            string setting;

            base.Initialize();
            settings = Settings;

            // Load required parameters
            if (!this.TryParseSignalID("voltageMagnitudeID", out m_voltageMagnitudeID))
                throw new InvalidOperationException("Voltage magnitude input signal ID was not found - this is a required input signal for the loss of field detector.");

            if (!this.TryParseSignalID("voltageAngleID", out m_voltageAngleID))
                throw new InvalidOperationException("Voltage angle input signal ID was not found - this is a required input signal for the loss of field detector.");

            if (!this.TryParseSignalID("currentMagnitudeID", out m_currentMagnitudeID))
                throw new InvalidOperationException("Current magnitude input signal ID was not found - this is a required input signal for the loss of field detector.");

            if (!this.TryParseSignalID("currentAngleID", out m_currentAngleID))
                throw new InvalidOperationException("Current angle input signal ID was not found - this is a required input signal for the loss of field detector.");

            if (!this.TryParseSignalID("warningSignalID", out m_warningSignalID))
                throw new InvalidOperationException("Warning signal output signal ID was not found - this is a required output signal for the loss of field detector.");

            if (!this.TryParseSignalID("realPowerID", out m_realPowerID))
                throw new InvalidOperationException("Real power output signal ID was not found - this is a required output signal for the loss of field detector.");

            if (!this.TryParseSignalID("reactivePowerID", out m_reactivePowerID))
                throw new InvalidOperationException("Reactive power output signal ID was not found - this is a required output signal for the loss of field detector.");

            if (!this.TryParseSignalID("qAreaValue", out m_qAreaValueID))
                throw new InvalidOperationException("Q-area value output signal ID was not found - this is a required output signal for the loss of field detector.");

            // Validate signal types of input signals
            if (!this.TryGetSignalType(m_voltageMagnitudeID, out signalType) || signalType != SignalType.VPHM)
                throw new InvalidOperationException(string.Format("Signal type of voltage magnitude input is incorrect. Type is {0} - should be VPHM.", signalType));

            if (!this.TryGetSignalType(m_voltageAngleID, out signalType) || signalType != SignalType.VPHA)
                throw new InvalidOperationException(string.Format("Signal type of voltage angle input is incorrect. Type is {0} - should be VPHA.", signalType));

            if (!this.TryGetSignalType(m_currentMagnitudeID, out signalType) || signalType != SignalType.IPHM)
                throw new InvalidOperationException(string.Format("Signal type of current magnitude input is incorrect. Type is {0} - should be IPHM.", signalType));

            if (!this.TryGetSignalType(m_currentAngleID, out signalType) || signalType != SignalType.IPHA)
                throw new InvalidOperationException(string.Format("Signal type of current angle input is incorrect. Type is {0} - should be IPHA.", signalType));

            // Load optional parameters
            if (settings.TryGetValue("pSet", out setting))
                m_pSet = double.Parse(setting);
            else
                m_pSet = -600;

            if (settings.TryGetValue("qSet", out setting))
                m_qSet = double.Parse(setting);
            else
                m_qSet = 200;

            if (settings.TryGetValue("qAreaSet", out setting))
                m_qAreaSet = double.Parse(setting);
            else
                m_qAreaSet = 500;

            if (settings.TryGetValue("voltageThreshold", out setting))
                m_voltageThreshold = double.Parse(setting);
            else
                m_voltageThreshold = 475000;

            if (settings.TryGetValue("analysisInterval", out setting))
                m_analysisInterval = int.Parse(setting);
            else
                m_analysisInterval = FramesPerSecond;

            m_count = 0;
            m_count1 = 0;
            m_count2 = 0;

            // Set input and output signals for routing
            InputSignalIDs.UnionWith(new Guid[] { m_voltageMagnitudeID, m_voltageAngleID, m_currentMagnitudeID, m_currentAngleID });
            OutputSignalIDs.UnionWith(new Guid[] { m_warningSignalID, m_realPowerID, m_reactivePowerID, m_qAreaValueID });
        }

        /// <summary>
        /// Publishes the <see cref="IFrame"/> of time-aligned collection of <see cref="IMeasurement"/> values that arrived within the
        /// adapter's defined <see cref="ConcentratorBase.LagTime"/>.
        /// </summary>
        /// <param name="frame"><see cref="IFrame"/> of measurements with the same timestamp that arrived within <see cref="ConcentratorBase.LagTime"/> that are ready for processing.</param>
        /// <param name="index">Index of <see cref="IFrame"/> within a second ranging from zero to <c><see cref="ConcentratorBase.FramesPerSecond"/> - 1</c>.</param>
        protected override void PublishFrame(IFrame frame, int index)
        {
            // Increment frame counter
            m_count++;

            if (m_count % m_analysisInterval == 0)
            {
                IMeasurement<double> measurement;
                double voltageMagnitude;
                double voltageAngle;
                double currentMagnitude;
                double currentAngle;
                double realPower;
                double reactivePower;
                double deltaT;
                bool warningSignaled = false;

                m_count1 = m_count2;
                m_count2 = m_count;

                if (frame.TryGetEntity(m_voltageMagnitudeID, out measurement))
                    voltageMagnitude = measurement.Value;
                else
                    return;

                if (frame.TryGetEntity(m_voltageAngleID, out measurement))
                    voltageAngle = Angle.FromDegrees(measurement.Value);
                else
                    return;

                if (frame.TryGetEntity(m_currentMagnitudeID, out measurement))
                    currentMagnitude = measurement.Value;
                else
                    return;

                if (frame.TryGetEntity(m_currentAngleID, out measurement))
                    currentAngle = Angle.FromDegrees(measurement.Value);
                else
                    return;

                realPower = 3 * voltageMagnitude * currentMagnitude * Math.Cos(voltageAngle - currentAngle) / SI.Mega;
                reactivePower = 3 * voltageMagnitude * currentMagnitude * Math.Sin(voltageAngle - currentAngle) / SI.Mega;
                deltaT = (m_count2 - m_count1) / FramesPerSecond;

                if (realPower < m_pSet && reactivePower > m_qSet)
                {
                    m_qAreamVar = m_qAreamVar + deltaT * (reactivePower - m_qSet);

                    if (m_qAreamVar > m_qAreaSet && voltageMagnitude < m_voltageThreshold / SqrtOf3)
                    {
                        warningSignaled = true;
                        OutputLOFWarning(realPower, reactivePower, m_qAreamVar);
                    }
                }
                else
                    m_qAreamVar = 0;

                // Expose output measurement values
                OnNewEntities(new ITimeSeriesEntity[]
                {
                    new Measurement<double>(m_warningSignalID, frame.Timestamp, warningSignaled ? 1.0D : 0.0D),
                    new Measurement<double>(m_realPowerID, frame.Timestamp, realPower),
                    new Measurement<double>(m_reactivePowerID, frame.Timestamp, reactivePower),
                    new Measurement<double>(m_qAreaValueID, frame.Timestamp, m_qAreamVar)
                });
            }
        }

        private void OutputLOFWarning(double realPower, double reactivePower, double qAreamVar)
        {
            OnStatusMessage("WARNING: Loss of Field Detected!\r\n        Real power = {0}\r\n    Reactive Power = {1}\r\n            Q Area = {2}\r\n", realPower, reactivePower, qAreamVar);
        }

        #endregion
    }
}