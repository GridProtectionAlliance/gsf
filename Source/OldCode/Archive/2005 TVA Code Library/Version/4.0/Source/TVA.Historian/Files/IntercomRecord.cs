//*******************************************************************************************************
//  IntercomRecord.cs
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
//  03/09/2007 - Pinal C. Patel
//       Generated original version of source code.
//  04/17/2009 - Pinal C. Patel
//       Converted to C#.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using TVA.Parsing;

namespace TVA.Historian.Files
{
    /// <summary>
    /// Represents a record in the <see cref="IntercomFile"/> that contains runtime information of a historian.
    /// </summary>
    /// <seealso cref="IntercomFile"/>
    public class IntercomRecord : ISupportBinaryImage
    {
        // **************************************************************************************************
        // *                                        Binary Structure                                        *
        // **************************************************************************************************
        // * # Of Bytes Byte Index Data Type  Property Name                                                 *
        // * ---------- ---------- ---------- --------------------------------------------------------------*
        // * 4          0-3        Int32      DataBlocksUsed                                                *
        // * 4          4-7        Boolean    RolloverInProggress                                           *
        // * 8          8-15       Double     LatestDataTime                                                *
        // * 4          16-19      Int32      LatestDataId                                                  *
        // * 160        20-179     Double(20) SourceLatestDataTime                                                     *
        // **************************************************************************************************

        #region [ Members ]

        // Constants

        /// <summary>
        /// Specifies the number of bytes in the binary image of <see cref="IntercomRecord"/>.
        /// </summary>
        public const int ByteCount = 180;

        // Fields
        private int m_recordID;
        private int m_dataBlocksUsed;
        private bool m_rolloverInProgress;
        private TimeTag m_latestDataTime;
        private int m_latestDataId;
        private List<TimeTag> m_sourceLatestDataTime;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="IntercomRecord"/> class.
        /// </summary>
        /// <param name="recordID">ID of the <see cref="IntercomRecord"/>.</param>
        public IntercomRecord(int recordID)
        {
            m_recordID = recordID;
            m_latestDataTime = TimeTag.MinValue;
            m_sourceLatestDataTime = new List<TimeTag>();
            for (int i = 0; i < 20; i++)
            {
                m_sourceLatestDataTime.Add(TimeTag.MinValue);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntercomRecord"/> class.
        /// </summary>
        /// <param name="recordID">ID of the <see cref="IntercomRecord"/>.</param>
        /// <param name="binaryImage">Binary image to be used for initializing <see cref="IntercomRecord"/>.</param>
        /// <param name="startIndex">0-based starting index of initialization data in the <paramref name="binaryImage"/>.</param>
        /// <param name="length">Valid number of bytes in <paramref name="binaryImage"/> from <paramref name="startIndex"/>.</param>
        public IntercomRecord(int recordID, byte[] binaryImage, int startIndex, int length)
            : this(recordID)
        {
            Initialize(binaryImage, startIndex, length);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the number of allocated <see cref="ArchiveDataBlock"/>s in the active <see cref="ArchiveFile"/>.
        /// </summary>
        /// <exception cref="ArgumentException">Value being set is not positive or zero.</exception>
        public int DataBlocksUsed
        {
            get
            {
                lock (this)
                {
                    return m_dataBlocksUsed;
                }
            }
            set
            {
                if (value < 0)
                    throw new ArgumentException("Value must be positive or zero.");

                lock (this)
                {
                    m_dataBlocksUsed = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the active <see cref="ArchiveFile"/> if being rolled-over.
        /// </summary>
        public bool RolloverInProgress
        {
            get
            {
                lock (this)
                {
                    return m_rolloverInProgress;
                }
            }
            set
            {
                lock (this)
                {
                    m_rolloverInProgress = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="TimeTag"/> of latest <see cref="ArchiveData"/> received by the active <see cref="ArchiveFile"/>.
        /// </summary>
        public TimeTag LatestDataTime
        {
            get
            {
                lock (this)
                {
                    return m_latestDataTime;
                }
            }
            set
            {
                lock (this)
                {
                    m_latestDataTime = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the historian identifier of latest <see cref="ArchiveData"/> received by the active <see cref="ArchiveFile"/>.
        /// </summary>
        public int LatestDataId
        {
            get
            {
                lock (this)
                {
                    return m_latestDataId;
                }
            }
            set
            {
                lock (this)
                {
                    m_latestDataId = value;
                }
            }
        }

        /// <summary>
        /// Gets a list of <see cref="TimeTag"/>s of the latest <see cref="ArchiveData"/> received from each of the <see cref="MetadataRecord.SourceId"/>s.
        /// </summary>
        public IList<TimeTag> SourceLatestDataTime
        {
            get
            {
                lock (this)
                {
                    return m_sourceLatestDataTime.AsReadOnly();
                }
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
        /// Gets the binary representation of <see cref="IntercomRecord"/>.
        /// </summary>
        public byte[] BinaryImage
        {
            get
            {
                byte[] image = new byte[ByteCount];

                lock (this)
                {
                    Array.Copy(EndianOrder.LittleEndian.GetBytes(m_dataBlocksUsed), 0, image, 0, 4);
                    Array.Copy(EndianOrder.LittleEndian.GetBytes(Convert.ToInt32(m_rolloverInProgress)), 0, image, 4, 4);
                    Array.Copy(EndianOrder.LittleEndian.GetBytes(m_latestDataTime.Value), 0, image, 8, 8);
                    Array.Copy(EndianOrder.LittleEndian.GetBytes(m_latestDataId), 0, image, 16, 4);
                    for (int i = 0; i < m_sourceLatestDataTime.Count; i++)
                    {
                        Array.Copy(EndianOrder.LittleEndian.GetBytes(m_sourceLatestDataTime[i].Value), 0, image, (20 + (i * 8)), 8);
                    }
                }

                return image;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes <see cref="IntercomRecord"/> from the specified <paramref name="binaryImage"/>.
        /// </summary>
        /// <param name="binaryImage">Binary image to be used for initializing <see cref="IntercomRecord"/>.</param>
        /// <param name="startIndex">0-based starting index of initialization data in the <paramref name="binaryImage"/>.</param>
        /// <param name="length">Valid number of bytes in <paramref name="binaryImage"/> from <paramref name="startIndex"/>.</param>
        /// <returns>Number of bytes used from the <paramref name="binaryImage"/> for initializing <see cref="IntercomRecord"/>.</returns>
        public int Initialize(byte[] binaryImage, int startIndex, int length)
        {
            if (length - startIndex >= ByteCount)
            {
                // Binary image has sufficient data.
                DataBlocksUsed = EndianOrder.LittleEndian.ToInt32(binaryImage, startIndex);
                RolloverInProgress = EndianOrder.LittleEndian.ToBoolean(binaryImage, startIndex + 4);
                LatestDataTime = new TimeTag(EndianOrder.LittleEndian.ToDouble(binaryImage, startIndex + 8));
                LatestDataId = EndianOrder.LittleEndian.ToInt32(binaryImage, startIndex + 16);
                for (int i = 0; i < m_sourceLatestDataTime.Count; i++)
                {
                    m_sourceLatestDataTime[i] = new TimeTag(EndianOrder.LittleEndian.ToDouble(binaryImage, startIndex + 20 + (i * 8)));
                }

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
