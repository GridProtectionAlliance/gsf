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

namespace TVA.PhasorProtocols.Ieee1344
{
    /// <summary>
    /// Represents the IEEE 1344 implementation of a <see cref="IConfigurationCell"/> that can be sent or received.
    /// </summary>
    [Serializable()]
    public class ConfigurationCell : ConfigurationCellBase
    {
        #region [ Members ]

        // Fields
        private CoordinateFormat m_coordinateFormat;
        private ushort m_statusFlags;

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
                Ieee1344.PhasorDefinition.CreateNewDefinition,
                Ieee1344.FrequencyDefinition.CreateNewDefinition,
                null, // IEEE 1344 doesn't define analogs
                Ieee1344.DigitalDefinition.CreateNewDefinition);
        }

        /// <summary>
        /// Creates a new <see cref="ConfigurationCell"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The reference to parent <see cref="ConfigurationFrame"/> of this <see cref="ConfigurationCell"/>.</param>
        /// <param name="idCode">The numeric ID code for this <see cref="ConfigurationCell"/>.</param>
        /// <param name="nominalFrequency">The nominal <see cref="LineFrequency"/> of the <see cref="FrequencyDefinition"/> of this <see cref="ConfigurationCell"/>.</param>
        public ConfigurationCell(ConfigurationFrame parent, ulong idCode, LineFrequency nominalFrequency)
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
            m_coordinateFormat = (CoordinateFormat)info.GetValue("coordinateFormat", typeof(CoordinateFormat));
            m_statusFlags = info.GetUInt16("statusFlags");
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets a reference to the parent <see cref="ConfigurationFrame"/> for this <see cref="ConfigurationCell"/>.
        /// </summary>
        public new ConfigurationFrame Parent
        {
            get
            {
                return base.Parent as ConfigurationFrame;
            }
            set
            {
                base.Parent = value;
            }
        }

        /// <summary>
        /// Gets or sets status flags of this <see cref="ConfigurationCell"/>.
        /// </summary>
        /// <remarks>
        /// These are bit flags, use properties to change basic values.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public ushort StatusFlags
        {
            get
            {
                return m_statusFlags;
            }
            set
            {
                m_statusFlags = value;
            }
        }

        /// <summary>
        /// Gets or sets the ID code of this <see cref="ConfigurationCell"/>.
        /// </summary>
        public new ulong IDCode
        {
            // IEEE 1344 only allows one device, so we share ID code with parent frame...
            get
            {
                return Parent.IDCode;
            }
            set
            {
                Parent.IDCode = value;

                // Base classes constrain maximum value to 65535
                base.IDCode = value > ushort.MaxValue ? ushort.MaxValue : (ushort)value;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if timestamp of this <see cref="ConfigurationCell"/> is valid based on GPS lock.
        /// </summary>
        public bool SynchronizationIsValid
        {
            get
            {
                return (m_statusFlags & (ushort)Bits.Bit15) == 0;
            }
            set
            {
                if (value)
                    m_statusFlags = (ushort)(m_statusFlags & ~(ushort)Bits.Bit15);
                else
                    m_statusFlags = (ushort)(m_statusFlags | (ushort)Bits.Bit15);
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if data of this <see cref="ConfigurationCell"/> is valid.
        /// </summary>
        public bool DataIsValid
        {
            get
            {
                return (m_statusFlags & (ushort)Bits.Bit14) == 0;
            }
            set
            {
                if (value)
                    m_statusFlags = (ushort)(m_statusFlags & ~(ushort)Bits.Bit14);
                else
                    m_statusFlags = (ushort)(m_statusFlags | (ushort)Bits.Bit14);
            }
        }

        /// <summary>
        /// Gets or sets trigger status of this <see cref="ConfigurationCell"/>.
        /// </summary>
        public TriggerStatus TriggerStatus
        {
            get
            {
                return (TriggerStatus)(m_statusFlags & Common.TriggerMask);
            }
            set
            {
                m_statusFlags = (ushort)((m_statusFlags & ~Common.TriggerMask) | (ushort)value);
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="DataFormat"/> for the <see cref="IPhasorDefinition"/> objects in the <see cref="ConfigurationCellBase.PhasorDefinitions"/> of this <see cref="ConfigurationCell"/>.
        /// </summary>
        /// <remarks>
        /// This property only supports scaled data; IEEE 1344 doesn't transport floating point values.
        /// </remarks>
        /// <exception cref="NotSupportedException">IEEE 1344 only supports scaled data.</exception>
        public override DataFormat PhasorDataFormat
        {
            get
            {
                return DataFormat.FixedInteger;
            }
            set
            {
                if (value != DataFormat.FixedInteger)
                    throw new NotSupportedException("IEEE 1344 only supports scaled data");
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="CoordinateFormat"/> for the <see cref="IPhasorDefinition"/> objects in the <see cref="ConfigurationCellBase.PhasorDefinitions"/> of this <see cref="ConfigurationCell"/>.
        /// </summary>
        public override CoordinateFormat PhasorCoordinateFormat
        {
            get
            {
                return m_coordinateFormat;
            }
            set
            {
                m_coordinateFormat = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="DataFormat"/> of the <see cref="FrequencyDefinition"/> of this <see cref="ConfigurationCell"/>.
        /// </summary>
        /// <remarks>
        /// This property only supports scaled data; IEEE 1344 doesn't transport floating point values.
        /// </remarks>
        /// <exception cref="NotSupportedException">IEEE 1344 only supports scaled data.</exception>
        public override DataFormat FrequencyDataFormat
        {
            get
            {
                return DataFormat.FixedInteger;
            }
            set
            {
                if (value != DataFormat.FixedInteger)
                    throw new NotSupportedException("IEEE 1344 only supports scaled data");
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="DataFormat"/> for the <see cref="IAnalogDefinition"/> objects in the <see cref="ConfigurationCellBase.AnalogDefinitions"/> of this <see cref="ConfigurationCell"/>.
        /// </summary>
        /// <remarks>
        /// <para>IEEE 1344 doesn't define any analog values.</para>
        /// <para>This property only supports scaled data; IEEE 1344 doesn't transport floating point values.</para>
        /// </remarks>
        /// <exception cref="NotSupportedException">IEEE 1344 only supports scaled data.</exception>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override DataFormat AnalogDataFormat
        {
            get
            {
                return DataFormat.FixedInteger;
            }
            set
            {
                if (value != DataFormat.FixedInteger)
                    throw new NotSupportedException("IEEE 1344 only supports scaled data");
            }
        }

        /// <summary>
        /// Gets the length of the <see cref="HeaderImage"/>.
        /// </summary>
        protected override int HeaderLength
        {
            get
            {
                return base.HeaderLength + 14;
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

                EndianOrder.BigEndian.CopyBytes(m_statusFlags, buffer, index);

                // Copy in station name
                index += 2;
                base.HeaderImage.CopyImage(buffer, ref index, base.HeaderLength);

                EndianOrder.BigEndian.CopyBytes(IDCode, buffer, index);
                EndianOrder.BigEndian.CopyBytes((ushort)PhasorDefinitions.Count, buffer, index + 8);
                EndianOrder.BigEndian.CopyBytes((ushort)DigitalDefinitions.Count, buffer, index + 10);

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
                return base.FooterLength + PhasorDefinitions.Count * PhasorDefinition.ConversionFactorLength + DigitalDefinitions.Count * DigitalDefinition.ConversionFactorLength;
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
                DigitalDefinition digitalDefinition;
                int x, index = 0;

                // Include conversion factors in configuration cell footer
                for (x = 0; x < PhasorDefinitions.Count; x++)
                {
                    phasorDefinition = PhasorDefinitions[x] as PhasorDefinition;

                    if (phasorDefinition != null)
                        phasorDefinition.ConversionFactorImage.CopyImage(buffer, ref index, PhasorDefinition.ConversionFactorLength);
                }

                for (x = 0; x < DigitalDefinitions.Count; x++)
                {
                    digitalDefinition = DigitalDefinitions[x] as DigitalDefinition;

                    if (digitalDefinition != null)
                        digitalDefinition.ConversionFactorImage.CopyImage(buffer, ref index, DigitalDefinition.ConversionFactorLength);
                }

                // Include nominal frequency
                base.FooterImage.CopyImage(buffer, ref index, base.FooterLength);

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

                baseAttributes.Add("Status Flags", StatusFlags.ToString());
                baseAttributes.Add("Synchronization Is Valid", SynchronizationIsValid.ToString());
                baseAttributes.Add("Data Is Valid", DataIsValid.ToString());
                baseAttributes.Add("Trigger Status", (int)TriggerStatus + ": " + TriggerStatus);

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

            m_statusFlags = EndianOrder.BigEndian.ToUInt16(binaryImage, index);
            index += 2;

            // Parse out station name
            index += base.ParseHeaderImage(binaryImage, index, length);

            IDCode = EndianOrder.BigEndian.ToUInt64(binaryImage, index);

            // Parse out total phasors and digitals defined for this device
            state.PhasorCount = EndianOrder.BigEndian.ToUInt16(binaryImage, index + 8);
            state.DigitalCount = EndianOrder.BigEndian.ToUInt16(binaryImage, index + 10);
            index += 12;

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
            DigitalDefinition digitalDefinition;
            int x, index = startIndex;

            // Parse conversion factors from configuration cell footer
            for (x = 0; x < PhasorDefinitions.Count; x++)
            {
                phasorDefinition = PhasorDefinitions[x] as PhasorDefinition;

                if (phasorDefinition != null)
                    index += phasorDefinition.ParseConversionFactor(binaryImage, index);
            }

            for (x = 0; x < DigitalDefinitions.Count; x++)
            {
                digitalDefinition = DigitalDefinitions[x] as DigitalDefinition;

                if (digitalDefinition != null)
                    index += digitalDefinition.ParseConversionFactor(binaryImage, index);
            }

            // Parse nominal frequency
            index += base.ParseFooterImage(binaryImage, index, length);

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
            info.AddValue("coordinateFormat", m_coordinateFormat, typeof(CoordinateFormat));
            info.AddValue("statusFlags", m_statusFlags);
        }

        #endregion

        #region [ Static ]

        // Static Methods

        // Delegate handler to create a new IEEE 1344 configuration cell
        internal static IConfigurationCell CreateNewCell(IChannelFrame parent, IChannelFrameParsingState<IConfigurationCell> state, int index, byte[] binaryImage, int startIndex, out int parsedLength)
        {
            ConfigurationCell configCell = new ConfigurationCell(parent as IConfigurationFrame);

            parsedLength = configCell.Initialize(binaryImage, startIndex, 0);

            return configCell;
        }

        #endregion       
    }
}