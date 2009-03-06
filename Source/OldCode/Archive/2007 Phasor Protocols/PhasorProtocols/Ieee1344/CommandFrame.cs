//*******************************************************************************************************
//  CommandFrame.cs
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
    /// Represents the IEEE 1344 implementation of a <see cref="ICommandFrame"/> that can be sent or received.
    /// </summary>
    /// <remarks>
    /// IEEE 1344 command frames are designed only to be sent to a device, not received from a device. As a result
    /// this frame does not implement <see cref="ISupportFrameImage{T}"/> for automated frame parsing.
    /// </remarks>
    [Serializable()]
    public class CommandFrame : CommandFrameBase
    {
        #region [ Members ]

        /// <summary>
        /// Total frame length of a IEEE 1344 <see cref="CommandFrame"/>.
        /// </summary>
        public const ushort FrameLength = 16;

        // Fields
        private ulong m_idCode;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="DataFrame"/> from the given <paramref name="binaryImage"/>.
        /// </summary>
        /// <param name="binaryImage">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="binaryImage"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="binaryImage"/>.</param>
        /// <remarks>
        /// This constructor is used by a consumer to parse a received IEEE 1344 data frame. Typically
        /// command frames are sent to a device. This constructor would used if this code was being used
        /// inside of a phasor measurement device.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> must be at least 16.</exception>
        public CommandFrame(byte[] binaryImage, int startIndex, int length)
            : base(new CommandCellCollection(0), DeviceCommand.ReservedBits)
        {
            if (length < FrameLength)
                throw new ArgumentOutOfRangeException("length");

            Initialize(binaryImage, startIndex, length);
        }
 
        /// <summary>
        /// Creates a new <see cref="CommandFrame"/> from specified parameters.
        /// </summary>
        /// <param name="idCode">The ID code of this <see cref="CommandFrame"/>.</param>
        /// <param name="command">The <see cref="DeviceCommand"/> for this <see cref="CommandFrame"/>.</param>
        /// <remarks>
        /// This constructor is used by a consumer to generate an IEEE 1344 command frame.
        /// </remarks>
        public CommandFrame(ulong idCode, DeviceCommand command)
            : base(new CommandCellCollection(0), command)
        {
            m_idCode = idCode;
        }

        /// <summary>
        /// Creates a new <see cref="CommandFrame"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected CommandFrame(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Deserialize command frame
            m_idCode = info.GetUInt64("idCode64Bit");
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets reference to the <see cref="CommandCellCollection"/> for this <see cref="CommandFrame"/>.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)] // IEEE 1344 command frame doesn't support extended data - so we hide cell collection property...
        public override CommandCellCollection Cells
        {            
            get
            {
                return base.Cells;
            }
        }

        /// <summary>
        /// Gets or sets extended binary image data for this <see cref="CommandFrame"/>.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)] // IEEE 1344 command frame doesn't support extended data - so we hide extended data property...
        public override byte[] ExtendedData
        {            
            get
            {
                return base.ExtendedData;
            }
            set
            {
                base.ExtendedData = value;
            }
        }

        /// <summary>
        /// Gets or sets the ID code of this <see cref="CommandFrame"/>.
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
                base.IDCode = m_idCode > ushort.MaxValue ? ushort.MaxValue : (ushort)value;
            }
        }

        /// <summary>
        /// Gets NTP based time representation of the ticks of this <see cref="CommandFrame"/>.
        /// </summary>
        public new NtpTimeTag TimeTag
        {
            get
            {
                return new NtpTimeTag(Timestamp);
            }
        }

        /// <summary>
        /// Gets the length of the <see cref="HeaderImage"/>.
        /// </summary>
        protected override int HeaderLength
        {
            get
            {
                return 12;
            }
        }

        /// <summary>
        /// Gets the binary header image of the <see cref="CommandFrame"/> object.
        /// </summary>
        protected override byte[] HeaderImage
        {
            get
            {
                byte[] buffer = new byte[HeaderLength];

                EndianOrder.BigEndian.CopyBytes((uint)TimeTag.Value, buffer, 0);
                EndianOrder.BigEndian.CopyBytes(m_idCode, buffer, 4);

                return buffer;
            }
        }

        /// <summary>
        /// <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for the <see cref="CommandFrame"/> object.
        /// </summary>
        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("64-Bit ID Code", IDCode.ToString());

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
            Timestamp = (new NtpTimeTag((double)EndianOrder.BigEndian.ToUInt32(binaryImage, startIndex))).ToDateTime().Ticks;
            m_idCode = EndianOrder.BigEndian.ToUInt64(binaryImage, startIndex + 4);
            return 12;
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

            // Serialize command frame
            info.AddValue("idCode64Bit", m_idCode);
        }

        #endregion       
    }
}