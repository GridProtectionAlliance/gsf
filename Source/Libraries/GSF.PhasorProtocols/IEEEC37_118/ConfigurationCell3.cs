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
using System.Runtime.Serialization;
using GSF.Units.EE;

// ReSharper disable VirtualMemberCallInConstructor
namespace GSF.PhasorProtocols.IEEEC37_118
{
    /// <summary>
    /// Represents the IEEE C37.118 implementation of a <see cref="IConfigurationCell"/> that can be sent or received.
    /// </summary>
    [Serializable]
    public class ConfigurationCell3 : ConfigurationCellBase
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
        public ConfigurationCell3(IConfigurationFrame parent) : base(parent, 0, Common.MaximumPhasorValues, Common.MaximumAnalogValues, Common.MaximumDigitalValues)
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
        protected ConfigurationCell3(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Deserialize configuration cell
            m_formatFlags = (FormatFlags)info.GetValue("formatFlags", typeof(FormatFlags));
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


        public override DataFormat PhasorDataFormat { get; set; }
        public override CoordinateFormat PhasorCoordinateFormat { get; set; }
        public override DataFormat FrequencyDataFormat { get; set; }
        public override DataFormat AnalogDataFormat { get; set; }

        /// <summary>
        /// Gets the maximum length of the <see cref="ConfigurationCellBase.StationName"/> of this <see cref="ConfigurationCell3"/>.
        /// </summary>
        public override int MaximumStationNameLength => 255;

        /// <summary>
        /// Gets flag that indicates if <see cref="ConfigurationCellBase.StationNameImage"/> should auto pad-right value to <see cref="MaximumStationNameLength"/>.
        /// </summary>
        public override bool AutoPadStationNameImage => false;

        /// <summary>
        /// Gets the length of the <see cref="HeaderImage"/>.
        /// </summary>
        protected override int HeaderLength => base.HeaderLength + 10;

        /// <summary>
        /// Gets the binary header image of the <see cref="ConfigurationCell3"/> object.
        /// </summary>
        protected override byte[] HeaderImage
        {
            get
            {
                byte[] buffer = new byte[HeaderLength];
                int index = 0;

                base.HeaderImage.CopyImage(buffer, ref index, base.HeaderLength);
                BigEndian.CopyBytes(IDCode, buffer, index);
                BigEndian.CopyBytes(StationName.Length, buffer, index);
                //BigEndian.CopyBytes(StationName, buffer, index);
                //BigEndian.CopyBytes(G_PMU_ID, buffer, index);
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
        /// Gets the binary footer image of the <see cref="ConfigurationCell3"/> object.
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

                baseAttributes.Add("Format Flags", (int)m_formatFlags + ": " + m_formatFlags);

                return baseAttributes;
            }
        }

        #endregion

        #region [ Methods ]

        string getLengthPrependedString(byte[] buffer, int index, out int length)
        {
            length = BigEndian.ToInt16(buffer, index);
            return ByteEncoding.ASCII.GetString(buffer, index + 2, length);
        }

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
            //index += base.ParseHeaderImage(buffer, startIndex, length); //This is wrong. Should call ConfigCellBase
            String StationName;
            int len;
            StationName = getLengthPrependedString(buffer, index, out len);
            index += len + 2;
            IDCode = BigEndian.ToUInt16(buffer, index);
            //State.G_PMU_ID = BigEndian.ToGuid(buffer, index + 2);
            m_formatFlags = (FormatFlags)BigEndian.ToUInt16(buffer, index + 16 + 2); //left as 16+x for clarity while editing, FIXME
            // Parse out total phasors, analogs and digitals defined for this device
            State.PhasorCount = BigEndian.ToUInt16(buffer, index + 16 + 4);
            State.AnalogCount = BigEndian.ToUInt16(buffer, index + 16 + 6);
            State.DigitalCount = BigEndian.ToUInt16(buffer, index + 16 + 8);


            index += 10 + 16; //FIXME: merge

            return (index - startIndex);
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
            //FIXME: magic goes here
            int index = startIndex;
            int strLength = 0;
            int x;
            //CHNAM, length-prepended string
            for (x = 0; x < State.PhasorCount; x++)
            {
                //State.PhasorName[x] = getLengthPrependedString(buffer, index, out strLength); //Need to store this somewhere...
                index += 2 + strLength; // for simplicity
            }
            for (x = 0; x < State.AnalogCount; x++)
            {
                //State.AnalogName[x] = getLengthPrependedString(buffer, index, out strLength);
                index += 2 + strLength;
            }
            for (x = 0; x < State.DigitalCount; x++)
            {
                //State.DigitalName[x] = getLengthPrependedString(buffer, index, out strLength);
                index += 2 + strLength;
            }

            //PHSCALE, 12 bytes of data flags, x PHNMR
            for (x = 0; x < State.PhasorCount; x++)
            {
                //State.PhasorScale[x] = buffer.BlockCopy(index, index + 12);
                index += 12;
            }

            //ANSCALE, 8 bytes x ANNMR
            for (x = 0; x < State.AnalogCount; x++)
            {
                //State.ANSCALE[x] = buffer.BlockCopy(index, index + 8);
                index += 12;

            }
            //DIGUNIT, 4 x DGNMR
            for (x = 0; x < State.DigitalCount; x++)
            {
                //State.DigitalStatus[x] = buffer.BlockCopy(index, index + 4);
                index += 4;
            }
            //PMU_LAT, 4 bytes, IEEE float, -90.0 to +90.0
            //State.DeviceLatitude = BigEndian.ToSingle(buffer, index);
            //PMU_LON, 4 bytes, IEEE float, -179.9... to +180
            //State.DeviceLongitude = BigEndian.ToSingle(buffer, index + 4);
            //PMU_ELEV, 4 bytes, IEEE float, infinity for unknown
            //State.DeviceElevation = BigEndian.ToSingle(buffer, index + 8);
            //SVC_CLASS, 1 ASCII char
            //State.ServiceClass = BigEndian.ToChar(buffer, index + 9);
            //WINDOW, 4 bytes, signed int
            //State.MeasurementWindow = BigEndian.ToInt32(buffer, index + 13);
            //GRP_DLY, 4 bytes, signed int
            //State.GroupDelay = BigEndian.ToInt32(buffer, index + 17);
            //FNOM, 2 bytes, unsigned int, Bit 0 is flag (50/60 hz)
            //State.FNOM = BigEndian.ToUInt16(buffer, index + 19);
            //CFGCNT, 2 bytes
            //State.CFGCNT = BigEndian.ToUInt16(buffer, index + 21);

            return startIndex; //FIXME
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

            // Parse nominal frequency
            index += base.ParseFooterImage(buffer, index, length);

            // Parse out configuration count (new for version 7.0)
            if (Parent.DraftRevision > DraftRevision.Draft6)
            {
                RevisionCount = BigEndian.ToUInt16(buffer, index);
                index += 2;
            }

            return (index - startIndex);
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
            ConfigurationCell3 configCell = new ConfigurationCell3(parent as IConfigurationFrame);

            parsedLength = configCell.ParseBinaryImage(buffer, startIndex, 0);

            return configCell;
        }

        #endregion
    }
}