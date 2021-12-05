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
//  02/08/2010 - James R. Carroll
//       Generated original version of source code.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Runtime.Serialization;
using GSF.IO.Checksums;
using GSF.Parsing;

namespace GSF.PhasorProtocols.Macrodyne
{
    /// <summary>
    /// Represents the Macrodyne implementation of a <see cref="IHeaderFrame"/> that can be sent or received.
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
        /// This constructor is used by a consumer or by <see cref="FrameImageParserBase{TTypeIdentifier,TOutputType}"/> to generate or parse a Macrodyne header frame.
        /// </remarks>
        public HeaderFrame()
            : base(new HeaderCellCollection(10))
        {
        }

        /// <summary>
        /// Creates a new <see cref="HeaderFrame"/>.
        /// </summary>
        /// <param name="headerData"><see cref="string"/> based data to include in this <see cref="HeaderFrame"/>.</param>
        /// <remarks>
        /// This constructor is used by a consumer to generate a Macrodyne header frame.
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
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the identifier that is used to identify the Macrodyne frame.
        /// </summary>
        public FrameType TypeID => Macrodyne.FrameType.HeaderFrame;

        /// <summary>
        /// Gets or sets current <see cref="CommonFrameHeader"/>.
        /// </summary>
        public CommonFrameHeader CommonHeader
        {
            get =>
m_frameHeader ??= new CommonFrameHeader();
            set
            {
                m_frameHeader = value;

                if (m_frameHeader is null)
                    return;

                State = m_frameHeader.State as IHeaderFrameParsingState;
            }
        }

        // This interface implementation satisfies ISupportFrameImage<FrameType>.CommonHeader
        ICommonHeader<FrameType> ISupportFrameImage<FrameType>.CommonHeader
        {
            get => CommonHeader;
            set => CommonHeader = value as CommonFrameHeader;
        }

        /// <summary>
        /// Gets the length of the <see cref="HeaderFrame"/>.
        /// </summary>
        /// <remarks>
        /// This property is overridden so the length can be extended to include a 1-byte checksum.
        /// </remarks>
        public override int BinaryLength
        {
            get
            {
                // We override normal binary length so we can extend length to include checksum.
                // Also, if frame length was parsed from stream header - we use that length
                // instead of the calculated length...
                if (ParsedBinaryLength > 0)
                    return ParsedBinaryLength;

                // Subtract one byte for Macrodyne 1-byte CRC
                return base.BinaryLength - 1;
            }
        }

        /// <summary>
        /// Gets the length of the <see cref="HeaderImage"/>.
        /// </summary>
        protected override int HeaderLength => CommonFrameHeader.FixedLength;

        /// <summary>
        /// Gets the binary header image of the <see cref="DataFrame"/> object.
        /// </summary>
        protected override byte[] HeaderImage => CommonHeader.BinaryImage;

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
        /// Determines if checksum in the <paramref name="buffer"/> is valid.
        /// </summary>
        /// <param name="buffer">Buffer image to validate.</param>
        /// <param name="startIndex">Start index into <paramref name="buffer"/> to perform checksum.</param>
        /// <returns>Flag that determines if checksum over <paramref name="buffer"/> is valid.</returns>
        /// <remarks>
        /// Default implementation expects 2-byte big-endian ordered checksum. So we override method since checksum
        /// in Macrodyne is a single byte.
        /// </remarks>
        protected override bool ChecksumIsValid(byte[] buffer, int startIndex)
        {
            int sumLength = BinaryLength - 2;
            return buffer[startIndex + BinaryLength - 1] == CalculateChecksum(buffer, startIndex + 1, sumLength);
        }

        /// <summary>
        /// Appends checksum onto <paramref name="buffer"/> starting at <paramref name="startIndex"/>.
        /// </summary>
        /// <param name="buffer">Buffer image on which to append checksum.</param>
        /// <param name="startIndex">Index into <paramref name="buffer"/> where checksum should be appended.</param>
        /// <remarks>
        /// Default implementation encodes checksum in big-endian order and expects buffer size large enough to accommodate
        /// 2-byte checksum representation. We override this method since checksum in Macrodyne is a single byte.
        /// </remarks>
        protected override void AppendChecksum(byte[] buffer, int startIndex)
        {
            buffer[startIndex] = (byte)CalculateChecksum(buffer, 1, startIndex);
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
            // Macrodyne uses 8-bit XOR checksum for frames
            return buffer.Xor8Checksum(offset, length);
        }

        #endregion
    }
}
