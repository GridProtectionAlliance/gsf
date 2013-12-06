//******************************************************************************************************
//  PowerCalculator.cs - Gbtc
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
//  05/16/2012 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using GSF;
using GSF.Collections;
using GSF.PhasorProtocols;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using GSF.Units;

namespace PowerCalculations
{
    /// <summary>
    /// Calculates MW and MVAR using Voltage and Current Magnitude and Angle signals input to the adapter.
    /// </summary>
    [Description("Power Calculator: calculates power and reactive power for synchrophasor measurements")]
    public class PowerCalculator : ActionAdapterBase
    {
        #region [ Members ]

        // Constants
        private const double SqrtOf3 = 1.7320508075688772935274463415059D;

        // Fields
        private Guid m_voltageAngleID;
        private Guid m_voltageMagnitudeID;
        private Guid m_currentAngleID;
        private Guid m_currentMagnitudeID;

        private Guid m_powerID;
        private Guid m_reactivePowerID;

        private bool m_trackRecentValues;
        private int m_sampleSize;
        private List<double> m_powerSample;
        private List<double> m_reactivePowerSample;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the signal ID of the voltage angle input measurement.
        /// </summary>
        [ConnectionStringParameter,
        Description("Defines the input signal ID for the voltage angle measurement of the power calculator; can be one of a filter expression, measurement key, point tag or Guid."),
        CustomConfigurationEditor("GSF.TimeSeries.UI.WPF.dll", "GSF.TimeSeries.UI.Editors.MeasurementEditor", "selectable=false")]
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
        /// Gets or sets the signal ID of the voltage magnitude input measurement.
        /// </summary>
        [ConnectionStringParameter,
        Description("Defines the input signal ID for the voltage magnitude measurement of the power calculator; can be one of a filter expression, measurement key, point tag or Guid."),
        CustomConfigurationEditor("GSF.TimeSeries.UI.WPF.dll", "GSF.TimeSeries.UI.Editors.MeasurementEditor", "selectable=false")]
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
        /// Gets or sets the signal ID of the current angle input measurement.
        /// </summary>
        [ConnectionStringParameter,
        Description("Defines the input signal ID for the current angle measurement of the power calculator; can be one of a filter expression, measurement key, point tag or Guid."),
        CustomConfigurationEditor("GSF.TimeSeries.UI.WPF.dll", "GSF.TimeSeries.UI.Editors.MeasurementEditor", "selectable=false")]
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
        /// Gets or sets the signal ID of the current magnitude input measurement.
        /// </summary>
        [ConnectionStringParameter,
        Description("Defines the input signal ID for the current magnitude measurement of the power calculator; can be one of a filter expression, measurement key, point tag or Guid."),
        CustomConfigurationEditor("GSF.TimeSeries.UI.WPF.dll", "GSF.TimeSeries.UI.Editors.MeasurementEditor", "selectable=false")]
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
        /// Gets or sets the signal ID of the power output measurement.
        /// </summary>
        [ConnectionStringParameter,
        Description("Defines the output signal ID for the power measurement of the power calculator; can be one of a filter expression, measurement key, point tag or Guid."),
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
        /// Gets or sets the signal ID of the reactive power output measurement.
        /// </summary>
        [ConnectionStringParameter,
        Description("Defines the output signal ID for the reactive power measurement of the power calculator; can be one of a filter expression, measurement key, point tag or Guid."),
        CustomConfigurationEditor("GSF.TimeSeries.UI.WPF.dll", "GSF.TimeSeries.UI.Editors.MeasurementEditor", "selectable=false")]
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
        /// Gets or sets flag that determines if the last few values should be monitored.
        /// </summary>
        [ConnectionStringParameter,
        Description("Flag that determines if the last few values should be monitored."),
        DefaultValue(true)]
        public bool TrackRecentValues
        {
            get
            {
                return m_trackRecentValues;
            }
            set
            {
                m_trackRecentValues = value;
            }
        }

        /// <summary>
        /// Gets or sets the sample size of the data to be monitored.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the sample size of the data to be monitored."),
        DefaultValue(5)]
        public int SampleSize
        {
            get
            {
                return m_sampleSize;
            }
            set
            {
                m_sampleSize = value;
            }
        }

        // ReSharper disable RedundantOverridenMember
        /// <summary>
        /// Gets or sets input signal IDs that the action adapter will produce, if any.
        /// </summary>
        /// <remarks>
        /// Overriding input signal IDs to remove its attributes such that it will not
        /// show up in the connection string parameters list. User should manually assign
        /// the <see cref="VoltageAngleID"/>, <see cref="VoltageMagnitudeID"/>,
        /// <see cref="CurrentAngleID"/> and <see cref="CurrentMagnitudeID"/> for the
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
        /// the <see cref="PowerID"/> and <see cref="ReactivePowerID"/> for the outputs
        /// of this calculator.
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
        /// Returns the detailed status of the <see cref="PowerCalculator"/>.
        /// </summary>
        public override string Status
        {
            get
            {
                const int ValuesToShow = 3;

                StringBuilder status = new StringBuilder();

                if (m_trackRecentValues)
                {
                    status.Append("         Last power values: ");

                    lock (m_powerSample)
                    {
                        // Display last several values
                        if (m_powerSample.Count > ValuesToShow)
                            status.Append(m_powerSample.GetRange(m_powerSample.Count - ValuesToShow - 1, ValuesToShow).Select(v => v.ToString("0.00MW")).ToDelimitedString(", "));
                        else
                            status.Append("Not enough values calculated yet...");
                    }
                    status.AppendLine();

                    status.Append("Last reactive power values: ");

                    lock (m_reactivePowerSample)
                    {
                        // Display last several values
                        if (m_reactivePowerSample.Count > ValuesToShow)
                            status.Append(m_reactivePowerSample.GetRange(m_reactivePowerSample.Count - ValuesToShow - 1, ValuesToShow).Select(v => v.ToString("0.00MVAR")).ToDelimitedString(", "));
                        else
                            status.Append("Not enough values calculated yet...");
                    }
                    status.AppendLine();
                }

                status.Append(base.Status);

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes the <see cref="PowerCalculator"/>.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            Dictionary<string, string> settings = Settings;
            string setting;

            // Load parameters
            if (settings.TryGetValue("trackRecentValues", out setting))
                m_trackRecentValues = setting.ParseBoolean();
            else
                m_trackRecentValues = true;

            if (settings.TryGetValue("sampleSize", out setting))    // Data sample size to monitor, in seconds
                m_sampleSize = int.Parse(setting);
            else
                m_sampleSize = 5;

            // TODO: This could greatly benefit from selecting inputs as complex numbers (i.e., source phasors) so angle and magnitude would be together

            // Get ID's and validate signal type for input measurements
            if (!this.TryParseSignalID(SignalType.VPHA, "voltageAngleID", out m_voltageAngleID))
                throw new InvalidOperationException("No signal ID could be parsed for the voltage angle input measurement.");

            if (!this.TryParseSignalID(SignalType.VPHM, "voltageMagnitudeID", out m_voltageMagnitudeID))
                throw new InvalidOperationException("No signal ID could be parsed for the voltage magnitude input measurement.");

            if (!this.TryParseSignalID(SignalType.IPHA, "currentAngleID", out m_currentAngleID))
                throw new InvalidOperationException("No signal ID could be parsed for the current angle input measurement.");

            if (!this.TryParseSignalID(SignalType.IPHM, "currentMagnitudeID", out m_currentMagnitudeID))
                throw new InvalidOperationException("No signal ID could be parsed for the current magnitude input measurement.");

            // Assign input measurements
            InputSignalIDs.Clear();
            InputSignalIDs.UnionWith(new[] { m_voltageAngleID, m_voltageMagnitudeID, m_currentAngleID, m_currentMagnitudeID });

            // Get ID's for output measurements
            if (!this.TryParseSignalID("powerID", out m_powerID))
                throw new InvalidOperationException("No signal ID could be parsed for the power output measurement.");

            if (!this.TryParseSignalID("reactivePowerID", out m_reactivePowerID))
                throw new InvalidOperationException("No signal ID could be parsed for the reactive power output measurement.");

            // Assign output measurements
            OutputSignalIDs.Clear();
            OutputSignalIDs.UnionWith(new[] { m_powerID, m_reactivePowerID });

            if (m_trackRecentValues)
            {
                m_powerSample = new List<double>();
                m_reactivePowerSample = new List<double>();
            }

            // Assign a default adapter name to be used if power calculator is loaded as part of an automated collection
            if (string.IsNullOrWhiteSpace(Name))
            {
                MeasurementKey key;
                string keyName = m_powerID.ToString();

                if (this.TryGetMeasurementKey(m_powerID, out key))
                    keyName = key.ToString();

                Name = string.Format("PC!{0}", keyName);
            }
        }

        /// <summary>
        /// Publish frame of time-aligned collection of measurement values that arrived within the defined lag time.
        /// </summary>
        /// <param name="frame">Frame of measurements with the same timestamp that arrived within lag time that are ready for processing.</param>
        /// <param name="index">Index of frame within a second ranging from zero to frames per second - 1.</param>
        protected override void PublishFrame(IFrame frame, int index)
        {
            double power = double.NaN, reactivePower = double.NaN;

            try
            {
                double voltageMagnitude = 0.0D, voltageAngle = 0.0D, currentMagnitude = 0.0D, currentAngle = 0.0D;
                IMeasurement<double> measurement;
                bool allValuesReceived = false;

                // Get each needed value from this frame
                if (frame.TryGetEntity(m_voltageMagnitudeID, out measurement) && measurement.ValueQualityIsGood())
                {
                    // Get voltage magnitude value
                    voltageMagnitude = measurement.Value;

                    if (frame.TryGetEntity(m_voltageAngleID, out measurement) && measurement.ValueQualityIsGood())
                    {
                        // Get voltage angle value
                        voltageAngle = measurement.Value;

                        if (frame.TryGetEntity(m_currentMagnitudeID, out measurement) && measurement.ValueQualityIsGood())
                        {
                            // Get current magnitude value
                            currentMagnitude = measurement.Value;

                            if (frame.TryGetEntity(m_currentAngleID, out measurement) && measurement.ValueQualityIsGood())
                            {
                                // Get current angle value
                                currentAngle = measurement.Value;
                                allValuesReceived = true;
                            }
                        }
                    }
                }

                if (allValuesReceived)
                {
                    double angleDifference = Math.Abs(voltageAngle - currentAngle);

                    if (angleDifference > 180)
                        angleDifference = 360 - angleDifference;

                    // Convert phase angle difference to radians
                    double impedancePhaseAngle = Angle.FromDegrees(angleDifference);

                    // Calculate line-to-neutral apparent power (S) vector magnitude in Mega volt-amps
                    double apparentPower = SqrtOf3 * (Math.Abs(voltageMagnitude) / SI.Mega) * Math.Abs(currentMagnitude);

                    // Calculate power (P) and reactive power (Q)
                    power = apparentPower * Math.Cos(impedancePhaseAngle);
                    reactivePower = apparentPower * Math.Sin(impedancePhaseAngle);

                    if (m_trackRecentValues)
                    {
                        // Add latest calculated power to data sample
                        lock (m_powerSample)
                        {
                            m_powerSample.Add(power);

                            // Maintain sample size
                            while (m_powerSample.Count > m_sampleSize)
                                m_powerSample.RemoveAt(0);
                        }

                        // Add latest calculated reactive power to data sample
                        lock (m_reactivePowerSample)
                        {
                            m_reactivePowerSample.Add(reactivePower);

                            // Maintain sample size
                            while (m_reactivePowerSample.Count > m_sampleSize)
                                m_reactivePowerSample.RemoveAt(0);
                        }
                    }
                }
            }
            finally
            {
                // Provide calculated measurements for external consumption
                OnNewEntities(new[]
                {
                    new Measurement<double>(m_powerID, frame.Timestamp, power),
                    new Measurement<double>(m_reactivePowerID, frame.Timestamp, reactivePower)
                });
            }
        }

        #endregion
    }
}