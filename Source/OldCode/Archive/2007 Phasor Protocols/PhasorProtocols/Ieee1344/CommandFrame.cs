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
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ComponentModel;
using PCS.Parsing;
using PCS.IO.Checksums;

namespace PCS.PhasorProtocols.Ieee1344
{
    /// <summary>
    /// Represents the IEEE 1344 implementation of a <see cref="ICommandFrame"/> that can be sent or received.
    /// </summary>
    [Serializable()]
    public class CommandFrame : CommandFrameBase, ISupportFrameImage<FrameType>
    {
        #region [ Members ]

        // Constants
        public const ushort FrameLength = 16;

        // Fields
        private ulong m_idCode;

        #endregion

        #region [ Constructors ]

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

        /// <summary>
        /// Creates a new <see cref="CommandFrame"/> from the specified parameters.
        /// </summary>
        /// <param name="idCode">The ID code of this <see cref="CommandFrame"/>.</param>
        /// <param name="command">The <see cref="DeviceCommand"/> for this <see cref="CommandFrame"/>.</param>
        public CommandFrame(ulong idCode, DeviceCommand command)
            : base(new CommandCellCollection(0), command)
        {
            m_idCode = idCode;
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
                base.IDCode = value % int.MaxValue;
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
            Timestamp = (new NtpTimeTag(EndianOrder.BigEndian.ToUInt32(binaryImage, startIndex))).ToDateTime().Ticks;
            m_idCode = EndianOrder.BigEndian.ToUInt64(binaryImage, startIndex + 4);
            return HeaderLength;
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
        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            base.GetObjectData(info, context);

            // Serialize command frame
            info.AddValue("idCode64Bit", m_idCode);
        }

        #endregion       
    }
}