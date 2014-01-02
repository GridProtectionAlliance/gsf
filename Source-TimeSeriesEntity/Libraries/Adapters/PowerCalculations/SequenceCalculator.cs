//******************************************************************************************************
//  SequenceCalculator.cs - Gbtc
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
using System.Collections.Concurrent;
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
using PhasorProtocolAdapters;

namespace PowerCalculations
{
    /// <summary>
    /// Calculates positive, negative and zero sequences using A, B and C phase voltage or current magnitude and angle signals input to the adapter.
    /// </summary>
    [Description("Sequence Calculator: positive, negative and zero sequences for synchrophasor measurements")]
    public class SequenceCalculator : ActionAdapterBase
    {
        #region [ Members ]

        // Constants
        private const double Rad120 = 2.0D * Math.PI / 3.0D;

        // Fields
        private Guid m_aPhaseMagnitudeID;
        private Guid m_aPhaseAngleID;
        private Guid m_bPhaseMagnitudeID;
        private Guid m_bPhaseAngleID;
        private Guid m_cPhaseMagnitudeID;
        private Guid m_cPhaseAngleID;

        private Guid m_positiveMagnitudeID;
        private Guid m_negativeMagnitudeID;
        private Guid m_zeroMagnitudeID;
        private Guid m_positiveAngleID;
        private Guid m_negativeAngleID;
        private Guid m_zeroAngleID;

        private string m_magnitudeUnits;
        private bool m_trackRecentValues;
        private int m_sampleSize;

        private List<double> m_positiveMagnitudeSample;
        private List<double> m_positiveAngleSample;
        private List<double> m_negativeMagnitudeSample;
        private List<double> m_negativeAngleSample;
        private List<double> m_zeroMagnitudeSample;
        private List<double> m_zeroAngleSample;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the signal ID for the magnitude of the A-phase voltage or current phasor measurement.
        /// </summary>
        [ConnectionStringParameter,
        Description("Defines the input signal ID for the magnitude of the A-phase voltage or current phasor measurement; can be one of a filter expression, measurement key, point tag, or Guid."),
        CustomConfigurationEditor("GSF.TimeSeries.UI.WPF.dll", "GSF.TimeSeries.UI.Editors.MeasurementEditor", "selectable=false")]
        public Guid APhaseMagnitudeID
        {
            get
            {
                return m_aPhaseMagnitudeID;
            }
            set
            {
                m_aPhaseMagnitudeID = value;
            }
        }

        /// <summary>
        /// Gets or sets the signal ID for the angle of the A-phase voltage or current phasor measurement.
        /// </summary>
        [ConnectionStringParameter,
        Description("Defines the input signal ID for the angle of the A-phase voltage or current phasor measurement; can be one of a filter expression, measurement key, point tag, or Guid."),
        CustomConfigurationEditor("GSF.TimeSeries.UI.WPF.dll", "GSF.TimeSeries.UI.Editors.MeasurementEditor", "selectable=false")]
        public Guid APhaseAngleID
        {
            get
            {
                return m_aPhaseAngleID;
            }
            set
            {
                m_aPhaseAngleID = value;
            }
        }

        /// <summary>
        /// Gets or sets the signal ID for the magnitude of the B-phase voltage or current phasor measurement.
        /// </summary>
        [ConnectionStringParameter,
        Description("Defines the input signal ID for the magnitude of the B-phase voltage or current phasor measurement; can be one of a filter expression, measurement key, point tag, or Guid."),
        CustomConfigurationEditor("GSF.TimeSeries.UI.WPF.dll", "GSF.TimeSeries.UI.Editors.MeasurementEditor", "selectable=false")]
        public Guid BPhaseMagnitudeID
        {
            get
            {
                return m_bPhaseMagnitudeID;
            }
            set
            {
                m_bPhaseMagnitudeID = value;
            }
        }

        /// <summary>
        /// Gets or sets the signal ID for the angle of the B-phase voltage or current phasor measurement.
        /// </summary>
        [ConnectionStringParameter,
        Description("Defines the input signal ID for the angle of the B-phase voltage or current phasor measurement; can be one of a filter expression, measurement key, point tag, or Guid."),
        CustomConfigurationEditor("GSF.TimeSeries.UI.WPF.dll", "GSF.TimeSeries.UI.Editors.MeasurementEditor", "selectable=false")]
        public Guid BPhaseAngleID
        {
            get
            {
                return m_bPhaseAngleID;
            }
            set
            {
                m_bPhaseAngleID = value;
            }
        }

        /// <summary>
        /// Gets or sets the signal ID for the magnitude of the C-phase voltage or current phasor measurement.
        /// </summary>
        [ConnectionStringParameter,
        Description("Defines the input signal ID for the magnitude of the C-phase voltage or current phasor measurement; can be one of a filter expression, measurement key, point tag, or Guid."),
        CustomConfigurationEditor("GSF.TimeSeries.UI.WPF.dll", "GSF.TimeSeries.UI.Editors.MeasurementEditor", "selectable=false")]
        public Guid CPhaseMagnitudeID
        {
            get
            {
                return m_cPhaseMagnitudeID;
            }
            set
            {
                m_cPhaseMagnitudeID = value;
            }
        }

        /// <summary>
        /// Gets or sets the signal ID for the angle of the C-phase voltage or current phasor measurement.
        /// </summary>
        [ConnectionStringParameter,
        Description("Defines the input signal ID for the angle of the C-phase voltage or current phasor measurement; can be one of a filter expression, measurement key, point tag, or Guid."),
        CustomConfigurationEditor("GSF.TimeSeries.UI.WPF.dll", "GSF.TimeSeries.UI.Editors.MeasurementEditor", "selectable=false")]
        public Guid CPhaseAngleID
        {
            get
            {
                return m_cPhaseAngleID;
            }
            set
            {
                m_cPhaseAngleID = value;
            }
        }

        /// <summary>
        /// Gets or sets the signal ID for the positive sequence phasor measurement.
        /// </summary>
        [ConnectionStringParameter,
        Description("Defines the output signal ID for the magnitude of the positive sequence phasor measurement; can be one of a filter expression, measurement key, point tag, or Guid."),
        CustomConfigurationEditor("GSF.TimeSeries.UI.WPF.dll", "GSF.TimeSeries.UI.Editors.MeasurementEditor", "selectable=false")]
        public Guid PositiveMagnitudeID
        {
            get
            {
                return m_positiveMagnitudeID;
            }
            set
            {
                m_positiveMagnitudeID = value;
            }
        }

        /// <summary>
        /// Gets or sets the signal ID for the positive sequence phasor measurement.
        /// </summary>
        [ConnectionStringParameter,
        Description("Defines the output signal ID for the angle of the positive sequence phasor measurement; can be one of a filter expression, measurement key, point tag, or Guid."),
        CustomConfigurationEditor("GSF.TimeSeries.UI.WPF.dll", "GSF.TimeSeries.UI.Editors.MeasurementEditor", "selectable=false")]
        public Guid PositiveAngleID
        {
            get
            {
                return m_positiveAngleID;
            }
            set
            {
                m_positiveAngleID = value;
            }
        }

        /// <summary>
        /// Gets or sets the signal ID for the negative sequence phasor measurement.
        /// </summary>
        [ConnectionStringParameter,
        Description("Defines the output signal ID for the magnitude of the negative sequence phasor measurement; can be one of a filter expression, measurement key, point tag, or Guid."),
        CustomConfigurationEditor("GSF.TimeSeries.UI.WPF.dll", "GSF.TimeSeries.UI.Editors.MeasurementEditor", "selectable=false")]
        public Guid NegativeMagnitudeID
        {
            get
            {
                return m_negativeMagnitudeID;
            }
            set
            {
                m_negativeMagnitudeID = value;
            }
        }

        /// <summary>
        /// Gets or sets the signal ID for the negative sequence phasor measurement.
        /// </summary>
        [ConnectionStringParameter,
        Description("Defines the output signal ID for the angle of the negative sequence phasor measurement; can be one of a filter expression, measurement key, point tag, or Guid."),
        CustomConfigurationEditor("GSF.TimeSeries.UI.WPF.dll", "GSF.TimeSeries.UI.Editors.MeasurementEditor", "selectable=false")]
        public Guid NegativeAngleID
        {
            get
            {
                return m_negativeAngleID;
            }
            set
            {
                m_negativeAngleID = value;
            }
        }

        /// <summary>
        /// Gets or sets the signal ID for the zero sequence phasor measurement.
        /// </summary>
        [ConnectionStringParameter,
        Description("Defines the output signal ID for the magnitude of the zero sequence phasor measurement; can be one of a filter expression, measurement key, point tag, or Guid."),
        CustomConfigurationEditor("GSF.TimeSeries.UI.WPF.dll", "GSF.TimeSeries.UI.Editors.MeasurementEditor", "selectable=false")]
        public Guid ZeroMagnitudeID
        {
            get
            {
                return m_zeroMagnitudeID;
            }
            set
            {
                m_zeroMagnitudeID = value;
            }
        }

        /// <summary>
        /// Gets or sets the signal ID for the zero sequence phasor measurement.
        /// </summary>
        [ConnectionStringParameter,
        Description("Defines the output signal ID for the angle of the zero sequence phasor measurement; can be one of a filter expression, measurement key, point tag, or Guid."),
        CustomConfigurationEditor("GSF.TimeSeries.UI.WPF.dll", "GSF.TimeSeries.UI.Editors.MeasurementEditor", "selectable=false")]
        public Guid ZeroAngleID
        {
            get
            {
                return m_zeroAngleID;
            }
            set
            {
                m_zeroAngleID = value;
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
        Description("Define the sample size of the data, in seconds, to be monitored."),
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

                status.AppendLine(">> Defined Inputs:");
                status.AppendLine("---------------------------");
                status.AppendFormat("         Phase-A magnitude: {0}", this.GetSignalInfo(m_aPhaseMagnitudeID));
                status.AppendLine();
                status.AppendFormat("             Phase-A angle: {0}", this.GetSignalInfo(m_aPhaseAngleID));
                status.AppendLine();
                status.AppendFormat("         Phase-B magnitude: {0}", this.GetSignalInfo(m_bPhaseMagnitudeID));
                status.AppendLine();
                status.AppendFormat("             Phase-B angle: {0}", this.GetSignalInfo(m_bPhaseAngleID));
                status.AppendLine();
                status.AppendFormat("         Phase-C magnitude: {0}", this.GetSignalInfo(m_cPhaseMagnitudeID));
                status.AppendLine();
                status.AppendFormat("             Phase-C angle: {0}", this.GetSignalInfo(m_cPhaseAngleID));
                status.AppendLine();
                status.AppendLine();

                status.AppendLine(">> Defined Outputs:");
                status.AppendLine("---------------------------");
                status.AppendFormat("    Positive-seq magnitude: {0}", this.GetSignalInfo(m_positiveMagnitudeID));
                status.AppendLine();
                status.AppendFormat("        Positive-seq angle: {0}", this.GetSignalInfo(m_positiveAngleID));
                status.AppendLine();
                status.AppendFormat("    Negative-seq magnitude: {0}", this.GetSignalInfo(m_negativeMagnitudeID));
                status.AppendLine();
                status.AppendFormat("        Negative-seq angle: {0}", this.GetSignalInfo(m_negativeAngleID));
                status.AppendLine();
                status.AppendFormat("        Zero-seq magnitude: {0}", this.GetSignalInfo(m_zeroMagnitudeID));
                status.AppendLine();
                status.AppendFormat("            Zero-seq angle: {0}", this.GetSignalInfo(m_zeroAngleID));
                status.AppendLine();
                status.AppendLine();

                if (m_trackRecentValues)
                {
                    status.AppendLine();

                    status.Append("   Last positive sequences: ");

                    lock (m_positiveMagnitudeSample)
                    {
                        lock (m_positiveAngleSample)
                        {
                            // Display last several values
                            if (m_positiveMagnitudeSample.Count > ValuesToShow)
                            {
                                List<double> positiveMagnitudeSample = m_positiveMagnitudeSample.GetRange(m_positiveMagnitudeSample.Count - ValuesToShow - 1, ValuesToShow);
                                List<double> positiveAngleSample = m_positiveAngleSample.GetRange(m_positiveMagnitudeSample.Count - ValuesToShow - 1, ValuesToShow);

                                status.Append(positiveMagnitudeSample.Zip(positiveAngleSample, (mV, mA) => string.Format("{0:0.00} {1}, {2:0.00}°", mV, m_magnitudeUnits, mA)).ToDelimitedString("\r\n                            "));
                            }
                            else
                            {
                                status.Append("Not enough values calculated yet...");
                            }
                        }
                    }
                    status.AppendLine();

                    status.Append("   Last negative sequences: ");

                    lock (m_negativeMagnitudeSample)
                    {
                        lock (m_negativeAngleSample)
                        {
                            // Display last several values
                            if (m_positiveMagnitudeSample.Count > ValuesToShow)
                            {
                                List<double> negativeMagnitudeSample = m_negativeMagnitudeSample.GetRange(m_negativeMagnitudeSample.Count - ValuesToShow - 1, ValuesToShow);
                                List<double> negativeAngleSample = m_negativeAngleSample.GetRange(m_negativeMagnitudeSample.Count - ValuesToShow - 1, ValuesToShow);

                                status.Append(negativeMagnitudeSample.Zip(negativeAngleSample, (mV, mA) => string.Format("{0:0.00} {1}, {2:0.00}°", mV, m_magnitudeUnits, mA)).ToDelimitedString("\r\n                            "));
                            }
                            else
                            {
                                status.Append("Not enough values calculated yet...");
                            }
                        }
                    }
                    status.AppendLine();

                    status.Append("       Last zero sequences: ");

                    lock (m_zeroMagnitudeSample)
                    {
                        lock (m_zeroAngleSample)
                        {
                            // Display last several values
                            if (m_zeroMagnitudeSample.Count > ValuesToShow)
                            {
                                List<double> zeroMagnitudeSample = m_zeroMagnitudeSample.GetRange(m_zeroMagnitudeSample.Count - ValuesToShow - 1, ValuesToShow);
                                List<double> zeroAngleSample = m_zeroAngleSample.GetRange(m_zeroMagnitudeSample.Count - ValuesToShow - 1, ValuesToShow);

                                status.Append(zeroMagnitudeSample.Zip(zeroAngleSample, (mV, mA) => string.Format("{0:0.00} {1}, {2:0.00}°", mV, m_magnitudeUnits, mA)).ToDelimitedString("\r\n                            "));
                            }
                            else
                            {
                                status.Append("Not enough values calculated yet...");
                            }
                        }
                    }
                    status.AppendLine();
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
            Dictionary<string, string> settings;
            string setting;
            Guid signalID;

            SignalType signalType = default(SignalType);
            List<Guid> inputMagnitudeIDs;
            List<Guid> inputAngleIDs;
            List<SignalType> inputMagnitudeTypes;
            List<SignalType> inputAngleTypes;

            MeasurementKey key;
            string keyName;

            base.Initialize();
            settings = Settings;

            // Load required parameters

            if (this.TryParseSignalID("aPhaseMagnitudeID", out signalID))
                m_aPhaseAngleID = signalID;
            else
                throw new InvalidOperationException("aPhaseMagnitudeID connection string parameter is invalid or not defined. A-phase magnitude is required for sequence calculation.");

            if (this.TryParseSignalID("aPhaseAngleID", out signalID))
                m_aPhaseAngleID = signalID;
            else
                throw new InvalidOperationException("aPhaseAngleID connection string parameter is invalid or not defined. A-phase angle is required for sequence calculation.");

            if (this.TryParseSignalID("bPhaseMagnitudeID", out signalID))
                m_bPhaseAngleID = signalID;
            else
                throw new InvalidOperationException("bPhaseMagnitudeID connection string parameter is invalid or not defined. B-phase magnitude is required for sequence calculation.");

            if (this.TryParseSignalID("bPhaseAngleID", out signalID))
                m_bPhaseAngleID = signalID;
            else
                throw new InvalidOperationException("bPhaseAngleID connection string parameter is invalid or not defined. B-phase angle is required for sequence calculation.");

            if (this.TryParseSignalID("cPhaseMagnitudeID", out signalID))
                m_cPhaseAngleID = signalID;
            else
                throw new InvalidOperationException("cPhaseMagnitudeID connection string parameter is invalid or not defined. C-phase magnitude is required for sequence calculation.");

            if (this.TryParseSignalID("cPhaseAngleID", out signalID))
                m_cPhaseAngleID = signalID;
            else
                throw new InvalidOperationException("cPhaseAngleID connection string parameter is invalid or not defined. C-phase angle is required for sequence calculation.");

            // Load optional parameters

            if (settings.TryGetValue("trackRecentValues", out setting))
                m_trackRecentValues = setting.ParseBoolean();
            else
                m_trackRecentValues = true;

            if (settings.TryGetValue("sampleSize", out setting))            // Data sample size to monitor, in seconds
                m_sampleSize = int.Parse(setting);
            else
                m_sampleSize = 5;

            // Validate parameters
            inputMagnitudeIDs = new List<Guid>()
            {
                m_aPhaseMagnitudeID,
                m_bPhaseMagnitudeID,
                m_cPhaseMagnitudeID
            };

            inputAngleIDs = new List<Guid>()
            {
                m_aPhaseAngleID,
                m_bPhaseAngleID,
                m_cPhaseAngleID
            };

            inputMagnitudeTypes = inputMagnitudeIDs
                .Where(id => this.TryGetSignalType(id, out signalType))
                .Select(id => signalType)
                .ToList();

            inputAngleTypes = inputAngleIDs
                .Where(id => this.TryGetSignalType(id, out signalType))
                .Select(id => signalType)
                .ToList();

            if (inputMagnitudeTypes.Count < 3 || inputAngleTypes.Count < 3)
                throw new InvalidOperationException("Types of all input signals must be defined.");

            if (inputMagnitudeTypes.All(type => type == SignalType.VPHM) && inputAngleTypes.All(type => type == SignalType.VPHA))
                m_magnitudeUnits = "Volts";
            else if (inputMagnitudeTypes.All(type => type == SignalType.IPHM) && inputAngleTypes.All(type => type == SignalType.IPHA))
                m_magnitudeUnits = "Amps";
            else
                throw new InvalidOperationException("Input signals must either be all voltages or all currents.");

            // Load input signal IDs into the set of input signal IDs
            InputSignalIDs.UnionWith(new List<Guid>()
            {
                m_aPhaseMagnitudeID, m_aPhaseAngleID,
                m_bPhaseMagnitudeID, m_bPhaseAngleID,
                m_cPhaseMagnitudeID, m_cPhaseAngleID
            });

            // Load output signal IDs into the set of output signal IDs
            OutputSignalIDs.UnionWith(new List<Guid>()
            {
                m_positiveMagnitudeID, m_positiveAngleID,
                m_negativeMagnitudeID, m_negativeAngleID,
                m_zeroMagnitudeID, m_zeroAngleID
            });

            if (m_trackRecentValues)
            {
                m_positiveMagnitudeSample = new List<double>();
                m_positiveAngleSample = new List<double>();
                m_negativeMagnitudeSample = new List<double>();
                m_negativeAngleSample = new List<double>();
                m_zeroMagnitudeSample = new List<double>();
                m_zeroAngleSample = new List<double>();
            }

            // Assign a default adapter name to be used if sequence calculator is loaded as part of automated collection
            if (string.IsNullOrWhiteSpace(Name))
            {
                keyName = m_aPhaseMagnitudeID.ToString();

                if (this.TryGetMeasurementKey(m_aPhaseMagnitudeID, out key))
                    keyName = key.ToString();

                Name = string.Format("SC!{0}", keyName);
            }
        }

        /// <summary>
        /// Publish frame of time-aligned collection of measurement values that arrived within the defined lag time.
        /// </summary>
        /// <param name="frame">Frame of measurements with the same timestamp that arrived within lag time that are ready for processing.</param>
        /// <param name="index">Index of frame within a second ranging from zero to frames per second - 1.</param>
        protected override void PublishFrame(IFrame frame, int index)
        {
            ComplexNumber positiveSequence = new ComplexNumber(double.NaN, double.NaN);
            ComplexNumber negativeSequence = new ComplexNumber(double.NaN, double.NaN);
            ComplexNumber zeroSequence = new ComplexNumber(double.NaN, double.NaN);

            IMeasurement<double> measurement;
            bool allValuesReceived;

            double mA;
            double aA;
            double mB;
            double aB;
            double mC;
            double aC;

            try
            {
                allValuesReceived = false;

                mA = 0.0D;
                aA = 0.0D;
                mB = 0.0D;
                aB = 0.0D;
                mC = 0.0D;
                aC = 0.0D;

                // Get all needed measurement values from this frame
                if (frame.TryGetEntity(m_aPhaseMagnitudeID, out measurement) && measurement.ValueQualityIsGood())
                {
                    // Get A-phase magnitude value
                    mA = measurement.Value;

                    if (frame.TryGetEntity(m_aPhaseAngleID, out measurement) && measurement.ValueQualityIsGood())
                    {
                        // Get A-phase angle value
                        aA = measurement.Value;

                        if (frame.TryGetEntity(m_bPhaseMagnitudeID, out measurement) && measurement.ValueQualityIsGood())
                        {
                            // Get B-phase magnitude value
                            mB = measurement.Value;

                            if (frame.TryGetEntity(m_bPhaseAngleID, out measurement) && measurement.ValueQualityIsGood())
                            {
                                // Get B-phase angle value
                                aB = measurement.Value;

                                if (frame.TryGetEntity(m_cPhaseMagnitudeID, out measurement) && measurement.ValueQualityIsGood())
                                {
                                    // Get C-phase magnitude value
                                    mC = measurement.Value;

                                    if (frame.TryGetEntity(m_cPhaseAngleID, out measurement) && measurement.ValueQualityIsGood())
                                    {
                                        // Get C-phase angle value
                                        aC = measurement.Value;

                                        allValuesReceived = true;
                                    }
                                }
                            }
                        }
                    }
                }


                if (allValuesReceived)
                {
                    ComplexNumber aPhase = new ComplexNumber(Angle.FromDegrees(aA), mA);
                    ComplexNumber bPhase = new ComplexNumber(Angle.FromDegrees(aB), mB);
                    ComplexNumber cPhase = new ComplexNumber(Angle.FromDegrees(aC), mC);

                    zeroSequence = (aPhase + bPhase + cPhase) / 3.0D;
                    positiveSequence = (aPhase + a * bPhase + aSq * cPhase) / 3.0D;
                    negativeSequence = (aPhase + aSq * bPhase + a * cPhase) / 3.0D;

                    if (m_trackRecentValues)
                    {
                        // Add latest positive sequence to data sample
                        lock (m_positiveMagnitudeSample)
                        {
                            m_positiveMagnitudeSample.Add(positiveSequence.Magnitude);

                            // Maintain sample size
                            while (m_positiveMagnitudeSample.Count > m_sampleSize)
                                m_positiveMagnitudeSample.RemoveAt(0);
                        }

                        lock (m_positiveAngleSample)
                        {
                            m_positiveAngleSample.Add(positiveSequence.Angle);

                            // Maintain sample size
                            while (m_positiveAngleSample.Count > m_sampleSize)
                                m_positiveAngleSample.RemoveAt(0);
                        }

                        // Add latest negative sequence to data sample
                        lock (m_negativeMagnitudeSample)
                        {
                            m_negativeMagnitudeSample.Add(negativeSequence.Magnitude);

                            // Maintain sample size
                            while (m_negativeMagnitudeSample.Count > m_sampleSize)
                                m_negativeMagnitudeSample.RemoveAt(0);
                        }

                        lock (m_negativeAngleSample)
                        {
                            m_negativeAngleSample.Add(negativeSequence.Angle);

                            // Maintain sample size
                            while (m_negativeAngleSample.Count > m_sampleSize)
                                m_negativeAngleSample.RemoveAt(0);
                        }

                        // Add latest zero sequence to data sample
                        lock (m_zeroMagnitudeSample)
                        {
                            m_zeroMagnitudeSample.Add(zeroSequence.Magnitude);

                            // Maintain sample size
                            while (m_zeroMagnitudeSample.Count > m_sampleSize)
                                m_zeroMagnitudeSample.RemoveAt(0);
                        }

                        lock (m_zeroAngleSample)
                        {
                            m_zeroAngleSample.Add(zeroSequence.Angle);

                            // Maintain sample size
                            while (m_zeroAngleSample.Count > m_sampleSize)
                                m_zeroAngleSample.RemoveAt(0);
                        }
                    }
                }
            }
            finally
            {
                // Provide calculated measurements for external consumption
                OnNewEntities(new IMeasurement[]
                {
                    new Measurement<double>(m_positiveMagnitudeID, frame.Timestamp, positiveSequence.Magnitude),
                    new Measurement<double>(m_positiveAngleID, frame.Timestamp, positiveSequence.Angle),
                    new Measurement<double>(m_negativeMagnitudeID, frame.Timestamp, negativeSequence.Magnitude),
                    new Measurement<double>(m_negativeAngleID, frame.Timestamp, negativeSequence.Angle),
                    new Measurement<double>(m_zeroMagnitudeID, frame.Timestamp, zeroSequence.Magnitude),
                    new Measurement<double>(m_zeroAngleID, frame.Timestamp, zeroSequence.Angle)
                });
            }
        }

        #endregion

        #region [ Static ]

        // a = e^((2/3) * pi * i)
        private static readonly ComplexNumber a = new ComplexNumber(Math.Cos(Rad120), Math.Sin(Rad120));
        private static readonly ComplexNumber aSq = a * a;

        #endregion
    }
}