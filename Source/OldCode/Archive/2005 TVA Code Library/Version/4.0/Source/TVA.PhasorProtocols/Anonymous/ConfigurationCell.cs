//*******************************************************************************************************
//  ConfigurationCell.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R. Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  05/05/2009 - James R. Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace TVA.PhasorProtocols.Anonymous
{
    /// <summary>
    /// Represents a protocol independent implementation of a <see cref="IConfigurationCell"/> that can be sent or received.
    /// </summary>
    [Serializable()]
    public class ConfigurationCell : ConfigurationCellBase
	{
        #region [ Members ]

        // Fields
        private DataFormat m_analogDataFormat;
        private DataFormat m_frequencyDataFormat;
        private DataFormat m_phasorDataFormat;
        private CoordinateFormat m_phasorCoordinateFormat;

        // We add cached signal type and statistical tracking information to our protocol independent configuration cell
        private Dictionary<SignalType, string[]> m_signalReferences;
        private Ticks m_lastReportTime;
        private long m_totalFrames;
        private long m_totalDataQualityErrors;
        private long m_totalTimeQualityErrors;
        private long m_totalDeviceErrors;
        private bool m_isVirtual;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ConfigurationCell"/> from specified parameters.
        /// </summary>
        /// <param name="idCode">The numeric ID code for this <see cref="ConfigurationCell"/>.</param>
        /// <param name="isVirtual">Assigns flag that determines if this <see cref="ConfigurationCell"/> is virtual.</param>
        public ConfigurationCell(ushort idCode, bool isVirtual)
            : this(null, idCode, isVirtual)
        {
        }

        /// <summary>
        /// Creates a new <see cref="ConfigurationCell"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The reference to parent <see cref="ConfigurationFrame"/> of this <see cref="ConfigurationCell"/>.</param>
        /// <param name="idCode">The numeric ID code for this <see cref="ConfigurationCell"/>.</param>
        /// <param name="isVirtual">Assigns flag that determines if this <see cref="ConfigurationCell"/> is virtual.</param>
        public ConfigurationCell(ConfigurationFrame parent, ushort idCode, bool isVirtual)
            : base(parent, idCode, int.MaxValue, int.MaxValue, int.MaxValue)
		{			
			m_signalReferences = new Dictionary<SignalType, string[]>();
			m_analogDataFormat = DataFormat.FloatingPoint;
			m_frequencyDataFormat = DataFormat.FloatingPoint;
			m_phasorDataFormat = DataFormat.FloatingPoint;
			m_phasorCoordinateFormat = CoordinateFormat.Polar;
            m_isVirtual = isVirtual;
		}

        /// <summary>
        /// Creates a new <see cref="ConfigurationCell"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected ConfigurationCell(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Deserialize configuration cell
            m_lastReportTime = info.GetInt64("lastReportTime");
            m_signalReferences = (Dictionary<SignalType, string[]>)info.GetValue("signalReferences", typeof(Dictionary<SignalType, string[]>));
            m_analogDataFormat = (DataFormat)info.GetValue("analogDataFormat", typeof(DataFormat));
            m_frequencyDataFormat = (DataFormat)info.GetValue("frequencyDataFormat", typeof(DataFormat));
            m_phasorDataFormat = (DataFormat)info.GetValue("phasorDataFormat", typeof(DataFormat));
            m_phasorCoordinateFormat = (CoordinateFormat)info.GetValue("phasorCoordinateFormat", typeof(CoordinateFormat));
            m_isVirtual = info.GetBoolean("isVirtual");
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="DataFormat"/> for the <see cref="IAnalogDefinition"/> objects in the <see cref="ConfigurationCellBase.AnalogDefinitions"/> of this <see cref="ConfigurationCell"/>.
        /// </summary>
        public override DataFormat AnalogDataFormat
        {
            get
            {
                return m_analogDataFormat;
            }
            set
            {
                m_analogDataFormat = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="DataFormat"/> of the <see cref="FrequencyDefinition"/> of this <see cref="ConfigurationCell"/>.
        /// </summary>
        public override DataFormat FrequencyDataFormat
        {
            get
            {
                return m_frequencyDataFormat;
            }
            set
            {
                m_frequencyDataFormat = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="DataFormat"/> for the <see cref="IPhasorDefinition"/> objects in the <see cref="ConfigurationCellBase.PhasorDefinitions"/> of this <see cref="ConfigurationCell"/>.
        /// </summary>
        public override DataFormat PhasorDataFormat
        {
            get
            {
                return m_phasorDataFormat;
            }
            set
            {
                m_phasorDataFormat = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="CoordinateFormat"/> for the <see cref="IPhasorDefinition"/> objects in the <see cref="ConfigurationCellBase.PhasorDefinitions"/> of this <see cref="ConfigurationCell"/>.
        /// </summary>
        public override CoordinateFormat PhasorCoordinateFormat
        {
            get
            {
                return m_phasorCoordinateFormat;
            }
            set
            {
                m_phasorCoordinateFormat = value;
            }
        }

        /// <summary>
        /// Gets or sets last report time of this <see cref="ConfigurationCell"/>.
        /// </summary>
        public Ticks LastReportTime
        {
            get
            {
                return m_lastReportTime;
            }
            set
            {
                m_lastReportTime = value;
            }
        }

        /// <summary>
        /// Gets or sets total frames of this <see cref="ConfigurationCell"/>.
        /// </summary>
        public long TotalFrames
        {
            get
            {
                return m_totalFrames;
            }
            set
            {
                m_totalFrames = value;
            }
        }

        /// <summary>
        /// Gets or sets total data quality errors of this <see cref="ConfigurationCell"/>.
        /// </summary>
        public long TotalDataQualityErrors
        {
            get
            {
                return m_totalDataQualityErrors;
            }
            set
            {
                m_totalDataQualityErrors = value;
            }
        }

        /// <summary>
        /// Gets or sets total time quality errors of this <see cref="ConfigurationCell"/>.
        /// </summary>
        public long TotalTimeQualityErrors
        {
            get
            {
                return m_totalTimeQualityErrors;
            }
            set
            {
                m_totalTimeQualityErrors = value;
            }
        }

        /// <summary>
        /// Gets or set total device errors of this <see cref="ConfigurationCell"/>.
        /// </summary>
        public long TotalDeviceErrors
        {
            get
            {
                return m_totalDeviceErrors;
            }
            set
            {
                m_totalDeviceErrors = value;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if this <see cref="ConfigurationCell"/> is virtual.
        /// </summary>
        public bool IsVirtual
        {
            get
            {
                return m_isVirtual;
            }
            set
            {
                m_isVirtual = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Get signal reference for specified <see cref="SignalType"/>.
        /// </summary>
        /// <param name="type"><see cref="SignalType"/> to request signal reference for.</param>
        /// <returns>Signal reference of given <see cref="SignalType"/>.</returns>
        public string GetSignalReference(SignalType type)
        {
            // We cache non-indexed signal reference strings so they don't need to be generated at each mapping call.
            // This helps with performance since the mappings for each signal occur 30 times per second.
            string[] references;

            // Look up synonym in dictionary based on signal type, if found return single element
            if (m_signalReferences.TryGetValue(type, out references))
                return references[0];

            // Create a new signal reference array (for single element)
            references = new string[1];

            // Create and cache new non-indexed signal reference
            references[0] = SignalReference.ToString(IDLabel, type);

            // Cache generated signal synonym
            m_signalReferences.Add(type, references);

            return references[0];
        }

        /// <summary>
        /// Get signal reference for specified <see cref="SignalType"/> and <paramref name="signalIndex"/>.
        /// </summary>
        /// <param name="type"><see cref="SignalType"/> to request signal reference for.</param>
        /// <param name="index">Index <see cref="SignalType"/> to request signal reference for.</param>
        /// <param name="count">Number of signals defined for this <see cref="SignalType"/>.</param>
        /// <returns>Signal reference of given <see cref="SignalType"/> and <paramref name="signalIndex"/>.</returns>
        public string GetSignalReference(SignalType type, int index, int count)
        {
            // We cache indexed signal reference strings so they don't need to be generated at each mapping call.
            // This helps with performance since the mappings for each signal occur 30 times per second.
            // For speed purposes we intentionally do not validate that signalIndex falls within signalCount, be
            // sure calling procedures are very careful with parameters...
            string[] references;

            // Look up synonym in dictionary based on signal type
            if (m_signalReferences.TryGetValue(type, out references))
            {
                // Verify signal count has not changed (we may have received new configuration from device)
                if (count == references.Length)
                {
                    // Create and cache new signal reference if it doesn't exist
                    if (references[index] == null)
                        references[index] = SignalReference.ToString(IDLabel, type, index + 1);

                    return references[index];
                }
            }

            // Create a new indexed signal reference array
            references = new string[count];

            // Create and cache new signal reference
            references[index] = SignalReference.ToString(IDLabel, type, index + 1);

            // Cache generated signal synonym array
            m_signalReferences.Add(type, references);

            return references[index];
        }

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            // Serialize configuration cell
            info.AddValue("lastReportTime", (long)m_lastReportTime);
            info.AddValue("signalReferences", m_signalReferences, typeof(Dictionary<SignalType, string[]>));
            info.AddValue("analogDataFormat", m_analogDataFormat, typeof(DataFormat));
            info.AddValue("frequencyDataFormat", m_frequencyDataFormat, typeof(DataFormat));
            info.AddValue("phasorDataFormat", m_phasorDataFormat, typeof(DataFormat));
            info.AddValue("phasorCoordinateFormat", m_phasorCoordinateFormat, typeof(CoordinateFormat));
            info.AddValue("isVirtual", m_isVirtual);
        }

        #endregion
	}	
}