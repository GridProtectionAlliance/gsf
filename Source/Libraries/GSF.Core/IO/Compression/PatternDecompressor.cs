//******************************************************************************************************
//  PatternDecompressor.cs - Gbtc
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
//  01/27/2012 - Stephen C. Wills
//       Generated original version of source code.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;

namespace GSF.IO.Compression
{
    /// <summary>
    /// Defines functions for decompression of data compressed by the PatternCompressor.
    /// </summary>
    public class PatternDecompressor
    {
        #region [ Members ]

        // Fields
        private byte[] m_dataBuffer;
        private int m_dataBufferOffset;
        private int m_dataBufferEnd;

        private uint[] m_backBuffer;
        private int m_backBufferStart;
        private int m_backBufferLength;

        private byte m_compressionStrength;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the compression strength defined by the compressed stream.
        /// </summary>
        public byte CompressionStrength
        {
            get
            {
                return m_compressionStrength;
            }
            private set
            {
                if (value > 31)
                    throw new ArgumentOutOfRangeException(nameof(value), "Compression strength must be 0 to 31");

                if (m_backBuffer == null || m_backBuffer.Length < value + 1)
                    m_backBuffer = new uint[value + 1];

                m_backBufferStart = 0;
                m_backBufferLength = 0;

                m_compressionStrength = value;
            }
        }

        /// <summary>
        /// Gets the length of the data left in the buffer.
        /// </summary>
        public int DataBufferLength
        {
            get
            {
                return m_dataBufferEnd - m_dataBufferOffset;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Inserts the given data into the data buffer for decompression.
        /// </summary>
        /// <param name="data">The data to be inserted into the data buffer.</param>
        public void AugmentBuffer(byte[] data)
        {
            AugmentBuffer(data, 0, data.Length);
        }

        /// <summary>
        /// Inserts the given data into the data buffer for decompression.
        /// </summary>
        /// <param name="data">The data to be inserted into the data buffer.</param>
        /// <param name="dataLength">The amount of data to be taken from the given buffer and placed in the data buffer.</param>
        public void AugmentBuffer(byte[] data, int dataLength)
        {
            AugmentBuffer(data, 0, dataLength);
        }

        /// <summary>
        /// Inserts the given data into the data buffer for decompression.
        /// </summary>
        /// <param name="data">The data to be inserted into the data buffer.</param>
        /// <param name="offset">The amount of data to be ignored at the beginning of the buffer.</param>
        /// <param name="dataLength">The amount of data to be taken from the given buffer and placed in the data buffer.</param>
        public void AugmentBuffer(byte[] data, int offset, int dataLength)
        {
            int dataBufferLength = DataBufferLength;
            int neededLength = dataBufferLength + dataLength;

            byte[] temp;
            int desiredLength;

            // First check to see if the data will fit in the remaining available buffer space.
            if (m_dataBuffer != null && dataLength < m_dataBuffer.Length - m_dataBufferEnd)
            {
                Buffer.BlockCopy(data, offset, m_dataBuffer, m_dataBufferEnd, dataLength);
                m_dataBufferEnd += dataLength;
            }

            // Next, check to see if the data will fit in the full buffer space.
            if (m_dataBuffer != null && neededLength < m_dataBuffer.Length)
            {
                Buffer.BlockCopy(m_dataBuffer, m_dataBufferOffset, m_dataBuffer, 0, dataBufferLength);
                Buffer.BlockCopy(data, offset, m_dataBuffer, dataBufferLength, dataLength);

                m_dataBufferOffset = 0;
                m_dataBufferEnd = neededLength;
            }

            // Buffer is not large enough: reallocate.
            if (m_dataBuffer != null && m_dataBuffer.Length < neededLength)
            {
                // Assume that the buffer will grow rather than shrink.
                // Hopefully, this reduces the need for reallocation.
                desiredLength = (int)(neededLength * 1.2);

                temp = new byte[desiredLength];
                Buffer.BlockCopy(m_dataBuffer, m_dataBufferOffset, temp, 0, dataBufferLength);
                Buffer.BlockCopy(data, offset, temp, dataBufferLength, dataLength);

                m_dataBuffer = temp;
                m_dataBufferOffset = 0;
                m_dataBufferEnd = neededLength;
            }

            // Buffer does not exist: allocate.
            if (m_dataBuffer == null)
            {
                // Assume that the buffer will grow rather than shrink.
                // Hopefully, this reduces the need for reallocation.
                desiredLength = (int)(neededLength * 1.2);

                m_dataBuffer = new byte[desiredLength];
                Buffer.BlockCopy(data, offset, m_dataBuffer, dataBufferLength, dataLength);

                m_dataBufferOffset = 0;
                m_dataBufferEnd = dataLength;
            }
        }

        /// <summary>
        /// Decompresses enough bytes of data to fill up the <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The buffer that holds the data.</param>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> cannot be null.</exception>
        /// <exception cref="ArgumentException"><paramref name="buffer"/> length must be a multiple of four.</exception>
        public void Decompress(byte[] buffer)
        {
            Decompress(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Decompresses <paramref name="length"/> bytes of data and places it in the <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The buffer that holds the data.</param>
        /// <param name="length">The amount of data to be decompressed and written to the buffer. The value of this parameter must be a multiple of four.</param>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> cannot be null.</exception>
        /// <exception cref="ArgumentException"><paramref name="length"/> must be a multiple of four.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> must be greater than or equal to zero.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> exceeds <paramref name="buffer"/> array boundaries.</exception>
        public void Decompress(byte[] buffer, int length)
        {
            Decompress(buffer, 0, length);
        }

        /// <summary>
        /// Decompresses <paramref name="length"/> bytes of data and places it in the <paramref name="buffer"/> starting at <paramref name="offset"/>.
        /// </summary>
        /// <param name="buffer">The buffer that holds the data.</param>
        /// <param name="offset">The amount of data at the beginning of the buffer that will not be overwritten.</param>
        /// <param name="length">The amount of data to be decompressed and written to the buffer. The value of this parameter must be a multiple of four.</param>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> cannot be null.</exception>
        /// <exception cref="ArgumentException"><paramref name="length"/> must be a multiple of four.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> must be greater than or equal to zero.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> must be greater than or equal to zero.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> exceeds <paramref name="buffer"/> array boundaries.</exception>
        public unsafe void Decompress(byte[] buffer, int offset, int length)
        {
            const int SizeOf32Bits = sizeof(uint);
            byte* start, iter, end;

            if ((object)buffer == null)
                throw new ArgumentNullException(nameof(buffer), "Cannot write data to null buffer");

            if (length % SizeOf32Bits != 0)
                throw new ArgumentException("Buffer length must be a multiple of four", nameof(length));

            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), "offset must be greater than or equal to zero");

            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), "length must be greater than or equal to zero");

            if (buffer.Length < offset + length)
                throw new ArgumentOutOfRangeException(nameof(length), "length exceeds buffer array boundaries");

            fixed (byte* pBuffer = buffer)
            {
                start = pBuffer + offset;
                end = start + length;

                for (iter = start; iter < end; iter += 4)
                    *(uint*)iter = DecompressValue();
            }
        }

        /// <summary>
        /// Decompresses eight bytes of data and writes the data into a 64-bit floating point number.
        /// </summary>
        /// <param name="value">The 64-bit floating point number that the data is to be written to.</param>
        public unsafe void Decompress(out double value)
        {
            fixed (double* pValue = &value)
            {
                // High word and low word on big endian systems
                uint* highWord = (uint*)pValue;
                uint* lowWord = highWord + 1;
                *highWord = DecompressValue();
                *lowWord = DecompressValue();
            }
        }

        /// <summary>
        /// Decompresses four bytes of data and writes the data into a 32-bit floating point number.
        /// </summary>
        /// <param name="value">The 32-bit floating point number that the data is to be written to.</param>
        public unsafe void Decompress(out float value)
        {
            fixed (float* pValue = &value)
                *(uint*)pValue = DecompressValue();
        }

        /// <summary>
        /// Decompresses eight bytes of data and writes the data into a 64-bit signed integer.
        /// </summary>
        /// <param name="value">The 64-bit integer that the data is to be written to.</param>
        public unsafe void Decompress(out long value)
        {
            fixed (long* pValue = &value)
            {
                // High word and low word on big endian systems
                uint* highWord = (uint*)pValue;
                uint* lowWord = highWord + 1;
                *highWord = DecompressValue();
                *lowWord = DecompressValue();
            }
        }

        /// <summary>
        /// Decompresses eight bytes of data and writes the data into a 64-bit unsigned integer.
        /// </summary>
        /// <param name="value">The 64-bit integer that the data is to be written to.</param>
        public unsafe void Decompress(out ulong value)
        {
            fixed (ulong* pValue = &value)
            {
                // High word and low word on big endian systems
                uint* highWord = (uint*)pValue;
                uint* lowWord = highWord + 1;
                *highWord = DecompressValue();
                *lowWord = DecompressValue();
            }
        }

        /// <summary>
        /// Decompresses four bytes of data and writes the data into a 32-bit signed integer.
        /// </summary>
        /// <param name="value">The 32-bit integer that the data is to be written to.</param>
        public unsafe void Decompress(out int value)
        {
            fixed (int* pValue = &value)
                *(uint*)pValue = DecompressValue();
        }

        /// <summary>
        /// Decompresses four bytes of data and writes the data into a 32-bit unsigned integer.
        /// </summary>
        /// <param name="value">The 32-bit integer that the data is to be written to.</param>
        public void Decompress(out uint value)
        {
            value = DecompressValue();
        }

        /// <summary>
        /// Decompresses four bytes of data and writes the data into a 16-bit signed integer. The high-order bytes are discarded.
        /// </summary>
        /// <param name="value">The 16-bit integer that the data is to be written to.</param>
        public void Decompress(out short value)
        {
            value = (short)DecompressValue();
        }

        /// <summary>
        /// Decompresses four bytes of data and writes the data into a 16-bit unsigned integer. The high-order bytes are discarded.
        /// </summary>
        /// <param name="value">The 16-bit integer that the data is to be written to.</param>
        public void Decompress(out ushort value)
        {
            value = (ushort)DecompressValue();
        }

        /// <summary>
        /// Decompresses four bytes of data and writes the data into an 8-bit integer. The high-order bytes are discarded.
        /// </summary>
        /// <param name="value">The 8-bit integer that the data is to be written to.</param>
        public void Decompress(out byte value)
        {
            value = (byte)DecompressValue();
        }

        /// <summary>
        /// Resets the decompressor by flushing the back buffer. Subsequent calls
        /// to the Decompress methods will not be decompressed using previously
        /// decompressed values.
        /// </summary>
        public void Reset()
        {
            m_backBufferStart = 0;
            m_backBufferLength = 0;
        }

        /// <summary>
        /// Clears out the data buffer so that subsequent calls to the Decompress
        /// methods do not use the data that was previously in the data buffer.
        /// </summary>
        /// <remarks>
        /// This method should not be called during normal operation, as data will
        /// be lost and the compressed stream will likely be corrupted. Only call
        /// this method if the stream has already been corrupted.
        /// </remarks>
        public void EmptyBuffer()
        {
            m_dataBufferOffset = 0;
            m_dataBufferEnd = 0;
        }

        // Decompresses four bytes and returns them as a 32-bit unsigned integer.
        private uint DecompressValue()
        {
            const int KeyMask = 0xE0;
            const int Uncompressed = 0xE0;
            const int ResetStream = 0xC0;

            byte decompressionKey;
            uint decompressedValue;

            if (DataBufferLength < 1)
                throw new InvalidOperationException("Unable to decompress: insufficient data in the buffer. Augment the data buffer.");

            decompressionKey = m_dataBuffer[m_dataBufferOffset];

            if ((decompressionKey & KeyMask) == Uncompressed)
                throw new NotSupportedException("Stream decompression of uncompressed data");

            if ((decompressionKey & KeyMask) == ResetStream)
            {
                // Decompression key indicates that this is
                // the first, uncompressed value in the stream
                Reset();
                decompressedValue = GetFirstValue();
            }
            else
            {
                // Decompression key indicates that this value is compressed
                decompressedValue = GetDecompressedValue();
            }

            // Insert the decompressed value into
            // the back buffer for future lookups
            InsertIntoBackBuffer(decompressedValue);

            return decompressedValue;
        }

        // Gets the first four data bytes in the stream and returns them as a 32-bit unsigned integer.
        private unsafe uint GetFirstValue()
        {
            // Size of the first value includes the 1-byte stream header
            const int FirstValueSize = 1 + sizeof(uint);
            const int CompressionStrengthMask = 0x1F;

            byte* iter;

            if (DataBufferLength < FirstValueSize)
                throw new InvalidOperationException("Unable to decompress: insufficient data in the buffer. Augment the data buffer.");

            fixed (byte* pDataBuffer = m_dataBuffer)
            {
                // Move the iterator up to the start of the data
                iter = pDataBuffer + m_dataBufferOffset;

                // Compression strength is found in the decompression key.
                // This is necessary to properly initialize the back buffer.
                CompressionStrength = (byte)(*iter & CompressionStrengthMask);

                // Advance the buffer offset
                m_dataBufferOffset += FirstValueSize;

                // First value is uncompressed
                return *(uint*)(iter + 1);
            }
        }

        // Decompresses four bytes of data and returns them as a 32-bit unsigned integer.
        private unsafe uint GetDecompressedValue()
        {
            const int SizeOf32Bits = sizeof(uint);
            const int BackReferenceMask = 0x1F;

            byte* iter;
            byte decompressionKey;
            byte difference, backBufferIndex;

            byte* pValue;
            uint value;

            fixed (byte* pDataBuffer = m_dataBuffer)
            {
                // Move the iterator up to the start of the data
                iter = pDataBuffer + m_dataBufferOffset;
                pValue = (byte*)&value;

                // Get the decompression key for this value
                decompressionKey = *iter;
                difference = (byte)(decompressionKey >> 5);
                backBufferIndex = (byte)(decompressionKey & BackReferenceMask);
                iter++;

                if (DataBufferLength < 1 + difference)
                    throw new InvalidOperationException("Unable to decompress: insufficient data in the buffer. Augment the data buffer.");

                // Initialize the value with data from the back buffer
                value = m_backBuffer[backBufferIndex];

                // If bytes are in big endian order, then low order bytes are right most in memory so skip ahead
                if (!BitConverter.IsLittleEndian)
                    pValue += SizeOf32Bits - difference;

                // Overwrite low order bytes with compressed data
                for (int i = 0; i < difference; i++, pValue++, iter++)
                    *pValue = *iter;

                // Advance the buffer offset and return the decompressed value
                m_dataBufferOffset += 1 + difference;
                return value;
            }
        }

        // Inserts the value into the back buffer.
        private void InsertIntoBackBuffer(uint value)
        {
            if (m_backBufferLength < m_compressionStrength + 1)
            {
                m_backBuffer[m_backBufferLength] = value;
                m_backBufferLength++;
            }
            else
            {
                m_backBuffer[m_backBufferStart] = value;
                m_backBufferStart++;

                if (m_backBufferStart >= m_backBufferLength)
                    m_backBufferStart = 0;
            }
        }

        #endregion

        #region [ Static ]

        // Static Methods

        /// <summary>
        /// Given the size of a compressed buffer, provides the maximum possible size of the decompressed data.
        /// </summary>
        /// <param name="compressedLength">Size of the compressed buffer.</param>
        /// <returns>The maximum possible size of the data after decompression.</returns>
        public static int MaximumSizeDecompressed(int compressedLength)
        {
            return 4 * compressedLength;
        }

        /// <summary>
        /// Decompress a byte array containing a sequential list of compressed 32-bit structures (e.g., floating point numbers, integers or unsigned integers) using a patterned compression method.
        /// </summary>
        /// <param name="source">The <see cref="Byte"/> array containing compressed 32-bit values to be decompressed. Decompression will happen inline on this buffer.</param>
        /// <param name="startIndex">An <see cref="Int32"/> representing the start index of the byte array.</param>
        /// <param name="dataLength">The number of bytes in the buffer that represents actual data.</param>
        /// <param name="bufferLength">The number of bytes available for use in the buffer; actual buffer length must be at least large enough to fit the maximum size of the decompressed data. See <see cref="MaximumSizeDecompressed"/>.</param>
        /// <returns>The new length of the buffer after compression, unless the data cannot be compressed. If the data cannot be compressed, the buffer will remain unchanged and zero will be returned.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> buffer cannot be null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="dataLength"/> must be greater than or equal to one.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="bufferLength"/> must be at least as large as <paramref name="dataLength"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="bufferLength"/> must be at least as large as is necessary to fit the maximum possible size of the decompressed data.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Actual length of <paramref name="source"/> buffer is less than specified <paramref name="bufferLength"/>.</exception>
        /// <remarks>
        /// <para>
        /// Decompression is performed inline. The source buffer must be large enough
        /// to contain the maximum possible size of the decompressed buffer. This
        /// maximum size can be obtained by using the <see cref="MaximumSizeDecompressed"/>
        /// method.
        /// </para>
        /// 
        /// <para>
        /// As an optimization this function is using pointers to native structures,
        /// as such the endian order decoding and encoding of the values will always
        /// be in the native endian order of the operating system.
        /// </para>
        /// </remarks>
        public static unsafe int DecompressBuffer(byte[] source, int startIndex, int dataLength, int bufferLength)
        {
            const int SizeOf32Bits = sizeof(uint);
            const int BackReferenceMask = 0x1F;

            int maxSize = MaximumSizeDecompressed(dataLength);
            byte compressionStrength = 5;

            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (dataLength < 1)
                throw new ArgumentOutOfRangeException(nameof(dataLength), "Data length must be greater than or equal to one");

            if (bufferLength < dataLength)
                throw new ArgumentOutOfRangeException(nameof(bufferLength), "Buffer length must be at least as large as original data length");

            if (bufferLength < maxSize)
                throw new ArgumentOutOfRangeException(nameof(bufferLength), "Buffer length must be at least as large as is necessary to fit the maximum possible size of the decompressed data");

            if (source.Length < bufferLength)
                throw new ArgumentOutOfRangeException(nameof(source), "Actual length of source buffer is less than specified buffer length");

            if (source[0] < 0xE0)
            {
                // Data is compressed. Get the compression strength
                compressionStrength = (byte)(source[0] & BackReferenceMask);
            }
            else
            {
                // Data is uncompressed. Remove the header and return
                Buffer.BlockCopy(source, startIndex + 1, source, startIndex, dataLength - 1);
                return dataLength - 1;
            }

            byte[] buffer = null;
            int maxQueueLength = compressionStrength + 1;
            uint* queue = stackalloc uint[maxQueueLength];
            int queueLength = 0;
            int usedLength = 0;
            int queueStartIndex = 0;

            // Initialize a buffer large enough to fit maximum possible decompressed size
            buffer = new byte[maxSize];

            // Pin buffers to be navigated so that .NET doesn't move them around
            fixed (byte* pSource = source, pBuffer = buffer)
            {
                byte* sourceIndex = pSource + 1;
                byte* bufferIndex = pBuffer;

                uint firstValue = *(uint*)sourceIndex;

                // Always add first value to the buffer and the back buffer as-is
                *(uint*)bufferIndex = firstValue;
                queue[0] = firstValue;
                queueLength++;

                // Advance pointers
                bufferIndex += SizeOf32Bits;
                sourceIndex += SizeOf32Bits;

                // Starting with second item, begin decompression sequence
                while ((int)(sourceIndex - pSource) < dataLength)
                {
                    byte* workingIndex = bufferIndex;
                    byte decompressionKey = 0;
                    byte backReferenceIndex = 0;
                    int smallestDifference = 0;

                    // Get decompression key for the next value
                    decompressionKey = *sourceIndex;
                    sourceIndex++;

                    // Obtain smallest difference from bits 5 through 7 and back reference index from bits 0 through 4
                    smallestDifference = decompressionKey >> 5;
                    backReferenceIndex = (byte)(decompressionKey & BackReferenceMask);

                    // Ensure that we have enough remaining data in the source buffer to decompress the next value
                    if ((int)(sourceIndex - pSource) + smallestDifference > dataLength)
                        throw new IndexOutOfRangeException("Source buffer does not end on a value boundary");

                    // Copy value from back buffer
                    *(uint*)workingIndex = queue[backReferenceIndex];

                    // If bytes are in big endian order, then low order bytes are right most in memory so skip ahead
                    if (!BitConverter.IsLittleEndian)
                        workingIndex += SizeOf32Bits - smallestDifference;

                    // Add only needed bytes to the output buffer (maybe none!)
                    for (int j = 0; j < smallestDifference; j++, workingIndex++, sourceIndex++)
                    {
                        *workingIndex = *sourceIndex;
                    }

                    // Place decompressed value into the queue
                    if (queueLength < maxQueueLength)
                    {
                        queue[queueLength] = *(uint*)bufferIndex;
                        queueLength++;
                    }
                    else
                    {
                        queue[queueStartIndex] = *(uint*)bufferIndex;
                        queueStartIndex++;

                        if (queueStartIndex >= queueLength)
                            queueStartIndex = 0;
                    }

                    // Setup to decompress the next value
                    bufferIndex += SizeOf32Bits;
                }

                usedLength = (int)(bufferIndex - pBuffer);

                // Overwrite source buffer with new compressed buffer
                Buffer.BlockCopy(buffer, 0, source, startIndex, usedLength);
            }

            return usedLength;
        }

        #endregion

        ///// <summary>
        ///// Decompresses a series of bytes and parses a binary image using the decompressed data.
        ///// </summary>
        ///// <param name="value">The object whose binary image is to be decompressed and parsed.</param>
        //public unsafe void Decompress(ISupportBinaryImage value)
        //{
        //    int binaryLength = value.BinaryLength;
        //    int bufferLength = Word.AlignDoubleWord(binaryLength);
        //    byte[] buffer = new byte[bufferLength];
        //    byte* iter, end;

        //    fixed (byte* start = buffer)
        //    {
        //        end = start + bufferLength;

        //        for (iter = start; iter < end; iter += 4)
        //            *(uint*)iter = DecompressValue();
        //    }

        //    value.ParseBinaryImage(buffer, 0, binaryLength);
        //}

    }
}
