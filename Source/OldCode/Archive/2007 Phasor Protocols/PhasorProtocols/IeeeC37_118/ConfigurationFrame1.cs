//*******************************************************************************************************
//  ConfigurationFrame.cs
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
    /// Represents the IEEE C37.118 implementation of a <see cref="IConfigurationFrame"/>, type 1, that can be sent or received.
    /// </summary>
    [Serializable()]
    public class ConfigurationFrame1 : ConfigurationFrameBase, ISupportFrameImage<FrameType>
    {
        #region [ Members ]

        // Constants
        private const ushort FixedHeaderLength = CommonFrameHeader.FixedLength + 6;

        // Fields
        private CommonFrameHeader m_frameHeader;
        private uint m_timebase;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ConfigurationFrame1"/>.
        /// </summary>
        /// <remarks>
        /// This constructor is used by <see cref="FrameImageParserBase{TTypeIdentifier,TOutputType}"/> to parse an IEEE C37.118 configuration frame.
        /// </remarks>
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected ConfigurationFrame1()
            : base(0, new ConfigurationCellCollection(), 0, 0)
        {
        }

        /// <summary>
        /// Creates a new <see cref="ConfigurationFrame1"/> from specified parameters.
        /// </summary>
        /// <param name="timebase">Timebase to use for fraction second resolution.</param>
        /// <param name="idCode">The ID code of this <see cref="ConfigurationFrame1"/>.</param>
        /// <param name="timestamp">The exact timestamp, in <see cref="Ticks"/>, of the data represented by this <see cref="ConfigurationFrame1"/>.</param>
        /// <param name="frameRate">The defined frame rate of this <see cref="ConfigurationFrame1"/>.</param>
        /// <remarks>
        /// This constructor is used by a consumer to generate an IEEE C37.118 configuration frame.
        /// </remarks>
        public ConfigurationFrame1(uint timebase, ushort idCode, Ticks timestamp, ushort frameRate)
            : base(idCode, new ConfigurationCellCollection(), timestamp, frameRate)
        {
            this.Timebase = timebase;
        }

        /// <summary>
        /// Creates a new <see cref="ConfigurationFrame1"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected ConfigurationFrame1(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Deserialize configuration frame
            m_frameHeader = (CommonFrameHeader)info.GetValue("frameHeader", typeof(CommonFrameHeader));
            m_timebase = info.GetUInt32("timebase");
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets reference to the <see cref="ConfigurationCellCollection"/> for this <see cref="ConfigurationFrame1"/>.
        /// </summary>
        public new ConfigurationCellCollection Cells
        {
            get
            {
                return base.Cells as ConfigurationCellCollection;
            }
        }

        /// <summary>
        /// Gets the <see cref="IeeeC37_118.DraftRevision"/> of this <see cref="ConfigurationFrame1"/>.
        /// </summary>
        public virtual DraftRevision DraftRevision
        {
            get
            {
                return IeeeC37_118.DraftRevision.Draft7;
            }
        }

        /// <summary>
        /// Gets the <see cref="FrameType"/> of this <see cref="ConfigurationFrame1"/>.
        /// </summary>
        public virtual FrameType TypeID
        {
            get
            {
                return IeeeC37_118.FrameType.ConfigurationFrame1;
            }
        }

        /// <summary>
        /// Gets or sets exact timestamp, in ticks, of the data represented by this <see cref="ConfigurationFrame1"/>.
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
        /// Gets or sets current <see cref="CommonFrameHeader"/>.
        /// </summary>
        public CommonFrameHeader CommonHeader
        {
            get
            {
                // Make sure frame header exists
                if (m_frameHeader == null)
                    m_frameHeader = new CommonFrameHeader(TypeID, base.Timestamp);

                return m_frameHeader;
            }
            set
            {
                m_frameHeader = value;

                if (m_frameHeader != null)
                {
                    State = m_frameHeader.State as IConfigurationFrameParsingState;
                    base.IDCode = m_frameHeader.IDCode;
                    base.Timestamp = m_frameHeader.Timestamp;
                    m_timebase = m_frameHeader.Timebase;
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
        /// Gets or sets the IEEE C37.118 protocol version of this <see cref="ConfigurationFrame1"/>.
        /// </summary>
        public byte Version
        {
            get
            {
                return CommonHeader.Version;
            }
            set
            {
                CommonHeader.Version = value;
            }
        }

        /// <summary>
        /// Gets or sets the IEEE C37.118 resolution of fractional time stamps of this <see cref="ConfigurationFrame1"/>.
        /// </summary>
        public uint Timebase
        {
            get
            {
                return m_timebase;
            }
            set
            {
                m_timebase = (value & ~Common.TimeQualityFlagsMask);
                CommonHeader.Timebase = (UInt24)m_timebase;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="TimeQualityFlags"/> of this <see cref="ConfigurationFrame1"/>.
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
        /// Gets or sets the <see cref="TimeQualityIndicatorCode"/> of this <see cref="ConfigurationFrame1"/>.
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
        /// Gets the length of the <see cref="HeaderImage"/>.
        /// </summary>
        protected override int HeaderLength
        {
            get
            {
                return FixedHeaderLength;
            }
        }

        /// <summary>
        /// Gets the binary header image of the <see cref="ConfigurationFrame1"/> object.
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

                byte[] buffer = new byte[FixedHeaderLength];
                int index = 0;

                CommonHeader.BinaryImage.CopyImage(buffer, ref index, CommonFrameHeader.FixedLength);
                EndianOrder.BigEndian.CopyBytes(m_timebase, buffer, index);
                EndianOrder.BigEndian.CopyBytes((ushort)Cells.Count, buffer, index + 4);

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
                return 2;
            }
        }

        /// <summary>
        /// Gets the binary footer image of the <see cref="ConfigurationFrame1"/> object.
        /// </summary>
        protected override byte[] FooterImage
        {
            get
            {
                byte[] buffer = new byte[2];

                EndianOrder.BigEndian.CopyBytes(FrameRate, buffer, 0);

                return buffer;
            }
        }

        /// <summary>
        /// <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for the <see cref="DataFrame"/> object.
        /// </summary>
        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                CommonHeader.AppendHeaderAttributes(baseAttributes);
                baseAttributes.Add("Draft Revision", (int)DraftRevision + ": " + DraftRevision);

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
            // Skip past header that was already parsed...
            startIndex += CommonFrameHeader.FixedLength;

            m_timebase = EndianOrder.BigEndian.ToUInt32(binaryImage, startIndex) & ~Common.TimeQualityFlagsMask;
            State.CellCount = EndianOrder.BigEndian.ToUInt16(binaryImage, startIndex + 4);

            return FixedHeaderLength;
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
            FrameRate = EndianOrder.BigEndian.ToUInt16(binaryImage, startIndex);
            return 2;
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
        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            base.GetObjectData(info, context);

            // Serialize configuration frame
            info.AddValue("frameHeader", m_frameHeader, typeof(CommonFrameHeader));
            info.AddValue("timebase", m_timebase);
        }

        #endregion
    }
}