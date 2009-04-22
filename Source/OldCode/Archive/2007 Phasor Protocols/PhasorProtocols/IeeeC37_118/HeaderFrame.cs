//*******************************************************************************************************
//  HeaderFrame.cs
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
using PCS.IO.Checksums;
using PCS.Parsing;

namespace PCS.PhasorProtocols.IeeeC37_118
{
    /// <summary>
    /// Represents the IEEE C37.118 implementation of a <see cref="IHeaderFrame"/> that can be sent or received.
    /// </summary>
    [Serializable()]
    public class HeaderFrame : HeaderFrameBase, ISupportFrameImage<FrameType>
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
        /// Gets or sets exact timestamp, in ticks, of the data represented by this <see cref="HeaderFrame"/>.
        /// </summary>
        /// <remarks>
        /// The value of this property represents the number of 100-nanosecond intervals that have elapsed since 12:00:00 midnight, January 1, 0001.
        /// </remarks>
        public override Ticks Timestamp
        {
            get
            {
                return CommonHeader.Timestamp;
            }
            set
            {
                // Keep timestamp updates synchrnonized...
                CommonHeader.Timestamp = value;
                base.Timestamp = value;
            }
        }

        /// <summary>
        /// Gets the identifier that is used to identify the IEEE C37.118 frame.
        /// </summary>
        public FrameType TypeID
        {
            get
            {
                return IeeeC37_118.FrameType.HeaderFrame;
            }
        }

        /// <summary>
        /// Gets or sets current <see cref="CommonFrameHeader"/>.
        /// </summary>
        public CommonFrameHeader CommonHeader
        {
            get
            {
                // Make sure frame header exists - using base class timestamp to
                // prevent recursion (m_frameHeader doesn't exist yet)
                if (m_frameHeader == null)
                    m_frameHeader = new CommonFrameHeader(TypeID, base.Timestamp);

                return m_frameHeader;
            }
            set
            {
                m_frameHeader = value;

                if (m_frameHeader != null)
                {
                    State = m_frameHeader.State as IHeaderFrameParsingState;
                    base.Timestamp = m_frameHeader.Timestamp;
                }
            }
        }

        // This interface implementation satisfies ISupportFrameImage<FrameType>.CommonHeader
        ICommonHeader<FrameType> ISupportFrameImage<FrameType>.CommonHeader
        {
            get
            {
                return CommonHeader;
            }
            set
            {
                CommonHeader = value as CommonFrameHeader;
            }
        }

        /// <summary>
        /// Gets the IEEE C37.118 protocol version of this <see cref="HeaderFrame"/>.
        /// </summary>
        public byte Version
        {
            get
            {
                return CommonHeader.Version;
            }
        }

        /// <summary>
        /// Gets the IEEE C37.118 timebase of this <see cref="HeaderFrame"/>.
        /// </summary>
        public uint TimeBase
        {
            get
            {
                return CommonHeader.Timebase;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="TimeQualityFlags"/> associated with this <see cref="HeaderFrame"/>.
        /// </summary>
        public TimeQualityFlags TimeQualityFlags
        {
            get
            {
                return CommonHeader.TimeQualityFlags;
            }
            set
            {
                CommonHeader.TimeQualityFlags = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="TimeQualityIndicatorCode"/> associated with this <see cref="HeaderFrame"/>.
        /// </summary>
        public TimeQualityIndicatorCode TimeQualityIndicatorCode
        {
            get
            {
                return CommonHeader.TimeQualityIndicatorCode;
            }
            set
            {
                CommonHeader.TimeQualityIndicatorCode = value;
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
        /// <param name="binaryImage">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="binaryImage"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="binaryImage"/>.</param>
        /// <returns>The length of the data that was parsed.</returns>
        protected override int ParseHeaderImage(byte[] binaryImage, int startIndex, int length)
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
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        #endregion
    }
}
