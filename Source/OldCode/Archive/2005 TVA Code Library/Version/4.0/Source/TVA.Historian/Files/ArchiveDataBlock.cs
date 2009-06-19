//*******************************************************************************************************
//  ArchiveDataBlock.cs
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
//  02/24/2007 - Pinal C. Patel
//       Generated original version of source code.
//  01/23/2008 - Pinal C. Patel
//       Removed IsForHistoricData and added IsActive to keep track of activity.
//  03/31/2008 - Pinal C. Patel
//       Modified code to use the same FileStream object used by FAT instead to creating a new one.
//       Removed IDisposable interface implementation and Size property.
//  04/20/2009 - Pinal C. Patel
//       Converted to C#.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace TVA.Historian.Files
{
    /// <summary>
    /// Represents a block of <see cref="ArchiveData"/> in an <see cref="ArchiveFile"/>.
    /// </summary>
    /// <seealso cref="ArchiveData"/>
    /// <seealso cref="ArchiveFile"/>
    public class ArchiveDataBlock
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Time in seconds after which the block is considered inactive if no reads or writes were performed.
        /// </summary>
        private const int InactivityPeriod = 300;

        // Fields
        private int m_index;
        private int m_historianID;
        private ArchiveFile m_parent;
        private long m_writeCursor;
        private DateTime m_lastActivityTime;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="ArchiveDataBlock"/> class. 
        /// </summary>
        /// <param name="parent">An <see cref="ArchiveFile"/> object.</param>
        /// <param name="index">0-based index of the <see cref="ArchiveDataBlock"/>.</param>
        /// <param name="historianID">Historian identifier whose <see cref="ArchiveData"/> is stored in the <see cref="ArchiveDataBlock"/>.</param>
        internal ArchiveDataBlock(ArchiveFile parent, int index, int historianID)
        {
            m_parent = parent;
            m_index = index;
            m_historianID = historianID;
            m_writeCursor = Location;
            m_lastActivityTime = DateTime.Now;
            // Initialize the write cursor position by reading existing data. We have to iterate through the returned
            // data in order for the write cursor to progress since Read() implements a "yield return" for return data.
            foreach (ArchiveData dataPoint in Read()) { }
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the 0-based index of the <see cref="ArchiveDataBlock"/>.
        /// </summary>
        public int Index
        {
            get
            {
                return m_index;
            }
        }

        /// <summary>
        /// Gets the start location (byte position) of the <see cref="ArchiveDataBlock"/> in the <see cref="ArchiveFile"/>.
        /// </summary>
        public long Location
        {
            get
            {
                return (m_index * (m_parent.DataBlockSize * 1024));
            }
        }

        /// <summary>
        /// Gets the maximum number of <see cref="ArchiveData"/> points that can be stored in the <see cref="ArchiveDataBlock"/>.
        /// </summary>
        public int Capacity
        {
            get
            {
                return ((m_parent.DataBlockSize * 1024) / ArchiveData.ByteCount);
            }
        }

        /// <summary>
        /// Gets the number of <see cref="ArchiveData"/> points that have been written to the <see cref="ArchiveDataBlock"/>.
        /// </summary>
        public int SlotsUsed
        {
            get
            {
                return (int)((m_writeCursor - Location) / ArchiveData.ByteCount);
            }
        }

        /// <summary>
        /// Gets the number of <see cref="ArchiveData"/> points that can to written to the <see cref="ArchiveDataBlock"/>.
        /// </summary>
        public int SlotsAvailable
        {
            get
            {
                return (Capacity - SlotsUsed);
            }
        }

        /// <summary>
        /// Gets a boolean value that indicates whether the <see cref="ArchiveDataBlock"/> is being actively used.
        /// </summary>
        public bool IsActive
        {
            get
            {
                double inactivity = DateTime.Now.Subtract(m_lastActivityTime).TotalSeconds;
                if (inactivity <= InactivityPeriod)
                {
                    return true;
                }
                else
                {
                    Trace.WriteLine(string.Format("Inactive for {0} seconds (Last activity = {1}; Time now = {2})", inactivity, m_lastActivityTime, DateTime.Now));
                    return false;
                }
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Reads existing <see cref="ArchiveData"/> points from the <see cref="ArchiveDataBlock"/>.
        /// </summary>
        public IEnumerable<ArchiveData> Read()
        {
            lock (m_parent.FileData)
            {
                // We'll start reading from where the data block begins.
                m_parent.FileData.Seek(Location, SeekOrigin.Begin);

                byte[] binaryImage = new byte[ArchiveData.ByteCount];
                for (int i = 1; i <= Capacity; i++)
                {
                    // Read the data in the block.
                    m_lastActivityTime = DateTime.Now;
                    m_parent.FileData.Read(binaryImage, 0, binaryImage.Length);
                    ArchiveData dataPoint = new ArchiveData(m_historianID, binaryImage, 0, binaryImage.Length);
                    if (!dataPoint.IsEmpty)
                    {
                        // There is data - use it.
                        m_writeCursor = m_parent.FileData.Position;
                        yield return dataPoint;
                    }
                    else
                    {
                        // Data is empty - stop reading.
                        yield break;
                    }
                }
            }
        }

        /// <summary>
        /// Writes the <paramref name="dataPoint"/> to the <see cref="ArchiveDataBlock"/>.
        /// </summary>
        /// <param name="dataPoint"><see cref="ArchiveData"/> point to write.</param>
        public void Write(ArchiveData dataPoint)
        {
            if (SlotsAvailable > 0)
            {
                // We have enough space to write the provided point data to the data block.
                m_lastActivityTime = DateTime.Now;
                lock (m_parent.FileData)
                {
                    // Write the data.
                    m_parent.FileData.Seek(m_writeCursor, SeekOrigin.Begin);
                    m_parent.FileData.Write(dataPoint.BinaryImage, 0, ArchiveData.ByteCount);
                    // Update the write cursor.
                    m_writeCursor = m_parent.FileData.Position;
                    // Flush the data if configured.
                    if (!m_parent.CacheWrites)
                        m_parent.FileData.Flush();
                }
            }
            else
            {
                throw (new InvalidOperationException("No slots available for writing new data."));
            }
        }

        /// <summary>
        /// Resets the <see cref="ArchiveDataBlock"/> by overwriting existing <see cref="ArchiveData"/> points with empty <see cref="ArchiveData"/> points.
        /// </summary>
        public void Reset()
        {
            m_writeCursor = Location;
            for (int i = 1; i <= Capacity; i++)
            {
                Write(new ArchiveData(m_historianID));
            }
            m_writeCursor = Location;
        }

        #endregion
    }
}