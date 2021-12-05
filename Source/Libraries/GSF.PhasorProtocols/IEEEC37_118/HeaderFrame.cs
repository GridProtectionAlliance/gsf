//******************************************************************************************************
//  HeaderFrame.cs - Gbtc
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
using System.Runtime.Serialization;
using GSF.IO.Checksums;
using GSF.Parsing;

namespace GSF.PhasorProtocols.IEEEC37_118
{
    /// <summary>
    /// Represents the IEEE C37.118 implementation of a <see cref="IHeaderFrame"/> that can be sent or received.
    /// </summary>
    [Serializable]
    public class HeaderFrame : HeaderFrameBase, ISupportSourceIdentifiableFrameImage<SourceChannel, FrameType>
    {
        #region [ Members ]

        // Fields
        private CommonFrameHeader m_frameHeader;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="HeaderFrame"/>.
        /// </summary>
        /// <remarks>
        /// This constructor is used by a consumer or by <see cref="FrameImageParserBase{TTypeIdentifier,TOutputType}"/> to generate or parse an IEEE C37.118 header frame.
        /// </remarks>
        public HeaderFrame()
            : base(new HeaderCellCollection(Common.MaximumDataLength - 1))
        {
        }

        /// <summary>
        /// Creates a new <see cref="HeaderFrame"/>.
        /// </summary>
        /// <param name="headerData"><see cref="string"/> based data to include in this <see cref="HeaderFrame"/>.</param>
        /// <remarks>
        /// This constructor is used by a consumer to generate an IEEE C37.118 header frame.
        /// </remarks>
        public HeaderFrame(string headerData)
            : this()
        {
            base.HeaderData = headerData;
        }

        /// <summary>
        /// Creates a new <see cref="HeaderFrame"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected HeaderFrame(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Deserialize data frame
            m_frameHeader = (CommonFrameHeader)info.GetValue("frameHeader", typeof(CommonFrameHeader));
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets exact timestamp, in ticks, of the data represented by this <see cref="HeaderFrame"/>.
        /// </summary>
        /// <remarks>
        /// The value of this property represents the number of 100-nanosecond intervals that have elapsed since 12:00:00 midnight, January 1, 0001.
        /// </remarks>
        public override Ticks Timestamp
        {
            get => CommonHeader.Timestamp;
            set
            {
                // Keep timestamp updates synchronized...
                CommonHeader.Timestamp = value;
                base.Timestamp = value;
            }
        }

        /// <summary>
        /// Gets the identifier that is used to identify the IEEE C37.118 frame.
        /// </summary>
        public FrameType TypeID => IEEEC37_118.FrameType.HeaderFrame;

        /// <summary>
        /// Gets or sets current <see cref="CommonFrameHeader"/>.
        /// </summary>
        public CommonFrameHeader CommonHeader
        {
            // Make sure frame header exists - using base class timestamp to
            // prevent recursion (m_frameHeader doesn't exist yet)
            get => m_frameHeader ??= new CommonFrameHeader(null, TypeID, base.IDCode, base.Timestamp, DraftRevision.Std2005);
            set
            {
                m_frameHeader = value;

                if (m_frameHeader is null)
                    return;

                State = m_frameHeader.State as IHeaderFrameParsingState;
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
        /// Gets the IEEE C37.118 protocol version of this <see cref="HeaderFrame"/>.
        /// </summary>
        public byte Version => CommonHeader.Version;

        /// <summary>
        /// Gets the IEEE C37.118 timebase of this <see cref="HeaderFrame"/>.
        /// </summary>
        public uint TimeBase => CommonHeader.Timebase;

        /// <summary>
        /// Gets or sets the <see cref="TimeQualityFlags"/> associated with this <see cref="HeaderFrame"/>.
        /// </summary>
        public TimeQualityFlags TimeQualityFlags
        {
            get => CommonHeader.TimeQualityFlags;
            set => CommonHeader.TimeQualityFlags = value;
        }

        /// <summary>
        /// Gets or sets the <see cref="TimeQualityIndicatorCode"/> associated with this <see cref="HeaderFrame"/>.
        /// </summary>
        public TimeQualityIndicatorCode TimeQualityIndicatorCode
        {
            get => CommonHeader.TimeQualityIndicatorCode;
            set => CommonHeader.TimeQualityIndicatorCode = value;
        }

        /// <summary>
        /// Gets the length of the <see cref="HeaderImage"/>.
        /// </summary>
        protected override int HeaderLength => CommonFrameHeader.FixedLength;

        /// <summary>
        /// Gets the binary header image of the <see cref="HeaderFrame"/> object.
        /// </summary>
        protected override byte[] HeaderImage
        {
            get
            {
                // Make sure to provide proper frame length for use in the common header image
                unchecked
                {
                    CommonHeader.FrameLength = (ushort)BinaryLength;
                }

                return CommonHeader.BinaryImage;
            }
        }

        /// <summary>
        /// <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for the <see cref="HeaderFrame"/> object.
        /// </summary>
        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                CommonHeader.AppendHeaderAttributes(baseAttributes);

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
            // We already parsed the frame header, so we just skip past it...
            return CommonFrameHeader.FixedLength;
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
            // IEEE C37.118 uses CRC-CCITT to calculate checksum for frames
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

            // Serialize data frame
            info.AddValue("frameHeader", m_frameHeader, typeof(CommonFrameHeader));
        }

        #endregion
    }
}
