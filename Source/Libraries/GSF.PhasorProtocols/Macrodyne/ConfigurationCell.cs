//******************************************************************************************************
//  ConfigurationCell.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  04/30/2009 - J. Ritchie Carroll
//       Generated original version of source code.
//  08/07/2009 - Josh L. Patterson
//       Edited Comments.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using GSF.Units.EE;

// ReSharper disable VirtualMemberCallInConstructor
namespace GSF.PhasorProtocols.Macrodyne
{
    /// <summary>
    /// Represents the Macrodyne implementation of a <see cref="IConfigurationCell"/> that can be sent or received.
    /// </summary>
    [Serializable]
    public class ConfigurationCell : ConfigurationCellBase
    {
        #region [ Members ]

        // Fields
        private ConfigurationCell m_configurationFileCell;
        private string m_sectionEntry;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ConfigurationCell"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The parent <see cref="ConfigurationFrame"/> reference to use.</param>
        /// <param name="deviceLabel">INI section device label to use.</param>
        public ConfigurationCell(ConfigurationFrame parent, string deviceLabel = null)
            : base(parent, 0, Common.MaximumPhasorValues, Common.MaximumAnalogValues, Common.MaximumDigitalValues)
        {
            // Assign station name that came in from header frame
            StationName = parent.StationName;

            if (!string.IsNullOrEmpty(deviceLabel))
                SectionEntry = deviceLabel;

            // Add a single frequency definition
            FrequencyDefinition = new FrequencyDefinition(this)
            {
                Label = "Line frequency"
            };

            OnlineDataFormatFlags flags = parent.OnlineDataFormatFlags;

            PhasorDefinitions.Add(new PhasorDefinition(this, "Phasor 1", PhasorType.Voltage, null));

            if ((flags & OnlineDataFormatFlags.Phasor2Enabled) == OnlineDataFormatFlags.Phasor2Enabled)
                PhasorDefinitions.Add(new PhasorDefinition(this, "Phasor 2", PhasorType.Voltage, null));

            if ((flags & OnlineDataFormatFlags.Phasor3Enabled) == OnlineDataFormatFlags.Phasor3Enabled)
                PhasorDefinitions.Add(new PhasorDefinition(this, "Phasor 3", PhasorType.Voltage, null));

            if ((flags & OnlineDataFormatFlags.Phasor4Enabled) == OnlineDataFormatFlags.Phasor4Enabled)
                PhasorDefinitions.Add(new PhasorDefinition(this, "Phasor 4", PhasorType.Voltage, null));

            if ((flags & OnlineDataFormatFlags.Phasor5Enabled) == OnlineDataFormatFlags.Phasor5Enabled)
                PhasorDefinitions.Add(new PhasorDefinition(this, "Phasor 5", PhasorType.Voltage, null));

            if ((flags & OnlineDataFormatFlags.Phasor6Enabled) == OnlineDataFormatFlags.Phasor6Enabled)
                PhasorDefinitions.Add(new PhasorDefinition(this, "Phasor 6", PhasorType.Voltage, null));

            if ((flags & OnlineDataFormatFlags.Phasor7Enabled) == OnlineDataFormatFlags.Phasor7Enabled)
                PhasorDefinitions.Add(new PhasorDefinition(this, "Phasor 7", PhasorType.Voltage, null));

            if ((flags & OnlineDataFormatFlags.Phasor8Enabled) == OnlineDataFormatFlags.Phasor8Enabled)
                PhasorDefinitions.Add(new PhasorDefinition(this, "Phasor 8", PhasorType.Voltage, null));

            if ((flags & OnlineDataFormatFlags.Phasor9Enabled) == OnlineDataFormatFlags.Phasor9Enabled)
                PhasorDefinitions.Add(new PhasorDefinition(this, "Phasor 9", PhasorType.Voltage, null));

            if ((flags & OnlineDataFormatFlags.Phasor10Enabled) == OnlineDataFormatFlags.Phasor10Enabled)
                PhasorDefinitions.Add(new PhasorDefinition(this, "Phasor 10", PhasorType.Voltage, null));

            if ((flags & OnlineDataFormatFlags.Digital1Enabled) == OnlineDataFormatFlags.Digital1Enabled)
                DigitalDefinitions.Add(new DigitalDefinition(this, "Digital 1"));

            if ((flags & OnlineDataFormatFlags.Digital2Enabled) == OnlineDataFormatFlags.Digital2Enabled)
                DigitalDefinitions.Add(new DigitalDefinition(this, "Digital 2"));
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
            try
            {
                m_sectionEntry = info.GetString("sectionEntry");
            }
            catch (SerializationException)
            {
                m_sectionEntry = null;
            }

            try
            {
                m_configurationFileCell = info.GetValue("configurationFileCell", typeof(ConfigurationCell)) as ConfigurationCell;
            }
            catch (SerializationException)
            {
                m_configurationFileCell = null;
            }
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets a reference to the parent <see cref="ConfigurationFrame"/> for this <see cref="ConfigurationCell"/>.
        /// </summary>
        public new ConfigurationFrame Parent
        {
            get => base.Parent as ConfigurationFrame;
            set => base.Parent = value;
        }

        /// <summary>
        /// Gets or sets reference to the <see cref="ConfigurationCell"/> loaded from the configuration file associated this <see cref="ConfigurationCell"/>.
        /// </summary>
        public ConfigurationCell ConfigurationFileCell
        {
            get => m_configurationFileCell;
            set => m_configurationFileCell = value;
        }

        /// <summary>
        /// Gets a reference to the <see cref="PhasorDefinitionCollection"/> of this <see cref="ConfigurationCell"/>.
        /// </summary>
        public override PhasorDefinitionCollection PhasorDefinitions => 
            m_configurationFileCell is null ? 
                base.PhasorDefinitions : 
                m_configurationFileCell.PhasorDefinitions;

        /// <summary>
        /// Gets or sets the station name of this <see cref="ConfigurationCell"/>.
        /// </summary>
        public override string StationName
        {
            get => m_configurationFileCell is null ? 
                base.StationName : 
                m_configurationFileCell.StationName;
            set
            {
                if (m_configurationFileCell is null)
                    base.StationName = value;
                else
                    m_configurationFileCell.StationName = value;
            }
        }

        /// <summary>
        /// Gets or sets section entry in INI based configuration file for this <see cref="ConfigurationCell"/>.
        /// </summary>
        public string SectionEntry
        {
            get => m_configurationFileCell is null ? 
                m_sectionEntry : 
                m_configurationFileCell.SectionEntry;
            set
            {
                m_sectionEntry = value.Trim();

                // Get ID label as substring of section entry
                if (!string.IsNullOrEmpty(m_sectionEntry))
                {
                    if (m_sectionEntry.Length > IDLabelLength)
                        IDLabel = m_sectionEntry.Substring(0, IDLabelLength);
                    else
                        IDLabel = m_sectionEntry;
                }
            }
        }

        /// <summary>
        /// Gets flag that determines if current <see cref="SectionEntry"/> defines a PDC block section.
        /// </summary>
        public bool IsPdcBlockSection
        {
            get
            {
                if (m_configurationFileCell is null)
                {
                    if (string.IsNullOrEmpty(m_sectionEntry))
                        return false;

                    return m_sectionEntry.Length > IDLabelLength;
                }

                return m_configurationFileCell.IsPdcBlockSection;
            }
        }

        /// <summary>
        /// Gets or sets the numeric ID code for this <see cref="ConfigurationCell"/>.
        /// </summary>
        public override ushort IDCode
        {
            get => m_configurationFileCell?.IDCode ?? base.IDCode;
            set
            {
                if (m_configurationFileCell is null)
                    base.IDCode = value;
                else
                    m_configurationFileCell.IDCode = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IFrequencyDefinition"/> of this <see cref="ConfigurationCell"/>.
        /// </summary>
        public override IFrequencyDefinition FrequencyDefinition
        {
            get => m_configurationFileCell is null ? 
                base.FrequencyDefinition : 
                m_configurationFileCell.FrequencyDefinition;
            set
            {
                if (m_configurationFileCell is null)
                    base.FrequencyDefinition = value;
                else
                    m_configurationFileCell.FrequencyDefinition = value;
            }
        }

        /// <summary>
        /// Gets or sets the nominal <see cref="LineFrequency"/> of the <see cref="FrequencyDefinition"/> of this <see cref="ConfigurationCell"/>.
        /// </summary>
        public override LineFrequency NominalFrequency
        {
            get => m_configurationFileCell?.NominalFrequency ?? base.NominalFrequency;
            set
            {
                if (m_configurationFileCell is null)
                {
                    base.NominalFrequency = value;
                }
                else
                {
                    m_configurationFileCell.NominalFrequency = value;
                }
            }
        }

        /// <summary>
        /// Gets the maximum length of the <see cref="StationName"/> of this <see cref="ConfigurationCell"/>.
        /// </summary>
        public override int MaximumStationNameLength =>
            // When the station name in the PDCstream is read from an INI file, there is no set limit.
            // For the Macrodyne protocol the unit ID cannot exceed 8 characters - but this protocol
            // is current read-only, so we don't worry about the limit...
            int.MaxValue;

        /// <summary>
        /// Gets or sets the <see cref="DataFormat"/> for the <see cref="IPhasorDefinition"/> objects in the <see cref="ConfigurationCellBase.PhasorDefinitions"/> of this <see cref="ConfigurationCell"/>.
        /// </summary>
        /// <remarks>
        /// This property only supports scaled data; Macrodyne doesn't transport floating-point values.
        /// </remarks>
        /// <exception cref="NotSupportedException">Macrodyne only supports scaled data.</exception>
        public override DataFormat PhasorDataFormat
        {
            get => DataFormat.FixedInteger;
            set
            {
                if (value != DataFormat.FixedInteger)
                    throw new NotSupportedException("Macrodyne only supports scaled data");
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="CoordinateFormat"/> for the <see cref="IPhasorDefinition"/> objects in the <see cref="ConfigurationCellBase.PhasorDefinitions"/> of this <see cref="ConfigurationCell"/>.
        /// </summary>
        /// <remarks>
        /// This property only supports rectangular phasor data; Macrodyne doesn't transport polar phasor values.
        /// </remarks>
        /// <exception cref="NotSupportedException">Macrodyne only supports rectangular phasor data.</exception>
        public override CoordinateFormat PhasorCoordinateFormat
        {
            get => CoordinateFormat.Rectangular;
            set
            {
                if (value != CoordinateFormat.Rectangular)
                    throw new NotSupportedException("Macrodyne only supports rectangular phasor data");
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="DataFormat"/> of the <see cref="FrequencyDefinition"/> of this <see cref="ConfigurationCell"/>.
        /// </summary>
        /// <remarks>
        /// This property only supports scaled data; Macrodyne doesn't transport floating-point values.
        /// </remarks>
        /// <exception cref="NotSupportedException">Macrodyne only supports scaled data.</exception>
        public override DataFormat FrequencyDataFormat
        {
            get => DataFormat.FixedInteger;
            set
            {
                if (value != DataFormat.FixedInteger)
                    throw new NotSupportedException("Macrodyne only supports scaled data");
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="DataFormat"/> for the <see cref="IAnalogDefinition"/> objects in the <see cref="ConfigurationCellBase.AnalogDefinitions"/> of this <see cref="ConfigurationCell"/>.
        /// </summary>
        /// <remarks>
        /// <para>Macrodyne doesn't define any analog values.</para>
        /// <para>This property only supports scaled data; Macrodyne doesn't transport floating point values.</para>
        /// </remarks>
        /// <exception cref="NotSupportedException">Macrodyne only supports scaled data.</exception>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override DataFormat AnalogDataFormat
        {
            get => DataFormat.FixedInteger;
            set
            {
                if (value != DataFormat.FixedInteger)
                    throw new NotSupportedException("Macrodyne only supports scaled data");
            }
        }

        /// <summary>
        /// <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for the <see cref="ConfigurationCell"/> object.
        /// </summary>
        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("INI File Section Entry", SectionEntry);

                return baseAttributes;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            // Serialize configuration cell
            info.AddValue("sectionEntry", SectionEntry);
            info.AddValue("configurationFileCell", m_configurationFileCell, typeof(ConfigurationCell));
        }

        #endregion

        #region [ Static ]

        // Static Methods

        // Delegate handler to create a new Macrodyne configuration cell
        internal static IConfigurationCell CreateNewCell(IChannelFrame parent, IChannelFrameParsingState<IConfigurationCell> state, int index, byte[] buffer, int startIndex, out int parsedLength)
        {
            parsedLength = 0;
            return new ConfigurationCell(parent as ConfigurationFrame);
        }

        #endregion
    }
}