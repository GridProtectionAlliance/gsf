//******************************************************************************************************
//  BlockAllocatedMemoryStream.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
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
//  06/14/2013 - J. Ritchie Carroll
//       Adapted from the "MemoryTributary" class written by Sebastian Friston:
//          Source Code: http://memorytributary.codeplex.com/
//          Article: http://www.codeproject.com/Articles/348590/A-replacement-for-MemoryStream
//  11/21/2016 - Steven E. Chisholm
//       A complete refresh of BlockAllocatedMemoryStream and how it works.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using GSF.Collections;

// ReSharper disable VirtualMemberCallInConstructor
namespace GSF.IO
{
    /// <summary>
    /// Defines a stream whose backing store is memory. Externally this class operates similar to a <see cref="MemoryStream"/>,
    /// internally it uses dynamically allocated buffer blocks instead of one large contiguous array of data.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="BlockAllocatedMemoryStream"/> has two primary benefits over a normal <see cref="MemoryStream"/>, first, the
    /// allocation of a large contiguous array of data in <see cref="MemoryStream"/> can fail when the requested amount of contiguous
    /// memory is unavailable - the <see cref="BlockAllocatedMemoryStream"/> prevents this; second, a <see cref="MemoryStream"/> will
    /// constantly reallocate the buffer size as the stream grows and shrinks and then copy all the data from the old buffer to the
    /// new - the <see cref="BlockAllocatedMemoryStream"/> maintains its blocks over its life cycle, unless manually cleared, thus
    /// eliminating unnecessary allocations and garbage collections when growing and reusing a stream.
    /// </para>
    /// <para>
    /// Important: Unlike <see cref="MemoryStream"/>, the <see cref="BlockAllocatedMemoryStream"/> will not use a user provided buffer
    /// as its backing buffer. Any user provided buffers used to instantiate the class will be copied into internally managed reusable
    /// memory buffers. Subsequently, the <see cref="BlockAllocatedMemoryStream"/> does not support the notion of a non-expandable
    /// stream. If you are using a <see cref="MemoryStream"/> with your own buffer, the <see cref="BlockAllocatedMemoryStream"/> will
    /// not provide any immediate benefit.
    /// </para>
    /// <para>
    /// Note that the <see cref="BlockAllocatedMemoryStream"/> will maintain all allocated blocks for stream use until the
    /// <see cref="Clear"/> method is called or the class is disposed.
    /// </para>
    /// <para>
    /// No members in the <see cref="BlockAllocatedMemoryStream"/> are guaranteed to be thread safe. Make sure any calls are
    /// synchronized when simultaneously accessed from different threads.
    /// </para>
    /// </remarks>
    public class BlockAllocatedMemoryStream : Stream
    {
        // Note: Since byte blocks are pooled, they will not be 
        //       initialized unless a Read/Write operation occurs 
        //       when m_position > m_length

        #region [ Members ]

        // Constants
        private const int BlockSize = 8 * 1024;
        private const int ShiftBits = 3 + 10;
        private const int BlockMask = BlockSize - 1;

        // Fields
        private List<byte[]> m_blocks;
        private long m_length;
        private long m_position;
        private long m_capacity;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of <see cref="BlockAllocatedMemoryStream"/>.
        /// </summary>
        public BlockAllocatedMemoryStream()
        {
            m_blocks = new List<byte[]>();
        }

        /// <summary>
        /// Initializes a new instance of <see cref="BlockAllocatedMemoryStream"/> from specified <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">Initial buffer to copy into stream.</param>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null.</exception>
        /// <remarks>
        /// Unlike <see cref="MemoryStream"/>, the <see cref="BlockAllocatedMemoryStream"/> will not use the provided
        /// <paramref name="buffer"/> as its backing buffer. The buffer will be copied into internally managed reusable
        /// memory buffers. Subsequently, the notion of a non-expandable stream is not supported.
        /// </remarks>
        public BlockAllocatedMemoryStream(byte[] buffer)
            : this(buffer, 0, (object)buffer == null ? 0 : buffer.Length)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="BlockAllocatedMemoryStream"/> from specified region of <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">Initial buffer to copy into stream.</param>
        /// <param name="startIndex">0-based start index into the <paramref name="buffer"/>.</param>
        /// <param name="length">Valid number of bytes within <paramref name="buffer"/> from <paramref name="startIndex"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> or <paramref name="length"/> is less than 0 -or- 
        /// <paramref name="startIndex"/> and <paramref name="length"/> will exceed <paramref name="buffer"/> length.
        /// </exception>
        /// <remarks>
        /// Unlike <see cref="MemoryStream"/>, the <see cref="BlockAllocatedMemoryStream"/> will not use the provided
        /// <paramref name="buffer"/> as its backing buffer. The buffer will be copied into internally managed reusable
        /// memory buffers. Subsequently, the notion of a non-expandable stream is not supported.
        /// </remarks>
        public BlockAllocatedMemoryStream(byte[] buffer, int startIndex, int length)
            : this()
        {
            buffer.ValidateParameters(startIndex, length);
            Write(buffer, startIndex, length);
        }

        /// <summary>
        /// Initializes a new instance of <see cref="BlockAllocatedMemoryStream"/> for specified <paramref name="capacity"/>.
        /// </summary>
        /// <param name="capacity">Initial length of the stream.</param>
        public BlockAllocatedMemoryStream(int capacity)
            : this()
        {
            SetLength(capacity);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets a value that indicates whether the <see cref="BlockAllocatedMemoryStream"/> object supports reading.
        /// </summary>
        /// <remarks>
        /// This is always <c>true</c>.
        /// </remarks>
        public override bool CanRead => true;

        /// <summary>
        /// Gets a value that indicates whether the <see cref="BlockAllocatedMemoryStream"/> object supports seeking.
        /// </summary>
        /// <remarks>
        /// This is always <c>true</c>.
        /// </remarks>
        public override bool CanSeek => true;

        /// <summary>
        /// Gets a value that indicates whether the <see cref="BlockAllocatedMemoryStream"/> object supports writing.
        /// </summary>
        /// <remarks>
        /// This is always <c>true</c>.
        /// </remarks>
        public override bool CanWrite => true;

        /// <summary>
        /// Gets current stream length for this <see cref="BlockAllocatedMemoryStream"/> instance.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        public override long Length
        {
            get
            {
                if (m_disposed)
                    throw new ObjectDisposedException("BlockAllocatedMemoryStream", "The stream is closed.");

                return m_length;
            }
        }

        /// <summary>
        /// Gets current stream position for this <see cref="BlockAllocatedMemoryStream"/> instance.
        /// </summary>
        /// <exception cref="IOException">Seeking was attempted before the beginning of the stream.</exception>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        public override long Position
        {
            get
            {
                if (m_disposed)
                    throw new ObjectDisposedException("BlockAllocatedMemoryStream", "The stream is closed.");

                return m_position;
            }
            set
            {
                if (m_disposed)
                    throw new ObjectDisposedException("BlockAllocatedMemoryStream", "The stream is closed.");

                if (value < 0L)
                    throw new IOException("Seek was attempted before the beginning of the stream.");

                m_position = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="BlockAllocatedMemoryStream"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; otherwise, <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    // Make sure buffer blocks get returned to the pool
                    if (disposing)
                        Clear();
                }
                finally
                {
                    m_disposed = true;          // Prevent duplicate dispose.
                    base.Dispose(disposing);    // Call base class Dispose().
                }
            }
        }

        /// <summary>
        /// Clears the entire <see cref="BlockAllocatedMemoryStream"/> contents and releases any allocated memory blocks.
        /// </summary>
        public void Clear()
        {
            m_position = 0;
            m_length = 0;
            m_capacity = 0;

            // In the event that an exception occurs, we don't want to have released blocks that are still in this memory stream.
            List<byte[]> blocks = m_blocks;

            m_blocks = new List<byte[]>();

            foreach (var block in blocks)
                MemoryBlockPool.Enqueue(block);
        }

        /// <summary>
        /// Sets the <see cref="Position"/> within the current stream to the specified value relative the <paramref name="origin"/>.
        /// </summary>
        /// <returns>
        /// The new position within the stream, calculated by combining the initial reference point and the offset.
        /// </returns>
        /// <param name="offset">The new position within the stream. This is relative to the <paramref name="origin"/> parameter, and can be positive or negative.</param>
        /// <param name="origin">A value of type <see cref="SeekOrigin"/>, which acts as the seek reference point.</param>
        /// <exception cref="IOException">Seeking was attempted before the beginning of the stream.</exception>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        public override long Seek(long offset, SeekOrigin origin)
        {
            if (m_disposed)
                throw new ObjectDisposedException("BlockAllocatedMemoryStream", "The stream is closed.");

            switch (origin)
            {
                case SeekOrigin.Begin:
                    if (offset < 0L)
                        throw new IOException("Seek was attempted before the beginning of the stream.");

                    m_position = offset;
                    break;
                case SeekOrigin.Current:
                    if (m_position + offset < 0L)
                        throw new IOException("Seek was attempted before the beginning of the stream.");

                    m_position += offset;
                    break;
                case SeekOrigin.End:
                    if (m_length + offset < 0L)
                        throw new IOException("Seek was attempted before the beginning of the stream.");

                    m_position = m_length + offset;
                    break;
            }

            // Note: the length is not adjusted after this seek to reflect what MemoryStream.Seek does
            return m_position;
        }

        /// <summary>
        /// Sets the length of the current stream to the specified value.
        /// </summary>
        /// <param name="value">The value at which to set the length.</param>
        /// <remarks>
        /// If this length is larger than the previous length, the data is initialized to 0's between the previous length and the current length.
        /// </remarks>
        public override void SetLength(long value)
        {
            if (value > m_capacity)
                EnsureCapacity(value);

            if (m_length < value)
                InitializeToPosition(value);

            m_length = value;

            if (m_position > m_length)
                m_position = m_length;
        }

        /// <summary>
        /// Reads a block of bytes from the current stream and writes the data to <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">When this method returns, contains the specified byte array with the values between <paramref name="startIndex"/> and (<paramref name="startIndex"/> + <paramref name="length"/> - 1) replaced by the characters read from the current stream.</param>
        /// <param name="startIndex">The byte offset in <paramref name="buffer"/> at which to begin reading.</param>
        /// <param name="length">The maximum number of bytes to read.</param>
        /// <returns>
        /// The total number of bytes written into the buffer. This can be less than the number of bytes requested if that number of bytes are not currently available, or zero if the end of the stream is reached before any bytes are read.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> or <paramref name="length"/> is less than 0 -or- 
        /// <paramref name="startIndex"/> and <paramref name="length"/> will exceed <paramref name="buffer"/> length.
        /// </exception>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        public override int Read(byte[] buffer, int startIndex, int length)
        {
            if (m_disposed)
                throw new ObjectDisposedException("BlockAllocatedMemoryStream", "The stream is closed.");

            buffer.ValidateParameters(startIndex, length);

            // Do not read beyond the end of the stream
            long remainingBytes = m_length - m_position;

            if (remainingBytes <= 0)
                return 0;

            if (length > remainingBytes)
                length = (int)remainingBytes;

            int bytesRead = length;

            // Must read 1 block at a time
            do
            {
                int blockOffset = (int)(m_position & BlockMask);
                int bytesToRead = Math.Min(length, BlockSize - blockOffset);

                Buffer.BlockCopy(m_blocks[(int)(m_position >> ShiftBits)], blockOffset, buffer, startIndex, bytesToRead);

                length -= bytesToRead;
                startIndex += bytesToRead;
                m_position += bytesToRead;
            }
            while (length > 0);

            return bytesRead;
        }

        /// <summary>
        /// Reads a byte from the current stream.
        /// </summary>
        /// <returns>
        /// The current byte cast to an <see cref="Int32"/>, or -1 if the end of the stream has been reached.
        /// </returns>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        public override int ReadByte()
        {
            if (m_disposed)
                throw new ObjectDisposedException("BlockAllocatedMemoryStream", "The stream is closed.");

            if (m_position >= m_length)
                return -1;

            byte value = m_blocks[(int)(m_position >> ShiftBits)][(int)(m_position & BlockMask)];
            m_position++;

            return value;
        }

        /// <summary>
        /// Writes a block of bytes to the current stream using data read from <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The buffer to write data from.</param>
        /// <param name="startIndex">The byte offset in <paramref name="buffer"/> at which to begin writing from.</param>
        /// <param name="length">The maximum number of bytes to write.</param>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> or <paramref name="length"/> is less than 0 -or- 
        /// <paramref name="startIndex"/> and <paramref name="length"/> will exceed <paramref name="buffer"/> length.
        /// </exception>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        public override void Write(byte[] buffer, int startIndex, int length)
        {
            if (m_disposed)
                throw new ObjectDisposedException("BlockAllocatedMemoryStream", "The stream is closed.");

            buffer.ValidateParameters(startIndex, length);

            if (m_position + length > m_capacity)
                EnsureCapacity(m_position + length);

            if (m_position > m_length)
                InitializeToPosition(m_position);

            if (m_length < m_position + length)
                m_length = m_position + length;

            if (length == 0)
                return;

            do
            {
                int blockOffset = (int)(m_position & BlockMask);
                int bytesToWrite = Math.Min(length, BlockSize - blockOffset);

                Buffer.BlockCopy(buffer, startIndex, m_blocks[(int)(m_position >> ShiftBits)], blockOffset, bytesToWrite);

                length -= bytesToWrite;
                startIndex += bytesToWrite;
                m_position += bytesToWrite;
            }
            while (length > 0);
        }

        /// <summary>
        /// Writes a byte to the current stream at the current position.
        /// </summary>
        /// <param name="value">The byte to write.</param>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        public override void WriteByte(byte value)
        {
            if (m_disposed)
                throw new ObjectDisposedException("BlockAllocatedMemoryStream", "The stream is closed.");

            if (m_position + 1 > m_capacity)
                EnsureCapacity(m_position + 1);

            if (m_position > m_length)
                InitializeToPosition(m_position);

            if (m_length < m_position + 1)
                m_length = m_position + 1;

            m_blocks[(int)(m_position >> ShiftBits)][m_position & BlockMask] = value;
            m_position++;
        }

        /// <summary>
        /// Writes the stream contents to a byte array, regardless of the <see cref="Position"/> property.
        /// </summary>
        /// <returns>A <see cref="byte"/>[] containing the current data in the stream</returns>
        /// <remarks>
        /// This may fail if there is not enough contiguous memory available to hold current size of stream.
        /// When possible use methods which operate on streams directly instead.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Cannot create a byte array with more than 2,147,483,591 elements.</exception>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        public byte[] ToArray()
        {
            if (m_disposed)
                throw new ObjectDisposedException("BlockAllocatedMemoryStream", "The stream is closed.");

            if (m_length > 0x7FFFFFC7L)
                throw new InvalidOperationException("Cannot create a byte array of size " + m_length);

            byte[] destination = new byte[m_length];
            long originalPosition = m_position;

            m_position = 0;
            Read(destination, 0, (int)m_length);
            m_position = originalPosition;

            return destination;
        }

        /// <summary>
        /// Reads specified number of bytes from source stream into this <see cref="BlockAllocatedMemoryStream"/>
        /// starting at the current position.
        /// </summary>
        /// <param name="source">The stream containing the data to copy</param>
        /// <param name="length">The number of bytes to copy</param>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        public void ReadFrom(Stream source, long length)
        {
            // Note: A faster way would be to write directly to the BlockAllocatedMemoryStream
            if (m_disposed)
                throw new ObjectDisposedException("BlockAllocatedMemoryStream", "The stream is closed.");

            byte[] buffer = MemoryBlockPool.Dequeue();

            do
            {
                int bytesRead = source.Read(buffer, 0, (int)Math.Min(BlockSize, length));

                if (bytesRead == 0)
                    throw new EndOfStreamException();

                length -= bytesRead;
                Write(buffer, 0, bytesRead);
            }
            while (length > 0);

            MemoryBlockPool.Enqueue(buffer);
        }

        /// <summary>
        /// Writes the entire stream into destination, regardless of <see cref="Position"/>, which remains unchanged.
        /// </summary>
        /// <param name="destination">The stream onto which to write the current contents.</param>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        public void WriteTo(Stream destination)
        {
            if (m_disposed)
                throw new ObjectDisposedException("BlockAllocatedMemoryStream", "The stream is closed.");

            long originalPosition = m_position;
            m_position = 0;

            CopyTo(destination);

            m_position = originalPosition;
        }

        /// <summary>
        /// Overrides the <see cref="Stream.Flush"/> method so that no action is performed.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method overrides the <see cref="Stream.Flush"/> method.
        /// </para>
        /// <para>
        /// Because any data written to a <see cref="BlockAllocatedMemoryStream"/> object is
        /// written into RAM, this method is superfluous.
        /// </para>
        /// </remarks>
        public override void Flush()
        {
            // Nothing to flush...
        }

        /// <summary>
        /// Makes sure desired <paramref name="length"/> can be accommodated by future data accesses.
        /// </summary>
        /// <param name="length">Minimum desired stream capacity.</param>
        private void EnsureCapacity(long length)
        {
            while (m_capacity < length)
            {
                m_blocks.Add(MemoryBlockPool.Dequeue());
                m_capacity += BlockSize;
            }
        }

        /// <summary>
        /// Initializes all of the bytes to zero.
        /// </summary>
        private void InitializeToPosition(long position)
        {
            long bytesToClear = position - m_length;

            while (bytesToClear > 0)
            {
                int bytesToClearInBlock = (int)Math.Min(bytesToClear, BlockSize - (m_length & BlockMask));
                Array.Clear(m_blocks[(int)(m_length >> ShiftBits)], (int)(m_length & BlockMask), bytesToClearInBlock);
                m_length += bytesToClearInBlock;
                bytesToClear = position - m_length;
            }
        }

        #endregion


        // Allow up to 100 items of 8KB items to remain on the buffer pool. This might need to be increased if the buffer pool becomes more 
        // extensively used. Allocation Statistics will be logged in the Logger.
        private static readonly DynamicObjectPool<byte[]> MemoryBlockPool = new DynamicObjectPool<byte[]>(() => new byte[BlockSize], 100);
    }
}