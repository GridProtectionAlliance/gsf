//******************************************************************************************************
//  SetFlagsFilterAdapter.cs - Gbtc
//
//  Copyright © 2017, Grid Protection Alliance.  All Rights Reserved.
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
//  11/17/2017 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;

namespace TestingAdapters
{
    /// <summary>
    /// Forces a set of flags to be turned on for a set of inputs.
    /// </summary>
    [Description("Set Flags: Forces a set of flags to be turned on for a set of inputs.")]
    public class SetFlagsFilterAdapter : FilterAdapterBase
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Default value for the <see cref="Flags"/> property.
        /// </summary>
        public const MeasurementStateFlags DefaultFlags = MeasurementStateFlags.Normal;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the set of flags to be forced on.
        /// </summary>
        [ConnectionStringParameter,
        DefaultValue(DefaultFlags),
        Description("Defines the set of flags to be forced on.")]
        public MeasurementStateFlags Flags { get; set; }

        /// <summary>
        /// Gets the flag indicating if this adapter supports temporal processing (it doesn't).
        /// </summary>
        public override bool SupportsTemporalProcessing
        {
            get
            {
                return false;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes the adapter's settings.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            Dictionary<string, string> settings = Settings;
            string setting;
            MeasurementStateFlags flags;

            if (settings.TryGetValue(nameof(Flags), out setting) && Enum.TryParse(setting, out flags))
                Flags = flags;
            else
                Flags = DefaultFlags;
        }

        /// <summary>
        /// Processes the measurements to apply the flags.
        /// </summary>
        /// <param name="measurements">The set of measurements to which to apply the flags.</param>
        protected override void ProcessMeasurements(IEnumerable<IMeasurement> measurements)
        {
            foreach (IMeasurement measurement in measurements)
                measurement.StateFlags |= Flags;
        }

        #endregion
    }
}
