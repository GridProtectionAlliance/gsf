//******************************************************************************************************
//  StateFlagsTransferAdapter.cs - Gbtc
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
//  11/22/2017 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using GSF;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;

namespace DataQualityMonitoring
{
    /// <summary>
    /// Transfers a set of flags from a collection of source measurements to their corresponding destination measurements.
    /// </summary>
    [Description("State Transfer: Transfers a set of flags from a collection of source measurements to their corresponding destination measurements.")]
    public class StateFlagsTransferAdapter : FilterAdapterBase
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Default value for the <see cref="Flags"/> property.
        /// </summary>
        public const MeasurementStateFlags DefaultFlags = MeasurementStateFlags.Normal;

        /// <summary>
        /// Default value for the <see cref="SupportsTemporalProcessing"/> property.
        /// </summary>
        public const bool DefaultSupportsTemporalProcessing = false;

        // Fields
        private MeasurementStateFlags m_flags;
        private MeasurementKey[] m_sourceMeasurements;
        private MeasurementKey[] m_destinationMeasurements;
        private bool m_supportsTemporalProcessing;

        private Dictionary<MeasurementKey, MeasurementKey> m_sourceToDestinationLookup;
        private ConcurrentDictionary<MeasurementKey, MeasurementStateFlags> m_destinationToStateLookup;

        #endregion

        #region [ Constructors ]

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the collection of measurements from which state will be transfered.
        /// </summary>
        [ConnectionStringParameter,
        Description("Defines the collection of measurements from which state will be transfered.")]
        public MeasurementKey[] SourceMeasurements
        {
            get
            {
                return m_sourceMeasurements;
            }
            set
            {
                m_sourceMeasurements = value;
            }
        }

        /// <summary>
        /// Gets or sets the collection of measurements to which state will be transferred.
        /// </summary>
        [ConnectionStringParameter,
        Description("Defines the collection of measurements to which state will be transferred.")]
        public MeasurementKey[] DestinationMeasurements
        {
            get
            {
                return m_destinationMeasurements;
            }
            set
            {
                m_destinationMeasurements = value;
            }
        }

        /// <summary>
        /// Gets or sets the flags to be transfered from source
        /// measurements to the corresponding destination measurements.
        /// </summary>
        [ConnectionStringParameter,
        DefaultValue(DefaultFlags),
        Description("Defines the set of flags to be transfered.")]
        public MeasurementStateFlags Flags
        {
            get
            {
                return m_flags;
            }
            set
            {
                m_flags = value;
            }
        }

        /// <summary>
        /// Gets the flag indicating if this adapter supports temporal processing.
        /// </summary>
        [ConnectionStringParameter,
        Description("Define the flag indicating if this adapter supports temporal processing."),
        DefaultValue(DefaultSupportsTemporalProcessing)]
        public override bool SupportsTemporalProcessing
        {
            get
            {
                return m_supportsTemporalProcessing;
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

            // Load required parameters

            if (settings.TryGetValue(nameof(SourceMeasurements), out setting))
                m_sourceMeasurements = ParseInputMeasurementKeys(DataSource, true, setting);
            else
                throw new ArgumentException("Missing required connection string parameter: SourceMeasurements", nameof(SourceMeasurements));

            if (settings.TryGetValue(nameof(DestinationMeasurements), out setting))
                m_destinationMeasurements = ParseInputMeasurementKeys(DataSource, true, setting);
            else
                throw new ArgumentException("Missing required connection string parameter: DestinationMeasurements", nameof(DestinationMeasurements));

            if (m_sourceMeasurements.Length != m_destinationMeasurements.Length)
                throw new ArgumentException($"Source and destination measurements are parallel arrays, therefore their lengths must match - Src: {m_sourceMeasurements.Length}, Dst: {m_destinationMeasurements.Length}");

            InputMeasurementKeys = m_sourceMeasurements.Concat(m_destinationMeasurements).ToArray();

            Dictionary<MeasurementKey, MeasurementKey> sourceToDestinationLookup = m_sourceMeasurements
                .Zip(m_destinationMeasurements, (Src, Dst) => new { Src, Dst })
                .ToDictionary(obj => obj.Src, obj => obj.Dst);

            Dictionary<MeasurementKey, MeasurementStateFlags> destinationToStateLookup = m_destinationMeasurements
                .ToDictionary(dst => dst, dst => MeasurementStateFlags.Normal);

            m_sourceToDestinationLookup = sourceToDestinationLookup;
            m_destinationToStateLookup = new ConcurrentDictionary<MeasurementKey, MeasurementStateFlags>(destinationToStateLookup);

            // Load optional parameters

            if (!settings.TryGetValue(nameof(Flags), out setting) || !Enum.TryParse(setting, out m_flags))
                m_flags = DefaultFlags;

            if (settings.TryGetValue(nameof(SupportsTemporalProcessing), out setting))
                m_supportsTemporalProcessing = setting.ParseBoolean();
            else
                m_supportsTemporalProcessing = DefaultSupportsTemporalProcessing;
        }

        /// <summary>
        /// Processes the measurements to apply the flags to signals with raised alarms.
        /// </summary>
        /// <param name="measurements">The collection of measurements to be processed.</param>
        protected override void ProcessMeasurements(IEnumerable<IMeasurement> measurements)
        {
            foreach (IMeasurement measurement in measurements)
            {
                MeasurementKey destinationKey;
                MeasurementStateFlags sourceFlags;

                if (m_sourceToDestinationLookup.TryGetValue(measurement.Key, out destinationKey))
                    m_destinationToStateLookup[destinationKey] = (measurement.StateFlags & m_flags);
                else if (m_destinationToStateLookup.TryGetValue(measurement.Key, out sourceFlags))
                    measurement.StateFlags |= sourceFlags;
            }
        }

        #endregion
    }
}
