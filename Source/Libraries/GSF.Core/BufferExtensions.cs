//******************************************************************************************************
//  BufferExtensions.cs - Gbtc
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
//  09/19/2008 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/03/2008 - J. Ritchie Carroll
//       Added "Combine" and "IndexOfSequence" overloaded extensions.
//  02/13/2009 - Josh L. Patterson
//       Edited Code Comments.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/31/2009 - Andrew K. Hill
//       Modified the following methods per unit testing:
//       BlockCopy(byte[], int, int)
//       Combine(byte[], byte[])
//       Combine(byte[], int, int, byte[], int, int)
//       Combine(byte[][])
//       IndexOfSequence(byte[], byte[])
//       IndexOfSequence(byte[], byte[], int)
//       IndexOfSequence(byte[], byte[], int, int)
//  11/22/2011 - J. Ritchie Carroll
//       Added common case buffer parameter validation extensions.
//  10/8/2012 - Danyelle Gilliam
//        Modified Header
//
//******************************************************************************************************




#region [ Contributor License Agreements ]

//******************************************************************************************************
//
//  Copyright © 2011, Grid Protection Alliance.  All Rights Reserved.
//
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//******************************************************************************************************

#endregion

using System;
using System.IO;

namespace GSF
{
    /// <summary>Defines extension functions related to buffer manipulation.</summary>
    public static class BufferExtensions
    {
        /// <summary>
        /// Validates that the specified <paramref name="startIndex"/> and <paramref name="length"/> are valid within the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">Buffer to validate.</param>
        /// <param name="startIndex">0-based start index into the <paramref name="buffer"/>.</param>
        /// <param name="length">Valid number of bytes within <paramref name="buffer"/> from <paramref name="startIndex"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> or <paramref name="length"/> is less than 0 -or- 
        /// <paramref name="startIndex"/> and <paramref name="length"/> will exceed <paramref name="buffer"/> length.
        /// </exception>
        public static void ValidateParameters(this byte[] buffer, int startIndex, int length)
        {
            if ((object)buffer == null)
                throw new ArgumentNullException("buffer");

            if (startIndex < 0)
                throw new ArgumentOutOfRangeException("startIndex", "Buffer parameter cannot be negative");

            if (length < 0)
                throw new ArgumentOutOfRangeException("length", "Buffer parameter cannot be negative");

            if (startIndex + length > buffer.Length)
                throw new ArgumentOutOfRangeException("length", string.Format("Buffer operation with startIndex of {0} and length of {1} will exceed {2} bytes of available buffer space", startIndex, length, buffer.Length));
        }

        /// <summary>Returns a copy of the specified portion of the <paramref name="buffer"/> buffer.</summary>
        /// <param name="buffer">Source buffer.</param>
        /// <param name="startIndex">Offset into <paramref name="buffer"/> buffer.</param>
        /// <param name="length">Length of <paramref name="buffer"/> buffer to copy at <paramref name="startIndex"/> offset.</param>
        /// <returns>A buffer of data copied from the specified portion of the source buffer.</returns>
        /// <remarks>
        /// <para>
        /// Returned buffer will be extended as needed to make it the specified <paramref name="length"/>, but
        /// it will never be less than the source buffer length - <paramref name="startIndex"/>.
        /// </para>
        /// <para>
        /// This is a convenience function. If an existing buffer is already available, using the <see cref="Buffer.BlockCopy"/>
        /// directly instead of this extension method will be optimal since this method always allocates a new return buffer.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> is outside the range of valid indexes for the source buffer -or-
        /// <paramref name="length"/> is less than 0.
        /// </exception>
        public static byte[] BlockCopy(this byte[] buffer, int startIndex, int length)
        {
            if ((object)buffer == null)
                throw new ArgumentNullException("buffer");

            if (startIndex < 0)
                throw new ArgumentOutOfRangeException("startIndex", "cannot be negative");

            if (length < 0)
                throw new ArgumentOutOfRangeException("length", "cannot be negative");

            if (startIndex >= buffer.Length)
                throw new ArgumentOutOfRangeException("startIndex", "not a valid index into the buffer");

            byte[] copiedBytes = new byte[buffer.Length - startIndex < length ? buffer.Length - startIndex : length];

            Buffer.BlockCopy(buffer, startIndex, copiedBytes, 0, copiedBytes.Length);

            return copiedBytes;
        }

        /// <summary>
        /// Combines buffers together as a single image.
        /// </summary>
        /// <param name="source">Source buffer.</param>
        /// <param name="other">Other buffer to combine to <paramref name="source"/> buffer.</param>
        /// <returns>Combined buffers.</returns>
        public static byte[] Combine(this byte[] source, byte[] other)
        {
            if ((object)source == null)
                throw new ArgumentNullException("source");

            if ((object)other == null)
                throw new ArgumentNullException("other");

            return source.Combine(0, source.Length, other, 0, other.Length);
        }

        /// <summary>
        /// Combines specified portions of buffers together as a single image.
        /// </summary>
        /// <param name="source">Source buffer.</param>
        /// <param name="sourceOffset">Offset into <paramref name="source"/> buffer to begin copy.</param>
        /// <param name="sourceCount">Number of bytes to copy from <paramref name="source"/> buffer.</param>
        /// <param name="other">Other buffer to combine to <paramref name="source"/> buffer.</param>
        /// <param name="otherOffset">Offset into <paramref name="other"/> buffer to begin copy.</param>
        /// <param name="otherCount">Number of bytes to copy from <paramref name="other"/> buffer.</param>
        /// <returns>Combined specified portions of both buffers.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="sourceOffset"/> or <paramref name="otherOffset"/> is outside the range of valid indexes for the associated buffer -or-
        /// <paramref name="sourceCount"/> or <paramref name="otherCount"/> is less than 0 -or- 
        /// <paramref name="sourceOffset"/> or <paramref name="otherOffset"/>, 
        /// and <paramref name="sourceCount"/> or <paramref name="otherCount"/> do not specify a valid section in the the associated buffer.
        /// </exception>
        public static byte[] Combine(this byte[] source, int sourceOffset, int sourceCount, byte[] other, int otherOffset, int otherCount)
        {
            if ((object)source == null)
                throw new ArgumentNullException("source");

            if ((object)other == null)
                throw new ArgumentNullException("other");

            if (sourceOffset < 0)
                throw new ArgumentOutOfRangeException("sourceOffset", "cannot be negative");

            if (otherOffset < 0)
                throw new ArgumentOutOfRangeException("otherOffset", "cannot be negative");

            if (sourceCount < 0)
                throw new ArgumentOutOfRangeException("sourceCount", "cannot be negative");

            if (otherCount < 0)
                throw new ArgumentOutOfRangeException("otherCount", "cannot be negative");

            if (sourceOffset >= source.Length)
                throw new ArgumentOutOfRangeException("sourceOffset", "not a valid index into source buffer");

            if (otherOffset >= other.Length)
                throw new ArgumentOutOfRangeException("otherOffset", "not a valid index into other buffer");

            if (sourceOffset + sourceCount > source.Length)
                throw new ArgumentOutOfRangeException("sourceCount", "exceeds source buffer size");

            if (otherOffset + otherCount > other.Length)
                throw new ArgumentOutOfRangeException("otherCount", "exceeds other buffer size");

            // Overflow is possible, but unlikely.  Therefore, this is omitted for performance
            // if ((int.MaxValue - sourceCount - otherCount) < 0)
            //    throw new ArgumentOutOfRangeException("sourceCount + otherCount", "exceeds maximum buffer size");

            // Combine buffers together as a single image
            byte[] combinedBuffer = new byte[sourceCount + otherCount];

            Buffer.BlockCopy(source, sourceOffset, combinedBuffer, 0, sourceCount);
            Buffer.BlockCopy(other, otherOffset, combinedBuffer, sourceCount, otherCount);

            return combinedBuffer;
        }

        /// <summary>
        /// Combines buffers together as a single image.
        /// </summary>
        /// <param name="source">Source buffer.</param>
        /// <param name="other1">First buffer to combine to <paramref name="source"/> buffer.</param>
        /// <param name="other2">Second buffer to combine to <paramref name="source"/> buffer.</param>
        /// <returns>Combined buffers.</returns>
        public static byte[] Combine(this byte[] source, byte[] other1, byte[] other2)
        {
            return (new byte[][] { source, other1, other2 }).Combine();
        }

        /// <summary>
        /// Combines buffers together as a single image.
        /// </summary>
        /// <param name="source">Source buffer.</param>
        /// <param name="other1">First buffer to combine to <paramref name="source"/> buffer.</param>
        /// <param name="other2">Second buffer to combine to <paramref name="source"/> buffer.</param>
        /// <param name="other3">Third buffer to combine to <paramref name="source"/> buffer.</param>
        /// <returns>Combined buffers.</returns>
        public static byte[] Combine(this byte[] source, byte[] other1, byte[] other2, byte[] other3)
        {
            return (new byte[][] { source, other1, other2, other3 }).Combine();
        }

        /// <summary>
        /// Combines buffers together as a single image.
        /// </summary>
        /// <param name="source">Source buffer.</param>
        /// <param name="other1">First buffer to combine to <paramref name="source"/> buffer.</param>
        /// <param name="other2">Second buffer to combine to <paramref name="source"/> buffer.</param>
        /// <param name="other3">Third buffer to combine to <paramref name="source"/> buffer.</param>
        /// <param name="other4">Fourth buffer to combine to <paramref name="source"/> buffer.</param>
        /// <returns>Combined buffers.</returns>
        public static byte[] Combine(this byte[] source, byte[] other1, byte[] other2, byte[] other3, byte[] other4)
        {
            return (new byte[][] { source, other1, other2, other3, other4 }).Combine();
        }

        /// <summary>
        /// Combines array of buffers together as a single image.
        /// </summary>
        /// <param name="buffers">Array of byte buffers.</param>
        /// <returns>Combined buffers.</returns>
        public static byte[] Combine(this byte[][] buffers)
        {
            if ((object)buffers == null)
                throw new ArgumentNullException("buffers");

            MemoryStream combinedBuffer = new MemoryStream();

            // Combine all currently queued buffers
            for (int x = 0; x < buffers.Length; x++)
            {
                if ((object)buffers[x] == null)
                    throw new ArgumentNullException("buffers[" + x + "]");

                combinedBuffer.Write(buffers[x], 0, buffers[x].Length);
            }

            // return combined data buffers
            return combinedBuffer.ToArray();
        }

        /// <summary>
        /// Searches for the specified sequence of <paramref name="bytesToFind"/> and returns the index of the first occurrence within the <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">Buffer to search.</param>
        /// <param name="bytesToFind">Byte sequence to search for.</param>
        /// <returns>The zero-based index of the first occurance of the sequence of <paramref name="bytesToFind"/> in the <paramref name="buffer"/>, if found; otherwise, -1.</returns>
        public static int IndexOfSequence(this byte[] buffer, byte[] bytesToFind)
        {
            if ((object)buffer == null)
                throw new ArgumentNullException("buffer");

            if ((object)bytesToFind == null)
                throw new ArgumentNullException("bytesToFind");

            return buffer.IndexOfSequence(bytesToFind, 0, buffer.Length);
        }

        /// <summary>
        /// Searches for the specified sequence of <paramref name="bytesToFind"/> and returns the index of the first occurrence within the range of elements in the <paramref name="buffer"/> that starts at the specified index.
        /// </summary>
        /// <param name="buffer">Buffer to search.</param>
        /// <param name="bytesToFind">Byte sequence to search for.</param>
        /// <param name="startIndex">Start index in the <paramref name="buffer"/> to start searching.</param>
        /// <returns>The zero-based index of the first occurance of the sequence of <paramref name="bytesToFind"/> in the <paramref name="buffer"/>, if found; otherwise, -1.</returns>
        public static int IndexOfSequence(this byte[] buffer, byte[] bytesToFind, int startIndex)
        {
            if ((object)buffer == null)
                throw new ArgumentNullException("buffer");

            if ((object)bytesToFind == null)
                throw new ArgumentNullException("bytesToFind");

            return buffer.IndexOfSequence(bytesToFind, startIndex, buffer.Length - startIndex);
        }

        /// <summary>
        /// Searches for the specified sequence of <paramref name="bytesToFind"/> and returns the index of the first occurrence within the range of elements in the <paramref name="buffer"/> that starts at the specified index and contains the specified number of elements.
        /// </summary>
        /// <param name="buffer">Buffer to search.</param>
        /// <param name="bytesToFind">Byte sequence to search for.</param>
        /// <param name="startIndex">Start index in the <paramref name="buffer"/> to start searching.</param>
        /// <param name="length">Number of bytes in the <paramref name="buffer"/> to search through.</param>
        /// <returns>The zero-based index of the first occurance of the sequence of <paramref name="bytesToFind"/> in the <paramref name="buffer"/>, if found; otherwise, -1.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="bytesToFind"/> is null or has zero length.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> is outside the range of valid indexes for the source buffer -or-
        /// <paramref name="length"/> is less than 0.
        /// </exception>
        public static int IndexOfSequence(this byte[] buffer, byte[] bytesToFind, int startIndex, int length)
        {
            if ((object)buffer == null)
                throw new ArgumentNullException("buffer");

            if ((object)bytesToFind == null || bytesToFind.Length == 0)
                throw new ArgumentNullException("bytesToFind");

            if (startIndex < 0)
                throw new ArgumentOutOfRangeException("startIndex", "cannot be negative");

            if (length < 0)
                throw new ArgumentOutOfRangeException("length", "cannot be negative");

            if (startIndex >= buffer.Length)
                throw new ArgumentOutOfRangeException("startIndex", "not a valid index into source buffer");

            if (startIndex + length > buffer.Length)
                throw new ArgumentOutOfRangeException("length", "exceeds buffer size");

            // Overflow is possible, but unlikely.  Therefore, this is omitted for performance
            // if ((int.MaxValue - startIndex - length) < 0)
            //    throw new ArgumentOutOfRangeException("startIndex + length", "exceeds maximum buffer size");            

            // Search for first byte in the sequence, if this doesn't exist then sequence doesn't exist
            int index = Array.IndexOf(buffer, bytesToFind[0], startIndex, length);

            if (bytesToFind.Length > 1)
            {
                bool foundSequence = false;

                while (index > -1 && !foundSequence)
                {
                    // See if next bytes in sequence match
                    for (int x = 1; x < bytesToFind.Length; x++)
                    {
                        // Make sure there's enough buffer remaining to accomodate this byte
                        if (index + x < startIndex + length)
                        {
                            // If sequence doesn't match, search for next first-byte
                            if (buffer[index + x] != bytesToFind[x])
                            {
                                index = Array.IndexOf(buffer, bytesToFind[0], index + 1, length - (index + 1));
                                break;
                            }

                            // If each byte to find matched, we found the sequence
                            foundSequence = (x == bytesToFind.Length - 1);
                        }
                        else
                        {
                            // Ran out of buffer, return -1
                            index = -1;
                        }
                    }
                }
            }

            return index;
        }

        /// <summary>Returns comparision results of two binary buffers.</summary>
        /// <param name="source">Source buffer.</param>
        /// <param name="other">Other buffer to compare to <paramref name="source"/> buffer.</param>
        /// <returns>
        /// <para>
        /// A signed integer that indicates the relative comparison of <paramref name="source"/> buffer and <paramref name="other"/> buffer.
        /// </para>
        /// <para>
        /// <list type="table">
        ///     <listheader>
        ///         <term>Return Value</term>
        ///         <description>Description</description>
        ///     </listheader>
        ///     <item>
        ///         <term>Less than zero</term>
        ///         <description>Source buffer is less than other buffer.</description>
        ///     </item>
        ///     <item>
        ///         <term>Zero</term>
        ///         <description>Source buffer is equal to other buffer.</description>
        ///     </item>
        ///     <item>
        ///         <term>Greater than zero</term>
        ///         <description>Source buffer is greater than other buffer.</description>
        ///     </item>
        /// </list>
        /// </para>
        /// </returns>
        public static int CompareTo(this byte[] source, byte[] other)
        {
            if ((object)source == null && (object)other == null)
            {
                // Both buffers are assumed equal if both are nothing.
                return 0;
            }
            else if ((object)source == null)
            {
                // Buffer 2 has data, and buffer 1 is nothing. Buffer 2 is assumed larger.
                return 1;
            }
            else if ((object)other == null)
            {
                // Buffer 1 has data, and buffer 2 is nothing. Buffer 1 is assumed larger.
                return -1;
            }
            else
            {
                int length1 = source.Length;
                int length2 = other.Length;

                if (length1 == length2)
                {
                    int comparision = 0;

                    // Compares elements of buffers that are of equal size.
                    for (int x = 0; x < length1; x++)
                    {
                        comparision = source[x].CompareTo(other[x]);

                        if (comparision != 0)
                            break;
                    }

                    return comparision;
                }
                else
                {
                    // Buffer lengths are unequal. Buffer with largest number of elements is assumed to be largest.
                    return length1.CompareTo(length2);
                }
            }
        }

        /// <summary>
        /// Returns comparision results of two binary buffers.
        /// </summary>
        /// <param name="source">Source buffer.</param>
        /// <param name="sourceOffset">Offset into <paramref name="source"/> buffer to begin compare.</param>
        /// <param name="other">Other buffer to compare to <paramref name="source"/> buffer.</param>
        /// <param name="otherOffset">Offset into <paramref name="other"/> buffer to begin compare.</param>
        /// <param name="count">Number of bytes to compare in both buffers.</param>
        /// <returns>
        /// <para>
        /// A signed integer that indicates the relative comparison of <paramref name="source"/> buffer and <paramref name="other"/> buffer.
        /// </para>
        /// <para>
        /// <list type="table">
        ///     <listheader>
        ///         <term>Return Value</term>
        ///         <description>Description</description>
        ///     </listheader>
        ///     <item>
        ///         <term>Less than zero</term>
        ///         <description>Source buffer is less than other buffer.</description>
        ///     </item>
        ///     <item>
        ///         <term>Zero</term>
        ///         <description>Source buffer is equal to other buffer.</description>
        ///     </item>
        ///     <item>
        ///         <term>Greater than zero</term>
        ///         <description>Source buffer is greater than other buffer.</description>
        ///     </item>
        /// </list>
        /// </para>
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="sourceOffset"/> or <paramref name="otherOffset"/> is outside the range of valid indexes for the associated buffer -or-
        /// <paramref name="count"/> is less than 0 -or- 
        /// <paramref name="sourceOffset"/> or <paramref name="otherOffset"/> and <paramref name="count"/> do not specify a valid section in the the associated buffer.
        /// </exception>
        public static int CompareTo(this byte[] source, int sourceOffset, byte[] other, int otherOffset, int count)
        {
            if ((object)source == null && (object)other == null)
            {
                // Both buffers are assumed equal if both are nothing.
                return 0;
            }
            else if ((object)source == null)
            {
                // Buffer 2 has data, and buffer 1 is nothing. Buffer 2 is assumed larger.
                return 1;
            }
            else if ((object)other == null)
            {
                // Buffer 1 has data, and buffer 2 is nothing. Buffer 1 is assumed larger.
                return -1;
            }
            else
            {
                if (sourceOffset < 0)
                    throw new ArgumentOutOfRangeException("sourceOffset", "cannot be negative");

                if (otherOffset < 0)
                    throw new ArgumentOutOfRangeException("otherOffset", "cannot be negative");

                if (count < 0)
                    throw new ArgumentOutOfRangeException("count", "cannot be negative");

                if (sourceOffset >= source.Length)
                    throw new ArgumentOutOfRangeException("sourceOffset", "not a valid index into source buffer");

                if (otherOffset >= other.Length)
                    throw new ArgumentOutOfRangeException("otherOffset", "not a valid index into other buffer");

                if (sourceOffset + count > source.Length)
                    throw new ArgumentOutOfRangeException("count", "exceeds source buffer size");

                if (otherOffset + count > other.Length)
                    throw new ArgumentOutOfRangeException("count", "exceeds other buffer size");

                // Overflow is possible, but unlikely.  Therefore, this is omitted for performance
                // if ((int.MaxValue - sourceOffset - count) < 0)
                //    throw new ArgumentOutOfRangeException("sourceOffset + count", "exceeds maximum buffer size");

                // Overflow is possible, but unlikely.  Therefore, this is omitted for performance
                // if ((int.MaxValue - otherOffset - count) < 0)
                //    throw new ArgumentOutOfRangeException("sourceOffset + count", "exceeds maximum buffer size");

                int comparision = 0;

                // Compares elements of buffers that are of equal size.
                for (int x = 0; x < count; x++)
                {
                    comparision = source[sourceOffset + x].CompareTo(other[otherOffset + x]);

                    if (comparision != 0)
                        break;
                }

                return comparision;
            }
        }
    }
}
