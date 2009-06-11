//*******************************************************************************************************
//  Payload.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel, Operations Data Architecture [TVA]
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-3024
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  07/06/2006 - Pinal C. Patel
//       Original version of source code generated
//  09/29/2008 - James R Carroll
//       Converted to C#.
//
//*******************************************************************************************************

using System;
using System.IO;
using System.Text;
using TVA.IO.Compression;
using TVA.Security.Cryptography;

namespace TVA.Communication
{
    /// <summary>
    /// A helper class containing methods for manipulation of payload.
    /// </summary>
    public static class Payload
    {
        /// <summary>
        /// Specifies the length of the segment in a "Payload-Aware" transmission that contains the payload length.
        /// </summary>
        public const int LengthSegment = 4;

        /// <summary>
        /// Default byte sequence used to mark the beginning of the payload in a "Payload-Aware" transmissions.
        /// </summary>
        public static byte[] DefaultMarker = { 0xAA, 0xBB, 0xCC, 0xDD };

        /// <summary>
        /// Adds header containing the <paramref name="marker"/> to the payload in the <paramref name="buffer"/> for "Payload-Aware" transmission.
        /// </summary>
        /// <param name="buffer">The buffer containing the payload.</param>
        /// <param name="offset">The offset in the <paramref name="buffer"/> at which the payload starts.</param>
        /// <param name="length">The lenght of the payload in the <paramref name="buffer"/> starting at the <paramref name="offset"/>.</param>
        /// <param name="marker">The byte sequence used to mark the beginning of the payload in a "Payload-Aware" transmissions.</param>
        public static void AddHeader(ref byte[] buffer, ref int offset, ref int length, byte[] marker)
        {
            // The resulting buffer will be at least 4 bytes bigger than the payload.

            // Resulting buffer = x bytes for payload marker + 4 bytes for the payload size + The payload
            byte[] result = new byte[(length - offset) + marker.Length + LengthSegment];

            // First, copy the the payload marker to the buffer.
            Buffer.BlockCopy(marker, 0, result, 0, marker.Length);
            // Then, copy the payload's size to the buffer after the payload marker.
            Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, result, marker.Length, LengthSegment);
            // At last, copy the payload after the payload marker and payload size.
            Buffer.BlockCopy(buffer, offset, result, marker.Length + LengthSegment, length);

            buffer = result;
            offset = 0;
            length = buffer.Length;
        }

        /// <summary>
        /// Determines whether or not the <paramref name="buffer"/> contains the header information of a "Payload-Aware" transmission.
        /// </summary>
        /// <param name="buffer">The buffer to be checked at index zero.</param>
        /// <param name="marker">The byte sequence used to mark the beginning of the payload in a "Payload-Aware" transmissions.</param>
        /// <returns>true if the buffer contains "Payload-Aware" transmission header; otherwise false.</returns>
        public static bool HasHeader(byte[] buffer, byte[] marker)
        {
            for (int i = 0; i <= marker.Length - 1; i++)
            {
                if (buffer[i] != marker[i])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Determines the length of a payload in a "Payload-Aware" transmission from the payload header information.
        /// </summary>
        /// <param name="buffer">The buffer containg payload header information starting at index zero.</param>
        /// <param name="marker">The byte sequence used to mark the beginning of the payload in a "Payload-Aware" transmissions.</param>
        /// <returns>Length of the payload.</returns>
        public static int ExtractLength(byte[] buffer, byte[] marker)
        {
            if (buffer.Length >= (marker.Length + LengthSegment) && HasHeader(buffer, marker))
            {
                // We have a buffer that's at least as big as the payload header and has the payload marker.
                return BitConverter.ToInt32(buffer, marker.Length);
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Performs the necessary uncompression and decryption on the data contained in the <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The buffer containing the data to be processed.</param>
        /// <param name="offset">The offset in the <paramref name="buffer"/> from where data is to be processed.</param>
        /// <param name="length">The length of data in <paramref name="buffer"/> starting at the <paramref name="offset"/>.</param>
        /// <param name="cryptoLevel">One of the <see cref="CipherStrength"/> values.</param>
        /// <param name="cryptoKey">The key to be used for decrypting the data in the <paramref name="buffer"/>.</param>
        /// <param name="compressLevel">One of the <see cref="CompressionStrength"/> values.</param>
        public static void ProcessReceived(ref byte[] buffer, ref int offset, ref int length, CipherStrength cryptoLevel, string cryptoKey, CompressionStrength compressLevel)
        {
            if (cryptoLevel != CipherStrength.None || compressLevel != CompressionStrength.NoCompression)
            {
                // Make a copy of the data to be processed.
                byte[] temp = buffer.BlockCopy(offset, length);

                if (cryptoLevel != CipherStrength.None)
                {
                    // Decrypt the data.
                    temp = temp.Decrypt(Encoding.ASCII.GetBytes(cryptoKey), cryptoLevel);
                    offset = 0;
                    length = temp.Length;
                }
                
                if (compressLevel != CompressionStrength.NoCompression)
                {
                    // Uncompress the data.
                    temp = new MemoryStream(temp).Decompress().ToArray();
                    offset = 0;
                    length = temp.Length;
                }

                if (temp.Length > buffer.Length)
                    // Processed data cannot fit in the existing buffer.
                    buffer = temp;
                else
                    // Copy the processed data into the existing buffer.
                    Buffer.BlockCopy(temp, offset, buffer, offset, length);
            }

            //if (cryptoLevel == CipherStrength.None && compressLevel == CompressionStrength.NoCompression)
            //{
            //    if (length - offset == data.Length)
            //        return data;
            //    else
            //        return data.BlockCopy(offset, length);
            //}
            //else
            //{
            //    if (cryptoLevel != CipherStrength.None)
            //    {
            //        data = data.Decrypt(offset, length, Encoding.ASCII.GetBytes(cryptoKey), cryptoLevel);
            //        offset = 0;
            //        length = data.Length;
            //    }

            //    if (compressLevel != CompressionStrength.NoCompression)
            //    {
            //        data = new MemoryStream(data, offset, length).Decompress().ToArray();
            //    }

            //    return data;
            //}
        }

        /// <summary>
        /// Performs the necessary compression and encryption on the data contained in the <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The buffer containing the data to be processed.</param>
        /// <param name="offset">The offset in the <paramref name="buffer"/> from where data is to be processed.</param>
        /// <param name="length">The length of data in <paramref name="buffer"/> starting at the <paramref name="offset"/>.</param>
        /// <param name="cryptoLevel">One of the <see cref="CipherStrength"/> values.</param>
        /// <param name="cryptoKey">The key to be used for encrypting the data in the <paramref name="buffer"/>.</param>
        /// <param name="compressLevel">One of the <see cref="CompressionStrength"/> values.</param>
        public static void ProcessTransmit(ref byte[] buffer, ref int offset, ref int length, CipherStrength cryptoLevel, string cryptoKey, CompressionStrength compressLevel)
        {
            if (compressLevel != CompressionStrength.NoCompression)
            {
                // Compress the data.
                buffer = new MemoryStream(buffer, offset, length).Compress(compressLevel).ToArray();
                offset = 0;
                length = buffer.Length;
            }

            if (cryptoLevel != CipherStrength.None)
            {
                // Encrypt the data.
                buffer = buffer.Encrypt(offset, length, Encoding.ASCII.GetBytes(cryptoKey), cryptoLevel);
                offset = 0;
                length = buffer.Length;
            }

            //if (cryptoLevel == CipherStrength.None && compressLevel == CompressionStrength.NoCompression)
            //{
            //    if (length - offset == data.Length)
            //        return data;
            //    else
            //        return data.BlockCopy(offset, length);
            //}
            //else
            //{
            //    if (compressLevel != CompressionStrength.NoCompression)
            //    {
            //        // Compress the data.
            //        data = new MemoryStream(data, offset, length).Compress(compressLevel).ToArray();
            //        offset = 0;
            //        length = data.Length;
            //    }

            //    if (cryptoLevel != CipherStrength.None)
            //    {
            //        // Encrypt the data.
            //        data = data.Encrypt(offset, length, Encoding.ASCII.GetBytes(cryptoKey), cryptoLevel);
            //    }

            //    return data;
            //}
        }
    }
}