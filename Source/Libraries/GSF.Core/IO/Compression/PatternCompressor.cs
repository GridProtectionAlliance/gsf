//******************************************************************************************************
//  PatternCompressor.cs - Gbtc
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
//  10/27/2011 - J. Ritchie Carroll
//       Initial version of source generated.
//  01/25/2012 - Stephen C. Wills
//       Removed 64-bit compression routine, added 32-bit decompression routine.
//  01/26/2012 - Stephen C. Wills
//       Changed class to non-static so that compressor instances
//       can be created to handle stream compression.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using GSF.Parsing;

namespace GSF.IO.Compression
{
    /// <summary>
    /// Defines functions used for high-speed pattern compression against native types.
    /// </summary>
    public class PatternCompressor
    {
        #region [ Members ]

        // Fields
        private byte[] m_compressedBuffer;
        private int m_compressedBufferLength;
        private int m_maxCompressedBufferLength;

        private uint[] m_backBuffer;
        private int m_backBufferStart;
        private int m_backBufferLength;

        private byte m_compressionStrength;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the PatternCompressor with the given compression strength.
        /// </summary>
        /// <param name="compressionStrength">The compression strength of the PatternCompressor.</param>
        public PatternCompressor(byte compressionStrength = 5)
        {
            CompressionStrength = compressionStrength;
            m_maxCompressedBufferLength = -1;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the buffer into which compressed values are written.
        /// </summary>
        /// <remarks>
        /// When setting the compressed buffer, the new buffer is assumed to be empty
        /// (<see cref="CompressedBufferLength"/> is set to zero).
        /// </remarks>
        public byte[] CompressedBuffer
        {
            get
            {
                return m_compressedBuffer;
            }
            set
            {
                m_compressedBuffer = value;
                m_compressedBufferLength = 0;
            }
        }

        /// <summary>
        /// Gets the amount of compressed data in the compressed buffer.
        /// </summary>
        public int CompressedBufferLength
        {
            get
            {
                return m_compressedBufferLength;
            }
        }

        /// <summary>
        /// Gets or sets the maximum compressed buffer length.
        /// </summary>
        /// <remarks>
        /// The compressor will not write beyond the MaxCompressedBufferLength.
        /// Defaults to the full length of the compressed buffer.
        /// Set to -1 to return to the default.
        /// </remarks>
        public int MaxCompressedBufferLength
        {
            get
            {
                if (m_maxCompressedBufferLength > 0)
                    return m_maxCompressedBufferLength;

                if (m_compressedBuffer != null)
                    return m_compressedBuffer.Length;

                return 0;
            }
            set
            {
                m_maxCompressedBufferLength = value;
            }
        }

        /// <summary>
        /// Gets or sets the compression strength of the PatternCompressor.
        /// </summary>
        /// <remarks>
        /// When setting the compression strength, the PatternCompressor
        /// is automatically reset (the back buffer is emptied).
        /// </remarks>
        public byte CompressionStrength
        {
            get
            {
                return m_compressionStrength;
            }
            set
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

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Compresses the given value and places it in the compressed buffer.
        /// </summary>
        /// <param name="value">The value to be compressed.</param>
        /// <returns>The size, in bytes, of the compressed value.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> cannot be null.</exception>
        public unsafe int Compress(ISupportBinaryImage value)
        {
            byte[] buffer;
            int bufferLength;

            int compressedSize = 0;
            byte* iter, end;

            if ((object)value == null)
                throw new ArgumentNullException(nameof(value));

            bufferLength = value.BinaryLength.AlignDoubleWord();
            buffer = new byte[bufferLength];

            fixed (byte* start = buffer)
            {
                value.GenerateBinaryImage(buffer, 0);
                end = start + bufferLength;

                for (iter = start; iter < end; iter += 4)
                    compressedSize += Compress(iter);
            }

            return compressedSize;
        }

        /// <summary>
        /// Compresses all of the data in the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The buffer to be compressed.</param>
        /// <returns>The size, in bytes, of the compressed value.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> cannot be null.</exception>
        /// <exception cref="ArgumentException"><paramref name="buffer"/> length must be a multiple of four.</exception>
        public int Compress(byte[] buffer)
        {
            return Compress(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Compresses <paramref name="length"/> bytes of data in the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The buffer to be compressed.</param>
        /// <param name="length">The amount of data to be compressed. Must be a multiple of four.</param>
        /// <returns>The size, in bytes, of the compressed value.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> cannot be null.</exception>
        /// <exception cref="ArgumentException"><paramref name="length"/> must be a multiple of four.</exception>]
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> must be greater than or equal to zero.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> exceeds <paramref name="buffer"/> array boundaries</exception>
        public int Compress(byte[] buffer, int length)
        {
            return Compress(buffer, 0, length);
        }

        /// <summary>
        /// Compresses <paramref name="length"/> bytes of data in the given <paramref name="buffer"/>, starting at <paramref name="offset"/>.
        /// </summary>
        /// <param name="buffer">The buffer to be compressed.</param>
        /// <param name="offset">The amount of data to ignore at the start of the buffer.</param>
        /// <param name="length">The amount of data to be compressed. Must be a multiple of four.</param>
        /// <returns>The size, in bytes, of the compressed value.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> cannot be null.</exception>
        /// <exception cref="ArgumentException"><paramref name="length"/> must be a multiple of four.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> must be greater than or equal to zero.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> must be greater than or equal to zero.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> exceeds <paramref name="buffer"/> array boundaries</exception>
        public unsafe int Compress(byte[] buffer, int offset, int length)
        {
            const int SizeOf32Bits = sizeof(uint);
            int compressedSize = 0;
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
                    compressedSize += Compress(iter);
            }

            return compressedSize;
        }

        /// <summary>
        /// Compresses the given value and places it in the compressed buffer.
        /// </summary>
        /// <param name="value">The value to be compressed.</param>
        /// <returns>The size, in bytes, of the compressed value.</returns>
        public unsafe int Compress(double value)
        {
            uint* pItem = (uint*)&value;
            int compressedSize = 0;

            compressedSize += Compress((byte*)pItem);
            compressedSize += Compress((byte*)(pItem + 1));

            return compressedSize;
        }

        /// <summary>
        /// Compresses the given value and places it in the compressed buffer.
        /// </summary>
        /// <param name="value">The value to be compressed.</param>
        /// <returns>The size, in bytes, of the compressed value.</returns>
        public unsafe int Compress(float value)
        {
            return Compress((byte*)&value);
        }

        /// <summary>
        /// Compresses the given value and places it in the compressed buffer.
        /// </summary>
        /// <param name="value">The value to be compressed.</param>
        /// <returns>The size, in bytes, of the compressed value.</returns>
        public unsafe int Compress(long value)
        {
            uint* pItem = (uint*)&value;
            int compressedSize = 0;

            compressedSize += Compress((byte*)pItem);
            compressedSize += Compress((byte*)(pItem + 1));

            return compressedSize;
        }

        /// <summary>
        /// Compresses the given value and places it in the compressed buffer.
        /// </summary>
        /// <param name="value">The value to be compressed.</param>
        /// <returns>The size, in bytes, of the compressed value.</returns>
        public unsafe int Compress(ulong value)
        {
            uint* pItem = (uint*)&value;
            int compressedSize = 0;

            compressedSize += Compress((byte*)pItem);
            compressedSize += Compress((byte*)(pItem + 1));

            return compressedSize;
        }

        /// <summary>
        /// Compresses the given value and places it in the compressed buffer.
        /// </summary>
        /// <param name="value">The value to be compressed.</param>
        /// <returns>The size, in bytes, of the compressed value.</returns>
        public unsafe int Compress(int value)
        {
            return Compress((byte*)&value);
        }

        /// <summary>
        /// Compresses the given value and places it in the compressed buffer.
        /// </summary>
        /// <param name="value">The value to be compressed.</param>
        /// <returns>The size, in bytes, of the compressed value.</returns>
        public unsafe int Compress(uint value)
        {
            return Compress((byte*)&value);
        }

        /// <summary>
        /// Compresses the given value and places it in the compressed buffer.
        /// </summary>
        /// <param name="value">The value to be compressed.</param>
        /// <returns>The size, in bytes, of the compressed value.</returns>
        public int Compress(short value)
        {
            return Compress((int)value);
        }

        /// <summary>
        /// Compresses the given value and places it in the compressed buffer.
        /// </summary>
        /// <param name="value">The value to be compressed.</param>
        /// <returns>The size, in bytes, of the compressed value.</returns>
        public int Compress(ushort value)
        {
            return Compress((uint)value);
        }

        /// <summary>
        /// Compresses the given value and places it in the compressed buffer.
        /// </summary>
        /// <param name="value">The value to be compressed.</param>
        /// <returns>The size, in bytes, of the compressed value.</returns>
        public int Compress(byte value)
        {
            return Compress((uint)value);
        }

        /// <summary>
        /// Sets <see cref="CompressedBufferLength"/> to zero and allows the
        /// compressor to write over the values in the compressed buffer.
        /// </summary>
        public void EmptyCompressedBuffer()
        {
            m_compressedBufferLength = 0;
        }

        /// <summary>
        /// Resets the compressor by flushing the back buffer. Subsequent calls
        /// to the Compress methods will not be compressed using previously
        /// compressed values.
        /// </summary>
        public void Reset()
        {
            m_backBufferStart = 0;
            m_backBufferLength = 0;
        }

        // Helper method to compress a generic set of four
        // bytes, given a pointer to the first byte.
        private unsafe int Compress(byte* pValue)
        {
            uint value = *(uint*)pValue;
            int compressedSize;

            if (m_compressedBuffer == null)
                throw new InvalidOperationException("Cannot write compressed value to a null buffer");

            if (m_backBufferLength == 0)
                compressedSize = InsertFirstValue(value);
            else
                compressedSize = InsertCompressedValue(value);

            InsertIntoBackBuffer(value);
            return compressedSize;
        }

        // Inserts the given value into the compressed buffer as the first,
        // uncompressed value in the stream. This is called when the back
        // buffer is empty.
        private unsafe int InsertFirstValue(uint value)
        {
            // Size of the first value includes the 1-byte stream header
            const int FirstValueSize = 1 + sizeof(uint);

            // Ensure that the value will fit in the compressed buffer
            if (MaxCompressedBufferLength - m_compressedBufferLength < FirstValueSize)
                throw new InvalidOperationException("Value cannot fit in compressed buffer. Empty compressed buffer.");

            // Insert value into compressed buffer
            fixed (byte* pCompressedBuffer = m_compressedBuffer)
            {
                byte* compressedBufferEnd = pCompressedBuffer + m_compressedBufferLength;
                *compressedBufferEnd = (byte)(0xC0 | m_compressionStrength);
                *(uint*)(compressedBufferEnd + 1) = value;
                m_compressedBufferLength += FirstValueSize;
            }

            return FirstValueSize;
        }

        // Compresses the given value and inserts it into the compressed buffer.
        private unsafe int InsertCompressedValue(uint value)
        {
            byte* compressedBufferEnd;

            byte* pValue = (byte*)&value;
            byte decompressionKey = GetDecompressionKey(value);
            byte difference = (byte)(decompressionKey >> 5);

            int compressedSize = 1 + difference;

            if (MaxCompressedBufferLength - m_compressedBufferLength < compressedSize)
                throw new InvalidOperationException("Value cannot fit in compressed buffer. Empty compressed buffer.");

            fixed (byte* pCompressedBuffer = m_compressedBuffer)
            {
                compressedBufferEnd = pCompressedBuffer + m_compressedBufferLength;
                *compressedBufferEnd = decompressionKey;
                compressedBufferEnd++;

                if (!BitConverter.IsLittleEndian)
                    pValue += sizeof(uint) - difference;

                for (int i = 0; i < difference; i++, compressedBufferEnd++, pValue++)
                    *compressedBufferEnd = *pValue;

                m_compressedBufferLength += compressedSize;
            }

            return compressedSize;
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

        // Gets the 8-bit decompression key for the given value
        // based on the values that are in the back buffer.
        private unsafe byte GetDecompressionKey(uint value)
        {
            uint* backBufferIterator;

            uint result;
            byte difference;

            byte smallestDifference = 5;
            int backBufferIndex = 0;

            // Pin back buffer to navigate using pointer
            fixed (uint* pBackBuffer = m_backBuffer)
            {
                backBufferIterator = pBackBuffer;

                // Compare item with each value in the back buffer to find the smallest difference
                for (int i = 0; i < m_backBufferLength; i++, backBufferIterator++)
                {
                    result = value ^ *backBufferIterator;

                    if (result > 0xFFFFFFu)
                        difference = 4;
                    else if (result > 0xFFFFu)
                        difference = 3;
                    else if (result > 0xFFu)
                        difference = 2;
                    else if (result > 0u)
                        difference = 1;
                    else
                        difference = 0;

                    if (difference < smallestDifference)
                    {
                        smallestDifference = difference;
                        backBufferIndex = i;
                    }
                }
            }

            // Return decompression key:
            //  backBufferIndex in bits 0 through 4
            //  smallestDifference in bits 5 through 7
            return (byte)((smallestDifference << 5) | backBufferIndex);
        }

        #endregion

        #region [ Static ]

        // Static Methods

        /// <summary>
        /// Compress a byte array containing a sequential list of 32-bit structures (e.g., floating point numbers, integers or unsigned integers) using a patterned compression method.
        /// </summary>
        /// <param name="source">The <see cref="Byte"/> array containing 32-bit values to compress. Compression will happen inline on this buffer.</param>
        /// <param name="startIndex">An <see cref="Int32"/> representing the start index of the byte array.</param>
        /// <param name="dataLength">The number of bytes in the buffer that represents actual data.</param>
        /// <param name="bufferLength">The number of bytes available for use in the buffer; actual buffer length must be at least one byte larger than <paramref name="dataLength"/> since it's possible that data cannot be compressed. This extra byte will be used indicate an uncompressed buffer.</param>
        /// <param name="compressionStrength">Specifies compression strength (0 to 31). Smaller numbers will run faster, larger numbers will yield better compression.</param>
        /// <returns>The new length of the buffer after compression.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> buffer cannot be null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="dataLength"/> must be greater than or equal to zero.</exception>
        /// <exception cref="ArgumentException"><paramref name="dataLength"/> must be an even multiple of 4.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="bufferLength"/> must be at least one byte larger than <paramref name="dataLength"/> in case data cannot be compressed.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Actual length of <paramref name="source"/> buffer is less than specified <paramref name="bufferLength"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="compressionStrength"/> must be 0 to 31.</exception>
        /// <remarks>
        /// As an optimization this function is using pointers to native structures, as such the endian order decoding and encoding of the values will always be in the native endian order of the operating system.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public static unsafe int CompressBuffer(byte[] source, int startIndex, int dataLength, int bufferLength, byte compressionStrength = 5)
        {
            const int SizeOf32Bits = sizeof(uint);

            // Queue length of 32 forces reservation of 3 bits on single byte decompression key to allow for back track indices of 0 to 31, this
            // means minimal compression size of 4 total bytes would be 1 byte (i.e., 1 byte for decompression key), or max of 75% compression.
            // Compression algorithm is best suited for data that differs fractionally over time (e.g., 60.05, 60.08, 60.09, 60.11...)

            if ((object)source == null)
                throw new ArgumentNullException(nameof(source));

            if (dataLength < 0)
                throw new ArgumentOutOfRangeException(nameof(dataLength), "Data length must be greater than or equal to zero");

            if (dataLength % SizeOf32Bits != 0)
                throw new ArgumentException("Data length must be a multiple of 4", nameof(dataLength));

            if (bufferLength < dataLength + 1)
                throw new ArgumentOutOfRangeException(nameof(bufferLength), "Buffer length must be at least one byte larger than original data length in case data cannot be compressed");

            if (source.Length < bufferLength)
                throw new ArgumentOutOfRangeException(nameof(source), "Actual length of source buffer is less than specified buffer length");

            if (compressionStrength > 31)
                throw new ArgumentOutOfRangeException(nameof(compressionStrength), "Compression strength must be 0 to 31");

            // Special case when there is not enough data to be able to compress
            if (dataLength <= 4)
            {
                Buffer.BlockCopy(source, startIndex, source, startIndex + 1, dataLength);
                source[0] = (byte)(0xE0 | compressionStrength);
                return dataLength + 1;
            }

            byte[] buffer = null;
            int maxQueueLength = compressionStrength + 1;
            uint* queue = stackalloc uint[maxQueueLength];
            int queueLength = 0;
            int usedLength;
            int count = dataLength / SizeOf32Bits;
            int queueStartIndex = 0;

            // Note that maximum zero compression size would be size of all original values plus one byte for each value
            buffer = new byte[dataLength + count];

            // Pin buffers to be navigated so that .NET doesn't move them around
            fixed (byte* pSource = source, pBuffer = buffer)
            {
                byte* bufferIndex = pBuffer;
                uint* values = (uint*)pSource;

                // Reserve initial byte for compression header
                *bufferIndex = compressionStrength;
                bufferIndex++;

                // Always add first value to the buffer as-is
                *(uint*)bufferIndex = *values;
                bufferIndex += SizeOf32Bits;

                // Initialize first set of queue values for back reference
                for (int i = 0; i < (count < maxQueueLength ? count : maxQueueLength); i++, values++, queueLength++)
                {
                    queue[i] = *values;
                }

                // Reset values collection pointer starting at second item
                values = (uint*)pSource;
                values++;

                // Starting with second item, begin compression sequence
                for (int index = 1; index < count; index++)
                {
                    uint test, current = *values;
                    byte backReferenceIndex = 0;
                    int smallestDifference = SizeOf32Bits;
                    int queueIndex = queueStartIndex;

                    // Test each item in back reference queue for best compression
                    for (int i = 0; i < (index < queueLength ? index : queueLength); i++)
                    {
                        int difference;

                        // Get first item from queue
                        test = queue[queueIndex];

                        // Xor current value and queue value (interpreted as integers) for total byte differences
                        uint result = current ^ test;

                        if (result > 0xFFFFFFu)
                            difference = 4; // Value differs by 4 bytes
                        else if (result > 0xFFFFu)
                            difference = 3; // Value differs by 3 bytes
                        else if (result > 0xFFu)
                            difference = 2; // Value differs by 2 bytes
                        else if (result > 0u)
                            difference = 1; // Value differs by 1 bytes
                        else
                            difference = 0; // Value differs by 0 bytes

                        // Item with the smallest difference in the back reference queue wins
                        if (difference < smallestDifference)
                        {
                            smallestDifference = difference;
                            backReferenceIndex = (byte)queueIndex;

                            // No need to check further if we've found a full match on all possible bytes
                            if (smallestDifference == 0)
                                break;
                        }

                        queueIndex++;

                        if (queueIndex >= queueLength)
                            queueIndex = 0;
                    }

                    // Calculate key that will be needed for proper decompression, that is: byte difference
                    // in bits 5 through 7 and the back reference xor value index in bits 0 through 4
                    byte decompressionKey = (byte)((byte)(smallestDifference << 5) | backReferenceIndex);

                    // Add decompression key to output buffer
                    *bufferIndex = decompressionKey;
                    bufferIndex++;

                    // Get a pointer to the best compression result
                    byte* pValues = (byte*)values;

                    // If desired bytes are in big endian order, then they are right most in memory so skip ahead
                    if (!BitConverter.IsLittleEndian)
                        pValues += SizeOf32Bits - smallestDifference;

                    // Add only needed bytes to the output buffer (maybe none!)
                    for (int j = 0; j < smallestDifference; j++, bufferIndex++, pValues++)
                    {
                        *bufferIndex = *pValues;
                    }

                    // After initial queue values, add newest item to the queue, replacing the old one
                    if (index >= queueLength)
                    {
                        queue[queueStartIndex] = current;

                        // Track oldest item in the queue as the starting location
                        queueStartIndex++;

                        if (queueStartIndex >= queueLength)
                            queueStartIndex = 0;
                    }

                    // Setup to compress the next value
                    values++;
                }

                usedLength = (int)(bufferIndex - pBuffer);

                // Check to see if we failed to compress data (hopefully rare)
                if (usedLength > dataLength)
                {
                    // Set compression buffer flags to uncompressed
                    *pBuffer |= (byte)0xE0;
                    Buffer.BlockCopy(source, startIndex, buffer, 1, dataLength);
                    usedLength = dataLength + 1;
                }

                // Overwrite source buffer with new compressed buffer
                Buffer.BlockCopy(buffer, 0, source, startIndex, usedLength);
            }

            return usedLength;
        }

        #endregion

        ///// <summary>
        ///// Compress a byte array containing a sequential list of 64-bit structures (e.g., double precision floating point numbers, long integers or unsigned long integers) using a patterned compression method.
        ///// </summary>
        ///// <param name="source">The <see cref="Byte"/> array containing 64-bit values to compress. Compression will happen inline on this buffer.</param>
        ///// <param name="startIndex">An <see cref="Int32"/> representing the start index of the byte array.</param>
        ///// <param name="dataLength">The number of bytes in the buffer that represents actual data.</param>
        ///// <param name="bufferLength">The number of bytes available for use in the buffer; actual buffer length must be at least as large as <paramref name="dataLength"/>.</param>
        ///// <param name="compressionStrength">Specifies compression strength (0 to 255). Smaller numbers will run faster, larger numbers will yield better compression.</param>
        ///// <returns>The new length of the buffer after compression.</returns>
        ///// <exception cref="ArgumentNullException"><paramref name="source"/> buffer cannot be null.</exception>
        ///// <exception cref="ArgumentOutOfRangeException"><paramref name="dataLength"/> must be greater than or equal to zero.</exception>
        ///// <exception cref="ArgumentException"><paramref name="dataLength"/> must be an even multiple of 8.</exception>
        ///// <exception cref="ArgumentOutOfRangeException"><paramref name="bufferLength"/> must be at least as large as <paramref name="dataLength"/>.</exception>
        ///// <exception cref="ArgumentOutOfRangeException">Actual length of <paramref name="source"/> buffer is less than specified <paramref name="bufferLength"/>.</exception>
        ///// <remarks>
        ///// As an optimization this function is using pointers to native structures, as such the endian order decoding and encoding of the values will always be in the native endian order of the operating system.
        ///// </remarks>
        //public unsafe static int Compress64bitEnumeration(this byte[] source, int startIndex, int dataLength, int bufferLength, byte compressionStrength = 5)
        //{
        //    const int SizeOf64Bits = sizeof(ulong);

        //    // Algorithm uses all 8-bits of decompression key plus 1 full byte for back track indices, 256 maximum, to yield 75% maximum compression.
        //    // Compression algorithm is best suited for data that differs fractionally over time (e.g., 60.05, 60.08, 60.09, 60.11...)
        //    if (source == null)
        //        throw new ArgumentNullException("source");

        //    if (dataLength < 0)
        //        throw new ArgumentOutOfRangeException("dataLength", "Data length must be greater than or equal to zero");

        //    if (dataLength % SizeOf64Bits != 0)
        //        throw new ArgumentException("Data length must be a multiple of 8", "dataLength");

        //    if (bufferLength < dataLength)
        //        throw new ArgumentOutOfRangeException("bufferLength", "Buffer length must be at least as large as original data length");

        //    if (source.Length < bufferLength)
        //        throw new ArgumentOutOfRangeException("source", "Actual length of source buffer is less than specified buffer length");

        //    byte[] buffer = null;
        //    int maxQueueLength = compressionStrength + 1;
        //    ulong* queue = stackalloc ulong[maxQueueLength];
        //    int queueLength = 0;
        //    int usedLength = 0;
        //    int count = dataLength / SizeOf64Bits;
        //    int queueStartIndex = 0;

        //    try
        //    {
        //        // Grab a working buffer from the pool, note that maximum zero compression size would be size of all original values plus two bytes for each value
        //        buffer = BufferPool.TakeBuffer(dataLength + 2 * count);

        //        // Pin buffers to be navigated so that .NET doesn't move them around
        //        fixed (byte* pSource = source, pBuffer = buffer)
        //        {
        //            byte* bufferIndex = pBuffer;
        //            ulong* values = (ulong*)pSource;

        //            // Always add first value to the buffer as-is
        //            *(ulong*)bufferIndex = *values;
        //            bufferIndex += SizeOf64Bits;

        //            // Initialize first set of queue values for back reference
        //            for (int i = 0; i < (count < maxQueueLength ? count : maxQueueLength); i++, values++, queueLength++)
        //            {
        //                queue[i] = *values;
        //            }

        //            // Reset values collection pointer starting at second item
        //            values = (ulong*)pSource;
        //            values++;

        //            // Starting with second item, begin compression sequence
        //            for (int index = 1; index < count; index++)
        //            {
        //                ulong test, current = *values;
        //                ulong bestResult = 0;
        //                byte backReferenceIndex = 0;
        //                int smallestDifference = SizeOf64Bits;
        //                int queueIndex = queueStartIndex;

        //                // Test each item in back reference queue for best compression
        //                for (int i = 0; i < (index < queueLength ? index : queueLength); i++)
        //                {
        //                    int difference;

        //                    // Get first item from queue
        //                    test = queue[queueIndex];

        //                    // Xor current value and queue value (interpreted as integers) for total byte differences
        //                    ulong result = current ^ test;

        //                    if ((result & 0xffffffffffffff) != result)
        //                        difference = 8; // Value differs by 8 bytes
        //                    else if ((result & 0xffffffffffff) != result)
        //                        difference = 7; // Value differs by 7 bytes
        //                    else if ((result & 0xffffffffff) != result)
        //                        difference = 6; // Value differs by 6 bytes
        //                    else if ((result & 0xffffffff) != result)
        //                        difference = 5; // Value differs by 5 bytes
        //                    else if ((result & 0xffffff) != result)
        //                        difference = 4; // Value differs by 4 bytes
        //                    else if ((result & 0xffff) != result)
        //                        difference = 3; // Value differs by 3 bytes
        //                    else if ((result & 0xff) != result)
        //                        difference = 2; // Value differs by 2 bytes
        //                    else if (result != 0)
        //                        difference = 1; // Value differs by 1 bytes
        //                    else
        //                        difference = 0; // Value differs by 0 bytes

        //                    // Item with the smallest difference in the back reference queue wins
        //                    if (difference < smallestDifference)
        //                    {
        //                        smallestDifference = difference;
        //                        backReferenceIndex = (byte)queueIndex;
        //                        bestResult = result;

        //                        // No need to check further if we've found a full match on all possible bytes
        //                        if (smallestDifference == 0)
        //                            break;
        //                    }

        //                    queueIndex++;

        //                    if (queueIndex >= queueLength)
        //                        queueIndex = 0;
        //                }

        //                //// Calculate key that will be needed for proper decompression
        //                //byte decompressionKey = (byte)(((uint)1 << smallestDifference) - 1);

        //                // Add decompression key to output buffer
        //                *bufferIndex = (byte)smallestDifference;
        //                bufferIndex++;

        //                // Add queue index to the output buffer
        //                *bufferIndex = backReferenceIndex;
        //                bufferIndex++;

        //                // Get a pointer to the best compression result
        //                byte* pResult = (byte*)&bestResult;

        //                // If desired bytes are in big endian order, then they are right most in memory so skip ahead
        //                if (!BitConverter.IsLittleEndian)
        //                    pResult += SizeOf64Bits - smallestDifference;

        //                // Add only needed bytes to the output buffer
        //                for (int j = 0; j < smallestDifference; j++, bufferIndex++, pResult++)
        //                {
        //                    *bufferIndex = *pResult;
        //                }

        //                // After initial queue values, add newest item to the queue, replacing the old one
        //                if (index >= queueLength)
        //                {
        //                    queue[queueStartIndex] = current;

        //                    // Track oldest item in the queue as the starting location
        //                    queueStartIndex++;

        //                    if (queueStartIndex >= queueLength)
        //                        queueStartIndex = 0;
        //                }

        //                // Setup to compress the next value
        //                values++;
        //            }

        //            usedLength = (int)(bufferIndex - pBuffer);

        //            // Check to see if we failed to compress data (hopefully rare)
        //            if (usedLength > dataLength)
        //                return 0;

        //            // Overwrite source buffer with new compressed buffer
        //            Buffer.BlockCopy(buffer, 0, source, startIndex, usedLength);
        //        }
        //    }
        //    finally
        //    {
        //        // Return buffer to queue so it can be reused
        //        if ((object)buffer != null)
        //            BufferPool.ReturnBuffer(buffer);
        //    }

        //    return usedLength;
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="values"></param>
        ///// <param name="endianOrder"></param>
        ///// <returns></returns>
        //public static byte[] Compress<T>(this IEnumerable<T> values, EndianOrder endianOrder = null) where T : struct
        //{
        //    if (values == null)
        //        throw new ArgumentNullException("values");

        //    int count = values.Count();

        //    if (count == 0)
        //        return new byte[0];

        //    if (endianOrder == null)
        //        endianOrder = NativeEndianOrder.Default;

        //    if (count == 1)
        //        return endianOrder.GetBytes(values.First());

        //    MemoryStream results = new MemoryStream();
        //    T value, lastValue = values.First();

        //    for (int i = 1; i < count; i++)
        //    {
        //        value = values.ElementAt(i);

        //    }

        //    return results.ToArray();
        //}
    }
}
