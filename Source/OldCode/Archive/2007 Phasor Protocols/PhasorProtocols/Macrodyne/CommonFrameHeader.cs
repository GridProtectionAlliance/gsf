//*******************************************************************************************************
//  CommonFrameHeader.cs
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
//  04/30/2009 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using TVA.Parsing;

namespace PhasorProtocols.Macrodyne
{
    /// <summary>
    /// Represents the common header for all Macrodyne frames of data.
    /// </summary>
    [Serializable()]
    public class CommonFrameHeader : CommonHeaderBase<int>, ISerializable
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Total fixed length of <see cref="CommonFrameHeader"/>.
        /// </summary>
        public const ushort FixedLength = 2;

        // Fields
        private StatusFlags m_statusFlags;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="CommonFrameHeader"/>.
        /// </summary>
        public CommonFrameHeader()
        {
        }

        /// <summary>
        /// Creates a new <see cref="CommonFrameHeader"/> from given <paramref name="binaryImage"/>.
        /// </summary>
        /// <param name="binaryImage">Buffer that contains data to parse.</param>
        /// <param name="startIndex">Start index into buffer where valid data begins.</param>
        public CommonFrameHeader(byte[] binaryImage, int startIndex)
        {
            // Validate Macrodyne data image
            if (binaryImage[startIndex] != PhasorProtocols.Common.SyncByte)
                throw new InvalidOperationException("Bad data stream, expected sync byte 0xAA as first byte in Macrodyne ON-LINE data frame, got 0x" + binaryImage[startIndex].ToString("X").PadLeft(2, '0'));

            // Parse relevant common header values
            m_statusFlags = (StatusFlags)binaryImage[startIndex + 1];
        }

        /// <summary>
        /// Creates a new <see cref="CommonFrameHeader"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected CommonFrameHeader(SerializationInfo info, StreamingContext context)
        {
            // Deserialize common frame header
            m_statusFlags = (StatusFlags)info.GetValue("statusFlags", typeof(StatusFlags));
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="Macrodyne.StatusFlags"/> of this <see cref="CommonFrameHeader"/>.
        /// </summary>
        public StatusFlags StatusFlags
        {
            get
            {
                return m_statusFlags;
            }
            set
            {
                m_statusFlags = value;
            }
        }

        /// <summary>
        /// Gets the binary image of the common header portion of this <see cref="CommonFrameHeader"/>.
        /// </summary>
        public byte[] BinaryImage
        {
            get
            {
                byte[] buffer = new byte[FixedLength];

                buffer[0] = PhasorProtocols.Common.SyncByte;
                buffer[1] = (byte)m_statusFlags;

                return buffer;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Serialize unique common frame header values
            info.AddValue("statusFlags", m_statusFlags, typeof(StatusFlags));
        }

        #endregion
    }
}