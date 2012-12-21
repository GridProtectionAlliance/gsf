//******************************************************************************************************
//  CalculatedMeasurementBase.cs - Gbtc
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
//  10/19/2009 - J. Ritchie Carroll
//       Generated original version of source code.
//  04/21/2010 - J. Ritchie Carroll
//       Added signal type summary to the calculated measurement status.
//  12/04/2012 - J. Ritchie Carroll
//       Migrated to PhasorProtocolAdapters project.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using GSF;
using GSF.PhasorProtocols;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;

namespace PhasorProtocolAdapters
{
    #region [ Enumerations ]

    /// <summary>
    /// Signal type enumeration.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The signal type represents the explicit type of a signal that a value represents.
    /// </para>
    /// <para>
    /// Contrast the <see cref="SignalType"/> enumeration with the <see cref="SignalKind"/>
    /// enumeration which defines an abstract type for a signal (e.g., simply phase or angle).
    /// </para>
    /// </remarks>
    [Serializable()]
    public enum SignalType
    {
        /// <summary>
        /// Current phase magnitude.
        /// </summary>
        IPHM = 1,
        /// <summary>
        /// Current phase angle.
        /// </summary>
        IPHA = 2,
        /// <summary>
        /// Voltage phase magnitude.
        /// </summary>
        VPHM = 3,
        /// <summary>
        /// Voltage phase angle.
        /// </summary>
        VPHA = 4,
        /// <summary>
        /// Frequency.
        /// </summary>
        FREQ = 5,
        /// <summary>
        /// Frequency delta (dF/dt).
        /// </summary>
        DFDT = 6,
        /// <summary>
        /// Analog value.
        /// </summary>
        ALOG = 7,
        /// <summary>
        /// Status flags.
        /// </summary>
        FLAG = 8,
        /// <summary>
        /// Digital value.
        /// </summary>
        DIGI = 9,
        /// <summary>
        /// Calculated value.
        /// </summary>
        CALC = 10,
        /// <summary>
        /// Statistical value.
        /// </summary>
        STAT = 11,
        /// <summary>
        /// Alarm value.
        /// </summary>
        ALRM = 12,
        /// <summary>
        /// Undefined signal.
        /// </summary>
        NONE = -1
    }

    #endregion

    /// <summary>
    /// Represents the base class for calculated measurements that use phasor data.
    /// </summary>
    /// <remarks>
    /// This base class extends <see cref="ActionAdapterBase"/> by automatically looking up the
    /// <see cref="SignalType"/> for each of the input and output measurements.
    /// </remarks>
    public abstract class CalculatedMeasurementBase : ActionAdapterBase
    {
        #region [ Members ]

        // Fields
        private SignalType[] m_inputMeasurementKeyTypes;
        private SignalType[] m_outputMeasurementTypes;
        private string m_configurationSection;
        private bool m_supportsTemporalProcessing;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets primary keys of input measurements the calculated measurement expects.
        /// </summary>
        public override MeasurementKey[] InputMeasurementKeys
        {
            get
            {
                return base.InputMeasurementKeys;
            }
            set
            {
                base.InputMeasurementKeys = value;

                m_inputMeasurementKeyTypes = new SignalType[value.Length];

                for (int i = 0; i < m_inputMeasurementKeyTypes.Length; i++)
                {
                    m_inputMeasurementKeyTypes[i] = LookupSignalType(value[i]);
                }
            }
        }

        /// <summary>
        /// Gets or sets output measurements that the calculated measurement will produce, if any.
        /// </summary>
        public override IMeasurement[] OutputMeasurements
        {
            get
            {
                return base.OutputMeasurements;
            }
            set
            {
                base.OutputMeasurements = value;

                m_outputMeasurementTypes = new SignalType[value.Length];

                for (int i = 0; i < m_outputMeasurementTypes.Length; i++)
                {
                    m_outputMeasurementTypes[i] = LookupSignalType(value[i].Key);
                }
            }
        }

        /// <summary>
        /// Gets or sets input measurement <see cref="SignalType"/>'s for each of the <see cref="ActionAdapterBase.InputMeasurementKeys"/>, if any.
        /// </summary>
        public virtual SignalType[] InputMeasurementKeyTypes
        {
            get
            {
                return m_inputMeasurementKeyTypes;
            }
        }

        /// <summary>
        /// Gets or sets output measurement <see cref="SignalType"/>'s for each of the <see cref="ActionAdapterBase.OutputMeasurements"/>, if any.
        /// </summary>
        public virtual SignalType[] OutputMeasurementTypes
        {
            get
            {
                return m_outputMeasurementTypes;
            }
        }

        /// <summary>
        /// Gets or sets the configuration section to use for this <see cref="CalculatedMeasurementBase"/>.
        /// </summary>
        public virtual string ConfigurationSection
        {
            get
            {
                return m_configurationSection;
            }
            set
            {
                m_configurationSection = value;
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

        /// <summary>
        /// Returns the detailed status of the calculated measurement.
        /// </summary>
        /// <remarks>
        /// Derived classes should extend status with implementation specific information.
        /// </remarks>
        public override string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();
                int count;

                status.AppendFormat("     Configuration section: {0}", ConfigurationSection);
                status.AppendLine();
                status.Append(base.Status);

                if (OutputMeasurements != null && OutputMeasurements.Length > 0)
                {
                    status.AppendLine();
                    status.AppendLine("Output measurements signal type summary:");
                    status.AppendLine();

                    foreach (SignalType signalType in Enum.GetValues(typeof(SignalType)))
                    {
                        count = OutputMeasurements.Where((key, index) => OutputMeasurementTypes[index] == signalType).Count();

                        if (count > 0)
                        {
                            status.AppendFormat("{0} {1} signal{2}", count.ToString().PadLeft(15), signalType.GetFormattedSignalTypeName(), count > 1 ? "s" : "");
                            status.AppendLine();
                        }
                    }
                }

                if (InputMeasurementKeys != null && InputMeasurementKeys.Length > 0)
                {
                    status.AppendLine();
                    status.AppendLine("Input measurement keys signal type summary:");
                    status.AppendLine();

                    foreach (SignalType signalType in Enum.GetValues(typeof(SignalType)))
                    {
                        count = InputMeasurementKeys.Where((key, index) => InputMeasurementKeyTypes[index] == signalType).Count();

                        if (count > 0)
                        {
                            status.AppendFormat("{0} {1} signal{2}", count.ToString().PadLeft(15), signalType.GetFormattedSignalTypeName(), count > 1 ? "s" : "");
                            status.AppendLine();
                        }
                    }
                }

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Intializes <see cref="CalculatedMeasurementBase"/>.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            Dictionary<string, string> settings = Settings;
            string setting;

            // Load optional parameters
            if (!settings.TryGetValue("configurationSection", out m_configurationSection))
                m_configurationSection = Name;

            if (string.IsNullOrEmpty(m_configurationSection))
                m_configurationSection = Name;

            if (settings.TryGetValue("supportsTemporalProcessing", out setting))
                m_supportsTemporalProcessing = setting.ParseBoolean();
            else
                m_supportsTemporalProcessing = false;
        }

        // Lookup signal type for given measurement key
        private SignalType LookupSignalType(MeasurementKey key)
        {
            try
            {
                DataRow[] filteredRows = DataSource.Tables["ActiveMeasurements"].Select(string.Format("ID = '{0}'", key.ToString()));

                if (filteredRows.Length > 0)
                    return (SignalType)Enum.Parse(typeof(SignalType), filteredRows[0]["SignalType"].ToString(), true);
            }
            catch (Exception ex)
            {
                OnProcessException(new InvalidOperationException(string.Format("Failed to lookup signal type for measurement {0}: {1}", key.ToString(), ex.Message), ex));
            }

            return SignalType.NONE;
        }

        #endregion
    }
}