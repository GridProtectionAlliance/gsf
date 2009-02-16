//*******************************************************************************************************
//  BufferExtensions.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  09/19/2008 - James R Carroll
//      Generated original version of source code.
//  12/03/2008 - James R Carroll
//      Added "Combine" and "IndexOfSequence" overloaded extensions.
//
//*******************************************************************************************************

using System;
using System.IO;

namespace PCS
{
    /// <summary>Defines extension functions related to buffer manipulation.</summary>
    public static class BufferExtensions
    {
        /// <summary>Returns a copy of the specified portion of the <paramref name="source"/> buffer.</summary>
        /// <param name="source">Source buffer.</param>
        /// <param name="startIndex">Offset into <paramref name="source"/> buffer.</param>
        /// <param name="length">Length of <paramref name="source"/> buffer to copy at <paramref name="startIndex"/> offset.</param>
        /// <returns>A buffer of data copied from the specified portion of the source buffer.</returns>
        /// <remarks>
        /// Returned buffer will be extended as needed to make it the specified <paramref name="length"/>, but
        /// it will never be less than the source buffer length - <paramref name="startIndex"/>.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> is outside the range of valid indexes for the source buffer -or-
        /// <paramref name="length"/> is less than 0.
        /// </exception>
        public static byte[] BlockCopy(this byte[] source, int startIndex, int length)
        {
            if (startIndex < 0)
                throw new ArgumentOutOfRangeException("startIndex", "cannot be negative");

            if (length < 0)
                throw new ArgumentOutOfRangeException("length", "cannot be negative");

            if (startIndex >= source.Length)
                throw new ArgumentOutOfRangeException("startIndex", "not a valid index into source buffer");

            byte[] copiedBytes = new byte[source.Length - startIndex < length ? source.Length - startIndex : length];

            Buffer.BlockCopy(source, startIndex, copiedBytes, 0, copiedBytes.Length);

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
            MemoryStream combinedBuffer = new MemoryStream();

            // Combine all currently queued buffers
            for (int x = 0; x <= buffers.Length - 1; x++)
            {
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
            if (bytesToFind == null || bytesToFind.Length == 0)
                throw new ArgumentNullException("bytesToFind");

            if (startIndex < 0)
                throw new ArgumentOutOfRangeException("startIndex", "cannot be negative");

            if (length < 0)
                throw new ArgumentOutOfRangeException("length", "cannot be negative");

            if (startIndex >= buffer.Length)
                throw new ArgumentOutOfRangeException("startIndex", "not a valid index into source buffer");

            if (startIndex + length > buffer.Length)
                throw new ArgumentOutOfRangeException("length", "exceeds buffer size");

            // Search for first byte in the sequence, if this doesn't exist then sequence doesn't exist
            int index = Array.IndexOf(buffer, bytesToFind[0], startIndex, length);

            if (bytesToFind.Length > 1)
            {
                bool foundSequence = false;

                while (index > 0 && !foundSequence)
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
                                index = Array.IndexOf(buffer, bytesToFind[0], index + 1, length - (index - startIndex));
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
            if (source == null && other == null)
            {
                // Both buffers are assumed equal if both are nothing.
                return 0;
            }
            else if (source == null)
            {
                // Buffer 2 has data, and buffer 1 is nothing. Buffer 2 is assumed larger.
                return 1;
            }
            else if (other == null)
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
                    for (int x = 0; x <= length1 - 1; x++)
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
            if (source == null && other == null)
            {
                // Both buffers are assumed equal if both are nothing.
                return 0;
            }
            else if (source == null)
            {
                // Buffer 2 has data, and buffer 1 is nothing. Buffer 2 is assumed larger.
                return 1;
            }
            else if (other == null)
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

                int comparision = 0;

                // Compares elements of buffers that are of equal size.
                for (int x = 0; x <= count - 1; x++)
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
