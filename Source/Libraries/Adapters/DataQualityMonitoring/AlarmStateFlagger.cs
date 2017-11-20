//******************************************************************************************************
//  AlarmStateFlagger.cs - Gbtc
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
//  11/20/2017 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using GSF;
using GSF.Data;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;

namespace DataQualityMonitoring
{
    /// <summary>
    /// Forces a set of flags to be turned on for a collection of inputs while their alarms are raised.
    /// </summary>
    [Description("Alarm Flags: Forces a set of flags to be turned on for a collection of inputs while their alarms are raised.")]
    public class AlarmStateFlagger : FilterAdapterBase
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
        private bool m_supportsTemporalProcessing;

        private Dictionary<MeasurementKey, MeasurementKey> m_alarmToSignalLookup;
        private ConcurrentDictionary<MeasurementKey, bool> m_signalToAlarmStateLookup;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the flags to be forced on while alarm is raised.
        /// </summary>
        [ConnectionStringParameter,
        DefaultValue(DefaultFlags),
        Description("Defines the set of flags to be forced on.")]
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
            MeasurementStateFlags flags;

            if (settings.TryGetValue(nameof(Flags), out setting) && Enum.TryParse(setting, out flags))
                m_flags = flags;
            else
                m_flags = DefaultFlags;

            if (settings.TryGetValue(nameof(SupportsTemporalProcessing), out setting))
                m_supportsTemporalProcessing = setting.ParseBoolean();
            else
                m_supportsTemporalProcessing = DefaultSupportsTemporalProcessing;

            const string AlarmTable = "Alarms";
            const string AlarmIDField = "AssociatedMeasurementID";
            const string SignalIDField = "SignalID";

            HashSet<MeasurementKey> inputMeasurementKeys = new HashSet<MeasurementKey>(InputMeasurementKeys);

            Func<DataRow, string, MeasurementKey> getKey = (row, field) =>
                MeasurementKey.LookUpBySignalID(row.ConvertField<Guid>(field));

            Dictionary<MeasurementKey, MeasurementKey> alarmToSignalLookup = DataSource.Tables[AlarmTable]
                .Select(AlarmIDField + " IS NOT NULL")
                .Select(row => new { AlarmID = getKey(row, AlarmIDField), SignalID = getKey(row, SignalIDField) })
                .Where(obj => inputMeasurementKeys.Contains(obj.AlarmID))
                .Where(obj => inputMeasurementKeys.Contains(obj.SignalID))
                .ToDictionary(obj => obj.AlarmID, obj => obj.SignalID);

            Dictionary<MeasurementKey, bool> signalToAlarmStateLookup = alarmToSignalLookup.Values
                .ToDictionary(key => key, key => false);

            m_alarmToSignalLookup = alarmToSignalLookup;
            m_signalToAlarmStateLookup = new ConcurrentDictionary<MeasurementKey, bool>(signalToAlarmStateLookup);
        }

        /// <summary>
        /// Processes the measurements to apply the flags to signals with raised alarms.
        /// </summary>
        /// <param name="measurements">The collection of measurements to be processed.</param>
        protected override void ProcessMeasurements(IEnumerable<IMeasurement> measurements)
        {
            IDictionary<MeasurementKey, MeasurementKey> alarmToSignalLookup = m_alarmToSignalLookup;
            IDictionary<MeasurementKey, bool> signalToAlarmStateLookup = m_signalToAlarmStateLookup;

            foreach (IMeasurement measurement in measurements)
            {
                MeasurementKey signalKey;
                bool alarmState;

                if (alarmToSignalLookup.TryGetValue(measurement.Key, out signalKey))
                    m_signalToAlarmStateLookup[signalKey] = (measurement.AdjustedValue != 0.0D);
                else if (signalToAlarmStateLookup.TryGetValue(measurement.Key, out alarmState) && alarmState)
                    measurement.StateFlags |= m_flags;
            }
        }

        #endregion
    }
}
