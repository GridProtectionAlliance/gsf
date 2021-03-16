//******************************************************************************************************
//  ConfigurationCell3.cs - Gbtc
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
//  09/08/2011 - Andrew Krohne
//       Generated original version of source code.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using GSF.Units.EE;

// ReSharper disable VirtualMemberCallInConstructor
namespace GSF.PhasorProtocols.IEEEC37_118
{
    /// <summary>
    /// Represents the IEEE C37.118 implementation of a <see cref="IConfigurationCell"/> that can be sent or received.
    /// </summary>
    [Serializable]
    public sealed class ConfigurationCell3 : ConfigurationCellBase
    {
        #region [ Members ]

        // Fields
        private FormatFlags m_formatFlags;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ConfigurationCell3"/>.
        /// </summary>
        /// <param name="parent">The reference to parent <see cref="IConfigurationFrame"/> of this <see cref="ConfigurationCell3"/>.</param>
        public ConfigurationCell3(IConfigurationFrame parent) : base(parent, 0, Common.MaximumPhasorValues, Common.MaximumAnalogValues, Common.MaximumDigitalValues, false)
        {
            // Define new parsing state which defines constructors for key configuration values
            State = new ConfigurationCellParsingState(
                PhasorDefinition3.CreateNewDefinition,
                IEEEC37_118.FrequencyDefinition.CreateNewDefinition,
                AnalogDefinition3.CreateNewDefinition,
                DigitalDefinition3.CreateNewDefinition);
        }

        /// <summary>
        /// Creates a new <see cref="ConfigurationCell3"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The reference to parent <see cref="ConfigurationFrame1"/> of this <see cref="ConfigurationCell3"/>.</param>
        /// <param name="idCode">The numeric ID code for this <see cref="ConfigurationCell3"/>.</param>
        /// <param name="nominalFrequency">The nominal <see cref="LineFrequency"/> of the <see cref="FrequencyDefinition"/> of this <see cref="ConfigurationCell3"/>.</param>
        public ConfigurationCell3(ConfigurationFrame1 parent, ushort idCode, LineFrequency nominalFrequency) //FIXME: Does this need to use config frame 3?
            : this(parent)
        {
            IDCode = idCode;
            NominalFrequency = nominalFrequency;
        }

        /// <summary>
        /// Creates a new <see cref="ConfigurationCell3"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        private ConfigurationCell3(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            float getSingle(string name)
            {
                return !(info.GetValue(name, typeof(string)) is string element) || element == "INF" ?
                    float.PositiveInfinity :
                    float.Parse(element);
            }

            // Deserialize configuration cell
            m_formatFlags = (FormatFlags)info.GetValue("formatFlags", typeof(FormatFlags));
            GlobalID = info.GetOrDefault("globalID", Guid.Empty);

            // Decode PMU_LAT, PMU_LON, PMU_ELEV, SVC_CLASS, WINDOW, GRP_DLY values
            Latitude = getSingle("latitude");
            Longitude = getSingle("longitude");
            Elevation = getSingle("elevation");
            ServiceClass = info.GetChar("serviceClass");
            Window = info.GetInt32("window");
            GroupDelay = info.GetInt32("groupDelay");
            DataModified = info.GetBoolean("dataModified");
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets a reference to the parent <see cref="ConfigurationFrame1"/> for this <see cref="ConfigurationCell3"/>.
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
        /// Gets or sets G_PMU_ID value for this <see cref="ConfigurationCell3"/>.
        /// </summary>
        public Guid GlobalID { get; set; } = Guid.Empty;

        /// <summary>
        /// Gets or sets PMU_LAT value for this <see cref="ConfigurationCell3"/>.
        /// </summary>
        public float Latitude { get; set; } = float.PositiveInfinity;

        /// <summary>
        /// Gets or sets PMU_LON value for this <see cref="ConfigurationCell3"/>.
        /// </summary>
        public float Longitude { get; set; } = float.PositiveInfinity;

        /// <summary>
        /// Gets or sets PMU_ELEV value for this <see cref="ConfigurationCell3"/>.
        /// </summary>
        public float Elevation { get; set; } = float.PositiveInfinity;

        /// <summary>
        /// Gets or sets SVC_CLASS value, 'M' or 'P', for this <see cref="ConfigurationCell3"/>.
        /// </summary>
        public char ServiceClass { get; set; } = 'M';

        /// <summary>
        /// Gets or sets WINDOW value for this <see cref="ConfigurationCell3"/>.
        /// </summary>
        public int Window { get; set; }

        /// <summary>
        /// Gets or sets GRP_DLY value for this <see cref="ConfigurationCell3"/>.
        /// </summary>
        public int GroupDelay { get; set; }

        /// <summary>
        /// Gets or sets flag indicating if data in cell is modified through configuration for this <see cref="ConfigurationCell3"/>.
        /// </summary>
        public bool DataModified { get; set; }

        /// <summary>
        /// Gets the maximum length of the <see cref="ConfigurationCellBase.StationName"/> of this <see cref="ConfigurationCell3"/>.
        /// </summary>
        public override int MaximumStationNameLength => byte.MaxValue;

        /// <summary>
        /// Gets flag that indicates if <see cref="ConfigurationCellBase.StationNameImage"/> should auto pad-right value to <see cref="MaximumStationNameLength"/>.
        /// </summary>
        public override bool AutoPadStationNameImage => false;

        /// <summary>
        /// Gets the length of the <see cref="HeaderImage"/>.
        /// </summary>
        protected override int HeaderLength => GetEncodedStringLength(StationName) + 26;

        /// <summary>
        /// Gets the binary header image of the <see cref="ConfigurationCell3"/> object.
        /// </summary>
        protected override byte[] HeaderImage
        {
            get
            {
                byte[] buffer = new byte[HeaderLength];
                int index = 0;

                index += EncodeLengthPrefixedString(StationName, buffer, index);
                index += BigEndian.CopyBytes(IDCode, buffer, index);
                index += GlobalID.ToRfcBytes(buffer, index);
                index += BigEndian.CopyBytes((ushort)m_formatFlags, buffer, index);
                index += BigEndian.CopyBytes((ushort)PhasorDefinitions.Count, buffer, index);
                index += BigEndian.CopyBytes((ushort)AnalogDefinitions.Count, buffer, index);
                BigEndian.CopyBytes((ushort)DigitalDefinitions.Count, buffer, index);

                return buffer;
            }
        }

        /// <summary>
        /// Gets the length of the <see cref="ConfigurationCellBase.BodyImage"/>.
        /// </summary>
        protected override int BodyLength
        {
            get
            {
                int length = 0;

                // Configuration Frame 3 cells have variable length labels
                foreach (IPhasorDefinition definition in PhasorDefinitions)
                {
                    if (definition is PhasorDefinition3 phasorDefinition)
                        length += phasorDefinition.LabelImage.Length;
                }

                foreach (IAnalogDefinition definition in AnalogDefinitions)
                {
                    if (definition is AnalogDefinition3 analogDefinition)
                        length += analogDefinition.LabelImage.Length;
                }

                foreach (IDigitalDefinition definition in DigitalDefinitions)
                {
                    if (definition is DigitalDefinition3 digitalDefinition)
                    {
                        // Force a refresh of label image before getting image length
                        digitalDefinition.Label = digitalDefinition.Label;
                        length += digitalDefinition.LabelImage.Length;
                    }
                }

                return length;
            }
        }

        /// <summary>
        /// Gets the length of the <see cref="FooterImage"/>.
        /// </summary>
        protected override int FooterLength =>
            PhasorDefinitions.Count * PhasorDefinition3.ConversionFactorLength + 
            AnalogDefinitions.Count * AnalogDefinition3.ConversionFactorLength + 
            DigitalDefinitions.Count * DigitalDefinition3.ConversionFactorLength +
            21 + base.FooterLength + 2;

        /// <summary>
        /// Gets the binary footer image of the <see cref="ConfigurationCell3"/> object.
        /// </summary>
        protected override byte[] FooterImage
        {
            get
            {
                byte[] buffer = new byte[FooterLength];
                int index = 0;

                // Include conversion factors in configuration cell footer
                foreach (IPhasorDefinition definition in PhasorDefinitions)
                {
                    if (definition is PhasorDefinition3 phasorDefinition)
                        phasorDefinition.ConversionFactorImage.CopyImage(buffer, ref index, PhasorDefinition3.ConversionFactorLength);
                }

                foreach (IAnalogDefinition definition in AnalogDefinitions)
                {
                    if (definition is AnalogDefinition3 analogDefinition)
                        analogDefinition.ConversionFactorImage.CopyImage(buffer, ref index, AnalogDefinition3.ConversionFactorLength);
                }

                foreach (IDigitalDefinition definition in DigitalDefinitions)
                {
                    if (definition is DigitalDefinition3 digitalDefinition)
                        digitalDefinition.ConversionFactorImage.CopyImage(buffer, ref index, DigitalDefinition3.ConversionFactorLength);
                }

                // Add PMU_LAT, PMU_LON, PMU_ELEV, SVC_CLASS, WINDOW, GRP_DLY values
                index += BigEndian.CopyBytes(Latitude, buffer, index);
                index += BigEndian.CopyBytes(Longitude, buffer, index);
                index += BigEndian.CopyBytes(Elevation, buffer, index);
                buffer[index++] = (byte)ServiceClass;
                index += BigEndian.CopyBytes(Window, buffer, index);
                index += BigEndian.CopyBytes(GroupDelay, buffer, index);

                // Include nominal frequency
                base.FooterImage.CopyImage(buffer, ref index, base.FooterLength);

                BigEndian.CopyBytes(RevisionCount, buffer, index);

                return buffer;
            }
        }

        /// <summary>
        /// <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for the <see cref="ConfigurationCell3"/> object.
        /// </summary>
        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("Format Flags", $"{(int)m_formatFlags}: {m_formatFlags}");
                baseAttributes.Add("Global ID", $"{GlobalID}: {ByteEncoding.Hexadecimal.GetString(GlobalID.ToRfcBytes(), ' ')}");
                baseAttributes.Add("Latitude", float.IsInfinity(Latitude) ? "INF" : Latitude.ToString(CultureInfo.InvariantCulture));
                baseAttributes.Add("Longitude", float.IsInfinity(Longitude) ? "INF" : Longitude.ToString(CultureInfo.InvariantCulture));
                baseAttributes.Add("Elevation", float.IsInfinity(Elevation) ? "INF" : Elevation.ToString(CultureInfo.InvariantCulture));
                baseAttributes.Add("Service Class", $"{ServiceClass}");
                baseAttributes.Add("Window", $"{Window}");
                baseAttributes.Add("Group Delay", $"{GroupDelay}");
                baseAttributes.Add("Data Modified", $"{DataModified}");

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
            StationName = DecodeLengthPrefixedString(buffer, ref index);

            IDCode = BigEndian.ToUInt16(buffer, index);
            index += 2;

            GlobalID = buffer.ToRfcGuid(index);
            index += 16;

            m_formatFlags = (FormatFlags)BigEndian.ToUInt16(buffer, index);
            index += 2;

            // Parse out total phasors, analogs and digitals defined for this device
            state.PhasorCount = BigEndian.ToUInt16(buffer, index);
            index += 2;
            
            state.AnalogCount = BigEndian.ToUInt16(buffer, index);
            index += 2;

            state.DigitalCount = BigEndian.ToUInt16(buffer, index);
            index += 2;

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
            foreach (IPhasorDefinition definition in PhasorDefinitions)
            {
                if (definition is PhasorDefinition3 phasorDefinition)
                    index += phasorDefinition.ParseConversionFactor(buffer, index);
            }

            foreach (IAnalogDefinition definition in AnalogDefinitions)
            {
                if (definition is AnalogDefinition3 analogDefinition)
                    index += analogDefinition.ParseConversionFactor(buffer, index);
            }

            foreach (IDigitalDefinition definition in DigitalDefinitions)
            {
                if (definition is DigitalDefinition3 digitalDefinition)
                    index += digitalDefinition.ParseConversionFactor(buffer, index);
            }

            // Parse PMU_LAT, PMU_LON, PMU_ELEV, SVC_CLASS, WINDOW, GRP_DLY values
            Latitude = BigEndian.ToSingle(buffer, index);
            index += 4;

            Longitude = BigEndian.ToSingle(buffer, index);
            index += 4;

            Elevation = BigEndian.ToSingle(buffer, index);
            index += 4;

            ServiceClass = (char)buffer[index++];

            Window = BigEndian.ToInt32(buffer, index);
            index += 4;

            GroupDelay = BigEndian.ToInt32(buffer, index);
            index += 4;

            // Parse nominal frequency
            index += base.ParseFooterImage(buffer, index, length);

            RevisionCount = BigEndian.ToUInt16(buffer, index);
            index += 2;

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
            info.AddValue("globalID", GlobalID.ToString());

            // Encode PMU_LAT, PMU_LON, PMU_ELEV, SVC_CLASS, WINDOW, GRP_DLY values
            info.AddValue("latitude", Latitude);
            info.AddValue("longitude", Longitude);
            info.AddValue("elevation", Elevation);
            info.AddValue("serviceClass", ServiceClass);
            info.AddValue("window", Window);
            info.AddValue("groupDelay", GroupDelay);
            info.AddValue("dataModified", DataModified);
        }

        #endregion

        #region [ Static ]

        // Static Methods

        // Delegate handler to create a new IEEE C37.118 configuration cell
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "This must match delegate signature")]
        internal static IConfigurationCell CreateNewCell(IChannelFrame parent, IChannelFrameParsingState<IConfigurationCell> state, int index, byte[] buffer, int startIndex, out int parsedLength)
        {
            ConfigurationCell3 configCell = new ConfigurationCell3(parent as IConfigurationFrame);

            parsedLength = configCell.ParseBinaryImage(buffer, startIndex, 0);

            return configCell;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetEncodedStringLength(string value)
        {
            if (value == null)
                return 1;

            return Encoding.UTF8.GetByteCount(value) + 1;
        }

        // Encode a string into a UTF-8 byte array, validating length.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte[] EncodeUTF8String(string value)
        {
            if (value == null)
                value = "";

            if (value.Length > byte.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(value), $"Cannot encode a length prefixed string for IEEE C37.118-2011 that is longer than {byte.MaxValue} bytes. Source string length before encoding is already {value.Length:N0} characters.");

            byte[] bytes = Encoding.UTF8.GetBytes(value);

            if (bytes.Length > byte.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(value), $"Cannot encode a length prefixed string for IEEE C37.118-2011 that is longer than {byte.MaxValue} bytes. UTF-8 encoded string length is {bytes.Length:N0} bytes.");

            return bytes;
        }

        /// <summary>
        /// Encodes a length-prefixed string. Length size is one byte per IEEE C37.118-2011 specification.
        /// </summary>
        /// <param name="value">String value to encode using UTF-8 encoding..</param>
        /// <param name="buffer">Target buffer.</param>
        /// <param name="offset">Target buffer offset.</param>
        /// <returns>Total number of encoded bytes copied into <paramref name="buffer"/>.</returns>
        public static int EncodeLengthPrefixedString(string value, byte[] buffer, int offset)
        {
            byte[] bytes = EncodeUTF8String(value);
            
            int totalLength = bytes.Length + 1;
            buffer.ValidateParameters(offset, totalLength);

            buffer[offset] = (byte)bytes.Length;
            Buffer.BlockCopy(bytes, 0, buffer, offset + 1, bytes.Length);

            return totalLength;
        }

        /// <summary>
        /// Encodes a length-prefixed string. Length size is one byte per IEEE C37.118-2011 specification.
        /// </summary>
        /// <param name="value">String value to encode using UTF-8 encoding..</param>
        /// <returns>Encoded string.</returns>
        public static byte[] EncodeLengthPrefixedString(string value)
        {
            byte[] bytes = EncodeUTF8String(value);
            byte[] buffer = new byte[bytes.Length + 1];
            
            buffer[0] = (byte)bytes.Length;
            Buffer.BlockCopy(bytes, 0, buffer, 1, bytes.Length);

            return buffer;
        }

        /// <summary>
        /// Decodes a length-prefixed string. Length size is one byte per IEEE C37.118-2011 specification.
        /// </summary>
        /// <param name="buffer">Source buffer.</param>
        /// <param name="offset">Source buffer offset. Value is auto-incremented.</param>
        /// <returns>Decoded string from bytes encoded in <paramref name="buffer"/>.</returns>
        public static string DecodeLengthPrefixedString(byte[] buffer, ref int offset)
        {
            if (buffer.Length - 1 < offset)
                throw new ArgumentOutOfRangeException(nameof(offset), $"offset of {offset} and length of 1 will exceed buffer size of {buffer.Length:N0}");

            int length = buffer[offset++];
            buffer.ValidateParameters(offset, length);

            string result = Encoding.UTF8.GetString(buffer, offset, length);
            offset += length;

            return result;
        }

        #endregion
    }
}