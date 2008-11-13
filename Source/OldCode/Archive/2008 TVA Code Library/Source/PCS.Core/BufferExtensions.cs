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

namespace PCS
{
    /// <summary>Defines extension functions related to buffer manipulation.</summary>
    public static class BufferExtensions
    {
        /// <summary>Returns a copy of the specified portion of the source buffer.</summary>
        /// <remarks>Grows or shrinks returned buffer, as needed, to make it the desired length.</remarks>
        public static byte[] BlockCopy(this byte[] source, int startIndex, int length)
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
    }
}
