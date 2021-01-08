//******************************************************************************************************
//  ConfigurationFrame.cs - Gbtc
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
//  01/14/2005 - J. Ritchie Carroll
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
using GSF.IO.Checksums;
using GSF.Parsing;
using GSF.Units.EE;

namespace GSF.PhasorProtocols.IEEE1344
{
    /// <summary>
    /// Represents the IEEE 1344 implementation of a <see cref="IConfigurationFrame"/> that can be sent or received.
    /// </summary>
    [Serializable]
    public class ConfigurationFrame : ConfigurationFrameBase, ISupportSourceIdentifiableFrameImage<SourceChannel, FrameType>
    {
        #region [ Members ]

        // Constants
        private const int FixedFooterLength = 2;

        // Fields
        private CommonFrameHeader m_frameHeader;
        private ulong m_idCode;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ConfigurationFrame"/>.
        /// </summary>
        /// <remarks>
        /// This constructor is used by <see cref="FrameImageParserBase{TTypeIdentifier,TOutputType}"/> to parse an IEEE 1344 configuration frame.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ConfigurationFrame()
            : base(0, new ConfigurationCellCollection(), 0, 0)
        {
        }

        /// <summary>
        /// Creates a new <see cref="ConfigurationFrame"/> from specified parameters.
        /// </summary>
        /// <param name="idCode">The ID code of this <see cref="ConfigurationFrame"/>.</param>
        /// <param name="timestamp">The exact timestamp, in <see cref="Ticks"/>, of the data represented by this <see cref="ConfigurationFrame"/>.</param>
        /// <param name="frameRate">The defined frame rate of this <see cref="ConfigurationFrame"/>.</param>
        /// <remarks>
        /// This constructor is used by a consumer to generate an IEEE 1344 configuration frame.
        /// </remarks>
        public ConfigurationFrame(ulong idCode, Ticks timestamp, ushort frameRate)
            : base(0, new ConfigurationCellCollection(), timestamp, frameRate)
        {
            IDCode = idCode;
        }

        /// <summary>
        /// Creates a new <see cref="ConfigurationFrame"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected ConfigurationFrame(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Deserialize configuration frame
            m_frameHeader = (CommonFrameHeader)info.GetValue("frameHeader", typeof(CommonFrameHeader));
            m_idCode = info.GetUInt64("idCode64Bit");
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets reference to the <see cref="ConfigurationCellCollection"/> for this <see cref="ConfigurationFrame"/>.
        /// </summary>
        public new ConfigurationCellCollection Cells => base.Cells as ConfigurationCellCollection;

        /// <summary>
        /// Gets or sets the ID code of this <see cref="ConfigurationFrame"/>.
        /// </summary>
        public new ulong IDCode
        {
            get => m_idCode;
            set
            {
                m_idCode = value;

                // Base classes constrain maximum value to 65535
                base.IDCode = value > ushort.MaxValue ? ushort.MaxValue : (ushort)value;
            }
        }

        /// <summary>
        /// Gets or sets exact timestamp, in ticks, of the data represented by this <see cref="ConfigurationFrame"/>.
        /// </summary>
        /// <remarks>
        /// The value of this property represents the number of 100-nanosecond intervals that have elapsed since 12:00:00 midnight, January 1, 0001.
        /// </remarks>
        public override Ticks Timestamp
        {
            get => CommonHeader.Timestamp;
            set
            {
                // Keep timestamp updates synchrnonized...
                CommonHeader.Timestamp = value;
                base.Timestamp = value;
            }
        }

        /// <summary>
        /// Gets the timestamp of this frame in NTP format.
        /// </summary>
        public new NtpTimeTag TimeTag => CommonHeader.TimeTag;

        /// <summary>
        /// Gets or sets the nominal <see cref="LineFrequency"/> of this <see cref="ConfigurationFrame"/>.
        /// </summary>
        public LineFrequency NominalFrequency
        {
            // Since IEEE 1344 only supports a single device there will only be one cell, so we just share nominal frequency with our only child
            // and expose the value at the parent level for convenience
            get => Cells[0].NominalFrequency;
            set => Cells[0].NominalFrequency = value;
        }

        /// <summary>
        /// Gets or sets the IEEE 1344 period value.
        /// </summary>
        public ushort Period
        {
            get => (ushort)((double)NominalFrequency / FrameRate * 100.0D);
            set => FrameRate = (ushort)((double)NominalFrequency * 100.0D / value);
        }

        /// <summary>
        /// Gets the identifier that is used to identify the IEEE 1344 frame.
        /// </summary>
        public FrameType TypeID => IEEE1344.FrameType.ConfigurationFrame;

        /// <summary>
        /// Gets or sets current <see cref="CommonFrameHeader"/>.
        /// </summary>
        public CommonFrameHeader CommonHeader
        {
            // Make sure frame header exists - using base class timestamp to
            // prevent recursion (m_frameHeader doesn't exist yet)
            get => m_frameHeader ?? (m_frameHeader = new CommonFrameHeader(TypeID, base.Timestamp));
            set
            {
                m_frameHeader = value;

                if (m_frameHeader is null)
                    return;

                State = m_frameHeader.State as IConfigurationFrameParsingState;
                base.Timestamp = m_frameHeader.Timestamp;
            }
        }

        // This interface implementation satisfies ISupportFrameImage<FrameType>.CommonHeader
        ICommonHeader<FrameType> ISupportFrameImage<FrameType>.CommonHeader
        {
            get => CommonHeader;
            set => CommonHeader = value as CommonFrameHeader;
        }

        /// <summary>
        /// Gets the length of the <see cref="HeaderImage"/>.
        /// </summary>
        protected override int HeaderLength => CommonFrameHeader.FixedLength;

        /// <summary>
        /// Gets the binary header image of the <see cref="ConfigurationFrame"/> object.
        /// </summary>
        protected override byte[] HeaderImage
        {
            get
            {
                // Make sure to provide proper frame length for use in the common header image
                CommonHeader.FrameLength = (ushort)BinaryLength;

                return CommonHeader.BinaryImage;
            }
        }

        /// <summary>
        /// Gets the length of the <see cref="FooterImage"/>.
        /// </summary>
        protected override int FooterLength => FixedFooterLength;

        /// <summary>
        /// Gets the binary footer image of the <see cref="ConfigurationFrame"/> object.
        /// </summary>
        protected override byte[] FooterImage => BigEndian.GetBytes(Period);

        /// <summary>
        /// <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for the <see cref="ConfigurationFrame"/> object.
        /// </summary>
        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                CommonHeader.AppendHeaderAttributes(baseAttributes);

                baseAttributes.Add("64-Bit ID Code", IDCode.ToString());
                baseAttributes.Add("Period", Period.ToString());

                return baseAttributes;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Parses the binary image.
        /// </summary>
        /// <param name="buffer">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="buffer"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="buffer"/>.</param>
        /// <returns>The length of the data that was parsed.</returns>
        /// <remarks>
        /// This method is overridden to parse from cumulated frame images.
        /// </remarks>
        public override int ParseBinaryImage(byte[] buffer, int startIndex, int length)
        {
            // If frame image collector was used, make sure and parse from entire frame image...
            if (m_frameHeader is null)
                return base.ParseBinaryImage(buffer, startIndex, length);

            // If all configuration frame images have been received, we can safely start parsing
            if (m_frameHeader.IsLastFrame)
            {
                FrameImageCollector frameImages = m_frameHeader.FrameImages;

                if (frameImages is null)
                    return State.ParsedBinaryLength;

                // Each individual frame will already have had a CRC check, so we implement standard parse to
                // bypass ChannelBase CRC frame validation on cumulative frame image
                buffer = frameImages.BinaryImage;
                length = frameImages.BinaryLength;
                startIndex = 0;

                // Parse out header, body and footer images
                startIndex += ParseHeaderImage(buffer, startIndex, length);
                startIndex += ParseBodyImage(buffer, startIndex, length - startIndex);
                startIndex += ParseFooterImage(buffer, startIndex, length - startIndex);

                // Include 2 bytes for CRC that was already validated
                return startIndex + 2;
            }

            // There are more configuration frame images coming, keep parser moving by returning total
            // frame length that was already parsed.
            return State.ParsedBinaryLength;
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
            // We already parsed the frame header, so we just skip past it...
            return CommonFrameHeader.FixedLength;
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
            // Period assignment calculates FrameRate using NominalFrequency which is shared
            // with first (and only) configuration cell, since cell was added during ParseBodyImage
            // of ChannelFrameBase, performing the following succeeds since parsing the footer
            // follows parsing the body :)
            Period = BigEndian.ToUInt16(buffer, startIndex);
            return FixedFooterLength;
        }

        /// <summary>
        /// Calculates checksum of given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">Buffer image over which to calculate checksum.</param>
        /// <param name="offset">Start index into <paramref name="buffer"/> to calculate checksum.</param>
        /// <param name="length">Length of data within <paramref name="buffer"/> to calculate checksum.</param>
        /// <returns>Checksum over specified portion of <paramref name="buffer"/>.</returns>
        protected override ushort CalculateChecksum(byte[] buffer, int offset, int length)
        {
            // IEEE 1344 uses CRC-CCITT to calculate checksum for frames
            return buffer.CrcCCITTChecksum(offset, length);
        }

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            // Serialize configuration frame
            info.AddValue("frameHeader", m_frameHeader, typeof(CommonFrameHeader));
            info.AddValue("idCode64Bit", m_idCode);
        }

        #endregion
    }
}