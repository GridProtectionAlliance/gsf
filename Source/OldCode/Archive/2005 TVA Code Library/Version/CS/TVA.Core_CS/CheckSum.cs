//*******************************************************************************************************
//  CheckSum.cs
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
//  09/17/2008 - James R Carroll
//       Generated original version of source code.
//  09/17/2008 - James R Carroll
//      Converted to C#.
//
//*******************************************************************************************************

using System;

namespace TVA
{
    /// <summary>Check sum functions</summary>
    public static class CheckSum
    {
        /// <summary>Calculates byte length (8-bit) XOR-based check-sum on specified portion of a buffer.</summary>
        /// <param name="data">Data buffer to perform XOR check-sum on.</param>
        /// <param name="startIndex">Starts index in data buffer to begin XOR check-sum.</param>
        /// <param name="length">Total number of bytes from <paramref name="startIndex">startIndex</paramref> to
        /// perform XOR check-sum over.</param>
        /// <returns>Byte length XOR check-sum.</returns>
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
        public static UInt16 Xor16BitCheckSum(this byte[] data, int startIndex, int length)
        {
            UInt16 sum = 0;

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
        public static UInt32 Xor32BitCheckSum(this byte[] data, int startIndex, int length)
        {
            UInt32 sum = 0;

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
        public static UInt64 Xor64BitCheckSum(this byte[] data, int startIndex, int length)
        {
            UInt64 sum = 0;

            for (int x = 0; x <= length - 1; x += 8)
            {
                sum ^= BitConverter.ToUInt64(data, startIndex + x);
            }

            return sum;
        }
    }
}