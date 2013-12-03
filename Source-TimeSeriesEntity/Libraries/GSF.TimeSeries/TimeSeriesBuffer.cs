//******************************************************************************************************
//  BufferBlockMeasurement.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  01/17/2013 - J. Ritchie Carroll
//       Generated original version of source code.
//  10/17/2013 - Stephen C. Wills
//       Redefined BufferBlockMeasurement to TimeSeriesBuffer, taking advantage of the new
//       TimeSeriesEntity hierarchy to eliminate the meaningless Value and StateFlags properties.
//       Also modified to properly implement IDisposable.
//
//******************************************************************************************************

using System;

namespace GSF.TimeSeries
{
    /// <summary>
    /// Represents a byte buffer that can be transported through the system as a <see cref="ITimeSeriesEntity"/>.
    /// </summary>
    public class TimeSeriesBuffer : TimeSeriesEntityBase, IDisposable
    {
        #region [ Members ]

        // Members
        private byte[] m_buffer;
        private readonly int m_length;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="TimeSeriesBuffer"/> from an existing buffer.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> or <paramref name="length"/> is less than 0 -or- 
        /// <paramref name="startIndex"/> and <paramref name="length"/> will exceed <paramref name="buffer"/> length.
        /// </exception>
        public TimeSeriesBuffer(Guid id, Ticks timestamp, byte[] buffer, int startIndex, int length)
            : base(id, timestamp)
        {
            // Validate buffer parameters
            buffer.ValidateParameters(startIndex, length);

            // We don't hold on to source buffer (we don't own it), so we grab one from the buffer pool
            m_buffer = BufferPool.TakeBuffer(length);

            // Copy buffer contents onto our local buffer
            System.Buffer.BlockCopy(buffer, startIndex, m_buffer, 0, length);
            m_length = length;

        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="TimeSeriesBuffer"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~TimeSeriesBuffer()
        {
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Cached buffer image.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Use the <see cref="Length"/> property to determine the valid number of bytes in the
        /// buffer. The actual length of the <see cref="Buffer"/> byte array may exceed the value
        /// of <see cref="Length"/>, however any bytes in the buffer beyond <see cref="Length"/>
        /// are considered unavailable and accessing these bytes may result in an exception.
        /// </para>
        /// <para>
        /// Do not cache <see cref="Buffer"/> for your own use, copy data from this buffer onto
        /// your own buffer as this buffer is from the buffer pool and will be returned back to
        /// the pool by this class.
        /// </para>
        /// </remarks>
        public byte[] Buffer
        {
            get
            {
                return m_buffer;
            }
        }

        /// <summary>
        /// Valid length of cached buffer image.
        /// </summary>
        public int Length
        {
            get
            {
                return m_length;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="TimeSeriesBuffer"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="TimeSeriesBuffer"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if ((object)m_buffer != null)
                    {
                        BufferPool.ReturnBuffer(m_buffer);
                        m_buffer = null;
                    }
                }
                finally
                {
                    m_disposed = true;  // Prevent duplicate dispose.
                }
            }
        }

        #endregion
    }
}