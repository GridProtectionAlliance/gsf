//*******************************************************************************************************
//  ArchiveData.cs
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
//  02/23/2007 - Pinal C. Patel
//       Generated original version of source code.
//  04/20/2009 - Pinal C. Patel
//       Converted to C#.
//
//*******************************************************************************************************

using System;

namespace TVA.Historian.Files
{
    /// <summary>
    /// Represents time series data stored in <see cref="ArchiveFile"/>.
    /// </summary>
    /// <seealso cref="ArchiveFile"/>
    /// <seealso cref="ArchiveFileAllocationTable"/>
    /// <seealso cref="ArchiveDataBlock"/>
    /// <seealso cref="ArchiveDataBlockPointer"/>
    public class ArchiveData : IDataPoint, IComparable
    {
        // **************************************************************************************************
        // *                                        Binary Structure                                        *
        // **************************************************************************************************
        // * # Of Bytes Byte Index Data Type  Property Name                                                 *
        // * ---------- ---------- ---------- --------------------------------------------------------------*
        // * 4          0-3        Int32      Time                                                          *
        // * 2          4-5        Int16      Flags (Quality & Milliseconds)                                *
        // * 4          6-9        Single     Value                                                         *
        // **************************************************************************************************

        #region [ Members ]

        // Constants

        /// <summary>
        /// Specifies the number of bytes in the binary image of <see cref="ArchiveData"/>.
        /// </summary>
        public const int ByteCount = 10;

        /// <summary>
        /// Specifies the bit-mask for <see cref="Quality"/> stored in <see cref="Flags"/>.
        /// </summary>
        [CLSCompliant(false)]
        protected const Bits QualityMask = Bits.Bit00 | Bits.Bit01 | Bits.Bit02 | Bits.Bit03 | Bits.Bit04;

        /// <summary>
        /// Specifies the bit-mask for <see cref="TimeTag"/> milliseconds stored in <see cref="Flags"/>.
        /// </summary>
        [CLSCompliant(false)]
        protected const Bits MillisecondMask = Bits.Bit05 | Bits.Bit06 | Bits.Bit07 | Bits.Bit08 | Bits.Bit09 | Bits.Bit10 | Bits.Bit11 | Bits.Bit12 | Bits.Bit13 | Bits.Bit14 | Bits.Bit15;

        // Fields
        private int m_historianID;
        private TimeTag m_time;
        private float m_value;
        private int m_flags;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="ArchiveData"/> class.
        /// </summary>
        /// <param name="historianID">Historian identifier of <see cref="ArchiveData"/>.</param>
        public ArchiveData(int historianID)
        {
            m_time = TimeTag.MinValue;
            this.HistorianID = historianID;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArchiveData"/> class.
        /// </summary>
        /// <param name="dataPoint">A time series data point.</param>
        public ArchiveData(IDataPoint dataPoint)
            : this(dataPoint.HistorianID, dataPoint.Time, dataPoint.Value, dataPoint.Quality)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArchiveData"/> class.
        /// </summary>
        /// <param name="historianID">Historian identifier of <see cref="ArchiveData"/>.</param>
        /// <param name="time"><see cref="TimeTag"/> of <see cref="ArchiveData"/>.</param>
        /// <param name="value">Floating-point value of <see cref="ArchiveData"/>.</param>
        /// <param name="quality"><see cref="Quality"/> of <see cref="ArchiveData"/>.</param>
        public ArchiveData(int historianID, TimeTag time, float value, Quality quality)
            : this(historianID)
        {
            this.Time = time;
            this.Value = value;
            this.Quality = quality;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArchiveData"/> class.
        /// </summary>
        /// <param name="historianID">Historian identifier of <see cref="ArchiveData"/>.</param>
        /// <param name="binaryImage">Binary image to be used for initializing <see cref="ArchiveData"/>.</param>
        /// <param name="startIndex">0-based starting index of initialization data in the <paramref name="binaryImage"/>.</param>
        /// <param name="length">Valid number of bytes in <paramref name="binaryImage"/> from <paramref name="startIndex"/>.</param>
        public ArchiveData(int historianID, byte[] binaryImage, int startIndex, int length)
            : this(historianID)
        {
            Initialize(binaryImage, startIndex, length);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the historian identifier of <see cref="ArchiveData"/>.
        /// </summary>
        /// <exception cref="ArgumentException">The value being assigned is not positive.</exception>
        public int HistorianID
        {
            get
            {
                return m_historianID;
            }
            set
            {
                if (value < 1)
                    throw new ArgumentException("Value must be positive.");

                m_historianID = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="TimeTag"/> of <see cref="ArchiveData"/>.
        /// </summary>
        /// <exception cref="ArgumentException">The value being assigned is not between 01/01/1995 and 01/19/2063.</exception>
        public virtual TimeTag Time
        {
            get
            {
                return m_time;
            }
            set
            {
                if (value < TimeTag.MinValue || value > TimeTag.MaxValue)
                    throw new ArgumentException("Value must between 01/01/1995 and 01/19/2063.");

                m_time = value;
                Flags = Flags.SetMaskedValue(MillisecondMask, m_time.ToDateTime().Millisecond << 5);
            }
        }

        /// <summary>
        /// Gets or sets the floating-point value of <see cref="ArchiveData"/>.
        /// </summary>
        public virtual float Value
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
        /// Gets or sets the <see cref="Quality"/> of <see cref="ArchiveData"/>.
        /// </summary>
        public virtual Quality Quality
        {
            get
            {
                return (Quality)Flags.GetMaskedValue(QualityMask);
            }
            set
            {
                Flags = Flags.SetMaskedValue(QualityMask, (int)value);
            }
        }

        /// <summary>
        /// Gets a boolean value that indicates whether <see cref="ArchiveData"/> contains any data.
        /// </summary>
        public virtual bool IsEmpty
        {
            get
            {
                return ((m_time == TimeTag.MinValue) && 
                        (m_value == default(float)) && 
                        (Quality == Quality.Unknown));
            }
        }

        /// <summary>
        /// Gets the length of the <see cref="BinaryImage"/>.
        /// </summary>
        public virtual int BinaryLength
        {
            get
            {
                return ByteCount;
            }
        }

        /// <summary>
        /// Gets the binary representation of <see cref="ArchiveData"/>.
        /// </summary>
        public virtual byte[] BinaryImage
        {
            get
            {
                byte[] image = new byte[ByteCount];

                Array.Copy(EndianOrder.LittleEndian.GetBytes((int)m_time.Value), 0, image, 0, 4);
                Array.Copy(EndianOrder.LittleEndian.GetBytes((short)m_flags), 0, image, 4, 2);
                Array.Copy(EndianOrder.LittleEndian.GetBytes(m_value), 0, image, 6, 4);

                return image;
            }
        }

        /// <summary>
        /// Gets or sets the 32-bit word used for storing data of <see cref="ArchiveData"/>.
        /// </summary>
        protected virtual int Flags
        {
            get
            {
                return m_flags;
            }
            set
            {
                m_flags = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes <see cref="ArchiveData"/> from the specified <paramref name="binaryImage"/>.
        /// </summary>
        /// <param name="binaryImage">Binary image to be used for initializing <see cref="ArchiveData"/>.</param>
        /// <param name="startIndex">0-based starting index of initialization data in the <paramref name="binaryImage"/>.</param>
        /// <param name="length">Valid number of bytes in <paramref name="binaryImage"/> from <paramref name="startIndex"/>.</param>
        /// <returns>Number of bytes used from the <paramref name="binaryImage"/> for initializing <see cref="ArchiveData"/>.</returns>
        public virtual int Initialize(byte[] binaryImage, int startIndex, int length)
        {
            if (length - startIndex >= ByteCount)
            {
                // Binary image has sufficient data.
                Flags = EndianOrder.LittleEndian.ToInt16(binaryImage, startIndex + 4);
                Value = EndianOrder.LittleEndian.ToSingle(binaryImage, startIndex  + 6);
                Time = new TimeTag(EndianOrder.LittleEndian.ToInt32(binaryImage, startIndex) +          // Seconds
                                      ((double)(m_flags.GetMaskedValue(MillisecondMask) >> 5) / 1000)); // Milliseconds

                return ByteCount;
            }
            else
            {
                // Binary image does not have sufficient data.
                return 0;
            }
        }

        /// <summary>
        /// Compares the current <see cref="ArchiveData"/> object to <paramref name="obj"/>.
        /// </summary>
        /// <param name="obj">Object against which the current <see cref="ArchiveData"/> object is to be compared.</param>
        /// <returns>
        /// Negative value if the current <see cref="ArchiveData"/> object is less than <paramref name="obj"/>, 
        /// Zero if the current <see cref="ArchiveData"/> object is equal to <paramref name="obj"/>, 
        /// Positive value if the current <see cref="ArchiveData"/> object is greater than <paramref name="obj"/>.
        /// </returns>
        public virtual int CompareTo(object obj)
        {
            ArchiveData other = obj as ArchiveData;
            if (other == null)
            {
                return 1;
            }
            else
            {
                int result = m_historianID.CompareTo(other.HistorianID);
                if (result != 0)
                    return result;
                else
                    return m_time.CompareTo(other.Time);
            }
        }

        /// <summary>
        /// Determines whether the current <see cref="ArchiveData"/> object is equal to <paramref name="obj"/>.
        /// </summary>
        /// <param name="obj">Object against which the current <see cref="ArchiveData"/> object is to be compared for equality.</param>
        /// <returns>true if the current <see cref="ArchiveData"/> object is equal to <paramref name="obj"/>; otherwise false.</returns>
        public override bool Equals(object obj)
        {
            return (CompareTo(obj) == 0);
        }

        /// <summary>
        /// Returns the text representation of <see cref="ArchiveData"/> object.
        /// </summary>
        /// <returns>A <see cref="string"/> value.</returns>
        public override string ToString()
        {
            return string.Format("ID={0}; Time={1}; Value={2}; Quality={3}", m_historianID, m_time, m_value, Quality);
        }

        /// <summary>
        /// Returns the hash code for the current <see cref="ArchiveData"/> object.
        /// </summary>
        /// <returns>A 32-bit signed integer value.</returns>
        public override int GetHashCode()
        {
            return m_historianID.GetHashCode();
        }

        #endregion
    }
}
