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
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using GSF;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;

namespace DataQualityMonitoring
{
    /// <summary>
    /// Produces a new signal by selecting the best values
    /// from a collection of concentrated data points.
    /// </summary>
    [Description("Best Value: Produces a new signal by selecting only the best values from a collection of input signals.")]
    public class BestValueSelector : ActionAdapterBase
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Defines the default value for the <see cref="PublishBadValues"/> property.
        /// </summary>
        public const bool DefaultPublishBadValues = false;

        /// <summary>
        /// Defines the default value for the <see cref="DefaultBadFlags"/> property.
        /// </summary>
        public const MeasurementStateFlags DefaultBadFlags =
            MeasurementStateFlags.AlarmHigh |
            MeasurementStateFlags.AlarmLow |
            MeasurementStateFlags.BadData |
            MeasurementStateFlags.CalculationError |
            MeasurementStateFlags.ComparisonAlarm |
            MeasurementStateFlags.FlatlineAlarm |
            MeasurementStateFlags.MeasurementError |
            MeasurementStateFlags.OverRangeError |
            MeasurementStateFlags.ReceivedAsBad |
            MeasurementStateFlags.SuspectData |
            MeasurementStateFlags.UnderRangeError;

        /// <summary>
        /// Defines the default value for the <see cref="HandleZeroAsBad"/> property.
        /// </summary>
        public const bool DefaultHandleZeroAsBad = false;

        /// <summary>
        /// Defines the default value for the <see cref="HandleNaNAsBad"/> property.
        /// </summary>
        public const bool DefaultHandleNaNAsBad = true;

        /// <summary>
        /// Defines the default value for the <see cref="SupportsTemporalProcessing"/> property.
        /// </summary>
        public const bool DefaultSupportsTemporalProcessing = false;

        // Fields
        private bool m_publishBadValues;
        private MeasurementStateFlags m_badFlags;
        private bool m_handleZeroAsBad;
        private bool m_handleNaNAsBad;
        private bool m_supportsTemporalProcessing;
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
        /// Gets or sets the value that determines whether the adapter should
        /// publish measurements if all of its inputs are found to be bad.
        /// </summary>
        [ConnectionStringParameter,
        DefaultValue(DefaultPublishBadValues),
        Description("Define the flag that indicates whether this adapter should publish measurements if all inputs are bad.")]
        public bool PublishBadValues
        {
            get
            {
                return m_publishBadValues;
            }
            set
            {
                m_publishBadValues = value;
            }
        }

        /// <summary>
        /// Gets or sets the flags that should be considered bad by this adapter.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the flags that are considered bad by this adapter."),
        DefaultValue(DefaultBadFlags)]
        public MeasurementStateFlags BadFlags
        {
            get
            {
                return m_badFlags;
            }
            set
            {
                m_badFlags = value;
            }
        }

        /// <summary>
        /// Gets the flag indicating if this adapter considers zero to be a bad value.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the flag indicating if this adapter considers zero to be a bad value."),
        DefaultValue(DefaultHandleZeroAsBad)]
        public bool HandleZeroAsBad
        {
            get
            {
                return m_handleZeroAsBad;
            }
            set
            {
                m_handleZeroAsBad = value;
            }
        }

        /// <summary>
        /// Gets the flag indicating if this adapter considers not-a-number to be a bad value.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the flag indicating if this adapter considers not-a-number to be a bad value."),
        DefaultValue(DefaultHandleNaNAsBad)]
        public bool HandleNaNAsBad
        {
            get
            {
                return m_handleNaNAsBad;
            }
            set
            {
                m_handleNaNAsBad = value;
            }
        }

        /// <summary>
        /// Gets the flag indicating if this adapter supports temporal processing.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the flag indicating if this adapter supports temporal processing."),
        DefaultValue(DefaultSupportsTemporalProcessing)]
        public override bool SupportsTemporalProcessing => m_supportsTemporalProcessing;

        /// <summary>
        /// Returns the detailed status of the action adapter.
        /// </summary>
        public override string Status
        {
            get
            {
                StringBuilder statusBuilder = new StringBuilder(base.Status);

                IEnumerable<MeasurementStateFlags> badFlags = Enum.GetValues(typeof(MeasurementStateFlags))
                    .Cast<MeasurementStateFlags>()
                    .Where(flag => flag != MeasurementStateFlags.Normal &&  m_badFlags.HasFlag(flag));

                string badFlagsText = string.Join("," + Environment.NewLine + "                              ", badFlags);

                statusBuilder.AppendLine();
                statusBuilder.AppendLine($"          Publish bad values: {(m_publishBadValues ? "Yes" : "No")}");
                statusBuilder.AppendLine($"                   Bad Flags: {badFlagsText}");
                statusBuilder.AppendLine($"          Handle Zero As Bad: {(m_handleZeroAsBad ? "Yes" : "No")}");
                statusBuilder.AppendLine($"           Handle NaN As Bad: {(m_handleNaNAsBad ? "Yes" : "No")}");
                statusBuilder.AppendLine($"Supports Temporal Processing: {(m_supportsTemporalProcessing ? "Yes" : "No")}");
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

            Dictionary<string, string> settings = Settings;
            string setting;

            if (settings.TryGetValue(nameof(PublishBadValues), out setting))
                m_publishBadValues = setting.ParseBoolean();
            else
                m_publishBadValues = DefaultHandleZeroAsBad;

            if (!settings.TryGetValue(nameof(BadFlags), out setting) || !Enum.TryParse(setting, out m_badFlags))
                m_badFlags = DefaultBadFlags;

            if (settings.TryGetValue(nameof(HandleZeroAsBad), out setting))
                m_handleZeroAsBad = setting.ParseBoolean();
            else
                m_handleZeroAsBad = DefaultHandleZeroAsBad;

            if (settings.TryGetValue(nameof(HandleNaNAsBad), out setting))
                m_handleNaNAsBad = setting.ParseBoolean();
            else
                m_handleNaNAsBad = DefaultHandleNaNAsBad;

            if (settings.TryGetValue(nameof(SupportsTemporalProcessing), out setting))
                m_supportsTemporalProcessing = setting.ParseBoolean();
            else
                m_supportsTemporalProcessing = DefaultSupportsTemporalProcessing;
        }

        /// <summary>
        /// Publish <see cref="IFrame"/> of time-aligned collection of <see cref="IMeasurement"/> values that arrived within the
        /// concentrator's defined <see cref="ConcentratorBase.LagTime"/>.
        /// </summary>
        /// <param name="frame"><see cref="IFrame"/> of measurements with the same timestamp that arrived within <see cref="ConcentratorBase.LagTime"/> that are ready for processing.</param>
        /// <param name="index">Index of <see cref="IFrame"/> within a second ranging from zero to <c><see cref="ConcentratorBase.FramesPerSecond"/> - 1</c>.</param>
        protected override void PublishFrame(IFrame frame, int index)
        {
            IMeasurement bestMeasurement = GetBestMeasurement(frame);

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
                newMeasurements[1].Value = Array.IndexOf(InputMeasurementKeys, bestMeasurement.Key) + 1;
            }

            OnNewMeasurements(newMeasurements);

            m_lastSelectedMeasurement = bestMeasurement.Key;
        }

        private IMeasurement GetBestMeasurement(IFrame frame)
        {
            IMeasurement measurement;

            if (m_publishBadValues)
            {
                return InputMeasurementKeys
                    .Select(key => frame.Measurements.TryGetValue(key, out measurement) ? measurement : null)
                    .Where(m => (object)m != null)
                    .OrderBy(m => (m_handleNaNAsBad && double.IsNaN(m.Value)) ? 1 : 0)
                    .ThenBy(m => (m_handleZeroAsBad && m.Value == 0.0D) ? 1 : 0)
                    .ThenBy(m => CountFlags(m.StateFlags & m_badFlags))
                    .FirstOrDefault();
            }

            return InputMeasurementKeys
                .Select(key => frame.Measurements.TryGetValue(key, out measurement) ? measurement : null)
                .Where(m => (object)m != null)
                .Where(m => !m_handleNaNAsBad || !double.IsNaN(m.Value))
                .Where(m => !m_handleZeroAsBad || m.Value != 0.0D)
                .Where(m => (m.StateFlags & m_badFlags) == MeasurementStateFlags.Normal)
                .FirstOrDefault();
        }

        private int CountFlags(MeasurementStateFlags flags)
        {
            if (flags == MeasurementStateFlags.Normal)
                return 0;

            uint count = (uint)flags;

            count = count - ((count >> 1) & 0x55555555u);
            count = ((count >> 2) & 0x33333333u) + (count & 0x33333333u);
            count = ((count >> 4) + count) & 0x0F0F0F0Fu;
            count = ((count >> 8) + count) & 0x00FF00FFu;
            count = ((count >> 16) + count) & 0x0000FFFFu;

            return (int)count;
        }

        #endregion
    }
}
