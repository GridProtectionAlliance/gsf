//*******************************************************************************************************
//  ConfigurationCell.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  11/12/2004 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace PCS.PhasorProtocols.IeeeC37_118
{
    /// <summary>
    /// Represents the IEEE C37.118 implementation of a <see cref="IConfigurationCell"/> that can be sent or received.
    /// </summary>
    [Serializable()]
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
            : base(parent, false, 0, Common.MaximumPhasorValues, Common.MaximumAnalogValues, Common.MaximumDigitalValues)
        {
            // Define new parsing state which defines constructors for key configuration values
            State = new ConfigurationCellParsingState(
                IeeeC37_118.PhasorDefinition.CreateNewDefinition,
                IeeeC37_118.FrequencyDefinition.CreateNewDefinition,
                IeeeC37_118.AnalogDefinition.CreateNewDefinition,
                IeeeC37_118.DigitalDefinition.CreateNewDefinition);
        }

        /// <summary>
        /// Creates a new <see cref="ConfigurationCell"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The reference to parent <see cref="ConfigurationFrame"/> of this <see cref="ConfigurationCell"/>.</param>
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
        /// Gets a reference to the parent <see cref="ConfigurationFrame"/> for this <see cref="ConfigurationCell"/>.
        /// </summary>
        public new ConfigurationFrame1 Parent
        {
            get
            {
                return base.Parent as ConfigurationFrame1;
            }
            set
            {
                base.Parent = value;
            }
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
            get
            {
                return m_formatFlags;
            }
            set
            {
                m_formatFlags = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="DataFormat"/> for the <see cref="IPhasorDefinition"/> objects in the <see cref="ConfigurationCellBase.PhasorDefinitions"/> of this <see cref="ConfigurationCell"/>.
        /// </summary>
        public override DataFormat PhasorDataFormat
        {
            get
            {
                return (((m_formatFlags & FormatFlags.Phasors) > 0) ? DataFormat.FloatingPoint : DataFormat.FixedInteger);
            }
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
            get
            {
                return (((m_formatFlags & FormatFlags.Coordinates) > 0) ? CoordinateFormat.Polar : CoordinateFormat.Rectangular);
            }
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
            get
            {
                return (((m_formatFlags & FormatFlags.Frequency) > 0) ? DataFormat.FloatingPoint : DataFormat.FixedInteger);
            }
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
            get
            {
                return (((m_formatFlags & FormatFlags.Analog) > 0) ? DataFormat.FloatingPoint : DataFormat.FixedInteger);
            }
            set
            {
                if (value == DataFormat.FloatingPoint)
                    m_formatFlags = m_formatFlags | FormatFlags.Analog;
                else
                    m_formatFlags = m_formatFlags & ~FormatFlags.Analog;
            }
        }

        /// <summary>
        /// Gets the length of the <see cref="HeaderImage"/>.
        /// </summary>
        protected override int HeaderLength
        {
            get
            {
                return base.HeaderLength + 10;
            }
        }

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
                EndianOrder.BigEndian.CopyBytes(IDCode, buffer, index);
                EndianOrder.BigEndian.CopyBytes((ushort)m_formatFlags, buffer, index + 2);
                EndianOrder.BigEndian.CopyBytes((ushort)PhasorDefinitions.Count, buffer, index + 4);
                EndianOrder.BigEndian.CopyBytes((ushort)AnalogDefinitions.Count, buffer, index + 6);
                EndianOrder.BigEndian.CopyBytes((ushort)DigitalDefinitions.Count, buffer, index + 8);

                return buffer;
            }
        }

        /// <summary>
        /// Gets the length of the <see cref="FooterImage"/>.
        /// </summary>
        protected override int FooterLength
        {
            get
            {
                return base.FooterLength + PhasorDefinitions.Count * PhasorDefinition.ConversionFactorLength + AnalogDefinitions.Count * AnalogDefinition.ConversionFactorLength + DigitalDefinitions.Count * DigitalDefinition.ConversionFactorLength + (Parent.DraftRevision > DraftRevision.Draft6 ? 2 : 0);
            }
        }

        /// <summary>
        /// Gets the binary footer image of the <see cref="ConfigurationCell"/> object.
        /// </summary>
        protected override byte[] FooterImage
        {
            get
            {
                byte[] buffer = new byte[FooterLength];
                PhasorDefinition phasorDefinition;
                AnalogDefinition analogDefinition;
                DigitalDefinition digitalDefinition;
                int x, index = 0;

                // Include conversion factors in configuration cell footer
                for (x = 0; x < PhasorDefinitions.Count; x++)
                {
                    phasorDefinition = PhasorDefinitions[x] as PhasorDefinition;

                    if (phasorDefinition != null)
                        phasorDefinition.ConversionFactorImage.CopyImage(buffer, ref index, PhasorDefinition.ConversionFactorLength);
                }

                for (x = 0; x < AnalogDefinitions.Count; x++)
                {
                    analogDefinition = AnalogDefinitions[x] as AnalogDefinition;

                    if (analogDefinition != null)
                        analogDefinition.ConversionFactorImage.CopyImage(buffer, ref index, AnalogDefinition.ConversionFactorLength);
                }

                for (x = 0; x < DigitalDefinitions.Count; x++)
                {
                    digitalDefinition = DigitalDefinitions[x] as DigitalDefinition;

                    if (digitalDefinition != null)
                        digitalDefinition.ConversionFactorImage.CopyImage(buffer, ref index, DigitalDefinition.ConversionFactorLength);
                }

                // Include nominal frequency
                base.FooterImage.CopyImage(buffer, ref index, base.FooterLength);

                // Include configuration count (new for version 7.0)
                if (Parent.DraftRevision > DraftRevision.Draft6)
                    EndianOrder.BigEndian.CopyBytes(RevisionCount, buffer, index);

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

                baseAttributes.Add("Format Flags", (int)m_formatFlags + ": " + m_formatFlags);

                return baseAttributes;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Parses the binary header image.
        /// </summary>
        /// <param name="binaryImage">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="binaryImage"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="binaryImage"/>.</param>
        /// <returns>The length of the data that was parsed.</returns>
        protected override int ParseHeaderImage(byte[] binaryImage, int startIndex, int length)
        {
            IConfigurationCellParsingState state = State;
            int index = startIndex;

            // Parse out station name
            index += base.ParseHeaderImage(binaryImage, startIndex, length);

            IDCode = EndianOrder.BigEndian.ToUInt16(binaryImage, index);
            m_formatFlags = (FormatFlags)EndianOrder.BigEndian.ToUInt16(binaryImage, index + 2);

            // Parse out total phasors, analogs and digitals defined for this device
            state.PhasorCount = EndianOrder.BigEndian.ToUInt16(binaryImage, index + 4);
            state.AnalogCount = EndianOrder.BigEndian.ToUInt16(binaryImage, index + 6);
            state.DigitalCount = EndianOrder.BigEndian.ToUInt16(binaryImage, index + 8);
            index += 10;

            return (index - startIndex);
        }

        /// <summary>
        /// Parses the binary footer image.
        /// </summary>
        /// <param name="binaryImage">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="binaryImage"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="binaryImage"/>.</param>
        /// <returns>The length of the data that was parsed.</returns>
        protected override int ParseFooterImage(byte[] binaryImage, int startIndex, int length)
        {
            PhasorDefinition phasorDefinition;
            AnalogDefinition analogDefinition;
            DigitalDefinition digitalDefinition;
            int x, index = startIndex;

            // Parse conversion factors from configuration cell footer
            for (x = 0; x < PhasorDefinitions.Count; x++)
            {
                phasorDefinition = PhasorDefinitions[x] as PhasorDefinition;

                if (phasorDefinition != null)
                    index += phasorDefinition.ParseConversionFactor(binaryImage, index);
            }

            for (x = 0; x < AnalogDefinitions.Count; x++)
            {
                analogDefinition = AnalogDefinitions[x] as AnalogDefinition;

                if (analogDefinition != null)
                    index += analogDefinition.ParseConversionFactor(binaryImage, index);
            }

            for (x = 0; x < DigitalDefinitions.Count; x++)
            {
                digitalDefinition = DigitalDefinitions[x] as DigitalDefinition;

                if (digitalDefinition != null)
                    index += digitalDefinition.ParseConversionFactor(binaryImage, index);
            }

            // Parse nominal frequency
            index += base.ParseFooterImage(binaryImage, index, length);

            // Parse out configuration count (new for version 7.0)
            if (Parent.DraftRevision > DraftRevision.Draft6)
            {
                RevisionCount = EndianOrder.BigEndian.ToUInt16(binaryImage, index);
                index += 2;
            }

            return (index - startIndex);
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
            info.AddValue("formatFlags", m_formatFlags, typeof(FormatFlags));
        }

        #endregion

        #region [ Static ]

        // Static Methods

        // Delegate handler to create a new IEEE C37.118 configuration cell
        internal static IConfigurationCell CreateNewCell(IChannelFrame parent, IChannelFrameParsingState<IConfigurationCell> state, int index, byte[] binaryImage, int startIndex, out int parsedLength)
        {
            ConfigurationCell configCell = new ConfigurationCell(parent as IConfigurationFrame);

            parsedLength = configCell.Initialize(binaryImage, startIndex, 0);

            return configCell;
        }

        #endregion
    }
}