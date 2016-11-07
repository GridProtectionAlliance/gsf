//******************************************************************************************************
//  ChecksumExtensions.cs - Gbtc
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
//  09/24/2008 - J. Ritchie Carroll
//       Generated original version of source code.
//  06/10/2009 - Mehulbhi Thakkar
//		 Added extension method for CRC-ModBus calculation.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************


namespace GSF.IO.Checksums
{
    /// <summary>Defines extension functions related to computing checksums.</summary>
    public static class ChecksumExtensions
    {
        /// <summary>Calculates the Adler-32 checksum on specified portion of a buffer.</summary>
        /// <param name="data">Data buffer to perform checksum on.</param>
        /// <param name="startIndex">Starts index in data buffer to begin checksum.</param>
        /// <param name="length">Total number of bytes from <paramref name="startIndex">startIndex</paramref> to
        /// perform checksum over.</param>
        /// <returns>Computed Adler-32 checksum over the specified portion of the buffer.</returns>
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

        /// <summary>Calculates the CRC-CCITT check-sum on specified portion of a buffer.</summary>
        /// <param name="data">Data buffer to perform check-sum on.</param>
        /// <param name="startIndex">Starts index in data buffer to begin check-sum.</param>
        /// <param name="length">Total number of bytes from <paramref name="startIndex">startIndex</paramref> to
        /// perform check-sum over.</param>
        /// <returns>Computed CRC-CCITT checksum over the specified portion of the buffer.</returns>
        /// <remarks>
        /// The CRC-CCITT is a table based 16-bit CRC popular for modem protocols defined for use by the
        /// Consultative Committee on International Telegraphy and Telephony (CCITT) 
        /// </remarks>
        public static ushort CrcCCITTChecksum(this byte[] data, int startIndex, int length)
        {
            CrcCCITT checksum = new CrcCCITT();

            checksum.Update(data, startIndex, length);

            return checksum.Value;
        }

        /// <summary>Calculates the CRC-ModBus check-sum on specified portion of a buffer.</summary>
        /// <param name="data">Data buffer to perform check-sum on.</param>
        /// <param name="startIndex">Starts index in data buffer to begin check-sum.</param>
        /// <param name="length">Total number of bytes from <paramref name="startIndex">startIndex</paramref> to
        /// perform check-sum over.</param>
        /// <returns>Computed CRC-ModBus checksum over the specified portion of the buffer.</returns>		
        public static ushort ModBusCrcChecksum(this byte[] data, int startIndex, int length)
        {
            Crc16 checksum = new Crc16(ChecksumType.ModBus);

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

        /// <summary>Calculates byte length (8-bit) XOR-based check-sum on specified portion of a buffer.</summary>
        /// <param name="data">Data buffer to perform XOR check-sum on.</param>
        /// <param name="startIndex">Starts index in data buffer to begin XOR check-sum.</param>
        /// <param name="length">Total number of bytes from <paramref name="startIndex">startIndex</paramref> to
        /// perform XOR check-sum over.</param>
        /// <returns>Byte length XOR check-sum.</returns>
        public static byte Xor8Checksum(this byte[] data, int startIndex, int length)
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
        public static ushort Xor16Checksum(this byte[] data, int startIndex, int length)
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
        public static uint Xor32Checksum(this byte[] data, int startIndex, int length)
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
        public static ulong Xor64Checksum(this byte[] data, int startIndex, int length)
        {
            Xor64 checksum = new Xor64();

            checksum.Update(data, startIndex, length);

            return checksum.Value;
        }
    }
}
