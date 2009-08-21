//*******************************************************************************************************
//  MetadataRecordConstantFields.cs
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
//  02/21/2007 - Pinal C. Patel
//       Generated original version of code based on DatAWare system specifications by Brian B. Fox, TVA.
//  04/20/2009 - Pinal C. Patel
//       Converted to C#.
//
//*******************************************************************************************************

using System;
using TVA.Parsing;

namespace TVA.Historian.Files
{
    /// <summary>
    /// Defines specific fields for <see cref="MetadataRecord"/>s that are of type <see cref="DataType.Constant"/>.
    /// </summary>
    /// <seealso cref="MetadataRecord"/>
    public class MetadataRecordConstantFields : ISupportBinaryImage
    {
        // **************************************************************************************************
        // *                                        Binary Structure                                        *
        // **************************************************************************************************
        // * # Of Bytes Byte Index Data Type  Property Name                                                 *
        // * ---------- ---------- ---------- --------------------------------------------------------------*
        // * 4          0-3        Single     Value                                                         *
        // * 4          4-7        Int32      DisplayDigits                                                 *
        // **************************************************************************************************

        #region [ Members ]

        // Constants

        /// <summary>
        /// Specifies the number of bytes in the binary image of <see cref="MetadataRecordConstantFields"/>.
        /// </summary>
        public const int ByteCount = 8;

        // Fields
        private float m_value;
        private int m_displayDigits;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataRecordConstantFields"/> class.
        /// </summary>
        internal MetadataRecordConstantFields()
        {
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the constant value of the <see cref="MetadataRecord"/>.
        /// </summary>
        public float Value
        {
            get
            {
                return m_value;
            }
            set
            {
                m_value = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of digits after the decimal point to be displayed for the <see cref="MetadataRecord"/>.
        /// </summary>
        public int DisplayDigits
        {
            get
            {
                return m_displayDigits;
            }
            set
            {
                m_displayDigits = value;
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
        /// Gets the binary representation of <see cref="MetadataRecordConstantFields"/>.
        /// </summary>
        public byte[] BinaryImage
        {
            get
            {
                byte[] image = new byte[ByteCount];

                Array.Copy(EndianOrder.LittleEndian.GetBytes(m_value), 0, image, 0, 4);
                Array.Copy(EndianOrder.LittleEndian.GetBytes(m_displayDigits), 0, image, 4, 4);

                return image;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes <see cref="MetadataRecordConstantFields"/> from the specified <paramref name="binaryImage"/>.
        /// </summary>
        /// <param name="binaryImage">Binary image to be used for initializing <see cref="MetadataRecordConstantFields"/>.</param>
        /// <param name="startIndex">0-based starting index of initialization data in the <paramref name="binaryImage"/>.</param>
        /// <param name="length">Valid number of bytes in <paramref name="binaryImage"/> from <paramref name="startIndex"/>.</param>
        /// <returns>Number of bytes used from the <paramref name="binaryImage"/> for initializing <see cref="MetadataRecordConstantFields"/>.</returns>
        public int Initialize(byte[] binaryImage, int startIndex, int length)
        {
            if (length - startIndex >= ByteCount)
            {
                // Binary image has sufficient data.
                Value = EndianOrder.LittleEndian.ToSingle(binaryImage, startIndex);
                DisplayDigits = EndianOrder.LittleEndian.ToInt32(binaryImage, startIndex + 4);

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
