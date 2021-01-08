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
//  11/12/2004 - J. Ritchie Carroll
//       Generated original version of source code.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  05/19/2011 - J. Ritchie Carroll
//       Added DST file support.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Text;
using GSF.Units.EE;

namespace GSF.PhasorProtocols.BPAPDCstream
{
    /// <summary>
    /// Represents the BPA PDCstream implementation of a <see cref="IConfigurationCell"/> that can be sent or received.
    /// </summary>
    [Serializable]
    public class ConfigurationCell : ConfigurationCellBase
    {
        #region [ Members ]

        // Constants
        private const int FixedBodyLength = 8;

        // Fields
        private ConfigurationCell m_configurationFileCell;
        private string m_sectionEntry;
        private FormatFlags m_formatFlags;
        private ushort m_offset;
        private ushort m_reserved;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ConfigurationCell"/>.
        /// </summary>
        /// <param name="parent">The reference to parent <see cref="IConfigurationFrame"/> of this <see cref="ConfigurationCell"/>.</param>
        public ConfigurationCell(IConfigurationFrame parent)
            : base(parent, 0, Common.MaximumPhasorValues, Common.MaximumAnalogValues, Common.MaximumDigitalValues)
        {
            // Define new parsing state which defines constructors for key configuration values
            State = new ConfigurationCellParsingState(
                PhasorDefinition.CreateNewDefinition,
                BPAPDCstream.FrequencyDefinition.CreateNewDefinition,
                AnalogDefinition.CreateNewDefinition,
                DigitalDefinition.CreateNewDefinition);
        }

        /// <summary>
        /// Creates a new <see cref="ConfigurationCell"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The reference to parent <see cref="ConfigurationFrame"/> of this <see cref="ConfigurationCell"/>.</param>
        /// <param name="idCode">The numeric ID code for this <see cref="ConfigurationCell"/>.</param>
        /// <param name="nominalFrequency">The nominal <see cref="LineFrequency"/> of the <see cref="FrequencyDefinition"/> of this <see cref="ConfigurationCell"/>.</param>
        [SuppressMessage("Microsoft.Design", "CA1026")]
        public ConfigurationCell(ConfigurationFrame parent, ushort idCode, LineFrequency nominalFrequency = LineFrequency.Hz60)
            : this(parent)
        {
            IDCode = idCode;
            NominalFrequency = nominalFrequency;
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
            m_sectionEntry = info.GetString("sectionEntry");
            m_formatFlags = (FormatFlags)info.GetValue("formatFlags", typeof(FormatFlags));
            m_offset = info.GetUInt16("offset");
            m_reserved = info.GetUInt16("reserved");
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
        public override PhasorDefinitionCollection PhasorDefinitions
        {
            get
            {
                if (m_configurationFileCell is null)
                    return base.PhasorDefinitions;

                return m_configurationFileCell.PhasorDefinitions;
            }
        }

        /// <summary>
        /// Gets or sets the station name of this <see cref="ConfigurationCell"/>.
        /// </summary>
        public override string StationName
        {
            get
            {
                if (m_configurationFileCell is null)
                    return base.StationName;

                return m_configurationFileCell.StationName;
            }
            set
            {
                if (m_configurationFileCell is null)
                    base.StationName = value;
                else
                    m_configurationFileCell.StationName = value;
            }
        }

        /// <summary>
        /// Gets the length of the <see cref="ConfigurationCellBase.IDLabel"/> of this <see cref="ConfigurationCell"/>.
        /// </summary>
        public override int IDLabelLength =>
            // BPA PDCstream ID label length is 4 characters - max!
            4;

        /// <summary>
        /// Gets flag that determines if source data is in the Phasor Data File Format (i.e., a DST file).
        /// </summary>
        public bool UsePhasorDataFileFormat => Parent.UsePhasorDataFileFormat;

        /// <summary>
        /// Gets or sets section entry in INI based configuration file for this <see cref="ConfigurationCell"/>.
        /// </summary>
        public string SectionEntry
        {
            get
            {
                if (m_configurationFileCell is null)
                    return m_sectionEntry;

                return m_configurationFileCell.SectionEntry;
            }
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
            get
            {
                if (m_configurationFileCell is null)
                    return base.IDCode;

                return m_configurationFileCell.IDCode;
            }
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
            get
            {
                if (m_configurationFileCell is null)
                    return base.FrequencyDefinition;

                return m_configurationFileCell.FrequencyDefinition;
            }
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
            get
            {
                if (m_configurationFileCell is null)
                    return base.NominalFrequency;

                return m_configurationFileCell.NominalFrequency;
            }
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
            // The station name in the PDCstream is read from an INI file, so there is no set limit
            int.MaxValue;

        /// <summary>
        /// Gets or sets BPA PDCstream descriptor offset of this <see cref="ConfigurationCell"/> in its data packet.
        /// </summary>
        public ushort Offset
        {
            get => m_offset;
            set => m_offset = value;
        }

        /// <summary>
        /// Gets or sets reserved word of this <see cref="ConfigurationCell"/>.
        /// </summary>
        public ushort Reserved
        {
            get => m_reserved;
            set => m_reserved = value;
        }

        /// <summary>
        /// Gets or sets format flags of this <see cref="ConfigurationCell"/>.
        /// </summary>
        /// <remarks>
        /// These are bit flags, use properties to change basic values.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public FormatFlags FormatFlags
        {
            get => m_formatFlags;
            set => m_formatFlags = value;
        }

        /// <summary>
        /// Gets or sets the <see cref="DataFormat"/> for the <see cref="IPhasorDefinition"/> objects in the <see cref="ConfigurationCellBase.PhasorDefinitions"/> of this <see cref="ConfigurationCell"/>.
        /// </summary>
        public override DataFormat PhasorDataFormat
        {
            get => (m_formatFlags & FormatFlags.Phasors) > 0 ? DataFormat.FloatingPoint : DataFormat.FixedInteger;
            set
            {
                if (value == DataFormat.FloatingPoint)
                    m_formatFlags = m_formatFlags | FormatFlags.Phasors;
                else
                    m_formatFlags = m_formatFlags & ~FormatFlags.Phasors;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="CoordinateFormat"/> for the <see cref="IPhasorDefinition"/> objects in the <see cref="ConfigurationCellBase.PhasorDefinitions"/> of this <see cref="ConfigurationCell"/>.
        /// </summary>
        public override CoordinateFormat PhasorCoordinateFormat
        {
            get => (m_formatFlags & FormatFlags.Coordinates) > 0 ? CoordinateFormat.Polar : CoordinateFormat.Rectangular;
            set
            {
                if (value == CoordinateFormat.Polar)
                    m_formatFlags = m_formatFlags | FormatFlags.Coordinates;
                else
                    m_formatFlags = m_formatFlags & ~FormatFlags.Coordinates;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="DataFormat"/> of the <see cref="FrequencyDefinition"/> of this <see cref="ConfigurationCell"/>.
        /// </summary>
        public override DataFormat FrequencyDataFormat
        {
            get => (m_formatFlags & FormatFlags.Frequency) > 0 ? DataFormat.FloatingPoint : DataFormat.FixedInteger;
            set
            {
                if (value == DataFormat.FloatingPoint)
                    m_formatFlags = m_formatFlags | FormatFlags.Frequency;
                else
                    m_formatFlags = m_formatFlags & ~FormatFlags.Frequency;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="DataFormat"/> for the <see cref="IAnalogDefinition"/> objects in the <see cref="ConfigurationCellBase.AnalogDefinitions"/> of this <see cref="ConfigurationCell"/>.
        /// </summary>
        public override DataFormat AnalogDataFormat
        {
            get => (m_formatFlags & FormatFlags.Analog) > 0 ? DataFormat.FloatingPoint : DataFormat.FixedInteger;
            set
            {
                if (value == DataFormat.FloatingPoint)
                    m_formatFlags = m_formatFlags | FormatFlags.Analog;
                else
                    m_formatFlags = m_formatFlags & ~FormatFlags.Analog;
            }
        }

        // The descriptor cell broadcasted by PDCstream only includes PMUID and offset, all
        // other metadata is defined in an external INI based configuration file - so we
        // override the base class image implementations which attempt to generate and
        // parse data based on a common nature of IEEE configuration frames

        /// <summary>
        /// Gets the length of the <see cref="HeaderImage"/>.
        /// </summary>
        protected override int HeaderLength => 0;

        /// <summary>
        /// Gets the binary header image of the <see cref="ConfigurationCell"/> object.
        /// </summary>
        protected override byte[] HeaderImage => null;

        /// <summary>
        /// Gets the length of the <see cref="BodyImage"/>.
        /// </summary>
        protected override int BodyLength => FixedBodyLength;

        /// <summary>
        /// Gets the binary body image of the <see cref="ConfigurationCell"/> object.
        /// </summary>
        protected override byte[] BodyImage
        {
            get
            {
                byte[] buffer = new byte[FixedBodyLength];
                int index = 0;

                IDLabelImage.CopyImage(buffer, ref index, IDLabelLength);   // PMUID
                BigEndian.CopyBytes(Reserved, buffer, index);   // Reserved
                BigEndian.CopyBytes(Offset, buffer, index + 2); // Offset

                return buffer;
            }
        }

        /// <summary>
        /// Gets the length of the <see cref="FooterImage"/>.
        /// </summary>
        protected override int FooterLength => 0;

        /// <summary>
        /// Gets the binary footer image of the <see cref="ConfigurationCell"/> object.
        /// </summary>
        protected override byte[] FooterImage => null;

        /// <summary>
        /// <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for the <see cref="ConfigurationCell"/> object.
        /// </summary>
        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("INI File Section Entry", SectionEntry);
                baseAttributes.Add("Offset", Offset.ToString());
                baseAttributes.Add("Reserved", Reserved.ToString());

                return baseAttributes;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Parses the binary header image.
        /// </summary>
        /// <param name="buffer">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="buffer"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="buffer"/>.</param>
        /// <returns>The length of the data that was parsed.</returns>
        protected override int ParseHeaderImage(byte[] buffer, int startIndex, int length)
        {
            // BPA PDC stream doesn't use standard configuration cell header like IEEE protocols do - so we override this function to do nothing
            return 0;
        }

        /// <summary>
        /// Parses the binary body image.
        /// </summary>
        /// <param name="buffer">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="buffer"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="buffer"/>.</param>
        /// <returns>The length of the data that was parsed.</returns>
        protected override int ParseBodyImage(byte[] buffer, int startIndex, int length)
        {
            IDLabel = Encoding.ASCII.GetString(buffer, startIndex, 4);
            Reserved = BigEndian.ToUInt16(buffer, startIndex + 4);
            Offset = BigEndian.ToUInt16(buffer, startIndex + 6);

            return FixedBodyLength;
        }

        /// <summary>
        /// Parses the binary footer image.
        /// </summary>
        /// <param name="buffer">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="buffer"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="buffer"/>.</param>
        /// <returns>The length of the data that was parsed.</returns>
        protected override int ParseFooterImage(byte[] buffer, int startIndex, int length)
        {
            // BPA PDC stream doesn't use standard configuration cell footer like IEEE protocols do - so we override this function to do nothing
            return 0;
        }

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
            info.AddValue("formatFlags", m_formatFlags, typeof(FormatFlags));
            info.AddValue("offset", m_offset);
            info.AddValue("reserved", m_reserved);
        }

        #endregion

        #region [ Static ]

        // Static Methods

        // Delegate handler to create a new BPA PDCstream configuration cell
        internal static IConfigurationCell CreateNewCell(IChannelFrame parent, IChannelFrameParsingState<IConfigurationCell> state, int index, byte[] buffer, int startIndex, out int parsedLength)
        {
            ConfigurationCell configCell = new ConfigurationCell(parent as IConfigurationFrame);

            parsedLength = configCell.ParseBinaryImage(buffer, startIndex, 0);

            return configCell;
        }

        #endregion
    }
}