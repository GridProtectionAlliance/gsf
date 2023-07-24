//******************************************************************************************************
//  NormalizeSubsecondTimestampsFilterAdapter.cs - Gbtc
//
//  Copyright © 2023, Grid Protection Alliance.  All Rights Reserved.
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
//  07/24/2023 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

// Ignore Spelling: Subsecond

using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;
using GSF;
using System.Collections.Generic;
using System.ComponentModel;

namespace TestingAdapters
{
    /// <summary>
    /// Normalizes subsecond timestamps for configured input measurements.
    /// </summary>
    [Description("Normalize Subsecond Timestamps: Updates subsecond timestamps for configured inputs to be normalized by defined frame rate.")]
    public class NormalizeSubsecondTimestampsFilterAdapter : FilterAdapterBase
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Default value for the <see cref="FramesPerSecond"/> property.
        /// </summary>
        public const int DefaultFramesPerSecond = 30;

        /// <summary>
        /// Default value for the <see cref="SupportsTemporalProcessing"/> property.
        /// </summary>
        public const bool DefaultSupportsTemporalProcessing = false;

        // Fields
        private bool m_supportsTemporalProcessing;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the number of frames per second used for subsecond timestamp normalization.
        /// </summary>
        [ConnectionStringParameter]
        [Description("Defines the number of frames per second used for subsecond timestamp normalization.")]
        [DefaultValue(DefaultFramesPerSecond)]
        public int FramesPerSecond { get; set; } = DefaultFramesPerSecond;

        /// <summary>
        /// Gets the flag indicating if this adapter supports temporal processing.
        /// </summary>
        /// <remarks>
        /// Use case exists when replaying historical data that subsecond timestamp normalization might be useful.
        /// </remarks>
        [ConnectionStringParameter]
        [Description("Define the flag indicating if this adapter supports temporal processing.")]
        [DefaultValue(DefaultSupportsTemporalProcessing)]
        public override bool SupportsTemporalProcessing => m_supportsTemporalProcessing;

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes <see cref="NormalizeSubsecondTimestampsFilterAdapter"/>.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            Dictionary<string, string> settings = Settings;

            if (settings.TryGetValue(nameof(FramesPerSecond), out string setting) && int.TryParse(setting, out int value))
                FramesPerSecond = value;

            if (settings.TryGetValue(nameof(SupportsTemporalProcessing), out setting))
                m_supportsTemporalProcessing = setting.ParseBoolean();
        }

        /// <summary>
        /// Processes the measurements to normalize subsecond timestamps.
        /// </summary>
        /// <param name="measurements">The set of measurements for which to normalize subsecond timestamps.</param>
        protected override void ProcessMeasurements(IEnumerable<IMeasurement> measurements)
        {
            foreach (IMeasurement measurement in measurements)
                measurement.Timestamp = Ticks.RoundToSubsecondDistribution(measurement.Timestamp, FramesPerSecond);
        }

        #endregion
    }
}
