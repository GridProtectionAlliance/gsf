//*******************************************************************************************************
//  StateRecordSummary.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel
//      Office: INFO SVCS APP DEV, CHATTANOOGA - MR BK-C
//       Phone: 423/751-3024
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  06/14/2007 - Pinal C. Patel
//       Generated original version of source code.
//  04/20/2009 - Pinal C. Patel
//       Converted to C#.
//
//*******************************************************************************************************

using System;
using TVA.Parsing;

namespace TVA.Historian.Files
{
    /// <summary>
    /// A class with just <see cref="StateRecord.CurrentData"/>. The <see cref="BinaryImage"/> of <see cref="MetadataRecordSummary"/> 
    /// is sent back as a reply to <see cref="DatAWare.Packets.PacketType11"/>.
    /// </summary>
    /// <seealso cref="StateRecord"/>
    /// <seealso cref="DatAWare.Packets.PacketType11"/>
    public class StateRecordSummary : ISupportBinaryImage
    {
        // **************************************************************************************************
        // *                                        Binary Structure                                        *
        // **************************************************************************************************
        // * # Of Bytes Byte Index Data Type  Property Name                                                 *
        // * ---------- ---------- ---------- --------------------------------------------------------------*
        // * 4          0-3        Int32      DatAWareId                                                    *
        // * 16         4-19       Byte(16)   CurrentData                                                   *
        // **************************************************************************************************

        #region [ Members ]

        // Constants

        /// <summary>
        /// Specifies the number of bytes in the binary image of <see cref="StateRecordSummary"/>.
        /// </summary>
        public const int ByteCount = 20;

        // Fields
        private int m_datawareId;
        private StateRecordData m_currentData;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="StateRecordSummary"/> class.
        /// </summary>
        /// <param name="record">A <see cref="StateRecord"/> object.</param>
        public StateRecordSummary(StateRecord record)
        {
            DatAWareId = record.DatAWareId;
            CurrentData = record.CurrentData;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateRecordSummary"/> class.
        /// </summary>
        /// <param name="binaryImage">Binary image to be used for initializing <see cref="StateRecordSummary"/>.</param>
        /// <param name="startIndex">0-based starting index of initialization data in the <paramref name="binaryImage"/>.</param>
        /// <param name="length">Valid number of bytes in <paramref name="binaryImage"/> from <paramref name="startIndex"/>.</param>
        public StateRecordSummary(byte[] binaryImage, int startIndex, int length)
        {
            Initialize(binaryImage, startIndex, length);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Same as <see cref="StateRecord.DatAWareId"/>.
        /// </summary>
        /// <exception cref="ArgumentException">Value being set is not positive or -1.</exception>
        public int DatAWareId
        {
            get
            {
                return m_datawareId;
            }
            private set
            {
                if (value < 1 && value != -1)
                    throw new ArgumentException("Value must be positive or -1.");

                m_datawareId = value;
            }
        }

        /// <summary>
        /// Same as <see cref="StateRecord.CurrentData"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">Value being set is null.</exception>
        public StateRecordData CurrentData
        {
            get
            {
                return m_currentData;
            }
            private set
            {
                if (value == null)
                    throw new ArgumentNullException();

                m_currentData = value;
            }
        }

        /// <summary>
        /// Gets the length of the <see cref="BinaryImage"/>.
        /// </summary>
        public int BinaryLength
        {
            get
            {
                return ByteCount;
            }
        }

        /// <summary>
        /// Gets the binary representation of <see cref="StateRecordSummary"/>.
        /// </summary>
        public byte[] BinaryImage
        {
            get
            {
                byte[] image = new byte[ByteCount];

                Array.Copy(EndianOrder.LittleEndian.GetBytes(m_datawareId), 0, image, 0, 4);
                Array.Copy(m_currentData.BinaryImage, 0, image, 4, StateRecordData.ByteCount);

                return image;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes <see cref="StateRecordSummary"/> from the specified <paramref name="binaryImage"/>.
        /// </summary>
        /// <param name="binaryImage">Binary image to be used for initializing <see cref="StateRecordSummary"/>.</param>
        /// <param name="startIndex">0-based starting index of initialization data in the <paramref name="binaryImage"/>.</param>
        /// <param name="length">Valid number of bytes in <paramref name="binaryImage"/> from <paramref name="startIndex"/>.</param>
        /// <returns>Number of bytes used from the <paramref name="binaryImage"/> for initializing <see cref="StateRecordSummary"/>.</returns>
        public int Initialize(byte[] binaryImage, int startIndex, int length)
        {
            if (length - startIndex >= ByteCount)
            {
                // Binary image has sufficient data.
                DatAWareId = EndianOrder.LittleEndian.ToInt32(binaryImage, startIndex);
                CurrentData = new StateRecordData(DatAWareId, binaryImage, startIndex + 4, StateRecordData.ByteCount);

                return ByteCount;
            }
            else
            {
                // Binary image does not have sufficient data.
                return 0;
            }
        }

        #endregion
    }
}
