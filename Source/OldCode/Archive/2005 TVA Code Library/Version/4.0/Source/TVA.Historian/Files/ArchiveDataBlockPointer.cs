//*******************************************************************************************************
//  ArchiveDataBlockPointer.cs
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
//  02/18/2007 - Pinal C. Patel
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
    /// Represents a pointer to an <see cref="ArchiveDataBlock"/>.
    /// </summary>
    /// <seealso cref="ArchiveFile"/>
    /// <seealso cref="ArchiveDataBlock"/>
    public class ArchiveDataBlockPointer : IComparable, ISupportBinaryImage
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Specifies the number of bytes in the binary image of <see cref="ArchiveDataBlockPointer"/>.
        /// </summary>
        public const int ByteCount = 12;

        // Fields
        private int m_index;
        private int m_datawareId;
        private TimeTag m_startTime;
        private ArchiveFile m_parent;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="ArchiveDataBlockPointer"/> class.
        /// </summary>
        /// <param name="parent">An <see cref="ArchiveFile"/> object.</param>
        /// <param name="index">0-based index of the <see cref="ArchiveDataBlockPointer"/>.</param>
        internal ArchiveDataBlockPointer(ArchiveFile parent, int index)
        {
            m_parent = parent;
            m_index = index;
            Reset();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArchiveDataBlockPointer"/> class.
        /// </summary>
        /// <param name="parent">An <see cref="ArchiveFile"/> object.</param>
        /// <param name="index">0-based index of the <see cref="ArchiveDataBlockPointer"/>.</param>
        /// <param name="binaryImage">Binary image to be used for initializing <see cref="ArchiveDataBlockPointer"/>.</param>
        /// <param name="startIndex">0-based starting index of initialization data in the <paramref name="binaryImage"/>.</param>
        /// <param name="length">Valid number of bytes in <paramref name="binaryImage"/> from <paramref name="startIndex"/>.</param>
        internal ArchiveDataBlockPointer(ArchiveFile parent, int index, byte[] binaryImage, int startIndex, int length)
            : this(parent, index)
        {
            Initialize(binaryImage, startIndex, length);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the DatAWare identifier of <see cref="ArchiveDataBlockPointer"/>.
        /// </summary>
        /// <exception cref="ArgumentException">Value being set is not positive or -1.</exception>
        public int DatAWareId
        {
            get
            {
                return m_datawareId;
            }
            set
            {
                if (value < 1 && value != -1)
                    throw new ArgumentException("Value must be positive or -1.");

                m_datawareId = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="TimeTag"/> of fist <see cref="ArchiveData"/> point in the <see cref="DataBlock"/>.
        /// </summary>
        /// <exception cref="ArgumentException">Value being set is not between 01/01/1995 and 01/19/2063.</exception>
        public TimeTag StartTime
        {
            get
            {
                return m_startTime;
            }
            set
            {
                if (value < TimeTag.MinValue || value > TimeTag.MaxValue)
                    throw new ArgumentException("Value must between 01/01/1995 and 01/19/2063.");

                m_startTime = value;
            }
        }

        /// <summary>
        /// Gets the 0-based index of the <see cref="ArchiveDataBlockPointer"/>.
        /// </summary>
        public int Index
        {
            get
            {
                return m_index;
            }
        }

        /// <summary>
        /// Gets the <see cref="ArchiveDataBlock"/> corresponding to the <see cref="ArchiveDataBlockPointer"/>.
        /// </summary>
        public ArchiveDataBlock DataBlock
        {
            get
            {
                return new ArchiveDataBlock(m_parent, m_index, m_datawareId);
            }
        }

        /// <summary>
        /// Gets a boolean value that indicates whether the <see cref="DataBlock"/> has been allocated to contain <see cref="ArchiveData"/>.
        /// </summary>
        public bool IsAllocated
        {
            get
            {
                return m_datawareId != -1 && m_startTime.CompareTo(TimeTag.MinValue) != 0;
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
        /// Gets the binary representation of <see cref="ArchiveDataBlockPointer"/>.
        /// </summary>
        public byte[] BinaryImage
        {
            get
            {
                byte[] image = new byte[ByteCount];

                Array.Copy(EndianOrder.LittleEndian.GetBytes(m_datawareId), 0, image, 0, 4);
                Array.Copy(EndianOrder.LittleEndian.GetBytes(m_startTime.Value), 0, image, 4, 8);

                return image;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Deallocates the <see cref="DataBlock"/> to store new <see cref="ArchiveData"/>.
        /// </summary>
        public void Reset()
        {
            m_datawareId = -1;
            m_startTime = TimeTag.MinValue;
        }

        /// <summary>
        /// Initializes <see cref="ArchiveDataBlockPointer"/> from the specified <paramref name="binaryImage"/>.
        /// </summary>
        /// <param name="binaryImage">Binary image to be used for initializing <see cref="ArchiveDataBlockPointer"/>.</param>
        /// <param name="startIndex">0-based starting index of initialization data in the <paramref name="binaryImage"/>.</param>
        /// <param name="length">Valid number of bytes in <paramref name="binaryImage"/> from <paramref name="startIndex"/>.</param>
        /// <returns>Number of bytes used from the <paramref name="binaryImage"/> for initializing <see cref="ArchiveDataBlockPointer"/>.</returns>
        public int Initialize(byte[] binaryImage, int startIndex, int length)
        {
            if (length - startIndex >= ByteCount)
            {
                // Binary image has sufficient data.
                DatAWareId = EndianOrder.LittleEndian.ToInt32(binaryImage, startIndex);
                StartTime = new TimeTag(EndianOrder.LittleEndian.ToDouble(binaryImage, startIndex + 4));

                return ByteCount;
            }
            else
            {
                // Binary image does not have sufficient data.
                return 0;
            }
        }

        /// <summary>
        /// Compares the current <see cref="ArchiveDataBlockPointer"/> object to <paramref name="obj"/>.
        /// </summary>
        /// <param name="obj">Object against which the current <see cref="ArchiveDataBlockPointer"/> object is to be compared.</param>
        /// <returns>
        /// Negative value if the current <see cref="ArchiveDataBlockPointer"/> object is less than <paramref name="obj"/>, 
        /// Zero if the current <see cref="ArchiveDataBlockPointer"/> object is equal to <paramref name="obj"/>, 
        /// Positive value if the current <see cref="ArchiveDataBlockPointer"/> object is greater than <paramref name="obj"/>.
        /// </returns>
        public virtual int CompareTo(object obj)
        {
            ArchiveDataBlockPointer other = obj as ArchiveDataBlockPointer;
            if (other == null)
            {
                return 1;
            }
            else
            {
                int result = m_datawareId.CompareTo(other.DatAWareId);
                if (result != 0)
                    return result;
                else
                    return m_startTime.CompareTo(other.StartTime);
            }
        }

        /// <summary>
        /// Determines whether the current <see cref="ArchiveDataBlockPointer"/> object is equal to <paramref name="obj"/>.
        /// </summary>
        /// <param name="obj">Object against which the current <see cref="ArchiveDataBlockPointer"/> object is to be compared for equality.</param>
        /// <returns>true if the current <see cref="ArchiveDataBlockPointer"/> object is equal to <paramref name="obj"/>; otherwise false.</returns>
        public override bool Equals(object obj)
        {
            return (CompareTo(obj) == 0);
        }

        /// <summary>
        /// Returns the text representation of <see cref="ArchiveDataBlockPointer"/> object.
        /// </summary>
        /// <returns>A <see cref="string"/> value.</returns>
        public override string ToString()
        {
            return string.Format("Id: {0}; Start time: {1}", m_datawareId, m_startTime);
        }

        /// <summary>
        /// Returns the hash code for the current <see cref="ArchiveDataBlock"/> object.
        /// </summary>
        /// <returns>A 32-bit signed integer value.</returns>
        public override int GetHashCode()
        {
            return m_datawareId.GetHashCode();
        }

        #endregion
    }
}