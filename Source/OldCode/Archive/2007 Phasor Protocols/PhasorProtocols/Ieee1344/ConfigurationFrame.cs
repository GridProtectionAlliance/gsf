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
//  01/14/2005 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.Serialization;
using PCS.Parsing;
using PCS.IO.Checksums;

namespace PCS.PhasorProtocols.Ieee1344
{
    /// <summary>
    /// Represents the IEEE 1344 implementation of a <see cref="IConfigurationFrame"/> that can be sent or received.
    /// </summary>
    [Serializable()]
    public class ConfigurationFrame : ConfigurationFrameBase, ISupportFrameImage<FrameType>
    {
        #region [ Members ]

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
        /// Creates a new <see cref="ConfigurationFrame"/> from the specified parameters.
        /// </summary>
        /// <param name="idCode">The ID code of this <see cref="ConfigurationFrame"/>.</param>
        /// <param name="timestamp">The exact timestamp, in <see cref="Ticks"/>, of the data represented by this <see cref="ConfigurationFrame"/>.</param>
        /// <param name="frameRate">The defined frame rate of this <see cref="ConfigurationFrame"/>.</param>
        /// <remarks>
        /// This constructor is used by a consumer to generate an IEEE 1344 configuration frame.
        /// </remarks>
        public ConfigurationFrame(ulong idCode, Ticks timestamp, short frameRate)
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
            m_idCode = info.GetUInt64("idCode64Bit");
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets reference to the <see cref="ConfigurationCellCollection"/> for this <see cref="ConfigurationFrame"/>.
        /// </summary>
        public new ConfigurationCellCollection Cells
        {
            get
            {
                return base.Cells as ConfigurationCellCollection;
            }
        }

        /// <summary>
        /// Gets or sets the ID code of this <see cref="ConfigurationFrame"/>.
        /// </summary>
        public new ulong IDCode
        {
            get
            {
                return m_idCode;
            }
            set
            {
                m_idCode = value;

                // Base classes constrain maximum value to 65535
                base.IDCode = value > ushort.MaxValue ? ushort.MaxValue : (ushort)value;
            }
        }

        /// <summary>
        /// Gets the timestamp of this frame in NTP format.
        /// </summary>
        public new NtpTimeTag TimeTag
        {
            get
            {
                return CommonHeader.TimeTag;
            }
        }

        /// <summary>
        /// Gets or sets the nominal <see cref="LineFrequency"/> of this <see cref="ConfigurationFrame"/>.
        /// </summary>
        public LineFrequency NominalFrequency
        {
            // Since IEEE 1344 only supports a single PMU there will only be one cell, so we just share nominal frequency with our only child
            // and expose the value at the parent level for convience
            get
            {
                return Cells[0].NominalFrequency;
            }
            set
            {
                Cells[0].NominalFrequency = value;
            }
        }

        /// <summary>
        /// Gets or sets the IEEE 1344 period value.
        /// </summary>
        public short Period
        {
            get
            {
                return (short)((double)NominalFrequency / (double)FrameRate * 100.0D);
            }
            set
            {
                FrameRate = (short)((double)NominalFrequency * 100.0D / (double)value);
            }
        }

        /// <summary>
        /// Gets the identifier that can is used to identify the IEEE 1344 frame.
        /// </summary>
        public FrameType TypeID
        {
            get
            {
                return Ieee1344.FrameType.ConfigurationFrame;
            }
        }

        /// <summary>
        /// Gets or sets current <see cref="CommonFrameHeader"/>.
        /// </summary>
        public CommonFrameHeader CommonHeader
        {
            get
            {
                if (m_frameHeader == null)
                    m_frameHeader = new CommonFrameHeader(Ieee1344.FrameType.ConfigurationFrame, Timestamp);

                return m_frameHeader;
            }
            set
            {
                m_frameHeader = value;
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
        /// Gets the length of the <see cref="HeaderImage"/>.
        /// </summary>
        protected override int HeaderLength
        {
            get
            {
                return CommonHeader.BinaryLength;
            }
        }

        /// <summary>
        /// Gets the binary header image of the <see cref="ConfigurationFrame"/> object.
        /// </summary>
        protected override byte[] HeaderImage
        {
            get
            {
                // Make sure to provide proper frame length in the header image 
                unchecked
                {
                    CommonHeader.FrameLength = (ushort)BinaryLength;
                }

                return CommonHeader.BinaryImage;
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
        /// Gets the binary footer image of the <see cref="ConfigurationFrame"/> object.
        /// </summary>
        protected override byte[] FooterImage
        {
            get
            {
                return EndianOrder.BigEndian.GetBytes(Period);
            }
        }

        /// <summary>
        /// <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for the <see cref="ConfigurationFrame"/> object.
        /// </summary>
        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                if (CommonHeader != null)
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
        /// <param name="binaryImage">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="binaryImage"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="binaryImage"/>.</param>
        /// <returns>The length of the data that was parsed.</returns>
        /// <remarks>
        /// This method is overriden to parse from cumulated frame images.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Invalid binary image detected - check sum did not match.</exception>
        public override int Initialize(byte[] binaryImage, int startIndex, int length)
        {
            // If frame image collector was used, make sure and parse from entire frame image...
            if (m_frameHeader != null)
            {
                // If all configuration frames have been received, we can safely start parsing
                if (m_frameHeader.IsLastFrame)
                {
                    FrameImageCollector frameImages = m_frameHeader.FrameImages;

                    if (frameImages != null)
                    {
                        // Each individual frame will already have had a CRC check, so we implement standard parse to
                        // bypass ChannelBase CRC frame validation on cumulative frame image
                        binaryImage = frameImages.BinaryImage;
                        length = frameImages.BinaryLength;
                        startIndex = 0;

                        // Parse out header, body and footer images
                        startIndex += ParseHeaderImage(binaryImage, startIndex, length);
                        startIndex += ParseBodyImage(binaryImage, startIndex, length - startIndex);
                        startIndex += ParseFooterImage(binaryImage, startIndex, length - startIndex);

                        return startIndex;
                    }
                }

                // There are more configuration frame images coming, keep parser moving by returning total
                // frame length that was already parsed.
                return State.ParsedBinaryLength;
            }

            return base.Initialize(binaryImage, startIndex, length);
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
            // Period assignment calculates FrameRate using NominalFrequency which is shared
            // with first (and only) configuration cell, since cell was added during ParseBodyImage
            // of ChannelFrameBase, performing the following succeeds since parsing the footer
            // follows parsing the body :)
            Period = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex);
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
            // IEEE 1344 uses CRC16 to calculate checksum for frames
            return buffer.Crc16Checksum(offset, length);
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
            info.AddValue("idCode64Bit", m_idCode);
        }

        #endregion
    }
}