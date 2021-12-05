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
namespace GSF.PhasorProtocols.IEEE1344
{
    /// <summary>
    /// Represents the IEEE 1344 implementation of a <see cref="IConfigurationCell"/> that can be sent or received.
    /// </summary>
    [Serializable]
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
                PhasorDefinition.CreateNewDefinition,
                IEEE1344.FrequencyDefinition.CreateNewDefinition,
                null, // IEEE 1344 doesn't define analogs
                DigitalDefinition.CreateNewDefinition);
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
            get => base.Parent as ConfigurationFrame;
            set => base.Parent = value;
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
            get => m_statusFlags;
            set => m_statusFlags = value;
        }

        /// <summary>
        /// Gets or sets the ID code of this <see cref="ConfigurationCell"/>.
        /// </summary>
        public new ulong IDCode
        {
            // IEEE 1344 only allows one device, so we share ID code with parent frame...
            get => Parent.IDCode;
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
            get => (m_statusFlags & (ushort)Bits.Bit15) == 0;
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
            get => (m_statusFlags & (ushort)Bits.Bit14) == 0;
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
            get => (TriggerStatus)(m_statusFlags & Common.TriggerMask);
            set => m_statusFlags = (ushort)((m_statusFlags & ~Common.TriggerMask) | (ushort)value);
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
            get => DataFormat.FixedInteger;
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
            get => m_coordinateFormat;
            set => m_coordinateFormat = value;
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
            get => DataFormat.FixedInteger;
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
            get => DataFormat.FixedInteger;
            set
            {
                if (value != DataFormat.FixedInteger)
                    throw new NotSupportedException("IEEE 1344 only supports scaled data");
            }
        }

        /// <summary>
        /// Gets the length of the <see cref="HeaderImage"/>.
        /// </summary>
        protected override int HeaderLength => base.HeaderLength + 14;

        /// <summary>
        /// Gets the binary header image of the <see cref="ConfigurationCell"/> object.
        /// </summary>
        protected override byte[] HeaderImage
        {
            get
            {
                byte[] buffer = new byte[HeaderLength];
                int index = 0;

                BigEndian.CopyBytes(m_statusFlags, buffer, index);

                // Copy in station name
                index += 2;
                base.HeaderImage.CopyImage(buffer, ref index, base.HeaderLength);

                BigEndian.CopyBytes(IDCode, buffer, index);
                BigEndian.CopyBytes((ushort)PhasorDefinitions.Count, buffer, index + 8);
                BigEndian.CopyBytes((ushort)DigitalDefinitions.Count, buffer, index + 10);

                return buffer;
            }
        }

        /// <summary>
        /// Gets the length of the <see cref="FooterImage"/>.
        /// </summary>
        protected override int FooterLength => base.FooterLength + PhasorDefinitions.Count * PhasorDefinition.ConversionFactorLength + DigitalDefinitions.Count * DigitalDefinition.ConversionFactorLength;

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
                    PhasorDefinition phasorDefinition = PhasorDefinitions[x] as PhasorDefinition;
                    phasorDefinition?.ConversionFactorImage.CopyImage(buffer, ref index, PhasorDefinition.ConversionFactorLength);
                }

                for (x = 0; x < DigitalDefinitions.Count; x++)
                {
                    DigitalDefinition digitalDefinition = DigitalDefinitions[x] as DigitalDefinition;
                    digitalDefinition?.ConversionFactorImage.CopyImage(buffer, ref index, DigitalDefinition.ConversionFactorLength);
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
                baseAttributes.Add("Trigger Status", $"{(int)TriggerStatus}: {TriggerStatus}");

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

            m_statusFlags = BigEndian.ToUInt16(buffer, index);
            index += 2;

            // Parse out station name
            index += base.ParseHeaderImage(buffer, index, length);

            IDCode = BigEndian.ToUInt64(buffer, index);

            // Parse out total phasors and digitals defined for this device
            state.PhasorCount = BigEndian.ToUInt16(buffer, index + 8);
            state.DigitalCount = BigEndian.ToUInt16(buffer, index + 10);
            index += 12;

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
            int index = startIndex;

            // Parse conversion factors from configuration cell footer
            for (int x = 0; x < PhasorDefinitions.Count; x++)
            {
                if (PhasorDefinitions[x] is PhasorDefinition phasorDefinition)
                    index += phasorDefinition.ParseConversionFactor(buffer, index);
            }

            for (int x = 0; x < DigitalDefinitions.Count; x++)
            {
                if (DigitalDefinitions[x] is DigitalDefinition digitalDefinition)
                    index += digitalDefinition.ParseConversionFactor(buffer, index);
            }

            // Parse nominal frequency
            index += base.ParseFooterImage(buffer, index, length);

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
            info.AddValue("coordinateFormat", m_coordinateFormat, typeof(CoordinateFormat));
            info.AddValue("statusFlags", m_statusFlags);
        }

        #endregion

        #region [ Static ]

        // Static Methods

        // Delegate handler to create a new IEEE 1344 configuration cell
        internal static IConfigurationCell CreateNewCell(IChannelFrame parent, IChannelFrameParsingState<IConfigurationCell> state, int index, byte[] buffer, int startIndex, out int parsedLength)
        {
            ConfigurationCell configCell = new(parent as IConfigurationFrame);

            parsedLength = configCell.ParseBinaryImage(buffer, startIndex, 0);

            return configCell;
        }

        #endregion
    }
}