//******************************************************************************************************
//  ArchiveDataBlock.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  02/24/2007 - Pinal C. Patel
//       Generated original version of source code.
//  01/23/2008 - Pinal C. Patel
//       Removed IsForHistoricData and added IsActive to keep track of activity.
//  03/31/2008 - Pinal C. Patel
//       Modified code to use the same FileStream object used by FAT instead to creating a new one.
//       Removed IDisposable interface implementation and Size property.
//  04/20/2009 - Pinal C. Patel
//       Converted to C#.
//  08/05/2009 - Josh L. Patterson
//       Edited Comments.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  09/23/2009 - Pinal C. Patel
//       Edited code comments.
//       Removed the dependency on ArchiveDataPoint.
//  10/14/2009 - Pinal C. Patel
//       Modified Write() to seek only when necessary.
//  10/11/2010 - Mihir Brahmbhatt
//       Updated header and license agreement.
//  11/18/2010 - J. Ritchie Carroll
//       Added a exception handler for reading (exposed via DataReadException event) to make sure
//       bad data or corruption in an archive file does not stop the read process.
//  11/30/2011 - J. Ritchie Carroll
//       Modified to support buffer optimized ISupportBinaryImage.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using GSF.Parsing;

namespace GSF.Historian.Files
{
    /// <summary>
    /// Represents a block of <see cref="ArchiveDataPoint"/>s in an <see cref="ArchiveFile"/>.
    /// </summary>
    /// <seealso cref="ArchiveFile"/>
    /// <seealso cref="ArchiveDataPoint"/>
    public class ArchiveDataBlock
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Time in seconds after which the block is considered inactive if no reads or writes were performed.
        /// </summary>
        private const int InactivityPeriod = 300;

        // Fields
        private readonly int m_index;
        private readonly int m_historianID;
        private readonly ArchiveFile m_parent;
        private readonly byte[] m_readBuffer;
        private long m_writeCursor;
        private DateTime m_lastActivityTime;

        /// <summary>
        /// Occurs when an <see cref="Exception"/> is encountered while reading <see cref="IDataPoint"/> from the <see cref="ArchiveDataBlock"/>.
        /// </summary>
        [Category("Data"),
        Description("Occurs when an Exception is encountered while reading an IDataPoint from the ArchiveDataBlock.")]
        public event EventHandler<EventArgs<Exception>> DataReadException;

        /// <summary>
        /// Occurs when an <see cref="Exception"/> is encountered while writing <see cref="IDataPoint"/> to the <see cref="ArchiveDataBlock"/>.
        /// </summary>
        [Category("Data"),
        Description("Occurs when an Exception is encountered while writing an IDataPoint to the ArchiveDataBlock.")]
        public event EventHandler<EventArgs<Exception>> DataWriteException;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="ArchiveDataBlock"/> class. 
        /// </summary>
        /// <param name="parent">An <see cref="ArchiveFile"/> object.</param>
        /// <param name="index">0-based index of the <see cref="ArchiveDataBlock"/>.</param>
        /// <param name="historianID">Historian identifier whose <see cref="ArchiveDataPoint"/> is stored in the <see cref="ArchiveDataBlock"/>.</param>
        /// <param name="preRead">true to pre-read data to locate write cursor.</param>
        internal ArchiveDataBlock(ArchiveFile parent, int index, int historianID, bool preRead = true)
        {
            m_parent = parent;
            m_index = index;
            m_historianID = historianID;
            m_readBuffer = new byte[ArchiveDataPoint.FixedLength];
            m_writeCursor = Location;
            m_lastActivityTime = DateTime.UtcNow;

            if (!preRead)
                return;

            // Scan through existing data to locate write cursor
            // ReSharper disable once UnusedVariable
            foreach (IDataPoint dataPoint in Read())
            {
            }
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the 0-based index of the <see cref="ArchiveDataBlock"/>.
        /// </summary>
        public int Index => m_index;

        /// <summary>
        /// Gets the start location (byte position) of the <see cref="ArchiveDataBlock"/> in the <see cref="ArchiveFile"/>.
        /// </summary>
        public long Location => m_index * m_parent.DataBlockSize * 1024;

        /// <summary>
        /// Gets the maximum number of <see cref="ArchiveDataPoint"/>s that can be stored in the <see cref="ArchiveDataBlock"/>.
        /// </summary>
        public int Capacity => m_parent.DataBlockSize * 1024 / ArchiveDataPoint.FixedLength;

        /// <summary>
        /// Gets the number of <see cref="ArchiveDataPoint"/>s that have been written to the <see cref="ArchiveDataBlock"/>.
        /// </summary>
        public int SlotsUsed => (int)((m_writeCursor - Location) / ArchiveDataPoint.FixedLength);

        /// <summary>
        /// Gets the number of <see cref="ArchiveDataPoint"/>s that can to written to the <see cref="ArchiveDataBlock"/>.
        /// </summary>
        public int SlotsAvailable => Capacity - SlotsUsed;

        /// <summary>
        /// Gets a boolean value that indicates whether the <see cref="ArchiveDataBlock"/> is being actively used.
        /// </summary>
        public bool IsActive => DateTime.UtcNow.Subtract(m_lastActivityTime).TotalSeconds <= InactivityPeriod;

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Reads existing <see cref="ArchiveDataPoint"/>s from the <see cref="ArchiveDataBlock"/>.
        /// </summary>
        /// <returns>Returns <see cref="ArchiveDataPoint"/>s from the <see cref="ArchiveDataBlock"/>.</returns>
        public IEnumerable<IDataPoint> Read()
        {
            ArchiveDataPoint dataPoint;

            lock (m_parent.FileData)
            {
                // We'll start reading from where the data block begins.
                m_parent.FileData.Seek(Location, SeekOrigin.Begin);

                for (int i = 0; i < Capacity; i++)
                {
                    // Read the data in the block.
                    m_lastActivityTime = DateTime.UtcNow;
                    m_parent.FileData.Read(m_readBuffer, 0, m_readBuffer.Length);

                    // Attempt to parse archive data point
                    try
                    {
                        dataPoint = new ArchiveDataPoint(m_historianID, m_readBuffer, 0, m_readBuffer.Length);
                    }
                    catch (Exception ex)
                    {
                        dataPoint = null;
                        OnDataReadException(ex);
                    }

                    if (dataPoint != null && !dataPoint.IsEmpty)
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
        /// <param name="dataPoint"><see cref="IDataPoint"/> to write.</param>
        /// <param name="suppressExceptions">Set to <c>true</c> to suppress write exceptions; defaults to <c>false</c>.</param>
        /// <returns>
        /// <c>true</c> if data point was written; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// If <paramref name="suppressExceptions"/> is <c>false</c>, the default value, any encountered
        /// exception will be thrown on the call stack.
        /// </remarks>
        public bool Write(IDataPoint dataPoint, bool suppressExceptions = false)
        {
            Exception ex;

            if (Write(dataPoint, out ex))
                return true;

            if (suppressExceptions)
                return false;

            throw ex;
        }

        /// <summary>
        /// Writes the <paramref name="dataPoint"/> to the <see cref="ArchiveDataBlock"/>.
        /// </summary>
        /// <param name="dataPoint"><see cref="IDataPoint"/> to write.</param>
        /// <param name="exception">Any <see cref="Exception"/> that may have been encountered while writing.</param>
        /// <returns>
        /// <c>true</c> if data point was written; otherwise, <c>false</c>.
        /// </returns>
        public bool Write(IDataPoint dataPoint, out Exception exception)
        {
            exception = null;

            try
            {
                TimeTag value = dataPoint.Time;

                // Do not attempt to write values with a bad timestamp, this will just throw an exception
                if (value.CompareTo(TimeTag.MinValue) < 0 || value.CompareTo(TimeTag.MaxValue) > 0)
                {
                    exception = new TimeTagException("Skipping data write for point: Bad time tag, value must between 01/01/1995 and 01/19/2063");
                }
                else
                {
                    if (SlotsAvailable > 0)
                    {
                        // We have enough space to write the provided point data to the data block.
                        m_lastActivityTime = DateTime.UtcNow;

                        lock (m_parent.FileData)
                        {
                            // Write the data.
                            if (m_writeCursor != m_parent.FileData.Position)
                                m_parent.FileData.Seek(m_writeCursor, SeekOrigin.Begin);

                            dataPoint.CopyBinaryImageToStream(m_parent.FileData);

                            // Update the write cursor.
                            m_writeCursor = m_parent.FileData.Position;

                            // Flush the data if configured.
                            if (!m_parent.CacheWrites)
                                m_parent.FileData.Flush();
                        }
                    }
                    else
                    {
                        exception = new InvalidOperationException("Skipping data write for point: No slots available for writing new data");
                    }
                }
            }
            catch (Exception ex)
            {
                exception = new InvalidOperationException($"Skipping data write for point: {ex.Message}", ex);
            }

            if ((object)exception == null)
                return true;

            OnDataWriteException(exception);

            return false;
        }

        /// <summary>
        /// Resets the <see cref="ArchiveDataBlock"/> by overwriting existing <see cref="ArchiveDataPoint"/>s with empty <see cref="ArchiveDataPoint"/>s.
        /// </summary>
        public void Reset()
        {
            m_writeCursor = Location;

            for (int i = 1; i <= Capacity; i++)
                Write(new ArchiveDataPoint(m_historianID), true);

            m_writeCursor = Location;
        }

        /// <summary>
        /// Raises the <see cref="DataReadException"/> event.
        /// </summary>
        /// <param name="ex"><see cref="Exception"/> to send to <see cref="DataReadException"/> event.</param>
        protected virtual void OnDataReadException(Exception ex)
        {
            DataReadException?.Invoke(this, new EventArgs<Exception>(ex));
        }

        /// <summary>
        /// Raises the <see cref="DataWriteException"/> event.
        /// </summary>
        /// <param name="ex"><see cref="Exception"/> to send to <see cref="DataWriteException"/> event.</param>
        protected virtual void OnDataWriteException(Exception ex)
        {
            DataWriteException?.Invoke(this, new EventArgs<Exception>(ex));
        }

        #endregion
    }
}