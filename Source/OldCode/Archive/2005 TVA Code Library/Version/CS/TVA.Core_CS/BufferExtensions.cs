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
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.IO;

namespace TVA
{
    /// <summary>Defines extension fucntions related to buffer manipulation.</summary>
    public static class BufferExtensions
    {
        /// <summary>Returns a copy of the specified portion of the source buffer.</summary>
        /// <remarks>Grows or shrinks returned buffer, as needed, to make it the desired length.</remarks>
        public static byte[] CopyBuffer(this byte[] source, int startIndex, int length)
        {
            byte[] copiedBytes =  new byte[source.Length - startIndex < length ? source.Length - startIndex : length];

            Buffer.BlockCopy(source, startIndex, copiedBytes, 0, copiedBytes.Length);

            return copiedBytes;
        }

        /// <summary>Returns comparision results of two binary buffers.</summary>
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
                // Code replicated here as an optimization, instead of calling overloaded CompareBuffers
                // to prevent duplicate "== null" checks for empty buffers. This function needs to
                // execute as quickly as possible given possible intended uses.
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
                        {
                            break;
                        }
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

        /// <summary>Returns comparision results of two binary buffers.</summary>
        public static int CompareTo(this byte[] source, int sourceOffset, int sourceLength, byte[] other, int otherOffset, int otherLength)
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
                if (sourceLength == otherLength)
                {
                    int comparision = 0;

                    // Compares elements of buffers that are of equal size.
                    for (int x = 0; x <= sourceLength - 1; x++)
                    {
                        comparision = source[sourceOffset + x].CompareTo(other[otherOffset + x]);
                        if (comparision != 0)
                        {
                            break;
                        }
                    }

                    return comparision;
                }
                else
                {
                    // Buffer lengths are unequal. Buffer with largest number of elements is assumed to be largest.
                    return sourceLength.CompareTo(otherLength);
                }
            }
        }

        /// <summary>Calculates byte length (8-bit) XOR-based check-sum on specified portion of a buffer.</summary>
        /// <param name="data">Data buffer to perform XOR check-sum on.</param>
        /// <param name="startIndex">Starts index in data buffer to begin XOR check-sum.</param>
        /// <param name="length">Total number of bytes from <paramref name="startIndex">startIndex</paramref> to
        /// perform XOR check-sum over.</param>
        /// <returns>Byte length XOR check-sum.</returns>
        [CLSCompliant(false)]
        public static byte Xor8BitCheckSum(this byte[] data, int startIndex, int length)
        {
            byte sum = 0;

            for (int x = 0; x <= length - 1; x++)
            {
                sum ^= data[startIndex + x];
            }

            return sum;
        }

        /// <summary>Calculates word length (16-bit) XOR-based check-sum on specified portion of a buffer.</summary>
        /// <param name="data">Data buffer to perform XOR check-sum on.</param>
        /// <param name="startIndex">Starts index in data buffer to begin XOR check-sum.</param>
        /// <param name="length">Total number of bytes from <paramref name="startIndex">startIndex</paramref> to
        /// perform XOR check-sum overs</param>
        /// <returns>Word length XOR check-sum.</returns>
        [CLSCompliant(false)]
        public static ushort Xor16BitCheckSum(this byte[] data, int startIndex, int length)
        {
            ushort sum = 0;

            for (int x = 0; x <= length - 1; x += 2)
            {
                sum ^= BitConverter.ToUInt16(data, startIndex + x);
            }

            return sum;
        }

        /// <summary>Calculates double-word length (32-bit) XOR-based check-sum on specified portion of a buffer.</summary>
        /// <param name="data">Data buffer to perform XOR check-sum on.</param>
        /// <param name="startIndex">Starts index in data buffer to begin XOR check-sum.</param>
        /// <param name="length">Total number of bytes from <paramref name="startIndex">startIndex</paramref> to
        /// perform XOR check-sum over.</param>
        /// <returns>Double-word length XOR check-sum.</returns>
        [CLSCompliant(false)]
        public static uint Xor32BitCheckSum(this byte[] data, int startIndex, int length)
        {
            uint sum = 0;

            for (int x = 0; x <= length - 1; x += 4)
            {
                sum ^= BitConverter.ToUInt32(data, startIndex + x);
            }

            return sum;
        }

        /// <summary>Calculates quad-word length (64-bit) XOR-based check-sum on specified portion of a buffer.</summary>
        /// <param name="data">Data buffer to perform XOR check-sum on.</param>
        /// <param name="startIndex">Starts index in data buffer to begin XOR check-sum.</param>
        /// <param name="length">Total number of bytes from <paramref name="startIndex">startIndex</paramref> to
        /// perform XOR check-sum over.</param>
        /// <returns>Quad-word length XOR check-sum.</returns>
        [CLSCompliant(false)]
        public static ulong Xor64BitCheckSum(this byte[] data, int startIndex, int length)
        {
            ulong sum = 0;

            for (int x = 0; x <= length - 1; x += 8)
            {
                sum ^= BitConverter.ToUInt64(data, startIndex + x);
            }

            return sum;
        }
    }
}
