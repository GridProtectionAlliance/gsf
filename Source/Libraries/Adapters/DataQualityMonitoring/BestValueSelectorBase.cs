//******************************************************************************************************
//  BestValueSelectorBase.cs - Gbtc
//
//  Copyright © 2026, Grid Protection Alliance.  All Rights Reserved.
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
//  10/30/2026 - J. Ritchie Carroll
//       Generated original version of source code, extracted from BestValueSelector.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using GSF;
using GSF.Diagnostics;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;

namespace DataQualityMonitoring
{
    /// <summary>
    /// Represents a base class for adapters that select the best values from redundant input measurements.
    /// </summary>
    /// <remarks>
    /// Defines the quality settings shared by best value selection adapters and the allocation-free
    /// evaluation of measurement quality used while processing frames.
    /// </remarks>
    public abstract class BestValueSelectorBase : ActionAdapterBase
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Defines the default value for the <see cref="PublishBadValues"/> property.
        /// </summary>
        public const bool DefaultPublishBadValues = false;

        /// <summary>
        /// Defines the default value for the <see cref="BadFlags"/> property.
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

        // Badness score weights - NaN outranks zero, which outranks any count of bad flags (at most 32)
        private const int NaNBadnessWeight = 1 << 12;
        private const int ZeroBadnessWeight = 1 << 11;

        // Fields
        private bool m_publishBadValues;
        private MeasurementStateFlags m_badFlags;
        private bool m_handleZeroAsBad;
        private bool m_handleNaNAsBad;
        private bool m_supportsTemporalProcessing;

        #endregion

        #region [ Properties ]

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

                return statusBuilder.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes the <see cref="BestValueSelectorBase"/>.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            Dictionary<string, string> settings = Settings;
            string setting;

            if (settings.TryGetValue(nameof(PublishBadValues), out setting))
                m_publishBadValues = setting.ParseBoolean();
            else
                m_publishBadValues = DefaultPublishBadValues;

            if (!settings.TryGetValue(nameof(BadFlags), out setting))
            {
                m_badFlags = DefaultBadFlags;
            }
            else if (!Enum.TryParse(setting, true, out m_badFlags))
            {
                m_badFlags = DefaultBadFlags;
                OnStatusMessage(MessageLevel.Warning, $"Failed to parse {nameof(BadFlags)} value \"{setting}\", default bad flags were selected.", flags: MessageFlags.UsageIssue);
            }

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
        /// Determines if <paramref name="measurement"/> is good per the configured quality settings.
        /// </summary>
        /// <param name="measurement">Measurement to evaluate, can be <c>null</c>.</param>
        /// <returns><c>true</c> if <paramref name="measurement"/> is defined and good; otherwise, <c>false</c>.</returns>
        protected bool IsGood(IMeasurement measurement)
        {
            if (measurement is null)
                return false;

            double value = measurement.Value;

            if (m_handleNaNAsBad && double.IsNaN(value))
                return false;

            if (m_handleZeroAsBad && value == 0.0D)
                return false;

            return (measurement.StateFlags & m_badFlags) == MeasurementStateFlags.Normal;
        }

        /// <summary>
        /// Gets a score that ranks how bad <paramref name="measurement"/> is, where lower is better and zero is good.
        /// </summary>
        /// <param name="measurement">Measurement to score.</param>
        /// <returns>Badness score of <paramref name="measurement"/>.</returns>
        /// <remarks>
        /// Ranks NaN values (when handled as bad) worst, then zero values (when handled as bad), then
        /// by the number of configured bad flags that are set. Callers selecting the least bad value
        /// should keep the first of equal scores so that input priority breaks ties.
        /// </remarks>
        protected int GetBadnessScore(IMeasurement measurement)
        {
            double value = measurement.Value;
            int score = CountFlags(measurement.StateFlags & m_badFlags);

            if (m_handleNaNAsBad && double.IsNaN(value))
                score += NaNBadnessWeight;

            if (m_handleZeroAsBad && value == 0.0D)
                score += ZeroBadnessWeight;

            return score;
        }

        #endregion

        #region [ Static ]

        // Static Methods
        private static int CountFlags(MeasurementStateFlags flags)
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
