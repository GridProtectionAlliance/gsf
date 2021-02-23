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
namespace GSF.PhasorProtocols.IEEEC37_118
{
    /// <summary>
    /// Represents the IEEE C37.118 implementation of a <see cref="IConfigurationCell"/> that can be sent or received.
    /// </summary>
    [Serializable]
    public class ConfigurationCell : ConfigurationCellBase
    {
        #region [ Members ]

        // Fields
        private FormatFlags m_formatFlags;

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
                IEEEC37_118.FrequencyDefinition.CreateNewDefinition,
                AnalogDefinition.CreateNewDefinition,
                DigitalDefinition.CreateNewDefinition);
        }

        /// <summary>
        /// Creates a new <see cref="ConfigurationCell"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The reference to parent <see cref="ConfigurationFrame1"/> of this <see cref="ConfigurationCell"/>.</param>
        /// <param name="idCode">The numeric ID code for this <see cref="ConfigurationCell"/>.</param>
        /// <param name="nominalFrequency">The nominal <see cref="LineFrequency"/> of the <see cref="FrequencyDefinition"/> of this <see cref="ConfigurationCell"/>.</param>
        public ConfigurationCell(ConfigurationFrame1 parent, ushort idCode, LineFrequency nominalFrequency)
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
            m_formatFlags = (FormatFlags)info.GetValue("formatFlags", typeof(FormatFlags));
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets a reference to the parent <see cref="ConfigurationFrame1"/> for this <see cref="ConfigurationCell"/>.
        /// </summary>
        public new ConfigurationFrame1 Parent
        {
            get => base.Parent as ConfigurationFrame1;
            set => base.Parent = value;
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
                    m_formatFlags |= FormatFlags.Phasors;
                else
                    m_formatFlags &= ~FormatFlags.Phasors;
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
                    m_formatFlags |= FormatFlags.Coordinates;
                else
                    m_formatFlags &= ~FormatFlags.Coordinates;
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
                    m_formatFlags |= FormatFlags.Frequency;
                else
                    m_formatFlags &= ~FormatFlags.Frequency;
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
                    m_formatFlags |= FormatFlags.Analog;
                else
                    m_formatFlags &= ~FormatFlags.Analog;
            }
        }

        /// <summary>
        /// Gets the length of the <see cref="HeaderImage"/>.
        /// </summary>
        protected override int HeaderLength => base.HeaderLength + 10;

        /// <summary>
        /// Gets the binary header image of the <see cref="ConfigurationCell"/> object.
        /// </summary>
        protected override byte[] HeaderImage
        {
            get
            {
                byte[] buffer = new byte[HeaderLength];
                int index = 0;

                base.HeaderImage.CopyImage(buffer, ref index, base.HeaderLength);
                BigEndian.CopyBytes(IDCode, buffer, index);
                BigEndian.CopyBytes((ushort)m_formatFlags, buffer, index + 2);
                BigEndian.CopyBytes((ushort)PhasorDefinitions.Count, buffer, index + 4);
                BigEndian.CopyBytes((ushort)AnalogDefinitions.Count, buffer, index + 6);
                BigEndian.CopyBytes((ushort)DigitalDefinitions.Count, buffer, index + 8);

                return buffer;
            }
        }

        /// <summary>
        /// Gets the length of the <see cref="FooterImage"/>.
        /// </summary>
        protected override int FooterLength => base.FooterLength + PhasorDefinitions.Count * PhasorDefinition.ConversionFactorLength + AnalogDefinitions.Count * AnalogDefinition.ConversionFactorLength + DigitalDefinitions.Count * DigitalDefinition.ConversionFactorLength + (Parent.DraftRevision > DraftRevision.Draft6 ? 2 : 0);

        /// <summary>
        /// Gets the binary footer image of the <see cref="ConfigurationCell"/> object.
        /// </summary>
        protected override byte[] FooterImage
        {
            get
            {
                byte[] buffer = new byte[FooterLength];
                int x, index = 0;

                // Include conversion factors in configuration cell footer
                for (x = 0; x < PhasorDefinitions.Count; x++)
                {
                    if (PhasorDefinitions[x] is PhasorDefinition phasorDefinition)
                        phasorDefinition.ConversionFactorImage.CopyImage(buffer, ref index, PhasorDefinition.ConversionFactorLength);
                }

                for (x = 0; x < AnalogDefinitions.Count; x++)
                {
                    if (AnalogDefinitions[x] is AnalogDefinition analogDefinition)
                        analogDefinition.ConversionFactorImage.CopyImage(buffer, ref index, AnalogDefinition.ConversionFactorLength);
                }

                for (x = 0; x < DigitalDefinitions.Count; x++)
                {
                    if (DigitalDefinitions[x] is DigitalDefinition digitalDefinition)
                        digitalDefinition.ConversionFactorImage.CopyImage(buffer, ref index, DigitalDefinition.ConversionFactorLength);
                }

                // Include nominal frequency
                base.FooterImage.CopyImage(buffer, ref index, base.FooterLength);

                // Include configuration count (new for version 7.0)
                if (Parent.DraftRevision > DraftRevision.Draft6)
                    BigEndian.CopyBytes(RevisionCount, buffer, index);

                return buffer;
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

                baseAttributes.Add("Format Flags", $"{(int)m_formatFlags}: {m_formatFlags}");

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
            IConfigurationCellParsingState state = State;
            int index = startIndex;

            // Parse out station name
            index += base.ParseHeaderImage(buffer, startIndex, length);

            IDCode = BigEndian.ToUInt16(buffer, index);
            m_formatFlags = (FormatFlags)BigEndian.ToUInt16(buffer, index + 2);

            // Parse out total phasors, analogs and digitals defined for this device
            state.PhasorCount = BigEndian.ToUInt16(buffer, index + 4);
            state.AnalogCount = BigEndian.ToUInt16(buffer, index + 6);
            state.DigitalCount = BigEndian.ToUInt16(buffer, index + 8);
            index += 10;

            return index - startIndex;
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
            int x, index = startIndex;

            // Parse conversion factors from configuration cell footer
            for (x = 0; x < PhasorDefinitions.Count; x++)
            {
                if (PhasorDefinitions[x] is PhasorDefinition phasorDefinition)
                    index += phasorDefinition.ParseConversionFactor(buffer, index);
            }

            for (x = 0; x < AnalogDefinitions.Count; x++)
            {
                if (AnalogDefinitions[x] is AnalogDefinition analogDefinition)
                    index += analogDefinition.ParseConversionFactor(buffer, index);
            }

            for (x = 0; x < DigitalDefinitions.Count; x++)
            {
                if (DigitalDefinitions[x] is DigitalDefinition digitalDefinition)
                    index += digitalDefinition.ParseConversionFactor(buffer, index);
            }

            // Parse nominal frequency
            index += base.ParseFooterImage(buffer, index, length);

            // Parse out configuration count (new for version 7.0)
            if (Parent.DraftRevision > DraftRevision.Draft6)
            {
                RevisionCount = BigEndian.ToUInt16(buffer, index);
                index += 2;
            }

            return index - startIndex;
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
            info.AddValue("formatFlags", m_formatFlags, typeof(FormatFlags));
        }

        #endregion

        #region [ Static ]

        // Static Methods

        // Delegate handler to create a new IEEE C37.118 configuration cell
        internal static IConfigurationCell CreateNewCell(IChannelFrame parent, IChannelFrameParsingState<IConfigurationCell> state, int index, byte[] buffer, int startIndex, out int parsedLength)
        {
            ConfigurationCell configCell = new ConfigurationCell(parent as IConfigurationFrame);

            parsedLength = configCell.ParseBinaryImage(buffer, startIndex, 0);

            return configCell;
        }

        #endregion
    }
}