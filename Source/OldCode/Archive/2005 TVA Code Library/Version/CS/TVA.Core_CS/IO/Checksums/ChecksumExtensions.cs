//*******************************************************************************************************
//  ChecksumExtensions.cs
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
//  09/24/2008 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.IO;

namespace TVA.IO.Checksums
{
    /// <summary>Defines extension functions related to computing checksums.</summary>
    [CLSCompliant(false)]
    public static class ChecksumExtensions
    {
        /// <summary>Calculates the Adler32 check-sum on specified portion of a buffer.</summary>
        /// <param name="data">Data buffer to perform check-sum on.</param>
        /// <param name="startIndex">Starts index in data buffer to begin check-sum.</param>
        /// <param name="length">Total number of bytes from <paramref name="startIndex">startIndex</paramref> to
        /// perform check-sum over.</param>
        /// <remarks>
        /// <para>
        /// Computes Adler32 checksum for a stream of data. An Adler32 checksum is not as reliable as a CRC32
        /// checksum, but a lot faster to compute.
        /// </para>
        /// <para>
        /// The specification for Adler32 may be found in RFC 1950. ZLIB Compressed Data Format Specification
        /// version 3.3.
        /// </para>
        /// </remarks>
        /// <returns>Computed Adler32 checksum over the specified portion of the buffer.</returns>
        public static uint Adler32Checksum(this byte[] data, int startIndex, int length)
        {
            Adler32 checksum = new Adler32();

            checksum.Update(data, startIndex, length);

            return checksum.Value;
        }

        /// <summary>Calculates the CRC16 check-sum on specified portion of a buffer.</summary>
        /// <param name="data">Data buffer to perform check-sum on.</param>
        /// <param name="startIndex">Starts index in data buffer to begin check-sum.</param>
        /// <param name="length">Total number of bytes from <paramref name="startIndex">startIndex</paramref> to
        /// perform check-sum over.</param>
        /// <returns>Computed CRC16 checksum over the specified portion of the buffer.</returns>
        public static ushort Crc16Checksum(this byte[] data, int startIndex, int length)
        {
            Crc16 checksum = new Crc16();

            checksum.Update(data, startIndex, length);

            return checksum.Value;
        }

        /// <summary>Calculates the CRC32 check-sum on specified portion of a buffer.</summary>
        /// <param name="data">Data buffer to perform check-sum on.</param>
        /// <param name="startIndex">Starts index in data buffer to begin check-sum.</param>
        /// <param name="length">Total number of bytes from <paramref name="startIndex">startIndex</paramref> to
        /// perform check-sum over.</param>
        /// <returns>Computed CRC32 checksum over the specified portion of the buffer.</returns>
        public static uint Crc32Checksum(this byte[] data, int startIndex, int length)
        {
            Crc32 checksum = new Crc32();

            checksum.Update(data, startIndex, length);

            return checksum.Value;
        }

        /// <summary>Calculates the StrangeCRC check-sum on specified portion of a buffer.</summary>
        /// <param name="data">Data buffer to perform check-sum on.</param>
        /// <param name="startIndex">Starts index in data buffer to begin check-sum.</param>
        /// <param name="length">Total number of bytes from <paramref name="startIndex">startIndex</paramref> to
        /// perform check-sum over.</param>
        /// <remarks>
        /// The StrangeCRC is used by BZip library algorithms which are specifically used for
        /// lossless, block-sorting data compression.
        /// </remarks>
        /// <returns>Computed StrangeCRC checksum over the specified portion of the buffer.</returns>
        public static uint StrangeCrcChecksum(this byte[] data, int startIndex, int length)
        {
            Crc32 checksum = new Crc32();

            checksum.Update(data, startIndex, length);

            return checksum.Value;
        }

        /// <summary>Calculates byte length (8-bit) XOR-based check-sum on specified portion of a buffer.</summary>
        /// <param name="data">Data buffer to perform XOR check-sum on.</param>
        /// <param name="startIndex">Starts index in data buffer to begin XOR check-sum.</param>
        /// <param name="length">Total number of bytes from <paramref name="startIndex">startIndex</paramref> to
        /// perform XOR check-sum over.</param>
        /// <returns>Byte length XOR check-sum.</returns>
        public static byte Xor8CheckSum(this byte[] data, int startIndex, int length)
        {
            Xor8 checksum = new Xor8();

            checksum.Update(data, startIndex, length);

            return checksum.Value;
        }

        /// <summary>Calculates word length (16-bit) XOR-based check-sum on specified portion of a buffer.</summary>
        /// <param name="data">Data buffer to perform XOR check-sum on.</param>
        /// <param name="startIndex">Starts index in data buffer to begin XOR check-sum.</param>
        /// <param name="length">Total number of bytes from <paramref name="startIndex">startIndex</paramref> to
        /// perform XOR check-sum overs</param>
        /// <returns>Word length XOR check-sum.</returns>
        public static ushort Xor16CheckSum(this byte[] data, int startIndex, int length)
        {
            Xor16 checksum = new Xor16();

            checksum.Update(data, startIndex, length);

            return checksum.Value;
        }

        /// <summary>Calculates double-word length (32-bit) XOR-based check-sum on specified portion of a buffer.</summary>
        /// <param name="data">Data buffer to perform XOR check-sum on.</param>
        /// <param name="startIndex">Starts index in data buffer to begin XOR check-sum.</param>
        /// <param name="length">Total number of bytes from <paramref name="startIndex">startIndex</paramref> to
        /// perform XOR check-sum over.</param>
        /// <returns>Double-word length XOR check-sum.</returns>
        public static uint Xor32CheckSum(this byte[] data, int startIndex, int length)
        {
            Xor32 checksum = new Xor32();

            checksum.Update(data, startIndex, length);

            return checksum.Value;
        }

        /// <summary>Calculates quad-word length (64-bit) XOR-based check-sum on specified portion of a buffer.</summary>
        /// <param name="data">Data buffer to perform XOR check-sum on.</param>
        /// <param name="startIndex">Starts index in data buffer to begin XOR check-sum.</param>
        /// <param name="length">Total number of bytes from <paramref name="startIndex">startIndex</paramref> to
        /// perform XOR check-sum over.</param>
        /// <returns>Quad-word length XOR check-sum.</returns>
        public static ulong Xor64CheckSum(this byte[] data, int startIndex, int length)
        {
            Xor64 checksum = new Xor64();

            checksum.Update(data, startIndex, length);

            return checksum.Value;
        }
    }
}
